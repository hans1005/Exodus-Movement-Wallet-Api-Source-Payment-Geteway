using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akeraiotitasoft.ExodusContainer
{
    public class Registration
    {
        public Type From { get; set; }

        public Type To { get; set; }

        public IScopeGovernor ScopeGovernor {get;set; }

        public ConcurrentDictionary<WeakReference, ConcurrentDictionary<Type, object>> ScopedCache { get; set; }

        public Func<IExodusContainer, Type, string, object> CreateFunc { get; set; }
    }
}
