namespace VSOmniBox.UI.Commands
{
    using System;
    using System.Windows.Input;

    internal sealed class LeftCommand : ICommand
    {
        private readonly OmniBoxViewModel model;

        public LeftCommand(OmniBoxViewModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

#pragma warning disable 067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (this.model.CaretIndex > 0)
            {
                --this.model.CaretIndex;
            }
        }
    }
}
