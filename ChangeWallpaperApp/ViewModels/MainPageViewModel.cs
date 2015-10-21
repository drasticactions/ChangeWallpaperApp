using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.UserProfile;
using ChangeWallpaperApp.Tools;
using HtmlAgilityPack;

namespace ChangeWallpaperApp.ViewModels
{
    public class MainPageViewModel : NotifierBase
    {
        public MainPageViewModel()
        {
            // Do proper delegate commands! I'm being lazy. :\
            ClickChangeWallpaperCommand = new AsyncDelegateCommand(async o => { await ChangeWallpaper(false); },
                o => true);
            ClickChangeWallpaperViaNasaCommand = new AsyncDelegateCommand(async o => { await ChangeWallpaperViaNasa(); },
    o => true);
            ClickChangeWallpaperLocalFileCommand = new AsyncDelegateCommand(async o => { await ChangeWallpaper(true); },
   o => true);
            SelectImageCommand = new AsyncDelegateCommand(async o => { await SelectImage(); },
                o => true);
        }

        private bool _isImageSelected;
        private bool _isLoading;
        private StorageFile _selectedImage;
        public bool IsImageSelected
        {
            get { return _isImageSelected; }
            set
            {
                SetProperty(ref _isImageSelected, value);
                OnPropertyChanged();
            }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged();
            }
        }

        public StorageFile SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                SetProperty(ref _selectedImage, value);
                OnPropertyChanged();
            }
        }

        public AsyncDelegateCommand SelectImageCommand { get; private set; }
        public AsyncDelegateCommand ClickChangeWallpaperViaNasaCommand { get; private set; }
        public AsyncDelegateCommand ClickChangeWallpaperLocalFileCommand { get; private set; }
        public AsyncDelegateCommand ClickChangeWallpaperCommand { get; private set; }
        public event EventHandler<EventArgs> ChangeSuccessful;
        public event EventHandler<EventArgs> ChangeFailed;

        public async Task SelectImage()
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file == null) return;
            SelectedImage = file;
            IsImageSelected = true;
        }

        public async Task ChangeWallpaper(bool useLocalFile)
        {

            // Again, being lazy. If you did the delegate command right,
            // this check would not be needed.
            if (SelectedImage == null && !useLocalFile)
            {
                return;
            }

            IsLoading = true;
            var success = false;
            // This may not work if group policies are set on the machine to disable changing these settings.
            // Also not all devices support it either.
            // Make sure you check before you try it!
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                if (useLocalFile)
                {
                    SelectedImage = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/ejp.jpg"));
                }
                success = await profileSettings.TrySetWallpaperImageAsync(SelectedImage);
            }
             IsLoading = false;
            base.RaiseEvent(success ? ChangeSuccessful : ChangeFailed, EventArgs.Empty);
        }

        public async Task ChangeWallpaperViaNasa()
        {
            var url = await GetImageLink();
            SelectedImage = await StoreImage("http://apod.nasa.gov/apod/" + url);
            await ChangeWallpaper(false);
        }

        private async Task<StorageFile> StoreImage(string url)
        {
            var fileName = "picture.jpg";
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(
                new Uri(url), 
                file);

            var res = await download.StartAsync();

            return file;
        }

        private async Task<string> GetImageLink()
        {
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync("http://apod.nasa.gov/apod/astropix.html");
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(string.Format("Failed to load page: {0}", string.Concat(result.StatusCode, Environment.NewLine, "http://apod.nasa.gov/apod/astropix.html")));
                }
                var stream = await result.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    var html = reader.ReadToEnd();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var image = doc.DocumentNode.Descendants("img").FirstOrDefault();
                    return image != null ? image.GetAttributeValue("src", string.Empty) : string.Empty;
                }
            }
        }
    }
}
