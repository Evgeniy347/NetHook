using NetHook.Cores.Inject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NetHook.Core
{
    public class LocalHookAdapter : IDisposable
    {
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();

        private readonly Dictionary<IntPtr, MemoryInstractions> _memories = new Dictionary<IntPtr, MemoryInstractions>();

        public Memory Memory { get; } = new Memory();

        private readonly static object _lock = new object();

        private LocalHookAdapter()
        {
            Process process = Process.GetCurrentProcess();
            Memory.Open(process);
        }

        public List<IHandlerHook> Handlers { get; } = new List<IHandlerHook>();

        public LocalHookCodeDoom CodeDoomProvider { get; } = new LocalHookCodeDoom();

        public void Install(StringBuilder stringBuilder = null)
        {
            Assembly assembly = CodeDoomProvider.Compiller();
            CodeDoomProvider.Clear();
            UnInstall();

            foreach (Type type in assembly.GetTypes().Where(x => typeof(LocalHookRuntimeInstance).IsAssignableFrom(x)))
            {
                LocalHookRuntimeInstance provider = (LocalHookRuntimeInstance)Activator.CreateInstance(type, new object[] { this });
                string log = CreateHook(provider.Method, provider.MethodHook);
                if (stringBuilder != null)
                    stringBuilder.AppendLine(log);
            }

            foreach (var keyValue in _memories)
                keyValue.Value.Install();

        }

        internal void UnInstall()
        {
            if (_memories.Count == 0)
                return;

            try
            {
                foreach (var keyValue in _memories)
                    keyValue.Value.Dispose();
            }
            finally
            {
                _memories.Clear();
            }
        }

        public MemoryInstractions Alloc(IntPtr address)
        {
            var newmem = Memory.Alloc(0x100, address);

            if (!_memories.TryGetValue(newmem, out MemoryInstractions memInstractions))
                _memories[newmem] = memInstractions = new MemoryInstractions(newmem, Memory, 0x100);

            return memInstractions;
        }

        private string CreateHook(MethodInfo method, MethodInfo methodHook)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                CheckMethod(method);
                CheckMethod(methodHook);

                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                RuntimeHelpers.PrepareMethod(methodHook.MethodHandle);

                var methodAddress = method.MethodHandle.GetFunctionPointer();
                var methodAddressHook = methodHook.MethodHandle.GetFunctionPointer();

                stringBuilder.AppendLine($"methodAddress     {methodAddress.ToHex()} {Memory.GetAddressBody(methodAddress).ToHex()}");
                stringBuilder.AppendLine($"methodAddressHook {methodAddressHook.ToHex()} {Memory.GetAddressBody(methodAddressHook).ToHex()}");

                var newmem = Alloc(methodAddress);

                var methodBody = GetMethodBody(methodAddress);
                var methodHookBody = GetMethodBody(methodAddressHook);

                methodBody.AddJMP(methodAddressHook);

                //^//
                // methodBody.AddJMP(newmem.EndAddress);

                byte[] origBody = methodBody.GetOriginalBody(methodBody.Size);

                methodHookBody.FindAndReplaceCall(newmem.EndAddress);

                newmem.Add(origBody);
                methodBody.CheckNop(origBody.Length);
                newmem.AddJMP(methodBody.Address.Add(origBody.Length));

                stringBuilder.AppendLine("Method Body");
                stringBuilder.AppendLine(methodBody.WriteLog());
                stringBuilder.AppendLine("New Memory");
                stringBuilder.AppendLine(newmem.WriteLog(false));
                stringBuilder.AppendLine("Method Body Hook");
                stringBuilder.AppendLine(methodHookBody.WriteLog());
            }
            catch (Exception ex)
            {
                Console.WriteLine(stringBuilder.ToString());
                throw new Exception($"Hook error method '{method.Name}' type '{method.DeclaringType.AssemblyQualifiedName}'", ex);
            }

            return stringBuilder.ToString();
        }

        public static bool CheckMethod(MethodInfo method, bool throwNewEx = true)
        {
            if (!CheckMethod(method, out string error))
                if (throwNewEx)
                    throw new NotImplementedException(error);
                else
                    return false;

            return true;
        }

        private static bool CheckMethod(MethodInfo method, out string result)
        {
            result = string.Empty;

            if (method.IsGenericMethod)
                result = $"IsGenericMethod";

            if (method.IsGenericMethodDefinition)
                result = $"IsGenericMethodDefinition";

            if (method.DeclaringType.IsGenericType)
                result = $"IsGenericType";

            if (method.DeclaringType.IsGenericTypeDefinition)
                result = $"IsGenericTypeDefinition";

            if (method.Name.Contains('<') || method.Name.Contains('<') || method.Name.Contains('.') || method.Name.Contains('\''))
                result = $"Запрещеные символы в названии метода '{method.Name}'";

            if (method.IsConstructor)
                result = $"Запрещено использовать конструкторы";

            if (!method.ReturnType.IsClass && method.ReturnType != typeof(void))
                result = $"Запрещено использовать структуры в качестве возвращаемого значения. ReturnType '{method.ReturnType.FullName}'";

            foreach (var parametr in method.GetParameters())
            {
                if (!parametr.ParameterType.IsClass)
                    result = $"Запрещено использовать структуры в качестве параметра функции. ParameterName '{parametr.Name}' Type '{parametr.ParameterType.FullName}'";

                if (parametr.ParameterType.IsByRef)
                    result = $"Запрещено использовать модификар ref или out для параметра функции. ParameterName '{parametr.Name}' Type '{parametr.ParameterType.FullName}'";

                if (parametr.DefaultValue != DBNull.Value && parametr.DefaultValue != null)
                    result = $"Запрещено использовать DefaultValue для параметра функции. ParameterName '{parametr.Name}' Type '{parametr.ParameterType.FullName}'";
            }

            return string.IsNullOrEmpty(result);
        }

        public MemoryInstractions GetMethodBody(IntPtr address)
        {
            address = Memory.GetAddressBody(address);

            if (!_memories.TryGetValue(address, out MemoryInstractions memInstractions))
                _memories[address] = memInstractions = new MemoryInstractions(address, Memory);

            return memInstractions;
        }

        public void RegisterHandler(IHandlerHook handler)
        {
            Handlers.Add(handler);
        }

        public void AddHook(params MethodInfo[] methods)
        {
            if (methods == null || methods.Length == 0)
                throw new ArgumentException(nameof(methods));

            foreach (MethodInfo method in methods)
            {
                CheckMethod(method);
                CodeDoomProvider.AddHook(method ?? throw new ArgumentNullException(nameof(method)));
            }
        }

        public void BeginInvoke(string methodID, object instance, object[] arguments)
        {
            foreach (var handler in Handlers)
                handler.BeforeInvoke(Get(methodID), instance, arguments);
        }

        public void AfterInvoke(string methodID, object instance, object @return, Exception ex)
        {
            foreach (var handler in Handlers)
                handler.AfterInvoke(Get(methodID), instance, @return, ex);
        }

        public string Add(MethodInfo method)
        {
            string key = method.DeclaringType.AssemblyQualifiedName + ";" + method.Name;
            _methods[key] = method;
            return key;
        }

        public static MethodInfo[] GetMethods(Type type)
        {
            var result = type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic)
                   .Where(x => x.DeclaringType == type && CheckMethod(x, false))
                   .ToArray();

            return result;
        }

        public MethodInfo Get(string key)
        {
            if (_methods.TryGetValue(key, out MethodInfo value))
                return value;

            string[] parts = key.Split(';');

            var valueFind = BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic;

            return _methods[key] = Type.GetType(parts[0]).GetMethod(parts[1], valueFind) ??
                throw new Exception($"Не найден метод '{key}'");
        }

        public void Dispose()
        {
            try
            {
                UnInstall();
                Memory.CloseHandle();
            }
            finally
            {
                Current = null;
            }
        }

        public static LocalHookAdapter Current { get; private set; }

        public static LocalHookAdapter CreateInstance()
        {
            lock (_lock)
            {
                if (Current != null)
                    throw new Exception("Экземпляр уже создан");

                return Current = new LocalHookAdapter();
            }
        }

        public static unsafe T GetInstance<T>(IntPtr ptr)
            where T : class
        {
            T temp = default;
            TypedReference tr = __makeref(temp);
            Marshal.WriteIntPtr(*(IntPtr*)(&tr), ptr);
            return temp ?? throw new NullReferenceException();
        }

        public static unsafe object GetInstance(IntPtr ptr, Type type)
        {
            object temp = GetInstance(ptr);

            if (!type.IsInstanceOfType(temp))
                throw new InvalidCastException($" invalid cast type '{type.FullName}' to type '{temp.GetType()}' from address {ptr}");

            return temp;
        }

        public static unsafe object GetInstance(object obj) => obj;

        public static unsafe object GetInstance(IntPtr ptr)
        {
            object temp = default;
            TypedReference tr = __makeref(temp);
            Marshal.WriteIntPtr(*(IntPtr*)(&tr), ptr);

            if (temp == null)
                throw new NullReferenceException();

            return temp;
        }

    }
}