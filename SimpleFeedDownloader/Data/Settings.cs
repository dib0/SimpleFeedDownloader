﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFeedDownloader.Data
{
    [Serializable]
    public class Settings
    {
        public string FeedUri;
        public string ProxyUri;
        public string ProxyUserName;
        public string ProxyPassword;
        public string DownloadDir;
        public List<DownloadItem> Items;
    }
}
