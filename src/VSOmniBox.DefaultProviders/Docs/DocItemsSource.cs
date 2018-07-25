using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Xml;
using VSOmniBox.API.Data;

namespace VSOmniBox.DefaultProviders.Docs
{
    internal class DocItemsSource : IOmniBoxItemsSource
    {
        public async System.Threading.Tasks.Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            if (searchString == null || searchString.Length < 3)
            {
                return;
            }

            try
            {
                string result;
                using (var client = new HttpClient())
                using (var response = await client.GetAsync($"https://docs.microsoft.com/api/search/rss?search={searchString}&locale=en-us", searchSession.CancellationToken))
                {
                    result = await response.Content.ReadAsStringAsync();
                }

                if (searchSession.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                XmlReader r = XmlReader.Create(new StringReader(result));
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
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Debug.Fail("Failed to search for doc item: " + ex.Message);
            }
        }
    }
}
