using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateProviders;
using System;
using System.Collections.Generic;
using VSOmniBox.API.Data;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    internal class ProjectTemplateSearchTask : VsSearchTask
    {
        private readonly IAsyncServiceProvider asyncServiceProvider;
        private IVsTemplateProvider installedTemplateProvider;
        public List<OmniBoxItem> results = new List<OmniBoxItem>();

        public ProjectTemplateSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchProviderCallback searchCallback, IAsyncServiceProvider asyncServiceProvider)
            : base(dwCookie, pSearchQuery, searchCallback)
        {
            Validate.IsNotNull(asyncServiceProvider, nameof(asyncServiceProvider));

            this.asyncServiceProvider = asyncServiceProvider;
        }

        /// <summary>
        /// Method that is called when a search is started
        /// </summary>
        protected override void OnStartSearch()
        {
            base.OnStartSearch();

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    //SearchStarted?.Invoke(this, EventArgs.Empty);

                    if (installedTemplateProvider == null)
                    {
                        IVsTemplateProviderFactory templateProviderFactory = await asyncServiceProvider.GetServiceAsync(typeof(Microsoft.Internal.VisualStudio.Shell.Interop.SVsDialogService)) as IVsTemplateProviderFactory;
                        if (templateProviderFactory == null)
                        {
                            throw new Exception("Unable to get template provider factory");
                        }

                        installedTemplateProvider = templateProviderFactory.GetInstalledTemplateProvider();
                        if (installedTemplateProvider == null)
                        {
                            throw new Exception("Unable to get the installed template provider");
                        }
                    }

                    var searchResultsNode = installedTemplateProvider.Search(SearchQuery.SearchString);
                    this.results.Clear();

                    foreach (IVsSearchResultTemplate template in searchResultsNode.Extensions)
                    {
                        SearchResults++;

                        OmniBoxItem obItem = new NPDTemplateSearchResultShim(template, installedTemplateProvider);

                        this.results.Add(obItem);
                        //SearchResultAvailable?.Invoke(this, new NewProjectSearchEventArgs(obItem));
                    }

                    // Notify listeners that the search is now complete
                    //SearchComplete?.Invoke(this, EventArgs.Empty);

                    // Also notify the search infrastructure that the search is complete
                    SearchCallback.ReportComplete(this, SearchResults);
                }
                catch (Exception)
                {
                }
            });
        }

        
    }
}
