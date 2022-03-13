using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.Custom;
using YouTube_Video_Uploader__cross_platform_.ViewModels;
using YouTube_Video_Uploader__cross_platform_.YouTube_API;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace YouTube_Video_Uploader__cross_platform_.Views
{
    public partial class MainWindow : Window
    {
        private OpenFolderDialog openFolderDialog;
        private OpenFileDialog openFileDialog;
        List<string> files;
        List<string> paths;

        public MainWindow()
        {
            InitializeComponent();
            files = new List<string>();
            paths = new List<string>();
            openFileDialog = new OpenFileDialog();
            openFileDialog.AllowMultiple = false;
            FileDialogFilter filter = new FileDialogFilter();
            filter.Extensions.Add("jpg");
            filter.Extensions.Add("png");
            filter.Extensions.Add("jpeg");
            openFileDialog.Filters.Add(filter);
            this.DataContext = new MainWindowViewModel();
            openFolderDialog = new OpenFolderDialog();
            //var D = Enum.GetValues(typeof(Categories)).Cast<Categories>();
            TitleBar.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: false);
            cb_categories.Items = Enum.GetValues(typeof(Categories));
            cb_privacies.Items = Enum.GetValues(typeof(Privacies));
            DisableElements();

        }
        private void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Btn_close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_minimize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Ch_nothumbnail_OnChecked(object sender, RoutedEventArgs e)
        {
            btn_chooseimage.IsEnabled = false;
            tb_image_path.IsEnabled = false;
        }

        private void Ch_nothumbnail_OnUnchecked(object sender, RoutedEventArgs e)
        {
            btn_chooseimage.IsEnabled = true;
            tb_image_path.IsEnabled = true;
        }

       async private void Button_OnClick(object sender, RoutedEventArgs e)
       {
            cb_videos.Items = null;
            paths.Clear();
            files.Clear();
            var result = await openFolderDialog.ShowAsync(this);
            if (result != null)
            {
                string[] arr_files = Directory.GetFiles(result);
            foreach (string filename in arr_files)
            {
                if (System.IO.Path.GetExtension(filename) == ".mp4")
                {
                    files.Add(System.IO.Path.GetFileName(filename));
                    paths.Add(filename);
                }
            }

            if (files.Count > 0)
            {   
                EnableElements();
                cb_videos.Items = files;
            }
            }
                
        }

       private void DisableElements()
       {
           tb_image_path.IsEnabled = false;
           tb_channellink.IsEnabled = false;
           tb_description.IsEnabled = false;
           tb_tags.IsEnabled = false;
           cb_videos.IsEnabled = false;
           tb_title.IsEnabled = false;
           btn_chooseimage.IsEnabled = false;
           btn_upload.IsEnabled = false;
           ch_nothumbnail.IsEnabled = false;
           cb_privacies.IsEnabled = false;
           cb_categories.IsEnabled = false;
       }
       private void EnableElements()
       {
           tb_image_path.IsEnabled = true;
           tb_channellink.IsEnabled = true;
           tb_description.IsEnabled = true;
           tb_tags.IsEnabled = true;
           cb_videos.IsEnabled = true;
           tb_title.IsEnabled = true;
           btn_chooseimage.IsEnabled = true;
           btn_upload.IsEnabled = true;
           ch_nothumbnail.IsEnabled = true;
           cb_privacies.IsEnabled = true;
           cb_categories.IsEnabled = true;
        }

        private async void Btn_chooseimage_OnClick(object sender, RoutedEventArgs e)
        {   
           
            var result = await openFileDialog.ShowAsync(this);
            if (result != null)
            {
                tb_image_path.Text = result[0];
            }
        }

       private async void Btn_upload_OnClick(object sender, RoutedEventArgs e)
       {
           if (cb_videos.SelectedItem != null && !String.IsNullOrEmpty(tb_title.Text) &&
               !String.IsNullOrEmpty(tb_channellink.Text) && cb_categories.SelectedItem != null && cb_privacies.SelectedItem != null
               && !String.IsNullOrEmpty(tb_description.Text) && !String.IsNullOrEmpty(tb_tags.Text))
           {
               if (tb_channellink.Text.StartsWith("https://www.youtube.com/channel/") ||
                   tb_channellink.Text.StartsWith("www.youtube.com/channel/") ||
                   tb_channellink.Text.StartsWith("youtube.com/channel/"))
               {
                   if (ch_nothumbnail.IsChecked == true)
                   {
                       UploadWithoutThumbnail();
                   }

                   if (ch_nothumbnail.IsChecked == false)
                   {
                       UploadWithThumbnail();
                   }
               }
           }
        }
       private async void UploadWithThumbnail()
       {
           var video = new YouTube_Video_Uploader__cross_platform_.YouTube_API.Video()
           {
               VideoPath = paths[cb_videos.SelectedIndex],
               Title = tb_title.Text,
               Description = tb_description.Text,

               Tags = tb_tags.Text.Split(','),

               Category = (Categories)(cb_categories.SelectedItem),
               Privacy = (Privacies)(cb_privacies.SelectedItem)
           };
           var thumbnail = new Thumbnail()
           {
               ThumbnailPath = tb_image_path.Text,
           };
           var jsonSecret = @"YouTube API\secret.json";
           var AppName = "YouTube Video Uploader";
           var channelID = tb_channellink.Text.Substring(tb_channellink.Text.LastIndexOf("/channel/") + 9);
           var result = await Credentials.FromSecret(jsonSecret, AppName, channelID).Authorize().Upload(video).Upload(thumbnail);
           if (result.Status == Statuses.Failed)
           {
             
               var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                   .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                   {
                       ButtonDefinitions = ButtonEnum.OkAbort,
                       ContentTitle = "Error",
                       ContentMessage = $"{result.Error}",
                       FontFamily = "YouTube Sans",
                   });
               msBoxStandardWindow.Show();
            }
       }
       private async void UploadWithoutThumbnail()
       {
           var video = new Video()
           {
               VideoPath = paths[cb_videos.SelectedIndex],
               Title = tb_title.Text,
               Description = tb_description.Text,

               Tags = tb_tags.Text.Split(','),

               Category = (Categories)(cb_categories.SelectedItem),
               Privacy = (Privacies)(cb_privacies.SelectedItem)
           };
           var jsonSecret = @"YouTube API\secret.json";
           var AppName = "YouTube Video Uploader";
           var channelID = tb_channellink.Text.Substring(tb_channellink.Text.LastIndexOf("/channel/") + 9);
           var result = await Credentials.FromSecret(jsonSecret, AppName, channelID).Authorize().Upload(video);
           if (result.Status == Statuses.Failed)
           {
               var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                   .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                   {
                       ButtonDefinitions = ButtonEnum.OkAbort,
                       ContentTitle = "Error",
                       ContentMessage = $"{result.Error}",
                       FontFamily = "YouTube Sans",
                   });
               msBoxStandardWindow.Show();
            }
       }
    }
}
