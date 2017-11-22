namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using VSOmniBox.API;

    internal class NavigateToSearchProvider : IOmniBoxSearchProvider
    {
        private const string QuickLaunchStoreName = "SearchProviders";
        private const string PackageKeyName = "Package";
        private const string MREProviderCategory = "3ef528c5-c45a-47e0-b9ee-a212a32a99ec";

        private List<INavigateToItemProvider> itemProviders;

        public static NavigateToSearchProvider Create(IEnumerable<INavigateToItemProvider> itemProviders)
        {
            return new NavigateToSearchProvider(itemProviders);
        }

        private NavigateToSearchProvider(IEnumerable<INavigateToItemProvider> itemProviders)
        {
            this.itemProviders = itemProviders.ToList();
        }

        public async Task StartSearchAsync(
            string searchString,
            IOmniBoxSearchCallback searchCallback,
            CancellationToken cancellationToken)
        {
            foreach (var prov in this.itemProviders)
            {
                await Task.Run(() => prov.StartSearch(new NavigateToCallbackShim(searchCallback), searchString));
            }
        }
    }
}