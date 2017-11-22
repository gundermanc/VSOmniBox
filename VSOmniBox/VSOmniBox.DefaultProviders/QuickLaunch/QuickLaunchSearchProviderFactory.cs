namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API;
    using VSOmniBox.DefaultProviders.NavigateTo;

    [Export(typeof(IOmniBoxSearchProviderFactory))]
    [Name(nameof(QuickLaunchSearchProviderFactory))]
    [Order(After = nameof(NavigateToSearchProviderFactory))]
    internal sealed class QuickLaunchSearchProviderFactory : IOmniBoxSearchProviderFactory
    {
        private readonly IOmniBoxShellServicesFactory shellServicesFactory;

        [ImportingConstructor]
        public QuickLaunchSearchProviderFactory(IOmniBoxShellServicesFactory shellServicesFactory)
        {
            this.shellServicesFactory = shellServicesFactory;
        }

        public IOmniBoxSearchProvider GetSearchProvider()
        {
            return QuickLaunchSearchProvider.Create(this.shellServicesFactory.ServiceProvider);
        }
    }
}
