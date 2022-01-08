using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Core
{
    [Flags]
    public enum AllocType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
    }
}
