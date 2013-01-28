private async Task<string> ProcessWithXLinq()
        {
            // you need to acquire a Bing Maps key. See http://www.bingmapsportal.com/
            string bingMapKey = "AmnBlc9IkIwPLm-ZIukvYhgFkdw7m4dcuoZYISm5BHh34scA8z2ucVqzLl83vc1g";    
            // the following uri will returns a response with xml content
            Uri uri = new Uri(String.Format("http://dev.virtualearth.net/REST/v1/Locations?q=manchester&o=xml&key={0}", bingMapKey));

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            // ReadAsStreamAsync() returns when the whole message is downloaded
            Stream stream = await response.Content.ReadAsStreamAsync(); 

            XDocument xdoc = XDocument.Load(stream);
            XNamespace xns = "http://schemas.microsoft.com/search/local/ws/rest/v1";
            var addresses = from node in xdoc.Descendants(xns + "Address")                               // query node named "Address"
                            where node.Element(xns + "CountryRegion").Value.Contains("United States")    // where CountryRegion contains "United States"
                            select node.Element(xns + "FormattedAddress").Value;                         // select the FormattedAddress node's value

            StringBuilder stringBuilder = new StringBuilder("Manchester in US: ");
            foreach (string name in addresses)
                stringBuilder.Append(name + "; ");

            return stringBuilder.ToString();
        }

