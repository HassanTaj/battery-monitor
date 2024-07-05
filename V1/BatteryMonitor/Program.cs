using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace BatteryMonitor {
    class Program {
        private static System.Timers.Timer _timer;
        private static NotifyIcon notifyIcon = new NotifyIcon();
        private const string appGuid = "03237313-9924-4f0a-b955-f036c9a377e9";
        private static Mutex mutex = new Mutex(true, $"{appGuid}");
        //[STAThread]
        static void Main(string[] args) {
            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                ShowDesktopNotification("Battery Monitor Started", "Service was started a new");
                RunApp();
                mutex.ReleaseMutex();
            }
            else {
                ShowDesktopNotification("Battery Monitor Already Running", "The Service Is Already Running.");
                //mutex.ReleaseMutex();
            }
        }

        public static void RunApp() {
            _timer = new System.Timers.Timer(5000) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            Thread.Sleep(Timeout.Infinite);
        }
         
        public static void TimerElapsed(object sender, ElapsedEventArgs e) {

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && SystemInformation.PowerStatus.BatteryLifePercent == 1)
                ShowDesktopNotification("Battery Fully Charged!", "Your Battery Has Been Fully Charged, You Should UnPlug The Charger!");

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline && SystemInformation.PowerStatus.BatteryLifePercent <= 0.20)
                ShowDesktopNotification("Battery Level Low!", "Your Battery Is Low, You Should Plug In The Charger!");
        }

        public static void ShowDesktopNotification(string title, string desc, int stayAlive = 2000) {
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = desc;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(stayAlive);
            notifyIcon.Visible = false;
        }

        /// <summary>
        /// this method is under test i hope it will help me register this app and run it at startup
        /// </summary>
        private void InstallMeOnStartUp() {
            try {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }
    }
}
