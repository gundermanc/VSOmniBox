namespace VSOmniBox.DefaultProviders.LightBulb
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.IDE)]
    [OmniBoxInitialResults]
    internal sealed class LightBulbItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly SVsServiceProvider serviceProvider;
        private readonly IVsEditorAdaptersFactoryService adaptersFactoryService;
        private readonly ILightBulbBroker lightBulbBroker;
        private readonly ISuggestedActionCategoryRegistryService2 categoryRegistryService;

        [ImportingConstructor]
        public LightBulbItemsSourceProvider(
            JoinableTaskContext joinableTaskContext,
            SVsServiceProvider serviceProvider,
            ILightBulbBroker lightBulbBroker,
            IVsEditorAdaptersFactoryService adaptersFactoryService,
            ISuggestedActionCategoryRegistryService2 categoryRegistryService)
        {
            this.joinableTaskContext = joinableTaskContext;
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.lightBulbBroker = lightBulbBroker ?? throw new ArgumentNullException(nameof(lightBulbBroker));
            this.adaptersFactoryService = adaptersFactoryService ?? throw new ArgumentNullException(nameof(adaptersFactoryService));
            this.categoryRegistryService = categoryRegistryService ?? throw new ArgumentNullException(nameof(categoryRegistryService));
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<IOmniBoxItemsSource>>(
                new [] 
                {
                    new LightBulbItemsSource(
                        this.joinableTaskContext,
                        this.serviceProvider,
                        this.adaptersFactoryService, 
                        this.lightBulbBroker, 
                        this.categoryRegistryService)
                });
        }
    }
}
