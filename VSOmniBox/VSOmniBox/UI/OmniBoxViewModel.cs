namespace VSOmniBox.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using VSOmniBox.API;
    using VSOmniBox.UI.Commands;

    // Ordinarily this class would expose properties to the view only for the data
    // that we wished to display. The actual interation with the data would happen
    // via the view, which has a direct hookup to the WPF input manager. In VS, however,
    // as long as the editor is visible and focused, it preempts everyone else as the
    // command handler. What this means is that for special non-TYPECHAR commands, such
    // as RETURN, BACKSPACE, COPY, PASTE, CUT, and SELECT ALL, input in our OmniBox is
    // redirected to the editor unless we also register a command chain node to the TextView
    // and intercept those commands and route them in. Unfortunely, WPF doesn't offer a way
    // to simulate keystrokes so I had to reimplement most of the high level TextBox behaviors
    // in this ViewModel as commands.
    internal sealed class OmniBoxViewModel : INotifyPropertyChanged
    {
        private readonly IOmniBoxBroker broker;

        private int caretIndex;
        private string searchString = string.Empty;
        private int selectedItemIndex = -1;

        public OmniBoxViewModel(IOmniBoxBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));

            // Initialize commands.
            this.BackspaceCommand = new BackspaceCommand(this);
            this.DismissCommand = new DismissCommand(this);
            this.DownCommand = new DownCommand(this);
            this.InvokeCommand = new InvokeCommand(this);
            this.LeftCommand = new LeftCommand(this);
            this.RightCommand = new RightCommand(this);
            this.UpCommand = new UpCommand(this);
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        public ICommand BackspaceCommand { get; }

        public ICommand DismissCommand { get; }

        public ICommand DownCommand { get; }

        public ICommand InvokeCommand { get; }

        public ICommand LeftCommand { get;  }

        public ICommand RightCommand { get; }

        public ICommand UpCommand { get; }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get => this.broker.IsVisible;
            set => this.broker.IsVisible = value;
        }

        public int CaretIndex
        {
            get => this.caretIndex;
            set
            {
                if (this.caretIndex != value)
                {
                    this.caretIndex = value;
                    this.NotifyPropertyChanged(nameof(CaretIndex));
                }
            }
        }

        public string SearchString
        {
            get => this.searchString;
            set
            {
                if (this.searchString != value)
                {
                    this.searchString = value ?? throw new ArgumentException($"{nameof(value)} cannot be null");
                    NotifyPropertyChanged(nameof(SearchString));
                }
            }
        }

        public int SelectedItemIndex
        {
            get => this.selectedItemIndex;
            set
            {
                if (this.selectedItemIndex != value)
                {
                    if (!this.IsValidSelectionIndex(value))
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    this.selectedItemIndex = value;
                    NotifyPropertyChanged(nameof(SelectedItemIndex));
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
