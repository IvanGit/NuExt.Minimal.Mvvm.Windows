namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides a base class for settings objects, extending the functionality of <see cref="ModelBase"/>.
    /// This class supports property change notifications, initialization, and dirty state management.
    /// It includes mechanisms to suspend and resume changes, as well as to mark the object as "dirty" when properties change.
    /// </summary>
    public abstract class SettingsBase : ModelBase
    {
        #region Internal Classes

        /// <summary>
        /// A helper class to manage the suspension of the "dirty" state.
        /// </summary>
        private class DirtySuspender : IDisposable
        {
            private readonly SettingsBase _this;

            public DirtySuspender(SettingsBase self)
            {
                _this = self;
                Interlocked.Increment(ref _this._isDirtySuspended);
            }

            public void Dispose()
            {
                Interlocked.Decrement(ref _this._isDirtySuspended);
            }
        }

        #endregion

        #region Properties

        private bool _isDirty;
        /// <summary>
        /// Gets a value indicating whether the object has been modified since its creation or last reset.
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            protected set => SetProperty(ref _isDirty, value);
        }

        private bool _isSuspended;
        /// <summary>
        /// Gets a value indicating whether property changes are currently suspended.
        /// </summary>
        public bool IsSuspended
        {
            get => _isSuspended;
            private set => SetProperty(ref _isSuspended, value);
        }

        private volatile int _isDirtySuspended;
        /// <summary>
        /// Gets a value indicating whether the dirty state tracking is currently suspended.
        /// </summary>
        private bool IsDirtySuspended => _isDirtySuspended != 0;

        #endregion

        #region Methods

        /// <summary>
        /// Validates the specified property value before it is set.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        protected virtual bool IsValidPropertyValue<T>(string? propertyName, T value)
        {
            return true;
        }

        private void MakeDirty(string? propertyName)
        {
            if (!IsInitialized || IsDirtySuspended ||
                propertyName is null or nameof(IsInitialized) or nameof(IsDirty) or nameof(IsSuspended))
            {
                return;
            }
            IsDirty = true;
        }

        /// <summary>
        /// Resets the dirty state of the object.
        /// </summary>
        public void ResetDirty()
        {
            IsDirty = false;
        }

        /// <summary>
        /// Resumes property change notifications after they were suspended.
        /// </summary>
        public void ResumeChanges()
        {
            IsSuspended = false;
        }

        protected override bool SetProperty<T>(ref T storage, T value, string? propertyName, out T oldValue)
        {
            if ((IsSuspended && propertyName is not nameof(IsSuspended)) || !IsValidPropertyValue(propertyName, value))
            {
                oldValue = default!;
                return false;
            }
            if (!base.SetProperty(ref storage, value, propertyName, out oldValue)) return false;
            MakeDirty(propertyName);
            return true;
        }

        /// <summary>
        /// Suspends property changes.
        /// </summary>
        public void SuspendChanges()
        {
            IsSuspended = true;
        }

        /// <summary>
        /// Suspends the dirty state tracking and returns a disposable object that resumes it when disposed.
        /// </summary>
        /// <returns>A disposable object that resumes dirty state tracking when disposed.</returns>
        public IDisposable SuspendDirty()
        {
            return new DirtySuspender(this);
        }

        #endregion
    }
}
