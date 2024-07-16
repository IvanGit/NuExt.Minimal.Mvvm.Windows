using System.Text;
using System.Windows;

namespace System.Diagnostics
{
    /// <summary>
    /// A custom <see cref="TraceListener"/> that captures binding errors and displays them in a message box.
    /// This listener is particularly useful for debugging purposes and identifying binding issues in WPF applications.
    /// </summary>
    /// <example>
    /// To use the <see cref="BindingErrorTraceListener"/>, add the following code to the startup of your WPF application,
    /// typically in the `App.xaml.cs` file or in the initialization logic of your main window:
    /// <code>
    /// public partial class App : Application
    /// {
    ///     protected override void OnStartup(StartupEventArgs e)
    ///     {
    ///         base.OnStartup(e);
    ///
    ///         // Enable tracing for data binding errors
    ///         PresentationTraceSources.Refresh();
    ///         PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
    ///         PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorTraceListener());
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class BindingErrorTraceListener : TraceListener
    {
        private readonly StringBuilder _messageBuilder = new();

        /// <summary>
        /// Writes a message to the trace listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string? message)
        {
            _messageBuilder.Append(message);
        }

        /// <summary>
        /// Writes a message to the trace listener and displays it in a message box with a warning icon.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string? message)
        {
            Write(message);

            MessageBox.Show(_messageBuilder.ToString(), "Binding error", MessageBoxButton.OK, MessageBoxImage.Warning);
            _messageBuilder.Clear();
        }
    }
}
