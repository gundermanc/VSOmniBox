namespace VSOmniBox.DefaultProviders.ContextualCommands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.IDE)]
    [OmniBoxInitialResults]
    internal sealed class ContextualCommandsItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly SVsServiceProvider serviceProvider;
        private readonly IVsEditorAdaptersFactoryService adaptersFactoryService;
        private readonly ITextStructureNavigatorSelectorService navigatorService;
        private DTE dte;

        [ImportingConstructor]
        public ContextualCommandsItemsSourceProvider(
            JoinableTaskContext joinableTaskContext,
            SVsServiceProvider serviceProvider,
            IVsEditorAdaptersFactoryService adaptersFactoryService,
            ITextStructureNavigatorSelectorService navigatorService)
        {
            this.joinableTaskContext = joinableTaskContext ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.adaptersFactoryService = adaptersFactoryService ?? throw new ArgumentNullException(nameof(adaptersFactoryService));
            this.navigatorService = navigatorService ?? throw new ArgumentNullException(nameof(navigatorService));
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
        {
            if (this.dte == null)
            {
                this.dte = (DTE)this.serviceProvider.GetService(typeof(DTE));
            }

            return System.Threading.Tasks.Task.FromResult<IEnumerable<IOmniBoxItemsSource>>(
                new[]
                {
                    new ContextualCommandsItemsSource(
                        this.joinableTaskContext,
                        this.serviceProvider,
                        this.adaptersFactoryService,
                        this.navigatorService)
                });
        }
    }
}
