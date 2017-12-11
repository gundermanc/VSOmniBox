namespace VSOmniBox.Service
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using VSOmniBox.API;
    using VSOmniBox.UI;

    [Export(typeof(IOmniBoxBroker))]
    internal sealed class OmniBoxBroker : IOmniBoxBroker, IOmniBoxSearchCallback
    {
        private readonly IEnumerable<Lazy<IOmniBoxSearchProviderFactory>> searchProviderFactories;
        private readonly OmniBoxViewModel model;

        private IEnumerable<IOmniBoxSearchProvider> searchProviders;

        private OmniBoxView view;
        private SearchTask currentSearch;

        [ImportingConstructor]
        public OmniBoxBroker([ImportMany]IEnumerable<Lazy<IOmniBoxSearchProviderFactory>> searchProviderFactories)
        {
            this.searchProviderFactories = searchProviderFactories
                ?? throw new ArgumentNullException(nameof(searchProviderFactories));

            this.model = new OmniBoxViewModel(this);
            this.model.PropertyChanged += OnModelPropertyChanged;
        }

        #region IOmniBoxBroker

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

        #endregion

        #region IOmniBoxSearchCallback

        public void AddItem(IOmniBoxItem item)
        {
            // TODO: this is going to flood the dispatcher queue and cause a HUGE number of allocs unless we batch this.
            // TODO: ensure we cancel search tasks and check window is still up before adding.
            if (this.view != null)
            {
                this.view.Dispatcher.InvokeAsync(() =>
                {
                    this.model.SearchResults.Add(item);
                });
            }
        }

        #endregion

        internal void InvokeBackspaceCommand()
        {
            if (this.model?.BackspaceCommand.CanExecute(null) ?? false)
            {
                this.model.BackspaceCommand.Execute(null);
            }
        }

        internal void InvokeDownCommand()
        {
            if (this.model?.DownCommand.CanExecute(null) ?? false)
            {
                this.model.DownCommand.Execute(null);
            }
        }

        internal void InvokeLeftCommand()
        {
            if (this.model?.LeftCommand.CanExecute(null) ?? false)
            {
                this.model.LeftCommand.Execute(null);
            }
        }

        internal void InvokeRightCommand()
        {
            if (this.model?.RightCommand.CanExecute(null) ?? false)
            {
                this.model.RightCommand.Execute(null);
            }
        }

        internal void InvokeReturnCommand()
        {
            if (this.model?.DownCommand.CanExecute(null) ?? false)
            {
                this.model.InvokeCommand.Execute(null);
            }
        }

        internal void InvokeUpCommand()
        {
            if (this.model?.UpCommand.CanExecute(null) ?? false)
            {
                this.model.UpCommand.Execute(null);
            }
        }

        private IEnumerable<IOmniBoxSearchProvider> SearchProviders
            => this.searchProviders ??
            (searchProviders = this.searchProviderFactories.Select(factory => factory.Value.CreateSearchProvider())).ToList();

        private bool IsSearchInProgress => this.currentSearch != null;

        private void OnDeactivated(object sender, EventArgs e) => this.IsVisible = false;

        private void OnViewClosed(object sender, EventArgs e) => this.DestroyView();

        private void CreateView()
        {
            Debug.Assert(this.view == null);
            Debug.Assert(this.model != null);

            this.view = new OmniBoxView()
            {
                DataContext = this.model,
                Owner = Application.Current.MainWindow
            };

            this.view.Closed += this.OnViewClosed;
            this.view.Deactivated += this.OnDeactivated;
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OmniBoxViewModel.SearchString))
            {
                this.StartOrUpdateSearch(this.view.SearchTextBox.Text);
            }
        }

        private void DestroyView()
        {
            Debug.Assert(this.view != null);

            this.view.Closed -= OnViewClosed;
            this.view.Deactivated -= OnDeactivated;
            this.view = null;
        }
    }
}
