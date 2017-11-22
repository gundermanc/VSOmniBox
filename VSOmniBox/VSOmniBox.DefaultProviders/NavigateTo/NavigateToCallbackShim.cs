namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using VSOmniBox.API;

    internal sealed class NavigateToCallbackShim : INavigateToCallback
    {
        private static readonly NavigateToOptions StaticOptions = new NavigateToOptions();
        private readonly IOmniBoxSearchCallback searchCallback;

        public NavigateToCallbackShim(IOmniBoxSearchCallback searchCallback)
        {
            this.searchCallback = searchCallback ?? throw new ArgumentNullException(nameof(searchCallback));
        }

        public INavigateToOptions Options => StaticOptions;

        public void AddItem(NavigateToItem item) => this.searchCallback.AddItem(NavigateToItemShim.Create(item));

        public void Done()
        {
            // TODO: report progress.
        }

        public void Invalidate()
        {
            // TODO: what is this??
        }

        public void ReportProgress(int current, int maximum)
        {
            // TODO: report progress.
        }
    }
}
