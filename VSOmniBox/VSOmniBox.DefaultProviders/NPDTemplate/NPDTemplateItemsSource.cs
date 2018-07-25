namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    using System;
    using System.Diagnostics;
    using Microsoft.Internal.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TemplateProviders;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;
    using Task = System.Threading.Tasks.Task;

    internal sealed class NPDTemplateItemsSource : IOmniBoxItemsSource
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncServiceProvider;

        public NPDTemplateItemsSource(JoinableTaskContext joinableTaskContext, SVsServiceProvider shellServiceProvider)
        {
            Validate.IsNotNull(shellServiceProvider, nameof(shellServiceProvider));
            Validate.IsNotNull(joinableTaskContext, nameof(joinableTaskContext));

            this.joinableTaskContext = joinableTaskContext
                ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            asyncServiceProvider = shellServiceProvider.GetService(typeof(SAsyncServiceProvider)) as Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
        }

        public Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            return this.joinableTaskContext.Factory.RunAsync(async delegate
            {
                try
                {
                    searchSession.CancellationToken.ThrowIfCancellationRequested();

                    // Has STA requirement, explicit marshal to avoid potential deadlocks.
                    await this.joinableTaskContext.Factory.SwitchToMainThreadAsync(searchSession.CancellationToken);

                    searchSession.CancellationToken.ThrowIfCancellationRequested();

                    IVsTemplateProviderFactory templateProviderFactory = await asyncServiceProvider
                        .GetServiceAsync(typeof(Microsoft.Internal.VisualStudio.Shell.Interop.SVsDialogService)) as IVsTemplateProviderFactory;

                    var installedTemplateProvider = templateProviderFactory.GetInstalledTemplateProvider();

                    var searchResultsNode = installedTemplateProvider.Search(searchString);

                    foreach (IVsSearchResultTemplate template in searchResultsNode.Extensions)
                    {
                        OmniBoxItem obItem = new NPDTemplateSearchResultShim(template, installedTemplateProvider);
                        searchSession.AddItem(obItem);

                        searchSession.CancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Debug.Fail("Exception during NPD search " + ex.Message);
                }

            }).Task;
        }
    }
}
