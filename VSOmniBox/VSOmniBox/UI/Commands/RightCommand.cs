namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;

    internal sealed class RightCommand : ICommand
    {
        private readonly OmniBoxViewModel model;

        public RightCommand(OmniBoxViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

#pragma warning disable 067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (this.model.CaretIndex < this.model.SearchString.Length)
            {
                ++this.model.CaretIndex;
            }
        }
    }
}
