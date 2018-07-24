using Microsoft.VisualStudio.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateProviders;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOmniBox.API.Data;
using VSOmniBox.DefaultProviders.QuickLaunch;

namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(NPDTemplateItemsSourceProvider))]
    [Order(Before = nameof(QuickLaunchItemsSourceProvider))]
    internal sealed class NPDTemplateItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly SVsServiceProvider shellServiceProvider;
        private readonly JoinableTaskContext joinableTaskContext;

        [ImportingConstructor]
        public NPDTemplateItemsSourceProvider(
            SVsServiceProvider shellServiceProvider,
            JoinableTaskContext joinableTaskContext)
        {
            this.shellServiceProvider = shellServiceProvider;
            this.joinableTaskContext = joinableTaskContext;
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateSearchProvidersAsync()
        {
            return System.Threading.Tasks.Task.FromResult(this.CreateSources());
        }

        // Must happen on the UI thread.
        private IEnumerable<IOmniBoxItemsSource> CreateSources()
        {
            yield return new NPDTemplateItemsSource(shellServiceProvider);
        }
    }
}
