namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell.Settings;
    using VSOmniBox.API;

    internal class QuickLaunchSearchProvider : IOmniBoxSearchProvider
    {
        private const string QuickLaunchStoreName = "SearchProviders";
        private const string PackageKeyName = "Package";
        private const string MREProviderCategory = "3ef528c5-c45a-47e0-b9ee-a212a32a99ec";
        private readonly IEnumerable<IVsSearchProvider> searchProviders;

        public static QuickLaunchSearchProvider Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var settingsManager = new ShellSettingsManager(serviceProvider);
            var settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);
            if (settingsStore.CollectionExists(QuickLaunchStoreName))
            {
                var searchProviders = new List<IVsSearchProvider>();

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
                                searchProviders.Add(searchProvider);
                            }
                        }
                    }
                }

                return new QuickLaunchSearchProvider(searchProviders);
            }

            return null;
        }

        private QuickLaunchSearchProvider(IEnumerable<IVsSearchProvider> searchProviders)
        {
            this.searchProviders = searchProviders;
        }

        // TODO: is this wrong?
        private static uint cookie;

        public async System.Threading.Tasks.Task StartSearchAsync(
            string searchString,
            IOmniBoxSearchCallback searchCallback,
            CancellationToken cancellationToken)
        {
            var tasks = new List<IVsSearchTask>();

            foreach (var searchProvider in this.searchProviders)
            {
                var searchTask = searchProvider.CreateSearch(++cookie, new SearchQueryShim(searchString), new SearchCallbackShim(searchCallback));

                tasks.Add(searchTask);
            }

            cancellationToken.Register(() => tasks.ForEach(task => task.Stop()));

            await System.Threading.Tasks.Task.WhenAll(tasks.Select(task => System.Threading.Tasks.Task.Run(() => task.Start())));
        }
    }
}