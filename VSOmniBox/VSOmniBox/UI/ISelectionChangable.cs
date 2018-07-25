namespace VSOmniBox.UI
{
    using VSOmniBox.API.Data;

    internal interface IPivotable
    {
        OmniBoxPivot Pivot { get; set; }
    }
}