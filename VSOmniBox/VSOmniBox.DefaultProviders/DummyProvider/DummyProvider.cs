namespace VSOmniBox.DefaultProviders.DummyProvider
{
    using System.ComponentModel.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API;

    [Export(typeof(IOmniBoxSearchProviderFactory))]
    [Name(nameof(DummyProviderFactory))]
    [Order]
    internal sealed class DummyProviderFactory : IOmniBoxSearchProviderFactory
    {
        public IOmniBoxSearchProvider GetSearchProvider()
        {
            return new DummyProvider();
        }
    }

    internal sealed class DummyProvider : IOmniBoxSearchProvider
    {
        public async Task StartSearch(IOmniBoxSearchCallback searchCallback, CancellationToken cancellationToken)
        {
            // Fill with dummy items.
            searchCallback.AddItem(new OmniBoxItem("Result 1"));
            searchCallback.AddItem(new OmniBoxItem("Result 2"));
            searchCallback.AddItem(new OmniBoxItem("Result 3"));
        }
    }
}
