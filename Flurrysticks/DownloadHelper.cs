﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;

namespace Flurrysticks
{
    class DownloadHelper
    {

        public async Task<XDocument> DownloadXML(string callURL)
        {


            while (App.lastRequestTimeOut > 0)
            {
                Debug.WriteLine("Waiting " + App.lastRequestTimeOut);
                await Task.Delay((int)App.lastRequestTimeOut);
                Debug.WriteLine("Wait is over, continuing...");
                App.lastRequestTimeOut = 0; // reseting timeOut
            }

            // the following uri will returns a response with xml content
            Debug.WriteLine("callURL:" + callURL);
            Uri uri = new Uri(callURL);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/xml"); // we want XML
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            // ReadAsStreamAsync() returns when the whole message is downloaded
            Stream stream = await response.Content.ReadAsStreamAsync();
            XDocument xdoc = XDocument.Load(stream);
            App.lastRequestTimeOut = App.lastRequestTimeOut + 1000;
            return xdoc; 

            /*

            XNamespace xns = "http://schemas.microsoft.com/search/local/ws/rest/v1";
            var addresses = from node in xdoc.Descendants(xns + "Address")                               // query node named "Address"
                            where node.Element(xns + "CountryRegion").Value.Contains("United States")    // where CountryRegion contains "United States"
                            select node.Element(xns + "FormattedAddress").Value;                         // select the FormattedAddress node's value

            StringBuilder stringBuilder = new StringBuilder("Manchester in US: ");
            foreach (string name in addresses)
                stringBuilder.Append(name + "; ");

            return stringBuilder.ToString();
            */
        }

    }
}