namespace VSOmniBox.API.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IOmniBoxItemsSourceProvider
    {
        Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync();
    }
}
