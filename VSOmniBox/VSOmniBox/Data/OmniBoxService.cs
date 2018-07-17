namespace VSOmniBox.Service
{
    using System;
    using System.ComponentModel.Composition;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API;

    [Export(typeof(IOmniBoxService))]
    internal sealed class OmniBoxService : IOmniBoxService
    {
        private readonly IEnumerable<Lazy<IOmniBoxSearchProviderFactory, IOrderable>> searchProviderFactories;

        private IOmniBoxBroker broker;

        [ImportingConstructor]
        public OmniBoxService([ImportMany]IEnumerable<Lazy<IOmniBoxSearchProviderFactory, IOrderable>> searchProviderFactories)
        {
            this.searchProviderFactories = searchProviderFactories
                ?? throw new ArgumentNullException(nameof(searchProviderFactories));
        }

        public IOmniBoxBroker Broker => broker
            ?? (broker = new OmniBoxBroker(Orderer.Order(this.searchProviderFactories).Select(item => item.Value)));
    }
}
