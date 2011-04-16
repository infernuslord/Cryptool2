using System;

namespace KeySearcher
{
    /// <summary>
    /// Represents one entry in our result list
    /// </summary>
    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        //-------
        public string User { get; set; }
        public DateTime Time { get; set; }
        public long Maschid { get; set; }
        public string Maschname { get; set; }
        //-------
    }
}