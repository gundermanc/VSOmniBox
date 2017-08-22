namespace VSOmniBox.API
{
    using System;

    public abstract class OmniBoxItem : IOmniBoxItem
    {
        public OmniBoxItem(string title = null)
        {
            this.Title = title;
        }

        public virtual string Title { get; }

        public abstract void Invoke();
    }
}
