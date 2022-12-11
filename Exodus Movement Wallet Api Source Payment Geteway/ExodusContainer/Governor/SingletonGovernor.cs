using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akeraiotitasoft.ExodusContainer.Governor
{
    public class SingletonGovernor : IScopeGovernor
    {
        public object GetScope(IExodusContainer exodusContainer, IExodusContainerScope exodusContainerScope)
        {
            return exodusContainer;
        }
    }
}
