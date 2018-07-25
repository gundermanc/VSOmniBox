namespace VSOmniBox.API.Data
{
    using System;
    using System.ComponentModel.Composition;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [MetadataAttribute]
    public sealed class OmniBoxPivotAttribute : Attribute
    {
        public OmniBoxPivotAttribute(OmniBoxPivot pivot)
        {
            this.Pivot = pivot;
        }

        public OmniBoxPivot Pivot { get; }
    }
}
