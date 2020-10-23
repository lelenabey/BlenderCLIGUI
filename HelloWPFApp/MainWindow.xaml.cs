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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AnimationButton.IsChecked == true)
            {

                Render_Animation(ListBox1.SelectedItem.ToString());
                
            }
            else if (StillButton.IsChecked == true)
            {

                Render_Still(ListBox1.SelectedItem.ToString());

            }
        }

        private async void myProcess_Exited(object sender, EventArgs e, String filename)
        {
            DateTime now = DateTime.Now;
            String timetext = now.Hour + ":" + now.Minute;
            var values = new Dictionary<string, string>
                {
                    { "value1", filename },
                    { "value2", timetext }
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://maker.ifttt.com/trigger/render_done/with/key/cYfoKvm6gdn7e49B2AptS0", content);

            var responseString = await response.Content.ReadAsStringAsync();
            //var responseString = await client.GetStringAsync("https://maker.ifttt.com/trigger/render_done/with/key/cYfoKvm6gdn7e49B2AptS0");

            
            this.Dispatcher.Invoke(() =>
            {
                Render_Queue();
            });

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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ListBox2.Items.Add(ListBox1.SelectedItem);
        }

        private void Queue_Start(object sender, RoutedEventArgs e)
        {
            if (ListBox2.HasItems == true)
            {
                Render_Queue();
            }
        }

        private void Delete_From_Queue(object sender, RoutedEventArgs e)
        {
            if (ListBox2.HasItems == true)
            {
                ListBox2.Items.Remove(ListBox2.SelectedItem);
            }
        }

        private void Render_Queue()
        {
            if (ListBox2.HasItems == true)
            {
                //Render_Still(ListBox2.Items.GetItemAt(0).ToString());
                Render_Animation(ListBox2.Items.GetItemAt(0).ToString());
                ListBox2.Items.Remove(ListBox2.Items.GetItemAt(0));
            }
            else
            {
                this.Close();
            }

        }

        private void Render_Animation(String filename)
        {
            string strCmdText;
            strCmdText = "-b \"" + filename + "\" ";
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

            Start_Process(strCmdText, filename);
        }

        private void Render_Still(String filename)
        {
            if (Int32.TryParse(FrameNumber.Text, out int frameNumber))
            {
                string strCmdText;
                strCmdText = "-b \"" + filename + "\" -F " + FileType.Text + " -f " + FrameNumber.Text;
                if (RenderPath.Text != "default")
                {
                    strCmdText = "-b \"" + ListBox1.SelectedItem.ToString() + "\" -o " + RenderPath.Text + "\\ -F " + FileType.Text + " -f " + FrameNumber.Text;
                }

                Start_Process(strCmdText, filename);
                
            }
            else
            {
                MessageBox.Show("Frame must be a number");
            }
            // Output: -105
        }

        private void Start_Process(String strCmdText, String filename)
        {
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
            process.Exited += new EventHandler((s, e) => myProcess_Exited(s, e, filename));
            process.Start();
        }
    }
}
