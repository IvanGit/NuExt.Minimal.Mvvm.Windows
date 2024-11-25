using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for models that can be expanded or collapsed.
    /// </summary>
    public abstract partial class ExpandableBase : ModelBase, IExpandable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the object is expanded.
        /// </summary>
        [Notify, CustomAttribute("global::System.Text.Json.Serialization.JsonIgnore")]
        private bool _isExpanded;

        /// <summary>
        /// Collapses the object by setting the <see cref="IsExpanded"/> property to <c>false</c>.
        /// </summary>
        public virtual void Collapse()
        {
            IsExpanded = false;
        }

        /// <summary>
        /// Expands the object by setting the <see cref="IsExpanded"/> property to <c>true</c>.
        /// </summary>
        public virtual void Expand()
        {
            IsExpanded = true;
        }
    }
}
