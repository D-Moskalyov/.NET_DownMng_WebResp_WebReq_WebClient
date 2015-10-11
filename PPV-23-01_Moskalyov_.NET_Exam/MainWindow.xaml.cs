using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;

namespace PPV_23_01_Moskalyov_.NET_Exam
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int port = 2000;
        string ip_address = "127.0.0.1";
        Socket socket;
        string pathToCopyTemp = string.Empty;
        List<string> fileList = new List<string>();
        List<string> pathes = new List<string>();
        long size1;
        long size2;
        long size3;

        Dictionary<System.Windows.Controls.Button, DownLoadInfo> pauseDict;
        Dictionary<System.Windows.Controls.Button, DownLoadInfo> stopDict;

        public MainWindow()
        {
            InitializeComponent();

            pauseDict = new Dictionary<System.Windows.Controls.Button, DownLoadInfo>();
            stopDict = new Dictionary<System.Windows.Controls.Button, DownLoadInfo>();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry ipList = Dns.Resolve(ip_address);
            IPAddress ip = ipList.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ip, port);

            try
            {
                socket.Connect(endpoint);

                label.Foreground = Brushes.Green;
                label.Content = "Success!";

            }
            catch (SocketException ex)
            {
                label.Foreground = Brushes.Red;
                label.Content = "Error!";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listBox.Items.Clear();
                fileList.Clear();
                pathes.Clear();
                pauseDict.Clear();
                stopDict.Clear();

                byte[] message_buffer = Encoding.UTF8.GetBytes("GetFileList");
                socket.Send(message_buffer);

                byte[] answerFromServer = new byte[1024];
                int byteCount = socket.Receive(answerFromServer);
                string files = Encoding.UTF8.GetString(answerFromServer, 0, byteCount);

                //socket.Close();

                int first = files.IndexOf(';');
                int last = files.LastIndexOf(';');

                char[] name1 = new char[first];
                char[] name2 = new char[last - first - 1];
                char[] name3 = new char[files.Length - last - 1];

                files.CopyTo(0, name1, 0, name1.Length);
                files.CopyTo(first + 1, name2, 0, name2.Length);
                files.CopyTo(last + 1, name3, 0, name3.Length);

                fileList.Add(new string(name1));
                fileList.Add(new string(name2));
                fileList.Add(new string(name3));

                listBox.Items.Add(fileList[0]);
                listBox.Items.Add(fileList[1]);
                listBox.Items.Add(fileList[2]);
            }
            catch (Exception ex)
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count != 0)
            {
                Console.WriteLine("sdadasd");
                try
                {
                    FolderBrowserDialog foldBrowDialog = new FolderBrowserDialog();
                    foldBrowDialog.Description = "Выбирите папку для сохранения";
                    DialogResult result = foldBrowDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        WebClient client1 = new WebClient();
                        WebClient client2 = new WebClient();
                        WebClient client3 = new WebClient();

                        client1.OpenReadCompleted += client_OpenReadCompleted;
                        client2.OpenReadCompleted += client2_OpenReadCompleted;
                        client3.OpenReadCompleted += client3_OpenReadCompleted;

                        pathToCopyTemp = foldBrowDialog.SelectedPath;

                        //pathes.Add(string.Concat(pathToCopyTemp, "\\", fileList[0]));
                        //pathes.Add(string.Concat(pathToCopyTemp, "\\", fileList[1]));
                        //pathes.Add(string.Concat(pathToCopyTemp, "\\", fileList[2]));
                        pathes.Add(string.Concat("http://networkexam.zz.mu/files/", fileList[0]));
                        pathes.Add(string.Concat("http://networkexam.zz.mu/files/", fileList[1]));
                        pathes.Add(string.Concat("http://networkexam.zz.mu/files/", fileList[2]));

                        //WebRequest webRequest1 = WebRequest.Create(pathes[0]);
                        //HttpWebResponse response1 = (HttpWebResponse)webRequest1.GetResponse();
                        //size1 = response1.ContentLength;
                        client1.OpenReadAsync(new Uri(pathes[0]));

                        //Thread.Sleep(1000);

                        //WebRequest webRequest2 = WebRequest.Create(pathes[1]);
                        //HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
                        //size2 = response2.ContentLength;
                        client2.OpenReadAsync(new Uri(pathes[1]));

                        //WebRequest webRequest3 = WebRequest.Create(pathes[2]);
                        //HttpWebResponse response3 = (HttpWebResponse)webRequest3.GetResponse();
                        //size3 = response3.ContentLength;
                        client3.OpenReadAsync(new Uri(pathes[2]));
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        void client3_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine("3");
                OpenReadCompletedGeneral(2, size3, e.Result);
            }
            else
            {
                Console.WriteLine("!3");
                System.Windows.MessageBox.Show("Путь не существует");
            }
        }

        void client2_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine("2");
                OpenReadCompletedGeneral(1, size2, e.Result);
            }
            else
            {
                Console.WriteLine("!2");
                System.Windows.MessageBox.Show("Путь не существует");
            }
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine("1");
                OpenReadCompletedGeneral(0, size1, e.Result);
            }
            else
            {
                Console.WriteLine("!1");
                System.Windows.MessageBox.Show("Путь не существует");
            }
        } 

        void OpenReadCompletedGeneral(int num, long size, Stream stream)
        {
            string pathToCopy = string.Concat(pathToCopyTemp, "\\", fileList[num]);
            //Console.WriteLine(pathToCopy);
            if (!File.Exists(pathToCopy))
            {
                System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                label.Content = fileList[num];
                label.Height = 30;
                label.Width = 90;
                label.Margin = new Thickness(5);
                label.ToolTip = pathToCopy;

                wrapPanel.Children.Add(label);

                System.Windows.Controls.ProgressBar pB = new System.Windows.Controls.ProgressBar();
                pB.Height = 20;
                pB.Width = 350;
                pB.Margin = new Thickness(5);

                wrapPanel.Children.Add(pB);

                System.Windows.Controls.Button pauseBtn = new System.Windows.Controls.Button();
                pauseBtn.Content = "||";
                pauseBtn.Height = pauseBtn.Width = 20;
                pauseBtn.Margin = new Thickness(5);
                pauseBtn.Click += pauseBtn_Click;

                wrapPanel.Children.Add(pauseBtn);

                System.Windows.Controls.Button stopBtn = new System.Windows.Controls.Button();
                stopBtn.Content = "[]";
                stopBtn.Height = stopBtn.Width = 20;
                stopBtn.Margin = new Thickness(5);
                stopBtn.Click += stopBtn_Click;

                wrapPanel.Children.Add(stopBtn);

                Thread thread = new Thread(DownloadInThread);
                thread.IsBackground = true;

                DownLoadInfo downInfo = new DownLoadInfo(thread, pathToCopy, pB, stream, label, size);

                pauseDict.Add(pauseBtn, downInfo);
                stopDict.Add(stopBtn, downInfo);

                thread.Start(downInfo);
            }
            else
            {
                System.Windows.MessageBox.Show(pathToCopy + " уже существует");
            }
        }

        void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pauseDict.ContainsKey((System.Windows.Controls.Button)sender))
            {
                DownLoadInfo dInfo = pauseDict[(System.Windows.Controls.Button)sender];

                if (!dInfo.Complete)
                {
                    Console.WriteLine(dInfo.Thread.ThreadState.ToString());
                    if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                    {
                        dInfo.Thread.Resume();
                        dInfo.ProgressBar.Foreground = Brushes.Green;
                    }
                    else
                    {
                        dInfo.Thread.Suspend();
                        dInfo.ProgressBar.Foreground = Brushes.Yellow;
                    }
                }
                else
                {
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
            }

        }

        void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (stopDict.ContainsKey((System.Windows.Controls.Button)sender))
            {
                DownLoadInfo dInfo = stopDict[(System.Windows.Controls.Button)sender];

                if (!dInfo.Complete)
                {
                    if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                        dInfo.Thread.Resume();

                    dInfo.Thread.Abort();
                    dInfo.Complete = true;
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
                else
                {
                    pauseDict.Remove((System.Windows.Controls.Button)sender);
                    stopDict.Remove((System.Windows.Controls.Button)sender);
                }
            }
        }

        void DownloadInThread(object dInfos)
        {

            int kbCount = 0;

            int part = 1024;

            DownLoadInfo dInfo = dInfos as DownLoadInfo;

            Stream stream = dInfo.Stream;

            FileStream fs = new FileStream(dInfo.Path, FileMode.Create);
            //FileStream fs = new FileStream("E:\\ubuntu-14.04.1-desktop-amd64.iso\0\0\0", FileMode.Create);

            byte[] buffer = new byte[part];

            int bytesCount = 0;

            try
            {
                while ((bytesCount = stream.Read(buffer, 0, part)) != 0)
                {
                    kbCount++;
                    //Console.WriteLine(dInfo.TotalSize.ToString());
                    dInfo.ProgressBar.Dispatcher.Invoke(new Action(() =>
                    {
                        //dInfo.ProgressBar.Value = kbCount * 100 / (dInfo.TotalSize / 1024);
                    }));

                    fs.Write(buffer, 0, bytesCount);
                }

                //Console.WriteLine(dInfo.Path + " completed");
                dInfo.Complete = true;

                stream.Close();
                fs.Close();
            }
            catch (ThreadAbortException exp)
            {
                //Console.WriteLine("Словил " + exp.Message);

                stream.Close();
                fs.Close();

                File.Delete(dInfo.Path);
                dInfo.Complete = true;
                dInfo.ProgressBar.Dispatcher.Invoke(new Action(() =>
                {
                    dInfo.ProgressBar.Foreground = Brushes.Red;
                }));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (socket != null)
            {
                Dictionary<System.Windows.Controls.Button, DownLoadInfo>.ValueCollection vCol = pauseDict.Values;

                foreach (DownLoadInfo dInfo in vCol)
                {
                    if ((dInfo.Thread.ThreadState & ThreadState.Suspended) != 0)
                    {
                        dInfo.Thread.Resume();
                    }
                    dInfo.Thread.Abort();
                }

                socket.Close();
            }

            
        }
    }
}
