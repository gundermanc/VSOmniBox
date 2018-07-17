namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    internal class NavigateToItemsSource : IOmniBoxItemsSource
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly INavigateToItemProvider itemProvider;

        public static NavigateToItemsSource Create(JoinableTaskContext joinableTaskContext, INavigateToItemProvider itemProvider)
            => new NavigateToItemsSource(joinableTaskContext, itemProvider);

        private NavigateToItemsSource(JoinableTaskContext joinableTaskContext, INavigateToItemProvider itemProvider)
        {
            this.joinableTaskContext = joinableTaskContext ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.itemProvider = itemProvider ?? throw new ArgumentNullException(nameof(itemProvider));
        }

        public async Task GetItemsAsync(
            string searchString,
            IOmniBoxSearchSession searchCallback)
        {
            await this.joinableTaskContext.Factory.SwitchToMainThreadAsync();

            searchCallback.CancellationToken.ThrowIfCancellationRequested();

            var callbackShim = new NavigateToCallbackShim(searchCallback);

            searchCallback.CancellationToken.Register(() => itemProvider.StopSearch());

            this.itemProvider.StartSearch(callbackShim, searchString);

            await callbackShim.Task;
        }
    }
}