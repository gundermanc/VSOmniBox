namespace VSOmniBox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    [Export]
    internal sealed class SearchController
    {
        private readonly IEnumerable<Lazy<IOmniBoxItemsSourceProvider>> itemsSourceProviders;
        private readonly JoinableTaskContext joinableTaskContext;
        private AsyncLazy<IReadOnlyList<IOmniBoxItemsSource>> itemsSources;
        private SearchTask currentSearch;

        [ImportingConstructor]
        public SearchController(
            [ImportMany]IEnumerable<Lazy<IOmniBoxItemsSourceProvider>> itemsSourceProviders,
            JoinableTaskContext joinableTaskContext)
        {
            this.itemsSourceProviders = itemsSourceProviders
                ?? throw new ArgumentNullException(nameof(itemsSourceProviders));
            this.joinableTaskContext = joinableTaskContext
                ?? throw new ArgumentNullException(nameof(joinableTaskContext));

            this.itemsSources = new AsyncLazy<IReadOnlyList<IOmniBoxItemsSource>>(
                () => this.CreateSourcesAsync(), joinableTaskContext.Factory);
        }

        public event EventHandler<ItemsUpdatedArgs> DataModelUpdated;

        // Assumed to be called from only UI thread.
        public void StartOrUpdateSearch(string searchString)
        {
            this.StopSearch();

            // Search text is blank. Skip search and update with empty model.
            if (searchString.Length == 0)
            {
                this.DataModelUpdated?.Invoke(
                    this,
                    new ItemsUpdatedArgs(new SearchDataModel(ImmutableArray<OmniBoxItem>.Empty)));
                return;
            }

            this.currentSearch = SearchTask.Create();

            this.joinableTaskContext.Factory.RunAsync(async delegate
            {
                var sources = await this.itemsSources.GetValueAsync();
                await this.PerformSearch(this.currentSearch, sources, searchString);
            });
        }

        // Assumed to be called from only UI thread.
        public void StopSearch()
        {
            if (this.currentSearch != null)
            {
                this.currentSearch.Dispose();
                this.currentSearch = null;
            }
        }

        private async Task PerformSearch(SearchTask searchTask, IReadOnlyList<IOmniBoxItemsSource> sources, string searchString)
        {
            // Do the search, if and only if the search hasn't been canceled and we aren't preempted by another task.
            if ((this.currentSearch != null) && (this.currentSearch == searchTask) && !searchTask.IsDisposed)
            {
                try
                {
                    var searchDataModel = await this.currentSearch.SearchAsync(sources, searchString);
                    if (!(this.currentSearch?.IsDisposed ?? true))
                    {
                        this.DataModelUpdated?.Invoke(this, new ItemsUpdatedArgs(searchDataModel));
                    }
                    this.currentSearch = null;
                }
                catch (OperationCanceledException) { }
            }
        }

        private async Task<IReadOnlyList<IOmniBoxItemsSource>> CreateSourcesAsync()
        {
            var sources = new List<IOmniBoxItemsSource>();

            // Create sources.
            foreach (var provider in this.itemsSourceProviders)
            {
                sources.AddRange(await provider.Value.CreateSearchProvidersAsync());
            }

            return sources;
        }
    }
}
