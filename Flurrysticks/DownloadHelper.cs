using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Flurrystics
{
    class DownloadHelper
    {

        List<string> queue = new List<string>();

        public async Task<XDocument> DownloadXML(string callURL, ProgressBar pb)
        {

            if (queue.Contains(callURL)) {
                Debug.WriteLine("callURL already in queue");
                return null;
            }

            XDocument xdoc = null;

            App.taskCount++;
            queue.Add(callURL);
            if (pb != null) { pb.Visibility = Windows.UI.Xaml.Visibility.Visible; }

            try
            {

                long waitTime = App.taskCount * 1250;

                Debug.WriteLine("Waiting " + waitTime);
                await Task.Delay((int)waitTime);
                Debug.WriteLine("Wait is over, continuing...");

                // the following uri will returns a response with xml content
                Debug.WriteLine("callURL:" + callURL);
                Uri uri = new Uri(callURL);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/xml"); // we want XML
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                // ReadAsStreamAsync() returns when the whole message is downloaded
                Stream stream = await response.Content.ReadAsStreamAsync();
                xdoc = XDocument.Load(stream);

            }
            catch (Exception) // catch generally all exceptions, not only: System.Net.Http.HttpRequestException
            {   // load failed
                Debug.WriteLine("Load data failed");
                xdoc = null;
            }

            App.taskCount--;
            queue.Remove(callURL);

            if (App.taskCount == 0)
            {
                if (pb != null) { pb.Visibility = Windows.UI.Xaml.Visibility.Collapsed;}
            }

            return xdoc; 
        }

    }
}
