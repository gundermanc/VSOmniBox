namespace VSOmniBox.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using VSOmniBox.API;
    using VSOmniBox.UI;

    internal sealed class OmniBoxBroker : IOmniBoxBroker, IOmniBoxSearchCallback
    {
        private readonly IEnumerable<IOmniBoxSearchProviderFactory> searchProviderFactories;

        private IEnumerable<IOmniBoxSearchProvider> searchProviders;

        private OmniBoxView view;
        private OmniBoxViewModel model;
        private SearchTask currentSearch;

        public OmniBoxBroker(IEnumerable<IOmniBoxSearchProviderFactory> searchProviderFactories)
        {
            this.searchProviderFactories = searchProviderFactories
                ?? throw new ArgumentNullException(nameof(searchProviderFactories));
        }

        public bool IsVisible
        {
            get => this.view?.IsVisible ?? false;
            set
            {
                if (this.IsVisible != value)
                {
                    if (value)
                    {
                        this.CreateView();

                        this.view.Visibility = Visibility.Visible;
                        this.view.Activate();
                    }
                    else
                    {
                        this.view.Close();
                    }
                }
            }
        }

        public void StartOrUpdateSearch(string searchQuery)
        {
            this.StopSearch();
            this.model.SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                this.currentSearch = SearchTask.StartNew(searchQuery, this, this.SearchProviders);
            }
        }

        public void StopSearch()
        {
            this.currentSearch?.Cancel();
            this.currentSearch = null;
        }

        private IEnumerable<IOmniBoxSearchProvider> SearchProviders
            => this.searchProviders ??
            (searchProviders = this.searchProviderFactories.Select(factory => factory.GetSearchProvider())).ToList();

        public bool IsSearchInProgress => this.currentSearch != null;

        private void OnDeactivated(object sender, EventArgs e) => this.IsVisible = false;

        private void OnViewClosed(object sender, EventArgs e) => this.DestroyView();

        private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.StartOrUpdateSearch(this.view.SearchTextBox.Text);
        }

        private void CreateView()
        {
            Debug.Assert(this.view == null);
            Debug.Assert(this.model == null);

            this.model = new OmniBoxViewModel(this);

            this.view = new OmniBoxView()
            {
                DataContext = this.model,
                Owner = Application.Current.MainWindow
            };

            this.view.Closed += this.OnViewClosed;
            this.view.Deactivated += this.OnDeactivated;
            this.view.SearchTextBox.TextChanged += OnSearchTextChanged;
        }

        private void DestroyView()
        {
            Debug.Assert(this.view != null);
            Debug.Assert(this.model != null);

            this.view.Closed -= OnViewClosed;
            this.view.Deactivated -= OnDeactivated;
            this.view.SearchTextBox.TextChanged -= OnSearchTextChanged;
            this.model = null;
            this.view = null;
        }

        #region IOmniBoxSearchCallback

        public void AddItem(IOmniBoxItem item)
        {
            // TODO: this is going to flood the dispatcher queue and cause a HUGE number of allocs unless we batch this.
            // TODO: ensure we cancel search tasks and check window is still up before adding.
            this.view.Dispatcher.InvokeAsync(() =>
            {
                this.model.SearchResults.Add(item);
            });
        }

        #endregion
    }
}
