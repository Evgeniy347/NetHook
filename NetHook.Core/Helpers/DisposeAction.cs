using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Core.Helpers
{
    /// <summary>
    /// Класс для вызова Action в dispose
    /// </summary>
    public class DisposeAction<TArgument> : IDisposable
    {
        private readonly TArgument _argument;
        private readonly Action<TArgument> _action;
        private bool _disposeObject;
        public DisposeAction(TArgument argument, Action<TArgument> action)

        {
            _argument = argument;

            _action = action ??
                throw new ArgumentNullException(nameof(action));
        }

        public void Dispose()
        {
            if (_disposeObject)
                throw new ObjectDisposedException("parent object");

            _action(_argument);
            _disposeObject = true;
        }
    }
}
