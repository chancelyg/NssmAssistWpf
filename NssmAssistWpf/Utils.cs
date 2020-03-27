using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.ServiceProcess;
using System.Text;

namespace NssmAssistWpf
{
    /// <summary>
    /// Utils Class
    /// Author: Chancel.Yang
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 检测服务是否正在运行
        /// </summary>
        /// <returns></returns>
        public static bool VerifiyServiceRunning(string serviceName)
        {
            //获得服务集合
            var serviceControllers = ServiceController.GetServices();
            //遍历服务集合，打印服务名和服务状态
            foreach (var service in serviceControllers)
            {
                if (serviceName.Equals(service.ServiceName))
                {
                    return service.Status == ServiceControllerStatus.Running;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测是否存在服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool VerifiyServiceExist(string serviceName)
        {

            //获得服务集合
            var serviceControllers = ServiceController.GetServices();
            //遍历服务集合，打印服务名和服务状态
            foreach (var service in serviceControllers)
            {
                if (serviceName.Equals(service.ServiceName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 解压ZIP
        /// </summary>
        /// <param name="args">要解压的ZIP文件和解压后保存到的文件夹</param>
        private static void UnZip(string zipPath,string targetPath)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPath));

            Encoding gbk = Encoding.GetEncoding("gbk");      // 防止中文名乱码
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = gbk.CodePage;

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string fileName = Path.GetFileName(theEntry.Name);


                if (fileName != String.Empty)
                {
                    //解压文件到指定的目录
                    string diretoryPath = Path.GetDirectoryName(Path.Combine(targetPath, theEntry.Name));
                    if (!Directory.Exists(diretoryPath)) {
                        Directory.CreateDirectory(diretoryPath);
                    }
                    FileStream streamWriter = File.Create(Path.Combine(targetPath, theEntry.Name));
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)       //  循环写入单个文件
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);     // 每次写入的文件大小
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();

        }


        /// <summary>
        /// 转换成本地路径
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="httpAuthBasic"></param>
        /// <param name="reviceProgressFunc"></param>
        /// <returns></returns>
        public static string ConvertToLocalPath(string fileUrl, string httpAuthBasic = null, Action<string> reviceProgressFunc = null)
        {
            string localPath = null;
            if ("http".Equals(fileUrl.Substring(0, 4)))
            {
                var tempFilePath = System.IO.Path.GetTempFileName();
                switch (fileUrl.Substring(fileUrl.Length - 3, 3))
                {
                    case "zip":
                        Utils.DownloadFile(fileUrl, tempFilePath, httpAuthBasic, reviceProgressFunc);
                        localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Guid.NewGuid().ToString());
                        Directory.CreateDirectory(localPath);
                        Utils.UnZip(tempFilePath, localPath);
                        break;
                    default:
                        Utils.DownloadFile(fileUrl, tempFilePath, httpAuthBasic, reviceProgressFunc);
                        localPath = tempFilePath;
                        break;
                }
            }
            if (!"http".Equals(fileUrl.Substring(0, 4)) && "./".Equals(fileUrl.Substring(0, 2)))
            {
                fileUrl = fileUrl.Remove(0, 2);
                localPath = AppDomain.CurrentDomain.BaseDirectory + fileUrl.Replace("/", "\\\\");
            }
            return localPath;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="downloadPath"></param>
        /// <param name="authBasicStr"></param>
        /// <param name="reviceProgressFunc"></param>
        public static void DownloadFile(string downloadUrl, string downloadPath, string authBasicStr = null, Action<string> reviceProgressFunc = null)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(downloadUrl);
            httpWebRequest.Headers.Add("Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.Default.GetBytes(authBasicStr))));
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var totalBytes = httpWebResponse.ContentLength;
            reviceProgressFunc?.Invoke(string.Format("开始下载文件({0}MB）", (totalBytes / 1024 / 1024).ToString()));
            Stream st = httpWebResponse.GetResponseStream();
            Stream so = new FileStream(downloadPath, FileMode.Create);
            var totalDownloadedByte = 0;
            byte[] fileByte = new byte[1024 * 1000];
            int osize = st.Read(fileByte, 0, (int)fileByte.Length);
            while (osize > 0)
            {
                totalDownloadedByte = osize + totalDownloadedByte;
                so.Write(fileByte, 0, osize);
                osize = st.Read(fileByte, 0, (int)fileByte.Length);
                reviceProgressFunc?.Invoke(string.Format("当前文件下载进度：{0}%", ((float)totalDownloadedByte / (float)totalBytes * 100).ToString()));
            }
            so.Close();
            st.Close();
        }

        /// <summary>
        /// 执行CMD命令
        /// </summary>
        /// <param name="command">CMD命令</param>
        /// <param name="seconds">等待时间</param>
        /// <returns></returns>
        public static int ExecuteCommand(string command, int seconds, Action<string> outputTextFunc = null)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.Start();//启动程序

            //向cmd窗口发送输入信息
            process.StandardInput.WriteLine(command + "&exit");

            process.StandardInput.AutoFlush = true;

            process.WaitForExit(60 * 1000);//等待程序执行完退出进程
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            outputTextFunc?.Invoke(process.StandardOutput.ReadToEnd());
            int exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }
    }
}
