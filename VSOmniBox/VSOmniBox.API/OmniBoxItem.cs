namespace VSOmniBox.API
{
    using System;

    public sealed class OmniBoxItem : IOmniBoxItem
    {
        public OmniBoxItem(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Cannot be null or whitespace", nameof(title));
            }

            this.Title = title;
        }

        public string Title { get; }
    }
}
