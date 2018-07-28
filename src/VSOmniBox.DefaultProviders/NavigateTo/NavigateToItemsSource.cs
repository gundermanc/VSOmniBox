namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    internal class NavigateToItemsSource : IOmniBoxItemsSource
    {
        private readonly SVsServiceProvider shellServiceProvider;
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly INavigateToItemProviderFactory factory;

        internal static NavigateToItemsSource Create(
            SVsServiceProvider shellServiceProvider,
            JoinableTaskContext joinableTaskContext,
            INavigateToItemProviderFactory factory)
            => new NavigateToItemsSource(shellServiceProvider, joinableTaskContext, factory);


        private NavigateToItemsSource(
            SVsServiceProvider shellServiceProvider,
            JoinableTaskContext joinableTaskContext,
            INavigateToItemProviderFactory factory)
        {
            this.shellServiceProvider = shellServiceProvider ?? throw new ArgumentNullException(nameof(shellServiceProvider));
            this.joinableTaskContext = joinableTaskContext ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async System.Threading.Tasks.Task GetItemsAsync(
            string searchString,
            IOmniBoxSearchSession searchCallback)
        {
            searchCallback.CancellationToken.ThrowIfCancellationRequested();

            var callbackShim = new NavigateToCallbackShim(searchCallback);

            // ItemsSources for NavigateTo are transient and have to be refreshed each search.
            if (this.factory.TryCreateNavigateToItemProvider(this.shellServiceProvider, out var itemProvider))
            {
                searchCallback.CancellationToken.Register(() => itemProvider.StopSearch());

                itemProvider.StartSearch(callbackShim, searchString);
                await callbackShim.Task;
            }
        }
    }
}