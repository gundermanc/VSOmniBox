namespace VSOmniBox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using VSOmniBox.API.Data;

    internal sealed class SearchTask : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool started;

        public static SearchTask Create() => new SearchTask();

        public bool IsDisposed { get; private set; }

        public async Task<SearchDataModel> SearchAsync(IEnumerable<IOmniBoxItemsSource> itemsSources, string searchString)
        {
            if (this.started)
            {
                throw new InvalidOperationException("Search has already started");
            }

            this.CheckCanceled();

            this.started = true;

            var searchProviderTasks = itemsSources.Select(
                itemsSource => new SearchProviderTask(
                    itemsSource,
                    this.cancellationTokenSource.Token)).ToList();

            // Wait for results from all providers from parallel computations on background thread.
            var providerResultsLists = await Task.WhenAll(
                searchProviderTasks.Select(
                    searchProviderTask => Task.Run(() => searchProviderTask.Search(searchString))));

            this.CheckCanceled();

            // Sort and filter on background thread.
            var sortedResults = await Task.Run(() => FlattenSortAndFilterLists(providerResultsLists));

            return new SearchDataModel(sortedResults);
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

        private void CheckCanceled()
        {
            if (this.IsDisposed || this.cancellationTokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        private static ImmutableArray<OmniBoxItem> FlattenSortAndFilterLists(IEnumerable<IEnumerable<OmniBoxItem>> providerResultsLists)
        {
            var flattenedResults = providerResultsLists
                .SelectMany(select => select);

            var builder = ImmutableArray.CreateBuilder<OmniBoxItem>();
            builder.AddRange(flattenedResults);

            // TODO: sort.

            return builder.ToImmutable();
        }
    }
}
