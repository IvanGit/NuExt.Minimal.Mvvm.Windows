using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base class for localizing message box buttons.
    /// </summary>
    public abstract class MessageBoxButtonLocalizerBase
    {
        /// <summary>
        /// Localizes the specified message box button.
        /// </summary>
        /// <param name="button">The message box button to localize.</param>
        /// <returns>The localized string representation of the message box button.</returns>
        public abstract string Localize(MessageBoxResult button);
    }

    /// <summary>
    /// A default implementation of <see cref="MessageBoxButtonLocalizerBase"/> that provides 
    /// basic localization for message box buttons.
    /// </summary>
    public class MessageBoxButtonLocalizer : MessageBoxButtonLocalizerBase
    {
        private static readonly MessageBoxButtonLocalizerBase s_default = new MessageBoxButtonLocalizer();
        private static MessageBoxButtonLocalizerBase? s_custom;

        /// <summary>
        /// Gets or sets the default instance of <see cref="MessageBoxButtonLocalizerBase"/>.
        /// If a custom instance is set, it will override the default instance.
        /// </summary>
        public static MessageBoxButtonLocalizerBase Default
        {
            get => s_custom ?? s_default;
            set => s_custom = value;
        }

        /// <summary>
        /// Localizes the specified message box button by returning its string representation.
        /// </summary>
        /// <param name="button">The message box button to localize.</param>
        /// <returns>The localized string representation of the message box button.</returns>
        public override string Localize(MessageBoxResult button)
        {
            return button switch
            {
                MessageBoxResult.OK => button.ToString(),
                MessageBoxResult.Cancel => button.ToString(),
                MessageBoxResult.Yes => button.ToString(),
                MessageBoxResult.No => button.ToString(),
                _ => string.Empty
            };
        }
    }
}
