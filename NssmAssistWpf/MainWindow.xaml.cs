using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssmAssistWpf
{
    /// <summary>
    /// Client register to backgroud service use NSSM
    /// Author: Chancel.Yang
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProgramArgsEntity programArgs;

        private Timer timerRefreshServicesView;

        private string nssmLocalPath;

        public MainWindow()
        {
            InitializeComponent();
            LogInfo("程序启动");
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
            if (!File.Exists(configFilePath))
            {
                LogWarn("配置文件错误，程序即将退出！");
                Environment.Exit(-1);
            }
            try
            {
                var configEntity = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(configFilePath));
                // 转换配置文件为本地文件
                var programArgsPath = Utils.ConvertToLocalPath(configEntity["ProgramArgsUrl"], configEntity["HttpAuthBasic"]);
                // 转换本地配置文件为对象
                programArgs = JsonConvert.DeserializeObject<ProgramArgsEntity>(File.ReadAllText(programArgsPath));
                this.Title = string.Format("{0} - {1}", Dns.GetHostName(), programArgs.ProgramTitle);
                CheckNSSMStatus();
                timerRefreshServicesView = new Timer((s) => { RefreshServicesView(); }, null, 30, 0);
            }
            catch (Exception e)
            {
                LogError(e, "配置文件异常，程序即将退出");
                Environment.Exit(-1);
            }
        }


        /// <summary>
        /// 检查NSSM依赖项状态
        /// </summary>
        private void CheckNSSMStatus()
        {
            LogInfo("正在检查依赖项NSSM");
            try
            {
                Action<string> progressLogAction = (s) => { LogInfo(s); };
                // 转换NSSM文件到本地文件
                var nssmUrl = Utils.ConvertToLocalPath(programArgs.NSSMInfo.Url, programArgs.NSSMInfo.HttpAuthBasic, progressLogAction);
                if (!File.Exists(nssmUrl))
                {
                    LogWarn("缺失依赖插件nssm，安装失败");
                    Environment.Exit(-1);
                    return;
                }
                nssmLocalPath = nssmUrl;
            }
            catch (Exception e)
            {
                LogError(e, "依赖插件NSSM安装失败，程序即将退出");
                LogWarn("依赖插件NSSM安装失败，程序即将退出");
                Environment.Exit(-1);
                throw;
            }
        }

        #region UI Function
        public void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            SetButtonStatus(false);
            var service = this.lvServices.SelectedItem as ServiceInfoEntity;
            Task.Factory.StartNew(() =>
            {
                // 检查即将安装的服务所需的依赖服务
                if (service.DependentService != null && !Utils.VerifiyServiceRunning(service.DependentService)) {
                    var DependentServiceName = service.DependentService;
                    // 查找服务的别名，如果没有在程序服务列表内则直接使用原始服务名称
                    foreach (var serviceItem in programArgs.Services) {
                        if (service.DependentService.Equals(serviceItem.ServiceName)) {
                            DependentServiceName = serviceItem.ServiceAlias;
                            break;
                        }
                    }
                    LogWarn("您选择安装的{0}依赖于{1}，必须先安装{1}", service.ServiceAlias, DependentServiceName);
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }

                // 检查是否已经存在服务
                if (Utils.VerifiyServiceExist(service.ServiceName))
                {
                    LogWarn("{0}服务已经安装，请先卸载服务后执行", service.ServiceName);
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }

                // 配置服务要运行的本地程序
                Action<string> progressLogAction = (s) => { LogInfo(s); };
                var serviceTempDiretoryPath = Utils.ConvertToLocalPath(service.ServiceProgramPath, service.HttpAuthBasic, progressLogAction);
                var serviceLocalDiretoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, service.ServiceName);
                try
                {
                    // 删除旧服务残留的本地程序
                    if (Directory.Exists(serviceLocalDiretoryPath))
                    {
                        Directory.Delete(serviceLocalDiretoryPath, true);
                    }
                    Directory.Move(serviceTempDiretoryPath, serviceLocalDiretoryPath);
                }
                catch (Exception ex)
                {
                    LogWarn("创建服务程序文件夹失败");
                    LogError(ex, "创建服务程序文件夹失败");
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }

                // 计算运行服务程序的路径
                var serviceExcutePath = Path.Combine(serviceLocalDiretoryPath, service.ServiceProgramName);
                // 检查服务要运行的程序是否存在
                if (!File.Exists(serviceExcutePath))
                {
                    LogWarn("服务要运行的程序不存在，安装失败");
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }
                
                // 注册服务
                Action<string> outputAction = (s) => { LogInfo(s); };
                Utils.ExecuteCommand(string.Format("\"{0}\" install \"{1}\" \"{2}\"",nssmLocalPath, service.ServiceName, serviceExcutePath), 0, outputAction);
                Utils.ExecuteCommand(string.Format("\"{0}\" start \"{1}\"", nssmLocalPath, service.ServiceName), 0, outputAction);
                LogWarn(Utils.VerifiyServiceRunning(service.ServiceName) ? "服务安装成功" :
                    "警告：注册服务不成功！请确保退出所有杀毒软件后使用【管理员权限运行】本程序");
                SetButtonStatus(true);
                
                // 刷新服务列表状态
                RefreshServicesView();

            });
        }

        public void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LogInfo("正在获取所有服务的状态");
            RefreshServicesView();
            LogInfo("获取所有服务状态成功");
        }

        public void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            SetButtonStatus(false);
            var service = this.lvServices.SelectedItem as ServiceInfoEntity;
            Task.Factory.StartNew(() =>
            {
                // 检查即将卸载的服务是否有其他依赖于它的服务
                foreach (var serviceItem in programArgs.Services) {
                    if (service.ServiceName.Equals(serviceItem.DependentService)) {
                        if (Utils.VerifiyServiceRunning(serviceItem.ServiceName)) {
                            LogWarn("{1}服务依赖于您当前要卸载的{0}，请先卸载{1}", service.ServiceAlias, serviceItem.ServiceAlias);
                            SetButtonStatus(true);
                            RefreshServicesView();
                            return;
                        }
                    }
                }

                // 检查要卸载的服务是否存在
                if (!Utils.VerifiyServiceExist(service.ServiceName))
                {
                    LogWarn("卸载服务失败，原因：服务不存在");
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }

                // 开始卸载服务
                Action<string> outputAction = (s) => { LogInfo(s); };
                Utils.ExecuteCommand(string.Format("\"{0}\" stop \"{1}\"", nssmLocalPath, service.ServiceName), 0, outputAction);
                Utils.ExecuteCommand(string.Format("\"{0}\" remove \"{1}\" confirm", nssmLocalPath, service.ServiceName), 0, outputAction);
                if (Utils.VerifiyServiceRunning(service.ServiceName))
                {
                    LogWarn("卸载服务失败，服务依旧存在，请确保本程序使用【管理员】权限运行");
                    SetButtonStatus(true);
                    RefreshServicesView();
                    return;
                }
                LogWarn("服务卸载完成");
                SetButtonStatus(true);

                // 刷新服务视图
                RefreshServicesView();
            });

        }

        /// <summary>
        /// 刷新服务视图
        /// </summary>
        private void RefreshServicesView()
        {
            // 更新服务状态集合
            foreach (var service in programArgs.Services)
            {
                service.ServiceAlias = service.ServiceAlias == null || service.ServiceAlias == "" ? service.ServiceName : service.ServiceAlias;
                service.ServiceInstallStatus = Utils.VerifiyServiceExist(service.ServiceName) ? "✔" : "✘";
                service.ServiceRunningStatus = Utils.VerifiyServiceRunning(service.ServiceName) ? "✔" : "✘";
            }
            // 更新服务状态UI
            this.Dispatcher.Invoke(new Action(()=> {
                this.lvServices.ItemsSource = null;
                this.lvServices.ItemsSource = programArgs.Services;
                this.lvServices.SelectedItem = this.lvServices.Items[0];
            }));
        }

        /// <summary>
        /// 改变按钮状态
        /// </summary>
        /// <param name="status"></param>
        private void SetButtonStatus(bool status)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.btnInstall.IsEnabled = status;
                this.btnRefresh.IsEnabled = status;
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
        private void rtbLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtbLog.ScrollToEnd();
        }
        #endregion


    }
}
