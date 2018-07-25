namespace VSOmniBox.Data
{
    using System;

    internal sealed class SearchDataModelUpdatedArgs : EventArgs
    {
        public SearchDataModelUpdatedArgs(SearchDataModel model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public SearchDataModel Model { get; }
    }
}
