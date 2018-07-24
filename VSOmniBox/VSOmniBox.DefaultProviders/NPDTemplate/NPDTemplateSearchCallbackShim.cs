namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TemplateProviders;
    using VSOmniBox.API.Data;

    internal sealed class NPDTemplateSearchCallbackShim : IVsSearchProviderCallback
    {
        private readonly IOmniBoxSearchSession searchCallback;
        private readonly TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public Task Task => this.taskCompletionSource.Task;

        public NPDTemplateSearchCallbackShim(IOmniBoxSearchSession searchCallback)
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
            if (dwResultsFound == 0) return;
            
                ProjectTemplateSearchTask t = pTask as ProjectTemplateSearchTask;

                foreach(var item in t.results)
                {
                    this.searchCallback.AddItem(item);
                }

            if (!this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsCanceled)
            {
                this.taskCompletionSource.SetResult(true);
            }
        }

        public void ReportResult(IVsSearchTask pTask, IVsSearchItemResult pSearchItemResult)
        {
            throw new NotImplementedException();
        }

        public void ReportResults(IVsSearchTask pTask, uint dwResults, IVsSearchItemResult[] pSearchItemResults)
        {
            throw new NotImplementedException();
        }
    }
}
