using System.Collections.Generic;
using System.Management;

namespace PCSmallHost.PrintCore
{
    public class Usb
    {
        public static string GetUsbPath()
        {
            string strDeviceName = "USB\\\\VID_28E9";
            string pid = "#{28d78fad-5a12-11d1-ae5b-0000f803a8c2}";
            string Win32_PnPEntity = "Select * From Win32_PnPEntity Where DeviceID like '%" + strDeviceName + "%'";
            ManagementObjectSearcher mySearcher = new ManagementObjectSearcher(Win32_PnPEntity);
            List<string> pathList = new List<string>();
            string path = string.Empty;
            foreach (ManagementObject mobj in mySearcher.Get())
            {
                //USB\\VID_28E9&PID_0289\\399823232008
                //\\?\usb#vid_28e9&pid_0289#399823232008#{28d78fad-5a12-11d1-ae5b-0000f803a8c2}
                string strDeviceID = mobj["DeviceID"].ToString();
                strDeviceID = strDeviceID.Replace("\\", "#");
                strDeviceID = "\\\\?\\" + strDeviceID + pid;
                pathList.Add(strDeviceID);
            }
            if (pathList.Count > 0)
            {
                path = pathList[0];
            }
            return path;
        }
    }
}
