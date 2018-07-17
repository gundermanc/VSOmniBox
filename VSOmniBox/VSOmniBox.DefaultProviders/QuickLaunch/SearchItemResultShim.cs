namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
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

        public override void Invoke() => this.searchItemResult.InvokeAction();
    }
}
