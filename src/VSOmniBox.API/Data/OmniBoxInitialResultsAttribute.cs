namespace VSOmniBox.API.Data
{
    using System;
    using System.ComponentModel.Composition;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [MetadataAttribute]
    public class OmniBoxInitialResultsAttribute : Attribute
    {
        public OmniBoxInitialResultsAttribute(bool initialResults = true)
        {
            this.InitialResults = initialResults;
        }

        public bool InitialResults { get; }
    }
}
