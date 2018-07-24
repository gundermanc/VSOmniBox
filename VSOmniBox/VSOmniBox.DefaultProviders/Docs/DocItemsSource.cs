using System.IO;
using System.Net;
using System.Xml;
using VSOmniBox.API.Data;

namespace VSOmniBox.DefaultProviders.Docs
{
    internal class DocItemsSource : IOmniBoxItemsSource
    {
        public async System.Threading.Tasks.Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            WebClient wc = new WebClient();
            var content = await wc.DownloadStringTaskAsync($"https://docs.microsoft.com/api/search/rss?search={searchString}&locale=en-us");
            if (searchSession.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            XmlReader r = XmlReader.Create(new StringReader(content));
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element && r.Name == "item")
                {
                    r.ReadToDescendant("title");
                    var title = r.ReadElementContentAsString("title", "");
                    r.ReadToNextSibling("description");
                    var desc = r.ReadElementContentAsString("description", "");
                    r.ReadToNextSibling("link");
                    var link = r.ReadElementContentAsString("link", "");
                    if (searchSession.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    searchSession.AddItem(new DocOmniBoxItem(title, desc, link));
                }
            }

            return;
        }
    }
}
