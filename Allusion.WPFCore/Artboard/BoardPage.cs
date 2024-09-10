namespace Allusion.WPFCore.Artboard
{
    [Serializable]
    public class BoardPage
    {
        public string Description { get; set; }
        public ImageItem[] ImageItems { get; set; }

        public NoteItem[] NoteItems { get; set; }

    }


}
