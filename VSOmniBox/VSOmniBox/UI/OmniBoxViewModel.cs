namespace VSOmniBox.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using VSOmniBox.API;
    using VSOmniBox.UI.Commands;

    internal sealed class OmniBoxViewModel : INotifyPropertyChanged
    {
        private readonly IOmniBoxBroker broker;

        private int selectedIndex = -1;

        public OmniBoxViewModel(IOmniBoxBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Initialize commands.
            this.DismissCommand = new DismissCommand(this);
            this.DownCommand = new DownCommand(this);
            this.InvokeCommand = new InvokeCommand(this);
            this.UpCommand = new UpCommand(this);
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        public ICommand DismissCommand { get; }

        public ICommand DownCommand { get; }

        public ICommand InvokeCommand { get; }

        public ICommand UpCommand { get; }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get => this.broker.IsVisible;
            set => this.broker.IsVisible = value;
        }

        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                if (this.selectedIndex != value)
                {
                    if (!this.IsValidSelectionIndex(value))
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    this.selectedIndex = value;
                    NotifyPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public ObservableCollection<IOmniBoxItem> SearchResults { get; } = new ObservableCollection<IOmniBoxItem>();

        #endregion

        #region Public Methods

        public bool IsValidSelectionIndex(int value)
            => (value >= -1) && (value < this.SearchResults.Count);

        #endregion

        #region Private Impl

        private void NotifyPropertyChanged(string paramName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));

        #endregion
    }
}
