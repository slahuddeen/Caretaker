using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;

using System.Timers;

namespace CaretakerV1
{

    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            WriteToFile("Started at : " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 36000;
            timer.Enabled = true;
        }
        protected override void OnStop()
        {
            WriteToFile("Stopped at : " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("resuming at " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            //log stuff, made where you droped the service
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\CaretakerLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
                using (StreamWriter sw = File.CreateText(filepath))
                    sw.WriteLine(Message);
            else
                using (StreamWriter sw = File.AppendText(filepath))
                    sw.WriteLine(Message);

            //logic
            string[] sSelectedRootPaths = ConfigurationManager.AppSettings["DeleteDirPaths"].Split(',');
            foreach (string sFolder in sSelectedRootPaths)
                FindFolders(sFolder, filepath);

        }


        protected void FindFolders(string sFolder, string filepath)
        {
            //gets the date from the config
            DateTime date = DateTime.Now.Subtract(TimeSpan.FromDays(Convert.ToInt32(ConfigurationManager.AppSettings.Get("DaysOld"))));
            var aFolders = Directory.GetDirectories(sFolder);
            var aFiles = Directory.GetFiles(sFolder);

            //recursive untill it gets allt the folders and files that fit the criteria
            foreach (string sFile in aFiles)
            {
                DateTime dFileCreationDate = System.IO.File.GetCreationTime(sFile);

                if (dFileCreationDate <= date)
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                        sw.WriteLine("found an outdated File, Deleting: " + sFile.ToString());

                    System.IO.File.Delete(sFile);
                }
            }    

            foreach (var subFolder in aFolders.ToList())
                FindFolders(subFolder, filepath);
        }
    }

}
