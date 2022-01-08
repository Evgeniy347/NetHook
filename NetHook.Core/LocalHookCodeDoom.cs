using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace NetHook.Core
{
    public class LocalHookCodeDoom
    {
        private CodeCompileUnit _targetUnit;
        private CodeNamespace _targetNameSpace;

        public LocalHookCodeDoom()
        {
            _targetUnit = new CodeCompileUnit();
            _targetNameSpace = new CodeNamespace("LocalHookProviders");
            _targetNameSpace.Imports.Add(new CodeNamespaceImport("System"));
            _targetNameSpace.Imports.Add(new CodeNamespaceImport(typeof(LocalHookCodeDoom).Namespace));
            _targetNameSpace.Imports.Add(new CodeNamespaceImport(typeof(BindingFlags).Namespace));
            _targetNameSpace.Imports.Add(new CodeNamespaceImport(typeof(PrePrepareMethodAttribute).Namespace));
            _targetUnit.Namespaces.Add(_targetNameSpace);
        }

        public void AddHook(MethodInfo methodInfo)
        {
            CodeTypeDeclaration targetClass = new CodeTypeDeclaration($"LocalHookProvider_{methodInfo.Name}_{_targetNameSpace.Types.Count}");
            targetClass.IsClass = true;
            targetClass.BaseTypes.Add(new CodeTypeReference(typeof(LocalHookRuntimeInstance)));
            targetClass.TypeAttributes =
                TypeAttributes.Public | TypeAttributes.Sealed;

            CreateConstructor(targetClass, methodInfo);

            CreateMethodHook(targetClass, methodInfo);

            _targetNameSpace.Types.Add(targetClass);
        }

        private void CreateMethodHook(CodeTypeDeclaration targetClass, MethodInfo methodInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodInfo.Name + "_Hook";

            bool isStatic = methodInfo.IsStatic;

            method.Attributes =
                MemberAttributes.Private | MemberAttributes.Static;

            method.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(PrePrepareMethodAttribute))));

            var parameters = methodInfo.GetParameters();

            if (!isStatic)
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "thisObj"));

            int i = 0;
            foreach (var parameter in parameters)
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), $"arg_{i++}"));

            string paramsStr = string.Join(", ", parameters.Select((x, y) => $"arg_{y}").ToArray());
            string objectArray = string.IsNullOrEmpty(paramsStr) ? "object[] objectArray = null" :
                $"object[] objectArray = new object[] {{ {paramsStr} }}";

            method.Statements.Add(new CodeSnippetExpression(objectArray));

            method.Statements.Add(new CodeSnippetExpression("object value = IntPtr.Zero"));
            method.Statements.Add(new CodeSnippetExpression("Exception e = null"));


            if (isStatic)
                method.Statements.Add(new CodeSnippetExpression("object thisObj = null"));
            else
                paramsStr = string.IsNullOrEmpty(paramsStr) ? "thisObj" : $"thisObj, {paramsStr}";

            method.Statements.Add(new CodeSnippetExpression($"LocalHookAdapter.Current.BeginInvoke(\"{LocalHookAdapter.Current.Add(methodInfo)}\", thisObj, objectArray)"));

            CodeTryCatchFinallyStatement tryCatchFinaly = new CodeTryCatchFinallyStatement();
            method.Statements.Add(tryCatchFinaly);

            if (methodInfo.ReturnType != typeof(void))
            {
                method.ReturnType = new CodeTypeReference(typeof(object));
                tryCatchFinaly.TryStatements.Add(new CodeSnippetExpression($"if (objectArray.Length > 0) value = {method.Name}({paramsStr})"));
                tryCatchFinaly.TryStatements.Add(new CodeSnippetExpression($"else value = {method.Name}({paramsStr})"));
            }
            else
            {
                tryCatchFinaly.TryStatements.Add(new CodeSnippetExpression($"if (objectArray.Length > 0) {method.Name}({paramsStr})"));
                tryCatchFinaly.TryStatements.Add(new CodeSnippetExpression($"else {method.Name}({paramsStr})"));
            }

            CodeCatchClause catchEx = new CodeCatchClause("ex", new CodeTypeReference(typeof(Exception)));
            tryCatchFinaly.CatchClauses.Add(catchEx);

            catchEx.Statements.Add(new CodeSnippetExpression("e = ex"));
            catchEx.Statements.Add(new CodeThrowExceptionStatement());

            tryCatchFinaly.FinallyStatements.Add(new CodeSnippetExpression($"LocalHookAdapter.Current.AfterInvoke(\"{LocalHookAdapter.Current.Add(methodInfo)}\", thisObj, value, e)"));

            if (methodInfo.ReturnType != typeof(void))
                method.Statements.Add(new CodeSnippetExpression($"return value"));

            targetClass.Members.Add(method);
        }

        private void CreateConstructor(CodeTypeDeclaration targetClass, MethodInfo methodInfo)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;

            constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(LocalHookAdapter), "adapter"));

            constructor.BaseConstructorArgs.Add(
               new CodeSnippetExpression($"{nameof(LocalHookAdapter)}.{nameof(LocalHookAdapter.Current)}.{nameof(LocalHookAdapter.Get)}(\"{LocalHookAdapter.Current.Add(methodInfo)}\")"));

            constructor.BaseConstructorArgs.Add(
                       new CodeSnippetExpression($" typeof({targetClass.Name}).GetMethod(\"{methodInfo.Name}_Hook\", BindingFlags.Static | BindingFlags.NonPublic)"));

            targetClass.Members.Add(constructor);
        }

        public Assembly Compiller()
        {
            using (CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                CompilerParameters options = new CompilerParameters();

                string[] locations = AppDomain.CurrentDomain.GetAssemblies().Select(TryLocation)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                options.ReferencedAssemblies.AddRange(locations);

                CompilerResults result = provider.CompileAssemblyFromDom(options, _targetUnit);

                try
                {
                    return result.CompiledAssembly;
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var line in result.Errors)
                        stringBuilder.AppendLine(line.ToString());

                    throw new Exception(stringBuilder.ToString(), ex);
                }
            }
        }

        public string TryLocation(Assembly assembly)
        {
            try
            {
                return assembly.Location;
            }
            catch
            {
                return null;
            }
        }

        public string Save()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";

            using (StringWriter sourceWriter = new StringWriter())
            {
                provider.GenerateCodeFromCompileUnit(
                    _targetUnit, sourceWriter, options);

                return sourceWriter.GetStringBuilder().ToString();
            }
        }
    }
}