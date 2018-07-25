namespace VSOmniBox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using VSOmniBox.API.Data;

    internal sealed class SearchProviderTask : IOmniBoxSearchSession
    {
        private readonly IOmniBoxItemsSource itemsSource;
        private readonly List<OmniBoxItem> items = new List<OmniBoxItem>();
        private readonly object listLock = new object();
        private bool started;

        public SearchProviderTask(IOmniBoxItemsSource itemsSource, CancellationToken cancellationToken)
        {
            this.itemsSource = itemsSource ?? throw new ArgumentNullException(nameof(itemsSource));
            this.CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public async Task<IEnumerable<OmniBoxItem>> Search(string searchString)
        {
            if (this.started)
            {
                throw new InvalidOperationException("Task has already been started");
            }

            started = true;

            await this.itemsSource.GetItemsAsync(searchString, this);

            // Just in case..
            lock (this.listLock)
            {
                return items;
            }
        }

        public void AddItem(OmniBoxItem item)
        {
            lock (this.listLock)
            {
                this.items.Add(item);
            }
        }
    }
}
