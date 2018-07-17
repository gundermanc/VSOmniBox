namespace VSOmniBox.API.Data
{
    using System.Threading.Tasks;

    public interface IOmniBoxItemsSource
    {
        Task GetItemsAsync(
            string searchString,
            IOmniBoxSearchSession searchSession);
    }
}