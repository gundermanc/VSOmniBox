namespace VSOmniBox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Text.PatternMatching;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    [Export]
    internal sealed class SearchController
    {
        private readonly IEnumerable<Lazy<IOmniBoxItemsSourceProvider, IOmniBoxItemsSourceProviderMetadata>> itemsSourceProviders;
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly Lazy<IPatternMatcherFactory> patternMatcherFactory;
        private AsyncLazy<ImmutableArray<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)>> itemsSources;
        private SearchTask currentSearch;

        [ImportingConstructor]
        public SearchController(
            [ImportMany]IEnumerable<Lazy<IOmniBoxItemsSourceProvider, IOmniBoxItemsSourceProviderMetadata>> itemsSourceProviders,
            JoinableTaskContext joinableTaskContext,
            Lazy<IPatternMatcherFactory> patternMatcherFactory)
        {
            this.itemsSourceProviders = itemsSourceProviders
                ?? throw new ArgumentNullException(nameof(itemsSourceProviders));
            this.joinableTaskContext = joinableTaskContext
                ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.patternMatcherFactory = patternMatcherFactory
                ?? throw new ArgumentNullException(nameof(patternMatcherFactory));

            this.itemsSources = new AsyncLazy<ImmutableArray<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)>>(
                () => this.CreateSourcesAsync(), joinableTaskContext.Factory);
        }

        public event EventHandler<SearchDataModelUpdatedArgs> DataModelUpdated;

        // Assumed to be called from only UI thread.
        public void StartOrUpdateSearch(string searchString)
        {
            this.StopSearch();

            // Search text is blank. Skip search and update with empty model.
            if (searchString.Length == 0)
            {
                this.DataModelUpdated?.Invoke(
                    this,
                    new SearchDataModelUpdatedArgs(SearchDataModel.Empty));
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

        public void WarmupFireAndForget()
        {
            this.joinableTaskContext.Factory.RunAsync(async delegate
            {
                await this.CreateSourcesAsync();
            });
        }

        private async Task PerformSearch(
            SearchTask searchTask,
            ImmutableArray<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)> sources,
            string searchString)
        {
            // Do the search, if and only if the search hasn't been canceled and we aren't preempted by another task.
            if ((this.currentSearch != null) && (this.currentSearch == searchTask) && !searchTask.IsDisposed)
            {
                try
                {
                    var searchDataModel = await this.currentSearch.SearchAsync(
                        sources,
                        this.patternMatcherFactory.Value,
                        searchString);
                    if (!(this.currentSearch?.IsDisposed ?? true))
                    {
                        this.DataModelUpdated?.Invoke(this, new SearchDataModelUpdatedArgs(searchDataModel));
                    }
                    this.currentSearch = null;
                }
                catch (OperationCanceledException) { }
            }
        }

        private async Task<ImmutableArray<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)>> CreateSourcesAsync()
        {
            var sources = ImmutableArray.CreateBuilder<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)>();

            foreach (var provider in this.itemsSourceProviders)
            {
                foreach (var source in await provider.Value.CreateItemsSourcesAsync())
                {
                    sources.Add((source, provider.Metadata));
                }
            }

            return sources.ToImmutable();
        }
    }
}
