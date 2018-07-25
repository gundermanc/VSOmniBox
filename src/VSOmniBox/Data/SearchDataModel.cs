namespace VSOmniBox.Data
{
    using System.Collections.Immutable;
    using VSOmniBox.API.Data;

    internal sealed class SearchDataModel
    {
        public static readonly SearchDataModel Empty = new SearchDataModel(ImmutableArray<OmniBoxItem>.Empty, ImmutableArray<OmniBoxItem>.Empty, ImmutableArray<OmniBoxItem>.Empty);

        public SearchDataModel(ImmutableArray<OmniBoxItem> codeItems, ImmutableArray<OmniBoxItem> ideItems, ImmutableArray<OmniBoxItem> helpItems)
        {
            this.CodeItems = codeItems;
            this.IDEItems = ideItems;
            this.HelpItems = helpItems;
        }

        public ImmutableArray<OmniBoxItem> CodeItems { get; }

        public ImmutableArray<OmniBoxItem> IDEItems { get; }

        public ImmutableArray<OmniBoxItem> HelpItems { get; }
    }
}
