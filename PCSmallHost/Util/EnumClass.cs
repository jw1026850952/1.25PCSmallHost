using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.Util
{
    /// <summary>
    /// 存放所有枚举的类
    /// </summary>
    public class EnumClass
    {
        /// <summary>
        /// 波特率分类
        /// </summary>
        public enum BaudRateClass
        {
            灯具 = 2400,
            配电箱和主机板 = 9600,
            其它 = 19200,
            报警器 = 115200
        }

        /// <summary>
        /// 配电箱状态字分类
        /// </summary>
        public enum DisBoxStatusClass
        {
            正常状态 = 0,
            充电器故障 = 4,
            市电电压故障 = 8,
            过载故障 = 16,
            逆变器故障 = 32,
            综合故障 = 64,
            电池组欠压故障 = 128,
            主控板掉线故障 = 256,
            电池故障 = 512,
            支路故障 = 1024,
            配电箱掉线故障 = 2044,
            强启 = 4096,
            自动 = 8192,
            手动 = 16384,
            测试 = 32768
        }

        

        /// <summary>
        /// 灯具故障分类
        /// </summary>
        public enum LightFaultClass
        {
            电池充电 = 8,
            电池故障 = 16,
            通信故障 = 64,
            光源故障 = 128
        }

        /// <summary>
        /// 灯具分类
        /// </summary>
        public enum LightClass
        {
            照明灯 = 1,
            双向标志灯,
            双头灯,
            双向地埋灯,
            安全出口灯,
            楼层灯,
            单向标志灯,
            单向地埋灯
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        public enum InitialPage
        {
            第一页 = 1,
            第二页,
            第三页,
            第四页,
            第五页
        }

        /// <summary>
        /// 火灾报警器设备类型
        /// </summary>
        public enum FireAlarmLinkDeviceClass
        {
            手动开关 = 1,
            烟雾传感器,
            其余所有
        }

        /// <summary>
        /// 权限分类
        /// </summary>
        public enum RoleClass
        {
            调试人员 = 1,
            管理员
        }

        /// <summary>
        /// 灯状态字分类
        /// </summary>
        public enum LightStatusClass
        {
            全灭 = 0,
            其它全亮 = 2,
            右亮 = 3,
            双向地埋灯全亮 = 5,
            左亮 = 6,
            双向标志灯全亮 = 7
        }

        /// <summary>
        /// 单灯控制分类
        /// </summary>
        public enum SingleLightControlClass
        {
            全亮 = 0x06,
            全灭 = 0X07,
            闪 = 0X08,
            主电 = 0X09,
            左亮 = 0X0A,
            右亮 = 0X0B
        }

        /// <summary>
        /// 单灯当前状态
        /// </summary>
        public enum SingleLightCurrentState
        {
            全灭 = 0,
            全亮 = 1,
            闪 = 2,
            主电 = 3,
            左亮 = 4,
            右亮 = 5
        }

        /// <summary>
        /// 主机板状态分类
        /// </summary>
        public enum HostBoardStatus
        {     
            电池充电=0X01,
            电池短路 =0X02,
            电池故障 = 0X04,
            二百二十伏故障 = 0X08,
            消音按键 = 0X10,
            强启按键 = 0X20,
            自检按键 = 0X40,
            通信故障 = 0XFE,
            月年检按键 = 0X80
        }

        public enum HostBoardReset
        {
            复位 = 0X01
        }

        /// <summary>
        /// 预案号
        /// </summary>
        public enum PlanNumber
        {
            预案1 = 1,
            预案2 = 2,
            预案3 = 3,
            预案4 = 4,
            预案5 = 5,
            预案6 = 6,
            预案7 = 7,
            预案8 = 8,
            预案9 = 9,
            预案10 = 10,
            预案11 = 11,
            预案12 = 12,
            预案13 = 13,
            预案14 = 14,
            预案15 = 15
        }

        /// <summary>
        /// 验证分类
        /// </summary>
        public enum VerifyClass
        {
            登录验证 = 1,
            权限验证=2,
            复位验证=3,
            退出验证=4
        }

        /// <summary>
        /// 故障类型
        /// </summary>
        public enum FaultType
        {
            普通故障 = 1,
            月检故障 = 2,
            年检故障 = 3,
            超时故障 = 4
        }

        /// <summary>
        /// 图标所属的三大表名
        /// </summary>
        public enum TableName
        {
            DistributionBox=1,
            Light=2,
            BlankIcon=3,
            PlanPartitionPointRecord=4,
            EscapeRoutes=5
        }
    }
}
