namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;

    internal sealed class BackspaceCommand : ICommand
    {
        private readonly OmniBoxViewModel model;

        public BackspaceCommand(OmniBoxViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

#pragma warning disable 067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var originalCaretIndex = this.model.CaretIndex;
            var originalSearchString = this.model.SearchString;
            if (originalSearchString.Length > 0
                && originalCaretIndex > 0
                && originalCaretIndex <= this.model.SearchString.Length)
            {
                this.model.SearchString = originalSearchString.Remove(this.model.CaretIndex - 1, 1);
                this.model.CaretIndex = originalCaretIndex - 1;
            }
        }
    }
}
