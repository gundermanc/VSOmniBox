namespace VSOmniBox.Service
{
    using System;
    using System.ComponentModel.Composition;
    using VSOmniBox.API;

    [Export(typeof(IOmniBoxShellServicesFactory))]
    internal class OmniBoxShellServicesFactory : IOmniBoxShellServicesFactory
    {
        public IServiceProvider ServiceProvider => VSPackage.ServiceProvider;
    }
}
