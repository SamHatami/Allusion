using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Allusion.Core;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class MainViewModel : Screen
{
    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private ProjectFile _project;
    private string _text;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            NotifyOfPropertyChange(nameof(Text));
        }
    }


    public MainViewModel()
    {
        //Move to project-handler, testing json seriazible for now
        var path = @"C:\Temp\project1.json";
        ProjectFile? project = ProjectFile.Read(Path.GetDirectoryName(path));
        if (project == null)
        {
            _project = new ProjectFile("AllusionTestProject1");
            ProjectFile.Save(_project, path);
        }
    }
    public void PasteOnCanvas(System.Windows.Point e)
    {
        var pastedImage = Clipboard.GetImage();
        //BitmapImage bitmap = 
        //if(image == null) return;
        using (var fileStream = new FileStream(@"C:\Temp\"+"1.png", FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(pastedImage));
            encoder.Save(fileStream);
        }
        //var newImageItem = new ImageItem();

        Images.Add(new ImageViewModel(pastedImage));
    }

    private void ExtractUrlFromDataObject()
    {
        IDataObject dataObject = Clipboard.GetDataObject();
        if (dataObject != null)
        {
            foreach (string format in dataObject.GetFormats())
            {
                object data = dataObject.GetData(format);
                // Inspect data for potential URL or source information
                Debug.WriteLine($"Format: {format}, Data: {data}");
            }
        }
    }
    public void Delete()
    {
        Text = "Pressed delete";
    }

    public void AddDropppedImages(ImageSource[] bitmaps)
    {
        int counter = bitmaps.Length;
        foreach (var bitmap in bitmaps)
        {
            Images.Add(new ImageViewModel(bitmap) {PosX = 10*counter, PosY = 10*counter});
            counter++;
        }
    }
}