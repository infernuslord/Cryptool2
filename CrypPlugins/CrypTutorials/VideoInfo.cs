namespace Cryptool.CrypTutorials
{
    public class VideoInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }           
        public string Url { get; set; }
        public string Timestamp { get; set; }
           
        public override string ToString()
        {
            return Title;
        }
    }
}
