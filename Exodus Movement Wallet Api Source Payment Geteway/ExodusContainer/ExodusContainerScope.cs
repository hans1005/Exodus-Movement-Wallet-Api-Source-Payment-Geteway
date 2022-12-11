using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akeraiotitasoft.ExodusContainer
{
    internal class ExodusContainerScope : IExodusContainerScope
    {
        private readonly ExodusContainer _exodusContainer;
        public ExodusContainerScope(ExodusContainer exodusContainer)
        {
            _exodusContainer = exodusContainer;
        }

        public void Dispose()
        {
            _exodusContainer.RemoveScopeKey(this);
        }

        public object Resolve(Type type)
        {
            return Resolve(type, null);
        }

        public object Resolve(Type type, string name)
        {
            return _exodusContainer.Resolve(type, name, this);
        }
    }
}
