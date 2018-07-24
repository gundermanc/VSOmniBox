namespace VSOmniBox.Data
{
    using System.Collections.Immutable;
    using VSOmniBox.API.Data;

    internal sealed class SearchDataModel
    {
        public static readonly SearchDataModel Empty = new SearchDataModel(ImmutableArray<OmniBoxItem>.Empty, ImmutableArray<OmniBoxItem>.Empty);

        public SearchDataModel(ImmutableArray<OmniBoxItem> codeItems, ImmutableArray<OmniBoxItem> ideItems)
        {
            this.CodeItems = codeItems;
            this.IDEItems = ideItems;
        }

        public ImmutableArray<OmniBoxItem> CodeItems { get; }

        public ImmutableArray<OmniBoxItem> IDEItems { get; }
    }
}
