namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public class SearchQueryShim : IVsSearchQuery
    {
        private static readonly char[] TokenSeparators = { ' ', ',', '.' };

        // Lazily initialized.
        private IVsSearchToken[] tokens;

        public SearchQueryShim(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                throw new ArgumentException("Argument cannot be null or whitespace", nameof(searchString));
            }

            this.SearchString = searchString;
        }

        public uint GetTokens(uint dwMaxTokens, IVsSearchToken[] rgpSearchTokens)
        {
            if (dwMaxTokens < 0)
            {
                throw new ArgumentException("Cannot be less than zero", nameof(dwMaxTokens));
            }
            else if (dwMaxTokens == 0)
            {
                return (uint)this.Tokens.Length;
            }

            if (rgpSearchTokens.Length < dwMaxTokens)
            {
                throw new ArgumentOutOfRangeException("Not enough buffer for requested count", nameof(rgpSearchTokens));
            }

            var tokens = this.Tokens;

            Array.Copy(tokens, rgpSearchTokens, dwMaxTokens);

            return Math.Min(dwMaxTokens, (uint)tokens.Length);
        }

        public string SearchString { get;  }

        // TODO: is this right?
        public uint ParseError => VSConstants.S_OK;

        public IVsSearchToken[] Tokens => this.tokens ?? (this.tokens = this.ParseTokens());

        private IVsSearchToken[] ParseTokens()
        {
            // TODO: less "poor man's", more "deliverable product"...
            // TODO: correct token start position.
            return this.SearchString.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => new SearchTokenShim(token, 0))
                .ToArray();
        }
    }
}
