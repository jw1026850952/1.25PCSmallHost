using PCSmallHost.DB.BLL;
using PCSmallHost.DB.Model;
using PCSmallHost.Util;
using PCSmallHost.UserWindow;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Win32;

namespace PCSmallHost.Util
{
    /// <summary>
    /// 存放常用公共方法的类
    /// </summary>
    public class CommonFunct
    {
        /// <summary>
        /// 根据16进制颜色转换成Brush对象
        /// </summary>       
        public static Brush GetBrush(string strBackGroundColor)
        {
            Color Color = (Color)ColorConverter.ConvertFromString(strBackGroundColor);
            return new SolidColorBrush(Color);
        }

        /// <summary>
        /// Md5加密(小写)
        /// </summary>        
        public static string Md5Encrypt(string strPassWord)
        {
            MD5CryptoServiceProvider MD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            strPassWord = BitConverter.ToString(MD5CryptoServiceProvider.ComputeHash(Encoding.Default.GetBytes(strPassWord )), 4, 8);
            strPassWord = strPassWord.Replace("-", string.Empty).ToLower();
            return strPassWord;
        }

        /// <summary>
        /// 弹窗显示信息
        /// </summary>
        public static void PopupWindow(string strMessage)
        {
            ShowMessage ShowMessage = new ShowMessage(strMessage);
            ShowMessage.Show();
        }

        /// <summary>
        /// 添加注册表记录
        /// </summary>
        public static void AddRegisterRecord(string strRegistryKeyName)
        {
            string strSubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            string strLocation = Assembly.GetExecutingAssembly().Location;
            RegistryKey registryKey = Registry.LocalMachine;
            RegistryKey registryKeySub = registryKey.CreateSubKey(strSubKey);
            object registryValue = registryKeySub.GetValue(strRegistryKeyName);
            if (registryValue == null)
            {
                registryKeySub.SetValue(strRegistryKeyName, strLocation);
            }
            registryKey.Close();
            registryKeySub.Close();
        }

        /// <summary>
        /// 字符串是否全为数字
        /// </summary>
        /// <returns></returns>
        public static bool IsNumeric(string strValue)
        {
            Regex Regex = new Regex(@"^[0-9]+$");
            return Regex.Match(strValue).Success;
        }

        /// <summary>
        /// 获取调试数据记录并写入文件
        /// </summary>
        public static void GetRemoteConnectRecord(string strMsg)
        {
            string strFilePath = string.Format("{0}\\RemoteConnectRecord.txt",
                System.Windows.Forms.Application.StartupPath);
            FileStream FileStream = new FileStream(strFilePath, FileMode.OpenOrCreate);
            byte[] RecordData;
            try
            {
                RecordData = System.Text.Encoding.Default.GetBytes(string.Format("{0}   {1}\r\n\r\n",
                    DateTime.Now.ToString(), strMsg));
            }
            catch (IOException ex)
            {
                RecordData = System.Text.Encoding.Default.GetBytes(ex.StackTrace);
            }
            FileStream.Position = FileStream.Length;
            FileStream.Write(RecordData, 0, RecordData.Length);
            FileStream.Flush();
            FileStream.Close();
        }

        /// <summary>
        /// 从指定的路径读取gif图片并转成Image
        /// </summary>       
        public static System.Drawing.Image ConvertGifFileToImage(string strGifFilePath)
        {
            FileStream FileStream = new FileStream(strGifFilePath, FileMode.Open);
            byte[] byteArray = new byte[FileStream.Length];
            FileStream.Read(byteArray, 0, byteArray.Length);
            FileStream.Seek(0, SeekOrigin.Begin);
            FileStream.Dispose();
            FileStream.Close();

            MemoryStream MemoryStream = new MemoryStream(byteArray);
            return System.Drawing.Image.FromStream(MemoryStream);
        }

        /// <summary>
        /// 测试添加配电箱
        /// </summary>
        public static void TestAddDistributionBox()
        {
            //DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo
            //{
            //    Code = "600001",
            //    Location = 0,
            //    Status = 0,
            //    ErrorTime = string.Empty,
            //    Disable = 0,
            //    OriginX = 0,
            //    OriginY = 0,
            //    TransformX = 0,
            //    TransformY = 0,               
            //    IsEmergency = 0
            //};
            //new CDistributionBox().Add(infoDistributionBox);
        }

