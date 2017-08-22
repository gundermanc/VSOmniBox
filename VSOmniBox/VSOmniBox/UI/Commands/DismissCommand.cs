namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;
    using VSOmniBox.UI;

    internal sealed class DismissCommand : ICommand
    {
        public DismissCommand(OmniBoxViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public OmniBoxViewModel ViewModel { get; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            // TODO: technically not correct, but there is little chance we'll need to disable in common scenarios.
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.IsVisible = false;
        }
    }
}
