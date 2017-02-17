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
using System.Text;

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
            ResultsTextBlock.Text = ParseResult(results) + "\n\n " + ParseOCRResults(ocrResults);
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

        private string ParseOCRResults(OcrResults results)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (results != null && results.Regions != null)
            {
                stringBuilder.Append(" ");
                stringBuilder.AppendLine();
                foreach (var item in results.Regions)
                {
                    foreach (var line in item.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            stringBuilder.Append(word.Text);
                            stringBuilder.Append(" ");
                        }
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }

        private string ParseResult(AnalysisResult result)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (result == null)
            {
                return stringBuilder.ToString();
            }

            if (result.Metadata != null)
            {
                stringBuilder.AppendLine("Image Format : " + result.Metadata.Format);
                stringBuilder.AppendLine("Image Dimensions : " + result.Metadata.Width + " x " + result.Metadata.Height);
            }

            if (result.ImageType != null)
            {
                string clipArtType;
                switch (result.ImageType.ClipArtType)
                {
                    case 0:
                        clipArtType = "0 Non-clipart";
                        break;
                    case 1:
                        clipArtType = "1 ambiguous";
                        break;
                    case 2:
                        clipArtType = "2 normal-clipart";
                        break;
                    case 3:
                        clipArtType = "3 good-clipart";
                        break;
                    default:
                        clipArtType = "Unknown";
                        break;
                }

                stringBuilder.AppendLine("Clip Art Type : " + clipArtType);

                string lineDrawingType;
                switch (result.ImageType.LineDrawingType)
                {
                    case 0:
                        lineDrawingType = "0 Non-LineDrawing";
                        break;
                    case 1:
                        lineDrawingType = "1 LineDrawing";
                        break;
                    default:
                        lineDrawingType = "Unknown";
                        break;
                }

                stringBuilder.AppendLine("Line Drawing Type : " + lineDrawingType);
            }


            if (result.Adult != null)
            {
                stringBuilder.AppendLine("Is Adult Content : " + result.Adult.IsAdultContent);
                stringBuilder.AppendLine("Adult Score : " + result.Adult.AdultScore);
                stringBuilder.AppendLine("Is Racy Content : " + result.Adult.IsRacyContent);
                stringBuilder.AppendLine("Racy Score : " + result.Adult.RacyScore);
            }

            if (result.Categories != null && result.Categories.Length > 0)
            {
                stringBuilder.AppendLine("Categories : ");
                foreach (var category in result.Categories)
                {
                    stringBuilder.AppendLine("   Name : " + category.Name + "; Score : " + category.Score);
                }
            }

            if (result.Faces != null && result.Faces.Length > 0)
            {
                stringBuilder.AppendLine("Faces : ");
                foreach (var face in result.Faces)
                {
                    stringBuilder.AppendLine("   Age : " + face.Age + "; Gender : " + face.Gender);
                }
            }

            if (result.Color != null)
            {
                stringBuilder.AppendLine("AccentColor : " + result.Color.AccentColor);
                stringBuilder.AppendLine("Dominant Color Background : " + result.Color.DominantColorBackground);
                stringBuilder.AppendLine("Dominant Color Foreground : " + result.Color.DominantColorForeground);

                if (result.Color.DominantColors != null && result.Color.DominantColors.Length > 0)
                {
                    string colors = "Dominant Colors : ";
                    foreach (var color in result.Color.DominantColors)
                    {
                        colors += color + " ";
                    }
                    stringBuilder.AppendLine(colors);
                }
            }

            if (result.Description != null)
            {
                stringBuilder.AppendLine("Description : ");
                foreach (var caption in result.Description.Captions)
                {
                    stringBuilder.AppendLine("   Caption : " + caption.Text + "; Confidence : " + caption.Confidence);
                }
                string tags = "   Tags : ";
                foreach (var tag in result.Description.Tags)
                {
                    tags += tag + ", ";
                }
                stringBuilder.AppendLine(tags);

            }

            if (result.Tags != null)
            {
                stringBuilder.AppendLine("Tags : ");
                foreach (var tag in result.Tags)
                {
                    stringBuilder.AppendLine("   Name : " + tag.Name + "; Confidence : " + tag.Confidence + "; Hint : " + tag.Hint);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
