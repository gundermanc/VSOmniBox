namespace VSOmniBox.API.Data
{
    public abstract class OmniBoxItem
    {
        public abstract string Title { get; }

        public abstract void Invoke();
    }
}
