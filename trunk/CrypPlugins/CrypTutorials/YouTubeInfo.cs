using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.CrypTutorials
{
       public class YouTubeInfo
       {
           public string LinkUrl { get; set; }
           public string EmbedUrl { get; set; }
           public string ThumbNailUrl { get; set; }
           public string Description { get; set; }
           public string Title { get; set; }
           public override string ToString()
           {
               return Title;
           }

       }
}
