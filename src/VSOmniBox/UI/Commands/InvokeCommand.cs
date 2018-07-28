namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;

    internal sealed class InvokeCommand : ICommand
    {
        private readonly OmniBoxViewModel model;

        public InvokeCommand(OmniBoxViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

#pragma warning disable 067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (this.model.IsValidSelectionIndex(this.model.SelectedItemIndex) &&
                (this.model.SelectedItemIndex > -1))
            {
                this.model.IsVisible = false;
                this.model.SearchResults[this.model.SelectedItemIndex].Invoke();
            }
        }
    }
}
