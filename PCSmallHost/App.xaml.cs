using Sugar.Log;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PCSmallHost
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            LoggerManager.Enable(LoggerType.File, LogLevel.Error);
            _ = LoggerManager.ClearHistories(duration: 30);

            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string msg;
            if (e.ExceptionObject is Exception exception)
            {
                msg = exception.Message;
            }
            else
            {
                msg = (string)e.ExceptionObject;
            }
            LoggerManager.WriteFatal(msg);
            if (MessageBox.Show("【1】程序发生错误，将终止，请联系开发商！") == MessageBoxResult.OK)
            {
                Environment.Exit(-1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                LoggerManager.WriteFatal(e.Exception.Message);
                if (MessageBox.Show("【2】程序发生错误，将终止，请联系开发商！") == MessageBoxResult.OK)
                {
                    Environment.Exit(-2);
                }
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                LoggerManager.WriteFatal(ex.Message);
                if (MessageBox.Show("【3】程序发生致命错误，将终止，请联系开发商！") == MessageBoxResult.OK)
                {
                    Environment.Exit(-3);
                }                
            }
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            PCSmallHost.App app = new PCSmallHost.App();
            try
            {
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception e)
            {
                LoggerManager.WriteFatal(e.Message);
                Environment.Exit(-2);
            }
        }

        private static Mutex m_mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            m_mutex = new Mutex(true, "PCSmallHost", out bool ret);
            if (!ret)
            {
                Environment.Exit(0);
            }
            base.OnStartup(e);
        }

        public static void Restartup()
        {
            try
            {
                m_mutex.Close();
                System.Windows.Forms.Application.Restart();
                Current.Shutdown();
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
