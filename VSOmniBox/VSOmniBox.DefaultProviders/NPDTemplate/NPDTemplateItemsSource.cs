using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOmniBox.API.Data;
using VSOmniBox.DefaultProviders.QuickLaunch;
using Task = System.Threading.Tasks.Task;

namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    internal class NPDTemplateItemsSource : IOmniBoxItemsSource
    {
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncProvider;
        private ProjectTemplateSearchTask searchTask;

        static uint cookie = 1;

        public NPDTemplateItemsSource(SVsServiceProvider shellServiceProvider)
        {
            Validate.IsNotNull(shellServiceProvider, nameof(shellServiceProvider));
            asyncProvider = shellServiceProvider.GetService(typeof(SAsyncServiceProvider)) as Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
        }

        public Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            if (searchSession.CancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(searchSession.CancellationToken);
            }

            var callbackShim = new NPDTemplateSearchCallbackShim(searchSession);

            this.searchTask = new ProjectTemplateSearchTask(cookie++, new SearchQueryShim(searchString), callbackShim, this.asyncProvider);
            this.searchTask.Start();

            return callbackShim.Task;

        }

        //#region IVsWindowSearch Members

        ///// <summary>
        ///// Gets the GUID of the search provider
        ///// </summary>
        //public Guid Category
        //{
        //    get { return new Guid(CategoryGuidString); }
        //}

        //private const string CategoryGuidString = "552292FA-7315-4B8F-8071-BFF5F1F9E9B0";

        ///// <summary>
        ///// Determines whether the search should be enabled
        ///// </summary>
        //public bool SearchEnabled
        //{
        //    get { return true; }
        //}

        ///// <summary>
        ///// Returns an interface that can be used to enumerate search filters.
        ///// </summary>
        //public IVsEnumWindowSearchFilters SearchFiltersEnum
        //{
        //    get { return null; }
        //}

        ///// <summary>
        ///// Allows the window search host to obtain overridable search options
        ///// </summary>
        //public IVsEnumWindowSearchOptions SearchOptionsEnum
        //{
        //    get { return null; }
        //}

        ///// <summary>
        ///// Clears the search result, for example, after the user has cleared the content of the search edit box
        ///// </summary>
        //public void ClearSearch()
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();

        //    lock (_lockObject)
        //    {
        //        SearchResultsTemplates.Clear();
        //    }

        //    SearchCompleted = false;
        //}

        //public IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        //{
        //    NewProjectSearchTask _searchTask = new NewProjectSearchTask(dwCookie, pSearchQuery, pSearchCallback, this.asyncProvider);
        //    _searchTask.SearchResultAvailable += OnSearchResultAvailable;
        //    _searchTask.SearchComplete += OnSearchComplete;
        //    _searchTask.SearchStarted += OnSearchStarted;
        //    return _searchTask;
        //}

        //private void OnSearchResultAvailable(object sender, NewProjectSearchEventArgs e)
        //{

        //}

        //public bool OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers)
        //{
        //    // Return false to indicate that no keys are handled by this method
        //    return false;
        //}

        //public void ProvideSearchSettings(IVsUIDataSource pSearchSettings)
        //{
        //    //_searchSettings = (SearchSettingsDataSource)pSearchSettings;
        //    //_searchSettings.SearchWatermark = StartPageResources.SearchTemplatesText;
        //    //_searchSettings.ControlMinWidth = 100;
        //    //_searchSettings.ControlMaxWidth = 1000;
        //    //_searchSettings.SearchStartType = VSSEARCHSTARTTYPE.SST_DELAYED;
        //    //_searchSettings.SearchProgressType = VSSEARCHPROGRESSTYPE.SPT_INDETERMINATE;
        //    //_searchSettings.SearchStartMinChars = 2;
        //}

        //#endregion


    }
}
