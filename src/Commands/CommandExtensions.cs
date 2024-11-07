﻿using System.Reflection;
using System.Windows.Input;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides extension methods for handling commands.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
        /// </summary>
        /// <param name="command">The command to raise the CanExecuteChanged event for.</param>
        /// <exception cref="ArgumentNullException">Thrown when the command is null.</exception>
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            Throw.IfNull(command);

            if (command is IRelayCommand relayCommand)
            {
                relayCommand.RaiseCanExecuteChanged();
                return;
            }

            // Fallback for non-IRelayCommand: Raise CanExecuteChanged event manually if possible
            var eventFields = command.GetType().GetAllFields(typeof(object), BindingFlags.Instance | BindingFlags.NonPublic, fi => string.Equals(fi.Name, nameof(ICommand.CanExecuteChanged), StringComparison.OrdinalIgnoreCase));//TODO optimize
            if (eventFields.Count == 0) return;
            if (eventFields[0].GetValue(command) is not EventHandler eventHandler) return;
            eventHandler.Invoke(command, EventArgs.Empty);
        }
    }
}
