namespace VSOmniBox.API.Data
{
    using Microsoft.VisualStudio.Imaging.Interop;

    public abstract class OmniBoxItem
    {
        public abstract string Title { get; }

        public abstract string Description { get; }

        public virtual ImageMoniker Icon { get; } = default;

        public virtual int Priority { get; } = 0;

        public abstract void Invoke();
    }
}
