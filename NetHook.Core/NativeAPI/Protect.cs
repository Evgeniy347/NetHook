using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Core
{
    [Flags]
    public enum Protect
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
    }
}
