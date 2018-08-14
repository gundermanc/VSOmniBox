
namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using System;
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
    using Microsoft.VisualStudio.PlatformUI;
    using VSOmniBox.API.Data;

    internal sealed class NavigateToItemShim : OmniBoxItem
    {
        private readonly INavigateToItemDisplay displayItem;

        public static NavigateToItemShim Create(NavigateToItem item)
            => new NavigateToItemShim(item.DisplayFactory.CreateItemDisplay(item));

        private NavigateToItemShim(INavigateToItemDisplay displayItem)
        {
            this.displayItem = displayItem ?? throw new ArgumentNullException(nameof(displayItem));
        }

        public override string Title => this.displayItem.Name;

        public override string Description => this.displayItem.Description;

        public override ImageMoniker Icon => KnownMonikers.Code;

        public override void Invoke() => this.displayItem.NavigateTo();
    }
}
