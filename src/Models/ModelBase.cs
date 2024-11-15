using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides a base class for models that supports property change notification and initialization.
    /// It leverages reflection to manage properties and provides methods to get property types, initialize the object,
    /// and set property values dynamically.
    /// </summary>
    [DataContract]
    public abstract class ModelBase : BindableBase
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>> s_typeProperties = new();

        private static readonly Func<Type, IDictionary<string, PropertyInfo>> s_typePropertiesFactory = GetProperties;

        #region Properties

        private bool _isInitialized;
        /// <summary>
        /// Gets a value indicating whether the object has been initialized.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                if (_isInitialized == value) return;
                _isInitialized = value;
                OnPropertyChanged(EventArgsCache.IsInitializedPropertyChanged);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the properties of the current type.
        /// </summary>
        /// <returns>A dictionary containing property information.</returns>
        protected IDictionary<string, PropertyInfo> GetProperties()
        {
            return s_typeProperties.GetOrAdd(GetType(), s_typePropertiesFactory);
        }

        /// <summary>
        /// Gets the properties of the specified type.
        /// </summary>
        /// <param name="type">The type to get properties for.</param>
        /// <returns>A dictionary containing property information.</returns>
        private static IDictionary<string, PropertyInfo> GetProperties(Type type)
        {
            var props = type.GetAllProperties(typeof(ModelBase), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var dict = new Dictionary<string, PropertyInfo>();
            foreach (var prop in props)
            {
                if (dict.ContainsKey(prop.Name))
                {
                    Debug.Assert(prop.GetGetMethod()!.IsVirtual);
                    continue;
                }
                dict.Add(prop.Name, prop);
            }
            return dict;
        }

        /// <summary>
        /// Gets the type of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The type of the property, or null if the property is not found.</returns>
        public Type? GetPropertyType(string propertyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is null or empty");
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
#else
            Throw.IfNullOrEmpty(propertyName);
#endif

            Debug.Assert(GetProperties().ContainsKey(propertyName), "propertyName is not defined");
            return GetProperties().TryGetValue(propertyName, out var pi) ? pi.PropertyType : null;
        }

        /// <summary>
        /// Initializes the object. This method can only be called once.
        /// </summary>
        /// <remarks>
        /// If the object is already initialized, this method does nothing.
        /// </remarks>
        public void Initialize()
        {
            Debug.Assert(!IsInitialized);
            if (IsInitialized)
            {
                return;
            }
            try
            {
                OnInitialize();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                throw;
            }
            IsInitialized = true;
        }

        /// <summary>
        /// Called during initialization to allow derived classes to perform custom initialization logic.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// Called during uninitialization to allow derived classes to perform custom uninitialization logic.
        /// </summary>
        protected virtual void OnUninitialize()
        {
        }

        /// <summary>
        /// Sets the value of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>true if the property value was set successfully; otherwise, false.</returns>
        public bool SetProperty(string propertyName, object? value)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is null or empty");
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
#else
            Throw.IfNullOrEmpty(propertyName);
#endif

            Debug.Assert(GetProperties().ContainsKey(propertyName), "propertyName is not defined");
            if (GetProperties().TryGetValue(propertyName, out var pi) && pi.CanWrite)
            {
                pi.SetValue(this, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Optionally uninitializes the object, performing necessary cleanup. This method can only be called once.
        /// </summary>
        /// <remarks>
        /// If the object is already uninitialized, this method does nothing. 
        /// </remarks>
        public void Uninitialize()
        {
            Debug.Assert(IsInitialized);
            if (!IsInitialized)
            {
                return;
            }
            try
            {
                OnUninitialize();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                throw;
            }
            IsInitialized = false;
        }

        #endregion
    }

    internal static partial class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs IsInitializedPropertyChanged = new(nameof(ModelBase.IsInitialized));
    }
}
