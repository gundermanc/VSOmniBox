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
    internal class NewProjectSearchTask : VsSearchTask
    {
        private readonly IAsyncServiceProvider _asyncServiceProvider;
        private IVsTemplateProvider installedTemplateProvider;
        public List<OmniBoxItem> results = new List<OmniBoxItem>();

        public NewProjectSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchProviderCallback pSearchCallback, IAsyncServiceProvider asyncServiceProvider)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            Validate.IsNotNull(asyncServiceProvider, nameof(asyncServiceProvider));

            _asyncServiceProvider = asyncServiceProvider;
        }

        /// <summary>
        /// Event that indicates a search was started
        /// </summary>
        public event EventHandler<EventArgs> SearchStarted;

        /// <summary>
        /// Event that indicates that a search result is available
        /// </summary>
        public event EventHandler<NewProjectSearchEventArgs> SearchResultAvailable;

        /// <summary>
        /// Event that indicates that a search was completed
        /// </summary>
        public event EventHandler<EventArgs> SearchComplete;

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

                    SearchStarted?.Invoke(this, EventArgs.Empty);

                    if (installedTemplateProvider == null)
                    {
                        IVsTemplateProviderFactory templateProviderFactory = await _asyncServiceProvider.GetServiceAsync(typeof(Microsoft.Internal.VisualStudio.Shell.Interop.SVsDialogService)) as IVsTemplateProviderFactory;
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
                    SearchComplete?.Invoke(this, EventArgs.Empty);

                    // Also notify the search infrastructure that the search is complete
                    SearchCallback.ReportComplete(this, SearchResults);
                }
                catch (Exception)
                {
                }
            });
        }

        
    }

    internal class NewProjectSearchEventArgs : EventArgs
    {
        public NewProjectSearchEventArgs(OmniBoxItem searchResult)
        {
            SearchResult = searchResult;
        }

        public OmniBoxItem SearchResult { get; }
    }
}
