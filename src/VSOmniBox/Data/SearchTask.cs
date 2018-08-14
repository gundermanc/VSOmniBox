namespace VSOmniBox.Data
{
    using Microsoft.VisualStudio.Text.PatternMatching;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using VSOmniBox.API.Data;

    internal sealed class SearchTask : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool started;

        public static SearchTask Create() => new SearchTask();

        public CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        public bool IsDisposed { get; private set; }

        public async Task<SearchDataModel> SearchAsync(
            IEnumerable<(IOmniBoxItemsSource source, IOmniBoxItemsSourceProviderMetadata metadata)> itemsSources,
            IPatternMatcherFactory patternMatcherFactory,
            string searchString,
            OmniBoxPivot pivot)
        {
            if (this.started)
            {
                throw new InvalidOperationException("Search has already started");
            }

            this.CheckCanceled();

            this.started = true;

            var searchTasks = new List<Task<ImmutableArray<OmniBoxItem>>>();

            // Start searches in parallel, if requested.
            this.StartPivotSearch(itemsSources, patternMatcherFactory, searchString, pivot, OmniBoxPivot.Code, searchTasks);
            this.StartPivotSearch(itemsSources, patternMatcherFactory, searchString, pivot, OmniBoxPivot.IDE, searchTasks);
            this.StartPivotSearch(itemsSources, patternMatcherFactory, searchString, pivot, OmniBoxPivot.Help, searchTasks);

            // Wait for all searches to complete in parallel.
            await Task.WhenAll(searchTasks);

            return new SearchDataModel(
                codeItems: searchTasks[0].Result,
                ideItems: searchTasks[1].Result,
                helpItems: searchTasks[2].Result);
        }

        private void StartPivotSearch(
            IEnumerable<(IOmniBoxItemsSource source, IOmniBoxItemsSourceProviderMetadata metadata)> itemsSources,
            IPatternMatcherFactory patternMatcherFactory,
            string searchString,
            OmniBoxPivot searchPivot,
            OmniBoxPivot pivot,
            IList<Task<ImmutableArray<OmniBoxItem>>> searchTasks)
        {
            if (searchPivot.HasFlag(pivot))
            {
                searchTasks.Add(
                    Task.Run(() => SearchAndSortAndFilterResultsAsync(
                        SelectPivotProviders(itemsSources, pivot),
                        patternMatcherFactory,
                        searchString),
                        this.CancellationToken));
            }
            else
            {
                searchTasks.Add(Task.FromResult(ImmutableArray<OmniBoxItem>.Empty));
            }
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
            }
        }

        private IEnumerable<IOmniBoxItemsSource> SelectPivotProviders(
            IEnumerable<(IOmniBoxItemsSource source, IOmniBoxItemsSourceProviderMetadata metadata)> itemsSources,
            OmniBoxPivot pivot)
                => itemsSources
                .Where(tuple => tuple.metadata.Pivot == pivot)
                .Select(tuple => tuple.source);

        private void CheckCanceled()
        {
            if (this.IsDisposed || this.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        private async Task<ImmutableArray<OmniBoxItem>> SearchAndSortAndFilterResultsAsync(
            IEnumerable<IOmniBoxItemsSource> itemsSources,
            IPatternMatcherFactory patternMatcherFactory,
            string searchString)
        {
            var searchProviderTasks = itemsSources.Select(
                itemsSource => new SearchProviderTask(
                    itemsSource,
                    this.CancellationToken)).ToList();

            this.CheckCanceled();

            // Wait for results from all providers from parallel computations on background thread.
            var providerResultsLists = await Task.WhenAll(
                searchProviderTasks.Select(
                    searchProviderTask => Task.Run(() => searchProviderTask.Search(searchString), this.CancellationToken)));

            this.CheckCanceled();

            // Sort and filter on background thread.
            return await Task.Run(
                () => FlattenSortAndFilterLists(patternMatcherFactory, providerResultsLists, searchString),
                this.CancellationToken);
        }

        private static ImmutableArray<OmniBoxItem> FlattenSortAndFilterLists(
            IPatternMatcherFactory patternMatcherFactory,
            IEnumerable<IEnumerable<OmniBoxItem>> providerResultsLists,
            string searchString)
        {
            IEnumerable<OmniBoxItem> filteredAndSortedResults;
            if (searchString.Length > 0)
            {
                // Create pattern matcher for ranking and sorting items.
                var patternMatcherOptions = new PatternMatcherCreationOptions(
                    CultureInfo.CurrentCulture,
                    PatternMatcherCreationFlags.AllowFuzzyMatching | PatternMatcherCreationFlags.AllowSimpleSubstringMatching);
                var patternMatcher = patternMatcherFactory.CreatePatternMatcher(
                    searchString,
                    patternMatcherOptions);

                filteredAndSortedResults = providerResultsLists
                    .SelectMany(select => select)
                    .Select(result => (item: result, titleMatch: patternMatcher.TryMatch(result.Title), descriptionMatch: patternMatcher.TryMatch(result.Description)))
                    .Where(patternMatch => patternMatch.titleMatch != null || patternMatch.descriptionMatch != null)
                    .OrderByDescending(result => result.item.Priority)
                    .ThenBy(result => result.titleMatch)
                    .ThenBy(result => result.descriptionMatch)
                    .Select(result => result.item);
            }
            else
            {
                // Pre-populating results, bypass pattern matcher.
                filteredAndSortedResults = providerResultsLists
                    .SelectMany(select => select)
                    .OrderByDescending(result => result.Priority);
            }

            var builder = ImmutableArray.CreateBuilder<OmniBoxItem>();
            builder.AddRange(filteredAndSortedResults);

            return builder.ToImmutable();
        }
    }
}
