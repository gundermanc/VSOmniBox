namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API.Data;
    using VSOmniBox.DefaultProviders.QuickLaunch;

    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(NPDTemplateItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.IDE)]
    [Order(Before = nameof(QuickLaunchItemsSourceProvider))]
    internal sealed class NPDTemplateItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly SVsServiceProvider shellServiceProvider;

        [ImportingConstructor]
        public NPDTemplateItemsSourceProvider(
            JoinableTaskContext joinableTaskContext,
            SVsServiceProvider shellServiceProvider)
        {
            this.joinableTaskContext = joinableTaskContext
                ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.shellServiceProvider = shellServiceProvider
                ?? throw new ArgumentNullException(nameof(shellServiceProvider));
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
        {
            return System.Threading.Tasks.Task.FromResult(this.CreateSources());
        }

        // Must happen on the UI thread.
        private IEnumerable<IOmniBoxItemsSource> CreateSources()
        {
            yield return new NPDTemplateItemsSource(
                this.joinableTaskContext,
                this.shellServiceProvider);
        }
    }
}
