using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Cryptool.CrypTutorials
{
    public class TutorialVideosManager
    {
        private readonly string _url = PluginBase.Properties.Settings.Default.CrypVideoTutorials_URL;
        private List<VideoInfo> _videoInfos;

        public TutorialVideosManager()
        {
            
        }

        /// <summary>
        /// Constructs a new TutorialVideosManager which asks the Video Web Server for
        /// Tutorial Videos Information
        /// </summary>
        /// <param name="url"></param>

        /// <summary>
        /// Fired if video Informations are fetched
        /// </summary>
        public event EventHandler<VideosFetchedEventArgs> OnVideosFetched;

        /// <summary>
        /// Fired in case of an error
        /// </summary>
        public event EventHandler OnVideosFetchErrorOccured;

        /// <summary>
        ///  Helper to generate Test Data for Gui Testing
        /// Fires OnVideosFetched in case of success (amount more than 0)
        /// Fires OnVideosFetchErrorOccured in case of an error (amount less or equal to 0)
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="amount"></param>
        public void GenerateTestData(String videoUrl, int amount)
        {
            if (amount <= 0)
            {
                if (OnVideosFetchErrorOccured != null)
                {
                    OnVideosFetchErrorOccured.Invoke(this, null);
                }
                return;
            }
            _videoInfos = new List<VideoInfo>();
            for (int i = 0; i < amount; i++)
            {
                _videoInfos.Add(new VideoInfo
                {
                    Description =
                        "A tutorial is a method of transferring knowledge and may be used as a part of a " +
                        "learning process. More interactive and specific than a book or a lecture; a " +
                        "tutorial seeks to teach by example and supply the information to complete a " +
                        "certain task.",
                    Id = i.ToString(CultureInfo.InvariantCulture),
                    Timestamp = DateTime.Now,
                    Title = "Tutorial - From Wikipedia, the free encyclopedia",
                    Url = videoUrl
                });
            }
            if (OnVideosFetched != null)
            {
                OnVideosFetched.Invoke(this, new VideosFetchedEventArgs(_videoInfos));
            }
        }

        /// <summary>
        /// Retrieve Video Informations from Server asynchronously
        /// Fires OnVideosFetched in case of success
        /// Fires OnVideosFetchErrorOccured in case of an error
        /// </summary>
        public void GetVideoInformationFromServer()
        {
            var fetchThread = new Thread(delegate()
            {
                try
                {
                    _videoInfos = new List<VideoInfo>();
                    XElement xraw = XElement.Load(_url);
                    XElement xroot = XElement.Parse(xraw.ToString());
                    IEnumerable<VideoInfo> links =
                        (from item in xroot.Descendants("video")
                        let id = item.Element("id")
                        let title = item.Element("title")
                        let description = item.Element("description")
                        let icon = item.Element("icon")
                        let url = item.Element("videoUrl")
                        let timestamp = item.Element("timestamp")
                        select new VideoInfo
                        {
                            Id = id.Value,
                            Title = title.Value,
                            Description = description.Value,
                            Icon = icon.Value,
                            Url = url.Value,
                            Timestamp = DateTime.Parse(timestamp.Value)
                        });
                    _videoInfos.AddRange(links);
                    if (OnVideosFetched != null)
                    {
                        OnVideosFetched.Invoke(this, new VideosFetchedEventArgs(_videoInfos));
                    }
                }
                catch (Exception exception)
                {
                    if (OnVideosFetchErrorOccured != null)
                    {
                        OnVideosFetchErrorOccured.Invoke(this,new ErrorEventArgs(exception));
                    }
                }
            });

            try
            {
                fetchThread.Start();
            }
            catch (Exception exception)
            {
                if (OnVideosFetchErrorOccured != null)
                {
                    OnVideosFetchErrorOccured.Invoke(this, new ErrorEventArgs(exception));
                }
            }
        }
    }

    public class VideosFetchedEventArgs : EventArgs
    {
        public VideosFetchedEventArgs(List<VideoInfo> videoInfos)
        {
            VideoInfos = videoInfos;
        }

        public List<VideoInfo> VideoInfos { get; private set; }
    }
}