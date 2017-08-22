namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSOmniBox.API;

    internal sealed class SearchCallbackShim : IVsSearchProviderCallback
    {
        private readonly IOmniBoxSearchCallback searchCallback;

        public SearchCallbackShim(IOmniBoxSearchCallback searchCallback)
        {
            this.searchCallback = searchCallback ?? throw new ArgumentNullException(nameof(searchCallback));
        }

        public void ReportProgress(IVsSearchTask pTask, uint dwProgress, uint dwMaxProgress)
        {
            // TODO: report progress.
        }

        public void ReportComplete(IVsSearchTask pTask, uint dwResultsFound)
        {
            // TODO: report complete.
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
