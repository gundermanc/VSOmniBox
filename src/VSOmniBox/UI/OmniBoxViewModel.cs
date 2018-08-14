namespace VSOmniBox.UI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using VSOmniBox.API.Data;
    using VSOmniBox.API.UI;
    using VSOmniBox.Data;
    using VSOmniBox.UI.Commands;

    internal sealed class OmniBoxViewModel : INotifyPropertyChanged, IInvokable, IPivotable
    {
        private const OmniBoxPivot AllItemsPivot = (OmniBoxPivot.Code | OmniBoxPivot.IDE | OmniBoxPivot.Help);
        private readonly IOmniBoxUIService broker;

        private string searchString = string.Empty;
        private int selectedItemIndex = -1;
        private SearchDataModel searchDataModel = SearchDataModel.Empty;
        private IReadOnlyList<OmniBoxItem> searchResults;
        private OmniBoxPivot currentPivot = OmniBoxPivot.Code | OmniBoxPivot.IDE | OmniBoxPivot.Help;

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

        public IReadOnlyList<OmniBoxItem> SearchResults
        {
            get => this.searchResults;
            private set
            {
                if (this.SearchResults != value)
                {
                    this.searchResults = value ?? throw new ArgumentNullException(nameof(value));
                    this.NotifyPropertyChanged(nameof(this.SearchResults));

                    this.SelectFirstNonPivot();
                }
            }
        }

        public OmniBoxPivot Pivot
        {
            get => this.currentPivot;
            set
            {
                if (this.currentPivot != value)
                {
                    this.currentPivot = value;
                    this.UpdateFromSearchDataModel(this.searchDataModel);
                    this.NotifyPropertyChanged(nameof(Pivot));
                }
            }
        }

        public bool IsAllItemsPivot
        {
            get => this.Pivot == AllItemsPivot;
            set
            {
                if (this.Pivot != AllItemsPivot)
                {
                    this.Pivot = AllItemsPivot;
                    this.NotifyPropertyChanged(nameof(IsAllItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsCodeItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsIDEItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsHelpItemsPivot));
                }
            }
        }

        public bool IsCodeItemsPivot
        {
            get => this.Pivot == OmniBoxPivot.Code;
            set
            {
                if (this.Pivot != OmniBoxPivot.Code)
                {
                    this.Pivot = OmniBoxPivot.Code;
                    this.NotifyPropertyChanged(nameof(IsAllItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsCodeItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsIDEItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsHelpItemsPivot));
                }
            }
        }

        public bool IsIDEItemsPivot
        {
            get => this.Pivot == OmniBoxPivot.IDE;
            set
            {
                if (this.Pivot != OmniBoxPivot.IDE)
                {
                    this.Pivot = OmniBoxPivot.IDE;
                    this.NotifyPropertyChanged(nameof(IsAllItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsCodeItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsIDEItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsHelpItemsPivot));
                }
            }
        }

        public bool IsHelpItemsPivot
        {
            get => this.Pivot == OmniBoxPivot.Help;
            set
            {
                if (this.Pivot != OmniBoxPivot.Help)
                {
                    this.Pivot = OmniBoxPivot.Help;
                    this.NotifyPropertyChanged(nameof(IsAllItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsCodeItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsIDEItemsPivot));
                    this.NotifyPropertyChanged(nameof(IsHelpItemsPivot));
                }
            }
        }

        #endregion

        #region Public Methods

        public bool IsValidSelectionIndex(int value)
            => (value >= -1) && (value < this.SearchResults.Count);

        public void UpdateFromSearchDataModel(SearchDataModel searchDataModel)
        {
            const int MaxResultsPerPivot = 3;
            const int MaxInitialResultsPerPivot = 5;

            this.searchDataModel = searchDataModel;

            // Show the full list if only one pivot selected.
            if ((this.Pivot.HasFlag(OmniBoxPivot.Code) && !this.Pivot.HasFlag(OmniBoxPivot.IDE) && !this.Pivot.HasFlag(OmniBoxPivot.Help)) ||
                (!this.Pivot.HasFlag(OmniBoxPivot.Code) && this.Pivot.HasFlag(OmniBoxPivot.IDE) && !this.Pivot.HasFlag(OmniBoxPivot.Help)) ||
                (!this.Pivot.HasFlag(OmniBoxPivot.Code) && !this.Pivot.HasFlag(OmniBoxPivot.IDE) && this.Pivot.HasFlag(OmniBoxPivot.Help)))
            {
                switch (this.Pivot)
                {
                    case OmniBoxPivot.Code:
                        this.SearchResults = searchDataModel.CodeItems;
                        break;
                    case OmniBoxPivot.IDE:
                        this.SearchResults = searchDataModel.IDEItems;
                        break;
                    case OmniBoxPivot.Help:
                        this.SearchResults = searchDataModel.HelpItems;
                        break;
                    default:
                        Debug.Fail("Unknown case");
                        break;
                }

                return;
            }

            var initialResults = this.SearchString.Length == 0;
            var maxResults = initialResults ? MaxInitialResultsPerPivot : MaxResultsPerPivot;
            var resultsListBuilder = ImmutableArray.CreateBuilder<OmniBoxItem>();

            if (this.Pivot.HasFlag(OmniBoxPivot.Code) && searchDataModel.CodeItems.Length > 0)
            {
                if (!initialResults)
                {
                    resultsListBuilder.Add(
                        new OmniBoxPivotItem(
                            Strings.CodePivotItemTitle,
                            description: string.Empty));
                }
                resultsListBuilder.AddRange(searchDataModel.CodeItems.Take(maxResults));
            }

            if (this.Pivot.HasFlag(OmniBoxPivot.IDE) && searchDataModel.IDEItems.Length > 0)
            {
                if (!initialResults)
                {
                    resultsListBuilder.Add(
                        new OmniBoxPivotItem(
                            Strings.IDEPivotItemTitle,
                            description: string.Empty));
                }
                resultsListBuilder.AddRange(searchDataModel.IDEItems.Take(maxResults));
            }

            if (this.Pivot.HasFlag(OmniBoxPivot.Help) && searchDataModel.HelpItems.Length > 0)
            {
                if (!initialResults)
                {
                    resultsListBuilder.Add(
                        new OmniBoxPivotItem(
                            Strings.HelpPivotItemTitle,
                            description: string.Empty));
                }
                resultsListBuilder.AddRange(searchDataModel.HelpItems.Take(maxResults));
            }


            this.SearchResults = resultsListBuilder.ToImmutable();
        }

        #endregion

        #region Private Impl

        private void SelectFirstNonPivot()
        {
            int newSelectedIndex = -1;

            for (int i = 0; i < this.SearchResults.Count; i++)
            {
                if (!(this.SearchResults[i] is OmniBoxPivotItem))
                {
                    newSelectedIndex = i;
                    break;
                }
            }

            this.SelectedItemIndex = newSelectedIndex;
        }

        private void NotifyPropertyChanged(string paramName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));

        #endregion
    }
}
