using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using VSOmniBox.API.Data;
using VSOmniBox.DefaultProviders.NavigateTo;

namespace VSOmniBox.DefaultProviders.Docs
{
    [Export(typeof(IOmniBoxItemsSourceProvider))]
    [Name(nameof(DocItemsSourceProvider))]
    [OmniBoxPivot(OmniBoxPivot.Help)]
    [Order(After = nameof(NavigateToItemsSourceProvider))]
    internal class DocItemsSourceProvider : IOmniBoxItemsSourceProvider
    {
        public Task<IEnumerable<IOmniBoxItemsSource>> CreateItemsSourcesAsync()
        {
            return Task.FromResult<IEnumerable<IOmniBoxItemsSource>>(new IOmniBoxItemsSource[] { new DocItemsSource() });
        }
    }
}
