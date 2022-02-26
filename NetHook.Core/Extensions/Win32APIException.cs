using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace NetHook.Cores.Extensions
{
    public class Win32APIException : Win32Exception
    {
        public Win32APIException()
            : this(Marshal.GetLastWin32Error())
        { }

        public Win32APIException(int error)
            : this(error, GetErrorMessage(error))
        {

        }


        public Win32APIException(string message)
            : this(Marshal.GetLastWin32Error(), GetNativeErrorCodeStr() + message)
        {
        }

        public Win32APIException(int error, string message)
            : base(error, GetNativeErrorCodeStr() + message)
        {
        }

        public Win32APIException(string message, Exception innerException)
            : base(GetNativeErrorCodeStr() + message, innerException)
        {
        }


        private static string GetErrorMessage(int error)
        {
            var methpd = typeof(Win32Exception).GetMethod("GetErrorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return GetNativeErrorCodeStr() + (string)methpd.Invoke(null, new object[] { error });
        }

        public static string GetNativeErrorCodeStr() => "NativeErrorCode: 0x" + Marshal.GetLastWin32Error().ToString("X") + " ";

    }
}
