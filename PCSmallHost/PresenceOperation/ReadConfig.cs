using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.PresenceOperation
{
    public class ReadConfig
    {
        public static int AutoCheckUsedTime
        {
            get
            {
                int time = 10;

                try
                {
                    string timeStr = ConfigurationManager.AppSettings["AutoCheckUsedTime"];

                    if (string.IsNullOrEmpty(timeStr))
                    {
                        timeStr = "10";
                    }

                    time = int.Parse(timeStr);
                }
                catch { }

                return time;
            }
        }
    }
}
