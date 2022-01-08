using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Core
{
    [Flags]
    public enum FreeType
    {
        Decommit = 0x4000,
        Release = 0x8000,
    }
}
