using Allusion.Core.Interfaces;

namespace Allusion.Core.Artboard
{
    [Serializable]
    public class ImageItem : IItem
    {

        public string ImageUri { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double Scale { get; set; }
        public int MemberOfPage { get; set; }

        public ImageItem(string imageUri, double posX, double posY, double scale, int pagerNr)
        {
            ImageUri = imageUri;
            PosX = posX;
            PosY = posY;
            Scale = scale;
            MemberOfPage = pagerNr;
        }

    }
}
