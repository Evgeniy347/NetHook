using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Handlers.Trace
{
    public class TraceHandlerSetting
    {
        public TraceHandlerSetting()
        { }

        public int CountThreadBuffer { get; set; } = 10000;
    }
}
