using System;
using System.Collections.Generic;

using System.Windows;
using System.IO;

using System.Diagnostics;

using Ookii.Dialogs.Wpf;
using System.Net.Http;

namespace HelloWPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {


            String files;
            try
            {
                using (var sr = new StreamReader(recentFilesDir.Text))
                {
                    //files = await sr.ReadToEndAsync();
                    files = await sr.ReadLineAsync();
                    while (files != null)
                    {
                        ListBox1.Items.Add(files);
                        files = await sr.ReadLineAsync();
                    }
                }
                ListBox1.SelectedItem = ListBox1.Items.GetItemAt(0);
            }
            catch (FileNotFoundException ex)
            {
                files = ex.Message;
            }
            IEnumerable<String> filetypes = new string[] { "TGA", "RAWTGA", "JPEG", "IRIS", "IRIZ", "AVIRAW", "AVIJPEG", "PNG", "BMP", "HDR", "TIFF", "OPEN_EXR", "OPEN_EXR_MULTILAYER", "MPEG", "CINEON", "DPX", "DDS", "JP2" };
            FileType.ItemsSource = filetypes;
            FileType.SelectedItem = "PNG";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AnimationButton.IsChecked == true)
            {

                string strCmdText;
                strCmdText = "-b \"" + ListBox1.SelectedItem.ToString() + "\" ";
                if (CustomSettings.IsChecked == true)
                {
                    if (RenderPath.Text != "default")
                    {
                        strCmdText += "-o " + RenderPath.Text + " ";
                    }
                    strCmdText += "-F " + FileType.Text + " ";

                    if (Int32.TryParse(FrameNumber.Text, out int frameNumber))
                    {
                        strCmdText += "-s " + FrameNumber.Text + " -e " + EndFrame.Text + " ";
                    }
                }
                
                strCmdText += "-a";

                Process process = new Process();
                process.StartInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false,
                    //StandardOutputEncoding = Encoding.UTF8,
                    FileName = blenderDirectory.Text + "\\blender.exe",
                    Arguments = strCmdText
                };
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(myProcess_Exited);
                process.Start();
                var responseString = await client.GetStringAsync("http://www.example.com/recepticle.aspx");
            }
            else if (StillButton.IsChecked == true)
            {
                if (Int32.TryParse(FrameNumber.Text, out int frameNumber))
                {
                    string strCmdText;
                    strCmdText = "-b \"" + ListBox1.SelectedItem.ToString() + "\" -F " + FileType.Text + " -f " + FrameNumber.Text;
                    if (RenderPath.Text != "default")
                    {
                        strCmdText = "-b \"" + ListBox1.SelectedItem.ToString() + "\" -o " +RenderPath.Text+"\\ -F " + FileType.Text + " -f " + FrameNumber.Text;
                    }
                    
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo()
                    {
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = false,
                        //StandardOutputEncoding = Encoding.UTF8,
                        FileName = blenderDirectory.Text + "\\blender.exe",
                        Arguments = strCmdText
                    };

                    process.Start();
                }
                else {
                    MessageBox.Show("Frame must be a number");
                        } 
                // Output: -105


            }
        }

        private void myProcess_Exited(object sender, EventArgs e)
        {

        }

        private void StillButton_Click(object sender, RoutedEventArgs e)
        {            
            FileType.Visibility = Visibility.Visible;
            FrameNumber.Visibility = Visibility.Visible;
            EndFrame.Visibility = Visibility.Hidden;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (CustomSettings.IsChecked == true && StillButton.IsChecked == false)
            {
                FileType.Visibility = Visibility.Visible;
                FrameNumber.Visibility = Visibility.Visible;
                EndFrame.Visibility = Visibility.Visible;
            }
            if (CustomSettings.IsChecked == false && StillButton.IsChecked == false)
            {
                FileType.Visibility = Visibility.Hidden;
                FrameNumber.Visibility = Visibility.Hidden;
                EndFrame.Visibility = Visibility.Hidden;
            }
        }

        private void AnimationButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomSettings.IsChecked == false)
            {
                FileType.Visibility = Visibility.Hidden;
                FrameNumber.Visibility = Visibility.Hidden;
            }
            else
            {
                EndFrame.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                RenderPath.Text = ookiiDialog.SelectedPath;
            }
        }
    }
}
