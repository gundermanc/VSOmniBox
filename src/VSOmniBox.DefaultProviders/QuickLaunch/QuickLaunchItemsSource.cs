namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSOmniBox.API.Data;

    internal class QuickLaunchItemsSource : IOmniBoxItemsSource
    {
        private readonly IVsSearchProvider searchProvider;
        private IVsSearchTask searchTask;

        public QuickLaunchItemsSource(IVsSearchProvider searchProvider)
        {
            this.searchProvider = searchProvider
                ?? throw new ArgumentNullException(nameof(searchProvider));
        }

        // TODO: is this wrong?
        private static uint cookie;

        public System.Threading.Tasks.Task GetItemsAsync(
            string searchString,
            IOmniBoxSearchSession session)
        {
            if (session.CancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(session.CancellationToken);
            }

            var callbackShim = new SearchCallbackShim(session);

            session.CancellationToken.Register(() => this.StopSearch(callbackShim));

            return this.StartSearch(searchString, callbackShim);
        }

        private Task StartSearch(string searchString, SearchCallbackShim callbackShim)
        {
            this.StopSearch(callbackShim: null);

            this.searchTask = searchProvider.CreateSearch(
                ++cookie,
                new SearchQueryShim(searchString),
                callbackShim);
            this.searchTask.Start();

            return callbackShim.Task;
        }

        private void StopSearch(SearchCallbackShim callbackShim)
        {
            if (this.searchTask != null)
            {
                var localSearchTask = this.searchTask;
                this.searchTask = null;
                localSearchTask.Stop();
                //callbackShim?.Cancel();
            }
        }
    }
}