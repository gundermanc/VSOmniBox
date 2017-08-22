namespace VSOmniBox.API
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOmniBoxSearchProvider
    {
        Task StartSearch(
            string searchString,
            IOmniBoxSearchCallback searchCallback,
            CancellationToken cancellationToken);
    }
}