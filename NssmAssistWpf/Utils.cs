using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
