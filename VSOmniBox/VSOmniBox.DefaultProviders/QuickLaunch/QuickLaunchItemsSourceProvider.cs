namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell.Settings;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API.Data;
    using VSOmniBox.DefaultProviders.NavigateTo;

    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(QuickLaunchItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.IDE)]
    [Order(After = nameof(NavigateToItemsSourceProvider))]
    internal sealed class QuickLaunchItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        private const string QuickLaunchStoreName = "SearchProviders";
        private const string PackageKeyName = "Package";
        private const string MREProviderCategory = "3ef528c5-c45a-47e0-b9ee-a212a32a99ec";

        private readonly SVsServiceProvider shellServiceProvider;

        [ImportingConstructor]
        public QuickLaunchItemsSourceProvider(SVsServiceProvider shellServiceProvider)
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
            var settingsManager = new ShellSettingsManager(this.shellServiceProvider);
            var settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);
            if (settingsStore.CollectionExists(QuickLaunchStoreName))
            {
                foreach (var searchProviderId in settingsStore.GetSubCollectionNames(QuickLaunchStoreName))
                {
                    var collectionPath = $@"{QuickLaunchStoreName}\{searchProviderId}";

                    if (settingsStore.PropertyExists(collectionPath, PackageKeyName) &&
                        (settingsStore.GetPropertyType(collectionPath, PackageKeyName) == SettingsType.String))
                    {
                        var packageId = settingsStore.GetString(collectionPath, PackageKeyName);

                        if ((packageId != null) &&
                            Guid.TryParse(packageId, out var packageGuid) &&
                            Guid.TryParse(searchProviderId, out var searchProviderGuid))
                        {
                            var searchProvider = VsShellUtilities
                                .TryGetPackageExtensionPoint<IVsSearchProvider, IVsSearchProvider>(packageGuid, searchProviderGuid);

                            if ((searchProvider != null) && (searchProvider.Category.ToString() != MREProviderCategory))
                            {
                                yield return new QuickLaunchItemsSource(searchProvider);
                            }
                        }
                    }
                }
            }
        }
    }
}
