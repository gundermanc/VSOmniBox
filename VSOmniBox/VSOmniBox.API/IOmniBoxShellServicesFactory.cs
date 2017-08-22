namespace VSOmniBox.API
{
    using System;

    public interface IOmniBoxShellServicesFactory
    {
        IServiceProvider ServiceProvider { get; }
    }
}
