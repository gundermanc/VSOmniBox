namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;

    internal sealed class UpCommand : ICommand
    {
        private readonly OmniBoxViewModel model;

        public UpCommand(OmniBoxViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        // TODO: implement event handler.
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this.model.IsValidSelectionIndex(this.model.SelectedIndex - 1);
        }

        public void Execute(object parameter)
        {
            --this.model.SelectedIndex;
        }
    }
}
