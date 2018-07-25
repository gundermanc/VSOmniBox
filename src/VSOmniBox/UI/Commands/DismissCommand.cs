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

#pragma warning disable 067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            this.ViewModel.IsVisible = false;
        }
    }
}
