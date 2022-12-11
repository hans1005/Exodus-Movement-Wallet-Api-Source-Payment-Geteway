using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Akeraiotitasoft.ExodusContainer.Governor
{
    public class PerThreadGovernor : IScopeGovernor
    {
        public object GetScope(IExodusContainer exodusContainer, IExodusContainerScope exodusContainerScope)
        {
            return Thread.CurrentThread;
        }
    }
}
