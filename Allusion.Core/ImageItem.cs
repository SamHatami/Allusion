namespace Allusion.Core
{
    [Serializable]
    public class ImageItem
    {

        public string ImageUri { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double Scale { get; set; }

        public ImageItem(string imageUri, double posX, double posY, double scale)
        {
            ImageUri = imageUri;
            PosX = posX;
            PosY = posY;
            Scale = scale;
        }
    }
}
