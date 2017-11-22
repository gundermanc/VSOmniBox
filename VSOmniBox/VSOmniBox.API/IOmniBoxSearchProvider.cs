namespace VSOmniBox.API
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOmniBoxSearchProvider
    {
        Task StartSearchAsync(
            string searchString,
            IOmniBoxSearchCallback searchCallback,
            CancellationToken cancellationToken);
    }
}