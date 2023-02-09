﻿using ICSharpCode.SharpZipLib.Zip;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfTmp.Util;

namespace WpfTmp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly Guid CLSID_WshShell = new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8");
        private static string GetShortCutTarget(string lnk)
        {
            if (System.IO.File.Exists(lnk))
            {
                dynamic objWshShell = null, objShortcut = null;
                try
                {
                    objWshShell = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_WshShell));
                    objShortcut = objWshShell.CreateShortcut(lnk);
                    return objShortcut.TargetPath;
                }
                finally
                {
                    Marshal.ReleaseComObject(objShortcut);
                    Marshal.ReleaseComObject(objWshShell);
                }
            }
            return null;
        }
        private static async Task DownloadFile(string url, FileInfo file)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36 Edg/97.0.1072.76");
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("Keep-Alive", "timeout=600");
            var response = await httpClient.GetAsync(url);

            try
            {
                var n = response.Content.Headers.ContentLength;
                var stream = await response.Content.ReadAsStreamAsync();
                using (var fileStream = file.Create())
                using (stream)
                {
                    byte[] buffer = new byte[1024];
                    var readLength = 0;
                    int length;
                    while ((length = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        readLength += length;

                       Debug.WriteLine("下载进度" + ((double)readLength) / n * 100);

                        // 写入到文件
                        fileStream.Write(buffer, 0, length);
                    }
                }

            }
            catch (Exception e)
            {
            }
        }
        /// <summary>
        /// unZip文件解压缩
        /// </summary>
        /// <param name="sourceFile">要解压的文件</param>
        /// <param name="path">要解压到的目录</param>
        public static void ZipDeCompress(string sourceFile, string path)
        {
            if (!File.Exists(sourceFile))
            {
                throw new ArgumentException("要解压的文件不存在。");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                 
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string fileName =  Path.GetFileName(theEntry.Name);
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(path+@"\"+directoryName);
                    }
                    if (fileName != string.Empty)
                    {
                        using (FileStream streamWriter = File.Create(path + @"\" + theEntry.Name))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string jaNetfilterLink = "https://github.com/copyer98/my-utils/raw/main/ja-netfilter-all.zip";
            var save = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\ja-netfilter.zip";
            var saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\ja-netfilter";
            Debug.WriteLine(save);
            FileInfo file = new FileInfo(save);

            Task downloadTask = Task.Run(async () =>
              {
                  await DownloadFile(jaNetfilterLink, file);

              });
            downloadTask.Wait();
            //string savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\ja-netfilter";
            //ZipDeCompress("res//ja-netfilter-all.zip", savePath);
        }

        private void WrapPanel_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("hhhhhh");
            Debug.WriteLine(sender);
            Debug.WriteLine(e); 
            //GetShortCutTarget(e.Data);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string path = @"D:\tmp\tmpgit";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); 
            }
            var git = new CommandRunner("git", path);
            //git.Run("init");
            File.Create(path + @"\read.txt");
            git.Run("add -A");
            git.Run(@"commit -m ""这是自动提交的""");
            MessageBox.Show("success", "title");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            }

            if ((bool)dialog.ShowDialog(this))
            {
                folder.Text = dialog.SelectedPath;
                //MessageBox.Show(this, $"The selected folder was:{Environment.NewLine}{dialog.SelectedPath}", "Sample folder browser dialog");
            }
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(folder.Text + @"\wav"))
            {
                Directory.CreateDirectory(folder.Text + @"\wav");
            }
            
            List<string> files = new List<string>(Directory.GetFiles(folder.Text));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(new string[] { $@"{folder.Text}\wav", Path.GetFileName($"{c}.wav") });
                
                if (Path.GetFileName(c).Contains(".wav.wav"))
                {
                    File.Delete(destFile);
                }
                ProcessStartInfo proc = new ProcessStartInfo(@"D:\programs\vgmstream-win\test.exe") { Arguments =  $@"""{Path.Combine(new string[] { folder.Text, Path.GetFileName(c) })}""" };
                Process.Start(proc);
                
                //覆盖模式  
                
               
            });

            MessageBox.Show("转换成功", "标题");
          
        }

        private void move_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(folder.Text + @"\wav"))
            {
                Directory.CreateDirectory(folder.Text + @"\wav");
            }

            List<string> files = new List<string>(Directory.GetFiles(folder.Text));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(new string[] { $@"{folder.Text}\wav", Path.GetFileName(c) });

                if (Path.GetFileName(c).Contains(".wav.wav"))
                {
                    File.Move(c, destFile);
                }
                

                //覆盖模式  

               

            });
            MessageBox.Show("移动成功", "title");
        }
    }
}
