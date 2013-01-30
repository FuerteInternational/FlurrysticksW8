using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace FlurrysticsBackgroundTask
{
    public sealed class BackgroundTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("BackgroundTask start!");
            var defferal = taskInstance.GetDeferral();
            string eDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            string sDate = eDate;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            IReadOnlyCollection<SecondaryTile> tiles = await SecondaryTile.FindAllForPackageAsync(); // get loop for all secondary tiles

            if (tiles != null) // if we got some tiles - let's update them!
            {

                Debug.WriteLine("tiles not null");
                int count = 1;
                int targetCount = 1;
                try
                {
                    targetCount = (int)localSettings.Values["lastUpdate"];
                }
                catch (System.NullReferenceException)
                {
                    Debug.WriteLine("failed to get targetCount");
                }
                targetCount++;
                if ((targetCount > tiles.Count()) || (targetCount == 0)) { targetCount = 1; } // reset to start
                localSettings.Values["lastUpdate"] = targetCount;

                Debug.WriteLine("targetCount=" + targetCount);

                foreach (SecondaryTile tile in tiles)
                {

                    Debug.WriteLine("Live tile update count:" + count);

                    if (count == targetCount)
                    { // only update one tile in loop

                        if (SecondaryTile.Exists(tile.TileId))
                        {
                            string[] p = tile.Arguments.Split('|'); // appName + "|" + appPlatform + "|" + apiKey + "|" + appApiKey;
                            string AppApiKey = p[3];
                            string ApiKey = p[2];
                            string Name = p[0];
                            string Platform = p[1];

                            //bool success = false;
                            XDocument result = null;
                            string callURL = "http://api.flurry.com/appMetrics/ActiveUsers?apiAccessCode=" + ApiKey + "&apiKey=" + p[3] + "&startDate=" + sDate + "&endDate=" + eDate;
                            Debug.WriteLine(callURL);

                            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(callURL);
                            Request.Method = "GET";
                            Request.Accept = "application/xml";
                            try
                            {
                                HttpWebResponse Response = (HttpWebResponse)await Request.GetResponseAsync();
                                if (Response != null)
                                {
                                    StreamReader ResponseDataStream = new StreamReader(Response.GetResponseStream());
                                    string response = ResponseDataStream.ReadToEnd();

                                    result = XDocument.Parse(response);

                                    var data = from query in result.Descendants("day")
                                               select new DataPoint
                                               {
                                                   Value = (double)query.Attribute("value"),
                                                   Label = (string)query.Attribute("date")
                                               };

                                    List<DataPoint> countedItems = data.ToList();

                                    if (countedItems.Count > 0)
                                    {
                                        string resultNumber = countedItems[0].Value.ToString();
                                        string resultDate = countedItems[0].Label.ToString();
                                        Debug.WriteLine("We got count for livetile! " + resultNumber);

                                        TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tile.TileId);

                                        // construct update    
                                        var tileTemplate = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText01);
                                        tileTemplate.GetElementsByTagName("text")[1].InnerText = resultNumber;
                                        tileTemplate.GetElementsByTagName("text")[2].InnerText = "Active users";
                                        tileTemplate.GetElementsByTagName("text")[3].InnerText = resultDate;
                                        tileTemplate.GetElementsByTagName("text")[0].InnerText = Name;
                                        TileNotification tileUpdate = new TileNotification(tileTemplate);
                                        // Set the expiration time on the notification
                                        tileUpdate.ExpirationTime = DateTime.Now.AddHours(6);
                                        tileUpdater.Update(new TileNotification(tileTemplate));
                                        //success = true;
                                    }
                                } // if response not null
                            } // try
                            catch (Exception)
                            { // failed download
                                Debug.WriteLine("Update for SecondaryTile failed");
                            }

                        } // if secondaryTile exists

                    } // only one in loop

                    count++;

                } // foreach SecondaryTile

            }
            defferal.Complete();
        }

        private class DataPoint
        {
            public string Label { get; set; }
            public double Value { get; set; }
        }

    }

}
