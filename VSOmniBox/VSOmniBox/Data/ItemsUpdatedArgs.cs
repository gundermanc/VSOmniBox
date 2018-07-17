namespace VSOmniBox.Data
{
    using System;

    internal sealed class ItemsUpdatedArgs : EventArgs
    {
        public ItemsUpdatedArgs(SearchDataModel model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public SearchDataModel Model { get; }
    }
}
