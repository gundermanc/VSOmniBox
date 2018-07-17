namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using VSOmniBox.API.Data;

    internal sealed class NavigateToCallbackShim : INavigateToCallback
    {
        private static readonly NavigateToOptions StaticOptions = new NavigateToOptions();
        private readonly IOmniBoxSearchSession searchCallback;
        private readonly TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public NavigateToCallbackShim(IOmniBoxSearchSession searchCallback)
        {
            this.searchCallback = searchCallback ?? throw new ArgumentNullException(nameof(searchCallback));
        }

        public INavigateToOptions Options => StaticOptions;

        // TODO: for some reason, at least one NavigateToItemProvider fails to indicate that it
        // is complete, resulting in search getting 'stuck'. For now I'm working around this by
        // timing out after two seconds.
        public Task Task => Task.WhenAny(this.taskCompletionSource.Task, Task.Delay(2000));

        public void AddItem(NavigateToItem item) => this.searchCallback.AddItem(NavigateToItemShim.Create(item));

        public void Cancel()
        {
            if (!this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsCanceled)
            {
                this.taskCompletionSource.SetCanceled();
            }
        }

        public void Done()
        {
            if (!this.taskCompletionSource.Task.IsCompleted && !this.taskCompletionSource.Task.IsCanceled)
            {
                this.taskCompletionSource.SetResult(true);
            }
        }

        // TODO: what is this supposed to do?
        public void Invalidate() => this.Done();

        public void ReportProgress(int current, int maximum)
        {
            if (current == maximum)
            {
                this.Done();
            }
        }
    }
}
