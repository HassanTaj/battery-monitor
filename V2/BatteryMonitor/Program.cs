using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace BatteryMonitor {
    class Program {
        private static System.Timers.Timer _timer;
        private static NotifyIcon notifyIcon = new NotifyIcon() {
            Visible = false,
        };

        //private static Image img = Image.FromFile("pv_logo.png");
        private static Image img = Image.FromFile("pv_icon.ico");
        private static Bitmap bmp = new Bitmap(img, 150, 150);

        private const string appGuid = "03237313-9924-4f0a-b955-f036c9a377e9";
        private static Mutex mutex = new Mutex(true, $"{appGuid}");
        //[STAThread]
        static void Main(string[] args) {
            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                ShowDesktopNotification("Battery Monitor Started", "Service was started a new");
                Console.WriteLine("start");
                RunApp();
                mutex.ReleaseMutex();
            } else {
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
            Console.WriteLine("Timer tik");

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && SystemInformation.PowerStatus.BatteryLifePercent == 1) {
                Console.WriteLine("full");
                ShowDesktopNotification("Battery Fully Charged!", "Your Battery Has Been Fully Charged, You Should UnPlug The Charger!");
            }

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline && SystemInformation.PowerStatus.BatteryLifePercent <= 0.20) {
                Console.WriteLine("low");
                ShowDesktopNotification("Battery Level Low!", "Your Battery Is Low, You Should Plug In The Charger!");
            }
        }

        public static void ShowDesktopNotification(string title, string desc, int stayAlive = 2000) {
            if (notifyIcon != null) {
                notifyIcon.Dispose();
            }
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = desc;
            //notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(stayAlive);

            notifyIcon.MouseClick += (e,s) => {
                if (notifyIcon != null) {
                    notifyIcon.Dispose();
                }
            };

            notifyIcon.BalloonTipClosed += (e, s) => {
                if (notifyIcon != null) {
                    notifyIcon.Dispose();
                }
            };

            notifyIcon.Click += (e, s) => {
                if (notifyIcon != null) {
                    notifyIcon.Dispose();
                }
            };
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
