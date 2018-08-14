namespace VSOmniBox.UI
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Windows;
    using VSOmniBox.API.Data;
    using VSOmniBox.API.UI;
    using VSOmniBox.Data;

    [Export(typeof(IOmniBoxUIService))]
    internal sealed class OmniBoxBroker : IOmniBoxUIService
    {
        private readonly SearchController searchController;

        private OmniBoxViewModel model;
        private OmniBoxView view;

        [ImportingConstructor]
        public OmniBoxBroker(SearchController searchController)
        {
            this.searchController = searchController
                ?? throw new ArgumentNullException(nameof(searchController));
        }

        #region IOmniBoxBroker

        public bool IsVisible
        {
            get
            {
                return this.view?.IsVisible ?? false;
            }

            set
            {
                if (this.IsVisible != value)
                {
                    if (value)
                    {
                        this.searchController.WarmupFireAndForget();
                        this.CreateView();

                        // Populate with initial results.
                        this.searchController.StartOrUpdateSearch(
                            searchString: string.Empty,
                            pivot: OmniBoxPivot.Code | OmniBoxPivot.IDE | OmniBoxPivot.Help,
                            initialOnly: true);

                        this.view.Visibility = Visibility.Visible;
                        this.view.Activate();
                    }
                    else
                    {
                        var owner = this.view.Owner;
                        this.view.Close();
                        owner.Activate();
                    }
                }
            }
        }

        #endregion

        internal void RaiseRoutedEvent(RoutedEventArgs e) => this.view?.RaiseEvent(e);

        private void OnDeactivated(object sender, EventArgs e) => this.IsVisible = false;

        private void OnViewClosed(object sender, EventArgs e) => this.DestroyView();

        private void CreateView()
        {
            Debug.Assert(this.view == null);
            Debug.Assert(this.model == null);

            this.searchController.DataModelUpdated += this.OnDataModelUpdated;

            this.model = new OmniBoxViewModel(this);
            this.model.PropertyChanged += OnViewModelPropertyChanged;

            // TODO: better ownership behavior for multiple window scenarios. For now, main is king.
            this.view = new OmniBoxView()
            {
                DataContext = this.model,
                Owner = Application.Current.MainWindow
            };

            this.view.Closed += this.OnViewClosed;
            this.view.Deactivated += this.OnDeactivated;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OmniBoxViewModel.SearchString) || e.PropertyName == nameof(OmniBoxViewModel.Pivot))
            {
                this.searchController.StartOrUpdateSearch(this.model.SearchString, this.model.Pivot);
            }
        }

        private void OnDataModelUpdated(object sender, SearchDataModelUpdatedArgs e) => this.model.UpdateFromSearchDataModel(e.Model);

        private void DestroyView()
        {
            Debug.Assert(this.view != null);
            Debug.Assert(this.model != null);

            this.searchController.DataModelUpdated -= this.OnDataModelUpdated;
            this.searchController.StopSearch();

            this.model.PropertyChanged -= OnViewModelPropertyChanged;
            this.model = null;

            this.view.Closed -= OnViewClosed;
            this.view.Deactivated -= OnDeactivated;
            this.view = null;
        }
    }
}
