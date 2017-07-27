namespace VSOmniBox.Service
{
    using System;
    using System.ComponentModel.Composition;
    using VSOmniBox.API;

    [Export(typeof(IOmniBoxService))]
    internal sealed class OmniBoxService : IOmniBoxService
    {
        private IOmniBoxBroker broker;

        public IOmniBoxBroker Broker => broker ?? (broker = new OmniBoxBroker());
    }
}
