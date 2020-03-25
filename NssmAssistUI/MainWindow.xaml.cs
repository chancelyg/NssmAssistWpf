using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssmAssistUI
{
    /// <summary>
    /// Client register to backgroud service use NSSM
    /// Author: Chancel.Yang
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServiceInfoEntity serviceInfoEntity;

        public MainWindow()
        {
            InitializeComponent();
            var serviceInfoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceInfo.json");
            if (!File.Exists(serviceInfoPath))
            {
                LogWarn("配置文件错误，程序即将退出！");
                Environment.Exit(-1);
            }
            try
            {
                serviceInfoEntity = JsonConvert.DeserializeObject<ServiceInfoEntity>(File.ReadAllText(serviceInfoPath));
                LogInfo("程序启动");
                LogInfo("正在检查服务状态");
                bool isInstalled = Utils.VerifiyServiceExist(serviceInfoEntity.ServiceName);
                if (isInstalled)
                {
                    LogInfo("服务已安装");
                }
                if (!isInstalled)
                {
                    LogInfo("服务未安装");
                    return;
                }
                bool isRunning = Utils.VerifiyServiceRunning(serviceInfoEntity.ServiceName);
                if (isRunning)
                {
                    LogInfo("服务正在运行中");
                }
                if (!isRunning)
                {
                    LogInfo("服务已安装但未运行");
                }
            }
            catch (Exception e)
            {
                LogError(e, "配置文件格式不正确，程序即将退出");
                Environment.Exit(-1);
            }
        }

        #region UI Function
        public void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            SetButtonStatus(false);
            if (Utils.VerifiyServiceExist(serviceInfoEntity.ServiceName))
            {
                LogWarn("服务已经安装，请先卸载服务后执行");
                SetButtonStatus(true);
                return;
            }
            var nssmPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm.exe");
            if (!File.Exists(nssmPath))
            {
                LogWarn("缺失依赖插件nssm，安装失败");
                return;
            }
            if (!File.Exists(serviceInfoEntity.ServiceProgramPath))
            {
                LogWarn("目标客户端不存在，安装失败");
                return;
            }
            Action<string> outputAction = (s) => { LogInfo(s); };
            Utils.ExecuteCommand(string.Format("\"{0}\" install \"{1}\" \"{2}\"", nssmPath, serviceInfoEntity.ServiceName, serviceInfoEntity.ServiceProgramPath), 0, outputAction);
            Utils.ExecuteCommand(string.Format("\"{0}\" start \"{1}\"", nssmPath, serviceInfoEntity.ServiceName), 0, outputAction);
            if (!Utils.VerifiyServiceRunning(serviceInfoEntity.ServiceName))
            {
                LogWarn("警告：注册服务不成功！请确保退出所有杀毒软件后使用【管理员权限运行】本程序点击修复程序");
            };
            SetButtonStatus(true);
        }

        public void btnRemove_Click(object sender, RoutedEventArgs e)
        {

            if (!Utils.VerifiyServiceExist(serviceInfoEntity.ServiceName))
            {
                LogWarn("卸载服务失败，原因：服务不存在");
                return;
            }
            var nssmPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm.exe");
            Action<string> outputAction = (s) => { LogInfo(s); };
            Utils.ExecuteCommand(string.Format("\"{0}\" stop \"{1}\"", nssmPath, serviceInfoEntity.ServiceName), 0, outputAction);
            Utils.ExecuteCommand(string.Format("\"{0}\" remove \"{1}\" confirm", nssmPath, serviceInfoEntity.ServiceName), 0, outputAction);
            if (Utils.VerifiyServiceRunning(serviceInfoEntity.ServiceName))
            {
                LogWarn("卸载服务失败，服务依旧存在，请确保本程序使用【管理员】权限运行");
                return;
            }
            try
            {
                if (serviceInfoEntity.ServiceProcessAlias == null || "".Equals(serviceInfoEntity.ServiceProcessAlias))
                {
                    serviceInfoEntity.ServiceProcessAlias = System.IO.Path.GetFileNameWithoutExtension(serviceInfoEntity.ServiceProgramPath);
                }
                foreach (Process thisproc in Process.GetProcessesByName(serviceInfoEntity.ServiceProcessAlias))
                {
                    thisproc.Kill();
                }
                LogWarn("服务卸载完成");
            }
            catch (Exception ex)
            {
                LogError(ex, "服务卸载失败");
            }
        }
        private void rtbLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtbLog.ScrollToEnd();
        }
        private void SetButtonStatus(bool status)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.btnInstall.IsEnabled = status;
                this.btnRemove.IsEnabled = status;
            }));
        }

        #endregion

        #region Log Module

        private void LogWarn(string text, params string[] strings)
        {
            text = string.Format(text, strings);
            string logText = string.Format("{0} - {1}\r", DateTime.Now.ToString(), text);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.rtbLog.AppendText(logText);

            }));
            MessageBox.Show(text);
        }

        private void LogInfo(string text, params string[] strings)
        {
            text = string.Format(text, strings);
            string logText = string.Format("{0} - {1}\r", DateTime.Now.ToString(), text);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.rtbLog.AppendText(logText);

            }));
        }

        private void LogError(Exception e, string text, params string[] strings)
        {
            text = string.Format(text, strings);
            string logText = string.Format("{0} - {1}，异常信息：{2}\r", DateTime.Now.ToString(), text, e.Message);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.rtbLog.AppendText(logText);

            }));
        } 
        #endregion
    }
}
