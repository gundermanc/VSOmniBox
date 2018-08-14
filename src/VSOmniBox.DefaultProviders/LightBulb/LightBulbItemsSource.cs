namespace VSOmniBox.DefaultProviders.LightBulb
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    internal class LightBulbItemsSource : IOmniBoxItemsSource
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly SVsServiceProvider serviceProvider;
        private readonly IVsEditorAdaptersFactoryService adaptersFactory;
        private readonly ILightBulbBroker lightBulbBroker;
        private readonly ISuggestedActionCategoryRegistryService2 categoryRegistryService;

        private IVsTextManager2 textManager;

        public LightBulbItemsSource(
            JoinableTaskContext joinableTaskContext, 
            SVsServiceProvider serviceProvider, 
            IVsEditorAdaptersFactoryService adaptersFactory,
            ILightBulbBroker lightBulbBroker,
            ISuggestedActionCategoryRegistryService2 categoryRegistryService)
        {
            this.joinableTaskContext = joinableTaskContext ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.adaptersFactory = adaptersFactory ?? throw new ArgumentNullException(nameof(adaptersFactory));
            this.lightBulbBroker = lightBulbBroker ?? throw new ArgumentNullException(nameof(lightBulbBroker));
            this.categoryRegistryService = categoryRegistryService ?? throw new ArgumentNullException(nameof(categoryRegistryService));
        }

        public async System.Threading.Tasks.Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            await this.joinableTaskContext.Factory.SwitchToMainThreadAsync();

            if (this.textManager == null)
            {
                this.textManager = (IVsTextManager2)this.serviceProvider.GetService(typeof(SVsTextManager));
            }

            if (!ErrorHandler.Succeeded(this.textManager.GetActiveView2(fMustHaveFocus: 1, pBuffer: null, grfIncludeViewFrameType: 0, out var ppView)))
            {
                return;
            }

            var textView = this.adaptersFactory.GetWpfTextView(ppView);
            if (textView != null)
            {
                if (await this.lightBulbBroker.HasSuggestedActionsAsync(this.categoryRegistryService.Any, textView, searchSession.CancellationToken))
                {
                    // TODO: too lazy...
#pragma warning disable CS0618 // Type or member is obsolete
                    var session = this.lightBulbBroker.CreateSession(this.categoryRegistryService.Any, textView);
#pragma warning restore CS0618 // Type or member is obsolete

                    if (session != null &&
                        session.TryGetSuggestedActionSets(out var actionSets) != QuerySuggestedActionCompletionStatus.Canceled)
                    {
                        foreach (var actionSet in actionSets)
                        {
                            foreach (var action in actionSet.Actions)
                            {
                                searchSession.AddItem(new LightBulbItem(action));
                            }
                        }
                    }
                }
            }
        }
    }
}