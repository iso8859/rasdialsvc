using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace rasdialsvc
{
    class Settings
    {
        public string pingHost { get; set; }
        public string rasEntry { get; set; }
        public int pingPeriodMs { get; set; }
        public int waitDialMs { get; set; }
        public int waitDisconnectMs { get; set; }

        public static Settings Read()
        {
            string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string file = Path.Combine(Path.GetDirectoryName(exe), Path.GetFileNameWithoutExtension(exe) + ".json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
        }
    }
    class Program
    {
        public class RunTask
        {
            AutoResetEvent m_exit = new AutoResetEvent(false);
            public void Start()
            {
                ThreadPool.QueueUserWorkItem(Run);
            }
            public void Run(object stateInfo)
            {
                //System.Diagnostics.Debugger.Launch();
                Settings settings = Settings.Read();
                do
                {
                    Ping pingSender = new Ping();
                    PingReply reply = pingSender.Send(settings.pingHost, 120);
                    if (reply.Status != IPStatus.Success)
                    {
                        System.Diagnostics.Process.Start("rasdial.exe", settings.rasEntry + " /DISCONNECT");
                        if (!m_exit.WaitOne(settings.waitDisconnectMs))
                        {
                            System.Diagnostics.Process.Start("rasdial.exe", settings.rasEntry);
                            m_exit.WaitOne(settings.waitDialMs);
                        }
                    }
                    // settings = Settings.Read();
                }
                while (!m_exit.WaitOne(settings.pingPeriodMs));
            }
            public void Stop()
            {
                m_exit.Set();
            }
        }
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<RunTask>(s =>
                {
                    s.ConstructUsing(name => new RunTask());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsPrompt(); // Enter the account that create the VPN profile ./user + password
                x.StartAutomatically();

                x.SetDescription("Dial vpn");
                x.SetDisplayName("rasdialsvc, autodial vpn on ping error.");
                x.SetServiceName("rasdialsvc");
            });

        }
    }
}