        /// <summary>
        /// 测试添加灯具
        /// </summary>
        public static void TestAddLight()
        {
            //List<LightInfo> lstlight = new List<LightInfo>();
            //LightInfo infoLight = new LightInfo
            //{
            //    Code = "700091",
            //    Location = 0,
            //    Status = 2,
            //    BeginStatus = 2,
            //    ErrorTime = string.Empty,
            //    Disable = 0,
            //    PlanLeft1 = 0,
            //    PlanLeft2 = 0,
            //    PlanLeft3 = 0,
            //    PlanLeft4 = 0,
            //    PlanLeft5 = 0,
            //    PlanRight1 = 0,
            //    PlanRight2 = 0,
            //    PlanRight3 = 0,
            //    PlanRight4 = 0,
            //    PlanRight5 = 0,
            //    LightClass = 7,
            //    DisBoxID = 595,
            //    LightIndex = 18,
            //    OriginX = 0,
            //    OriginY = 0,
            //    TransformX = 0,
            //    TransformY = 0,               
            //    IsAuth = 0
            //};
            //lstlight.Add(infoLight);
            //new CLight().Save(lstlight);
            //new CLight().Add(infoLight);
        }

        /// <summary>
        /// 获取灯具类型
        /// </summary>
        /// <param name="lightClass"></param>
        /// <returns></returns>
        public static string GetLightClass(LightInfo infoLight)
        {
            //int IntervalCode = 55000;
            switch (infoLight.LightClass)
            {
                case (int)EnumClass.LightClass.照明灯:
                    return EnumClass.LightClass.照明灯.ToString();
                case (int)EnumClass.LightClass.双向标志灯:
                    return EnumClass.LightClass.双向标志灯.ToString();
                case (int)EnumClass.LightClass.双头灯:
                    return EnumClass.LightClass.双头灯.ToString();
                case (int)EnumClass.LightClass.双向地埋灯:
                    return EnumClass.LightClass.双向地埋灯.ToString();
                case (int)EnumClass.LightClass.安全出口灯:
                    return EnumClass.LightClass.安全出口灯.ToString();
                case (int)EnumClass.LightClass.楼层灯:
                    return EnumClass.LightClass.楼层灯.ToString();
                case (int)EnumClass.LightClass.单向标志灯:
                    return EnumClass.LightClass.单向标志灯.ToString();
                case (int)EnumClass.LightClass.单向地埋灯:
                    return EnumClass.LightClass.单向地埋灯.ToString();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取灯具类型
        /// </summary>
        /// <param name="LightCode"></param>
        /// <returns></returns>
        public static string GetLightClass(object LightCode)
        {
            string type = LightCode is BlankIconInfo ? LightCode.GetType().GetProperty("Type").GetValue(LightCode).ToString() : LightCode.ToString();
            switch(type)
            {
                case "照明灯":
                    return EnumClass.LightClass.照明灯.ToString();
                case "双向标志灯":
                    return EnumClass.LightClass.双向标志灯.ToString();
                case "双头灯":
                    return EnumClass.LightClass.双头灯.ToString();
                case "双向地埋灯":
                    return EnumClass.LightClass.双向地埋灯.ToString();
                case "安全出口":
                    return EnumClass.LightClass.安全出口灯.ToString();
                case "楼层指示":
                    return EnumClass.LightClass.楼层灯.ToString();
                case "单向左向":
                case "单向右向":
                    return EnumClass.LightClass.单向标志灯.ToString();
                case "单向地埋灯":
                    return EnumClass.LightClass.单向地埋灯.ToString();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取灯具状态
        /// </summary>        
        public static string GetLightStatus(LightInfo infoLight, DistributionBoxInfo infoDistributionBox, bool isLightBeginStatus)
        {
            string strLightStatus = string.Empty;
            if (((infoDistributionBox.Status & 0X07FC) & 0X07FC) == 0X07FC && infoDistributionBox.Shield == 1)
            {
                strLightStatus += "正常";
            }
            else
            {
                int lightStatus = 0;
                if (isLightBeginStatus)
                {
                    lightStatus = infoLight.BeginStatus;
                }
                else
                {
                    lightStatus = infoLight.Status;
                }

                if ((lightStatus & (int)EnumClass.LightFaultClass.通信故障) != 0)
                {
                    if (infoLight.Shield == 0)
                    {
                        strLightStatus += "通信故障";
                    }
                    else
                    {
                        strLightStatus += GetNormalLampStatus(infoLight);
                    }
                }
                else
                {
                    if ((lightStatus & (int)EnumClass.LightFaultClass.光源故障) != 0)
                    {

                        strLightStatus += infoLight.Shield == 0 ? "光源故障" : GetNormalLampStatus(infoLight);
                    }
                    else
                    {
                        if ((lightStatus & (int)EnumClass.LightFaultClass.电池故障) != 0)
                        {
                            strLightStatus += infoLight.Shield == 0 ? "电池故障" : GetNormalLampStatus(infoLight);
                        }
                        else
                        {
                            if (!isLightBeginStatus && infoLight.IsEmergency == 1)
                            {
                                strLightStatus += "应急";
                            }
                            else
                            {
                                lightStatus = lightStatus & 0X07;
                                if (infoLight.LightClass == (int)EnumClass.LightClass.双向标志灯 || infoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                                {
                                    switch (lightStatus)
                                    {
                                        case (int)EnumClass.LightStatusClass.双向标志灯全亮:
                                        case (int)EnumClass.LightStatusClass.双向地埋灯全亮:
                                            strLightStatus += "全亮";
                                            break;
                                        case (int)EnumClass.LightStatusClass.左亮:
                                            strLightStatus += "左亮";
                                            break;
                                        case (int)EnumClass.LightStatusClass.右亮:
                                            strLightStatus += "右亮";
                                            break;
                                        case (int)EnumClass.LightStatusClass.全灭:
                                            strLightStatus += "全灭";
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (lightStatus)
                                    {
                                        case (int)EnumClass.LightStatusClass.其它全亮:
                                            strLightStatus += "|全亮|";
                                            break;
                                        case (int)EnumClass.LightStatusClass.全灭:
                                            strLightStatus += "|全灭|";
                                            break;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return strLightStatus;
        }

        private static string GetNormalLampStatus(LightInfo infoLight)
        {
            if (infoLight.LightClass == (int)EnumClass.LightClass.照明灯 || infoLight.LightClass == (int)EnumClass.LightClass.双头灯)
            {
                return "|全灭|";
            }
            else
            {
                return "|全亮|";
            }
        }

        /// <summary>
        /// 获取自带电池灯具的电池状态
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <param name="infoLight"></param>
        /// <returns></returns>
        public static string GetLightBatteryStatus(DistributionBoxInfo infoDistributionBox,LightInfo infoLight)
        {
            string LightBatteryStatus = string.Empty;
            if ((infoDistributionBox.Status & 0X0002) != 0)//判断是否是配电箱模式
            {
                if((infoLight.Status & (int)EnumClass.LightFaultClass.通信故障) != 0)
                {
                    LightBatteryStatus = "——";
                }
                else
                {
                    if ((infoLight.Status & (int)EnumClass.LightFaultClass.电池故障) != 0)
                    {
                        LightBatteryStatus += "故障";
                    }
                    else
                    {
                        if ((infoLight.Status & (int)EnumClass.LightFaultClass.电池充电) != 0)
                        {
                            LightBatteryStatus += "充电";
                        }
                        else
                        {
                            LightBatteryStatus += "主电";
                        }
                    }
                }
            }
            else
            {
                LightBatteryStatus += "非自带电池型";
            }
            return LightBatteryStatus;
        }

        /// <summary>
        /// 获取EPS状态
        /// </summary>
        /// <param name="EPSStatus"></param>
        /// <returns></returns>
        public static string GetEPSStatus(int EPSStatus)
        {
            int ConversionValue = Convert.ToInt32(EPSStatus) & 0X07FC;
            if ((ConversionValue & 0X07FC) == 0X07FC)
            {
                return "配电箱掉线故障";
            }
            if ((ConversionValue & 0X100) == 0X100)
            {
                return "主控板掉线故障";
            }
            if ((ConversionValue & 0X08) == 0X08)
            {
                return "主电故障";
            }
            if ((ConversionValue & 0X04) == 0X04)
            {
                return "充电器故障";
            }
            if ((ConversionValue & 0X200) == 0X200)
            {
                return "电池故障";
            }
            if ((ConversionValue & 0X80) == 0X80)
            {
                return "电池组欠压故障";
            }
            if ((ConversionValue & 0X400) == 0X400)
            {
                return "支路故障";
            }
            if ((ConversionValue & 0X10) == 0X10)
            {
                return "过载故障";
            }
            if ((ConversionValue & 0X40) == 0X40)
            {
                return "综合故障";
            }
            if ((ConversionValue & 0X20) == 0X20)
            {
                return "逆变器故障";
            }
            return "正常";
        }

        /// <summary>
        /// 时间格式化
        /// </summary>
        /// <returns></returns>
        public static string ForMatTime(int time)
        {
            return time > 9 ? time.ToString() : string.Format("0{0}", time);
        }
    }
}
