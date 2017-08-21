namespace VSOmniBox.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using VSOmniBox.API;
    using VSOmniBox.UI.Commands;

    internal sealed class OmniBoxViewModel
    {
        private readonly IOmniBoxBroker broker;

        public OmniBoxViewModel(IOmniBoxBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Initialize commands.
            this.DismissCommand = new DismissCommand(this);
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        public ICommand DismissCommand { get; }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get => this.broker.IsVisible;
            set => this.broker.IsVisible = value;
        }

        public ObservableCollection<IOmniBoxItem> SearchResults { get; } = new ObservableCollection<IOmniBoxItem>();

        #endregion

        #region Private Impl

        private void NotifyPropertyChanged(string paramName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));

        #endregion
    }
}
