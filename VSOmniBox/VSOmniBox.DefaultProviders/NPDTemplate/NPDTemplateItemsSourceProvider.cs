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
    [OmniBoxPivot(OmniBoxPivot.IDE)]
    [Order(Before = nameof(QuickLaunchItemsSourceProvider))]
    internal sealed class NPDTemplateItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly SVsServiceProvider shellServiceProvider;

        [ImportingConstructor]
        public NPDTemplateItemsSourceProvider(
            SVsServiceProvider shellServiceProvider)
        {
            this.shellServiceProvider = shellServiceProvider;
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
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
