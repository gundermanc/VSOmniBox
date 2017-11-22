namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API;
    using VSOmniBox.DefaultProviders.QuickLaunch;

    [Export(typeof(IOmniBoxSearchProviderFactory))]
    [Name(nameof(NavigateToSearchProviderFactory))]
    [Order(Before = nameof(QuickLaunchSearchProviderFactory))]
    internal sealed class NavigateToSearchProviderFactory : IOmniBoxSearchProviderFactory
    {
        private readonly IOmniBoxShellServicesFactory shellServicesFactory;
        private readonly IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories;

        private IOmniBoxSearchProvider searchProvider;

        [ImportingConstructor]
        public NavigateToSearchProviderFactory(
            IOmniBoxShellServicesFactory shellServicesFactory,
            [ImportMany]IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories)
        {
            this.shellServicesFactory = shellServicesFactory;
            this.itemProviderFactories = itemProviderFactories;
        }

        public IOmniBoxSearchProvider GetSearchProvider()
        {
            return this.searchProvider
                ?? (this.searchProvider = NavigateToSearchProvider.Create(this.CreateItemProviders()));
        }

        private IEnumerable<INavigateToItemProvider> CreateItemProviders()
        {
            foreach (var factory in this.itemProviderFactories)
            {
                if (factory.Value.TryCreateNavigateToItemProvider(
                    this.shellServicesFactory.ServiceProvider,
                    out var provider))
                {
                    yield return provider;
                }
            }
        }
    }
}
