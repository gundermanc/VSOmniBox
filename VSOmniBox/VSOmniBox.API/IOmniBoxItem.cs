namespace VSOmniBox.API
{
    public interface IOmniBoxItem
    {
        string Title { get; }

        void Invoke();
    }
}
