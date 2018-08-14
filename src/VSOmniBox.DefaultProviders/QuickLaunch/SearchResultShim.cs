namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSOmniBox.API.Data;

    internal sealed class SearchItemResultShim : OmniBoxItem
    {
        private readonly IVsSearchItemResult searchItemResult;

        public SearchItemResultShim(IVsSearchItemResult searchItemResult)
        {
            this.searchItemResult = searchItemResult
                ?? throw new ArgumentNullException(nameof(searchItemResult));
        }

        public override string Title => this.searchItemResult.DisplayText;

        public override string Description => this.searchItemResult.Description;

        public override ImageMoniker Icon
        {
            get
            {
                switch (searchItemResult.SearchProvider.Category.ToString().ToUpperInvariant())
                {
                    case "F7A34A7C-B596-4BFA-B119-321539FC96ED": // Commands
                        return KnownMonikers.ApplicationBarCommand;
                    case "28A7EDD7-524F-4C82-8E1E-1E472357454D": // Documents.
                        return KnownMonikers.Document;
                    case "3EF528C5-C45A-47E0-B9EE-A212A32A99EC": // MRU.
                        return KnownMonikers.Group;
                    case "CAF11422-9334-422C-91D9-656307E3B552": // Setup
                        return KnownMonikers.VisualStudio;
                    case "258EBB6F-5C3B-4241-B929-71EC7A18DBF0": // Tools options
                        return KnownMonikers.Settings;
                    case "042C2B4B-C7F7-49DB-B7A2-402EB8DC7892": // Nuget packages search.
                        return KnownMonikers.NuGet;
                    default:
                        return KnownMonikers.AbstractCube;
                }
            }
        }

        public override void Invoke() => this.searchItemResult.InvokeAction();
    }
}
