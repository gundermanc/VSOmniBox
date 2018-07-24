namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API.Data;
    using VSOmniBox.DefaultProviders.QuickLaunch;

    //[Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(NavigateToItemsSourceProvider))]
    [Order(Before = nameof(QuickLaunchItemsSourceProvider))]
    internal sealed class NavigateToItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly SVsServiceProvider shellServiceProvider;
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories;

        private AsyncLazy<IEnumerable<INavigateToItemProvider>> itemProviders;

        [ImportingConstructor]
        public NavigateToItemsSourceProvider(
            SVsServiceProvider shellServiceProvider,
            JoinableTaskContext joinableTaskContext,
            [ImportMany]IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories)
        {
            this.shellServiceProvider = shellServiceProvider;
            this.joinableTaskContext = joinableTaskContext;
            this.itemProviderFactories = itemProviderFactories;

            this.itemProviders = new AsyncLazy<IEnumerable<INavigateToItemProvider>>(
                () => System.Threading.Tasks.Task.FromResult(this.CreateItemProviders()));
        }

        public async Task<IEnumerable<IOmniBoxItemsSource>> CreateSearchProvidersAsync()
        {
            return ((await this.itemProviders
                .GetValueAsync())
                .Select(provider => NavigateToItemsSource.Create(this.joinableTaskContext, provider)));
        }

        private IEnumerable<INavigateToItemProvider> CreateItemProviders()
        {
            foreach (var factory in this.itemProviderFactories)
            {
                if (factory.Value.TryCreateNavigateToItemProvider(
                    this.shellServiceProvider,
                    out var provider))
                {
                    yield return provider;
                }
            }
        }
    }
}
