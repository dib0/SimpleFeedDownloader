using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using Argotic.Syndication;
using SimpleFeedDownloader.Data;
using System.IO;

namespace SimpleFeedDownloader
{
    class Program
    {
        #region Properties
        private const string SettingsFileName = "SimpleFeedDownloader.settings";
        private static Settings settings, settingsToSave;
        private static List<string> matches = new List<string>();
        private static WebProxy proxy = null;
        #endregion

        #region Static methods
        static void Main(string[] args)
        {
            ReadSettings();
            if (settings == null)
            {
                Console.WriteLine("Error: No settings found.");
                
                return;
            }

            // Set the proxy if configured
            if (!string.IsNullOrEmpty(settings.ProxyUri))
            {
                NetworkCredential nc = null;
                if (!string.IsNullOrEmpty(settings.ProxyUserName))
                    nc = new NetworkCredential(settings.ProxyUserName, settings.ProxyPassword);

                if (nc == null)
                    proxy = new WebProxy(settings.ProxyUri);
                else
                    proxy = new WebProxy(settings.ProxyUri, true, new string[] { }, nc);
            }

            Uri feedUri = new Uri(settings.FeedUri);
            RssFeed feed = null;
            if (proxy == null)
                feed = RssFeed.Create(feedUri);
            else
                feed = RssFeed.Create(feedUri, null, proxy);

            foreach (RssItem item in feed.Channel.Items)
            {
                // Match the title
                if (IsMatch(item.Title))
                {
                    if (item.Enclosures.Count() > 0)
                    {
                        Uri fileUri = null;
                        foreach (RssEnclosure enc in item.Enclosures)
                        {
                            if (enc.ContentType == "application/x-bittorrent")
                                fileUri = enc.Url;
                        }

                        if (fileUri != null)
                        {
                            DownloadItem matchedItem = settings.Items.Where(i => item.Title.StartsWith(i.Match, StringComparison.OrdinalIgnoreCase)).First();
                            DownloadItem matchedItemToSave = settingsToSave.Items.Where(i => item.Title.StartsWith(i.Match, StringComparison.OrdinalIgnoreCase)).First();
                            DateTime lastMatch = matchedItem.LastMatched;
                            
                            if (item.PublicationDate > lastMatch)
                                DownloadFile(fileUri, matchedItemToSave);
                        }
                    }
                }
            }

            SaveSettings();
        }

        static bool IsMatch(string title)
        {
            bool isMatch = false;
            foreach (string m in matches)
            {
                if (title.StartsWith(m, StringComparison.OrdinalIgnoreCase))
                    isMatch = true;
            }

            return isMatch;
        }

        private static void DownloadFile(Uri link, DownloadItem item)
        {
            WebClient wc = new WebClient();

            if (proxy != null)
                wc.Proxy = proxy;

            wc.DownloadFile(link, settings.DownloadDir + link.Segments[link.Segments.Count() - 1]);

            item.LastMatched = DateTime.Now;
        }

        private static void ReadSettings()
        {
            if (File.Exists(SettingsFileName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (StreamReader sr = new StreamReader(SettingsFileName))
                {
                    settings = (Settings)ser.Deserialize(sr);
                }
                ser = new XmlSerializer(typeof(Settings));
                using (StreamReader sr = new StreamReader(SettingsFileName))
                {
                    settingsToSave = (Settings)ser.Deserialize(sr);
                }

                foreach (DownloadItem item in settings.Items)
                    matches.Add(item.Match);
            }
        }

        private static void SaveSettings()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Settings));
            using (StreamWriter sw = new StreamWriter(SettingsFileName))
                ser.Serialize(sw, settingsToSave);
        }
        #endregion
    }
}
