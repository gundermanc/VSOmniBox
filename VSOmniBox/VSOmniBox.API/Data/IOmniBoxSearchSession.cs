namespace VSOmniBox.API.Data
{
    using System.Threading;

    public interface IOmniBoxSearchSession
    {
        CancellationToken CancellationToken { get; }

        void AddItem(OmniBoxItem item);
    }
}
