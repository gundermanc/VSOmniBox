namespace VSOmniBox.UI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using VSOmniBox.API.Data;
    using VSOmniBox.API.UI;
    using VSOmniBox.Data;
    using VSOmniBox.UI.Commands;

    internal sealed class OmniBoxViewModel : INotifyPropertyChanged, IInvokable
    {
        private readonly IOmniBoxUIService broker;

        private string searchString = string.Empty;
        private int selectedItemIndex = -1;

        public OmniBoxViewModel(IOmniBoxUIService broker)
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

        // TODO: make reference data model directly.
        public IReadOnlyList<OmniBoxItem> SearchResults { get; private set; }

        #endregion

        #region Public Methods

        public bool IsValidSelectionIndex(int value)
            => (value >= -1) && (value < this.SearchResults.Count);

        public void UpdateFromSearchDataModel(SearchDataModel searchDataModel)
        {
            const int MaxResultsPerPivot = 3;

            var resultsListBuilder = ImmutableArray.CreateBuilder<OmniBoxItem>();

            if (searchDataModel.CodeItems.Length > 0)
            {
                resultsListBuilder.Add(new OmniBoxPivotItem(Strings.CodePivotItemTitle, string.Empty, action: null));
                resultsListBuilder.AddRange(searchDataModel.CodeItems.Take(MaxResultsPerPivot));
            }

            if (searchDataModel.IDEItems.Length > 0)
            {
                resultsListBuilder.Add(new OmniBoxPivotItem(Strings.IDEPivotItemTitle, string.Empty, action: null));
                resultsListBuilder.AddRange(searchDataModel.IDEItems.Take(MaxResultsPerPivot));
            }

            this.SearchResults = resultsListBuilder.ToImmutable();
            this.NotifyPropertyChanged(nameof(this.SearchResults));

            // Select first item, if there is one.
            this.SelectedItemIndex = this.SearchResults.Count > 0 ? 0 : -1;
        }

        #endregion

        #region Private Impl

        private void NotifyPropertyChanged(string paramName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));

        #endregion
    }
}
