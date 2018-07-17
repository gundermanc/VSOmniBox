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
        private Thread owningThread;
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
            return items;
        }

        public void AddItem(OmniBoxItem item)
        {
            if (this.owningThread == null)
            {
                this.owningThread = Thread.CurrentThread;
            }
            else if (this.owningThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Cannot add item from this thread. Session claimed by another thread");
            }

            this.items.Add(item);
        }
    }
}
