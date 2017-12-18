namespace VSOmniBox.API
{
    public interface IOmniBoxBroker
    {
        bool IsVisible { get; set; }

        // TODO: expose these? These should go on a separate search service.
        void StartOrUpdateSearch(string searchQuery);
        void StopSearch();
    }
}
