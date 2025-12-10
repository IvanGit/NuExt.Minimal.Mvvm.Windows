using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// A class that locates and initializes views based on view models, inheriting from <see cref="ViewLocatorBase"/>.
    /// </summary>
    public class ViewLocator : ViewLocatorBase
    {
        private static readonly ViewLocatorBase s_default = new ViewLocator();
        private static ViewLocatorBase? s_custom;

        private readonly ConcurrentDictionary<string, Type> _registeredTypes = new();
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new();

        #region Properties

        /// <summary>
        /// Gets the assemblies to search for view types.
        /// </summary>
        protected IEnumerable<Assembly> Assemblies => GetAssemblies();

        /// <summary>
        /// Gets or sets the default instance of the <see cref="ViewLocatorBase"/>.
        /// </summary>
        public static ViewLocatorBase Default
        {
            get => s_custom ?? s_default;
            set => s_custom = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the internal cache of view types.
        /// </summary>
        public void ClearCache()
        {
            _cachedTypes.Clear();
        }

        /// <summary>
        /// Gets the assemblies to search for view types. This method can be overridden to customize the assembly collection.
        /// </summary>
        /// <returns>An enumerable collection of assemblies.</returns>
        protected virtual IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Gets the type of the view based on the specified view name.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>The type of the view if found; otherwise, null.</returns>
        protected override Type? GetViewType(string? viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                return null;
            }
            if (_registeredTypes.TryGetValue(viewName!, out var registeredType))
            {
                return registeredType;
            }
            if (_cachedTypes.TryGetValue(viewName!, out var cachedType))
            {
                return cachedType;
            }
            foreach (var type in GetTypes())
            {
                Debug.Assert(type != null);
                if (type!.Name == viewName)
                {
                    return (_cachedTypes[type.Name] = type);
                }
                if (type.FullName == viewName)
                {
                    return (_cachedTypes[type.FullName] = type);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all types from the assemblies specified by <see cref="Assemblies"/>.
        /// </summary>
        /// <returns>An enumerable collection of types.</returns>
        protected virtual IEnumerable<Type> GetTypes()
        {
            foreach (Assembly assembly in Assemblies)
            {
                Type?[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                foreach (var type in types)
                {
                    if (type == null)
                    {
                        continue;
                    }
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Registers a view type with a specified name.
        /// </summary>
        /// <param name="name">The name to associate with the view type.</param>
        /// <param name="type">The type of the view to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if the name or type is null.</exception>
        public void RegisterType(string name, Type type)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(type);

            _registeredTypes[name] = type;
        }

        /// <summary>
        /// Unregisters a view type with a specified name.
        /// </summary>
        /// <param name="name">The name associated with the view type to unregister.</param>
        /// <returns>True if the view type was successfully unregistered; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the name is null or empty.</exception>
        public bool UnregisterType(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            return _registeredTypes.TryRemove(name, out _);
        }

        #endregion
    }
}
