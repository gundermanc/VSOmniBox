namespace VSOmniBox.Data
{
    using System.ComponentModel;
    using VSOmniBox.API.Data;

    public interface IOmniBoxItemsSourceProviderMetadata
    {
        [DefaultValue(false)]
        bool InitialResults { get; }

        OmniBoxPivot Pivot { get; }
    }
}
