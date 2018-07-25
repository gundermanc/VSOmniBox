namespace VSOmniBox.DefaultProviders.QuickLaunch
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class SearchTokenShim : IVsSearchToken
    {
        public SearchTokenShim(string originalTokenText, uint tokenStartPosition)
        {
            if (string.IsNullOrWhiteSpace(originalTokenText))
            {
                throw new System.ArgumentException("Cannot be null or whitespace", nameof(originalTokenText));
            }

            if (TokenStartPosition < 0)
            {
                throw new ArgumentException("Cannot be less than zero", nameof(TokenStartPosition));
            }

            this.OriginalTokenText = originalTokenText;
            this.TokenStartPosition = tokenStartPosition;
        }

        public string OriginalTokenText { get; }

        public uint TokenStartPosition { get; }

        // TODO: perform parse, remove quotes, etc.
        public string ParsedTokenText => this.OriginalTokenText;

        public uint ParseError => VSConstants.S_OK;
    }
}
