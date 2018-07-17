namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSOmniBox.API.Data;

    internal sealed class SearchCallbackShim : IVsSearchProviderCallback
    {
        private readonly IOmniBoxSearchSession searchCallback;
        private readonly TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public Task Task => this.taskCompletionSource.Task;

        public SearchCallbackShim(IOmniBoxSearchSession searchCallback)
        {
            this.searchCallback = searchCallback ?? throw new ArgumentNullException(nameof(searchCallback));
        }

        public void Cancel()
        {
            if (!this.taskCompletionSource.Task.IsCompleted || !this.taskCompletionSource.Task.IsCanceled)
            {
                this.taskCompletionSource.SetCanceled();
            }
        }

        public void ReportProgress(IVsSearchTask pTask, uint dwProgress, uint dwMaxProgress)
        {
            // TODO: report progress.
        }

        public void ReportComplete(IVsSearchTask pTask, uint dwResultsFound)
        {
            if (!this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsCanceled)
            {
                this.taskCompletionSource.SetResult(true);
            }
        }

        public void ReportResult(IVsSearchTask pTask, IVsSearchItemResult pSearchItemResult)
            => this.searchCallback.AddItem(new SearchItemResultShim(pSearchItemResult));

        public void ReportResults(IVsSearchTask pTask, uint dwResults, IVsSearchItemResult[] pSearchItemResults)
        {
            if (dwResults != pSearchItemResults.Length)
            {
                throw new ArgumentException("Count must match array length or be lesser");
            }

            for (uint i = 0; i < dwResults; i++)
            {
                this.ReportResult(pTask, pSearchItemResults[i]);
            }
        }
    }
}
