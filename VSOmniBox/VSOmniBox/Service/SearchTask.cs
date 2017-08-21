namespace VSOmniBox.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using VSOmniBox.API;

    internal sealed class SearchTask
    {
        private readonly string query;
        private readonly IOmniBoxSearchCallback searchCallback;
        private readonly IEnumerable<IOmniBoxSearchProvider> searchProviders;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static SearchTask StartNew(
            string query,
            IOmniBoxSearchCallback searchCallback,
            IEnumerable<IOmniBoxSearchProvider> searchProviders)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new System.ArgumentException("Must be non-empty and non-null", nameof(query));
            }

            if (searchCallback == null)
            {
                throw new System.ArgumentNullException(nameof(searchCallback));
            }

            if (searchProviders == null)
            {
                throw new System.ArgumentNullException(nameof(searchProviders));
            }

            var searchTask = new SearchTask(query, searchCallback, searchProviders);

            searchTask.Start();
            return searchTask;
        }

        private SearchTask(
            string query,
            IOmniBoxSearchCallback searchCallback,
            IEnumerable<IOmniBoxSearchProvider> searchProviders)
        {
            this.query = query;
            this.searchCallback = searchCallback;
            this.searchProviders = searchProviders;
        }

        public void Cancel() => this.cancellationTokenSource.Cancel();

        private void Start()
        {
            // TODO: is this actually on a non-UI thread?
            // TODO: alert caller when this task completes.
            Parallel.ForEach(this.searchProviders, PerformSearch);
        }

        private void PerformSearch(IOmniBoxSearchProvider searchProvider)
            => searchProvider.StartSearch(this.searchCallback, this.cancellationTokenSource.Token);
    }
}
