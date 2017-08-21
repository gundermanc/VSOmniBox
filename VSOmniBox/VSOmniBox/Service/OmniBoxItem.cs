namespace VSOmniBox.Service
{
    using System;
    using VSOmniBox.API;

    internal sealed class OmniBoxItem : IOmniBoxItem
    {
        public OmniBoxItem(string title)
        {
            this.Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        public string Title { get; }
    }
}
