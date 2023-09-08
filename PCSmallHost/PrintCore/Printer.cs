using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.PrintCore
{
    public class Printer
    {
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Connect_Usb(byte[] portname);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_PrintString(byte[] str, int count = 0);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int QueryPrinterStatus();
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_Reset();
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_SetAlignment(byte nAlignment);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_SetTextScale(byte nWidthScale, byte nHeightScale);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_EPSON_PrintQRCode(byte nUnitWidth, byte bytenECCLevel, byte[] str);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Printer_Line(byte lines);
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_HalfCutBlackMarkPaper();
        [DllImport("PrinterLib.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool POS_FullCutBlackMarkPaper();

        public static async void print(string str)
        {
            //string usbPath = "\\\\?\\usb#vid_28e9&pid_0289#399823232008#{28d78fad-5a12-11d1-ae5b-0000f803a8c2}";
            await Task.Run(() =>
            {
                string usbPath = Usb.GetUsbPath();
                if (usbPath == string.Empty)
                {
                    return;
                }
                byte[] pathBytes = Encoding.Default.GetBytes(usbPath);
                if (Connect_Usb(pathBytes))
                {
                    POS_SetAlignment(0);
                    POS_SetTextScale(8, 8);
                    string dateTimeNow = DateTime.Now.ToString();
                    byte[] printContent = Encoding.Default.GetBytes(string.Format("{0}{1}{2}\n\n\n", dateTimeNow, " ", str));
                    POS_PrintString(printContent);
                }
            });
        }
    }
}
