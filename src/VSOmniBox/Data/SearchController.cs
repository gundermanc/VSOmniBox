namespace VSOmniBox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.Composition;
    using System.Linq;
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
        private AsyncLazy<ImmutableArray<(IOmniBoxItemsSource source, IOmniBoxItemsSourceProviderMetadata metadata)>> itemsSources;

        // Should only be accessed from UI thread.
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
        public void StartOrUpdateSearch(string searchString, OmniBoxPivot pivot, bool initialOnly = false)
        {
            if (!this.joinableTaskContext.IsOnMainThread)
            {
                throw new InvalidOperationException("Must happen on non-UI thread");
            }

            this.StopSearch();

            // Search text is blank. Skip search and update with empty model, unless we're pre-populating.
            if (searchString.Length == 0 && !initialOnly)
            {
                this.DataModelUpdated?.Invoke(
                    this,
                    new SearchDataModelUpdatedArgs(SearchDataModel.Empty));
                return;
            }

            this.currentSearch = SearchTask.Create();

            this.joinableTaskContext.Factory.RunAsync(async delegate
            {
                IEnumerable<(IOmniBoxItemsSource source, IOmniBoxItemsSourceProviderMetadata metadata)> sources = await this.itemsSources.GetValueAsync();
                if (initialOnly)
                {
                    sources = sources.Where(source => source.metadata.InitialResults);
                }
                await this.PerformSearch(this.currentSearch, sources, searchString, pivot, initialOnly);
            });
        }

        // Assumed to be called from only UI thread.
        public void StopSearch()
        {
            if (!this.joinableTaskContext.IsOnMainThread)
            {
                throw new InvalidOperationException("Must happen on non-UI thread");
            }

            var currentSearch = this.currentSearch;
            this.currentSearch = null;

            if (currentSearch != null)
            {
                currentSearch.Dispose();
            }
        }

        public void WarmupFireAndForget()
        {
            this.joinableTaskContext.Factory.RunAsync(async delegate
            {
                await this.itemsSources.GetValueAsync();
            });
        }

        private async Task PerformSearch(
            SearchTask searchTask,
            IEnumerable<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)> sources,
            string searchString,
            OmniBoxPivot pivot,
            bool initialOnly)
        {
            if (!this.joinableTaskContext.IsOnMainThread)
            {
                throw new InvalidOperationException("Must happen on non-UI thread");
            }

            // Do the search, if and only if the search hasn't been canceled and we aren't preempted by another task.
            if (!searchTask.IsDisposed && (this.currentSearch == searchTask))
            {
                try
                {
                    var searchDataModel = await Task.Run(
                        () => searchTask.SearchAsync(
                            sources,
                            this.patternMatcherFactory.Value,
                            searchString,
                            pivot),
                        searchTask.CancellationToken);

                    // Raise event for results changed iif we aren't preempted by another task.
                    if (!searchTask.IsDisposed && (this.currentSearch == searchTask))
                    {
                        this.DataModelUpdated?.Invoke(this, new SearchDataModelUpdatedArgs(searchDataModel));

                        // Search completed, dispose of the search resources.
                        this.currentSearch.Dispose();
                        this.currentSearch = null;
                    }
                }
                catch (OperationCanceledException) { }
            }
        }

        private async Task<ImmutableArray<(IOmniBoxItemsSource, IOmniBoxItemsSourceProviderMetadata)>> CreateSourcesAsync()
        {
            if (!this.joinableTaskContext.IsOnMainThread)
            {
                throw new InvalidOperationException("Must happen on non-UI thread");
            }

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
