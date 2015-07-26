using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFeedDownloader.Data
{
    [Serializable]
    public class Settings
    {
        public string FeedUri;
        public string DownloadDir;
        public List<DownloadItem> Items;
    }
}
