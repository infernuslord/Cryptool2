using System.Collections.Generic;


namespace DimCodeEncoder.model
{
    class DimCodeReturnValue
    {
        public byte[] PureBitmap { get; set; }
        public byte[] PresentationBitmap{ get;  set; } 
        public List<ColoredString> Legend { get; set; }
    }
}
