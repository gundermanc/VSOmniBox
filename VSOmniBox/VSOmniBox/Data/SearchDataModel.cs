namespace VSOmniBox.Data
{
    using System.Collections.Immutable;
    using VSOmniBox.API.Data;

    internal sealed class SearchDataModel
    {
        public SearchDataModel(ImmutableArray<OmniBoxItem> items)
        {
            this.Items = items;
        }

        public ImmutableArray<OmniBoxItem> Items { get; } = ImmutableArray<OmniBoxItem>.Empty;
    }
}
