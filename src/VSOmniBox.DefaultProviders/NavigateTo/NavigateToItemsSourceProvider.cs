namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API.Data;

    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(NavigateToItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.Code)]
    internal sealed class NavigateToItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private readonly SVsServiceProvider shellServiceProvider;
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories;

        [ImportingConstructor]
        public NavigateToItemsSourceProvider(
            SVsServiceProvider shellServiceProvider,
            JoinableTaskContext joinableTaskContext,
            [ImportMany]IEnumerable<Lazy<INavigateToItemProviderFactory>> itemProviderFactories)
        {
            this.shellServiceProvider = shellServiceProvider;
            this.joinableTaskContext = joinableTaskContext;
            this.itemProviderFactories = itemProviderFactories;
        }

        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
        {
            // TODO: the file name provider currently breaks search because it fails to report search completion.
            // Turning it off until we figure out what's wrong.
            return System.Threading.Tasks.Task.FromResult<IEnumerable<IOmniBoxItemsSource>>(this.itemProviderFactories
                .Where(factory => factory.Value.GetType().FullName != "Microsoft.VisualStudio.Language.NavigateTo.FileNameProvider.NavigateToItemProviderFactory")
                .Select(lazy => NavigateToItemsSource.Create(this.shellServiceProvider, this.joinableTaskContext, lazy.Value)));
        }
    }
}
