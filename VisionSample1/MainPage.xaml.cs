using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VisionSample1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly string _subscriptionKey;

        public MainPage()
        {
            //set your key here
            _subscriptionKey = "15ef53c9c4ae4748baacbbced200cfe4";
            this.InitializeComponent();
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".bmp");
            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                await ShowPreviewAndAnalyzeImage(file);
            }
        }

        private async Task ShowPreviewAndAnalyzeImage(StorageFile file)
        {
            //preview image
            if (file != null)
            {
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                ImageToAnalyze.Source = bitmap;
            }
            else
            {             
                //
            }

            //analyze image
            var results = await AnalyzeImage(file);

            //"fr", "ru", "it", "hu", "ja", etc...
            var ocrResults = await AnalyzeImageForText(file, "en");

            //parse result
            //ResultsTextBlock.Text = ParseResult(results) + "\n\n " + ParseOCRResults(ocrResults);
        }

        private async Task<AnalysisResult> AnalyzeImage(StorageFile file)
        {

            VisionServiceClient VisionServiceClient = new VisionServiceClient(_subscriptionKey);

            using (Stream imageFileStream = await file.OpenStreamForReadAsync())
            {
                // Analyze the image for all visual features
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories
            , VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType
            , VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }
        }

        private async Task<OcrResults> AnalyzeImageForText(StorageFile file, string language)
        {
            //language = "fr", "ru", "it", "hu", "ja", etc...
            VisionServiceClient VisionServiceClient = new VisionServiceClient(_subscriptionKey);
            using (Stream imageFileStream = await file.OpenStreamForReadAsync())
            {
                OcrResults ocrResult = await VisionServiceClient.RecognizeTextAsync(imageFileStream, language);
                return ocrResult;
            }
        }
    }
}
