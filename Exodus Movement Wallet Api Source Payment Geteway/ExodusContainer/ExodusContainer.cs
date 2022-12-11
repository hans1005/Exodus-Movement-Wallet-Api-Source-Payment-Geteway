using TypeReferenceEqualWeakReference = Akeraiotitasoft.ReferenceEqualWeakReference.ReferenceEqualWeakReference; // work around for name collision
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Collections.Concurrent;
#if LOGGING
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#endif
#if DEBUG
using System.Diagnostics;
#endif


namespace Akeraiotitasoft.ExodusContainer
{
    public class ExodusContainer : IExodusContainer
    {
        Timer _timer = new Timer();
        object _timerLockObject = new object();
        WeakReference _garbageCollectable = new WeakReference(new object());

#if LOGGING
        private ILogger<ExodusContainer> _logger;

        private ILogger<ExodusContainer> Logger
        {
            get
            {
                try
                {
                    if (_logger != null)
                    {
                        return _logger;
                    }
                    var registration = FindRegistration(typeof(ILogger<ExodusContainer>));
                    if (registration != null)
                    {
                        object scope = registration.ScopeGovernor.GetScope(this, null);
                        if (object.ReferenceEquals(scope, this))
                        {
                            // it is a singleton.  So save time in the next call
                            _logger = (ILogger<ExodusContainer>)Resolve(typeof(ILogger<ExodusContainer>));
                            return _logger;
                        }
                        else
                        {
                            // we could have a transient logger or a per thread or per request logger
                            // or a different scope for that matter
                            return (ILogger<ExodusContainer>)Resolve(typeof(ILogger<ExodusContainer>));
                        }
                    }
                }
#if DEBUG
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    Debug.WriteLine("Error in obtaining logger " + ex.ToString());
#endif
#if !DEBUG
                catch
                {
#endif
                // we can do nothing because this is logging
                // we don't want to bring down the app because of this
            }
                return NullLogger<ExodusContainer>.Instance;
            }
        }
#endif

