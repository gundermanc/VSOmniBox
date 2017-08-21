namespace VSOmniBox.API
{
    public interface IOmniBoxBroker
    {
        bool IsVisible { get; set; }

        void StartOrUpdateSearch(string searchQuery);

        void StopSearch();
    }
}
