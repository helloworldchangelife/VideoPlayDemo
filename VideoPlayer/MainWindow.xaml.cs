using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Timers;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using System.Net;

namespace VideoPlayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public class MyArgs
    {
        public static bool flag = false;
        public static string arg;
    }

    public partial class MainWindow : Window
    {
       
        string pathMedia = "E:\\Videos";//用来记录当前列表的

        private const string defaultPath = "./Dictary.bat";

        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("./Dictary.bat"))
            {
                return;
            }
            StreamReader sr = new StreamReader("./Dictary.bat", Encoding.Default);
            pathMedia = sr.ReadLine();
           
            SetFileList();
        }

        private bool flag = false; //视频状态 播放中为 true;

        private void Paly_Click(object sender, RoutedEventArgs e)
        {
            if (!flag)
            {
                mediaElement1.Play();
                flag = true;
                play.Content = "暂停";
            }
            else
            {
                mediaElement1.Pause();
                flag = false;
                play.Content = "播放";
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement1.Pause();
            flag = false;
            play.Content = "播放";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            mediaElement1.Stop();
            flag = false;
            play.Content = "播放";
        }


        private void Slider_VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement1.Volume = Volume1.Value;
            int? value =(int?) (Volume1?.Value * 100);
            if (volumLabelValue != null)
            {
                volumLabelValue.Content = Convert.ToString(value)+"%";
            }
        }
    

        private void Slider_TimeChanged(object sebder, RoutedPropertyChangedEventArgs<double> e)
        {
            int Slidervalue = (int)TimeLine.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0,0,Slidervalue );
            mediaElement1.Position  = ts;
            BeginTime.Content = ChangeToDate(ts.TotalMilliseconds/1000);
        }

        DispatcherTimer timer = null;
        private void mediaElement1_MediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializePropertyValue();
                TimeLine.Maximum = mediaElement1.NaturalDuration.TimeSpan.TotalMilliseconds;
                string date = ChangeToDate(TimeLine.Maximum/1000);
                EndTime.Content = date;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(1000);
                timer.Tick += new EventHandler(timer_tick);
                timer.Start();
            }
            catch 
            {
               
            }
           
            
        }

        private string ChangeToDate(double maximum)
        {
            long max = Convert.ToInt64(maximum);
            long hours = max / 60/60;
            max = max - hours * 60 * 60;
            long minutes = max / 60;
            max = max - minutes * 60;
            long seconds = max;
            string value = ($"{hours}:{minutes}:{seconds}");
            return value;
            //int minutes = (int) ((maximum - hours * 60 * 60 * 1000) % 1000 % 60);
            //maximum = maximum - hours * 60 * 60 * 1000 - minutes * 60 * 1000;
            //int seconds = (int) maximum / 1000;
        }

        private void timer_tick(object sender, EventArgs e)
        {
            TimeLine.Value = mediaElement1.Position.TotalMilliseconds;
        }

        private void InitializePropertyValue()
        {
            mediaElement1.Volume =  Volume1.Value;
        }


        private void Open_File(object sender, RoutedEventArgs e)
        {
            ShellContainer selectedFolder = null;
            selectedFolder = KnownFolders.SampleVideos as ShellContainer;
            CommonOpenFileDialog cfd = new CommonOpenFileDialog();
            cfd.InitialDirectoryShellContainer = selectedFolder;
            cfd.EnsureReadOnly = true;
            cfd.Filters.Add(new CommonFileDialogFilter("MP4 Files", "*.mp4"));
            cfd.Filters.Add(new CommonFileDialogFilter("WMV Files","*.wmv"));
            cfd.Filters.Add(new CommonFileDialogFilter("AVI Files", "*.avi"));
            
            cfd.Filters.Add(new CommonFileDialogFilter("MPE Files","*.mp3"));

            if (cfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                mediaElement1.Source = new Uri(cfd.FileName,UriKind.Relative);
            }
        }

        private void SetFileList()
        {
            try
            {
                var files = from f in (Directory.GetFiles(pathMedia))
                    where f.Contains(".mp4") || f.Contains(".mp3") || f.Contains(".wvm")
                    select f;

                foreach (var file in files)
                {
                    listView1.Items.Add(file.Substring(file.LastIndexOf('\\') + 1));
                }
            }
            catch (Exception e)
            {
               // Console.WriteLine(e);
                //throw;
            }
          
            
        }

        private void Change_Folder(object sender, RoutedEventArgs e)
        {
            

            ShellContainer selectedFolder = null;
            selectedFolder = KnownFolders.SampleVideos as ShellContainer;

            CommonOpenFileDialog cfd = new CommonOpenFileDialog();
            cfd.IsFolderPicker = true;
            cfd.InitialDirectoryShellContainer = selectedFolder;
            cfd.EnsureReadOnly = true;
            if (cfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SetConfFile(cfd.FileName);
                pathMedia = cfd.FileName;
                SetFileList();
            }

        }

        private void SetConfFile(string name,string path =defaultPath )
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            byte[] date = System.Text.Encoding.Default.GetBytes(name);
            fs.SetLength(0);
            fs.Write(date, 0, date.Length);
            fs.Flush();
            fs.Close();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fileName = listView1.SelectedValue.ToString();
            mediaElement1.Source = new Uri(pathMedia + "//" + fileName);
            mediaElement1.Play();
            play.Content = "暂停";
            flag = true;
        }

        private void Pre_Click(object sender, RoutedEventArgs e)
        {
            int index = listView1.SelectedIndex;
            listView1.SelectedIndex = (index - 1)>=0 ? index-1 : listView1.Items.Count-1;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (listView1.Items.Count <= 0)
            {
                return;
            }
            int index = listView1.SelectedIndex;
            listView1.SelectedIndex = (index + 1) % (listView1.Items.Count);
            string fileName = listView1.SelectedValue.ToString();
            mediaElement1.Source = new Uri(pathMedia + "//" + fileName);
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            string [] s  = (string[])e.Data.GetData(DataFormats.FileDrop,false);
           
            if (File.Exists(s[0]))
            {
                mediaElement1.Source = new Uri(s[0]);
                mediaElement1.Play();
                flag = true;
                play.Content = "暂停";
            }
            else if(Directory.Exists(s[0]))
            {
                var files = from f in (Directory.GetFiles(s[0]))
                    where f.Contains(".mp4") || f.Contains(".mp3") || f.Contains(".wvm")
                    select f;

                if (files.Count()!=0)
                {
                    SetConfFile(s[0]);
                    pathMedia = s[0];
                }
                listView1.Items.Clear();
                foreach (var file in files)
                {
                    listView1.Items.Add(file.Substring(file.LastIndexOf('\\') + 1));
                }
            }
        }

        private void MediaElement1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mediaElement1.Source == null)
            {
                return;
            }
            else
            {
                if (flag)
                {
                    mediaElement1.Pause();
                    flag = false;
                    play.Content = "播放";
                }
                else
                {
                    mediaElement1.Play();
                    flag = true;
                    play.Content = "暂停";
                }
            }
        }
    }
}