        public ExodusContainer()
        {
            _timer.Interval = 5000; // 5 seconds
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

#if LOGGING
        public ExodusContainer(ILogger<ExodusContainer> logger)
        {
            _logger = logger;
            _timer.Interval = 5000; // 5 seconds
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }
#endif

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
#if LOGGING
                Logger.LogDebug("_timer_Elapsed: Timer Elapsed for Disposing Dead Scopes");
#endif
                if (!_garbageCollectable.IsAlive)
                {
#if LOGGING
                    Logger.LogDebug("_timer_Elapsed: the weak reference indicated that it has been garbage collected.");
#endif
                    lock (_timerLockObject)
                    {
#if LOGGING
                        Logger.LogDebug("_timer_Elapsed: entered the lock");
#endif
                        if (!_garbageCollectable.IsAlive)
                        {
#if LOGGING
                            Logger.LogDebug("_timer_Elapsed: within the lock, the weak reference target is still not recreated");
#endif
                            try
                            {
                                _timer.Stop();
                                TryRemovingDeadScopes();
                            }
#if LOGGING
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "_timer_Elapsed: caught exception");
                                throw;
                            }
#endif
                            finally
                            {
                                _garbageCollectable.Target = new object();
                                _timer.Start();
                            }
                        }
#if LOGGING
                        else
                        {
                            Logger.LogDebug("_timer_Elapsed: within the lock, the weak reference target was already recreated");
                        }

                        Logger.LogDebug("_timer_Elapsed: end of the lock");
#endif
                    }
                }
#if LOGGING
                Logger.LogDebug("_timer_Elapsed: the method is completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "_timer_Elapsed: caught exception");
#else
            }
            catch
            {
                // not logging so just do nothing
                // exceptions with the timer method should not
                // bring down your application
#endif
            }
        }

        private void TryRemovingDeadScopes()
        {
#if LOGGING
            Logger.LogDebug("TryRemovingDeadScopes: Tries to remove dead scopes");
            try
            {
#endif
                foreach (var registration in _registrations.Values)
                {
                    WeakReference[] scopeKeys = registration.ScopedCache.Keys.ToArray();
                    foreach (WeakReference scopeKey in scopeKeys)
                    {
                        if (!scopeKey.IsAlive)
                        {
                            ConcurrentDictionary<Type, object> typeDictionary = registration.ScopedCache[scopeKey];
                            if (typeDictionary != null)
                            {
                                Type[] typeKeys = typeDictionary.Keys.ToArray();
                                foreach (Type typeKey in typeKeys)
                                {
                                    object obj = typeDictionary[typeKey];
                                    if (obj != null && obj is IDisposable disposable)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }
                            //registration.ScopedCache.Remove(scopeKey);
                            ConcurrentDictionary<Type, object> dummy; // just to satisfy method signature, discarded.
                            registration.ScopedCache.TryRemove(scopeKey, out dummy);
                        }
                    }
                }
                ConcurrentBag<WeakReference> oldTransientList = _disposableTransients;
                _disposableTransients = new ConcurrentBag<WeakReference>();
                foreach (WeakReference weakReference in oldTransientList)
                {
                    if (weakReference.IsAlive)
                    {
                        _disposableTransients.Add(weakReference);
                    }
                }
                
#if LOGGING
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TryRemovingDeadScopes: caught exception");
            }
            Logger.LogDebug("TryRemovingDeadScopes: the method completed");
#endif
        }

        public void RemoveScopeKey(object scope)
        {
            TypeReferenceEqualWeakReference scopeKey = new TypeReferenceEqualWeakReference(scope);
            foreach (var registration in _registrations.Values)
            {
                if (registration.ScopedCache.ContainsKey(scopeKey))
                {
                    ConcurrentDictionary<Type, object> typeDictionary = registration.ScopedCache[scopeKey];
                    if (typeDictionary != null)
                    {
                        Type[] typeKeys = typeDictionary.Keys.ToArray();
                        foreach (Type typeKey in typeKeys)
                        {
                            object obj = typeDictionary[typeKey];
                            if (obj != null && obj is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    //registration.ScopedCache.Remove(scopeKey);
                    ConcurrentDictionary<Type, object> dummy; // just to satisfy method signature, discarded.
                    registration.ScopedCache.TryRemove(scopeKey, out dummy);
                }
            }
        }

        private ConcurrentDictionary<RegistrationKey, Registration> _registrations = new ConcurrentDictionary<RegistrationKey, Registration>();
        private ConcurrentDictionary<RegistrationKey, object> _singletons = new ConcurrentDictionary<RegistrationKey, object>();
        private ConcurrentBag<WeakReference> _disposableTransients = new ConcurrentBag<WeakReference>();
        public void Register(Type from, Type to, IScopeGovernor scopeGovernor)
        {
            Register(from, to, null, scopeGovernor);
        }

        public void Register(Type from, Type to, string name, IScopeGovernor scopeGovernor)
        {
            Registration registration = new Registration();
            registration.From = from;
            registration.To = to;
            registration.ScopedCache = new ConcurrentDictionary<WeakReference, ConcurrentDictionary<Type, object>>();
            registration.ScopeGovernor = scopeGovernor;
            //_registrations.Add(new RegistrationKey() { Type = from, Name = name }, registration);
            _registrations.TryAdd(new RegistrationKey() { Type = from, Name = name }, registration);
        }

        public void Register(Type from, IScopeGovernor scopeGovernor, Func<IExodusContainer, Type, string, object> createFunc)
        {
            Register(from, null, scopeGovernor, createFunc);
        }

        public void Register(Type from, string name, IScopeGovernor scopeGovernor, Func<IExodusContainer, Type, string, object> createFunc)
        {
            Registration registration = new Registration();
            registration.From = from;
            registration.CreateFunc = createFunc;
            registration.ScopedCache = new ConcurrentDictionary<WeakReference, ConcurrentDictionary<Type, object>>();
            registration.ScopeGovernor = scopeGovernor;
            //_registrations.Add(new RegistrationKey() { Type = from, Name = name }, registration);
            _registrations.TryAdd(new RegistrationKey() { Type = from, Name = name }, registration);
        }

        public bool IsRegistered(Type type)
        {
            return IsRegistered(type, null);
        }

        public bool IsRegistered(Type type, string name)
        {
            return FindRegistration(type, name) != null;
        }

        public object Resolve(Type type)
        {
            return Resolve(type, null);
        }

        public object Resolve(Type type, string name)
        {
            return Resolve(type, name, null);
        }

        internal object Resolve(Type type, string name, IExodusContainerScope exodusContainerScope)
        {
            Registration registration;
            if ((registration = FindRegistration(type, name)) != null)
            {
                object scope = registration.ScopeGovernor.GetScope(this, exodusContainerScope);
                if (scope == null)
                {
                    object obj = CreateType(type, name, registration);
                    if (obj is IDisposable disposable)
                    {
                        _disposableTransients.Add(new WeakReference(disposable));
                    }
                    return obj;
                }
                WeakReference weakReference = new TypeReferenceEqualWeakReference(scope);
                scope = null; // don't keep a reference
                ConcurrentDictionary<Type, object> typeCache;
                if (registration.ScopedCache.ContainsKey(weakReference))
                {
                    typeCache = registration.ScopedCache[weakReference];
                    if (typeCache != null && typeCache.ContainsKey(type))
                    {
                        return typeCache[type];
                    }
                }
                object savable = CreateType(type, name, registration);
                if (!registration.ScopedCache.ContainsKey(weakReference))
                {
                    typeCache = new ConcurrentDictionary<Type, object>();
                    registration.ScopedCache.TryAdd(weakReference, typeCache);
                }
                else
                {
                    typeCache = registration.ScopedCache[weakReference];
                    if (typeCache == null)
                    {
                        typeCache = new ConcurrentDictionary<Type, object>();
                        registration.ScopedCache[weakReference] = typeCache;
                    }
                }
                //typeCache.Add(type, savable);
                typeCache.TryAdd(type, savable);
                return savable;
            }
            throw new InvalidOperationException($"No registration for type {type.FullName}.");
        }

        public IExodusContainerScope BeginScope()
        {
            return new ExodusContainerScope(this);
        }

        private Registration FindRegistration(Type type)
        {
            return FindRegistration(type, null);
        }

        private Registration FindRegistration(Type type, string name)
        {
            RegistrationKey key = new RegistrationKey() { Type = type, Name = name };
            if (_registrations.ContainsKey(key))
            {
                return _registrations[key];
            }
            if (key.Type.IsGenericType)
            {
                Type genericType = key.Type.GetGenericTypeDefinition();
                RegistrationKey genericKey = new RegistrationKey() { Type = genericType, Name = key.Name };
                if (_registrations.ContainsKey(genericKey))
                {
                    return _registrations[genericKey];
                }
            }

            return null;
        }

        private object CreateType(Type resolve, string name, Registration registration)
        {
            if (registration.CreateFunc == null)
            {
                if (registration.From.ContainsGenericParameters)
                {
                    Type[] genericArguments = resolve.GetGenericArguments();
                    Type genericTo = registration.To.MakeGenericType(genericArguments);
                    if (!genericTo.ContainsGenericParameters && resolve.IsAssignableFrom(genericTo))
                    {
                            return CallConstructor(genericTo);
                    }
                }
                else if (resolve == registration.From)
                {
                    return CallConstructor(registration.To);
                }
                throw new InvalidOperationException("I don't know how to construct this.");
            }
            else
            {
                return registration.CreateFunc(this, resolve, name);
            }
        }

        private object CallConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"{type.FullName} does not have any public constructors.");
            }
            int maxParameters = constructors.Max(x => x.GetParameters().Length);
            ConstructorInfo longestConstructor = constructors.First(x => x.GetParameters().Length == maxParameters);
            object[] parameterValues = new object[maxParameters];
            var parameters = longestConstructor.GetParameters();
            for (int i = 0; i < maxParameters; i++)
            {
                var parameter = parameters[i];
                parameterValues[i] = Resolve(parameter.ParameterType);
            }
            return longestConstructor.Invoke(parameterValues);
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
#if LOGGING

#endif
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var registration in _registrations.Values)
                    {
                        foreach (var typeDictionary in registration.ScopedCache)
                        {
                            foreach (var cache in typeDictionary.Value)
                            {
                                IDisposable disposable = cache.Value as IDisposable;
                                disposable?.Dispose();
                            }
                        }
                    }
                    foreach (var disposableWeakReference in _disposableTransients)
                    {
                        if (disposableWeakReference.Target is IDisposable disposable)
                        {
                            disposable?.Dispose();
                        }
                    }
                    _registrations = new ConcurrentDictionary<RegistrationKey, Registration>();
                    _disposableTransients = new ConcurrentBag<WeakReference>();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ExodusContainer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
#endregion
    }
}
