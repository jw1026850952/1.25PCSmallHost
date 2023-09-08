using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using PCSmallHost.ConSysProtocol;
using PCSmallHost.DB.BLL;
using PCSmallHost.DB.DAL;
using PCSmallHost.DB.DBUtility;
using PCSmallHost.DB.Model;
using PCSmallHost.FireAlarmLink;
using PCSmallHost.FireAlarmLinkage;
using PCSmallHost.GraphicalFunction.EscapeWays;
using PCSmallHost.GraphicalFunction.ViewModel;
using PCSmallHost.PresenceOperation;
using PCSmallHost.PrintCore;
using PCSmallHost.UserWindow;
using PCSmallHost.Util;
using Sugar.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PCSmallHost
{
    /// <summary>
    /// 主程序入口
    /// </summary>
    public partial class MainWindow : Window
    {
        private struct LampNoLoginTag
        {
            public LightInfo infoLight;
            public int tabIndex;
        }

        private struct LayerImageTag
        {
            public object equipment;
            public int status;
            public Point OriginPoint;
        }

        private struct light
        {
            public LightInfo infolight;
            public Point perpendicular;
        };
        /// <summary>
        /// 系统版本确定，false为送检版本
        /// </summary>
        private readonly bool IsCommodity = false;
        /// <summary>
        /// 旧串口提示内容
        /// </summary>
        private string OldShowText = string.Empty;
        /// <summary>
        /// 新串口提示内容
        /// </summary>
        private string NewShowText = string.Empty;
        /// <summary>
        /// 自检总时间
        /// </summary>
        private int SelfCheckingTotalTime;
        /// <summary>
        /// 是否系统自检
        /// </summary>
        private bool IsCheckIndicatorLight = false;
        /// <summary>
        /// 总共执行的时间
        /// </summary>
        private int TotalExecuteSecond;
        /// <summary>
        /// 逃生路线及转折点数量
        /// </summary>
        public static int NewAddNum = 0;

        private System.Windows.Forms.Timer SearchLampProgressBarValueTimer;
        /// <summary>
        /// 定时刷新复位进度条值
        /// </summary>
        private System.Windows.Forms.Timer RefreshProgressBarValueTimer;
        /// <summary>
        /// 复位定时器间隔
        /// </summary>
        private readonly int TimerInterval = 4000;
        /// <summary>
        /// 当前复位进度条值
        /// </summary>
        private double CurrentProgressBarValue;
        /// <summary>
        /// 执行复位指令休眠时间
        /// </summary>
        private readonly int ExeInstructSleepTime = 90;
        /// <summary>
        /// 定时检测手动月年检是否有故障
        /// </summary>
        private System.Windows.Forms.Timer MonthOrSeasonCheckFault;
        /// <summary>
        /// 是否检测EPS状态
        /// </summary>
        private bool IsQueryEPSAndLight = true;

        private bool IsQueryLight = true;
        /// <summary>
        /// 定时执行月检
        /// </summary>
        private System.Windows.Forms.Timer ExecuteMonthCheckTimer;
        /// <summary>
        /// 定时执行年检
        /// </summary>
        private System.Windows.Forms.Timer ExecuteSeasonCheckTimer;

        private bool IsFunOpen;
        /// <summary>
        /// 是否按下强启按钮（外部按钮）
        /// </summary>
        private bool IsKeyEmergency;
        /// <summary>
        /// 是否按下自检按钮（外部按钮）
        /// </summary>
        private bool SelfCheckPhyButton;
        /// <summary>
        /// 是否按下消音按钮（外部按钮）
        /// </summary>
        private bool MutePhyButton;
        /// <summary>
        /// 是否按下月年检按钮（外部按钮）
        /// </summary>
        private bool MonthlyAndYearlyCheckPhyButton;
        /// <summary>
        /// 是否按下复位按钮（外部按钮）
        /// </summary>
        private bool ResetSystemPhyButton;
        /// <summary>
        /// 是否系统月测
        /// </summary>
        private bool IsSystemMonthDetection;
        /// <summary>
        /// 是否系统年检
        /// </summary>
        private bool IsSystemSeasonDetection;
        /// <summary>
        /// 是否处于月年检中的主电计时状态
        /// </summary>
        private bool IsSystemMPDetection;
        /// <summary>
        /// 是否刷新自检应急时间
        /// </summary>
        private bool IsOpenEmTime;
        /// <summary>
        /// 是否刷新自检主电时间
        /// </summary>
        private bool IsOpenMainTime;
        /// <summary>
        /// 下一次月检或年检，false为月检，true为年检
        /// </summary>
        //private bool IsMonthOrSeason;
        /// <summary>
        /// 月检次数
        /// </summary>
        //private int MonthlyCheckTime = 0;
        /// <summary>
        /// 是否加速月年检
        /// </summary>
        private bool IsAccelerateDetection;
        /// <summary>
        /// 是否按下加速月年检按键
        /// </summary>
        private bool IsKeyAccelerateCheck;
        /// <summary>
        /// 是否模拟联动
        /// </summary>
        public bool IsSimulationLinkage;
        /// <summary>
        /// 是否联动计时
        /// </summary>
        public bool IsLinkageTiming;
        /// <summary>
        /// 是否刷新进度条
        /// </summary>
        private bool IsRefreshProgressBar;
        /// <summary>
        /// 是否一键组网
        /// </summary>
        private bool IsOneKey;
        /// <summary>
        /// 是否进行模拟联动定时发送
        /// </summary>
        private bool IsSimulateFireAlarmLink = true;
        /// <summary>
        /// 模拟联动执行预案计时
        /// </summary>
        private System.Windows.Forms.Timer SimulateFireAlarmLinkExePlanTimer;
        /// <summary>
        /// 全体应急计时
        /// </summary>
        private System.Windows.Forms.Timer AllEmergencyTotalTimer;
        /// <summary>
        /// 断主电应急计时
        /// </summary>
        private System.Windows.Forms.Timer NoMainEmergencyTime;
        /// 在规定时间内处理火警联动请求的定时器
        /// </summary>
        //private System.Windows.Forms.Timer RealFireAlarmLinkRequestTimer;
        /// <summary>
        /// 全体主电定时器
        /// </summary>
        private System.Windows.Forms.Timer AllMainEleTimer;
        /// <summary>
        /// 是否真实火灾联动
        /// </summary>
        private bool IsRealFireAlarmLink;
        /// <summary>
        /// 是否消音
        /// </summary>
        private bool IsMute = false;//bool类型默认初始值为false
        /// <summary>
        /// 最近的系统故障
        /// </summary>
        private FaultRecordInfo oldInfoFaultRecord = new FaultRecordInfo();
        /// <summary>
        /// 是否定时查询EPS和灯具数据
        /// </summary>
        private bool IsTimingQueryEPSOrLight;
        /// <summary>
        /// 是否结束查询EPS和灯具数据
        /// </summary>
        private bool IsFinishQueryEPSOrLight = true;
        /// <summary>
        /// 是否显示图标面板信息
        /// </summary>
        private bool IsShowIconSearchCodePanelLogin = false;
        /// <summary>
        /// 是否打开图形界面
        /// </summary>
        private bool IsOpenLayerModeNoLogin = false;
        /// <summary>
        /// 是否打开图形界面
        /// </summary>
        private bool IsOpenLayerModeLogin = false;
        /// <summary>
        /// 是否全体主电
        /// </summary>
        private bool IsAllMainEle = true;
        /// <summary>
        /// 断主电应急
        /// </summary>
        private bool IsEmergency = false;
        /// <summary>
        /// 断主电后应急时间是否达到设定时间
        /// </summary>
        private bool IsTranscendEmergency;
        /// <summary>
        /// 是否强制应急
        /// </summary>
        public bool IsComEmergency = false;
        /// <summary>
        /// 获取配置文件中调试参数的值
        /// </summary>
        public static bool IsRemoteConnect;
        /// <summary>
        /// 是否拖拽楼层图标
        /// </summary>
        private bool IsDragIconSearchCode;
        /// <summary>
        /// 是否编辑全部图标
        /// </summary>
        private bool IsEditAllIcon;
        /// <summary>
        /// 是否添加逃生路线
        /// </summary>
        private bool IsPrintLine;
        /// <summary>
        /// 是否显示逃生路线方向
        /// </summary>
        private bool IsShowDirection;
        /// <summary>
        /// 是否登录
        /// </summary>
        private bool IsLogin;
        /// <summary>
        /// 是否进入复位系统
        /// </summary>
        private bool IsEnterResetSystem;
        /// <summary>
        /// 记录主控器状态
        /// </summary>
        private string MasterControllerStatus = "正常";
        /// <summary>
        /// EPS首页展示当前页
        /// </summary>
        private int EPSImageDisplayCurrentPage;
        /// <summary>
        /// EPS首页展示总共页
        /// </summary>
        private int EPSImageDisplayTotalPage;
        /// <summary>
        /// 灯具首页展示当前页
        /// </summary>
        private int LampImageDisplayCurrentPage;
        /// <summary>
        /// 灯具首页展示总共页
        /// </summary>
        private int LampImageDisplayTotalPage;
        /// <summary>
        /// 配电箱列表当前页
        /// </summary>
        private int EPSListCurrentPageLogin;
        /// <summary>
        /// 配电箱列表总共页
        /// </summary>
        private int EPSListTotalPageLogin;
        /// <summary>
        /// 配电箱总共页
        /// </summary>
        private int EPSListTotalPageNoLogin;
        /// <summary>
        /// 灯具当前页
        /// </summary>
        private int LightListCurrentPageLogin;
        /// <summary>
        /// 灯具当前页
        /// </summary>
        private int LightListCurrentPageNoLogin;
        /// <summary>
        /// 灯具总共页
        /// </summary>
        private int LightListTotalPageLogin;
        /// <summary>
        /// 历史记录当前页
        /// </summary>
        private int HistoryListCurrentPage;
        /// <summary>
        /// 灯具总共页
        /// </summary>
        private int LightListTotalPageNoLogin;
        /// <summary>
        /// 配电箱列表最大列数
        /// </summary>
        private readonly int EPSListColumnCount = 3;
        /// <summary>
        /// 配电箱列表最大行数
        /// </summary>
        private readonly int EPSListMaxRowCountLogin = 8;
        /// <summary>
        /// 灯具列表列数
        /// </summary>
        private readonly int LightListColumnCountLogin = 5;
        /// <summary>
        /// 灯具列表列数
        /// </summary>
        private readonly int LightListColumnCountNoLogin = 7;
        /// <summary>
        /// 灯具列表行数最大值
        /// </summary>
        private readonly int LightListMaxRowCountNoLogin = 5;
        /// <summary>
        /// 灯具列表中间列索引
        /// </summary>
        private readonly int LightListMiddleColumnIndex = 3;
        /// <summary>
        /// 灯具列表中间行索引
        /// </summary>
        private readonly int LightListMiddleRowIndex = 3;
        /// <summary>
        /// 灯具列表最大行数
        /// </summary>
        private readonly int LightListMaxRowCountLogin = 8;
        /// <summary>
        /// 首页EPS列表最大列数
        /// </summary>
        private readonly int EPSImageDisplayColumnCount = 4;
        /// <summary>
        /// 首页EPS列表最大行数
        /// </summary>
        private readonly int EPSImageDisplayMaxRowCount = 3;
        /// <summary>
        /// 首页灯具列表最大列数
        /// </summary>
        private readonly int LampImageDisplayColumnCount = 4;
        /// <summary>
        /// 防火分区数量
        /// </summary>
        private readonly int FireAlarmZoneCount = 255;
        /// <summary>
        /// 间隔码
        /// </summary>
        private readonly int IntervalCode = 50000;
        /// <summary>
        /// 配电箱最小码
        /// </summary>
        private readonly int MinEPSCode = 600001;
        /// <summary>
        /// 配电箱最大码
        /// </summary>
        private readonly int MaxEPSCode = 600500;
        /// <summary>
        /// 搜索EPS百分比
        /// </summary>
        private readonly int SearchEPSPercent = 5;
        /// <summary>
        /// 搜索灯具百分比
        /// </summary>
        private readonly int SearchLightPercent = 100;
        /// <summary>
        /// 正在搜索的EPS个数
        /// </summary>
        private int SearchingEPSCount;
        /// <summary>
        /// 正在搜索的灯具进度条
        /// </summary>
        private double SearchingLightCount;
        /// <summary>
        /// 灯的类别
        /// </summary>
        private readonly int LightType = 8;
        /// <summary>
        /// 灯码范围的个数
        /// </summary>
        private readonly int LightCodeRangeTotalCount = 8 * 9999;
        /// <summary>
        /// 自检加速(月检和年检加起来共十二次，其中月检十一次，年检一次)
        /// </summary>
        private readonly int MonthAndSeasonCheckTime = 12;
        ///<summary>
        ///当前自检次数
        ///</summary>
        private int MonthAndSeasonCheckCurrentTime = 1;
        /// <summary>
        /// 全部EPS寻灯码休眠时间
        /// </summary>
        private readonly int FindLightByAllEPSTime = 120;
        /// <summary>
        /// 当前正在查询灯状态的配电箱
        /// </summary>
        private int EPSQueryCurrentIndex = -5;
        /// <summary>
        /// 当前查询状态的灯具
        /// </summary>
        private int LightQueryCurrentIndex;
        /// <summary>
        /// 当前选中查询的灯状态的配电箱
        /// </summary>
        private int SelectEPSQueryCurrentIndex;
        private readonly DistributionBoxInfo QueryEPSLately = new DistributionBoxInfo();
        /// <summary>
        /// 当前正在查询的配电箱
        /// </summary>
        private DistributionBoxInfo QueryEPS = new DistributionBoxInfo();
        /// <summary>
        /// 当前正在查询的灯具
        /// </summary>
        private readonly LightInfo QueryLamp = new LightInfo();
        /// <summary>
        /// 当前正在查看灯的配电箱
        /// </summary>
        private int EPSViewCurrentIndexNoLogin = 0;
        /// <summary>
        /// 当前正在查看灯的配电箱
        /// </summary>
        private int EPSViewCurrentIndexLogin = 0;
        /// <summary>
        /// 总楼层数
        /// </summary>
        private int TotalFloor;
        /// <summary>
        /// 一页显示七个楼层
        /// </summary>
        private readonly int PerPageFloorNoLogin = 8;
        /// <summary>
        /// 显示当前楼层的页面
        /// </summary>
        private int CurrentPageFloorNoLogin = 1;
        /// <summary>
        /// 当前楼层
        /// </summary>
        private int CurrentSelectFloorLogin = 1;
        /// <summary>
        /// 正在移动的设备
        /// </summary>
        private Image MoveEquipment;
        /// <summary>
        /// 主窗体的固定子控件个数
        /// </summary>
        private readonly int MainWindowChildCount = 7;//34
        /// <summary>
        /// 灯码长度
        /// </summary>
        private readonly int LightCodeLength = 6;
        /// <summary>
        /// EPS码长度
        /// </summary>
        private readonly int EPSCodeLength = 6;
        /// <summary>
        /// 灯具类型
        /// </summary>
        private int LightClass;
        /// <summary>
        /// 模拟火灾分区号
        /// </summary>
        private int SimulateFireAlarmLinkZoneNumber;
        /// <summary>
        /// 应急总计时
        /// </summary>
        private int AllEmergencyTotalTime;
        /// <summary>
        /// 时分秒间隔
        /// </summary>
        private readonly int TimeInterval = 60;
        /// <summary>
        /// 一般指令执行次数
        /// </summary>
        private readonly int ExecuteCommonStructTimes = 3;
        /// <summary>
        /// 主电指令执行次数
        /// </summary>
        private readonly int ExecuteAllMainEleStructTime = 2;
        /// <summary>
        /// 执行一般指令休眠时间
        /// </summary>
        private readonly int ExeCommonInstructSleepTime = 10;
        /// <summary>
        /// 执行全体主电或者全体应急指令休眠时间
        /// </summary>
        private readonly int ExeAllEmergencyOrAllMainEleInstrcutSleepTime = 300;
        /// <summary>
        /// 系统自检定时器间隔
        /// </summary>
        private readonly int SystemSelfCheckTimerInterval = 2000;
        /// <summary>
        /// 主机板自检检测定时器间隔
        /// </summary>
        //private int HostBoardSystemSelfCheckTimerInterval = 3000;
        /// <summary>
        /// 模拟联动执行预案时间定时器间隔
        /// </summary>
        private readonly int SimulateFireAlarmLinkExePlanTimerInterval = 4000;
        /// <summary>
        /// 旧主机故障
        /// </summary>
        private readonly string OldHostFault = string.Empty;
        /// <summary>
        /// 新主机故障
        /// </summary>
        private string NewHostFault = string.Empty;
        /// <summary>
        /// 新灯具故障
        /// </summary>
        private readonly List<string> LampFault = new List<string>();

        //private List<int> LstOldZoneNumber = new List<int>();
        /// <summary>
        /// 故障总数
        /// </summary>
        private int FaultTotalCount;
        /// <summary>
        /// 灯具故障数
        /// </summary>
        //private int FaultLampCount;
        /// <summary>
        /// 检测主机串口是否更新
        /// </summary>
        private bool ReplaceHostSerialPort;
        /// <summary>
        /// 检测配电箱串口是否更新
        /// </summary>
        private bool ReplaceDisSerialPort;
        /// <summary>
        /// 检测火警串口是否更新
        /// </summary>
        private bool ReplaceFireSerialPort;
        /// <summary>
        /// 处理火警联动请求的最大时间(10分钟)
        /// </summary>
        private readonly int SetRequestRealFireAlarmLinkMaxTime = 10 * 60;
        /// <summary>
        /// 处理火警联动请求的累计时间
        /// </summary>
        private int SetRequestRealFireAlarmLinkCalcuTime;
        /// <summary>
        /// 当前执行指令时间
        /// </summary>
        private int CurrentExeInstructTime;
        /// <summary>
        /// 执行指令最大时间 
        /// </summary>
        private readonly int MaxExeInstructTime = 2000;
        /// <summary>
        /// 复位等待时间
        /// </summary>
        private readonly int ResetWaitTime = 9000;
        /// <summary>
        /// 一般定时器间隔
        /// </summary>
        private readonly int CommonTimerInterval = 1000;
        /// <summary>
        /// 联动归一的预案号
        /// </summary>
        private readonly int FireAlarmLinkNormalZoneNumber = 5;
        /// <summary>
        /// 当前楼层EPS和灯具总数
        /// </summary>
        private int CurrentFloorEPSAndLightTotalCountLogin;
        /// <summary>
        /// 当前楼层EPS和灯具总数
        /// </summary>
        private readonly int CurrentFloorEPSAndLightTotalCountNoLogin;
        /// <summary>
        /// 当前选择的转折点
        /// </summary>
        private Point CurrentChoPoint = new Point();
        /// <summary>
        /// 码文本字体大小
        /// </summary>
        private readonly double CodeFontSize = 25;
        /// <summary>
        /// 文本控件顶部边缘
        /// </summary>
        private readonly double MarginTop = 5;
        /// <summary>
        /// 灯具文本控件左边偏移量
        /// </summary>
        private readonly double LightLabelLeftOffsetNoLogin = 120;
        /// <summary>
        /// 灯具文本控件顶部偏移量
        /// </summary>
        private readonly double LightLabelTopOffsetNoLogin = 40;
        /// <summary>
        /// 图标面板左边偏移量
        /// </summary>
        private readonly double PanelLeftOffset = 35;
        /// <summary>
        /// 图标面板右边偏移量
        /// </summary>
        private readonly double PanelRightOffset = 20;
        /// <summary>
        /// 图标面板顶部偏移量
        /// </summary>
        private readonly double PanelTopOffset = 30;
        /// <summary>
        /// 文本框宽度
        /// </summary>
        private readonly double LabelWidth = 150;
        /// <summary>
        /// 文本框高度
        /// </summary>
        private readonly double LabelHeight = 48;
        /// <summary>
        /// 放大缩小图纸的倍数变化值
        /// </summary>
        private readonly double Delta = 0.5;
        /// <summary>
        /// 放大缩小的最小比例尺
        /// </summary>
        private readonly double MinScaleTransform = 1;
        /// <summary>
        /// 放大缩小的最大比例尺
        /// </summary>
        private readonly double MaxScaleTransform = 16;
        /// <summary>
        /// 图纸上的图标尺寸
        /// </summary>
        private double IconSearchCodeSizeNoLogin = 25;
        /// <summary>
        /// 图纸上的图标尺寸
        /// </summary>
        private double IconSearchCodeSizeLogin = 20;//30
        /// <summary>
        /// 图纸上的图标尺寸(用于初始化原来的大小)
        /// </summary>
        private readonly double OriginIconSearchCodeSize = 20;
        /// <summary>
        /// 逃生路线转折点图标尺寸
        /// </summary>
        private readonly double IconSearchRouteCodeSize = 10;
        /// <summary>
        /// 逃生路线转折点图标尺寸（用于初始化原来的大小）
        /// </summary>
        private readonly double OriginIconSearchRouteCodeSize = 10;
        /// <summary>
        /// 放大缩小后图标坐标变化，以及平移图纸后图标坐标是否移出图纸判断的固定比例
        /// </summary>
        private readonly double FixedScaleTransform = 2;
        /// <summary>
        /// 文本框背景色
        /// </summary>
        private readonly string LabelBackground = "#1B3142";
        /// <summary>
        /// 码文本框字体颜色
        /// </summary>
        private readonly string CodeLabelForeground = "#FFFFFF";
        /// <summary>
        /// EPS或灯具应急时字体颜色
        /// </summary>
        private readonly string CodeLabelEmergencyForeground = "#FF0000";
        /// <summary>
        /// EPS或灯具故障时字体颜色
        /// </summary>
        private readonly string CodeLabelFaultForeground = "#FFFF00";
        /// <summary>
        /// 点击码文本框时的背景色
        /// </summary>
        private readonly string ClickedCodeLabelBackground = "#008689";
        /// <summary>
        /// 未选中的楼层文字颜色
        /// </summary>
        private readonly string UnSelectFloorForeground = "#FFFFFF";
        /// <summary>
        /// 选中的楼层文字颜色
        /// </summary>
        private readonly string SelectFloorForeground = "#000000";
        /// <summary>
        /// 当前选中的楼层
        /// </summary>
        private string CurrentSelectFloorNoLogin = "1层";
        /// <summary>
        /// 楼层图纸路径
        /// </summary>
        private string FloorDrawingPath;
        /// <summary>
        /// 存储系统的图层文件名
        /// </summary>
        private List<FloorNameInfo> LstFloorName = new List<FloorNameInfo>();
        /// <summary>
        /// 存放配电箱下灯具的灯码
        /// </summary>
        private string[] LightCodeByEPS;
        /// <summary>
        /// 存放所有的配电箱码
        /// </summary>
        private List<string> EPSCodeFastly;
        /// <summary>
        /// 应急总计时
        /// </summary>
        private string StrAllEmergencyTotalTime;
        /// <summary>
        /// GIF动画文件路径
        /// </summary>
        private string GifFilePath;
        /// <summary>
        /// EPS信息
        /// </summary>
        //private double[] EPSInfo;
        /// <summary>
        /// EPS下灯具状态
        /// </summary>
        private int[] LightStatusByEPSArray;
        /// <summary>
        /// 当前选定的图标
        /// </summary>
        private object SelectIconSearchCode;
        /// <summary>
        /// 从列表中选中的配电箱或者灯具或者报警图标
        /// </summary>
        //private object SelectEquipment;
        /// <summary>
        /// 聚焦的密码框
        /// </summary>
        private PasswordBox FocusPasswordBox;
        /// <summary>
        /// 当前复位进度条
        /// </summary>
        private ProgressBar ResettingProgressBar;
        /// <summary>
        /// 当前楼层真实火灾点数量
        /// </summary>
        private int PartitionPointCurrentFloorNoLoginNum = 0;
        /// <summary>
        /// 当前聚焦的时间文本框
        /// </summary>
        private TextBox FocusTime;
        /// <summary>
        /// 聚焦的内容文本框
        /// </summary>
        private TextBox FocusTextBox;
        /// <summary>
        /// 聚焦的内容label
        /// </summary>
        private Label FocusLabel;
        /// <summary>
        /// 聚焦的返回图片
        /// </summary>
        private Image FocusReturn;
        /// <summary>
        /// 聚焦的灯具状态图片
        /// </summary>
        private Image LampStatus;
        /// <summary>
        /// 选中的线段
        /// </summary>
        private EscapeLinesInfo SelectedLine;

        private Image LampInitialState;
        /// <summary>
        /// 左亮的灯具
        /// </summary>
        private readonly List<LightInfo> LeftBrightLamp = new List<LightInfo>();
        /// <summary>
        /// 需要右亮的灯具
        /// </summary>
        private readonly List<LightInfo> RightBrightLamp = new List<LightInfo>();
        /// <summary>
        /// 需要双亮的灯具
        /// </summary>
        private readonly List<LightInfo> TwoBrightLamp = new List<LightInfo>();
        /// <summary>
        /// 选中的EPS
        /// </summary>
        private DistributionBoxInfo SelectInfoEPSNoLogin = new DistributionBoxInfo();
        /// <summary>
        /// 选中的EPS
        /// </summary>
        private DistributionBoxInfo SelectInfoEPSLogin = new DistributionBoxInfo();
        /// <summary>
        /// 图层里逃生路线转折点
        /// </summary>
        private List<EscapeRoutesInfo> LstEscapeRoutes = new List<EscapeRoutesInfo>();
        /// <summary>
        /// 所有的配电箱
        /// </summary>
        private List<DistributionBoxInfo> LstDistributionBox;
        /// <summary>
        /// 所有设备的图层坐标记录
        /// </summary>
        private List<CoordinateInfo> LstCoordinate;
        /// <summary>
        /// 所有历史记录
        /// </summary>
        private List<HistoricalEventInfo> LstHistoricalEvent;
        /// <summary>
        /// 在历史记录上显示全部记录
        /// </summary>
        private readonly List<HistoricalEventInfo> LstHistoricalShow = new List<HistoricalEventInfo>();
        /// <summary>
        /// 最近出现的故障
        /// </summary>
        private FaultRecordInfo LatelyFaultRecord;
        /// <summary>
        /// 所有故障记录
        /// </summary>
        public static List<FaultRecordInfo> LstFaultRecord;
        /// <summary>
        /// 当前楼层的配电箱
        /// </summary>
        private readonly List<DistributionBoxInfo> LstDistributionBoxCurrentFloorNoLogin = new List<DistributionBoxInfo>();
        /// /// <summary>
        /// 当前楼层的配电箱
        /// </summary>       
        private readonly List<DistributionBoxInfo> LstDistributionBoxCurrentFloorLogin = new List<DistributionBoxInfo>();
        /// <summary>
        /// 当前楼层设备的坐标记录
        /// </summary>
        private List<CoordinateInfo> LstCoordinateCurrentFloorNoLogin = new List<CoordinateInfo>();
        /// <summary>
        /// 当前楼层设备的坐标记录
        /// </summary>
        private List<CoordinateInfo> LstCoordinateCurrentFloorLogin = new List<CoordinateInfo>();
        /// <summary>
        /// 所有楼层的配电箱
        /// </summary>
        private readonly List<DistributionBoxInfo> LstDistributionBoxAllFloor;
        /// <summary>
        /// 故障的配电箱
        /// </summary>
        private readonly DistributionBoxInfo InfoDistributionBoxFault;
        /// <summary>
        /// 选中的灯
        /// </summary>
        private LightInfo SelectInfoLightLogin = new LightInfo();
        /// <summary>
        /// 所有的灯具
        /// </summary>
        private List<LightInfo> LstLight;
        /// <summary>
        /// 所有空白灯具
        /// </summary>
        private List<BlankIconInfo> LstBlankIcon;
        /// <summary>
        /// 配电箱下的所有灯具(视图)
        /// </summary>
        private List<LightInfo> LstLightViewByDisBoxIDNoLogin = new List<LightInfo>();
        /// <summary>
        /// 配电箱下的所有灯具(视图)
        /// </summary>
        private List<LightInfo> LstLightViewByDisBoxIDLogin = new List<LightInfo>();
        /// <summary>
        /// 搜索编码的灯具列表
        /// </summary>
        private List<LightInfo> LstLightIconSearchCodeList;
        /// <summary>
        /// 当前楼层的灯具
        /// </summary>
        private readonly List<LightInfo> LstLightCurrentFloorNoLogin = new List<LightInfo>();
        /// <summary>
        /// 当前楼层的灯具
        /// </summary>
        private readonly List<LightInfo> LstLightCurrentFloorLogin = new List<LightInfo>();
        /// <summary>
        /// 所有楼层的灯具
        /// </summary>
        private readonly List<LightInfo> LstLightAllFloor;
        /// <summary>
        /// 定时查询的灯具
        /// </summary>
        private List<LightInfo> LstLightQueryByEPSID;
        /// <summary>
        /// 状态发生变化的灯具
        /// </summary>
        private readonly List<LightInfo> LstChangeStateLamp;
        /// <summary>
        /// 所有故障的灯具
        /// </summary>
        private readonly List<LightInfo> LstLightFault = new List<LightInfo>();
        /// <summary>
        /// 所有的系统参数
        /// </summary>
        private List<GblSettingInfo> LstGblSetting;
        /// <summary>
        /// 所有的消音设置内容
        /// </summary>
        private List<OtherFaultRecordInfo> LstOtherFaultRecord;
        /// <summary>
        /// 所有的防火分区
        /// </summary>
        private readonly List<FireAlarmZoneInfo> LstFireAlarmZone = new List<FireAlarmZoneInfo>();
        /// <summary>
        /// 火灾报警器类型
        /// </summary>
        private List<FireAlarmTypeInfo> LstFireAlarmType;
        /// <summary>
        /// 分区设置数据
        /// </summary>
        private List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet;
        /// <summary>
        /// 报警点记录
        /// </summary>
        private List<PlanPartitionPointRecordInfo> LstPlanPartitionPointRecord;
        /// <summary>
        /// 真实火灾联动号
        /// </summary>
        private List<int> LstFireAlarmLinkZoneNumber = new List<int>();
        /// <summary>
        /// 当前楼层的报警点记录
        /// </summary>
        private List<PlanPartitionPointRecordInfo> LstPartitionPointCurrentFloorLogin = new List<PlanPartitionPointRecordInfo>();
        /// <summary>
        /// 逃生路线的线段
        /// </summary>
        private List<EscapeLinesInfo> LstEscapeLines;

        private List<EscapeLinesInfo> LstEscapeLinesCurrentFloorLogin = new List<EscapeLinesInfo>();
        /// <summary>
        /// 当前楼层的逃生路线转折点记录
        /// </summary>
        private List<EscapeRoutesInfo> LstEscapeRoutesCurrentFloorLogin = new List<EscapeRoutesInfo>();
        /// <summary>
        /// 放大缩小后显示在当前楼层图形界面的路线转折点记录
        /// </summary>
        private readonly List<EscapeRoutesInfo> LstEscapeRoutesCurrentFloorLoginScaling = new List<EscapeRoutesInfo>();
        /// <summary>
        /// 当前楼层的报警点记录
        /// </summary>
        private readonly List<PlanPartitionPointRecordInfo> LstPartitionPointCurrentFloorNoLogin = new List<PlanPartitionPointRecordInfo>();
        /// <summary>
        /// 定义真实联动的委托
        /// </summary>
        public delegate void DelStartRealFireAlarmLink(List<int> LstFireAlarmLinkZoneNumber);
        /// <summary>
        /// 定义单灯控制的委托
        /// </summary>
        public delegate void DelSingleLightControl(EnumClass.SingleLightControlClass SingleLightControlClass, string strLightCode, string strEPSCode);
        /// <summary>
        /// 定义指定EPS应急或者主电的委托
        /// </summary>      
        public delegate void DelEmergencyOrMainEleByEPS(bool isEmergency, string strEPSCode);
        /// <summary>
        /// 定义移除图标的委托
        /// </summary>        
        public delegate void DelRemoveIconSearchCode(object infoDisBoxOrLight);
        /// <summary>
        /// 定义移除预案分区报警点记录的委托
        /// </summary>      
        public delegate void DelRemovePartitionPoint(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord);
        /// <summary>
        /// 定义改变选中逃生路线线段颜色的委托
        /// </summary>
        /// <param name="LineName"></param>
        public delegate void DelShowSelectEscapeLine(string LineName);
        /// <summary>
        /// 定义去除所选中的逃生路线线段
        /// </summary>
        public delegate void DelRemoveSelectedEscapeLine();
        /// <summary>
        /// 定义图形界面设备编辑信息的委托
        /// </summary>
        public delegate void DelUpdateFacPlanInLayer(DistributionBoxInfo infoDistributionBox, LightInfo infoLight, object infoDisBoxOrLight);
        /// <summary>
        /// 定义图形界面灯具设备与逃生路线的关联
        /// </summary>
        /// <param name="infoLight"></param>
        public delegate void DelLampRelationLines(LightInfo infoLight, DistributionBoxInfo infoDistributionBox);
        /// <summary>
        /// 定义图形界面获取设备旋转角度的委托
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <param name="infoLight"></param>
        public delegate void DelDeviceIconRotate(DistributionBoxInfo infoDistributionBox, LightInfo infoLight);
        /// <summary>
        ///定义主机板通讯的委托
        /// </summary>      
        public delegate void DelHostBoardCom(byte ReturnHostBoardStatus);
        /// <summary>
        /// 真实联动
        /// </summary>
        public static DelStartRealFireAlarmLink OnStartRealFireAlarmLink;
        /// <summary>
        /// 移除图标
        /// </summary>
        public static DelRemoveIconSearchCode OnStartRemoveIconSearchCode;
        /// <summary>
        /// 单灯控制
        /// </summary>
        public static DelSingleLightControl OnStartSingleLightControl;
        /// <summary>
        /// 指定EPS应急或者主电
        /// </summary>
        public static DelEmergencyOrMainEleByEPS OnStartEmergencyOrMainEleByEPS;
        /// <summary>
        /// 显示出图形界面设备编辑界面
        /// </summary>
        public static DelUpdateFacPlanInLayer OnstartUpdateFacPlanInLayer;
        /// <summary>
        /// 设置灯具设备与逃生路线的关联信息
        /// </summary>
        public static DelLampRelationLines OnStartLampRelationLines;
        /// <summary>
        /// 获取图形界面图标旋转的角度
        /// </summary>
        public static DelDeviceIconRotate OnstartDeviceIconRotate;
        /// <summary>
        /// 移除预案分区报警点记录
        /// </summary>
        public static DelRemovePartitionPoint OnStartRemovePartitionPoint;
        /// <summary>
        /// 显示出已选中的逃生路线线段
        /// </summary>
        public static DelShowSelectEscapeLine OnSartShowSelectEscapeLine;
        /// <summary>
        /// 去除已选中的逃生路线线段
        /// </summary>
        public static DelRemoveSelectedEscapeLine OnStartRemoveSelectedEscapeLine;
        /// <summary>
        /// 记录图标信息面板边缘
        /// </summary>
        private Point LightInfoPanelMargin;
        /// <summary>
        /// 图纸的中心坐标
        /// </summary>
        private Point FloorDrawingCenterLogin;
        /// <summary>
        /// 未登录的图纸中心坐标
        /// </summary>
        private Point FloorDrawingCenterNoLogin;
        /// <summary>
        /// 图纸的高度宽度
        /// </summary>
        private Point FloorDrawingPosition;

        private Point FloorDrawingPositionNoLogin;
        /// <summary>
        /// 图纸边框左侧线段
        /// </summary>
        private readonly EscapeLinesInfo FloorDrawingLeft = new EscapeLinesInfo();
        /// <summary>
        /// 图纸边框右侧线段
        /// </summary>
        private readonly EscapeLinesInfo FloorDrawingRight = new EscapeLinesInfo();
        /// <summary>
        /// 图纸边框顶部线段
        /// </summary>
        private readonly EscapeLinesInfo FloorDrawingTop = new EscapeLinesInfo();
        /// <summary>
        /// 图纸边框底部线段
        /// </summary>
        private readonly EscapeLinesInfo FloorDrawingBottom = new EscapeLinesInfo();
        /// <summary>
        /// 记录拖拽到图纸上时的转换坐标
        /// </summary>
        private Point DragFloorNoLogin;
        /// <summary>
        /// 记录拖拽到图纸上时的转换坐标
        /// </summary>
        private Point DragFloorLogin;
        /// <summary>
        /// 记录上次拖拽到图纸上时的坐标
        /// </summary>
        private Point LastDragFloorNoLogin;
        /// <summary>
        /// 记录上次拖拽到图纸上时的坐标
        /// </summary>
        private Point LastDragFloorLogin;
        /// <summary>
        /// 记录拖拽到图纸上时的原始坐标
        /// </summary>
        private Point OriginDragFloorLogin;
        /// <summary>
        /// 记录未登录时拖拽到图纸上时的原始坐标
        /// </summary>
        private Point OriginDragFloorNoLogin;
        /// <summary>
        /// 记录配电箱或者灯具拖拽的起始坐标
        /// </summary>
        private Point StartPositionDragFloor;
        /// <summary>
        /// 记录未登录时配电箱或者灯具拖拽的起始坐标
        /// </summary>
        private Point StartPositionDragFloorNoLogin;
        /// <summary>
        /// 移动图层上图标的转换坐标
        /// </summary>
        private Point NewMoveAllIconLogin;
        /// <summary>
        /// 移动图层上图标的原始坐标
        /// </summary>
        private Point OriginMoveAllIconLogin;
        /// <summary>
        /// 移动图层上图标后，记录未登录的转换坐标
        /// </summary>
        private Point NewMoveAllIconNoLogin;
        /// <summary>
        /// 移动图层上图标后，记录未登录的原始坐标
        /// </summary>
        private Point OriginMoveAllIconNoLogin;
        /// <summary>
        /// EPS和灯具图标信息面板的坐标
        /// </summary>
        private Point IconSearchCodeInfoPanelPosNoLogin;
        /// <summary>
        /// EPS和灯具图标信息面板的坐标
        /// </summary>
        private Point IconSearchCodeInfoPanelPosLogin;
        /// <summary>
        /// 报警点信息面板的坐标
        /// </summary>
        private Point PartitionPointInfoPanelPosLogin;
        /// <summary>
        /// EPS和灯具图标信息面板的高度宽度
        /// </summary>
        private Point IconSearchCodeInfoPanelAreaNoLogin;
        /// <summary>
        /// EPS和灯具图标信息面板的高度宽度
        /// </summary>
        private Point IconSearchCodeInfoPanelAreaLogin;
        /// <summary>
        /// 报警点信息面板的高度宽度
        /// </summary>
        private Point PartitionPointInfoPanelAreaLogin;
        /// <summary>
        /// 图层智能添加的空白图标
        /// </summary>
        private Image BlankIcon = new Image();
        /// <summary>
        /// 调整图纸大小
        /// </summary>
        private SliderControl SliderControlNoLogin;
        /// <summary>
        /// 调整图纸大小
        /// </summary>
        private SliderControl SliderControlLogin;
        /// <summary>
        /// 进行图纸变换的组件
        /// </summary>
        private TransformGroup TransformGroupNoLogin;
        /// <summary>
        /// 进行图纸变换的组件
        /// </summary>
        private TransformGroup TransformGroupLogin;
        /// <summary>
        /// 在二维空间内平移的组件
        /// </summary>
        private TranslateTransform TranslateTransformNoLogin;
        /// <summary>
        /// 在二维空间内平移的组件
        /// </summary>
        private TranslateTransform TranslateTransformLogin;
        /// <summary>
        /// 在二维空间内缩放的控件
        /// </summary>
        private ScaleTransform ScaleTransformNoLogin;
        /// <summary>
        /// 在二维空间内缩放的控件
        /// </summary>
        private ScaleTransform ScaleTransformLogin;
        /// <summary>
        /// 记录灯具故障数目
        /// </summary>
        private Dictionary<string,int> FaultLightCount = new Dictionary<string,int>();
        /// <summary>
        /// 保存报错记录阈值
        /// </summary>
        private Dictionary<string,int> FaultCord = new Dictionary<string, int>();
        /// <summary>
        /// 判断电池短路是否输出
        /// </summary>
        private bool BatteryShort = false;

        /// <summary>
        /// 自检时故障数记录
        /// </summary>
        private int FaultCount=0;

        private bool IsLoadPicture = false;

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        private const uint WM_SYSCOMMAND = 0X0112;
        private const uint SC_MONITORPOWER = 0XF170;

        private readonly CDistributionBox ObjDistributionBox = new CDistributionBox();
        private readonly CLight ObjLight = new CLight();
        private readonly CGblSetting ObjGblSetting = new CGblSetting();
        private readonly COtherFaultRecord ObjOtherFaultRecord = new COtherFaultRecord();
        private readonly CFireAlarmType ObjFireAlarmType = new CFireAlarmType();
        private readonly CFireAlarmPartitionSet ObjFireAlarmPartitionSet = new CFireAlarmPartitionSet();
        private readonly CPlanPartitionPointRecord ObjPlanPartitionPointRecord = new CPlanPartitionPointRecord();
        private readonly CHistoricalEvent ObjHistoricalEvent = new CHistoricalEvent();
        private readonly CFloorName ObjFloorName = new CFloorName();
        private readonly CEscapeRoutes ObjEscapeRoutes = new CEscapeRoutes();
        private readonly CFaultRecord ObjFaultRecord = new CFaultRecord();
        private readonly CEscapeLines ObjEscapeLines = new CEscapeLines();
        private readonly CCoordinate ObjCoordinate = new CCoordinate();
        private readonly CBlankIcon ObjBlankIcon = new CBlankIcon();

        private readonly SerialMonitor _fireAlarmSerialMonitor = new SerialMonitor();
        private FireAlarmBase _fireAlarm;
        private string _fireAlarmType;
        //private string SelectedLineName;

        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            InitBasicData();
            InitFireAlarmPartitionSet();
            InitAllSerialPort();
            InitFireAlarmLinkZone();
            InitMainWindow();
            InitGifFile();
            //InitLayerModeNoLogin();
            UpdateFaultLightCount();
            InitEPSShowNoLogin();
            //InitIconSearchCodeListLogin();
            AuthIconSearchCode();
            

            CheckUsedManager.TimeToFinishedEvebt -= new EventHandler(CheckUsedManager_TimeToFinishedEvent);

            CheckUsedManager.TimeToFinishedEvebt += new EventHandler(CheckUsedManager_TimeToFinishedEvent);
        }

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, uint wParam, int lParam);

        /// <summary>
        /// 30分钟无操作后，自动息屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUsedManager_TimeToFinishedEvent(object sender, EventArgs e)
        {
            //SetSuspendState(false, true, true);//电脑进入休眠状态

            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitMainWindow()
        {

            FloorDrawingPath = string.Format("{0}\\FloorDrawing", System.Windows.Forms.Application.StartupPath);
            LightInfoPanelMargin = new Point(Convert.ToDouble(stpInformationDisplay.GetValue(Canvas.LeftProperty)), Convert.ToDouble(stpInformationDisplay.GetValue(Canvas.TopProperty)));

            OnStartRealFireAlarmLink = StartRealFireAlarmLink;//真实联动
            OnStartRemoveIconSearchCode = RemoveIconSearchCode;
            OnStartSingleLightControl = SingleLightControl;
            OnStartEmergencyOrMainEleByEPS = EmergencyOrMainEleByEPS;
            OnStartRemovePartitionPoint = RemovePartitionPoint;
            OnstartUpdateFacPlanInLayer = UpdateFacPlanInLayer;
            OnStartLampRelationLines = LampRelationLines;
            OnstartDeviceIconRotate = StartDeviceIconRotate;
            OnSartShowSelectEscapeLine = ShowSelectEscapeLine;
            OnStartRemoveSelectedEscapeLine = RemoveSelectedEscapeLine;

            TransformGroupNoLogin = gdFloorDrawingNoLogin.FindResource("tfgFloorDrawingNoLogin") as TransformGroup;
            TransformGroupLogin = gdFloorDrawingLogin.FindResource("tfgFloorDrawingLogin") as TransformGroup;
            ScaleTransformNoLogin = TransformGroupNoLogin.Children[0] as ScaleTransform;
            ScaleTransformLogin = TransformGroupLogin.Children[0] as ScaleTransform;
            TranslateTransformNoLogin = TransformGroupNoLogin.Children[1] as TranslateTransform;
            TranslateTransformLogin = TransformGroupLogin.Children[1] as TranslateTransform;

            FloorDrawingCenterLogin = new Point(445, 470);//575,385
            FloorDrawingCenterNoLogin = new Point(582, 260);
            StartPositionDragFloor = new Point(210, 132);//175,185   235,195
            StartPositionDragFloorNoLogin = new Point(140, 32);

            FloorDrawingLeft.TransformX1 = 223;//210;
            FloorDrawingLeft.TransformY1 = 135;//132;
            FloorDrawingLeft.TransformX2 = 223;//210;
            FloorDrawingLeft.TransformY2 = 584;//582;
            FloorDrawingRight.TransformX1 = 960;//950;
            FloorDrawingRight.TransformY1 = 135;//132;
            FloorDrawingRight.TransformX2 = 960;//950;
            FloorDrawingRight.TransformY2 = 584;//582;
            FloorDrawingTop.TransformX1 = 223;//210;
            FloorDrawingTop.TransformY1 = 135;// 132;
            FloorDrawingTop.TransformX2 = 960;// 950;
            FloorDrawingTop.TransformY2 = 135;// 132;
            FloorDrawingBottom.TransformX1 = 223;// 210;
            FloorDrawingBottom.TransformY1 = 584;// 582;
            FloorDrawingBottom.TransformX2 = 960;// 950;
            FloorDrawingBottom.TransformY2 = 584;// 582;

            FloorDrawingPosition = new Point(imgFloorDrawingLogin.Width, imgFloorDrawingLogin.Height);
            FloorDrawingPositionNoLogin = new Point(imgFloorDrawingNoLogin.Width, imgFloorDrawingNoLogin.Height);
            IconSearchCodeInfoPanelAreaNoLogin = new Point(stpIconSearCodeInfoNoLogin.Width, stpIconSearCodeInfoNoLogin.Height);
            IconSearchCodeInfoPanelAreaLogin = new Point(stpIconSearCodeInfoLogin.Width, stpIconSearCodeInfoLogin.Height);
            PartitionPointInfoPanelAreaLogin = new Point(stpPartitionPointInfoLogin.Width, stpPartitionPointInfoLogin.Height);

            SliderControlNoLogin = new SliderControl();
            SliderControlLogin = new SliderControl();

            SliderControlNoLogin.sdFloorDrawing.Value = SliderControlLogin.sdFloorDrawing.Value = 1;
            SliderControlNoLogin.sdFloorDrawing.ValueChanged += SliderControlNoLogin_ValueChanged;
            SliderControlLogin.sdFloorDrawing.ValueChanged += SliderControlLogin_ValueChanged;

            //this.stpSliderControlNoLogin.Children.Add(SliderControlNoLogin);
            stpSliderControlLogin.Children.Add(SliderControlLogin);
        }

        /// <summary>
        /// 初始化图形界面当前楼层页面
        /// </summary>
        private void InitCurrengPageFloorNoLogin()
        {
            CurrentPageFloorNoLogin = 1;
            CurrentSelectFloorNoLogin = "1层";
        }

        /// <summary>
        /// 初始化图形界面
        /// </summary>
        private void InitLayerModeNoLogin()
        {
            if (!IsOpenLayerModeNoLogin)
            {
                OpenLayerModeNoLoginPage();
                SetIsOpenLayerModeNoLogin(true);
                ShowTotalFloorNoLogin();
            }
            if (IsRealFireAlarmLink)
            {
                SwitchFloorNoLogin();
            }
        }

        /// <summary>
        /// 更新EPS下灯具故障数目
        /// </summary>
        private void UpdateFaultLightCount()
        {
            foreach(DistributionBoxInfo distributionBox in LstDistributionBox)
            {
                int num = LstLight.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 && Convert.ToString(x.DisBoxID) == distributionBox.Code|| (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0 && Convert.ToString(x.DisBoxID) == distributionBox.Code || (x.Status & (int)EnumClass.LightFaultClass.电池故障) != 0 && Convert.ToString(x.DisBoxID) == distributionBox.Code).Count - LstLight.FindAll(x => ((x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.电池故障) != 0) && x.Shield == 1 && Convert.ToString(x.DisBoxID) == distributionBox.Code).Count;
                if (FaultLightCount.ContainsKey(distributionBox.Code))
                {
                    FaultLightCount[distributionBox.Code] = num;
                }
                else
                {
                    FaultLightCount.Add(distributionBox.Code, num);
                }
                
                
            }
            
        }


        /// <summary>
        /// 未登录时显示EPS简要信息
        /// </summary>
        private void InitEPSShowNoLogin()
        {
            EPSImageDisplay.Children.Clear();
            for (int i = (EPSImageDisplayCurrentPage - 1) * EPSImageDisplayMaxRowCount * EPSImageDisplayColumnCount; i < EPSImageDisplayCurrentPage * EPSImageDisplayMaxRowCount * EPSImageDisplayColumnCount; i++)
            {
                if (i >= LstDistributionBox.Count)
                {
                    break;
                }
                StackPanel stackpanel = GetEPSStackPanel(LstDistributionBox[i], i % (EPSImageDisplayMaxRowCount * EPSImageDisplayColumnCount));
                EPSImageDisplay.Children.Add(stackpanel);
            }
        }

        /// <summary>
        /// EPS巡检时改变EPS图标
        /// </summary>
        private void ChangeGivenEPSShow()
        {
                #region
                UpdateFaultLightCount();
                for (int i = 0; i < EPSImageDisplay.Children.Count; i++)
                {
                    StackPanel stack = EPSImageDisplay.Children[i] as StackPanel;
                    if (stack != null && (DistributionBoxInfo)stack.Tag == QueryEPS)
                    {
                        for (int h = 0; h < stack.Children.Count; h++)
                        {
                            Image image = stack.Children[h] as Image;
                            if (image != null)
                            {
                                image.Source = InspectGetEPSStateIamge(QueryEPS);
                            }
                            else
                            {
                                Label label = stack.Children[h] as Label;
                                if (label != null && label.Tag != null && label.Tag.ToString() == "状态")
                                {
                                    label.Content = InspectGainEPSStatus(QueryEPS);
                                }
                            }

                        }
                        break;
                    }
                }
                #endregion

        }

        /// <summary>
        /// 
        /// 灯具巡检时改变灯具的图标
        /// </summary>
        private void ChangeGivenLampShow(LightInfo infoLight)
        {
                #region
                for (int i = 0; i < LampByEPSNoLogin.Children.Count; i++)
                {
                    StackPanel stack = LampByEPSNoLogin.Children[i] as StackPanel;
                    if (stack != null && stack.Tag != null && (stack.Tag as LightInfo).Code == infoLight.Code && (stack.Tag as LightInfo).DisBoxID == infoLight.DisBoxID)
                    {
                        for (int h = 0; h < stack.Children.Count; h++)
                        {
                            Image image = stack.Children[h] as Image;
                            if (image != null)
                            {
                                LampNoLoginTag lampNoLoginTag = new LampNoLoginTag
                                {
                                    infoLight = infoLight,
                                    tabIndex = ((LampNoLoginTag)image.Tag).tabIndex
                                };
                                image.Tag = lampNoLoginTag;
                                ImageShowByLightClass(infoLight, image);
                            }
                        }
                        break;
                    }
                }
                #endregion
        }

        private void LampLastUpNoLogin(object sender, MouseButtonEventArgs e)
        {
            if (LampImageDisplayCurrentPage == 1)
            {
                LampImageDisplayCurrentPage = LampImageDisplayTotalPage;
            }
            else
            {
                LampImageDisplayCurrentPage--;
            }
            LampByEPSCurrentPage.Content = LampImageDisplayCurrentPage;
            InitLampShowNologin();
        }

        private void LampNextUpNoLogin(object sender, MouseButtonEventArgs e)
        {
            if (LampImageDisplayCurrentPage == LampImageDisplayTotalPage)
            {
                LampImageDisplayCurrentPage = 1;
            }
            else
            {
                LampImageDisplayCurrentPage++;
            }
            LampByEPSCurrentPage.Content = LampImageDisplayCurrentPage;
            InitLampShowNologin();
        }

        private void InitLampShowNologin()
        {
            LampByEPSNoLogin.Children.Clear();
            for (int i = (LampImageDisplayCurrentPage - 1) * 4 * 4; i < LampImageDisplayCurrentPage * 4 * 4; i++)
            {
                if (i >= LstLightViewByDisBoxIDNoLogin.Count)
                {
                    break;
                }
                StackPanel stackpanel = GetLampStackPanel(LstLightViewByDisBoxIDNoLogin[i], i % (4 * 4));
                LampByEPSNoLogin.Children.Add(stackpanel);
            }
        }

        /// <summary>
        /// 初始化图形界面
        /// </summary>
        private void InitLayerModeLogin()
        {
            if (!IsOpenLayerModeLogin)
            {
                ForceSaveIconSearchCodeLogin();
                SetIsEditAllIcon(false);
                SetIsOpenLayerModeLogin(true);
                OpenLayerModeLoginPage();
                ShowTotalFloorLogin();
                InitIconSearchCodeListLogin();
            }
            SwitchFloorLogin();
        }

        /// <summary>
        /// 显示图形界面楼层数
        /// </summary>
        private void ShowTotalFloorNoLogin()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "ConstructionFloor");
            TotalFloor = Convert.ToInt32(infoGblSetting.SetValue);

            FloorPage.Content = CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1);
            FloorTotalNoLogin.Content = TotalFloor.ToString();
            tbxJumpSelectFloorNoLogin.Clear();
        }

        /// <summary>
        /// 显示图形界面楼层数
        /// </summary>
        private void ShowTotalFloorLogin()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "ConstructionFloor");
            TotalFloor = Convert.ToInt32(infoGblSetting.SetValue);

            TotalFloorNum.Content = TotalFloor;
            tbxJumpSelectFloorLogin.Clear();
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            Task.Run(() =>
            {
                //更新时间显示
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (!IsSimulationLinkage && !IsRefreshProgressBar)
                        {
                            ShowDateTime();
                            
                        }
                        await Task.Delay(500);
                    }
                }));
            });
            Task.Run(() =>
            {
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsAutomaticBackups").SetValue))
                        {
                            AutomaticBackups();
                        }
                        await Task.Delay(1000);
                    }
                }));
            });
            Task.Run(() =>
            {
                //释放资源
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(5000);
                        GC.Collect();
                    }
                }));
            });

            //_ = Dispatcher.BeginInvoke(new Action(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(1000);
            //        //Protocol.Heartbeat();
            //    }
            //}));

            //界面刷新
            Task.Run(() =>
            {
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(5000);
                        if (IsQueryEPSAndLight || IsQueryLight)
                        {
                            if (!IsSimulationLinkage && !IsRefreshProgressBar)
                            {
                                //await QueryEPSAndLightTimer_Tick(null, null);
                                if (IsOpenLayerModeLogin || IsOpenLayerModeNoLogin)
                                {
                                    RefreshAllIcon();
                                }
                                else
                                {
                                    RefreshMainView();
                                }
                                ChangeGivenEPSShow();
                                ShowEPSListLogin();
                                ModifyHostState();
                                ShowHistoryEventRecordLog();

                              
                            }
                        }

                        if (EPSDetailPageNoLogin.Visibility == System.Windows.Visibility.Visible && QueryEPS.Code != null)
                        {
                            //QueryEPSDetailPageNoLogin(LstDistributionBox[SelectEPSQueryCurrentIndex].Code);
                            if (IsQueryLight)
                            {
                                for (int i = 0; i < LstLightQueryByEPSID.Count; i++)
                                {
                                    ChangeGivenLampShow(LstLightQueryByEPSID[i]);
                                }
                            }
                          
                        }
                          
                      
                    }
                }));
            });


            //EPS数据巡检
            Task.Run(async() =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (IsQueryEPSAndLight)
                    {
                        if (!IsSimulationLinkage && !IsRefreshProgressBar)
                        {
                            //await QueryEPSAndLightTimer_Tick(null, null);
                            ClearStatErrorVar();
                            QueryEPSAndLightStatus();
                            RefreshEPSAndLightInfo();
                            //SwitchEPSQueryCurrentIndex();
                        }
                    }
                  
                    //Thread.Sleep(1000);
                }
            });




            //灯具数据巡检
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000);
                        if (IsQueryLight)
                        {
                            if (QueryEPS.Code != null)
                            {
                                if (LstFaultRecord.Find(x => x.Subject == QueryEPS.Code && x.ChildSubject == null && x.Fault == "配电箱掉线故障") != null)
                                {
                                    ComFailLights();
                                    LightStatusByEPSArray = null;
                                    IsQueryEPSAndLight = true;
                                  
                                }
                                else
                                {
                                    if (LightStatusByEPSArray != null)
                                    {
                                        QueryLights();
                                    }
                                    else
                                    {
                                        IsQueryEPSAndLight = true;
                                    }
                                }
                            }
                            else
                            {
                                IsQueryEPSAndLight = true;
                            }
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        LoggerManager.WriteError(ex.ToString());
                    }

                }
            });

            //主机板巡检
            Task.Run(() =>
            {
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsQueryEPSAndLight)
                        {
                            if (!IsSimulationLinkage && !IsRefreshProgressBar)
                            {
                                HostBoardCom();
                            }
                        }
                        await Task.Delay(2000);
                    }
                }));
            });

            //物理按钮检测状态
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (!IsSimulationLinkage && !IsRefreshProgressBar)
                        {
                            //AbsFireAlarmLink.SendHostBoardData(AbsFireAlarmLink.HostBoardSendStatus);
                            DealHostBoardReceiveData();
                        }
                        await Task.Delay(500);
                    }
                }));
            });

            //更新模拟联动时间
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsLinkageTiming)//IsSimulationLinkage)
                        {
                            CalcuSimulateFireAlarmLinkTime();
                        }
                        await Task.Delay(1000);
                    }
                }));
            });
            Task.Run(() =>
            {
                //真实联动计时
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsRealFireAlarmLink)
                        {
                            CalCuRealLinkFireAlarmLinkTime();
                        }
                        await Task.Delay(1000);
                    }
                }));
            });

            //固定时间内刷新图层界面
            Task.Run(() =>
            {
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsEditAllIcon)
                        {
                            RefreshAllIcon();
                        }
                        await Task.Delay(1000);
                    }
                }));
            });

            //刷新自检进度
            Task.Run(() =>
            {
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsOpenEmTime)
                        {
                            RefreshMonthCheckProcess_Tick(null, null);
                        }
                        await Task.Delay(1000);
                    }
                }));
            });
            Task.Run(() =>
            {
                //刷新自检主电计时
                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    while (true)
                    {
                        if (IsOpenMainTime)
                        {
                            MainPowerTiming_Tick(null, null);
                        }
                        await Task.Delay(1000);
                    }
                }));
            });

            //模拟联动每四秒发送一次联动指令
            SimulateFireAlarmLinkExePlanTimer = new System.Windows.Forms.Timer
            {
                Interval = SimulateFireAlarmLinkExePlanTimerInterval
            };
            SimulateFireAlarmLinkExePlanTimer.Tick += SimulateFireAlarmLinkExePlanTimer_Tick;
            SimulateFireAlarmLinkExePlanTimer.Enabled = false;


            //月检时间显示
            ExecuteMonthCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = CommonTimerInterval
            };
            ExecuteMonthCheckTimer.Tick += ExecuteMonthCheckTimer_Tick;
            ExecuteMonthCheckTimer.Enabled = false;

            //年检时间显示
            ExecuteSeasonCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = CommonTimerInterval
            };
            ExecuteSeasonCheckTimer.Tick += ExecuteSeasonCheckTimer_Tick;
            ExecuteSeasonCheckTimer.Enabled = false;

            MonthOrSeasonCheckFault = new System.Windows.Forms.Timer
            {
                Interval = 3000
            };
            MonthOrSeasonCheckFault.Tick += MonthOrSeasonCheckFault_Tick;
            MonthOrSeasonCheckFault.Enabled = false;

            AllEmergencyTotalTimer = new System.Windows.Forms.Timer
            {
                Interval = CommonTimerInterval
            };
            AllEmergencyTotalTimer.Tick += AllEmergencyTotalTimer_Tick;
            AllEmergencyTotalTimer.Enabled = false;

            NoMainEmergencyTime = new System.Windows.Forms.Timer
            {
                Interval = CommonTimerInterval
            };
            NoMainEmergencyTime.Tick += NoMainEmergencyTime_Tick;
            NoMainEmergencyTime.Enabled = false;

            AllMainEleTimer = new System.Windows.Forms.Timer
            {
                Interval = CommonTimerInterval
            };
            AllMainEleTimer.Tick += AllMainEleTimer_Tick;
            AllMainEleTimer.Enabled = true;

            RefreshProgressBarValueTimer = new System.Windows.Forms.Timer
            {
                Interval = TimerInterval,
                //RefreshProgressBarValueTimer.Tick += RefreshProgressBarValueTimer_Tick;
                Enabled = true
            };

            SearchLampProgressBarValueTimer = new System.Windows.Forms.Timer
            {
                Interval = 4800
            };
            SearchLampProgressBarValueTimer.Tick += SearchLampProgressBarValueTimer_Tick;
            SearchLampProgressBarValueTimer.Enabled = false;

            //await QueryEps();
        }

        private void RefreshMonthCheckProcess_Tick(object sender, EventArgs e)
        {
            if (LstFaultRecord.Count != 0 && LstFaultRecord[LstFaultRecord.Count - 1] != LatelyFaultRecord)//出现新故障
            {
                FaultCount++;
                if (FaultCount >= 10)
                {
                    FaultCount = 0;
                    TotalExecuteSecond = 0;
                    if (!IsKeyAccelerateCheck)
                    {
                        SelfCheckingSecond.Content = SelfCheckingMinute.Content = SelfCheckingHour.Content = 0;
                        SelfCheckingFrequency.Visibility = SelfCheckingFrequencyUnit.Visibility = SelfCheckPrompt.Visibility = System.Windows.Visibility.Hidden;
                        SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = SelfCheckingFrequency.Foreground = SelfCheckingFrequencyUnit.Foreground = CommonFunct.GetBrush("#8CB3D9");
                        SelfCheckingResetSystem.Value = 0;
                    }
                    else
                    {
                        KeyCheckNum.Content = 1;
                        MonthlyORYearly.Content = "月检";
                        CheckType.Content = "应急时间";
                        KeyCheckingHour.Content = KeyCheckingMinute.Content = KeyCheckingSecond.Content = 0;
                        MonthlyAndYearlyCheckKey.Visibility = System.Windows.Visibility.Hidden;

                    }
                    IsOpenEmTime = false;
                    IsOpenMainTime = false;
                    CommonFunct.PopupWindow("出现新故障，自检失败！");
                    SetAllMainEleTimer(true);
                    AllMainEle();
                    SetCheckSelfTestFeedBackPage(true);
                    SystemSelfCheck();
                    IsKeyAccelerateCheck = false;
                    IsAccelerateDetection = false;
                    IsSystemMonthDetection = false;
                    IsSystemSeasonDetection = false;
                }
                
            }
            else
            {
                FaultCount = 0;
                if (!IsKeyAccelerateCheck)
                {
                    TotalExecuteSecond++;
                    SelfCheckingMinute.Content = TotalExecuteSecond / TimeInterval;
                    SelfCheckingSecond.Content = TotalExecuteSecond % TimeInterval;

                    if (int.Parse(SelfCheckingSecond.Content.ToString()) + int.Parse(SelfCheckingMinute.Content.ToString()) * 60 == SelfCheckingTotalTime)
                    {
                        TotalExecuteSecond = 0;
                        SelfCheckingSecond.Content = SelfCheckingMinute.Content = SelfCheckingHour.Content = 0;
                        SetAllMainEleTimer(true);
                        AllMainEle();
                        IsOpenEmTime = false;

                        if (MonthAndSeasonCheckCurrentTime == 0 || MonthAndSeasonCheckTime == MonthAndSeasonCheckCurrentTime || !IsAccelerateDetection)
                        {
                            SelfCheckingFrequency.Visibility = SelfCheckingFrequencyUnit.Visibility = SelfCheckPrompt.Visibility = System.Windows.Visibility.Hidden;
                            SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = SelfCheckingFrequency.Foreground = SelfCheckingFrequencyUnit.Foreground = CommonFunct.GetBrush("#8CB3D9");

                            SelfCheckingResetShow.Visibility = System.Windows.Visibility.Visible;
                            for (int i = 1; i <= SelfCheckingResetSystem.Maximum; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                Thread.Sleep(50);
                                SelfCheckingResetSystem.Value = i;
                            }
                            if (SelfCheckingResetSystem.Value == SelfCheckingResetSystem.Maximum)
                            {
                                SelfCheckingResetSystem.Value = 0;
                                SelfCheckingResetShow.Visibility = System.Windows.Visibility.Hidden;
                            }
                            SetCheckSelfTestFeedBackPage(true);
                            SystemSelfCheck();
                        }
                        else
                        {
                            IsSystemMPDetection = true;
                            IsOpenMainTime = true;
                            while (IsOpenMainTime)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
                else
                {
                    TotalExecuteSecond++;
                    KeyCheckingMinute.Content = TotalExecuteSecond / TimeInterval;
                    KeyCheckingSecond.Content = TotalExecuteSecond % TimeInterval;

                    if (int.Parse(KeyCheckingSecond.Content.ToString()) + int.Parse(KeyCheckingMinute.Content.ToString()) * 60 == SelfCheckingTotalTime)
                    {
                        TotalExecuteSecond = 0;
                        KeyCheckingSecond.Content = KeyCheckingMinute.Content = KeyCheckingHour.Content = 0;
                        SetAllMainEleTimer(true);
                        AllMainEle();
                        IsOpenEmTime = false;

                        if (MonthAndSeasonCheckTime == 0 || MonthAndSeasonCheckTime == MonthAndSeasonCheckCurrentTime || !IsAccelerateDetection)
                        {
                            KeyCheckNum.Content = 1;
                            MonthlyORYearly.Content = "月检";
                            CheckType.Content = "应急时间";
                            KeyCheckingHour.Content = KeyCheckingMinute.Content = KeyCheckingSecond.Content = 0;
                            MonthlyAndYearlyCheckKey.Visibility = System.Windows.Visibility.Hidden;
                            SetCheckSelfTestFeedBackPage(true);
                            SystemSelfCheck();
                        }
                        else
                        {
                            IsSystemMPDetection = true;
                            IsOpenMainTime = true;
                            while (IsOpenMainTime)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
            }
        }

        private void MainPowerTiming_Tick(object sender, EventArgs e)
        {
            if (LstFaultRecord.Count != 0 && LstFaultRecord[LstFaultRecord.Count - 1] != LatelyFaultRecord)//出现新故障
            {
                if(FaultCount >= 10)
                {
                    FaultCount = 0;
                    TotalExecuteSecond = 0;
                    if (!IsKeyAccelerateCheck)
                    {
                        SelfCheckingSecond.Content = SelfCheckingMinute.Content = SelfCheckingHour.Content = 0;
                        SelfCheckingFrequency.Visibility = SelfCheckingFrequencyUnit.Visibility = SelfCheckPrompt.Visibility = System.Windows.Visibility.Hidden;
                        SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = SelfCheckingFrequency.Foreground = SelfCheckingFrequencyUnit.Foreground = CommonFunct.GetBrush("#8CB3D9");
                        SelfCheckingResetSystem.Value = 0;
                    }
                    else
                    {
                        KeyCheckNum.Content = 1;
                        MonthlyORYearly.Content = "月检";
                        CheckType.Content = "应急时间";
                        KeyCheckingHour.Content = KeyCheckingMinute.Content = KeyCheckingSecond.Content = 0;
                        MonthlyAndYearlyCheckKey.Visibility = System.Windows.Visibility.Hidden;

                    }
                    IsOpenEmTime = false;
                    IsOpenMainTime = false;
                    CommonFunct.PopupWindow("出现新故障，自检失败！");
                    SetAllMainEleTimer(true);
                    AllMainEle();
                    SetCheckSelfTestFeedBackPage(true);
                    SystemSelfCheck();
                    IsAccelerateDetection = false;
                }
                FaultCount++;

            }
            else
            {
                FaultCount = 0;
                if (!IsKeyAccelerateCheck)
                {
                    IsComEmergency = false;
                    SelfCheckingTime.Content = "主电计时";
                    TotalExecuteSecond++;
                    SelfCheckingMinute.Content = TotalExecuteSecond / TimeInterval;
                    SelfCheckingSecond.Content = TotalExecuteSecond % TimeInterval;

                    if (int.Parse(SelfCheckingSecond.Content.ToString()) + int.Parse(SelfCheckingMinute.Content.ToString()) * 60 == 10)
                    {
                        TotalExecuteSecond = 0;
                        SelfCheckingSecond.Content = SelfCheckingMinute.Content = SelfCheckingHour.Content = 0;
                        SelfCheckingTime.Content = "应急计时";
                        IsOpenMainTime = false;
                        
                    }
                }
                else
                {
                    IsComEmergency = false;
                    CheckType.Content = "主电计时";
                    TotalExecuteSecond++;
                    KeyCheckingSecond.Content = TotalExecuteSecond % TimeInterval;
                    KeyCheckingMinute.Content = TotalExecuteSecond / TimeInterval;

                    if (int.Parse(KeyCheckingSecond.Content.ToString()) + int.Parse(KeyCheckingMinute.Content.ToString()) * 60 == 10)
                    {
                        TotalExecuteSecond = 0;
                        KeyCheckingHour.Content = KeyCheckingMinute.Content = KeyCheckingSecond.Content = 0;
                        CheckType.Content = "应急计时";
                        IsOpenMainTime = false;
                        
                    }
                }
            }
        }

        private void SearchLampProgressBarValueTimer_Tick(object sender, EventArgs e)
        {
            SearchingLightCount++;
            pgbSearchLamp.Value = SearchingLightCount;
        }

        /// <summary>
        /// 加载基础数据
        /// </summary>
        private void InitBasicData()
        {
            VersionNum.Content = "V" + new Version(System.Windows.Forms.Application.ProductVersion).ToString();
            IsRemoteConnect = Convert.ToBoolean(ConfigurationManager.AppSettings["IsRemoteConnect"]);
            AbsFireAlarmLink.HostBoardSendStatus = 0X00;
            BatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryLevel3.png", UriKind.Relative));
            HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryLevel3.png", UriKind.Relative));
            LstDistributionBox = ObjDistributionBox.GetAll();
            LstLight = ObjLight.GetAll();
            LstCoordinate = ObjCoordinate.GetAll();
            LstGblSetting = ObjGblSetting.GetAll();
            LstOtherFaultRecord = ObjOtherFaultRecord.GetAll();
            LstFireAlarmType = ObjFireAlarmType.GetAll();
            LstPlanPartitionPointRecord = ObjPlanPartitionPointRecord.GetAll();
            ObjFaultRecord.DeleteAll();
            LstFaultRecord = ObjFaultRecord.GetAll();
            LstHistoricalEvent = ObjHistoricalEvent.GetAll();
            LstFloorName = ObjFloorName.GetAll();
            LstDistributionBox.Sort();
            LstLight.Sort();
            InitialEPSandLamp();
            //InitializationHistory();
            ShowHistoryEventRecordLog();
            AddHistoricalEvent("系统开机");
            DeleteFaultData();

            if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsMonthlyInspection").SetValue))
            {
                ExecuteMonthCheckTimer.Enabled = true;
            }
            else
            {
                ExecuteMonthCheckTimer.Enabled = false;
            }

            if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection").SetValue))
            {
                ExecuteSeasonCheckTimer.Enabled = true;
            }
            else
            {
                ExecuteSeasonCheckTimer.Enabled = false;
            }

            for (int i = 0; i < LstOtherFaultRecord.Count; i++)
            {
                LstOtherFaultRecord[i].IsExist = 0;
            }
            EPSImageDisplayCurrentPage = 1;
            EPSListCurrentPageLogin = 1;
            EPSImageDisplayTotalPage = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (EPSImageDisplayColumnCount * EPSImageDisplayMaxRowCount) + 1 : 1;
            LampTotalNumber.Content = LstLight.Count;
            EPSTotalNumber.Content = LstDistributionBox.Count;
            EPSShowCurrentPage.Content = EPSImageDisplayCurrentPage;
            EPSShowTotalPage.Content = EPSImageDisplayTotalPage;
        }

        /// <summary>
        /// 初次启动程序根据原始数据添加显示历史记录
        /// </summary>
        private void InitializationHistory()
        {
            for (int i = 0; i < LstDistributionBox.Count; i++)
            {
                string faultInfo = string.Empty;
                //AddHistoricalEvent(LstDistributionBox[i].Code + GetEPSStatus(LstDistributionBox[i].Status), LstDistributionBox[i].ErrorTime);
                //事件记录加具体eps位置
                if (LstDistributionBox[i].Address != "安装位置未初始化")
                {
                    AddHistoricalEvent(LstDistributionBox[i].Address + LstDistributionBox[i].Code + GetEPSStatus(LstDistributionBox[i].Status), LstDistributionBox[i].ErrorTime);
                    faultInfo = string.Format("{0}{1}{2}", LstDistributionBox[i].Address, LstDistributionBox[i].Code, GetEPSStatus(LstDistributionBox[i].Status));
                }
                else
                {
                    AddHistoricalEvent(LstDistributionBox[i].Code + GetEPSStatus(LstDistributionBox[i].Status), LstDistributionBox[i].ErrorTime);
                    faultInfo = string.Format("{0}{1}", LstDistributionBox[i].Code, GetEPSStatus(LstDistributionBox[i].Status));
                }
                //打印Eps故障信息
                //string faultInfo = string.Format("{0}{1}", LstDistributionBox[i].Code, GetEPSStatus(LstDistributionBox[i].Status));

                FaultPrint(1, faultInfo);
            }
            for (int i = 0; i < LstLight.Count; i++)
            {
                DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == LstLight[i].DisBoxID.ToString());
                if (LstLight[i].Status == (int)EnumClass.LightFaultClass.光源故障)
                {
                    string faultInfo = string.Empty;
                    //AddHistoricalEvent(string.Format("{0}的{1}光源故障", LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                    //事件记录加具体eps位置
                    if (infoDistributionBox.Address != "安装位置未初始化")
                    {
                        AddHistoricalEvent(string.Format("{0}{1}的{2}光源故障", infoDistributionBox.Address, LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                        faultInfo = string.Format("{0}{1}的{2}光源故障", infoDistributionBox.Address, LstLight[i].DisBoxID, LstLight[i].Code);
                    }
                    else
                    {
                        AddHistoricalEvent(string.Format("{0}的{1}光源故障", LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                        faultInfo = string.Format("{0}的{1}光源故障", LstLight[i].DisBoxID, LstLight[i].Code);
                    }
                    //string faultInfo = string.Format("{0}的{1}光源故障", LstLight[i].DisBoxID, LstLight[i].Code);
                    //打印灯具故障信息
                    FaultPrint(2, faultInfo);
                }
                if (LstLight[i].Status == (int)EnumClass.LightFaultClass.通信故障)
                {
                    string faultInfo = string.Empty;
                    //AddHistoricalEvent(string.Format("{0}的{1}通讯故障", LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                    //事件记录加具体eps位置
                    if (infoDistributionBox.Address != "安装位置未初始化")
                    {
                        AddHistoricalEvent(string.Format("{0}{1}的{2}通讯故障", infoDistributionBox.Address, LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                        faultInfo = string.Format("{0}{1}的{2}通讯故障", infoDistributionBox.Address, LstLight[i].DisBoxID, LstLight[i].Code);
                    }
                    else
                    {
                        AddHistoricalEvent(string.Format("{0}的{1}通讯故障", LstLight[i].DisBoxID, LstLight[i].Code), LstLight[i].ErrorTime);
                        faultInfo = string.Format("{0}的{1}通讯故障", LstLight[i].DisBoxID, LstLight[i].Code);
                    }
                    //string faultInfo = string.Format("{0}的{1}通讯故障", LstLight[i].DisBoxID, LstLight[i].Code);
                    //打印灯具故障信息
                    FaultPrint(2, faultInfo);
                }
            }
        }

        /// <summary>
        /// 初始化设备应急状态
        /// </summary>
        private void InitialEPSandLamp()
        {
            LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            ObjDistributionBox.Save(LstDistributionBox);

            LstLight.ForEach(x => x.IsEmergency = 0);
            ObjLight.Save(LstLight);
        }

        /// <summary>
        /// 将历史记录展示在主页上
        /// </summary>
        private void ShowHistoryEventRecordLog()
        {
            EventLog.Items.Clear();
            int startIndex = Math.Max(0, LstHistoricalEvent.Count - 21);
            int endIndex = LstHistoricalEvent.Count;

            for (int i = startIndex; i < endIndex; i++)
            {
                EventLog.Items.Insert(0, string.Format(LstHistoricalEvent[i].EventTime + " " + LstHistoricalEvent[i].EventContent));
            }
        }


        /// <summary>
        /// 启动程序时清除所有故障，重新巡检
        /// </summary>
        private void DeleteFaultData()
        {
            ObjFaultRecord.DeleteAll();

            LstDistributionBox.ForEach(x => x.Status = 0);
            LstDistributionBox.ForEach(x => x.ErrorTime = string.Empty);
            ObjDistributionBox.Save(LstDistributionBox);



            for (int i = 0; i < LstLight.Count; i++)
            {
                if (LstLight[i].Status == (int)EnumClass.LightFaultClass.光源故障 || LstLight[i].Status == (int)EnumClass.LightFaultClass.通信故障)
                {
                    LstLight[i].Status = 0;
                    LstLight[i].ErrorTime = string.Empty;
                }
            }
            ObjLight.Save(LstLight);
        }

        /// <summary>
        /// 初始化分区设置
        /// </summary>
        private void InitFireAlarmPartitionSet()
        {
            LstFireAlarmPartitionSet = ObjFireAlarmPartitionSet.GetAll();
        }

        /// <summary>
        /// 初始化所有防火分区
        /// </summary>
        private void InitFireAlarmLinkZone()
        {
            for (int i = 1; i <= FireAlarmZoneCount; i++)
            {
                FireAlarmZoneInfo infoFireAlarmLinkZone = new FireAlarmZoneInfo
                {
                    FireAlarmLinkZoneNumber = i,
                };
                LstFireAlarmZone.Add(infoFireAlarmLinkZone);
            }
        }

        /// <summary>
        /// 初始化Gif动画
        /// </summary>
        private void InitGifFile()
        {
            GifFilePath = string.Format("{0}\\GifFilePath\\FireAlarmLink.gif", System.Windows.Forms.Application.StartupPath);

            (winSimulateLinkPicHost.Child as System.Windows.Forms.PictureBox).Image
             = CommonFunct.ConvertGifFileToImage(GifFilePath);
        }

        /// <summary>
        /// 初始化所有串口
        /// </summary>
        private void InitAllSerialPort()
        {
            try
            {
                IsTimingQueryEPSOrLight = false;
                while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeCommonInstructSleepTime);
                    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                }
                CurrentExeInstructTime = 0;

                Protocol.InitEPSSeialPort(LstGblSetting);
                AbsFireAlarmLink.InitHostBoardAndFireAlarmLinkSerialPort(LstGblSetting, LstFireAlarmType, LstFireAlarmPartitionSet, this);
                //FireAlarmLinkInterface.InitFireAlarmSerialPort();
                InitFireAlarm();

                IsTimingQueryEPSOrLight = true;
            }
            catch (Exception ex)
            {
                LoggerManager.WriteFatal(ex.Message);
            }
        }

        /// <summary>
        /// 初始化火灾报警器串口
        /// </summary>
        private void InitFireAlarm()
        {
            _fireAlarmType = new CFireAlarmType().GetAll().Find(it => it.IsCurrentFireAlarm == 1).FireAlarmCode;
            List<GblSettingInfo> settings = ObjGblSetting.GetAll();
            string fireAlarmPort = settings?.Find(it => it.Key == "FireAlarmPort")?.SetValue;
            string fireAlarmBaudRateStr = settings?.Find(it => it.Key == "FireAlarmBaudRate")?.SetValue;
            bool cvtFireAlarmBuadRateStatus = int.TryParse(fireAlarmBaudRateStr, out int fireAlarmBaudRate);
            if (string.IsNullOrEmpty(fireAlarmPort) || !cvtFireAlarmBuadRateStatus)
            {
                return;
            }

            if (_fireAlarmType == "GST5000H")
            {
                FireAlarmLinkInterface.CloseFireAlarmSerialPort();
                _fireAlarm = GST5000H.Instance;
                _fireAlarm.Enable(fireAlarmPort, fireAlarmBaudRate,
                    queryPeriod: 1000, readDelayXms: 200, writeDelayXms: 50,
                    writeTimeout: 1000, readTimeout: 1000, isDropOldMessage: true);
            }
            else if (_fireAlarmType == "Tanda")
            {
                FireAlarmLinkInterface.CloseFireAlarmSerialPort();
                if (_fireAlarm != null)
                {
                    _fireAlarm.Disable();
                }
                else
                {
                    _fireAlarm = Tanda.Instance;
                }
                _fireAlarm.Enable(fireAlarmPort, fireAlarmBaudRate,
                    queryPeriod: 1000, readDelayXms: 200, writeDelayXms: 50,
                    writeTimeout: 1000, readTimeout: 1000, isDropOldMessage: true);
            }
            else
            {
                if (_fireAlarm != null)
                {
                    _fireAlarm.Disable();
                }
                FireAlarmLinkInterface.InitFireAlarmSerialPort();
            }
        }

        /// <summary>
        /// 定时巡检EPS和灯具
        /// </summary>
        private async Task InspectEPSAndLightTimer()
        {
            //try
            //{
            //    await QueryEPSAndLightStatus();
            //    if(IsOpenLayerModeLogin || IsOpenLayerModeNoLogin)
            //    {
            //        RefreshAllIcon();
            //    }
            //    else
            //    {
            //        RefreshMainView();
            //    }
                
                
            //    //InitEPSShowNoLogin();
            //    ChangeGivenEPSShow();
            //    ShowEPSListLogin();
            //    RefreshEPSAndLightInfo();
            //    SwitchEPSQueryCurrentIndex();
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.ToString());
            //    LoggerManager.WriteError(ex.ToString());
            //}
        }

        /// <summary>
        ///// TODO 定时查询EPS和灯具状态
        /// </summary>
        private void QueryEPSAndLightStatus()
        {
            try
            {
                #region
                if (IsTimingQueryEPSOrLight && !IsDragIconSearchCode)
                {
                    IsFinishQueryEPSOrLight = true;
                    IsQueryEPSAndLight = false;
                    #region
                    if (LstDistributionBox.Count > 0)
                    {
                        if (EPSQueryCurrentIndex >= LstDistributionBox.Count)
                        {
                            EPSQueryCurrentIndex = 0;
                            IsFinishQueryEPSOrLight = true;
                            IsQueryEPSAndLight = true;
                            return;
                        }
                        DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo();
                        if (SelectInfoEPSNoLogin.ID != 0 || SelectInfoEPSLogin.ID != 0 || SelectInfoLightLogin.ID != 0)
                        {
                            infoDistributionBox = LstDistributionBox[SelectEPSQueryCurrentIndex];
                            EPSQueryCurrentIndex--;
                            if(EPSQueryCurrentIndex < 0)
                            {
                                EPSQueryCurrentIndex = 0;
                            }
                        }
                        else
                        {
                            if (EPSQueryCurrentIndex < 0)
                            {
                                EPSQueryCurrentIndex = 0;
                            }
                            infoDistributionBox = LstDistributionBox[EPSQueryCurrentIndex];
                        }
                        QueryEPS = infoDistributionBox;

                        LstLightQueryByEPSID = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBox.Code));
                        AllMainEleTimer.Enabled = false;

                        if (infoDistributionBox.Shield == 0)
                        {
                            infoDistributionBox = GetEPSProperty(infoDistributionBox, GetEPSNews(infoDistributionBox));//通过通讯查询获取EPS信息
                        }

                        #region 巡检灯状态
                        LightStatusByEPSArray = null;
                        if (LstLightQueryByEPSID.Count != 0)
                        {
                            LightStatusByEPSArray = Protocol.QueryLightStatusByEPS(infoDistributionBox.Code);
                            if (LightStatusByEPSArray != null)
                            {
                                IsQueryEPSAndLight = false;
                            }
                        }
                        #endregion

                        ObjDistributionBox.Update(infoDistributionBox);
                        IsQueryEPSAndLight = false;
                    }
                    else
                    {
                        IsQueryEPSAndLight = true;
                    }
                    #endregion
                    IsFinishQueryEPSOrLight = false;
                }
                #endregion
                SwitchEPSQueryCurrentIndex();
            }
            catch(Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        /// <summary>
        /// 获取EPS信息
        /// </summary>
        /// <param name="infoDistributionBox">EPS实例</param>
        /// <returns></returns>
        private double[] GetEPSNews(DistributionBoxInfo infoDistributionBox)
        {
            //Task<double[]> task = Task.Run<double[]>(() =>
            //{
                double[] result = Protocol.GetEPSInfo(infoDistributionBox.Code).Result;
                if (result == null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        result = Protocol.GetEPSInfo(infoDistributionBox.Code).Result;
                        System.Windows.Forms.Application.DoEvents();//刷新界面
                        if (result != null)
                        {
                            break;
                        }
                    }
                }

                return result;
            //});
            //return task;
        }

        private DistributionBoxInfo GetEPSProperty(DistributionBoxInfo infoDistributionBox, double[] data)
        {
            if (data != null)
            {
                infoDistributionBox.ErrorTime = JudgeEPSNews(infoDistributionBox, data);
                infoDistributionBox.Test = GetEPSTest(data[4]);
                infoDistributionBox.AutoManual = GetEPSAutoManual(data[4]);
                infoDistributionBox.QiangQi = GetEPSQiangQi(data[4]);
                infoDistributionBox.Status = (int)data[4];
                infoDistributionBox.MainEleVoltage = data[0];
                infoDistributionBox.DischargeVoltage = data[2];
                infoDistributionBox.BatteryVoltage = data[1];
                infoDistributionBox.DischargeCurrent = data[3];
            }
            else
            {
                if (!IsRealFireAlarmLink)
                {
                    infoDistributionBox.Status = (int)EnumClass.DisBoxStatusClass.配电箱掉线故障;
                    infoDistributionBox.ErrorTime = JudgeEPSNews(infoDistributionBox, null);
                    infoDistributionBox.MainEleVoltage = infoDistributionBox.BatteryVoltage = infoDistributionBox.DischargeVoltage = infoDistributionBox.DischargeCurrent = infoDistributionBox.Test = infoDistributionBox.QiangQi = 0;
                    infoDistributionBox.AutoManual = 2;
                }
            }
            infoDistributionBox.IsEmergency = GetEM_IsEmergency(infoDistributionBox);
            return infoDistributionBox;
        }

        /// <summary>
        /// 判断EPS是否处于测试状态，是则返回1，否则返回0
        /// </summary>
        /// <param name="news">EPS实时状态</param>
        /// <returns></returns>
        private int GetEPSTest(double news)
        {
            return ((Convert.ToInt32(news) & 0XF000) & 0X1000) == 0X1000 ? 1 : 0;
        }

        /// <summary>
        /// 判断EPS当前状态，自动则返回0，手动则返回1，两者都不是则返回2
        /// </summary>
        /// <param name="news">EPS实时状态</param>
        /// <returns></returns>
        private int GetEPSAutoManual(double news)
        {
            return ((Convert.ToInt32(news) & 0XF000) & 0X2000) == 0X2000 ? 0 : ((Convert.ToInt32(news) & 0XF000) == 0 ? 2 : 1);
        }

        /// <summary>
        /// 判断EPS是否处于强启状态，是则返回1，否则返回0
        /// </summary>
        /// <param name="news">EPS实时状态</param>
        /// <returns></returns>
        private int GetEPSQiangQi(double news)
        {
            return ((Convert.ToInt32(news) & 0XF000) & 0X8000) == 0X8000 ? 1 : 0;
        }

        /// <summary>
        /// 保存灯具状态信息
        /// </summary>
        private void QueryLights()
        {
            //LightStatusByEPSArray EPS灯具状态字  LstLightQueryByEPSID EPS下的全部灯具
            if (IsQueryLight)
            {
                if (LstChangeStateLamp != null)
                {
                    LstChangeStateLamp.Clear();
                }

                for (int i = 0; i < LstLightQueryByEPSID.Count; i++)
                {
                    if (i > LightStatusByEPSArray.Length)
                    {
                        LstLightQueryByEPSID[i].Status = LstLightQueryByEPSID[i].BeginStatus;
                    }
                    else
                    {
                        LstLightQueryByEPSID[i].Status = LightStatusByEPSArray[i];
                        LstLightQueryByEPSID[i].ErrorTime = JudgeLampNews(LstLightQueryByEPSID[i]);
                    }

                    LstLightQueryByEPSID[i].IsEmergency = GetEM_IsEmergency(LstLightQueryByEPSID[i]);

                    int index = LstLight.FindIndex(x => x.ID == LstLightQueryByEPSID[i].ID);
                    LstLight[index] = LstLightQueryByEPSID[i];
                }
                ObjLight.Save(LstLightQueryByEPSID);
                UpdateFaultLightCount();
                GetEdition(IsCommodity);
                LstLightFault.Clear();
                LstLightFault.AddRange(LstLight.FindAll(x => x.ErrorTime != string.Empty));
                LightStatusByEPSArray = null;
                LstLightQueryByEPSID.Clear();//巡检完指定EPS下的灯具后删除当前集合的内容
                IsQueryEPSAndLight = true;
            }
        }

        /// <summary>
        /// 判断设备是否处于应急状态
        /// </summary>
        /// <param name="infoDisOrLamp">所判断的设备</param>
        /// <returns></returns>
        private int GetEM_IsEmergency(object infoDisOrLamp)
        {
            int result = 0;
            DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo();
            if (infoDisOrLamp is DistributionBoxInfo)
            {
                infoDistributionBox = infoDisOrLamp as DistributionBoxInfo;
            }
            else
            {
                infoDistributionBox = LstDistributionBox.Find(x => x.Code == (infoDisOrLamp as LightInfo).DisBoxID.ToString());
            }

            if (IsComEmergency||IsRealFireAlarmLink || IsSimulationLinkage || (AbsFireAlarmLink.HostBoardReturnStatus != 254 && ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.二百二十伏故障) != 0)) || ADDEPSStatus(infoDistributionBox.Status) == (int)EnumClass.DisBoxStatusClass.市电电压故障 || infoDistributionBox.Test == 1 || infoDistributionBox.AutoManual == 1 || infoDistributionBox.QiangQi == 1)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }
            return result;
        }

        /// <summary>
        /// 判断EPS状态是否改变，改变则记录，并且返回EPS的故障时间
        /// </summary>
        /// <param name="infoDistributionBox">EPS实例</param>
        /// <param name="status">实时状态</param>
        /// <returns></returns>
        private string JudgeEPSNews(DistributionBoxInfo infoDistributionBox, double[] status)
        {
            string result = infoDistributionBox.ErrorTime;
            if (status == null)
            {
                
                string key = infoDistributionBox.Code + "配电箱掉线故障";
                if (!FaultCord.ContainsKey(key) || FaultCord.Count == 0)
                {
                    FaultCord.Add(key, 0);
                    result = DateTime.Now.ToString();
                    SaveEPSNews(infoDistributionBox, "配电箱掉线故障", result);
                }
                else
                {
                    if (FaultCord[key] == 1)
                    {
                        FaultCord[key] = 0;
                        result = DateTime.Now.ToString();
                        SaveEPSNews(infoDistributionBox, "配电箱掉线故障", result);
                    }
                    
                }
            }
            else
            {
                //FaultRecordInfo infoFaultRecord = LstFaultRecord.Find(x => x.Subject == infoDistributionBox.Code && x.ChildSubject == null && x.Fault == "配电箱掉线故障");
                //if (infoFaultRecord != null)
                //{
                //    LstFaultRecord.Remove(infoFaultRecord);
                //    ObjFaultRecord.Delete(infoFaultRecord.ID);
                //}
                //if ((infoDistributionBox.Status & 0X07FC) != ADDEPSStatus(Convert.ToDouble(status[4].ToString())))
                //{

                // 使用 LINQ 查询筛选出具有包含特定片段的键，并进行统一赋值
                
                result = ADDEPSStatus(Convert.ToDouble(status[4].ToString())) == 0 ? string.Empty : DateTime.Now.ToString();
                    if (ADDEPSStatus(Convert.ToDouble(status[4].ToString())) == 0)
                    {
                        string key = infoDistributionBox.Code+"配电箱";
                        var faultToUpdate = FaultCord.Keys.Where(x => x.Contains(key)).ToList();
                        foreach (var fault in faultToUpdate)
                        {
                            
                             FaultCord[fault] = 1;
                            
                            
                        }

                        SaveEPSNews(infoDistributionBox, "恢复正常", result);
                        
                        
                    }
                    else
                    {
                        string key =  infoDistributionBox.Code + GetEPSStatus(ADDEPSStatus(Convert.ToDouble(status[4].ToString())));
                        if (!FaultCord.ContainsKey(key) || FaultCord.Count == 0)
                        {
                            FaultCord.Add(key, 0);
                            
                            SaveEPSNews(infoDistributionBox, GetEPSStatus(ADDEPSStatus(Convert.ToDouble(status[4].ToString()))), result);
                        }
                        else
                        {
                            if (FaultCord[key] == 1)
                            {
                                FaultCord[key] = 0;
                               
                                SaveEPSNews(infoDistributionBox, GetEPSStatus(ADDEPSStatus(Convert.ToDouble(status[4].ToString()))), result);
                            }

                        }

                        
                    }
                
            }

            return result;
        }

        /// <summary>
        /// 判断灯具状态是否改变，改变则记录
        /// </summary>
        /// <param name="infoLight">灯具实例</param>
        /// <returns></returns>
        private string JudgeLampNews(LightInfo infoLight)
        {
            string news = "";
            string NowTime = infoLight.ErrorTime;
            news = (infoLight.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 ? "通信故障" : (infoLight.Status & (int)EnumClass.LightFaultClass.光源故障) != 0 ? "光源故障" : (infoLight.Status & (int)EnumClass.LightFaultClass.电池故障) != 0 ? "电池故障" : "恢复正常";
            //FaultRecordInfo fault = LstFaultRecord.Find(x => x.Subject == infoLight.DisBoxID.ToString() && x.ChildSubject == infoLight.Code && x.Fault == news);
            if (news == "恢复正常")
            {
                if (infoLight.ErrorTime != string.Empty)
                {
                    NowTime = string.Empty;

                    string key = string.Format("{0}的{1}灯具", infoLight.DisBoxID, infoLight.Code);
                    var faultToUpdate = FaultCord.Keys.Where(x => x.Contains(key)).ToList();
                    foreach (var fault in faultToUpdate)
                    {

                        FaultCord[fault] = 1;


                    }
                    SaveLampNews(infoLight, "恢复正常", DateTime.Now.ToString());
                    

                }
                
            }
            if(news == "通信故障"|| news == "光源故障" || news == "电池故障")
            {
                string key = string.Format("{0}的{1}灯具" + news, infoLight.DisBoxID, infoLight.Code);


                if (!FaultCord.ContainsKey(key) || FaultCord.Count == 0)
                {
                    NowTime = DateTime.Now.ToString();
                    SaveLampNews(infoLight, news, NowTime);
                    FaultCord.Add(key, 0);
                }
                else
                {
                    if (FaultCord[key] == 1)
                    {
                        NowTime = DateTime.Now.ToString();
                        SaveLampNews(infoLight, news, NowTime);
                        FaultCord[key] = 0;
                    }
                }
                
            }

            return NowTime;

        }

        /// <summary>
        /// 记录EPS实时信息
        /// </summary>
        /// <param name="infoDistributionBox">EPS实例</param>
        /// <param name="news">实时信息</param>
        private void SaveEPSNews(DistributionBoxInfo infoDistributionBox, string news, string time)
        {
            if (news == "恢复正常")
            {
                if (LstFaultRecord.FindAll(x => x.Subject == infoDistributionBox.Code && x.ChildSubject == null).Count != 0)
                {
                    //AddHistoricalEvent(infoDistributionBox.Code + news, time);
                    //事件记录加具体eps位置
                    if (infoDistributionBox.Address != "安装位置未初始化")
                    {
                        AddHistoricalEvent(infoDistributionBox.Address + infoDistributionBox.Code + news, time);
                    }
                    else
                    {
                        AddHistoricalEvent(infoDistributionBox.Code + news, time);
                    }
                }
            }
            else
            {
                    string faultInfo = string.Empty;
                    //AddHistoricalEvent(infoDistributionBox.Code + news, time);
                    //事件记录加具体eps位置
                    if (infoDistributionBox.Address != "安装位置未初始化")
                    {
                        AddHistoricalEvent(infoDistributionBox.Address + infoDistributionBox.Code + news, time);
                        faultInfo = string.Format("{0}{1}{2}", infoDistributionBox.Address, infoDistributionBox.Code, news);
                    }
                    else
                    {
                        AddHistoricalEvent(infoDistributionBox.Code + news, time);
                        faultInfo = string.Format("{0}{1}", infoDistributionBox.Code, news);
                    }
                    //打印Eps故障信息
                    //FaultPrint(1, string.Format("{0}{1}", infoDistributionBox.Code, news));
                    FaultPrint(1, faultInfo);
                
            }
            RecordFault(infoDistributionBox.Code, null, news);
            GetEdition(IsCommodity);
        }

        /// <summary>
        /// 记录灯具实时信息
        /// </summary>
        /// <param name="infoLight">灯具实例</param>
        /// <param name="news">实时信息</param>
        private void SaveLampNews(LightInfo infoLight, string news, string time)
        {
            string faultInfo = string.Empty;
            
            //AddHistoricalEvent(string.Format("{0}的{1}灯具" + news, infoLight.DisBoxID, infoLight.Code), time);
            //事件记录加具体eps位置
            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString());
            if(infoLight.Shield == 0 && infoDistributionBox.Shield == 0)
            {
                if (infoDistributionBox.Address != "安装位置未初始化")
                {
                    AddHistoricalEvent(string.Format("{0}{1}的{2}灯具" + news, infoDistributionBox.Address, infoLight.DisBoxID, infoLight.Code), time);
                    faultInfo = string.Format("{0}{1}的{2}灯具{3}", infoDistributionBox.Address, infoLight.DisBoxID.ToString(), infoLight.Code, news);
                }
                else
                {
                    AddHistoricalEvent(string.Format("{0}的{1}灯具" + news, infoLight.DisBoxID, infoLight.Code), time);
                    faultInfo = string.Format("{0}的{1}灯具{2}", infoLight.DisBoxID.ToString(), infoLight.Code, news);
                }
            }
            
            //灯具故障打印
            //string faultInfo = string.Format("{0}的{1}灯具{2}", infoLight.DisBoxID.ToString(), infoLight.Code, news);
            if (news != "恢复正常" && infoLight.Shield == 0 && infoDistributionBox.Shield == 0)
            {
                FaultPrint(2, faultInfo);
            }
            RecordFault(infoLight.DisBoxID.ToString(), infoLight.Code, news);
        }

        /// <summary>
        /// EPS通讯故障的状态下灯具状态的转换
        /// </summary>
        private void ComFailLights()
        {
            const int batchSize = 200; // 每批处理的数量

            for (int startIndex = 0; startIndex < LstLightQueryByEPSID.Count; startIndex += batchSize)
            {
                int endIndex = Math.Min(startIndex + batchSize, LstLightQueryByEPSID.Count);

                // 批量处理灯具故障
                ProcessBatchFailLights(startIndex, endIndex);
                //await Task.Delay(30);
            }

            // 清空故障列表
            LstLightFault.Clear();
            LstLightFault.AddRange(LstLightQueryByEPSID.FindAll(x => x.ErrorTime != string.Empty));

            // 保存灯具信息
            ObjLight.Save(LstLight);
        }

        private void ProcessBatchFailLights(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                LightInfo infoLight = LstLight.Find(x => x.Code == LstLightQueryByEPSID[i].Code && x.DisBoxID == LstLightQueryByEPSID[i].DisBoxID);
                if (infoLight != null)
                {
                    LstLightQueryByEPSID[i].Status = infoLight.Status = (int)EnumClass.LightFaultClass.通信故障;
                    LstLightQueryByEPSID[i].ErrorTime = infoLight.ErrorTime = DateTime.Now.ToString();
                    LstLightQueryByEPSID[i].IsEmergency = infoLight.IsEmergency = 0;

                    string key = string.Format("{0}的{1}灯具" + "通信故障", infoLight.DisBoxID, infoLight.Code);
                    if (!FaultCord.ContainsKey(key) || FaultCord.Count == 0)
                    {
                        SaveLampNews(infoLight, "通信故障", infoLight.ErrorTime);
                        FaultCord.Add(key, 0);
                    }
                    else
                    {
                        if (FaultCord[key] == 1)
                        {
                            SaveLampNews(infoLight, "通信故障", infoLight.ErrorTime);
                            FaultCord[key] = 0;
                        }
                    }

                    //await QueryLightByEPS();

                    int index = LstLight.FindIndex(x => x.ID == LstLightQueryByEPSID[i].ID);
                    LstLight[index] = LstLightQueryByEPSID[i];
                }
            }
        }


        /// <summary>
        /// 巡检保存eps当前状态
        /// </summary>
        /// <param name="EPSReturnStatus"></param>
        /// <returns></returns>
        private int ADDEPSStatus(double EPSReturnStatus)
        {
            int ConversionValue = Convert.ToInt32(EPSReturnStatus) & 0X07FC;
            if ((ConversionValue & 0X07FC) == 0X07FC)
            {
                return (int)EnumClass.DisBoxStatusClass.配电箱掉线故障;
            }
            if ((ConversionValue & 0X100) == 0X100)
            {
                return (int)EnumClass.DisBoxStatusClass.主控板掉线故障;
            }
            if ((ConversionValue & 0X08) == 0X08)
            {
                return (int)EnumClass.DisBoxStatusClass.市电电压故障;
            }
            if ((ConversionValue & 0X04) == 0X04)
            {
                return (int)EnumClass.DisBoxStatusClass.充电器故障;
            }
            if ((ConversionValue & 0X200) == 0X200)
            {
                return (int)EnumClass.DisBoxStatusClass.电池故障;
            }
            if ((ConversionValue & 0X80) == 0X80)
            {
                return (int)EnumClass.DisBoxStatusClass.电池组欠压故障;
            }
            if ((ConversionValue & 0X400) == 0X400)
            {
                return (int)EnumClass.DisBoxStatusClass.支路故障;
            }
            if ((ConversionValue & 0X10) == 0X10)
            {
                return (int)EnumClass.DisBoxStatusClass.过载故障;
            }
            if ((ConversionValue & 0X40) == 0X40)
            {
                return (int)EnumClass.DisBoxStatusClass.综合故障;
            }
            if ((ConversionValue & 0X20) == 0X20)
            {
                return (int)EnumClass.DisBoxStatusClass.逆变器故障;
            }
            return (int)EnumClass.DisBoxStatusClass.正常状态;
        }

        /// <summary>
        /// 根据EPS状态显示相应的文字
        /// </summary>
        /// <param name="EPSStatus"></param>
        /// <returns></returns>
        private string GetEPSStatus(int EPSStatus)
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
        /// 刷新主界面视图
        /// </summary>
        private void RefreshMainView()
        {
            if (SelectInfoEPSNoLogin.ID != 0)
            {
                SelectEPSQueryCurrentIndex = LstDistributionBox.FindIndex(x => x.ID == SelectInfoEPSNoLogin.ID);
                EPSStateNoLogin.Content = SelectInfoEPSNoLogin.Shield == 0 ? GetEPSStatus(SelectInfoEPSNoLogin.Status) : "正常";
                MainEleVoltageNoLogin.Content = SelectInfoEPSNoLogin.MainEleVoltage;//主电电压
                OutVoltage.Content = SelectInfoEPSNoLogin.DischargeVoltage;//输出电压
                BatteryVoltageNoLogin.Content = SelectInfoEPSNoLogin.BatteryVoltage;//电池电压
                DischargeCurrentNoLogin.Content = SelectInfoEPSNoLogin.DischargeCurrent;//放电电流

                //正极温度
            }

            if (SelectInfoEPSLogin.ID != 0)
            {
                SelectEPSQueryCurrentIndex = LstDistributionBox.FindIndex(x => x.ID == SelectInfoEPSLogin.ID);
                CurrentState.Content = SelectInfoEPSLogin.Shield == 0 ? (GetEPSStatus(SelectInfoEPSLogin.Status) == "正常" ? "正常" : "故障") : "正常";
                MainVoltage.Content = SelectInfoEPSLogin.MainEleVoltage;//主电电压
                OutputVoltage.Content = SelectInfoEPSLogin.DischargeVoltage;//输出电压
                BatteryVoltage.Content = SelectInfoEPSLogin.BatteryVoltage;//电池电压
                DischargeCurrent.Content = SelectInfoEPSLogin.DischargeCurrent;//放电电流
            }

            if (SelectInfoLightLogin.ID != 0)
            {
                SelectEPSQueryCurrentIndex = LstDistributionBox.FindIndex(x => x.Code == SelectInfoLightLogin.DisBoxID.ToString());
                LampState.Content = CommonFunct.GetLightStatus(SelectInfoLightLogin, LstDistributionBox.Find(x => x.Code == SelectInfoLightLogin.DisBoxID.ToString()), false);
            }
        }

        private void ClearSelect()
        {
            SelectInfoEPSNoLogin = new DistributionBoxInfo();
            SelectInfoEPSLogin = new DistributionBoxInfo();
            SelectInfoLightLogin = new LightInfo();
        }

        /// <summary>
        /// 主机板通讯
        /// </summary>
        private void HostBoardCom()
        {
            //DealHostBoardReceiveData();
            RefreshHostBoardInfo();
            //SetHostBoardSendStatus();
        }

        /// <summary>
        /// 处理主机板接收数据
        /// </summary>
        private void DealHostBoardReceiveData()
        {
            try
            {
                if (!IsRealFireAlarmLink && !IsSimulationLinkage && AbsFireAlarmLink.HostBoardReturnStatus != (byte)EnumClass.HostBoardStatus.通信故障)
                {
                    if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.强启按键) != 0)
                    {
                        if (!IsKeyEmergency)
                        {
                            AllEmergencyTotalTime = 0;
                            IsKeyEmergency = true;
                            btnEmergencyLogin_Click(null, null);
                        }
                    }
                    else
                    {
                        if (IsKeyEmergency)
                        {
                            CancelOrReset.Tag = "复位";
                            AllEmergencyTotalTimer.Enabled = false;

                            btnCancelOrReset_Click(CancelOrReset, null);

                            DetermineEmergency.Visibility = CancelOrReset.Visibility = System.Windows.Visibility.Visible;
                            CompulsoryEmergencyLogin.Visibility = System.Windows.Visibility.Hidden;
                            if (IsFunOpen)
                            {
                                FunctionPage.Visibility = TimeAndState.Visibility = MasterController.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                Homepage.Visibility = System.Windows.Visibility.Visible;
                            }

                            IsKeyEmergency = false;
                        }

                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.自检按键) != 0)
                        {
                            if (!SelfCheckPhyButton && !IsKeyEmergency)
                            {
                                if (IsLogin)
                                {
                                    btnSystemSelfCheck_Click(null, null);
                                }
                                else
                                {
                                    CommonFunct.PopupWindow("请登录后再使用该功能！");
                                }
                                SelfCheckPhyButton = true;
                            }
                        }
                        else
                        {
                            SelfCheckPhyButton = false;
                        }

                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.消音按键) != 0)
                        {
                            if (!MutePhyButton && !IsKeyEmergency)
                            {
                                btnMute_Click(null, null);
                                MutePhyButton = true;
                            }
                        }
                        else
                        {
                            MutePhyButton = false;
                        }

                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.月年检按键) != 0)
                        {
                            if (!MonthlyAndYearlyCheckPhyButton && !IsKeyEmergency)
                            {
                                if (IsLogin)
                                {
                                    mainwindow.IsEnabled = false;
                                    KeyMonthlyAndYearLyCheck();
                                    mainwindow.IsEnabled = true;
                                }
                                else
                                {
                                    CommonFunct.PopupWindow("请登录后再使用该功能！");
                                }
                                MonthlyAndYearlyCheckPhyButton = true;
                            }
                        }
                        else
                        {
                            MonthlyAndYearlyCheckPhyButton = false;
                        }

                        if ((AbsFireAlarmLink.HostBoardReturnReset & (byte)EnumClass.HostBoardReset.复位) != 0)
                        {
                            if (!ResetSystemPhyButton && !IsKeyEmergency)
                            {
                                ResetSystemButton();
                                ResetSystemPhyButton = true;
                            }
                        }
                        else
                        {
                            ResetSystemPhyButton = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        /// <summary>
        /// 清空统计故障变量
        /// </summary>
        private void ClearStatErrorVar()
        {
            FaultTotalCount = 0;//故障总数
            //FaultLampCount = 0;//灯具故障数
            //InfoDistributionBoxFault = null;//故障EPS
            //LstLightFault.Clear();//故障灯具
        }

        /// <summary>
        /// 刷新主机板信息
        /// </summary>
        private void RefreshHostBoardInfo()
        {
            #region
            OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description
            == ImgHostFault.Tag.ToString());
            bool isNormal = AbsFireAlarmLink.HostBoardReturnStatus != (byte)EnumClass.HostBoardStatus.通信故障;
            if (!isNormal)
            {
                for (int i = 0; i < 10; i++)
                {
                    AbsFireAlarmLink.SendHostBoardData(AbsFireAlarmLink.HostBoardSendStatus);
                    System.Windows.Forms.Application.DoEvents();
                    if (AbsFireAlarmLink.HostBoardReturnStatus != (byte)EnumClass.HostBoardStatus.通信故障)
                    {
                        isNormal = true;
                        break;
                    }
                }
            }

            if (isNormal)
            {
                if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电控制通讯故障") != null)
                {
                    ObjFaultRecord.Delete(LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电控制通讯故障").ID);
                    LstFaultRecord.Remove(LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电控制通讯故障"));
                }

                if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.二百二十伏故障) != 0)//220V主电故障
                {
                    //FaultTotalCount++;
                    NewHostFault = "主电故障";
                    if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电故障") == null)
                    {
                        RecordFault("主控器", null, "主电故障");
                    }
                    GetEdition(IsCommodity);
                    HostStatus.Content = "主电故障";
                    HostStatusLogin.Content = "故障";
                    if (infoOtherFaultRecord.IsExist != 1)
                    {
                        AddHistoricalEvent("主电故障");
                        //打印主机故障信息
                        FaultPrint(0, "主电故障");
                    }

                    if (infoOtherFaultRecord.IsExist == 0)
                    {
                        infoOtherFaultRecord.IsExist = 1;
                    }
                    if (!IsRealFireAlarmLink && IsTimingQueryEPSOrLight && !IsSimulationLinkage && !IsTranscendEmergency)
                    {
                        IsAllMainEle = false;
                        IsEmergency = true;//断主电应急
                        NoMainEmergencyTime.Enabled = true;
                    }

                    ALLEmergency.Visibility = System.Windows.Visibility.Visible;
                    HpALLEmergency.Visibility = System.Windows.Visibility.Visible;
                    MasterControllerStatus = "主电故障";
                }
                else
                {
                    if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电故障") != null)
                    {
                        NoMainEmergencyTime.Enabled = false;
                        AllEmergencyTotalTime = 0;
                        ObjFaultRecord.Delete(LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电故障").ID);
                        LstFaultRecord.Remove(LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电故障"));

                        if (IsEmergency)
                        {
                            Protocol.AllMainEle();//前一次状态为应急，目前应急取消，恢复主电
                        }
                        IsAllMainEle = true;
                        IsEmergency = false;

                    }

                    if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池故障) != 0 || (AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池短路) != 0)//电池开路故障
                    {
                        //FaultTotalCount++;
                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池故障) != 0)
                        {
                            NewHostFault = "电池开路故障";
                            if (LstFaultRecord.Find(x => x.Subject == "主控器" && (x.Fault == "电池开路故障" || x.Fault == "电池短路故障")) == null)
                            {
                                RecordFault("主控器", null, NewHostFault);
                            }
                            BatteryShort = false;
                        }
                        else
                        {
                            
                            NewHostFault = "电池短路故障";
                            if (LstFaultRecord.Find(x => x.Subject == "主控器" &&  x.Fault == "电池短路故障") == null)
                            {

                                RecordFault("主控器", null, NewHostFault);
                            }
                        }
                        
                        GetEdition(IsCommodity);
                        HostStatus.Content =  NewHostFault;
                        HostStatusLogin.Content = "故障";
                        
                        if (infoOtherFaultRecord.IsExist != 1)
                        {
                            AddHistoricalEvent(NewHostFault);
                            //打印主机故障信息
                            FaultPrint(0, NewHostFault);
                        }
                        if ( NewHostFault== "电池短路故障" && !BatteryShort)
                        {
                            AddHistoricalEvent(NewHostFault);
                            //打印主机故障信息
                            FaultPrint(0, NewHostFault);
                            BatteryShort = true;
                        }

                        if (infoOtherFaultRecord.IsExist == 0)
                        {
                            infoOtherFaultRecord.IsExist = 1;
                        }
                        MasterControllerStatus = NewHostFault;
                    }

                    if (AbsFireAlarmLink.HostBoardReturnStatus == 1 || AbsFireAlarmLink.HostBoardReturnStatus == 0)
                    {
                        if (infoOtherFaultRecord.IsExist == 1)
                        {
                            NewHostFault = string.Empty;
                            List<FaultRecordInfo> HostFaultRecord = LstFaultRecord.FindAll(x => x.Subject == "主控器");
                            if (HostFaultRecord != null)
                            {
                                for (int i = 0; i < HostFaultRecord.Count; i++)
                                {
                                    ObjFaultRecord.Delete(HostFaultRecord[i].ID);
                                    LstFaultRecord.Remove(HostFaultRecord[i]);
                                }
                            }
                            AddHistoricalEvent("系统恢复正常");
                            infoOtherFaultRecord.IsExist = 0;
                            BatteryShort = false;

                            if (MasterControllerStatus != "手动月检故障" && MasterControllerStatus != "手动年检故障")
                            {
                                HostStatus.Content = HostStatusLogin.Content = "正常";
                                MasterControllerStatus = "正常";
                            }
                        }

                        IsTranscendEmergency = false;
                        ALLEmergency.Visibility = System.Windows.Visibility.Hidden;
                        HpALLEmergency.Visibility = System.Windows.Visibility.Hidden;
                        GetEdition(IsCommodity);
                    }
                }
                if (!IsRealFireAlarmLink && !IsSimulationLinkage)
                {
                    SetAllEPSStatus(IsEmergency);
                    SetAllLightStatus(IsEmergency);
                }
            }
            else//主电控制通讯故障
            {
                if (infoOtherFaultRecord.Disable == 0)
                {
                    NewHostFault = "主电控制通讯故障";
                    if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.Fault == "主电控制通讯故障") == null)
                    {
                        RecordFault("主控器", null, "主电控制通讯故障");
                        GetEdition(IsCommodity);
                    }
                    HostStatus.Content = "主电控制通讯故障";
                    HostStatusLogin.Content = "故障";
                    if (infoOtherFaultRecord.IsExist != 1)
                    {
                        AddHistoricalEvent("主电控制通讯故障");
                        FaultPrint(0, "主电控制通讯故障");
                    }
                }
                if (infoOtherFaultRecord.IsExist == 0)
                {
                    infoOtherFaultRecord.IsExist = 1;
                }
                MasterControllerStatus = "主电控制通讯故障";
            }
            #endregion

        }

        /// <summary>
        /// 主机板灯板控制
        /// </summary>
        private void NewHostLampControl()
        {
            if (AbsFireAlarmLink.HostBoardReturnStatus != (byte)EnumClass.HostBoardStatus.通信故障)
            {
                int hostSendStatus = 0;
                if (IsCheckIndicatorLight)
                {
                    hostSendStatus += AbsFireAlarmLink.HostBoardSendStatus;
                }
                else
                {
                    //IsSimulationLinkage
                    if ((IsRealFireAlarmLink || IsLinkageTiming || IsKeyEmergency || IsComEmergency || IsSystemMonthDetection || IsSystemSeasonDetection) && !IsSystemMPDetection)
                    {
                        hostSendStatus += 2;//应急灯亮
                        hostSendStatus += 0X40;//联动应急蜂鸣器响
                        if (LstOtherFaultRecord.FindAll(x => x.IsExist == 1 && x.Disable == 0).Count != 0)//存在故障，未屏蔽故障
                        {
                            hostSendStatus += 4;//故障灯亮
                        }

                    }
                    else
                    {
                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.二百二十伏故障) == 0)
                        {
                            hostSendStatus += 1;//主电灯亮

                            if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池充电) != 0)
                            {
                                hostSendStatus += 32;//充电灯亮
                            }
                        }
                        else
                        {
                            hostSendStatus += 2;//应急灯亮
                        }

                        if (LstOtherFaultRecord.FindAll(x => x.IsExist == 1 && x.Disable == 0).Count != 0)//存在故障，未屏蔽故障
                        {
                            hostSendStatus += 4;//故障灯亮
                            if (!IsMute)//系统未消音
                            {
                                if (LstFaultRecord.FindAll(x => x.Subject == "主控器" && (x.Fault == "月检故障" || x.Fault == "年检故障")).Count != 0)
                                {
                                    hostSendStatus += 0X80;//月年检故障蜂鸣器响
                                }
                                else
                                {
                                    hostSendStatus += 0X08;//故障蜂鸣器响
                                }
                            }
                        }
                    }

                    if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池短路) == 0 && (AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池故障) == 0)
                    {
                        hostSendStatus += 16;//备电灯亮
                    }

                }

                AbsFireAlarmLink.HostBoardSendStatus = (byte)hostSendStatus;
                AbsFireAlarmLink.SendHostBoardData((byte)hostSendStatus);
            }
        }

        /// <summary>
        /// 主机板灯板控制(送检)
        /// </summary>
        private void OldHostLampControl()
        {
            if (AbsFireAlarmLink.HostBoardReturnStatus != (byte)EnumClass.HostBoardStatus.通信故障)
            {
                int hostSendStatus = 0;
                if (IsCheckIndicatorLight)
                {
                    hostSendStatus += AbsFireAlarmLink.HostBoardSendStatus;
                }
                else
                {
                    if (IsRealFireAlarmLink || IsSimulationLinkage || IsKeyEmergency || IsComEmergency)
                    {
                        hostSendStatus += 2;//应急灯亮
                        hostSendStatus += 0X40;//蜂鸣器响
                    }
                    else
                    {
                        if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.二百二十伏故障) == 0)
                        {
                            hostSendStatus += 1;//主电灯亮

                            if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池充电) != 0)
                            {
                                hostSendStatus += 32;//充电灯亮
                            }
                        }
                        else
                        {
                            hostSendStatus += 2;//应急灯亮
                        }
                    }

                    if (LstFaultRecord.Count != 0 )
                    {

                        //if (LstFaultRecord[LstFaultRecord.Count - 1].Fault == oldInfoFaultRecord.Fault && LstFaultRecord[LstFaultRecord.Count - 1].Subject == oldInfoFaultRecord.Subject && LstFaultRecord[LstFaultRecord.Count - 1].ChildSubject == null?true : LstFaultRecord[LstFaultRecord.Count - 1].ChildSubject==oldInfoFaultRecord.ChildSubject && LstFaultRecord[LstFaultRecord.Count - 1].ID == oldInfoFaultRecord.ID && LstFaultRecord[LstFaultRecord.Count - 1].Chi)
                        if (LstFaultRecord[LstFaultRecord.Count-1].Equals(oldInfoFaultRecord))
                        {
                            if (LstOtherFaultRecord.FindAll(x => x.IsExist == 1 && x.Disable == 0).Count != 0)//存在故障，未屏蔽故障
                            {
                                hostSendStatus += 4;//故障灯亮
                                if (!IsMute && !IsComEmergency && !IsKeyEmergency && !IsSimulationLinkage && !IsRealFireAlarmLink)//系统未消音
                                {
                                    hostSendStatus += 8;//蜂鸣器响
                                }
                                
                            }
                        }
                        else
                        {
                            hostSendStatus += 4;//故障灯亮
                            if(!IsComEmergency && !IsKeyEmergency && !IsSimulationLinkage && !IsRealFireAlarmLink)
                            {
                                hostSendStatus += 8;//蜂鸣器响
                            }
                            
                            IsMute = false;
                            oldInfoFaultRecord = LstFaultRecord[LstFaultRecord.Count - 1];
                        }
                        GetMuteImage();
                    }

                    if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池短路) == 0 && (AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池故障) == 0)
                    {
                        hostSendStatus += 16;//备电灯亮
                    }

                }
                AbsFireAlarmLink.HostBoardSendStatus = (byte)hostSendStatus;
                AbsFireAlarmLink.SendHostBoardData((byte)hostSendStatus);
            }
        }

        /// <summary>
        /// 根据版本实现系统功能效果
        /// </summary>
        private void GetEdition(bool IsGoods)
        {
            if (IsGoods)
            {
                NewHostLampControl();
            }
            else
            {
                OldHostLampControl();
            }
        }

        /// <summary>
        /// 实时记录系统故障
        /// </summary>
        /// <param name="subject">主设备</param>
        /// <param name="childSubject">子设备</param>
        /// <param name="fault">故障描述</param>
        private void RecordFault(string subject, string childSubject, string fault)
        {
            FaultRecordInfo infoFault = new FaultRecordInfo();
            string faulttype;
            List<FaultRecordInfo> LstFault = new List<FaultRecordInfo>();
            if (IsSystemMonthDetection)
            {
                faulttype = EnumClass.FaultType.月检故障.ToString();
            }
            else
            {
                if (IsSystemSeasonDetection)
                {
                    faulttype = EnumClass.FaultType.年检故障.ToString();
                }
                else
                {
                    faulttype = EnumClass.FaultType.普通故障.ToString();
                }
            }

            if (fault == "恢复正常" || fault == "配电箱掉线故障" || fault == "通信故障")
            {
                LstFault = LstFaultRecord.FindAll(x => x.Subject == subject && x.ChildSubject == childSubject && x.Fault != "配电箱掉线故障" && x.Fault != "通信故障");
                if (LstFault.Count != 0)
                {
                    for (int i = 0; i < LstFault.Count; i++)
                    {
                        LstFaultRecord.Remove(LstFault[i]);
                        ObjFaultRecord.Delete(LstFault[i].ID);
                    }
                }
                if (fault != "恢复正常" && LstFaultRecord.Find(x => x.Subject == subject && x.ChildSubject == childSubject && (x.Fault == "配电箱掉线故障" || x.Fault == "通信故障")) == null)
                {
                    infoFault.Subject = subject;
                    infoFault.ChildSubject = childSubject;
                    infoFault.FaultType = faulttype;
                    infoFault.Fault = fault;
                    LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                }

                if (fault == "恢复正常")
                {
                    LstFault = LstFaultRecord.FindAll(x => x.Subject == subject && x.ChildSubject == childSubject && (x.Fault == "配电箱掉线故障" || x.Fault == "通信故障"));
                    if (LstFault.Count != 0)
                    {
                        for (int i = 0; i < LstFault.Count; i++)
                        {
                            LstFaultRecord.Remove(LstFault[i]);
                            ObjFaultRecord.Delete(LstFault[i].ID);
                        }
                    }
                }
            }
            else
            {
                if (LstFaultRecord.Find(x => x.Subject == subject && x.ChildSubject == childSubject && x.Fault == fault) == null)
                {
                    //LstFault = LstFaultRecord.FindAll(x => x.Subject == subject && x.ChildSubject == childSubject && (x.Fault == "配电箱掉线故障" || x.Fault == "通信故障"));
                    //if (LstFault.Count != 0)
                    //{
                    //    for (int i = 0; i < LstFault.Count; i++)
                    //    {
                    //        LstFaultRecord.Remove(LstFault[i]);
                    //        ObjFaultRecord.Delete(LstFault[i].ID);
                    //    }
                    //}
                    infoFault.Subject = subject;
                    infoFault.ChildSubject = childSubject;
                    infoFault.FaultType = faulttype;
                    infoFault.Fault = fault;
                    //LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                }
            }

            FaultRecordInfo infoMOSFault = LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.月检故障.ToString());
            if (LstFaultRecord.FindAll(x => x.FaultType == EnumClass.FaultType.月检故障.ToString() && x.Fault != EnumClass.FaultType.月检故障.ToString() && x.Fault != EnumClass.FaultType.超时故障.ToString()).Count != 0 && infoMOSFault == null)
            {
                infoFault.Subject = "主控器";
                infoFault.ChildSubject = null;
                infoFault.FaultType = EnumClass.FaultType.月检故障.ToString();
                infoFault.Fault = "月检故障";
                //LstFaultRecord.Add(infoFault);
                ObjFaultRecord.Add(infoFault);
            }
            if (infoMOSFault != null && LstFaultRecord.FindAll(x => x.FaultType == EnumClass.FaultType.月检故障.ToString() && x.Fault != EnumClass.FaultType.月检故障.ToString() && x.Fault != EnumClass.FaultType.超时故障.ToString()).Count == 0)
            {
                //LstFaultRecord.Remove(infoMOSFault);
                ObjFaultRecord.Delete(infoMOSFault.ID);
            }

            infoMOSFault = LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.年检故障.ToString());
            if (infoMOSFault == null && LstFaultRecord.FindAll(x => x.FaultType == EnumClass.FaultType.年检故障.ToString() && x.Fault != EnumClass.FaultType.年检故障.ToString() && x.Fault != EnumClass.FaultType.超时故障.ToString()).Count != 0)
            {
                infoFault.Subject = "主控器";
                infoFault.ChildSubject = null;
                infoFault.FaultType = EnumClass.FaultType.年检故障.ToString();
                infoFault.Fault = "年检故障";
                //LstFaultRecord.Add(infoFault);
                ObjFaultRecord.Add(infoFault);
            }
            if (infoMOSFault != null && LstFaultRecord.FindAll(x => x.FaultType == EnumClass.FaultType.年检故障.ToString() && x.Fault != EnumClass.FaultType.年检故障.ToString() && x.Fault != EnumClass.FaultType.超时故障.ToString()).Count == 0)
            {
                //LstFaultRecord.Remove(infoMOSFault);
                ObjFaultRecord.Delete(infoMOSFault.ID);
            }

            LstFaultRecord.Clear();
            LstFaultRecord = ObjFaultRecord.GetAll();
        }


        /// <summary>
        /// 刷新EPS和灯具信息
        /// </summary>
        private void RefreshEPSAndLightInfo()
        {
            if(LstOtherFaultRecord == null)
            {
                LstOtherFaultRecord = ObjOtherFaultRecord.GetAll();
            }
            
            string epsFaultTag = string.Empty;
            string lampFaultTag = string.Empty;
            Dispatcher.Invoke(() =>
            {
                epsFaultTag = ImgEPSFault.Tag.ToString();
                lampFaultTag = ImgLampFault.Tag.ToString();
            });

            OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description
            == epsFaultTag);
            List<FaultRecordInfo> OldFault = LstFaultRecord.FindAll(x => x.Subject.Substring(0, 1) == "6" && LstDistributionBox.Find(y => x.Subject == y.Code) == null && x.ChildSubject == null);
            LstFaultRecord.RemoveAll(x => OldFault.Find(y => x.Subject == y.Subject) != null);
            if (LstFaultRecord.FindAll(x => x.Subject.Substring(0, 1) == "6" && x.ChildSubject == null).Count != 0)
            {
                if (infoOtherFaultRecord.IsExist == 0)
                {
                    infoOtherFaultRecord.IsExist = 1;
                }
            }
            else
            {
                infoOtherFaultRecord.IsExist = 0;
            }

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == lampFaultTag);
            List<LightInfo> OldLight = LstLightFault.FindAll(x => LstLight.Find(y => y.Code == x.Code && y.DisBoxID == x.DisBoxID) == null);
            LstLightFault.RemoveAll(x => OldLight.Find(y => y.Code == x.Code && y.DisBoxID == x.DisBoxID) != null);

            if (LstLightFault.Count != 0)
            {
                if (infoOtherFaultRecord.IsExist == 0)
                {
                    infoOtherFaultRecord.IsExist = 1;
                }
            }
            else
            {
                infoOtherFaultRecord.IsExist = 0;
            }
            ObjOtherFaultRecord.Save(LstOtherFaultRecord);
        }

        /// <summary>
        /// 切换EPS查询当前索引
        /// </summary>
        private void SwitchEPSQueryCurrentIndex()
        {
            if (LstDistributionBox == null)
            {
                LstDistributionBox = ObjDistributionBox.GetAll();
            }
            if (LstDistributionBox.Count > 0)
            {
                if (EPSQueryCurrentIndex == LstDistributionBox.Count - 1 )
                {
                    EPSQueryCurrentIndex = 0;
                }
                else
                {
                    EPSQueryCurrentIndex++;
                }
            }
        }

        private void SwitchLightQueryCurrentIndex()
        {
            if (LstLightQueryByEPSID.Count > 0)
            {
                LightQueryCurrentIndex++;
            }
            else
            {
                //LightStatusByEPSArray = null;
                IsQueryEPSAndLight = true;
            }
        }

        /// <summary>
        /// 设置是否登录
        /// </summary>
        private void SetIsLogin(bool isEnabled)
        {
            IsLogin = isEnabled;
        }

        /// <summary>
        /// 设置全部EPS状态
        /// </summary>
        /// <param name="isEmergency"></param>
        private void SetAllEPSStatus(bool isEmergency)
        {
            if (isEmergency)
            {
                for (int i = 0; i < LstDistributionBox.Count; i++)
                {
                    if (ADDEPSStatus(LstDistributionBox[i].Status) == (int)EnumClass.DisBoxStatusClass.配电箱掉线故障)
                    {
                        LstDistributionBox[i].IsEmergency = 0;
                    }
                    else
                    {
                        LstDistributionBox[i].IsEmergency = 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < LstDistributionBox.Count; i++)
                {
                    if (ADDEPSStatus(LstDistributionBox[i].Status) == (int)EnumClass.DisBoxStatusClass.市电电压故障)
                    {
                        LstDistributionBox[i].IsEmergency = 1;
                    }
                    else
                    {
                        LstDistributionBox[i].IsEmergency = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 设置全部灯具状态
        /// </summary>
        /// <param name="isEmergency"></param>
        private void SetAllLightStatus(bool isEmergency)
        {
            if (isEmergency)
            {

                LstLight.FindAll(x => x.ErrorTime == string.Empty).ForEach(x => x.IsEmergency = 1);
            }
            else
            {
                DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo();
                for (int i = 0; i < LstLight.Count; i++)
                {
                    infoDistributionBox = LstDistributionBox.Find(x => x.Code == LstLight[i].DisBoxID.ToString());
                    if ((infoDistributionBox.Status & 0X08) == 0X08)
                    {
                        LstLight[i].IsEmergency = 1;
                    }
                    else
                    {
                        LstLight[i].IsEmergency = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 设置是否主体主电
        /// </summary>
        private void SetIsAllMainEle(bool isEnabled)
        {
            IsAllMainEle = isEnabled;
        }

        /// <summary>
        /// 设置是否编辑全部图标
        /// </summary>      
        private void SetIsEditAllIcon(bool isEnabled)
        {
            IsEditAllIcon = isEnabled;
        }

        /// <summary>
        /// 改变是否拖拽楼层图标
        /// </summary>      
        private void SetIsDragIconSearchCode(bool isEnabled)
        {
            IsDragIconSearchCode = isEnabled;
        }

        /// <summary>
        /// 设置是否真实火灾联动
        /// </summary>
        private void SetIsRealFireAlarmLink(bool isEnabled)
        {
            IsRealFireAlarmLink = isEnabled;
        }

        /// <summary>
        /// 设置是否进入复位系统
        /// </summary>       
        private void SetIsEnterResetSystem(bool isEnabled)
        {
            IsEnterResetSystem = isEnabled;
        }

        /// <summary>
        /// 设置模拟联动执行预案定时器
        /// </summary>       
        private void SetSimulateFireAlarmLinkExePlanTimer(bool isEnabled)
        {
            SimulateFireAlarmLinkExePlanTimer.Enabled = isEnabled;
        }

        /// <summary>
        /// 检查是否正在真实火灾联动
        /// </summary>
        /// <returns></returns>
        private bool CheckIsRealFireAlarmLinkNow()
        {
            if (IsRealFireAlarmLink)
            {
                CommonFunct.PopupWindow("正在火灾联动，请先复位！");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定EPS应急或者主电
        /// </summary>
        private void EmergencyOrMainEleByEPS(bool isEmergency, string strEPSCode)
        {
            Task task = Task.Run(() =>
            {
                DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strEPSCode);
                IsTimingQueryEPSOrLight = false;
                CurrentExeInstructTime = 0;

                if (isEmergency)
                {
                    #region
                    //for (int i = 0; i < ExecuteCommonStructTimes; i++)
                    //{
                    //    System.Windows.Forms.Application.DoEvents();
                    //    Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                    //    Protocol.EmergencyOrMainEleByEPS(isEmergency, strEPSCode);
                    //}
                    #endregion
                    Protocol.EmergencyOrMainEleByEPS(isEmergency, strEPSCode);
                    infoDistributionBox.IsEmergency = 1;
                    List<LightInfo> LstEPSLamp = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBox.Code));
                    LstEPSLamp.ForEach(x => x.IsEmergency = 1);
                    ObjDistributionBox.Update(infoDistributionBox);
                }
                else
                {
                    #region
                    //for (int i = 0; i < ExecuteAllMainEleStructTime; i++)
                    //{
                    //    System.Windows.Forms.Application.DoEvents();
                    //    Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                    //    Protocol.EmergencyOrMainEleByEPS(isEmergency, strEPSCode);
                    //}
                    #endregion
                    Protocol.EmergencyOrMainEleByEPS(isEmergency, strEPSCode);
                    infoDistributionBox.IsEmergency = 0;
                    List<LightInfo> LstEPSLamp = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBox.Code));
                    LstEPSLamp.ForEach(x => x.IsEmergency = 0);
                    ObjDistributionBox.Update(infoDistributionBox);

                    //等待EPS复位
                    while (CurrentExeInstructTime < ResetWaitTime)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(ExeCommonInstructSleepTime);
                        CurrentExeInstructTime += ExeCommonInstructSleepTime;
                    }
                    CurrentExeInstructTime = 0;
                }

                IsTimingQueryEPSOrLight = true;
            });
        }

        private void CurrentDateTimer_Tick(object sender, EventArgs e)
        {
            ShowDateTime();
        }

        private void AllMainEleTimer_Tick(object sender, EventArgs e)
        {
            CommonResetSystem();
        }

        private void ScalingLayer_Tick(object sender, EventArgs e)
        {
            RefreshAllIcon();
        }

        private void MonthOrSeasonCheckFault_Tick(object sender, EventArgs e)
        {
            FaultRecordInfo infoFault = new FaultRecordInfo();
            DateTime NowTime = DateTime.Now;
            DateTime NextMonthCheckTime = Convert.ToDateTime(LstGblSetting.Find(x => x.Key == "NextMonthCheckTime").SetValue).AddDays(7);
            DateTime NextSeasonCheckTime = Convert.ToDateTime(LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime").SetValue).AddDays(7);
            if (DateTime.Compare(NowTime, NextMonthCheckTime) > 0)
            {
                //RecordFault("主控器", null, "月检故障");
                if (LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.超时故障.ToString() && x.FaultType == EnumClass.FaultType.月检故障.ToString()) == null)
                {
                    infoFault.Subject = "主控器";
                    infoFault.ChildSubject = null;
                    infoFault.FaultType = EnumClass.FaultType.月检故障.ToString();
                    infoFault.Fault = EnumClass.FaultType.超时故障.ToString();
                    LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                    AddHistoricalEvent("手动月检故障");
                    //主机故障打印
                    FaultPrint(0, infoFault.Fault);
                }
            }
            else
            {
                infoFault = LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.超时故障.ToString() && x.FaultType == EnumClass.FaultType.月检故障.ToString());
                if (infoFault != null)
                {
                    LstFaultRecord.Remove(infoFault);
                    ObjFaultRecord.Delete(infoFault.ID);
                    AddHistoricalEvent("手动月检恢复正常");
                }
            }

            if (DateTime.Compare(NowTime, NextSeasonCheckTime) > 0)
            {
                infoFault = new FaultRecordInfo();
                if (LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.超时故障.ToString() && x.FaultType == EnumClass.FaultType.年检故障.ToString()) == null)
                {
                    infoFault.Subject = "主控器";
                    infoFault.ChildSubject = null;
                    infoFault.FaultType = EnumClass.FaultType.年检故障.ToString();
                    infoFault.Fault = EnumClass.FaultType.超时故障.ToString();
                    LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                    AddHistoricalEvent("手动年检故障");
                    //主机故障打印
                    FaultPrint(0, infoFault.Fault);
                }
            }
            else
            {
                infoFault = LstFaultRecord.Find(x => x.Fault == EnumClass.FaultType.超时故障.ToString() && x.FaultType == EnumClass.FaultType.年检故障.ToString());
                if (infoFault != null)
                {
                    LstFaultRecord.Remove(infoFault);
                    ObjFaultRecord.Delete(infoFault.ID);
                    AddHistoricalEvent("手动年检恢复正常");
                }
            }
        }

        private async Task QueryEPSAndLightTimer_Tick(object sender, EventArgs e)
        {
            //if (stpSetCommunication.Visibility == System.Windows.Visibility.Hidden)
            //{
            //    SetComSerialPortData();
            //}
            ClearStatErrorVar();
            await InspectEPSAndLightTimer();
            ModifyHostState();
            ShowHistoryEventRecordLog();

        }

        private void QueryLightByEPS()
        {
            if (IsQueryLight)
            {
                ModifyHostState();
                ShowHistoryEventRecordLog();
            }
        }



        private void ModifyHostState()
        {
            //故障图标的显示
            ShowALLFault();

            LstGblSetting.Clear();
            LstGblSetting = ObjGblSetting.GetAll();
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "HostBatVoltage");
            //更换电池图标
            if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.电池充电) != 0)
            {
                BatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryCharge.png", UriKind.Relative));
                HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryCharge.png", UriKind.Relative));
            }
            else
            {
                if (LstFaultRecord.FindAll(x => x.Subject == "主控器" && (x.Fault == "电池开路故障" || x.Fault == "电池短路故障")).Count != 0)
                {
                    BatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryNo.png", UriKind.Relative));
                    HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryNo.png", UriKind.Relative));
                }
                else
                {
                    if (infoGblSetting.SetValue != string.Empty)
                    {
                        if (Convert.ToDouble(infoGblSetting.SetValue) >= 12.5)
                        {
                            BatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryLevel3.png", UriKind.Relative));
                            HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\BatteryLevel3.png", UriKind.Relative));
                        }
                        else if (Convert.ToDouble(infoGblSetting.SetValue) >= 11.5 && Convert.ToDouble(infoGblSetting.SetValue) < 12.5)
                        {
                            BatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel2.png", UriKind.Relative));
                            HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel2.png", UriKind.Relative));
                        }
                        else
                        {
                            BatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel1.png", UriKind.Relative));
                            HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel1.png", UriKind.Relative));
                        }
                    }
                    else
                    {
                        BatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel0.png", UriKind.Relative));
                        HPBatImage.Source = new BitmapImage(new Uri("\\Pictures\\batteryLevel0.png", UriKind.Relative));
                    }
                }
            }
        }

        private void ExecuteMonthCheckTimer_Tick(object sender, EventArgs e)
        {
            //StopRefreshExecuteMonthCheckTiming();
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            if (Convert.ToDateTime(infoGblSetting.SetValue).Year != DateTime.Now.Year || Convert.ToDateTime(infoGblSetting.SetValue).Month != DateTime.Now.Month)//判断该月是不是需要执行年检
            {
                if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsMonthlyInspection").SetValue))
                {
                    bool isSuccess = ExecuteMonthCheck();
                    if (isSuccess)
                    {
                        //MonthlyCheckTime++;//计算月检次数
                        btnMonthCheckByHand_Click(null, null);
                    }
                }
            }
        }

        private void ExecuteSeasonCheckTimer_Tick(object sender, EventArgs e)
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            if (Convert.ToDateTime(infoGblSetting.SetValue).Year == DateTime.Now.Year && Convert.ToDateTime(infoGblSetting.SetValue).Month == DateTime.Now.Month)
            {
                if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection").SetValue))
                {
                    bool isSuccess = ExecuteSeasonCheck();
                    if (isSuccess)
                    {
                        btnSeasonCheckByHand_Click(null, null);
                        infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
                        DateTime dtSeasonNextTime = Convert.ToDateTime(infoGblSetting.SetValue);
                        YearlyMonth.Text = dtSeasonNextTime.Month.ToString();
                    }
                }
                //MonthlyCheckTime = 0;//清空月检次数
                //IsMonthOrSeason = false;//年检后下次月检
            }
            //if (IsMonthOrSeason)
            //{
            //    if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection").SetValue))
            //    {
            //        bool isSuccess = ExecuteSeasonCheck();
            //        if (isSuccess)
            //        {
            //            btnSeasonCheckByHand_Click(null, null);
            //            infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            //            DateTime dtSeasonNextTime = Convert.ToDateTime(infoGblSetting.SetValue);
            //            this.YearlyMonth.Text = dtSeasonNextTime.Month.ToString();
            //        }
            //    }
            //    MonthlyCheckTime = 0;//清空月检次数
            //    IsMonthOrSeason = false;//年检后下次月检
            //}
        }

        private void SimulateFireAlarmLinkTimer_Tick(object sender, EventArgs e)
        {
            CalcuSimulateFireAlarmLinkTime();
        }

        private void RealFireAlarmLinkTimer_Tick(object sender, EventArgs e)
        {
            CalCuRealLinkFireAlarmLinkTime();
        }

        private void SimulateFireAlarmLinkExePlanTimer_Tick(object sender, EventArgs e)
        {
            SimulateFireAlarmLinkExePlan();
        }

        private void AllEmergencyTotalTimer_Tick(object sender, EventArgs e)
        {
            CalcuAllEmergencyTotalTime();
        }

        private void NoMainEmergencyTime_Tick(object sender, EventArgs e)
        {
            if (!IsRealFireAlarmLink && AllEmergencyTotalTime % 5 == 0)
            {
                Protocol.AllEmergency();
            }
            AllEmergencyTotalTime++;
            if (AllEmergencyTotalTime >= int.Parse(LstGblSetting.Find(x => x.Key == "EmergencyTime").SetValue) * 60)
            {
                AllEmergencyTotalTime = 0;
                NoMainEmergencyTime.Enabled = false;
                IsAllMainEle = true;
                IsEmergency = false;//断主电应急
                IsTranscendEmergency = true;
                AllMainEle();//全体主电
            }
        }

        private void RealFireAlarmLinkRequestTimer_Tick(object sender, EventArgs e)
        {
            RealFireAlarmLinkRequest();
        }

        /// <summary>
        /// 按下消音按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            SetIsMute();
        }

        /// <summary>
        /// 手动启动强制应急
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmergencyLogin_Click(object sender, RoutedEventArgs e)
        {
            ClearIconSearchCode();
            OpenAllEmergencyPage();
            DetEmergency();
            IndicatorLight();
        }

        private void FireLayerDisplay(object sender, RoutedEventArgs e)
        {
            SetIsLogin(false);
            ClearAllElementLogin();
            MenuCancel();
            //InitEPSShowNoLogin();

            InitLayerModeNoLogin();
        }

        /// <summary>
        /// 添加历史事件数据
        /// </summary>
        /// <param name="thing"></param>
        private void AddHistoricalEvent(string thing)
        {
            HistoricalEventInfo infoHistoricalEvent = new HistoricalEventInfo
            {
                EventTime = DateTime.Now.ToString(),
                EventContent = thing
            };
            if (LstHistoricalEvent.Count == 0)
            {
                infoHistoricalEvent.ID = 1;
            }
            else
            {
                infoHistoricalEvent.ID = LstHistoricalEvent[LstHistoricalEvent.Count - 1].ID + 1;
            }
            LstHistoricalEvent.Add(infoHistoricalEvent);
            ObjHistoricalEvent.Add(infoHistoricalEvent);
        }

        private void AddHistoricalEvent(string thing, string time)
        {
            HistoricalEventInfo infoHistoricalEvent = new HistoricalEventInfo
            {
                EventContent = thing,
                EventTime = time == string.Empty ? DateTime.Now.ToString() : time
            };
            if (LstHistoricalEvent.Count == 0)
            {
                infoHistoricalEvent.ID = 1;
            }
            else
            {
                infoHistoricalEvent.ID = LstHistoricalEvent[LstHistoricalEvent.Count - 1].ID + 1;
            }
            LstHistoricalEvent.Add(infoHistoricalEvent);
            ObjHistoricalEvent.Add(infoHistoricalEvent);                
        }

        private void btnMenuCancel_Click(object sender, RoutedEventArgs e)
        {
            AddHistoricalEvent("退出登录");

            SetIsLogin(false);
            ClearAllElementLogin();
            MenuCancel();

            EPSTotalNumber.Content = LstDistributionBox.Count;
            LampTotalNumber.Content = LstLight.Count;

            EPSShowCurrentPage.Content = 1;
            EPSShowTotalPage.Content = LstDistributionBox.Count % 12 == 0 ? LstDistributionBox.Count / 12 : LstDistributionBox.Count / 12 + 1;
            EPSImageDisplayTotalPage = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (EPSImageDisplayColumnCount * EPSImageDisplayMaxRowCount) + 1 : 1;


            InitEPSShowNoLogin();
            ShowHistoryEventRecordLog();
        }

        private void btnExitSystem_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = ShowExit();
            if (isSuccess)
            {
                AddHistoricalEvent("系统关机");
                ObjFaultRecord.DeleteAll();//清除系统所有故障记录
                Environment.Exit(0);
            }
            else
            {
                if (stpLayerModeNoLogin.Visibility == System.Windows.Visibility.Hidden)
                {
                    MasterController.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void btnMonthCheckByHand_Click(object sender, RoutedEventArgs e)
        {
            IsSystemMonthDetection = true;
            AllEmergencySystem();//全体应急
            StartMonthCheckByHand();
            bool IsSucceed = MonthCheckByHand(string.Empty);
            SetCheckSelfTestFeedBackPage(true);
            SystemSelfCheck();//显示月检结果
            EndMonthCheckByHand(IsSucceed);
            SetAllMainEleTimer(true);
            IsSystemMonthDetection = false;
        }

        private void btnSeasonCheckByHand_Click(object sender, RoutedEventArgs e)
        {
            IsSystemSeasonDetection = true;
            AllEmergencySystem();
            StartSeasonCheckByHand();
            bool IsSucceed = SeasonCheckByHand();
            SetCheckSelfTestFeedBackPage(true);
            SystemSelfCheck();//显示年检结果
            EndSeasonCheckByHand(IsSucceed);
            SetAllMainEleTimer(true);
            IsSystemSeasonDetection = false;
        }

        /// <summary>
        /// 按下自检按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSystemSelfCheck_Click(object sender, RoutedEventArgs e)
        {
            SetCheckSelfTestFeedBackPage(true);
            IsQueryEPSAndLight = false;
            AbsFireAlarmLink.IsHostThread = false;
            Thread Check = new Thread(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    SetIsAllMainEle(false);
                    RecordCheckSelfTestInfo();
                    SetExitCheckSelfTestFeedBack(false);
                    SystemSelfCheck();
                    SetExitCheckSelfTestFeedBack(true);
                    SetIsAllMainEle(true);
                }));
            });

            Thread Show = new Thread(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    FunctionPage.Visibility = stpCheckSelfTestFeedBack.Visibility = System.Windows.Visibility.Visible;
                    //CheckTop.Content = "系统自检中..";
                    //Thread.Sleep(5000);
                    Check.Start();
                    Check.IsBackground = true;
                }));
            });
            Show.Start();

            //Task.Run(() =>
            //{
            //});

            //Thread Check = new Thread(() =>
            //{
            //    this.Dispatcher.Invoke(new Action(() =>
            //    {
            //        SetIsAllMainEle(false);
            //        RecordCheckSelfTestInfo();
            //        SetExitCheckSelfTestFeedBack(false);
            //        SystemSelfCheck();
            //        SetExitCheckSelfTestFeedBack(true);
            //        SetIsAllMainEle(true);
            //    }));
            //});
            //Check.Start();
            //Check.IsBackground = true;
        }

        private void btnExitCheckSelfTestFeedBack_Click(object sender, RoutedEventArgs e)
        {
            tbCheckSelfTestFeedBack.Text = string.Empty;
            SetCheckSelfTestFeedBackPage(false);
        }

        private void btnAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            AdvancedSettings AdvancedSettings = new AdvancedSettings();
            AdvancedSettings.ShowDialog();
        }

        /// <summary>
        /// 自动备份
        /// </summary>
        private void AutomaticBackups()
        {
            try
            {
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "DataSavePath");

                string[] Files = Directory.GetFiles(infoGblSetting.SetValue);
                if (Files.Length != 0)
                {
                    #region 获取下一次备份时间
                    FileInfo file = new FileInfo(Files[0]);
                    DateTime LastTime = file.CreationTime;
                    foreach (string File in Files)
                    {
                        file = new FileInfo(File);
                        if (file.CreationTime > LastTime)
                        {
                            LastTime = file.CreationTime;
                        }
                    }

                    DateTime NextTime = GetNextBackUpTime(LastTime);
                    #endregion

                    if (DateTime.Now >= NextTime)
                    {
                        BackUp(infoGblSetting.SetValue);
                    }
                }
                else
                {
                    BackUp(infoGblSetting.SetValue);
                }
            }
            catch
            {
                CommonFunct.PopupWindow("数据备份失败！");
            }
        }

        /// <summary>
        /// 备份数据
        /// </summary>
        /// <param name="SaveAddress"></param>
        private void BackUp(string SaveAddress)
        {
            string appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string dbFilePath = appStartupPath + @"\PCSmallHost.db";
            string dbNewFilePath = SaveAddress + @"\PCSmallHostBackUp" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".db";

            File.Copy(dbFilePath, dbNewFilePath);//拷贝数据库文件
            if (File.Exists(dbNewFilePath))
            {
                CommonFunct.PopupWindow("数据备份完成！\n 文件路径：\n" + dbNewFilePath);
            }
        }

        /// <summary>
        /// 获取下一次备份的时间
        /// </summary>
        /// <param name="LastTime">最近一次备份的时间</param>
        /// <returns></returns>
        private DateTime GetNextBackUpTime(DateTime LastTime)
        {
            switch (LstGblSetting.Find(x => x.Key == "BackUpIntervalTime").SetValue)
            {
                case "12小时":
                    return LastTime.AddHours(12);
                case "1天":
                    return LastTime.AddDays(1);
                case "5天":
                    return LastTime.AddDays(5);
                case "10天":
                    return LastTime.AddDays(10);
                case "20天":
                    return LastTime.AddDays(20);
                case "1月":
                    return LastTime.AddMonths(1);
                default:
                    return LastTime;
            }
        }

        private void btnModifyTotalFloor_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = ModifyTotalFloor();
            if (isSuccess)
            {
                InitCurrengPageFloorNoLogin();
                SwitchFloorLogin();
            }

        }

        private void btnJumpSelectFloorNoLogin_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = JumpSelectFloorNoLogin();
            if (isSuccess)
            {
                SwitchFloorNoLogin();
            }
        }

        private void btnJumpSelectFloorLogin_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = JumpSelectFloorLogin();
            if (isSuccess)
            {
                SwitchFloorLogin();
            }
            stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 图形界面上EPS和灯具一键清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOneKeyClear_Click(object sender, RoutedEventArgs e)
        {
            bool isOneKeyClear = OneKeyClear();
            if (isOneKeyClear)
            {
                ResetPositionFloorLogin();//清除相对的EPS和灯具楼层Location记录
                ClearAllElementLogin();
                InitIconSearchCodeListLogin();
                GetDataCurrentFloorLogin();
                RefreshLayerModeLogin(true, true);//刷新图层楼层
            }
            BlankIcon = new Image();
            PointEdit.Visibility = System.Windows.Visibility.Hidden;
            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
        }

        private void lvIconSearchCodeList_MouseMove(object sender, MouseEventArgs e)
        {
            GetSelectIconSearchCode(sender, e);
        }

        private void lvIconSearchCodeList_TouchMove(object sender, TouchEventArgs e)
        {
            GetSelectIconSearchCode(sender);
        }

        private void imgUpNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isSuccess = LastPageFloorNoLogin();
            if (isSuccess)
            {
                SwitchFloorNoLogin();
            }
        }

        private void imgDownNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isSuccess = NextPageFloorNoLogin();
            if (isSuccess)
            {
                SwitchFloorNoLogin();
            }
        }

        private void SliderControlNoLogin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool isScaleTransformDrawingNoLogin = ScaleTransformDrawingNoLogin(e);
            if (isScaleTransformDrawingNoLogin)
            {
                RefreshLayerModeNoLogin(false, true);
            }
        }

        private void SliderControlLogin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool isScaleTransformDrawingLogin = ScaleTransformDrawingLogin(e);
            if (isScaleTransformDrawingLogin)
            {
                RefreshLayerModeLogin(false, true);
            }
        }

        private void ctcFloorDrawingNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClearShowIconSearchCodeInfoNoLogin(false);
                ClickDownFloorDrawingNoLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingNoLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClickUpFloorDrawingNoLogin();
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingNoLogin_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                ClearShowIconSearchCodeInfoNoLogin(false);
                ClickDownFloorDrawingNoLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingNoLogin_TouchUp(object sender, TouchEventArgs e)
        {
            try
            {
                ClickUpFloorDrawingNoLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClearAllIconPanelLogin();
                ClickDownFloorDrawingLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                ClearAllIconPanelLogin();
                ClickDownFloorDrawingLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClickUpFloorDrawingLogin();
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_TouchUp(object sender, TouchEventArgs e)
        {
            try
            {
                ClickUpFloorDrawingLogin(e);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingNoLogin_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    bool isTranslateTransformDrawingNoLogin = TranslateTransformDrawingNoLogin(e);
                    if (isTranslateTransformDrawingNoLogin)
                    {
                        RefreshLayerModeNoLogin(false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingNoLogin_TouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                bool isTranslateTransformDrawingNoLogin = TranslateTransformDrawingNoLogin(e);
                if (isTranslateTransformDrawingNoLogin)
                {
                    RefreshLayerModeNoLogin(false, false);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (MoveEquipment == null)
                    {
                        bool isTranslateTransformDrawingLogin = TranslateTransformDrawingLogin(e);
                        if (isTranslateTransformDrawingLogin)
                        {
                            RefreshLayerModeLogin(false, false);
                        }
                    }
                    else
                    {
                        if (MoveEquipment is Image)
                        {
                            MovePartitionPointLogin(sender, e);
                            RefreshPartitionPointLogin();
                        }
                        else
                        {
                            bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                            if (isSuccess)
                            {
                                ClearAllIconPanelLogin();
                                RefreshIconSearchCodeLogin();
                            }
                        }
                    }
                }
                else
                {
                    if (MoveEquipment != null)
                    {
                        MoveEquipment = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_TouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                if (MoveEquipment == null)
                {
                    bool isTranslateTransformDrawingLogin = TranslateTransformDrawingLogin(e);
                    if (isTranslateTransformDrawingLogin)
                    {
                        RefreshLayerModeLogin(false, false);
                    }
                }
                else
                {
                    if (MoveEquipment is Image)
                    {
                        MovePartitionPointLogin(sender, e);
                        RefreshPartitionPointLogin();
                    }
                    else
                    {
                        bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                        if (isSuccess)
                        {
                            ClearAllIconPanelLogin();
                            RefreshIconSearchCodeLogin();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                DragOverAllIconLogin(e);
                SetIsDragIconSearchCode(true);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void ctcFloorDrawingLogin_Drop(object sender, DragEventArgs e)
        {
            try
            {
                bool isSuccess = CheckIsEditAllIcon();
                if (isSuccess)
                {
                    DropListIconToFloorDrawingLogin();
                    RemoveIconSearchCodeListLogin();
                    GetDataCurrentFloorLogin();
                    SetIsDragIconSearchCode(false);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        /// <summary>
        /// 显示转折点
        /// </summary>
        /// <param name="coordinateX"></param>
        /// <param name="coordinateY"></param>
        /// <param name="index"></param>
        private void AddTurningPointImage(double coordinateX, double coordinateY, int index)
        {
            if (IsPrintLine)
            {
                CoordinateInfo infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TransformX == coordinateX && x.TransformY == coordinateY);
                Label label = new Label
                {
                    Height = 20,
                    Width = 20,
                    Content = LstEscapeRoutes.Find(x => x.ID == infoCoordinate.TableID).TurnIndex,
                    FontSize = 7,
                    Foreground = CommonFunct.GetBrush("#cc0033"),
                    Margin = new Thickness(coordinateX - 15, coordinateY - 5, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                cvsMainWindow.Children.Add(label);
            }

            Image image = new Image
            {
                Tag = index,
                Height = 10,
                Width = 10,
                Source = new BitmapImage(new Uri("/Pictures/TurningPoint-Normal.png", UriKind.Relative)),
                Margin = new Thickness(coordinateX - 5, coordinateY - 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            cvsMainWindow.Children.Add(image);
        }

        private void printing(EscapeLinesInfo infoEscapeLines)
        {
            Line line = new Line();
            if (SelectedLine == infoEscapeLines)
            {
                line.Stroke = System.Windows.Media.Brushes.Red;
            }
            else
            {
                line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
            }
            if (stpLayerModeLogin.Visibility == System.Windows.Visibility.Visible)
            {
                line.X1 = infoEscapeLines.TransformX1;
                line.Y1 = infoEscapeLines.TransformY1;
                line.X2 = infoEscapeLines.TransformX2;
                line.Y2 = infoEscapeLines.TransformY2;
            }
            else
            {
                line.X1 = infoEscapeLines.NLineX1;
                line.X2 = infoEscapeLines.NLineX2;
                line.Y1 = infoEscapeLines.NLineY1;
                line.Y2 = infoEscapeLines.NLineY2;
            }
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Top;
            line.StrokeThickness = 4;
            cvsMainWindow.Children.Add(line);
        }

        private void printing(double lineX1, double lineX2, double lineY1, double lineY2)
        {
            Line line = new Line
            {
                Stroke = System.Windows.Media.Brushes.LightSkyBlue,
                X1 = lineX1,
                X2 = lineX2,
                Y1 = lineY1,
                Y2 = lineY2,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4
            };
            cvsMainWindow.Children.Add(line);
        }

        private void GetCoordinates_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //QueryEPSAndLightTimer.Enabled = false;
            IsQueryEPSAndLight = false;
            NewAddNum++;
            EscapeRoutesInfo infoEscapeRoutes = new EscapeRoutesInfo();
            CoordinateInfo infoCoordinate = new CoordinateInfo();

            Point p = e.GetPosition(cvsMainWindow);//imgFloorDrawingLogin

            DragFloorLogin = e.GetPosition(ctcFloorDrawingLogin);
            OriginDragFloorLogin = TransformGroupLogin.Inverse.Transform(DragFloorLogin);

            #region 计算登录后转换坐标
            //计算转换后的坐标:当前在图纸上的坐标加上起始坐标再加上图标原始大小跟变化后大小的差距
            DragFloorLogin = new Point(DragFloorLogin.X + 223 - (Math.Round(10 * ScaleTransformLogin.ScaleX) - 10) / FixedScaleTransform, DragFloorLogin.Y + 135 - (Math.Round(10 * ScaleTransformLogin.ScaleX) - 10) / FixedScaleTransform);
            OriginDragFloorLogin = new Point(OriginDragFloorLogin.X + 223, OriginDragFloorLogin.Y
             + 135);

            DragFloorLogin = new Point(Math.Round(DragFloorLogin.X), Math.Round(DragFloorLogin.Y));
            OriginDragFloorLogin = new Point(Math.Round(OriginDragFloorLogin.X), Math.Round(OriginDragFloorLogin.Y));
            #endregion

            DragFloorLogin = p;
            p = OriginDragFloorLogin;
            if (p.X >= 223 && p.X <= 960 && p.Y >= 135 && p.Y <= 584)
            {
                double x = p.X;
                double y = p.Y;
                Point NLPoint = ComputePointNoLogin(new Point(Math.Round(p.X), Math.Round(p.Y)));

                infoEscapeRoutes.StartingPoint = 0;
                infoEscapeRoutes.EndPoint = 0;
                infoEscapeRoutes.TurnIndex = LstEscapeRoutes.Count + 1;

                infoCoordinate.OriginX = Math.Round(p.X);
                infoCoordinate.OriginY = Math.Round(p.Y);
                infoCoordinate.NLOriginX = Math.Round(NLPoint.X);
                infoCoordinate.NLOriginY = Math.Round(NLPoint.Y);
                infoCoordinate.TransformX = Math.Round(DragFloorLogin.X);
                infoCoordinate.TransformY = Math.Round(DragFloorLogin.Y);
                infoCoordinate.Location = CurrentSelectFloorLogin;
                infoCoordinate.TableName = EnumClass.TableName.EscapeRoutes.ToString();
                infoCoordinate.TableID = ObjEscapeRoutes.Add(infoEscapeRoutes);
                ObjCoordinate.Add(infoCoordinate);
                //if (!LstEscapeRoutesCurrentFloorLogin.Contains(infoEscapeRoutes))
                //{
                //LstEscapeRoutesCurrentFloorLogin.Add(infoEscapeRoutes);
                //}
                LstEscapeRoutes = ObjEscapeRoutes.GetAll();
                LstCoordinate = ObjCoordinate.GetAll();
                LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(a => a.Location == CurrentSelectFloorLogin);
                AddTurningPointImage(infoCoordinate.TransformX, infoCoordinate.TransformY, NewAddNum);
                ShowEscapePointIndex();
            }
            //QueryEPSAndLightTimer.Enabled = true;
            IsQueryEPSAndLight = true;
        }

        private void ShowEscapePointIndex()
        {
            ObservableCollection<int> PointIndex = new ObservableCollection<int>();
            List<CoordinateInfo> LstCod = LstCoordinateCurrentFloorLogin.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString());
            for (int i = 0; i < LstCod.Count; i++)
            {
                PointIndex.Add(LstEscapeRoutes.Find(x => x.ID == LstCod[i].TableID).TurnIndex);
            }

            PointNum.ItemsSource = PointIndex;
            PointNum.SelectedIndex = 0;
        }

        private void CanelCoordinates_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LstEscapeRoutes.Count > 1)
            {
                gdFloorDrawingLogin.Children.RemoveAt(gdFloorDrawingLogin.Children.Count - 1);
                gdFloorDrawingLogin.Children.RemoveAt(gdFloorDrawingLogin.Children.Count - 1);
                ObjEscapeRoutes.Delete(LstEscapeRoutes[LstEscapeRoutes.Count - 1].ID);
                LstEscapeRoutes.Remove(LstEscapeRoutes[LstEscapeRoutes.Count - 1]);
            }
            else if (LstEscapeRoutes.Count == 1)
            {
                gdFloorDrawingLogin.Children.RemoveAt(gdFloorDrawingLogin.Children.Count - 1);
                ObjEscapeRoutes.Delete(LstEscapeRoutes[LstEscapeRoutes.Count - 1].ID);
                LstEscapeRoutes.Remove(LstEscapeRoutes[LstEscapeRoutes.Count - 1]);
            }
        }

        private void imgIconSearchCodeLogin_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (IsEditAllIcon)
                    {
                        if (MoveEquipment != null)
                        {
                            bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                            if (isSuccess)
                            {
                                ClearAllIconPanelLogin();
                                RefreshIconSearchCodeLogin();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgIconSearchCodeLogin_TouchMove(object sender, TouchEventArgs e)
        {
            try
            {
                if (IsEditAllIcon)
                {
                    if (MoveEquipment != null)
                    {
                        bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                        if (isSuccess)
                        {
                            ClearAllIconPanelLogin();
                            RefreshIconSearchCodeLogin();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgIconSearchCodeNoLogin_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                ClearShowIconSearchCodeInfoNoLogin(true);
                ShowIconSearCodeInfoNoLogin(((LayerImageTag)(sender as Image).Tag).equipment);
                MoveIconSearchCodeInfoPanelNoLogin(((LayerImageTag)(sender as Image).Tag).equipment);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgIconSearchCodeNoLogin_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                ClearShowIconSearchCodeInfoNoLogin(false);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgIconSearchCodeNoLogin_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                ClearShowIconSearchCodeInfoNoLogin(true);
                ShowIconSearCodeInfoNoLogin(((LayerImageTag)(sender as Image).Tag).equipment);
                MoveIconSearchCodeInfoPanelNoLogin(((LayerImageTag)(sender as Image).Tag).equipment);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgIconSearchCodeLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            if (e.RightButton == MouseButtonState.Pressed)
            {
                SetIsShowAllIconPanelLogin(true);
                ClearAllIconPanelLogin();
                ShowIconSearchCodePanelLogin(((LayerImageTag)(sender as Image).Tag).equipment, (sender as Image));
                MoveIconSearchCodeInfoPanelLogin(((LayerImageTag)(sender as Image).Tag).equipment);
                SetIsShowAllIconPanelLogin(false);
                //this.Homepage.Visibility = this.FunctionPage.Visibility = System.Windows.Visibility.Hidden;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsEditAllIcon)
                {
                    MoveEquipment = sender as Image;
                    //Console.WriteLine(MoveEquipment);
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    LoggerManager.WriteError(ex.ToString());
            //}
        }

        private void imgIconSearchCodeLogin_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                ctcFloorDrawingLogin_MouseMove(null, null);
            }
            catch (Exception ex)
            {
                LoggerManager.WriteError(ex.ToString());
            }
        }

        private void imgPartitionPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                ClearAllIconPanelLogin();
                //MovePartitionInfoPanelLogin((sender as Image).Tag as PlanPartitionPointRecordInfo);
                ShowPartitionPanelLogin((sender as Image).Tag as PlanPartitionPointRecordInfo);
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsEditAllIcon)
                {
                    MoveEquipment = sender as Image;
                }
            }
        }

        private void imgPartitionPoint_MouseMove(object sender, MouseEventArgs e)
        {
            MovePartitionPointLogin(sender, e);
            RefreshPartitionPointLogin();
        }

        private void imgPartitionPoint_TouchMove(object sender, TouchEventArgs e)
        {
            if (MoveEquipment != null)
            {
                MovePartitionPointLogin(sender, e);
                RefreshPartitionPointLogin();
            }
        }

        private void tbxIconSearchCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            InitIconSearchCodeListLogin();
        }

        private void btnImportFloorDrawing_Click(object sender, RoutedEventArgs e)
        {
            ImportFloorDrawing();
            bool isSuccess = IsLoadPicture;
            if (isSuccess)
            {
                LoadFloorDrawingLogin();
            }
        }

        private void btnEditAllIcon_Click(object sender, RoutedEventArgs e)
        {
            //先去掉事件后添加事件，防止事件叠加
            ctcFloorDrawingLogin.MouseDown -= ctcFloorDrawingLogin_MouseDown;
            ctcFloorDrawingLogin.MouseUp -= ctcFloorDrawingLogin_MouseUp;
            ctcFloorDrawingLogin.MouseMove -= ctcFloorDrawingLogin_MouseMove;
            ctcFloorDrawingLogin.TouchDown -= ctcFloorDrawingLogin_TouchDown;
            ctcFloorDrawingLogin.TouchUp -= ctcFloorDrawingLogin_TouchUp;
            ctcFloorDrawingLogin.TouchMove -= ctcFloorDrawingLogin_TouchMove;
            ctcFloorDrawingLogin.DragOver -= ctcFloorDrawingLogin_DragOver;
            ctcFloorDrawingLogin.Drop -= ctcFloorDrawingLogin_Drop;

            ctcFloorDrawingLogin.MouseDown += ctcFloorDrawingLogin_MouseDown;
            ctcFloorDrawingLogin.MouseUp += ctcFloorDrawingLogin_MouseUp;
            ctcFloorDrawingLogin.MouseMove += ctcFloorDrawingLogin_MouseMove;
            ctcFloorDrawingLogin.TouchDown += ctcFloorDrawingLogin_TouchDown;
            ctcFloorDrawingLogin.TouchUp += ctcFloorDrawingLogin_TouchUp;
            ctcFloorDrawingLogin.TouchMove += ctcFloorDrawingLogin_TouchMove;
            ctcFloorDrawingLogin.DragOver += ctcFloorDrawingLogin_DragOver;
            ctcFloorDrawingLogin.Drop += ctcFloorDrawingLogin_Drop;

            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;

            PointEdit.Visibility = AnalogLinkage.Visibility = System.Windows.Visibility.Hidden;
            //ScalingLayer.Enabled = true;
            SetIsEditAllIcon(true);
            InitIconSearchCodeListLogin();//初始化已有的数据图标列表
            EditAllIcon();
            ClearAllIconPanelLogin();
            InitPartitionLogin();
        }

        private void btnSaveAllIcon_Click(object sender, RoutedEventArgs e)
        {
            SetIsEditAllIcon(false);
            SaveAllIcon();
            ClearAllIconPanelLogin();
            ClearPartitionPointLogin();

            ctcFloorDrawingLogin.MouseDown -= ctcFloorDrawingLogin_MouseDown;
            ctcFloorDrawingLogin.MouseUp -= ctcFloorDrawingLogin_MouseUp;
            ctcFloorDrawingLogin.MouseMove -= ctcFloorDrawingLogin_MouseMove;
            ctcFloorDrawingLogin.TouchDown -= ctcFloorDrawingLogin_TouchDown;
            ctcFloorDrawingLogin.TouchUp -= ctcFloorDrawingLogin_TouchUp;
            ctcFloorDrawingLogin.TouchMove -= ctcFloorDrawingLogin_TouchMove;
            ctcFloorDrawingLogin.DragOver -= ctcFloorDrawingLogin_DragOver;
            ctcFloorDrawingLogin.Drop -= ctcFloorDrawingLogin_Drop;

            ctcFloorDrawingLogin.MouseDown += ctcFloorDrawingLogin_MouseDown;
            ctcFloorDrawingLogin.MouseUp += ctcFloorDrawingLogin_MouseUp;
            ctcFloorDrawingLogin.MouseMove += ctcFloorDrawingLogin_MouseMove;
            ctcFloorDrawingLogin.TouchDown += ctcFloorDrawingLogin_TouchDown;
            ctcFloorDrawingLogin.TouchUp += ctcFloorDrawingLogin_TouchUp;
            ctcFloorDrawingLogin.TouchMove += ctcFloorDrawingLogin_TouchMove;
            ctcFloorDrawingLogin.DragOver += ctcFloorDrawingLogin_DragOver;
            ctcFloorDrawingLogin.Drop += ctcFloorDrawingLogin_Drop;

            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
            ctcFloorDrawingLogin.MouseRightButtonDown -= CanelCoordinates_MouseRightButtonDown;

            PointEdit.Visibility = System.Windows.Visibility.Hidden;
            //ScalingLayer.Enabled = false;
        }

        private void SimulateFireAlarmLinkInLayer()
        {

            PointEdit.Visibility = System.Windows.Visibility.Hidden;
            AnalogLinkage.Visibility = System.Windows.Visibility.Visible;

            ObservableCollection<string> resPlanNum = new ObservableCollection<string>();
            List<int> PlanNum = new List<int>();
            for (int i = 0; i < LstDistributionBoxCurrentFloorLogin.Count; i++)
            {
                if (LstDistributionBoxCurrentFloorLogin[i].Plan1 != 0)
                {
                    PlanNum.Add(LstDistributionBoxCurrentFloorLogin[i].Plan1);
                }
                if (LstDistributionBoxCurrentFloorLogin[i].Plan2 != 0)
                {
                    PlanNum.Add(LstDistributionBoxCurrentFloorLogin[i].Plan2);
                }
                if (LstDistributionBoxCurrentFloorLogin[i].Plan3 != 0)
                {
                    PlanNum.Add(LstDistributionBoxCurrentFloorLogin[i].Plan3);
                }
                if (LstDistributionBoxCurrentFloorLogin[i].Plan4 != 0)
                {
                    PlanNum.Add(LstDistributionBoxCurrentFloorLogin[i].Plan4);
                }
                if (LstDistributionBoxCurrentFloorLogin[i].Plan5 != 0)
                {
                    PlanNum.Add(LstDistributionBoxCurrentFloorLogin[i].Plan5);
                }
            }
            PlanNum.Distinct();
            PlanNum.Sort();
            resPlanNum.Add("主联动");
            for (int i = 0; i < PlanNum.Count; i++)
            {
                resPlanNum.Add(PlanNum[i].ToString());
            }
            ResPlanNum.ItemsSource = resPlanNum;
            ResPlanNum.SelectedIndex = 0;

            ObservableCollection<string> FireAlarmType = new ObservableCollection<string>();
            for (int i = 0; i < LstFireAlarmType.Count; i++)
            {
                FireAlarmType.Add(LstFireAlarmType[i].FireAlarmName);
            }
            FireHostType.ItemsSource = FireAlarmType;
            FireHostType.SelectedIndex = 0;
            RefreshLayerModeLogin(true, true);
        }

        private void SimulateCancelLinkage()
        {
            AnalogLinkage.Visibility = System.Windows.Visibility.Hidden;
            SliderControlLogin.IsEnabled = true;
            IsShowDirection = false;//取消刷新逃生路线功能
            DelLines();
            RefreshLayerModeLogin(true, true);

            SetSimulateFireAlarmLinkExePlanTimer(false);
            IsRefreshProgressBar = true;//刷新进度条
            IsLinkageTiming = false;//停止计时
            //SetSimulateFireAlarmLinkTimer(false);
            SetIsEnterResetSystem(true);//进入复位
            AbsFireAlarmLink.SendHostBoardData(0X01);
            AbsFireAlarmLink.HostBoardSendStatus = 0X01;
            SetAllMainEleTimer(true);
            RecordSimulateFireAlarmLinkInfo();
            Resetting.Visibility = System.Windows.Visibility.Visible;
            ResettingProgressBar = ResetSystem;

            AllMainEle();
            GetEdition(IsCommodity);
            StopRefreshProgressBarValueTimer();
            
            AllEmergencyTotalTimer.Enabled = false;


            CurrentProgressBarValue = 0.0;
            RefreshProgressBarValueTimer.Enabled = true;
            ResettingProgressBar.Value = CurrentProgressBarValue;


            labSimulateLinkEmergencyTime.Content = "00:00:00";
            SetSimulateFireAlarmLinkPage(false);
            IsRefreshProgressBar = false;
            SetIsEnterResetSystem(false);//退出复位
            Thread.Sleep(9000);
            IsSimulationLinkage = false;
        }

        private void imgMuteSet_MouseDown1(object sender, MouseButtonEventArgs e)
        {
            ImageMuteSet1(sender as Image);
        }

        private void imgSillentProcessing_Click(object sender, MouseButtonEventArgs e)
        {
            string path = (sender as Image).Source.ToString();
            if (path.Contains("UnSelected"))
            {
                ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Selected.png", UriKind.Relative));
                ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Selected.png", UriKind.Relative));
                ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Selected.png", UriKind.Relative));

                SilentProcessing.Source = new BitmapImage(new Uri("\\Pictures\\SilentProcessing-Selected.png", UriKind.Relative));
            }
            else
            {
                ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Unchecked.png", UriKind.Relative));
                ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Unchecked.png", UriKind.Relative));
                ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Unchecked.png", UriKind.Relative));

                SilentProcessing.Source = new BitmapImage(new Uri("\\Pictures\\SilentProcessing-UnSelected.png", UriKind.Relative));
            }
        }

        private void CheckSetTime_GotFocus(object sender, RoutedEventArgs e)
        {
            GetFocusTime(sender);
        }

        /// <summary>
        /// 文本框接收到焦点时执行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FocusTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FocusTextBox != null)
            {
                FocusTextBox.Background = new SolidColorBrush(Colors.Transparent);
            }
            GetFocusTextBox(sender);
            FocusTextBox.Background = CommonFunct.GetBrush("#00A040");
            ShowSystemKeyBoard();

            if (LampNewCode.Visibility == System.Windows.Visibility.Visible)
            {
                LightInfo infoLight = new LightInfo
                {
                    Code = LampNewCode.Text,
                    LightClass = int.Parse(LampNewCode.Text.Substring(0, 1))
                };
                if (infoLight.LightClass == 5)
                {
                    if (infoLight.Code.Substring(1, 1) == "5")
                    {
                        infoLight.LightClass = 6;
                    }
                }
                Type.Content = CommonFunct.GetLightClass(infoLight);
            }
        }

        private void FocusLabelBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Label FocusLabelBox = sender as Label;
            KeyBoard KeyBoard = new KeyBoard(FocusLabelBox.Content.ToString());
            KeyBoard.ShowDialog();

            FocusLabelBox.Content = KeyBoard.TextBoxContent;
        }


        private void EPSCodeNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeEPSCodeBackgroundNoLogin(sender as Label);
            ShowEPSInfoNoLogin((sender as Label).Tag as DistributionBoxInfo);
        }

        private void EPSCode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeEPSCodeBackground(sender as Label);
            ShowEPSLamp((sender as Label).Tag as DistributionBoxInfo);
            LeftBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan4.GotFocus -= FocusTextBox_GotFocus;

        }

        private void EPSCodeLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeDisBoxCodeBackgroundLogin(sender as Label);
            ShowEPSInfoLogin((sender as Label).Tag as DistributionBoxInfo);
            EPSReservePlan1.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan2.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan3.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan4.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan5.GotFocus -= FocusTextBox_GotFocus;
        }

        private void LightCodeLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetLightCodeBackgroundLogin(sender as Label);
            ShowLightInfoLogin((sender as Label).Tag as LightInfo);
            GetImageLightBeginStatus();
            ChangeLightControlShow();
            LeftBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
        }

        private void LightCodeNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeLightCodeBackgroundNoLogin(sender as Label, false);
            MoveShowLightInfoPanel((sender as Label).TabIndex);
            ShowLightInfoNoLogin((sender as Label).Tag as LightInfo);
        }

        private void LightCodeNoLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangeLightCodeBackgroundNoLogin(sender as Label, true);
        }

        private void LightCodeNoLogin_TouchDown(object sender, TouchEventArgs e)
        {
            ChangeLightCodeBackgroundNoLogin(sender as Label, false);
            MoveShowLightInfoPanel((sender as Label).TabIndex);
            ShowLightInfoNoLogin(((sender as Label).Tag) as LightInfo);
        }

        private void LightCodeNoLogin_TouchUp(object sender, TouchEventArgs e)
        {
            ChangeLightCodeBackgroundNoLogin(sender as Label, true);
        }

        private void tbxLightInstallPos_GotFocus(object sender, RoutedEventArgs e)
        {
            LampsPositionText.Background = CommonFunct.GetBrush("#00A040");
            ShowSystemChineseKeyBoard((sender as TextBox));
        }

        private void tbxEPSInstall_GotFocus(object sender, RoutedEventArgs e)
        {
            EPSInitialPositionText.Background = CommonFunct.GetBrush("#00A040");
            ShowSystemChineseKeyBoard((sender as TextBox));
        }

        private void btnLightPlanSave_Click(object sender, RoutedEventArgs e)
        {
            SaveLightPlan(SelectInfoLightLogin, LeftBrightPlan1.Text, LeftBrightPlan2.Text, LeftBrightPlan3.Text, LeftBrightPlan4.Text, RightBrightPlan1.Text, RightBrightPlan2.Text, RightBrightPlan3.Text, RightBrightPlan4.Text);
            ShowLightInfoLogin(SelectInfoLightLogin);
            btnLightPlanCancel_Click(sender, e);
            CommonFunct.PopupWindow("修改灯具预案成功！");
        }

        private void LabelPageFloorNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //ClickCurrentFloor(sender);
            SwitchFloorNoLogin();
        }

        private async void btnSetCommunicationConfirm_Click(object sender, RoutedEventArgs e)
        {
            IsQueryEPSAndLight = false;
            AbsFireAlarmLink.IsHostThread = false;
            string SerialPortType = (sender as Button).Tag.ToString();
            LoggerManager.WriteDebug(SerialPortType);
            bool isCheckSetComAllSerialPort = await CheckSetComAllSerialPort(SerialPortType);
            //if (isCheckSetComAllSerialPort)
            //{
            //    Protocol.InitDisBoxSerialPort(this.cbxSetComDisBoxPort.SelectedValue.ToString());
            //    AbsFireAlarmLink.OpenHostBoardSerialPort(this.cbxSetComHostBoardPort.SelectedValue.ToString());
            //    //InitAllSerialPort();
            InitFireAlarm();
            //}
            IsQueryEPSAndLight = true;
            AbsFireAlarmLink.IsHostThread = true;
        }

        private void btnHideInitialEPSInfo_Click(object sender, RoutedEventArgs e)
        {
            HideInitialEPSInfo();
            EPSAndLightStatCount();
        }

        private void btnHideInitialLightInfo_Click(object sender, RoutedEventArgs e)
        {
            HideInitialLightInfo();
            EPSAndLightStatCount();
        }

        /// <summary>
        /// 设置是否消音
        /// </summary>
        private void SetIsMute()
        {
            GetMuteImage();
            IsMute = !IsMute;
            GetEdition(IsCommodity);

            if (IsMute)
            {
               
                AddHistoricalEvent("系统消音");
            }
            else
            {
                AddHistoricalEvent("取消消音");
            }
        }

        private void GetMuteImage()
        {
            Dispatcher.Invoke(() =>
            {
                if (IsMute)
                {
                    NoiseReduction.Source = new BitmapImage(new Uri("Pictures\\Silence.png", UriKind.Relative));
                    SilenceHomePage.Source = new BitmapImage(new Uri("Pictures\\Silence.png", UriKind.Relative));
                    //WriteToEventLog(string.Format("{0}   取消消音", DateTime.Now.ToString()));

                }
                else
                {
                    NoiseReduction.Source = new BitmapImage(new Uri("Pictures\\UnSilence.png", UriKind.Relative));
                    SilenceHomePage.Source = new BitmapImage(new Uri("Pictures\\UnSilence.png", UriKind.Relative));
                    //WriteToEventLog(string.Format("{0}   系统消音", DateTime.Now.ToString()));
                }
            });
        }

        /// <summary>
        /// 设置是否自动火灾联动接收
        /// </summary>
        private void SetIsAutoFireAlarmLink(bool isAutoFireAlarmLink)
        {
            if (isAutoFireAlarmLink)
            {
                AutomaticReceptionYes.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                AutomaticReceptionNo.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
            }
            else
            {
                AutomaticReceptionYes.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
                AutomaticReceptionNo.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// 设置是否联动归一
        /// </summary>      
        private void SetIsFireAlarmLinkNormal(bool isFireAlarmLinkNormal)
        {
            if (isFireAlarmLinkNormal)
            {
                LinkageUnificationYes.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                LinkageUnificationNo.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
            }
            else
            {
                LinkageUnificationYes.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
                LinkageUnificationNo.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// 保存EPS安装位置
        /// </summary>
        private void SaveEPSInstallPos()
        {
            if (SelectInfoEPSLogin != null)
            {
                SelectInfoEPSLogin.Address = EPSInitialPositionText.Text;
                ObjDistributionBox.Update(SelectInfoEPSLogin);
                CommonFunct.PopupWindow("修改EPS地址成功！");
            }
            else
            {
                CommonFunct.PopupWindow("修改的EPS为空！");
            }
        }

        /// <summary>
        /// 保存EPS预案
        /// </summary>
        private void SaveEPSPlan(DistributionBoxInfo infoDistributionBox, string Plan1, string Plan2, string Plan3, string Plan4, string Plan5)
        {
            if (infoDistributionBox != null)
            {
                IsTimingQueryEPSOrLight = false;
                while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeCommonInstructSleepTime);
                    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                }
                CurrentExeInstructTime = 0;

                infoDistributionBox.Plan1 = Convert.ToInt32(Plan1);
                infoDistributionBox.Plan2 = Convert.ToInt32(Plan2);
                infoDistributionBox.Plan3 = Convert.ToInt32(Plan3);
                infoDistributionBox.Plan4 = Convert.ToInt32(Plan4);
                infoDistributionBox.Plan5 = Convert.ToInt32(Plan5);
                ObjDistributionBox.Update(infoDistributionBox);

                Protocol.UpdateEPSPlan(infoDistributionBox);

                IsTimingQueryEPSOrLight = true;
            }
        }

        /// <summary>
        /// 保存灯具安装位置
        /// </summary>
        private void SaveLightInstallPos()
        {
            if (SelectInfoLightLogin != null)
            {
                SelectInfoLightLogin.Address = LampsPositionText.Text;
                ObjLight.Update(SelectInfoLightLogin);
                //LstLight = ObjLight.GetAll();
                int index = LstLight.FindIndex(x => x.ID == SelectInfoLightLogin.ID);
                LstLight[index] = SelectInfoLightLogin;
                CommonFunct.PopupWindow("修改灯具地址成功！");
            }
        }

        /// <summary>
        /// 保存灯具初始状态
        /// </summary>
        private void SaveLightBeginStatus()
        {
            if (SelectInfoLightLogin.ID != 0)
            {
                IsTimingQueryEPSOrLight = false;
                while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeCommonInstructSleepTime);
                    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                }
                CurrentExeInstructTime = 0;

                SelectInfoLightLogin.BeginStatus = GetDataLightBeginStatus();
                ObjLight.Update(SelectInfoLightLogin);

                Protocol.SetLightBeginStatus(SelectInfoLightLogin.Code, SelectInfoLightLogin.BeginStatus, SelectInfoEPSLogin.Code);

                IsTimingQueryEPSOrLight = true;
                CommonFunct.PopupWindow("修改灯具初始状态成功！");
            }
        }

        /// <summary>
        /// 保存灯具预案
        /// </summary>
        private void SaveLightPlan(LightInfo infoLight, string PlanLeft1, string PlanLeft2, string PlanLeft3, string PlanLeft4, string PlanRight1, string PlanRight2, string PlanRight3, string PlanRight4)
        {
            if (infoLight != null && infoLight.LightClass != (int)EnumClass.LightClass.照明灯 && infoLight.LightClass != (int)EnumClass.LightClass.双头灯)
            {
                IsTimingQueryEPSOrLight = false;
                while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeCommonInstructSleepTime);
                    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                }
                CurrentExeInstructTime = 0;

                infoLight.PlanLeft1 = (PlanLeft1 != string.Empty && CommonFunct.IsNumeric(PlanLeft1)) ? Convert.ToInt32(PlanLeft1) : 0;
                infoLight.PlanLeft2 = (PlanLeft2 != string.Empty && CommonFunct.IsNumeric(PlanLeft2)) ? Convert.ToInt32(PlanLeft2) : 0;
                infoLight.PlanLeft3 = (PlanLeft3 != string.Empty && CommonFunct.IsNumeric(PlanLeft3)) ? Convert.ToInt32(PlanLeft3) : 0;
                infoLight.PlanLeft4 = (PlanLeft4 != string.Empty && CommonFunct.IsNumeric(PlanLeft4)) ? Convert.ToInt32(PlanLeft4) : 0;
                infoLight.PlanRight1 = (PlanRight1 != string.Empty && CommonFunct.IsNumeric(PlanRight1)) ? Convert.ToInt32(PlanRight1) : 0;
                infoLight.PlanRight2 = (PlanRight2 != string.Empty && CommonFunct.IsNumeric(PlanRight2)) ? Convert.ToInt32(PlanRight2) : 0;
                infoLight.PlanRight3 = (PlanRight3 != string.Empty && CommonFunct.IsNumeric(PlanRight3)) ? Convert.ToInt32(PlanRight3) : 0;
                infoLight.PlanRight4 = (PlanRight4 != string.Empty && CommonFunct.IsNumeric(PlanRight4)) ? Convert.ToInt32(PlanRight4) : 0;
                ObjLight.Update(infoLight);

                Protocol.SetLightLeftPlan(infoLight);
                if (infoLight.LightClass == (int)EnumClass.LightClass.双向标志灯 || infoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                {
                    Protocol.SetLightRightPlan(infoLight);
                }

                IsTimingQueryEPSOrLight = true;
            }
        }

        /// <summary>
        /// 执行月检
        /// </summary>
        private bool ExecuteMonthCheck()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextMonthCheckTime");
            DateTime dtNow = DateTime.Now;
            DateTime dtMonthNextTime = Convert.ToDateTime(infoGblSetting.SetValue);

            if (dtNow.Month == dtMonthNextTime.Month && dtNow.Day == dtMonthNextTime.Day && dtNow.Hour == dtMonthNextTime.Hour && dtNow.Minute == dtMonthNextTime.Minute && dtNow.Second >= dtMonthNextTime.Second)
            {
                infoGblSetting.SetValue = dtMonthNextTime.AddMonths(int.Parse(dtNow.Month.ToString()) - int.Parse(dtMonthNextTime.Month.ToString()) + 1).ToString();//执行完一次后月份加一
                ObjGblSetting.Update(infoGblSetting);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 执行年检
        /// </summary>
        private bool ExecuteSeasonCheck()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            DateTime dtNow = DateTime.Now;
            DateTime dtSeasonNextTime = Convert.ToDateTime(infoGblSetting.SetValue);
            if (dtNow.Year == dtSeasonNextTime.Year && dtNow.Month == dtSeasonNextTime.Month && dtNow.Day == dtSeasonNextTime.Day && dtNow.Hour == dtSeasonNextTime.Hour && dtNow.Minute == dtSeasonNextTime.Minute && dtNow.Second >= dtSeasonNextTime.Second)
            {
                infoGblSetting.SetValue = dtSeasonNextTime.AddMonths(3).ToString();
                ObjGblSetting.Update(infoGblSetting);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 模拟联动执行预案
        /// </summary>
        private void SimulateFireAlarmLinkExePlan()
        {
            if (!IsEnterResetSystem && SimulateFireAlarmLinkExePlanTimer.Enabled && IsSimulateFireAlarmLink)
            {
                IsSimulateFireAlarmLink = false;
                if (IsRealFireAlarmLink)
                {
                    IsTimingQueryEPSOrLight = false;
                    IsQueryEPSAndLight = false;
                    if (_fireAlarmType == "GST5000H")
                    {
                        Protocol.SendDataExecutePlanByAllLight(5);
                    }
                    else
                    {
                        GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
                        bool isFireAlarmLinkNormal = Convert.ToBoolean(infoGblSetting.SetValue);


                        if (isFireAlarmLinkNormal)
                        {
                            Protocol.ExecutePlanByAllLight(FireAlarmLinkNormalZoneNumber);
                        }
                        else
                        {
                            List<FireAlarmZoneInfo> LstFireAlarmZoneRealFireAlarmLink = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow);
                            foreach (FireAlarmZoneInfo infoFireAlarmZone in LstFireAlarmZoneRealFireAlarmLink)
                            {
                                if (SimulateFireAlarmLinkExePlanTimer.Enabled)
                                {
                                    Protocol.ExecutePlanByAllLight(infoFireAlarmZone.FireAlarmLinkZoneNumber);
                                    System.Windows.Forms.Application.DoEvents();
                                }
                            }
                        }
                    }
                    IsTimingQueryEPSOrLight = true;
                    IsQueryEPSAndLight = true;
                }
                else
                {
                    //IsTimingQueryEPSOrLight是否定时查询EPS和灯具数据
                    if (SimulateFireAlarmLinkExePlanTimer.Enabled)
                    {
                        IsTimingQueryEPSOrLight = false;

                        GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
                        bool isFireAlarmLinkNormal = Convert.ToBoolean(infoGblSetting.SetValue);
                        if (isFireAlarmLinkNormal)
                        {
                            Protocol.ExecutePlanByAllLight(FireAlarmLinkNormalZoneNumber);
                        }
                        else
                        {
                            Protocol.ExecutePlanByAllLight(SimulateFireAlarmLinkZoneNumber);
                        }
                        IsTimingQueryEPSOrLight = true;
                    }
                }
                IsSimulateFireAlarmLink = true;
            }
        }



        /// <summary>
        /// 记录模拟联动历史
        /// </summary>
        private void RecordSimulateFireAlarmLinkHistory()
        {
            WriteToLinkHistory(string.Format("{0}   模拟联动", DateTime.Now.ToString()));
            //模拟联动打印
            Printer.print("模拟联动");
        }

        /// <summary>
        /// 记录模拟联动信息
        /// </summary>
        private void RecordSimulateFireAlarmLinkInfo()
        {
            AddHistoricalEvent("模拟联动");
            WriteToLinkHistory(string.Format("{0}   模拟联动结束", DateTime.Now.ToString()));
            WriteToLinkHistory(string.Format("{0}   应急总时长{1}", DateTime.Now.ToString(), labSimulateLinkEmergencyTime.Content));
            AddHistoricalEvent(string.Format("模拟联动结束，应急总时长{0}", labSimulateLinkEmergencyTime.Content));
            //模拟联动打印
            Printer.print(string.Format("模拟联动结束，应急总时长{0}", labSimulateLinkEmergencyTime.Content));
        }

        /// <summary>
        /// 写入记录日志
        /// </summary>
        /// <param name="strMessage"></param>
        private Task WriteToEventLog(string strMessage)
        {
            Task task = Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    EventLog.Items.Insert(0, strMessage);
                });
            });
            return task;
        }

        /// <summary>
        /// 写入联动历史
        /// </summary>
        private void WriteToLinkHistory(string strMessage)
        {
            SimulationLinkageHistory.Items.Insert(0, strMessage);

        }

        /// <summary>
        /// 打开分区设置页面
        /// </summary>
        private void OpenPartitionSetPage()
        {
            PartitionSet PartitionSet = new PartitionSet(LstFireAlarmPartitionSet);
            PartitionSet.ShowDialog();
        }

        /// <summary>
        /// 设置模拟联动定时器
        /// </summary>
        private void SetSimulateFireAlarmLinkTimer(bool isEnabled)
        {
            //SimulateFireAlarmLinkTimer.Enabled = isEnabled;
        }

        /// <summary>
        /// 设置模拟联动页面
        /// </summary>
        private void SetSimulateFireAlarmLinkPage(bool isVisible)
        {
            if (isVisible)
            {
                //this.LinkageImage.Source = new BitmapImage(new Uri("\\Pictures\\LinkageBackground.jpg", UriKind.Relative));
                PlanNum.Content = SimulateFireAlarmLinkZoneNumber;
                FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                stpSimulateLink.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                stpSimulateLink.Visibility = System.Windows.Visibility.Hidden;
                Resetting.Visibility = System.Windows.Visibility.Hidden;
                FunctionPage.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 设置真实火灾联动计时定时器
        /// </summary>      
        private void SetRealFireAlarmLinkTimer(bool isEnabled)
        {
            //RealFireAlarmLinkTimer.Enabled = isEnabled;
        }

        /// <summary>
        /// 设置全体主电定时器
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetAllMainEleTimer(bool isEnabled)
        {
            AllMainEleTimer.Enabled = isEnabled;
        }

        /// <summary>
        /// 关闭真实联动页面
        /// </summary>
        private void SetRealFireAlarmLinkPage(bool isVisible, List<int> LstFireAlarmLinkZoneNumber)
        {

        }

        /// <summary>
        /// 打开应急页面
        /// </summary>
        private void OpenAllEmergencyPage()
        {
            if (IsKeyEmergency)
            {
                DetermineEmergency.Visibility = CancelOrReset.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                DetermineEmergency.Visibility = CancelOrReset.Visibility = System.Windows.Visibility.Visible;
            }
            if (FunctionPage.Visibility == System.Windows.Visibility.Hidden)
            {
                IsFunOpen = false;
                FunctionPage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                IsFunOpen = true;
            }

            CompulsoryEmergencyLogin.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 获取模拟联动分区号
        /// </summary>
        private void GetSimulateFireAlarmLinkZoneNumber()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
            bool isFireAlarmLinkNormal = Convert.ToBoolean(infoGblSetting.SetValue);
            if (isFireAlarmLinkNormal)
            {
                SimulateFireAlarmLinkZoneNumber = 5;
            }
            else
            {
                SimulateFireAlarmLinkZoneNumber = Convert.ToInt32(LinkageNumber.Content);
                SimulateFireAlarmLinkZoneNumber = SimulateFireAlarmLinkZoneNumber > LstFireAlarmZone.Count ? LstFireAlarmZone.Count : SimulateFireAlarmLinkZoneNumber;
            }
        }

        /// <summary>
        /// 清除应急计时
        /// </summary>
        private void ClearAllEmergencyCalcuTime()
        {
            AllEmergencyTotalTime = 0;
        }

        /// <summary>
        /// 计算模拟联动时间
        /// </summary>
        private void CalcuSimulateFireAlarmLinkTime()
        {
            AllEmergencyTotalTime++;
            labSimulateLinkEmergencyTime.Content = string.Format("{0}:{1}:{2}", CommonFunct.ForMatTime(AllEmergencyTotalTime / (TimeInterval * TimeInterval)), CommonFunct.ForMatTime((AllEmergencyTotalTime / TimeInterval) % TimeInterval), CommonFunct.ForMatTime(AllEmergencyTotalTime % TimeInterval));
        }

        /// <summary>
        /// 真实联动应急计时
        /// </summary>
        private void CalCuRealLinkFireAlarmLinkTime()
        {
            AllEmergencyTotalTime++;
            StrAllEmergencyTotalTime = string.Format("{0}:{1}:{2}", CommonFunct.ForMatTime(AllEmergencyTotalTime / (TimeInterval * TimeInterval)), CommonFunct.ForMatTime((AllEmergencyTotalTime / TimeInterval) % TimeInterval), CommonFunct.ForMatTime(AllEmergencyTotalTime % TimeInterval));
        }

        /// <summary>
        /// 计算全体应急总时长
        /// </summary>
        private void CalcuAllEmergencyTotalTime()
        {
            AllEmergencyTotalTime++;
            StrAllEmergencyTotalTime = string.Format("{0}:{1}:{2}", CommonFunct.ForMatTime(AllEmergencyTotalTime / (TimeInterval * TimeInterval)), CommonFunct.ForMatTime((AllEmergencyTotalTime / TimeInterval) % TimeInterval), CommonFunct.ForMatTime(AllEmergencyTotalTime % TimeInterval));
            EmergencyLoginTime.Content = StrAllEmergencyTotalTime;
        }

        /// <summary>
        /// 真实火灾联动请求
        /// </summary>
        private void RealFireAlarmLinkRequest()
        {
            SetRequestRealFireAlarmLinkCalcuTime++;
            if (SetRequestRealFireAlarmLinkCalcuTime == SetRequestRealFireAlarmLinkMaxTime)
            {
                SetRealFireAlarmLinkPage(false, null);
            }
        }

        /// <summary>
        /// 开启真实火灾联动
        /// </summary>
        //private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        //private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private void StartRealFireAlarmLink(List<int> LstFireAlarmLinkZoneNumber)
        {
            try
            {
                IsQueryEPSAndLight = false;
                IsTimingQueryEPSOrLight = false;
                PartitionPointCurrentFloorNoLoginNum = LstFireAlarmLinkZoneNumber.Count;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        GetRealFireAlarmLinkZoneNumber(LstFireAlarmLinkZoneNumber);
                        bool isAutoFireAlarmLink = CheckIsAutoFireAlarmLink();
                        if (isAutoFireAlarmLink)
                        {
                            btnAllEmergencyInLayer.IsEnabled = LoginInLayer.IsEnabled = false;
                            //ShowRealFireAlarmLinkInfoPanel();//显示联动信息面板
                            StartRealFireAlarmLinkEnvir(LstFireAlarmLinkZoneNumber);
                        }
                        else
                        {
                            if (IsRealFireAlarmLink)
                            {
                                StartRealFireAlarmLinkEnvir(LstFireAlarmLinkZoneNumber);
                            }
                            else
                            {
                                SetRealFireAlarmLinkPage(true, LstFireAlarmLinkZoneNumber);
                            }
                        }

                    }
                    finally
                    {
                        IsQueryEPSAndLight = true;
                        IsTimingQueryEPSOrLight = true;
                    }
                }));
            }
            catch (Exception ex)
            {
                LoggerManager.WriteDebug(ex.Message);
            }
            finally
            {
                IsQueryEPSAndLight = true;
                IsTimingQueryEPSOrLight = true;
                // semaphore.Release();
            }
        }

        /// <summary>
        /// 检查是否自动火灾联动
        /// </summary>        
        private bool CheckIsAutoFireAlarmLink()
        {
            LstGblSetting = ObjGblSetting.GetAll();
            bool isAutoFireAlarmLink = Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsAutoFireAlarmLink").SetValue);//是否自动接收联动请求
            if (!isAutoFireAlarmLink)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 开启真实火灾联动环境
        /// </summary>
        private void StartRealFireAlarmLinkEnvir(List<int> LstFireAlarmLinkZoneNumber)
        {
            RealFireAlarmLinkExecutePlan(LstFireAlarmLinkZoneNumber);//真实联动执行预案
            if (!IsRealFireAlarmLink)
            {
                SetIsAllMainEle(false);
                SetIsRealFireAlarmLink(true);
                SetAllEPSStatus(true);
                SetAllLightStatus(true);
                ClearAllEmergencyCalcuTime();//清除应急计时
                GetRealFireAlarmLinkCurrentFloor();//获取真实火灾发生楼层
                SetAllEPSStatus(true);
                SetAllLightStatus(true);
                SetSimulateFireAlarmLinkExePlanTimer(true);
            }
            FireLayerDisplay(null, null);
            RecordRealFireAlarmLinkInfo();//记录真实火灾联动信息
            stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
            ShowRealFireAlarmLinkInfoPanel();//显示联动信息面板
        }

        /// <summary>
        /// 获取火灾联动分区
        /// </summary>
        private void GetRealFireAlarmLinkZoneNumber(List<int> LstFireAlarmLinkZoneNumber)
        {
            foreach (int fireAlarmLinkZoneNumber in LstFireAlarmLinkZoneNumber)
            {
                FireAlarmZoneInfo infoFireAlarmZone =
                    LstFireAlarmZone.Find(x => x.FireAlarmLinkZoneNumber == fireAlarmLinkZoneNumber);
                infoFireAlarmZone.FireAlarmLinkStatCount++;
                infoFireAlarmZone.IsFireAlarmLinkNow = true;
            }
        }

        /// <summary>
        /// 真实联动执行预案
        /// </summary>
        private void RealFireAlarmLinkExecutePlan(List<int> LstFireAlarmLinkZoneNumber)
        {
            if (!IsEnterResetSystem)
            {
                IsTimingQueryEPSOrLight = false;
                IsQueryEPSAndLight = false;
                //while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                //{
                //    System.Windows.Forms.Application.DoEvents();
                //    Thread.Sleep(ExeCommonInstructSleepTime);
                //    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                //}
                //CurrentExeInstructTime = 0;

                if (_fireAlarmType == "GST5000H")
                {
                    if (!_fireAlarm.HasAlarmed)
                    {
                        Protocol.SendBatchDatSetSingleLight(TwoBrightLamp, LeftBrightLamp, RightBrightLamp);
                    }

                    for (int j = 0; j < ExecuteCommonStructTimes; j++)
                    {
                        Protocol.SendDataExecutePlanByAllLight(5);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                else
                {
                    GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
                    bool isFireAlarmLinkNormal = Convert.ToBoolean(infoGblSetting.SetValue);


                    if (isFireAlarmLinkNormal)
                    {
                        for (int j = 0; j < ExecuteCommonStructTimes; j++)
                        {
                            Protocol.ExecutePlanByAllLight(FireAlarmLinkNormalZoneNumber);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        //List<FireAlarmZoneInfo> LstFireAlarmZoneRealFireAlarmLink = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow);
                        foreach (int infoFireAlarmZone in LstFireAlarmLinkZoneNumber)
                        {
                            for (int j = 0; j < ExecuteCommonStructTimes; j++)
                            {
                                Protocol.ExecutePlanByAllLight(infoFireAlarmZone);
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                    }
                }
                GetEdition(IsCommodity);
                IsQueryEPSAndLight = true;
                IsTimingQueryEPSOrLight = true;
            }
        }

        /// <summary>
        /// 隧道双向标志灯方案
        /// </summary>
        private void RealFireAlarmLinkPlan()
        {
            if (!IsEnterResetSystem)
            {
                FireAlarmZoneInfo minInfoAlarmZone = new FireAlarmZoneInfo();
                FireAlarmZoneInfo maxInfoAlarmZone = new FireAlarmZoneInfo();
                IsTimingQueryEPSOrLight = false;
                while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(ExeCommonInstructSleepTime);
                    CurrentExeInstructTime += ExeCommonInstructSleepTime;
                }
                CurrentExeInstructTime = 0;

                List<FireAlarmZoneInfo> LstFireAlarmZoneRealFireAlarmLink = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow);
                foreach (FireAlarmZoneInfo infoFireAlarmZone in LstFireAlarmZoneRealFireAlarmLink)
                {
                    //for (int j = 0; j < ExecuteCommonStructTimes; j++)
                    //{
                    //    Protocol.ExecutePlanByAllLight(infoFireAlarmZone.FireAlarmLinkZoneNumber);

                    //    FireAlarmLampControl();
                    //}
                }

                IsTimingQueryEPSOrLight = true;
            }
        }

        /// <summary>
        /// 显示真实火灾联动信息面板
        /// </summary>
        private void ShowRealFireAlarmLinkInfoPanel()
        {
            LstFireAlarmLinkZoneNumber = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow).Select(x => x.FireAlarmLinkZoneNumber).ToList<int>();
            labRealFireAlarmLinkInfo.Text = string.Format("{0}区火灾", string.Join(",", LstFireAlarmLinkZoneNumber));
            stpRealFireAlarmLinkInfo.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 隐藏真实火灾联动信息面板
        /// </summary>
        private void HideRealFireAlarmLinkInfoPanel()
        {
            stpRealFireAlarmLinkInfo.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 获取真实火灾发生楼层
        /// </summary>
        private void GetRealFireAlarmLinkCurrentFloor()
        {
            LstFireAlarmLinkZoneNumber = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow).Select(x => x.FireAlarmLinkZoneNumber).ToList<int>();
            foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPlanPartitionPointRecord)
            {
                //try
                //{
                if (LstFireAlarmLinkZoneNumber?.Count > 0 && infoPlanPartitionPointRecord.PlanPartition == LstFireAlarmLinkZoneNumber[0])
                {
                    CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
                    if (infoCoordinate != null)
                    {
                        CurrentPageFloorNoLogin = (infoCoordinate.Location - 1) / PerPageFloorNoLogin + 1;
                        CurrentSelectFloorNoLogin = string.Format("{0}层", infoCoordinate.Location);
                    }
                    break;
                }
                //}
                //catch
                //{

                //}
            }
        }

        /// <summary>
        /// 添加报警点图标和逃生路线
        /// </summary>
        private void AddPartitionPointNoLogin()
        {
            foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPartitionPointCurrentFloorNoLogin)
            {
                FireAlarmZoneInfo infoFireAlarmZone = LstFireAlarmZone.Find(x => x.FireAlarmLinkZoneNumber == infoPlanPartitionPointRecord.PlanPartition);
                if (infoFireAlarmZone.IsFireAlarmLinkNow && infoFireAlarmZone.FireAlarmLinkStatCount == 1)
                {
                    CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
                    if (infoCoordinate != null)
                    {
                        if (infoCoordinate.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)))
                        {
                            FireAlarmLinkPoint FireAlarmLinkPoint = GetPartitionPointNoLogin(infoPlanPartitionPointRecord);
                            cvsMainWindow.Children.Add(FireAlarmLinkPoint);
                        }
                    }
                }
            }
            RefreshEscapeRoutesNoLogin();
        }

        /// <summary>
        /// 加载真实火灾发生楼层的报警点
        /// </summary>
        private void LoadPartitionPointNoLogin()
        {
            foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPartitionPointCurrentFloorNoLogin)
            {
                FireAlarmZoneInfo infoFireAlarmZone = LstFireAlarmZone.Find(x => x.FireAlarmLinkZoneNumber == infoPlanPartitionPointRecord.PlanPartition);

                if (infoFireAlarmZone.IsFireAlarmLinkNow && infoFireAlarmZone.FireAlarmLinkStatCount > 0)
                {
                    FireAlarmLinkPoint FireAlarmLinkPoint = GetPartitionPointNoLogin(infoPlanPartitionPointRecord);
                    cvsMainWindow.Children.Add(FireAlarmLinkPoint);
                }
            }
        }

        /// <summary>
        /// 记录真实火灾联动信息
        /// </summary>
        private void RecordRealFireAlarmLinkInfo()
        {
            List<int> LstFireAlarmLinkZoneNumber = LstFireAlarmZone.FindAll(x => x.IsFireAlarmLinkNow).Select(x => x.FireAlarmLinkZoneNumber).ToList<int>();
            AddHistoricalEvent(string.Format("{0}区火灾", string.Join(",", LstFireAlarmLinkZoneNumber)));
            //火灾联动信息打印
            string fireAlarmInfo = string.Format("{0}区火灾", string.Join(",", LstFireAlarmLinkZoneNumber));
            Printer.print(fireAlarmInfo);
        }

        /// <summary>
        /// 设置火灾自动接收联动
        /// </summary>
        private void SetIsAutoFireAlarmLink()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsAutoFireAlarmLink");
            infoGblSetting.SetValue = ((AutomaticReceptionYes.Source as BitmapImage).UriSource == new Uri("\\Pictures\\Selected.png", UriKind.Relative)) ? "true" : "false";
            ObjGblSetting.Update(infoGblSetting);
            CommonFunct.PopupWindow("设置火灾自动接收联动成功！");
        }

        /// <summary>
        /// 设置火灾是否联动归一
        /// </summary>
        private void SetIsFireAlarmLinkNormal()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
            infoGblSetting.SetValue = ((LinkageUnificationYes.Source as BitmapImage).UriSource == new Uri("\\Pictures\\Selected.png", UriKind.Relative)) ? "true" : "false";
            ObjGblSetting.Update(infoGblSetting);
            if (Convert.ToBoolean(infoGblSetting.SetValue))
            {
                CommonFunct.PopupWindow("设置火灾联动归一成功！");
            }
            else
            {
                CommonFunct.PopupWindow("关闭火灾联动归一成功！");
            }
        }

        /// <summary>
        /// 设置火灾报警器类型
        /// </summary>
        private void SetFireAlarmType()
        {
            // TODO 设置火灾报警器
            FireAlarmTypeInfo infoFireAlarmType = LstFireAlarmType.Find(x => x.IsCurrentFireAlarm == 1);
            infoFireAlarmType.IsCurrentFireAlarm = 0;

            infoFireAlarmType = LstFireAlarmType.Find(x => x.FireAlarmName == FireAlarmType.SelectedItem.ToString());
            infoFireAlarmType.IsCurrentFireAlarm = 1;
            ObjFireAlarmType.Save(LstFireAlarmType);
            Task.Run(InitFireAlarm).ConfigureAwait(false);
            CommonFunct.PopupWindow("火灾报警器类型修改成功！");
            //InitFireAlarm();
        }

        /// <summary>
        /// 记录系统自检信息
        /// </summary>
        private void RecordCheckSelfTestInfo()
        {
            WriteToEventLog(string.Format("{0}   进行系统自检！", DateTime.Now.ToString()));
        }

        /// <summary>
        /// 设置自检反馈页面
        /// </summary>
        private void SetCheckSelfTestFeedBackPage(bool isVisible)
        {
            bool IsHomepage = Homepage.Visibility == System.Windows.Visibility.Visible ? true : false;
            SelfTestResults.Visibility = System.Windows.Visibility.Hidden;
            if (isVisible)
            {
                if (IsHomepage)
                {
                    FunctionPage.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                if (IsHomepage)
                {
                    FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            stpCheckSelfTestFeedBack.Visibility = isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 设置自检反馈退出
        /// </summary>       
        private void SetExitCheckSelfTestFeedBack(bool isVisible)
        {
            btnExitCheckSelfTestFeedBack.Visibility = isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 系统自检
        /// </summary>
        private void SystemSelfCheck()
        {
            IsTimingQueryEPSOrLight = false;
            IsQueryEPSAndLight = false;
            IsCheckIndicatorLight = true;
            IsComEmergency = false;
            //bool IsExit = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            //Thread check = new Thread(() =>
            //{
            //if ((AbsFireAlarmLink.HostBoardReturnStatus & (byte)EnumClass.HostBoardStatus.自检按键) != 0)
            //{

            byte[] HostStatus = { 0x01, 0x10, 0x02, 0x20, 0x04, 0x08, 0x01 };//主电，备电，应急，充电,故障，语音，主电
            byte LastByte = AbsFireAlarmLink.HostBoardSendStatus;
            for (int i = 0; i <= 6; i++)//检测面板上指示灯以及音响器件功能
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(1000);
                AbsFireAlarmLink.HostBoardSendStatus = HostStatus[i];
                AbsFireAlarmLink.SendHostBoardData(HostStatus[i]);
            }
            AbsFireAlarmLink.HostBoardSendStatus = LastByte;
            AbsFireAlarmLink.SendHostBoardData(LastByte);
            for (int i = 0; i < ExecuteCommonStructTimes; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                Protocol.AllEmergency();//全体应急
            }
            IsQueryEPSAndLight = true;
            AbsFireAlarmLink.IsHostThread = true;
            IsCheckIndicatorLight = false;
            //}
            //IsCheckIndicatorLight = false;
            //});
            //check.Start();
            //check.IsBackground = true;

            string strCheckSelfTestFeedBack = "液晶显示正常\r\n";
            tbCheckSelfTestFeedBack.Text = strCheckSelfTestFeedBack;

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(SystemSelfCheckTimerInterval);

            strCheckSelfTestFeedBack += "时钟自检正常\r\n";
            tbCheckSelfTestFeedBack.Text = strCheckSelfTestFeedBack;

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(SystemSelfCheckTimerInterval);

            strCheckSelfTestFeedBack += "灯具数据自检正常\r\n";
            Thread.Sleep(SystemSelfCheckTimerInterval);

            //while (!IsExit)
            //{
            //    if (!IsCheckIndicatorLight)
            //    {
            FaultRecordInfo infoFaultRecord = new FaultRecordInfo();
            if (LstFaultRecord.FindAll(x => x.Subject == "主控器").Count == 0)
            {
                strCheckSelfTestFeedBack += "系统主机正常\r\n";
            }
            else
            {
                infoFaultRecord = LstFaultRecord.FindLast(x => x.Subject == "主控器");
                strCheckSelfTestFeedBack += "系统主机" + infoFaultRecord.Fault + "\r\n";
            }

            //判断故障记录里是否存在EPS故障
            if (LstFaultRecord.FindAll(x => x.Subject.Substring(0, 1) == "6" && x.ChildSubject == null).Count == 0 )
            {
                strCheckSelfTestFeedBack += "配电箱运行正常\r\n";
            }
            else
            {
                List<FaultRecordInfo> LstFault = LstFaultRecord.FindAll(x => x.Subject.Substring(0, 1) == "6");
                List<FaultRecordInfo> ReDuplicate = new List<FaultRecordInfo>();
                for (int i = 0; i < LstFault.Count; i++)
                {
                    if (i == 0)
                    {
                        ReDuplicate.Add(LstFault[i]);
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (LstFault[i].Subject != LstFault[j].Subject && i == j + 1)
                            {
                                ReDuplicate.Add(LstFault[i]);
                            }
                        }
                    }
                }

                for (int i = 0; i < ReDuplicate.Count; i++)
                {
                    if (ReDuplicate.Count > 10)
                    {
                        if (i < 10)
                        {
                            if (i == 10 - 1)
                            {
                                strCheckSelfTestFeedBack += ReDuplicate[i].Subject + "配电箱运行异常\r\n";
                            }
                            else
                            {
                                strCheckSelfTestFeedBack += ReDuplicate[i].Subject + ",";
                            }
                        }
                    }
                    else
                    {
                        if (i == ReDuplicate.Count - 1)
                        {
                            strCheckSelfTestFeedBack += ReDuplicate[i].Subject + "配电箱运行异常\r\n";
                        }
                        else
                        {
                            strCheckSelfTestFeedBack += ReDuplicate[i].Subject + ",";
                        }
                    }
                }
            }

            //判断故障记录里是否存在灯具故障
            if (LstFaultRecord.FindAll(x => x.Subject != "主控器" && x.ChildSubject != null).Count == 0)
            {
                strCheckSelfTestFeedBack += "灯具工作正常";

            }
            else
            {
                strCheckSelfTestFeedBack += LstFaultRecord.FindAll(x => x.Subject != "主控器" && x.ChildSubject != null).Count + "个灯具工作异常";

            }

            #region 旧版系统自检
            //OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description ==
            //this.ImgEPSFault.Tag.ToString());
            //if (infoOtherFaultRecord.IsExist == 1)
            //{
            //    strCheckSelfTestFeedBack += "EPS和灯具数据自检异常\r\n";
            //}
            //else
            //{
            //    infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description
            //    == this.ImgLampFault.Tag.ToString());
            //    if (infoOtherFaultRecord.IsExist == 1)
            //    {
            //        strCheckSelfTestFeedBack += "EPS和灯具数据自检异常\r\n";
            //    }
            //    else
            //    {
            //        strCheckSelfTestFeedBack += "EPS和灯具数据自检正常\r\n";
            //    }
            //}
            //this.tbCheckSelfTestFeedBack.Text = strCheckSelfTestFeedBack;

            //System.Windows.Forms.Application.DoEvents();
            //Thread.Sleep(HostBoardSystemSelfCheckTimerInterval);

            //infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description
            //== this.ImgHostFault.Tag.ToString());
            //if (infoOtherFaultRecord.IsExist == 1)
            //{
            //    strCheckSelfTestFeedBack += "主机板自检故障\r\n";
            //}
            //else
            //{
            //    strCheckSelfTestFeedBack += "主机板自检正常\r\n";
            //}
            //this.tbCheckSelfTestFeedBack.Text = strCheckSelfTestFeedBack;

            //System.Windows.Forms.Application.DoEvents();
            //Thread.Sleep(SystemSelfCheckTimerInterval);

            //strCheckSelfTestFeedBack += "系统自检完成！";
            #endregion
            tbCheckSelfTestFeedBack.Text = strCheckSelfTestFeedBack;

            for (int i = 0; i < ExecuteAllMainEleStructTime; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                Protocol.AllMainEle();
            }

            if (LstFaultRecord.Count == 0)
            {
                Result.Content = "系统正常！请退出！";
            }
            else
            {
                Result.Content = "系统异常！请检查！";
            }
            SelfTestResults.Visibility = System.Windows.Visibility.Visible;
            IsTimingQueryEPSOrLight = true;
            IsQueryEPSAndLight = true;
            //IsExit = true;//退出循环
            //    }
            //    System.Windows.Forms.Application.DoEvents();
            //    Thread.Sleep(100);
            //}
            IsTimingQueryEPSOrLight = true;
            IsQueryEPSAndLight = true;
        }

        /// <summary>
        /// 月检设置确定New
        /// </summary>
        private void MonthlyInspectionSetOK()
        {

            string NowYear = DateTime.Now.ToString("yyyy");
            string NowMonth = DateTime.Now.ToString("MM");
            string time = "";

            int month = 0;
            month = Convert.ToInt32(NowMonth);
            time = NowYear + "/" + NowMonth + "/";

            //DateTime dtMonthCheckSet = Convert.ToDateTime(this.dtMonthCheckSet.SelectedDate);
            string strMonthCheckSet = string.Format("{0}{1}  {2}:{3}:{4}", time, MonthlyDay.Text
                , MonthlyHour.Text, MonthlyMinute.Text, MonthlySecond.Text);
            
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextMonthCheckTime");
            infoGblSetting.SetValue = strMonthCheckSet;
            ObjGblSetting.Update(infoGblSetting);
            if (!Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsMonthlyInspection").SetValue))
            {
                CommonFunct.PopupWindow("月检已关闭");
            }
            else
            {
                CommonFunct.PopupWindow("已开始自动月检" + "\n" + "时间修改为" + "\n" + MonthlyDay.Text + "日" + MonthlyHour.Text + ":" + MonthlyMinute.Text + ":" + MonthlySecond.Text);
            }
        }

        private void YearlyInspectionSetOK()
        {
            string NowYear = DateTime.Now.ToString("yyyy");
            string strSeasonCheckSet = string.Format("{0}/{1}/{2}  {3}:{4}:{5}", NowYear, YearlyMonth.Text, YearlyDay.Text, YearlyHour.Text, YearlyMinute.Text, YearlySecond.Text);

            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            infoGblSetting.SetValue = strSeasonCheckSet;
            ObjGblSetting.Update(infoGblSetting);
            if (!Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection").SetValue))
            {
                CommonFunct.PopupWindow("年检已关闭");
            }
            else
            {
                CommonFunct.PopupWindow("已开始自动年检" + "\n" + "时间修改为" + "\n" + YearlyMonth.Text + "月" + YearlyDay.Text + "日" + YearlyHour.Text + ":" + YearlyMinute.Text + ":" + YearlySecond.Text);
            }
            ExecuteSeasonCheckTimer.Interval = CommonTimerInterval;
        }

        /// <summary>
        /// 开始手动月检
        /// </summary>
        private void StartMonthCheckByHand()
        {
            AddHistoricalEvent("进行月检");
        }

        /// <summary>
        /// 手动月检
        /// </summary>
        private bool MonthCheckByHand(string strMonthCheckTitle)
        {
            MonthCheck MonthCheck = new MonthCheck(strMonthCheckTitle);
            bool hasNewError = MonthCheck.ShowDialog() ?? false;
            if (hasNewError)
            {
                CommonFunct.PopupWindow("出现新故障，自检失败！");
            }
            return hasNewError;
        }

        /// <summary>
        /// 结束手动月检
        /// </summary>
        private void EndMonthCheckByHand(bool IsSucceed)
        {
            //LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            //LstLight.ForEach(x => x.IsEmergency = 0);
            //ObjDistributionBox.Save(LstDistributionBox);
            //ObjLight.Save(LstLight);
            FaultRecordInfo infoFault = new FaultRecordInfo();
            if (IsSucceed)
            {
                if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.ChildSubject == null && x.FaultType == EnumClass.FaultType.月检故障.ToString() && x.Fault == "月检故障") == null)
                {
                    infoFault.Subject = "主控器";
                    infoFault.ChildSubject = null;
                    infoFault.FaultType = EnumClass.FaultType.月检故障.ToString();
                    infoFault.Fault = "月检故障";
                    LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                }
                IsComEmergency = false;
                AddHistoricalEvent("月检失败");
                //打印月检故障
                Printer.print("月检失败");
            }
            else
            {
                infoFault = LstFaultRecord.Find(x => x.Subject == "主控器" && x.ChildSubject == null && x.FaultType == EnumClass.FaultType.月检故障.ToString() && x.Fault == "月检故障");
                if (infoFault != null)
                {
                    LstFaultRecord.Remove(infoFault);
                    ObjFaultRecord.Delete(infoFault.ID);
                }
                IsComEmergency = false;
                IsEmergency = false;
                AddHistoricalEvent("月检结束");
            }
        }

        /// <summary>
        /// 开始手动年检
        /// </summary>
        private void StartSeasonCheckByHand()
        {
            AddHistoricalEvent("进行年检");
        }

        /// <summary>
        /// 手动年检
        /// </summary>
        private bool SeasonCheckByHand()
        {
            SeasonCheck SeasonCheck = new SeasonCheck();
            bool hasNewError = SeasonCheck.ShowDialog() ?? false;
            if (hasNewError)
            {
                CommonFunct.PopupWindow("出现新故障，自检失败！");
            }
            return hasNewError;
        }

        /// <summary>
        /// 结束手动年检
        /// </summary>
        private void EndSeasonCheckByHand(bool IsSucceed)
        {
            //LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            //LstLight.ForEach(x => x.IsEmergency = 0);
            //ObjDistributionBox.Save(LstDistributionBox);
            //ObjLight.Save(LstLight);
            FaultRecordInfo infoFault = new FaultRecordInfo();
            if (IsSucceed)
            {
                if (LstFaultRecord.Find(x => x.Subject == "主控器" && x.ChildSubject == null && x.FaultType == EnumClass.FaultType.年检故障.ToString() && x.Fault == "年检故障") == null)
                {
                    infoFault.Subject = "主控器";
                    infoFault.ChildSubject = null;
                    infoFault.FaultType = EnumClass.FaultType.年检故障.ToString();
                    infoFault.Fault = "年检故障";
                    LstFaultRecord.Add(infoFault);
                    ObjFaultRecord.Add(infoFault);
                }
                IsComEmergency = false;
                AddHistoricalEvent("年检失败");
                //打印年检故障
                Printer.print("年检失败");
            }
            else
            {
                infoFault = LstFaultRecord.Find(x => x.Subject == "主控器" && x.ChildSubject == null && x.FaultType == EnumClass.FaultType.年检故障.ToString() && x.Fault == "年检故障");
                if (infoFault != null)
                {
                    LstFaultRecord.Remove(infoFault);
                    ObjFaultRecord.Delete(infoFault.ID);
                }
                AddHistoricalEvent("年检结束");
            }
        }

        /// <summary>
        /// 开始月检，年检加速
        /// </summary>
        private void StartMonthAndSeasonCheckSpeedUp()
        {
            AddHistoricalEvent("开始月检，年检加速");
        }

        /// <summary>
        /// 结束月检，年检加速
        /// </summary>
        private void EndMonthAndSeasonCheckSpeedUp()
        {
            AddHistoricalEvent("月检，年检加速完成");
        }

        private void CheckError()
        {
            AddHistoricalEvent("自检失败，请检查新故障！");
        }

        /// <summary>
        /// 进入下一楼层页面
        /// </summary>
        private bool NextPageFloorNoLogin()
        {
            if (CurrentPageFloorNoLogin >= TotalFloor)
            {
                return false;
            }

            CurrentPageFloorNoLogin++;
            CurrentSelectFloorNoLogin = CurrentPageFloorNoLogin + "层";
            return true;
        }

        /// <summary>
        /// 进入下一楼层页面
        /// </summary>
        private bool NextPageFloorLogin()
        {
            if (CurrentSelectFloorLogin >= TotalFloor)
            {
                return false;
            }

            CurrentSelectFloorLogin++;
            return true;
        }

        /// <summary>
        /// 返回上一楼层页面
        /// </summary>
        private bool LastPageFloorNoLogin()
        {
            if (CurrentPageFloorNoLogin == 1)
            {
                return false;
            }
            CurrentPageFloorNoLogin--;
            CurrentSelectFloorNoLogin = CurrentPageFloorNoLogin + "层";
            return true;
        }

        /// <summary>
        /// 返回上一个楼层页面
        /// </summary>
        private bool LastPageFloorLogin()
        {
            if (CurrentSelectFloorLogin == 1)
            {
                return false;
            }

            CurrentSelectFloorLogin--;
            return true;
        }

        /// <summary>
        /// 修改总楼层数
        /// </summary>
        private bool ModifyTotalFloor()
        {
            string strTotalFloor = FloorTotalNum.Text;
            if (strTotalFloor == string.Empty)
            {
                System.Windows.MessageBox.Show("请填写总楼层数！", "提示");
                return false;
            }

            bool isNumeric = CommonFunct.IsNumeric(strTotalFloor);
            if (!isNumeric)
            {
                System.Windows.MessageBox.Show("总楼层数格式错误，请重新填写！", "提示");
                return false;
            }

            int totalFloor = Convert.ToInt32(strTotalFloor);
            if (totalFloor < 1)
            {
                System.Windows.MessageBox.Show("总楼层数不能小于1！", "提示");
                return false;
            }

            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("是否确认修改总楼层数？", "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.No)
            {
                return false;
            }

            TotalFloor = totalFloor;
            CurrentSelectFloorLogin = 1;
            TotalFloorNum.Content = TotalFloor;

            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "ConstructionFloor");
            infoGblSetting.SetValue = strTotalFloor;
            ObjGblSetting.Update(infoGblSetting);
            CommonFunct.PopupWindow("修改总楼层数成功！");
            return true;
        }

        /// <summary>
        /// 进入选中的楼层
        /// </summary>
        private bool JumpSelectFloorNoLogin()
        {
            string strJumpSelectFloor = tbxJumpSelectFloorNoLogin.Text;
            if (strJumpSelectFloor == string.Empty)
            {
                System.Windows.MessageBox.Show("请填写要进入的楼层！", "提示");
                return false;
            }

            bool isNumeric = CommonFunct.IsNumeric(strJumpSelectFloor);
            if (!isNumeric)
            {
                System.Windows.MessageBox.Show("要进入的楼层格式错误，请重新填写！", "提示");
                return false;
            }

            int jumpSelectByFloor = Convert.ToInt32(strJumpSelectFloor);
            if (jumpSelectByFloor == 0)
            {
                System.Windows.MessageBox.Show("要进入的楼层不能为0！", "提示");
                return false;
            }

            if (jumpSelectByFloor > TotalFloor)
            {
                System.Windows.MessageBox.Show("要进入的楼层不能大于总楼层数！", "提示");
                return false;
            }

            CurrentSelectFloorNoLogin = string.Format("{0}层", jumpSelectByFloor);
            //CurrentPageFloorNoLogin = (jumpSelectByFloor - 1) / PerPageFloorNoLogin + 1;
            return true;
        }

        /// <summary>
        /// 进入选中的楼层
        /// </summary>
        private bool JumpSelectFloorLogin()
        {
            string strJumpSelectFloor = tbxJumpSelectFloorLogin.Text;
            if (strJumpSelectFloor == string.Empty)
            {
                System.Windows.MessageBox.Show("请填写要进入的楼层！", "提示");
                return false;
            }

            bool isNumeric = CommonFunct.IsNumeric(strJumpSelectFloor);
            if (!isNumeric)
            {
                System.Windows.MessageBox.Show("要进入的楼层格式错误，请重新填写！", "提示");
                return false;
            }

            int jumpSelectByFloor = Convert.ToInt32(strJumpSelectFloor);
            if (jumpSelectByFloor == 0)
            {
                System.Windows.MessageBox.Show("要进入的楼层不能为0！", "提示");
                return false;
            }

            if (jumpSelectByFloor > TotalFloor)
            {
                System.Windows.MessageBox.Show("要进入的楼层不能大于总楼层数！", "提示");
                return false;
            }

            CurrentSelectFloorLogin = jumpSelectByFloor;
            return true;
        }

        /// <summary>
        /// 放大缩小图纸
        /// </summary>       
        private bool ScaleTransformDrawingNoLogin(RoutedPropertyChangedEventArgs<double> e)
        {
            Point PointToContent = TransformGroupNoLogin.Inverse.Transform(FloorDrawingCenterNoLogin);
            double scaleX = Math.Round(ScaleTransformNoLogin.ScaleX + (e.NewValue - e.OldValue) * Delta, 2);
            double scaleY = Math.Round(ScaleTransformNoLogin.ScaleY + (e.NewValue - e.OldValue) * Delta, 2);
            if ((scaleX < MinScaleTransform && scaleY < MinScaleTransform) || (scaleX > MaxScaleTransform && scaleY > MaxScaleTransform))
            {
                return false;
            }

            ScaleTransformNoLogin.ScaleX = scaleX;
            ScaleTransformNoLogin.ScaleY = scaleY;
            IconSearchCodeSizeNoLogin = Math.Round(OriginIconSearchCodeSize * ScaleTransformNoLogin.ScaleX);

            double translateTransformX = -1 * (PointToContent.X * ScaleTransformNoLogin.ScaleX - FloorDrawingCenterLogin.X);
            double translateTransformY = -1 * (PointToContent.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingCenterLogin.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformNoLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingPosition.Y);
            if (translateTransformX > 0)
            {
                TranslateTransformNoLogin.X = 0;
            }
            else if (translateTransformX <= 0 && translateTransformX > minTranslateTransformX)
            {
                TranslateTransformNoLogin.X = translateTransformX;
            }
            else
            {
                TranslateTransformNoLogin.X = minTranslateTransformX;
            }

            if (translateTransformY > 0)
            {
                TranslateTransformNoLogin.Y = 0;
            }
            else if (translateTransformY <= 0 && translateTransformY > minTranslateTransformY)
            {
                TranslateTransformNoLogin.Y = translateTransformY;
            }
            else
            {
                TranslateTransformNoLogin.Y = minTranslateTransformY;
            }
            return true;
        }

        private void ScaleTransformDrawingNoLogin()
        {
            Point PointToContent = TransformGroupNoLogin.Inverse.Transform(FloorDrawingCenterLogin);
            double scaleX = Math.Round(ScaleTransformNoLogin.ScaleX + 1.16, 2);
            double scaleY = Math.Round(ScaleTransformNoLogin.ScaleY + 1.16, 2);

            ScaleTransformNoLogin.ScaleX = scaleX;
            ScaleTransformNoLogin.ScaleY = scaleY;
            IconSearchCodeSizeNoLogin = Math.Round(OriginIconSearchCodeSize * ScaleTransformNoLogin.ScaleX);

            double translateTransformX = -1 * (PointToContent.X * ScaleTransformNoLogin.ScaleX - FloorDrawingCenterLogin.X);
            double translateTransformY = -1 * (PointToContent.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingCenterLogin.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformNoLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingPosition.Y);
            if (translateTransformX > 0)
            {
                TranslateTransformNoLogin.X = 0;
            }
            else if (translateTransformX <= 0 && translateTransformX > minTranslateTransformX)
            {
                TranslateTransformNoLogin.X = translateTransformX;
            }
            else
            {
                TranslateTransformNoLogin.X = minTranslateTransformX;
            }

            if (translateTransformY > 0)
            {
                TranslateTransformNoLogin.Y = 0;
            }
            else if (translateTransformY <= 0 && translateTransformY > minTranslateTransformY)
            {
                TranslateTransformNoLogin.Y = translateTransformY;
            }
            else
            {
                TranslateTransformNoLogin.Y = minTranslateTransformY;
            }

        }

        /// <summary>
        /// 放大缩小图纸
        /// </summary>     
        private bool ScaleTransformDrawingLogin(RoutedPropertyChangedEventArgs<double> e)
        {
            Point PointToContent = TransformGroupLogin.Inverse.Transform(FloorDrawingCenterLogin);
            double scaleX = Math.Round(ScaleTransformLogin.ScaleX + (e.NewValue - e.OldValue) * Delta, 2);
            double scaleY = Math.Round(ScaleTransformLogin.ScaleY + (e.NewValue - e.OldValue) * Delta, 2);
            if ((scaleX < MinScaleTransform && scaleY < MinScaleTransform) || (scaleX > MaxScaleTransform && scaleY > MaxScaleTransform))
            {
                return false;
            }

            ScaleTransformLogin.ScaleX = scaleX;
            ScaleTransformLogin.ScaleY = scaleY;
            IconSearchCodeSizeLogin = Math.Round(OriginIconSearchCodeSize * ScaleTransformLogin.ScaleX);

            double translateTransformX = -1 * (PointToContent.X * ScaleTransformLogin.ScaleX - FloorDrawingCenterLogin.X);
            double translateTransformY = -1 * (PointToContent.Y * ScaleTransformLogin.ScaleY - FloorDrawingCenterLogin.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformLogin.ScaleY - FloorDrawingPosition.Y);
            if (translateTransformX > 0)
            {
                TranslateTransformLogin.X = 0;
            }
            else if (translateTransformX <= 0 && translateTransformX > minTranslateTransformX)
            {
                TranslateTransformLogin.X = translateTransformX;
            }
            else
            {
                TranslateTransformLogin.X = minTranslateTransformX;
            }

            if (translateTransformY > 0)
            {
                TranslateTransformLogin.Y = 0;
            }
            else if (translateTransformY <= 0 && translateTransformY > minTranslateTransformY)
            {
                TranslateTransformLogin.Y = translateTransformY;
            }
            else
            {
                TranslateTransformLogin.Y = minTranslateTransformY;
            }
            return true;
        }

        /// <summary>
        /// 平移图纸
        /// </summary>       
        private bool TranslateTransformDrawingNoLogin(MouseEventArgs e)
        {
            if (LastDragFloorNoLogin == new Point(0, 0))
            {
                return false;
            }

            Point PointPosition = e.GetPosition(ctcFloorDrawingNoLogin);
            double translateTransformX = Math.Round(TranslateTransformNoLogin.X, 2) - (DragFloorNoLogin.X - PointPosition.X);
            double translateTransformY = Math.Round(TranslateTransformNoLogin.Y, 2) - (DragFloorNoLogin.Y - PointPosition.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformNoLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingPosition.Y);
            if (Math.Round(ScaleTransformNoLogin.ScaleX, 2) <= MinScaleTransform)
            {
                return false;
            }

            if (translateTransformX >= 0 || translateTransformX <= minTranslateTransformX || translateTransformY >= 0 || translateTransformY <= minTranslateTransformY)
            {
                return false;
            }

            TranslateTransformNoLogin.X = translateTransformX;
            TranslateTransformNoLogin.Y = translateTransformY;
            DragFloorNoLogin = PointPosition;
            return true;
        }

        /// <summary>
        /// 平移图纸
        /// </summary>     
        private bool TranslateTransformDrawingNoLogin(TouchEventArgs e)
        {
            if (LastDragFloorNoLogin == new Point(0, 0))
            {
                return false;
            }

            Point PointPosition = e.GetTouchPoint(ctcFloorDrawingNoLogin).Position;
            double translateTransformX = Math.Round(TranslateTransformNoLogin.X, 2) - (DragFloorNoLogin.X - PointPosition.X);
            double translateTransformY = Math.Round(TranslateTransformNoLogin.Y, 2) - (DragFloorNoLogin.Y - PointPosition.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformNoLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformNoLogin.ScaleY - FloorDrawingPosition.Y);
            if (Math.Round(ScaleTransformNoLogin.ScaleX) <= MinScaleTransform)
            {
                return false;
            }

            if (translateTransformX >= 0 || translateTransformX <= minTranslateTransformX || translateTransformY >= 0 || translateTransformY <= minTranslateTransformY)
            {
                return false;
            }

            TranslateTransformNoLogin.X = translateTransformX;
            TranslateTransformNoLogin.Y = translateTransformY;
            DragFloorNoLogin = PointPosition;
            return true;
        }

        /// <summary>
        /// 平移图纸
        /// </summary>
        private bool TranslateTransformDrawingLogin(MouseEventArgs e)
        {
            if (LastDragFloorLogin == new Point(0, 0))
            {
                return false;
            }

            Point PointPosition = e.GetPosition(ctcFloorDrawingLogin);
            double translateTransformX = Math.Round(TranslateTransformLogin.X, 2) - (DragFloorLogin.X - PointPosition.X);
            double translateTransformY = Math.Round(TranslateTransformLogin.Y, 2) - (DragFloorLogin.Y - PointPosition.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformLogin.ScaleY - FloorDrawingPosition.Y);
            if (Math.Round(ScaleTransformLogin.ScaleX, 2) <= MinScaleTransform)
            {
                return false;
            }

            if (translateTransformX >= 0 || translateTransformX <= minTranslateTransformX || translateTransformY >= 0 || translateTransformY <= minTranslateTransformY)
            {
                return false;
            }

            TranslateTransformLogin.X = translateTransformX;
            TranslateTransformLogin.Y = translateTransformY;
            DragFloorLogin = PointPosition;
            return true;
        }

        /// <summary>
        /// 平移图纸
        /// </summary>        
        private bool TranslateTransformDrawingLogin(TouchEventArgs e)
        {
            if (LastDragFloorLogin == new Point(0, 0) || IsShowIconSearchCodePanelLogin)
            {
                return false;
            }

            Point PointPosition = e.GetTouchPoint(ctcFloorDrawingLogin).Position;
            double translateTransformX = Math.Round(TranslateTransformLogin.X, 2) - (DragFloorLogin.X - PointPosition.X);
            double translateTransformY = Math.Round(TranslateTransformLogin.Y, 2) - (DragFloorLogin.Y - PointPosition.Y);
            double minTranslateTransformX = -1 * (FloorDrawingPosition.X * ScaleTransformLogin.ScaleX - FloorDrawingPosition.X);
            double minTranslateTransformY = -1 * (FloorDrawingPosition.Y * ScaleTransformLogin.ScaleY - FloorDrawingPosition.Y);
            if (Math.Round(ScaleTransformLogin.ScaleX, 2) <= MinScaleTransform)
            {
                return false;
            }

            if (translateTransformX >= 0 || translateTransformX <= minTranslateTransformX || translateTransformY >= 0 || translateTransformY <= minTranslateTransformY)
            {
                return false;
            }

            TranslateTransformLogin.X = translateTransformX;
            TranslateTransformLogin.Y = translateTransformY;
            DragFloorLogin = PointPosition;
            return true;
        }

        /// <summary>
        /// 删除业务数据
        /// </summary>
        private void DeleteBusinessData()
        {
            OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgEPSFault.Tag.ToString());
            infoOtherFaultRecord.IsExist = 0;
            ObjOtherFaultRecord.Update(infoOtherFaultRecord);

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgLampFault.Tag.ToString());
            infoOtherFaultRecord.IsExist = 0;
            ObjOtherFaultRecord.Update(infoOtherFaultRecord);


            ObjDistributionBox.DeleteAll();//清除EPS信息
            ObjLight.DeleteAll();//清除灯具信息
            ObjBlankIcon.DeleteAll();//清除空白控件
            ObjCoordinate.DeleteAll();//清除坐标数据
            ObjPlanPartitionPointRecord.DeleteAll();//清除EPS和灯具在图形界面上的位置
            ObjFaultRecord.DeleteAll();//清除故障信息
            ObjHistoricalEvent.DeleteAll();//清除历史记录

            LstDistributionBox?.Clear();
            LstLight?.Clear();
            LstBlankIcon?.Clear();
            LstCoordinate?.Clear();
            LstPlanPartitionPointRecord?.Clear();
            LstFaultRecord?.Clear();
            LstHistoricalEvent?.Clear();
            ClearEPSAndLamp();
        }

        /// <summary>
        /// 清除EPS数据和单灯控制界面数据
        /// </summary>
        private void ClearEPSAndLamp()
        {
            //EPS界面
            EPSCode.Content = MainVoltage.Content = BatteryVoltage.Content = OutputVoltage.Content = DischargeCurrent.Content = TwoWaySignLamp.Content = DoubleHeadLamp.Content = BidirectionalBuriedLamp.Content = Floodlight.Content = OneWaySignLamp.Content = EXIT.Content = OneWayBuriedLamp.Content = FloorIndication.Content = FaultLight.Content = LampTotal.Content = 0;
            CurrentState.Content = "正常";
            EPSInitialPositionText.Text = string.Empty;
            EPSReservePlan1.Text = EPSReservePlan2.Text = EPSReservePlan3.Text = EPSReservePlan4.Text = EPSReservePlan5.Text = "0";

            //单灯控制界面
            LampEPS.Content = LampCode.Content = 0;
            LampClass.Content = LampState.Content = LampsPositionText.Text = string.Empty;

            LampBright.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Unchecked.png", UriKind.Relative));
            LampExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Unchecked.png", UriKind.Relative));
            LampShine.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Unchecked.png", UriKind.Relative));
            LampMainEle.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Unchecked.png", UriKind.Relative));
            LampLeftOpen.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Unchecked.png", UriKind.Relative));
            LampRightOpen.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Unchecked.png", UriKind.Relative));

            InitialStateBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusOpenUnClicked.png", UriKind.Relative));
            InitialStateLeftBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
            InitialStateRightBright.Source = new BitmapImage(new Uri("\\Picture\\BeginStatusRightOpenUnClicked.png", UriKind.Relative));
            InitialStateExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusCloseUnClicked.png", UriKind.Relative));

            LeftBrightPlan1.Text = LeftBrightPlan2.Text = LeftBrightPlan3.Text = LeftBrightPlan4.Text = RightBrightPlan1.Text = RightBrightPlan2.Text = RightBrightPlan3.Text = RightBrightPlan4.Text = "0";
        }

        /// <summary>
        /// 设置主窗体是否可操作
        /// </summary>
        private void SetEnableMainWindow(bool isEnabled)
        {
            TimeAndState.IsEnabled = Initialization.IsEnabled = isEnabled;
        }

        /// <summary>
        /// 搜索所有配电箱
        /// </summary>
        private void SearchEPS()
        {
            //QueryEPSAndLightTimer.Enabled = false;
            IsQueryLight = false;
            IsQueryEPSAndLight = false;
            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;
            Protocol.EnterSearchEPS();

            for (int i = MinEPSCode; i <= MaxEPSCode; i++)
            {
                SearchingEPSCount++;
                EPSQuantitySearched.Content = SearchingEPSCount;
                pgbSearchEPS1.Value = SearchingEPSCount / SearchEPSPercent;

                bool isSuccess = Protocol.FindEPS(i.ToString());
                if (isSuccess)
                {
                    int ID = AddEPS(i.ToString());
                    AddCoordinate(EnumClass.TableName.DistributionBox.ToString(), ID);
                }
            }

            Protocol.ExitSearchEPS();
            LstDistributionBox.Sort();
            LstFaultRecord.Clear();
            ObjFaultRecord.DeleteAll();
            LstLightFault.Clear();
            IsTimingQueryEPSOrLight = true;
            //QueryEPSAndLightTimer.Enabled = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
        }

        /// <summary>
        /// 快速搜索所有配电箱
        /// </summary>
        private void SearchEPSFastly()
        {
            //QueryEPSAndLightTimer.Enabled = false;
            IsQueryLight = false;
            IsQueryEPSAndLight = false;
            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;
            Protocol.EnterSearchEPS();

            EPSTotalQuantity.Content = 100;
            SearchedNum.Content = TotalSearch.Content = 0;
            TotalSearch.Content = 100;

            Thread thread = new Thread(() =>
            {
                if (IsOneKey)
                {
                    for (int i = 0; i <= 100; i++)
                    {
                        SearchedNum.Dispatcher.Invoke(new Action(() =>
                        {
                            SearchedNum.Content = i;
                        }));

                        OneKeySearchSpeed.Dispatcher.Invoke(new Action(() =>
                        {
                            OneKeySearchSpeed.Value = i;
                        }));
                        //Thread.Sleep(50);
                    }
                }
                else
                {
                    for (int i = 0; i < 100; i++)
                    {
                        SearchingEPSCount++;

                        EPSQuantitySearched.Dispatcher.Invoke(new Action(() =>
                        {
                            EPSQuantitySearched.Content = SearchingEPSCount;
                        }));

                        pgbSearchEPS1.Dispatcher.Invoke(new Action(() =>
                        {
                            pgbSearchEPS1.Value = SearchingEPSCount;
                        }));
                    }
                }
            })
            {
                IsBackground = true
            };
            thread.Start();

            string[] result = Protocol.FindEPSFastly();
            if (result != null)
            {
                EPSCodeFastly = result.ToList();
                EPSCodeFastly.Sort();
                if (EPSCodeFastly != null)
                {
                    for (int j = 0; j < EPSCodeFastly.Count; j++)
                    {
                        string strEPSCode = EPSCodeFastly[j];
                        if (Convert.ToInt32(strEPSCode) != 0 && strEPSCode.Length == EPSCodeLength)
                        {
                            int ID = AddEPS(strEPSCode);
                            AddCoordinate(EnumClass.TableName.DistributionBox.ToString(), ID);
                        }
                    }
                }
                Protocol.ExitSearchEPS();
                LstDistributionBox.Sort();
                LstFaultRecord?.Clear();
                ObjFaultRecord.DeleteAll();
                LstLightFault?.Clear();
            }
            IsTimingQueryEPSOrLight = true;
            //QueryEPSAndLightTimer.Enabled = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
        }


        /// <summary>
        /// 搜索所有灯具
        /// </summary>
        private void SearchLight()
        {
            //QueryEPSAndLightTimer.Enabled = false;
            IsQueryLight = false;
            IsQueryEPSAndLight = false;
            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;
            Protocol.FindLightByAllEPS();

            LampTotalQuantity.Content = LightCodeRangeTotalCount;
            while (SearchingLightCount < LightCodeRangeTotalCount)//搜灯时，进度条和搜灯数量随着变化
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(FindLightByAllEPSTime);

                SearchingLightCount++;
                LampQuantitySearched.Content = SearchingLightCount;
                pgbSearchLamp.Value = SearchingLightCount / LightType / SearchLightPercent;
            }

            foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBox)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);

                for (int i = 0; i < ExecuteCommonStructTimes; i++)
                {
                    LightCodeByEPS = Protocol.QueryLightByEPS(infoDistributionBox.Code);
                    if (LightCodeByEPS != null)
                    {
                        for (int j = 0; j < LightCodeByEPS.Length; j++)
                        {
                            string strLightCode = LightCodeByEPS[j];
                            if (Convert.ToInt32(strLightCode) != 0 && strLightCode.Length == LightCodeLength)
                            {
                                int ID = ObjLight.Add(AddLight(strLightCode, int.Parse(infoDistributionBox.Code), j));
                                AddCoordinate(EnumClass.TableName.Light.ToString(), ID);
                            }
                        }
                        break;
                    }
                }
            }

            //ObjLight.Save(LstLight);
            LstLight = ObjLight.GetAll();

            LampQuantitySearched.Content = LampTotalQuantity.Content = 0;
            pgbSearchLamp.Value = 0;

            LstFaultRecord.Clear();
            ObjFaultRecord.DeleteAll();
            IsTimingQueryEPSOrLight = true;
            //QueryEPSAndLightTimer.Enabled = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
        }

        /// <summary>
        /// 快速搜索所有灯具
        /// </summary>
        private void SearchLightFastly()
        {
            //QueryEPSAndLightTimer.Enabled = false;
            IsQueryLight = false;
            IsQueryEPSAndLight = false;
            IsTimingQueryEPSOrLight = false;
            ObjLight.DeleteAll();
            LstLight?.Clear();
            LightStatusByEPSArray = null;
            LstLightQueryByEPSID?.Clear();
            //SearchLampProgressBarValueTimer.Enabled = true;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;
            //while(SearchLampProgressBarValueTimer.Enabled)
            //{
            //    EPSCodeFastly = Protocol.FindLightByAllEPSFastly();

            if (LstDistributionBox.Count != 0)
            {
                Protocol.SearchLampFastly();
                if (!IsOneKey)
                {
                    LampQuantitySearched.Content = 0;//已经搜索上灯具的EPS数量
                    LampTotalQuantity.Content = LstDistributionBox.Count();//总EPS数
                }
                LampQuantitySearched.Content = 0;//已经搜索上灯具的EPS数量
                LampTotalQuantity.Content = LstDistributionBox.Count();//总EPS数
                int CompleteSearchEPSNum = 0;//记录已经搜索完灯具的EPS数量
                for (int i = 0; i < LstDistributionBox.Count(); i++)
                {
                    CompleteSearchEPSNum++;
                    LampQuantitySearched.Content = i;
                    LightCodeByEPS = Protocol.QueryLightByEPSFastly(LstDistributionBox[i].Code);
                    if (LightCodeByEPS != null)
                    {
                        for (int j = 0; j < LightCodeByEPS.Length; j++)
                        {
                            string strLightCode = LightCodeByEPS[j];
                            LightInfo infoLight = LstLight.Find(x => x.Code == strLightCode && x.DisBoxID == int.Parse(LstDistributionBox[i].Code));
                            if (Convert.ToInt32(strLightCode) != 0 && strLightCode.Length == LightCodeLength && infoLight == null)
                            {
                                int ID = ObjLight.Add(AddLight(strLightCode, int.Parse(LstDistributionBox[i].Code), j));
                                AddCoordinate(EnumClass.TableName.Light.ToString(), ID);
                            }
                        }
                    }
                    //ObjLight.Save(LstLight);
                    LstLight = ObjLight.GetAll();
                    if (!IsOneKey)
                    {
                        if (CompleteSearchEPSNum != 0 && CompleteSearchEPSNum != LstDistributionBox.Count())
                        {
                            SearchingLightCount = (double)100 / LstDistributionBox.Count() * CompleteSearchEPSNum;
                            pgbSearchLamp.Value = SearchingLightCount;
                        }
                        if (i + 1 == LstDistributionBox.Count())
                        {
                            SearchLampProgressBarValueTimer.Enabled = false;

                            for (int m = (int)pgbSearchLamp.Value; m < pgbSearchLamp.Maximum; m++)
                            {
                                pgbSearchLamp.Value = m;
                            }
                        }
                    }
                    else
                    {
                        SearchedNum.Content = CompleteSearchEPSNum;
                        if (CompleteSearchEPSNum != 0 && CompleteSearchEPSNum != LstDistributionBox.Count())
                        {
                            SearchingLightCount = (double)100 / LstDistributionBox.Count() * CompleteSearchEPSNum;
                            OneKeySearchSpeed.Value = SearchingLightCount;
                        }
                        if (i + 1 == LstDistributionBox.Count())
                        {
                            SearchLampProgressBarValueTimer.Enabled = false;

                            for (int m = (int)OneKeySearchSpeed.Value; m < OneKeySearchSpeed.Maximum; m++)
                            {
                                OneKeySearchSpeed.Value = m;
                            }
                        }
                    }
                }

            }
            // }
            //ObjLight.Save(LstLight);
            //LstLight = ObjLight.GetAll();
            pgbSearchLamp.Value = 0;
            LampQuantitySearched.Content = 0;
            LampTotalQuantity.Content = 0;
            Protocol.AllMainEle();
            LstFaultRecord.Clear();
            ObjFaultRecord.DeleteAll();
            FaultCord.Clear();
            IsTimingQueryEPSOrLight = true;
            //QueryEPSAndLightTimer.Enabled = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
        }



        /// <summary>
        /// 显示初始化EPS信息
        /// </summary>
        private void ShowInitialEPSInfo()
        {
            labInitialEPSInfo.Content = LstDistributionBox.Count;
            stpInitialEPSInfo.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 隐藏初始化EPS信息
        /// </summary>
        private void HideInitialEPSInfo()
        {
            SearchingEPSCount = 0;
            EPSQuantitySearched.Content = SearchingEPSCount;
            pgbSearchEPS1.Value = SearchingEPSCount;
            stpInitialEPSInfo.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 显示提示框
        /// </summary>
        /// <param name="tip">显示内容</param>
        /// <param name="isShow">是否显示</param>
        private void InitialTips(string tip, bool isShow)
        {
            tips.Content = tip;
            if (isShow)
            {
                stpInitialTips.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                stpInitialTips.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// 显示初始化灯具信息
        /// </summary>
        private void ShowInitialLightInfo()
        {
            labShowInitialFloodLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count;
            labShowInitialTwoWaySignLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count;
            labShowInitialDoubleHeadLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count;
            labShowInitialTwoWayBuriedLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count;
            labShowInitialExitLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count;
            labShowInitialFloorLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count;
            labShowInitialOneWaySignLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count;
            labShowInitialOneWayBuriedLightInfo.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count;
            labShowInitialAllLightInfo.Content = LstLight.Count;
            stpInitialLightInfo.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 隐藏初始化灯具信息
        /// </summary>
        private void HideInitialLightInfo()
        {
            SearchingLightCount = 0;
            stpInitialLightInfo.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 统计EPS和灯具数量
        /// </summary>
        private void EPSAndLightStatCount()
        {
            EPSTotal.Content = LstDistributionBox.Count;//EPS总数
            EPSFaultNum.Content = LstDistributionBox.FindAll(x => (x.Status & 0X07FC) != (int)EnumClass.DisBoxStatusClass.正常状态).Count - LstDistributionBox.FindAll(x => x.Shield == 1).Count;

            FloodlightNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count;
            TwoWaySignLampNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count;
            DoubleHeadLampNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count;
            BidirectionalBuriedLampNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count;
            EXITNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count;
            FloorIndicationNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count;
            OneWaySignLampNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count;
            OneWayBuriedLampNum.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count;
            LampSum.Content = LstLight.Count;
            LampFaultNum.Content = LstLight.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0).Count - LstLight.FindAll(x => ((x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0) && x.Shield == 1).Count - LstLight.FindAll(x => x.Shield == 0 && LstDistributionBox.Find(y => y.Code == x.DisBoxID.ToString() && ((y.Status & 0X07FC) & 0X07FC) == 0X07FC && y.Shield == 1) != null).Count;
            EPSTotalQuantity.Content = 0;
        }

        private int AddEPS(string strEPSCode)
        {
            int ID = 0;
            DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo
            {
                Code = strEPSCode,
                Address = "安装位置未初始化",
                Status = (int)EnumClass.DisBoxStatusClass.正常状态,
                ErrorTime = string.Empty,
                Disable = 0,
                Plan1 = 0,
                Plan2 = 0,
                Plan3 = 0,
                Plan4 = 0,
                Plan5 = 0,
                Test = 0,
                QiangQi = 0,
                AutoManual = 0,
                Shield = 0
            };
            ID = ObjDistributionBox.Add(infoDistributionBox);
            LstDistributionBox.Add(infoDistributionBox);
            return ID;
        }

        private void AddCoordinate(string tableName, int tableID)
        {
            CoordinateInfo infoCoordinate = new CoordinateInfo
            {
                TableName = tableName,
                TableID = tableID,
                Location = 0,
                OriginX = 0,
                OriginY = 0,
                NLOriginX = 0,
                NLOriginY = 0,
                TransformX = 0,
                TransformY = 0
            };
            ObjCoordinate.Add(infoCoordinate);
            LstCoordinate.Add(infoCoordinate);
        }

        /// <summary>
        /// 添加灯具到数据库
        /// </summary>
        private LightInfo AddLight(string strLightCode, int disBoxID, int lightIndex)
        {
            LightInfo infoLight = new LightInfo
            {
                Code = strLightCode,
                Address = "安装位置未初始化",
                //Status = 0,
                ErrorTime = string.Empty,
                Disable = 0,
                PlanLeft1 = 0,
                PlanLeft2 = 0,
                PlanLeft3 = 0,
                PlanLeft4 = 0,
                PlanLeft5 = 0,
                PlanRight1 = 0,
                PlanRight2 = 0,
                PlanRight3 = 0,
                PlanRight4 = 0,
                PlanRight5 = 0,
                LightClass = Convert.ToInt32(strLightCode.Substring(0, 1)),
                DisBoxID = disBoxID,
                LightIndex = lightIndex,
                IsEmergency = 0,
                RtnDirection = 0,
                Shield = 0
            };

            if (Convert.ToInt32(infoLight.Code.Substring(0, 1)) == 5)
            {
                if (Convert.ToInt32(infoLight.Code.Substring(1, 1)) >= 5)
                {
                    infoLight.LightClass = 6;
                }
                else
                {
                    infoLight.LightClass = 5;
                }
            }

            if (infoLight.LightClass == 1 || infoLight.LightClass == 3)
            {
                infoLight.CurrentState = (int)EnumClass.SingleLightCurrentState.全灭;
            }
            else
            {
                infoLight.CurrentState = (int)EnumClass.SingleLightCurrentState.全亮;
            }

            infoLight.BeginStatus = infoLight.Status = GetLightInitStatus(infoLight.LightClass);
            LstLight.Add(infoLight);

            return infoLight;
        }

        /// <summary>
        /// 显示登录界面
        /// </summary>
        private bool ShowLogin()
        {
            Login1 Login = new Login1(LstGblSetting, (int)EnumClass.VerifyClass.登录验证);
            Login.ShowDialog();
            return Login.IsSuccessCheck;
        }

        private bool ShowExit()
        {
            Login1 Login = new Login1(LstGblSetting, (int)EnumClass.VerifyClass.退出验证);
            Login.ShowDialog();
            return Login.IsSuccessCheck;
        }

        /// <summary>
        /// 显示主界面
        /// </summary>
        private void ShowMainWindow()
        {
            FunctionPage.Visibility = System.Windows.Visibility.Hidden;
            Homepage.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 退出主菜单
        /// </summary>
        private void MenuCancel()
        {
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 打开灯具汇总界面
        /// </summary>
        private void OpenLightSummaryNoLoginPage()
        {
            CLight cLight = new CLight();
            List<LightInfo> lightInfos = cLight.GetAll();
            EPSListTotalPageNoLogin = LstDistributionBox.Count;
            LightListCurrentPageNoLogin = 1;
            LightListTotalPageNoLogin = LstLightViewByDisBoxIDNoLogin.Count != 0 ? (LstLightViewByDisBoxIDNoLogin.Count - 1) / (LightListMaxRowCountNoLogin * LightListColumnCountNoLogin) + 1 : 1;



            FloodLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count;
            TwoWaySignLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count;
            DoubleHeadLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count;
            TwoWayBuriedLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count;
            FireEscape.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count;
            FloorIndicate.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count;
            OneWaySignLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count;
            OneWayBuriedLight.Content = lightInfos.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count;

            EPSSum.Content = LstDistributionBox.Count;
            EPSFaultSum.Content = LstDistributionBox.FindAll(x => (x.Status & 0X07FC) != (int)EnumClass.DisBoxStatusClass.正常状态).Count - LstDistributionBox.FindAll(x => x.Shield == 1).Count;
            LampAmount.Content = lightInfos.Count;

            LampFaultAmount.Content = LstLight.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0).Count - LstLight.FindAll(x => ((x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0) && x.Shield == 1).Count - LstLight.FindAll(x => x.Shield == 0 && LstDistributionBox.Find(y => y.Code == x.DisBoxID.ToString() && ((y.Status & 0X07FC) & 0X07FC) == 0X07FC && y.Shield == 1) != null).Count;

            if (LampCollect.Visibility == System.Windows.Visibility.Hidden)
            {
                EPSCurrentByLampCollect.Content = EPSViewCurrentIndexNoLogin + 1;//当前查看EPS的索引
                EPSTotalByLampCollect.Content = EPSListTotalPageNoLogin;//EPS总数
                EPSCodeByLampCollect.Content = SelectInfoEPSNoLogin.Code;//当前查看的EPS码
                LampCurrentPageByLampCollect.Content = LightListCurrentPageNoLogin;//灯具当前页数
                LampTotalPageByCollect.Content = LightListTotalPageNoLogin;//灯具总页数
                Homepage.Visibility = System.Windows.Visibility.Hidden;
                TimeAndState.Visibility = LampCollect.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                LampCurrentPageByLampCollect.Content = LightListCurrentPageNoLogin;//灯具当前页数
                LampTotalPageByCollect.Content = LightListTotalPageNoLogin;//灯具总页数
            }
        }

        /// <summary>
        /// 打开单灯控制页面
        /// </summary>
        private void OpenSingleLightControlPage()
        {
            EPSListCurrentPageLogin = 1;
            EPSListTotalPageLogin = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (4 * 3) + 1 : 1;
            LightListCurrentPageLogin = 1;
            LightListTotalPageLogin = LstLightViewByDisBoxIDLogin.Count != 0 ? (LstLightViewByDisBoxIDLogin.Count - 1) / (LightListColumnCountLogin * LightListMaxRowCountLogin) + 1 : 1;

            EPSCurrentPage.Content = EPSListCurrentPageLogin;
            EPSTotalPages.Content = EPSListTotalPageLogin;
            LampCurrentPage.Content = LightListCurrentPageLogin;
            LampTotalPages.Content = LightListTotalPageLogin;

            if (LstDistributionBox.Count != 0)
            {
                LightInfo infoLight = LstLight.Find(x => x.DisBoxID == int.Parse(LstDistributionBox[0].Code));
                if (infoLight != null)
                {
                    ShowLightInfoLogin(LstLight.Find(x => x.DisBoxID == int.Parse(LstDistributionBox[0].Code)));
                    GetImageLightCurrentState();
                    GetImageLightBeginStatus();
                    ChangeLightControlShow();
                }
            }
        }

        /// <summary>
        /// 打开图形界面
        /// </summary>
        private void OpenLayerModeNoLoginPage()
        {
            stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
            MasterController.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 打开图形界面
        /// </summary>
        private void OpenLayerModeLoginPage()
        {
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = stpLayerModeLogin.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 显示系统数字键盘
        /// </summary>
        private void ShowSystemKeyBoard()
        {
            KeyBoard KeyBoard = new KeyBoard(FocusTextBox.Text);
            KeyBoard.ShowDialog();

            FocusTextBox.Text = KeyBoard.TextBoxContent;
            //FocusTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));


        }

        private void ShowSystemKeyBoardLabel()
        {
            KeyBoard KeyBoard = new KeyBoard(FocusLabel.Content.ToString());
            KeyBoard.ShowDialog();

            FocusLabel.Content = KeyBoard.TextBoxContent;
        }

        /// <summary>
        /// 显示系统中文键盘
        /// </summary>
        private void ShowSystemChineseKeyBoard(TextBox TextBox)
        {
            ChineseKeyBoard ChineseKeyBoard = new ChineseKeyBoard(TextBox.Text);
            ChineseKeyBoard.ShowDialog();

            if (ChineseKeyBoard.IsFinishFill)
            {
                TextBox.Text = ChineseKeyBoard.Address;
            }
        }

        /// <summary>
        /// 显示日期
        /// </summary>
        private void ShowDateTime()
        {
            DateTime DateTime = DateTime.Now;
            labCurrentTime.Content = string.Format("{0}  {1}", DateTime.ToShortDateString(), DateTime.ToLongTimeString());
            PageTime.Content = string.Format("{0}  {1}", DateTime.ToShortDateString(), DateTime.ToLongTimeString());
        }

        /// <summary>
        /// 执行一次月检或年检
        /// </summary>
        /// <param name="isMonthCheck"></param>
        private void ExeMonthOrSeasonCheck(bool isMonthCheck)
        {
            IsEmergency = true;
            IsComEmergency = true;
            AllMainEleTimer.Enabled = false;
            AllEmergencyTotalTimer.Enabled = true;

            RecordAllEmergencyInfo();
            AllEmergencySystem();
            IndicatorLight();

            if (isMonthCheck)
            {
                IsSystemMonthDetection = true;
                StartMonthCheckByHand();
                if (!IsKeyAccelerateCheck && !IsAccelerateDetection)
                {
                    SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = CommonFunct.GetBrush("#FFFFFF");
                    SelfCheckingTitle.Content = "正在进行手动月检...";
                    SelfCheckPrompt.Visibility = System.Windows.Visibility.Visible;
                }

                //if (IsAccelerateDetection)
                //{
                //    SelfCheckingTotalTime = 20;
                //}
                //else
                //{
                //    SelfCheckingTotalTime = 450;
                //}
                SelfCheckingTotalTime = 35;

                IsOpenEmTime = true;
                while (IsOpenEmTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                }
                EndMonthCheckByHand(!IsSystemMonthDetection);
                IsSystemMonthDetection = false;
            }
            else
            {
                IsSystemSeasonDetection = true;
                StartSeasonCheckByHand();
                if (!IsKeyAccelerateCheck)
                {
                    SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = CommonFunct.GetBrush("#FFFFFF");
                    SelfCheckingTitle.Content = "正在进行手动年检...";
                    SelfCheckPrompt.Visibility = System.Windows.Visibility.Visible;
                }
                SelfCheckingTotalTime = 1800;
                IsOpenEmTime = true;
                while (IsOpenEmTime)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                EndSeasonCheckByHand(!IsSystemSeasonDetection);
                IsSystemSeasonDetection = false;
            }
        }

        /// <summary>
        /// 执行十一次月检和一次年检
        /// </summary>
        private void ExeMonthAndSeasonCheck()
        {
            IsAccelerateDetection = true;
            if (!IsKeyAccelerateCheck)//不是按下加速月年检外壳按钮
            {
                SelfCheckingTitle.Content = "正在进行加速年月检...";
                SelfCheckPrompt.Visibility = System.Windows.Visibility.Visible;
                SelfCheckingFrequency.Visibility = SelfCheckingFrequencyUnit.Visibility = System.Windows.Visibility.Visible;
                SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = SelfCheckingFrequency.Foreground = SelfCheckingFrequencyUnit.Foreground = CommonFunct.GetBrush("#FFFFFF");
            }
            else
            {
                MonthlyAndYearlyCheckKey.Visibility = System.Windows.Visibility.Visible;
            }
            IsSystemMonthDetection = true;
            for (int i = 1; i <= MonthAndSeasonCheckTime; i++)
            {
                if (!IsAccelerateDetection)
                {
                    if (LstFaultRecord.Count != 0 && LstFaultRecord[LstFaultRecord.Count - 1] != LatelyFaultRecord)//出现新故障
                    {
                        EndMonthCheckByHand(!IsAccelerateDetection);
                    }
                    IsSystemMonthDetection = false;
                    break;
                }
                if (!IsKeyAccelerateCheck)
                {
                    MonthAndSeasonCheckCurrentTime = i;
                    SelfCheckingFrequency.Content = i;
                }
                else
                {
                    MonthAndSeasonCheckCurrentTime = i;
                    KeyCheckNum.Content = i;
                }
                if (i == MonthAndSeasonCheckTime)
                {
                    IsSystemMonthDetection = false;
                    IsSystemSeasonDetection = true;
                    //EndMonthCheckByHand(!IsAccelerateDetection);
                    //StartSeasonCheckByHand();
                    ExeMonthOrSeasonCheck(false);
                    //EndSeasonCheckByHand(!IsAccelerateDetection);
                    EndMonthAndSeasonCheckSpeedUp();
                    IsAccelerateDetection = false;
                    //IsSystemDetection = false;
                    IsSystemSeasonDetection = false;
                }
                else
                {
                    ExeMonthOrSeasonCheck(true);
                }
            }
        }

        private void DisableButton(bool IsDisable)
        {
            if (IsDisable)//开始自检
            {
                SelfInspectionReturn.MouseDown -= Return_MouseDown;
                SelfInspectionReturn.MouseUp -= Return_MouseUp;
                MonthlyInspectionByHand.IsEnabled = false;
                YearlyInspectionByHand.IsEnabled = false;
                MonthAndYearCheckSpeedUp.IsEnabled = false;
            }
            else//结束自检
            {
                SelfInspectionReturn.MouseDown += Return_MouseDown;
                SelfInspectionReturn.MouseUp += Return_MouseUp;
                MonthlyInspectionByHand.IsEnabled = true;
                YearlyInspectionByHand.IsEnabled = true;
                MonthAndYearCheckSpeedUp.IsEnabled = true;
            }

        }

        /// <summary>
        /// 全体应急
        /// </summary>
        private void AllEmergency()
        {
            IsTimingQueryEPSOrLight = false;
            //IsComEmergency = true;
            ////while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            ////{
            ////    System.Windows.Forms.Application.DoEvents();
            ////    Thread.Sleep(ExeCommonInstructSleepTime);
            ////    CurrentExeInstructTime += ExeCommonInstructSleepTime;
            ////}
            ////CurrentExeInstructTime = 0;
            //GetEdition(IsCommodity);
            for (int i = 0; i < ExecuteCommonStructTimes; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                Protocol.AllEmergency();
                
            }

            
            IsTimingQueryEPSOrLight = true;
        }


        private void OldEmergency()
        {
            IsEmergency = true;
            IsComEmergency = true;
            AllMainEleTimer.Enabled = false;
            AllEmergencyTotalTimer.Enabled = true;
            LstDistributionBox.ForEach(x => x.IsEmergency = 1);
            LstLight.ForEach(x => x.IsEmergency = 1);
            ObjDistributionBox.Save(LstDistributionBox);
            ObjLight.Save(LstLight);

            RecordAllEmergencyInfo();
            AllEmergencySystem();
            
            IndicatorLight();
            //SetAllLightStatus(true);
        }

        /// <summary>
        /// 全体主电
        /// </summary>
        private void AllMainEle()
        {
            IsTimingQueryEPSOrLight = false;
            //while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            //{
            //    System.Windows.Forms.Application.DoEvents();
            //    Thread.Sleep(ExeCommonInstructSleepTime);
            //    CurrentExeInstructTime += ExeCommonInstructSleepTime;
            //}
            //CurrentExeInstructTime = 0;

            for (int i = 0; i < ExecuteAllMainEleStructTime; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeAllEmergencyOrAllMainEleInstrcutSleepTime);
                Protocol.AllMainEle();
            }

            IsTimingQueryEPSOrLight = true;
        }

        /// <summary>
        /// 记录全体应急信息
        /// </summary>
        private void RecordAllEmergencyInfo()
        {
            AddHistoricalEvent("系统启动应急！");
            Printer.print("系统启动应急！");
        }

        /// <summary>
        /// 记录全体主电信息
        /// </summary>
        private void RecordAllMainEleInfo()
        {
            AddHistoricalEvent(string.Format("{0}   系统复位！", DateTime.Now.ToString()));
            Printer.print("系统复位！");
            //WriteToEventLog(string.Format("{0}   系统复位！", DateTime.Now.ToString()));
        }

        /// <summary>
        /// 记录真实联动总时长
        /// </summary>
        private void RecordRealFireAlarmLinkCalcuTotalTime()
        {
            AddHistoricalEvent(string.Format("系统联动结束|联动总时长{0}", StrAllEmergencyTotalTime));
            //打印联动信息
            Printer.print(string.Format("系统联动结束|联动总时长{0}", StrAllEmergencyTotalTime));
            //WriteToEventLog(string.Format("{0}   系统联动结束|联动总时长{1}", DateTime.Now.ToString(), StrAllEmergencyTotalTime));
        }

        /// <summary>
        /// 显示图形界面楼层
        /// </summary>
        private void ShowCurrentPageFloorNoLogin()
        {
            //this.stpCurrentPageFloorNoLogin.Children.Clear();
            for (int i = (CurrentPageFloorNoLogin - 1) * PerPageFloorNoLogin + 1; i <= CurrentPageFloorNoLogin * PerPageFloorNoLogin; i++)
            {
                if (i > TotalFloor)
                {
                    break;
                }

                System.Windows.Controls.Label LabelPageFloorNoLogin = new System.Windows.Controls.Label
                {
                    Margin = new Thickness(20, 0, 0, 0),
                    Foreground = CommonFunct.GetBrush(UnSelectFloorForeground),
                    FontSize = 20,
                    Content = string.Format("{0}层", i),
                    VerticalContentAlignment = System.Windows.VerticalAlignment.Center
                };//显示楼层

                if (i == (CurrentPageFloorNoLogin - 1) * PerPageFloorNoLogin + 1 && CurrentSelectFloorNoLogin == string.Empty)
                {
                    LabelPageFloorNoLogin.Foreground = CommonFunct.GetBrush(SelectFloorForeground);
                    CurrentSelectFloorNoLogin = LabelPageFloorNoLogin.Content.ToString();
                }
                else
                {
                    if (CurrentSelectFloorNoLogin == LabelPageFloorNoLogin.Content.ToString())
                    {
                        LabelPageFloorNoLogin.Foreground = CommonFunct.GetBrush(SelectFloorForeground);
                    }
                }
                LabelPageFloorNoLogin.MouseDown += LabelPageFloorNoLogin_MouseDown;
                //this.stpCurrentPageFloorNoLogin.Children.Add(LabelPageFloorNoLogin);
            }
        }

        /// <summary>
        /// 显示图形界面楼层
        /// </summary>
        private void ShowCurrentPageFloorLogin()
        {
            labCurrentFloorLogin.Content = CurrentSelectFloorLogin;
        }

        /// <summary>
        /// 加载楼层图纸
        /// </summary>
        private void LoadFloorDrawingNoLogin()
        {
            LstFloorName = ObjFloorName.GetAll();
            if (LstFloorName.Count > 0)
            {
                DirectoryInfo DirectoryInfo = new DirectoryInfo(FloorDrawingPath);

                FileInfo[] FileInfoArray;
                try
                {
                    FileInfoArray = DirectoryInfo.GetFiles();
                }
                catch
                {
                    FileInfoArray = new FileInfo[] { };
                }

                foreach (FileInfo FileInfo in FileInfoArray)
                {
                    string[] floorname;
                    if (int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)) > LstFloorName.Count)
                    {
                        floorname = LstFloorName[LstFloorName.Count - 1].FloorName.Split('.');
                    }
                    else
                    {
                        floorname = LstFloorName[int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)) - 1].FloorName.Split('.');
                    }
                    string floorName = string.Empty;
                    for (int i = 0; i < floorname.Length; i++)
                    {
                        if (i < floorname.Length - 2)
                        {
                            floorName += floorname[i] + ".";
                        }
                        if (i == floorname.Length - 2)
                        {
                            floorName += floorname[i];
                        }
                    }
                    if (FileInfo.Name == floorName + "." + floorname[floorname.Length - 1])
                    {
                        imgFloorDrawingNoLogin.Source = null;
                        imgFloorDrawingNoLogin.Source = GetImageByPath(string.Format("{0}\\{1}", FileInfo.DirectoryName, FileInfo.Name));
                        FloorNameNoLogin.Content = floorName;
                        break;
                    }
                }

                FloorPage.Content = CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1);
            }
        }

        /// <summary>
        /// 加载楼层图纸
        /// </summary>
        private void LoadFloorDrawingLogin()
        {
            LstFloorName = ObjFloorName.GetAll();
            if (LstFloorName.Count > 0)
            {
                DirectoryInfo DirectoryInfo = new DirectoryInfo(FloorDrawingPath);
                FileInfo[] FileInfoArray;
                try
                {
                    FileInfoArray = DirectoryInfo.GetFiles();
                }
                catch
                {
                    FileInfoArray = new FileInfo[] { };
                }

                foreach (FileInfo FileInfo in FileInfoArray)
                {
                    string[] floorname;
                    if (CurrentSelectFloorLogin > LstFloorName.Count)
                    {
                        floorname = LstFloorName[LstFloorName.Count - 1].FloorName.Split('.');
                    }
                    else
                    {
                        floorname = LstFloorName[CurrentSelectFloorLogin - 1].FloorName.Split('.');
                    }
                    string floorName = string.Empty;
                    for (int i = 0; i < floorname.Length; i++)
                    {
                        if (i < floorname.Length - 2)
                        {
                            floorName += floorname[i] + ".";
                        }
                        if (i == floorname.Length - 2)
                        {
                            floorName += floorname[i];
                        }
                    }

                    if (FileInfo.Name == floorName + "." + floorname[floorname.Length - 1])
                    {
                        imgFloorDrawingLogin.Source = GetImageByPath(string.Format("{0}\\{1}", FileInfo.DirectoryName, FileInfo.Name));
                        floorname = FileInfo.Name.Split('.');
                        FloorName.Content = floorName;
                        break;
                    }
                    //if (FileInfo.Name.StartsWith(string.Format("{0}层", CurrentSelectFloorLogin)))
                    //{
                    //    this.imgFloorDrawingLogin.Source = GetImageByPath(string.Format("{0}\\{1}", FileInfo.DirectoryName, FileInfo.Name));
                    //    break;
                    //}
                }
            }
        }

        /// <summary>
        /// 根据路径获取图片
        /// </summary>      
        private BitmapImage GetImageByPath(string strFilePath)
        {
            BitmapImage BitmapImage = new BitmapImage();
            BitmapImage.BeginInit();
            BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            using (Stream Stream = new MemoryStream(File.ReadAllBytes(strFilePath)))
            {   
                BitmapImage.StreamSource = Stream;
                BitmapImage.EndInit();
                BitmapImage.Freeze();
            }
            return BitmapImage;
        }

        /// <summary>
        /// 拖拽图层图标(图层模式编辑)
        /// </summary>
        private void DragOverAllIconLogin(System.Windows.DragEventArgs e)
        {
            DragFloorLogin = e.GetPosition(ctcFloorDrawingLogin);
            OriginDragFloorLogin = TransformGroupLogin.Inverse.Transform(DragFloorLogin);

            #region 计算登录后转换坐标
            //计算转换后的坐标:当前在图纸上的坐标加上起始坐标再加上图标原始大小跟变化后大小的差距
            DragFloorLogin = new Point(DragFloorLogin.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, DragFloorLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform);
            OriginDragFloorLogin = new Point(OriginDragFloorLogin.X + StartPositionDragFloor.X, OriginDragFloorLogin.Y + StartPositionDragFloor.Y);

            DragFloorLogin = new Point(Math.Round(DragFloorLogin.X), Math.Round(DragFloorLogin.Y));
            OriginDragFloorLogin = new Point(Math.Round(OriginDragFloorLogin.X), Math.Round(OriginDragFloorLogin.Y));
            #endregion

            #region 计算登录前转换坐标
            ComputePointNoLogin(DragFloorLogin, OriginDragFloorLogin);
            #endregion
        }

        private void ComputePointNoLogin(Point dragfloor, Point origindragfloor)
        {
            double x, y;//定义横坐标和纵坐标
            double multiple = 1.16;//登录后图层与未登录的图层之间的倍数
            #region dragfloornologin
            double angle = Math.Atan((dragfloor.Y - StartPositionDragFloor.Y) / (dragfloor.X - StartPositionDragFloor.X)) * 180 / Math.PI;//图标坐标与图层原始坐标组成的线的角度

            double distance = Math.Sqrt((dragfloor.X - StartPositionDragFloor.X) * (dragfloor.X - StartPositionDragFloor.X) + (dragfloor.Y - StartPositionDragFloor.Y) * (dragfloor.Y - StartPositionDragFloor.Y));//登录后图层原始坐标与图标坐标的距离

            x = Math.Abs(distance * multiple * Math.Cos(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.X;
            y = Math.Abs(distance * multiple * Math.Sin(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.Y;

            DragFloorNoLogin = new Point(Math.Round(x), Math.Round(y));
            #endregion

            #region origindragfloornologin
            angle = Math.Atan((origindragfloor.Y - StartPositionDragFloor.Y) / (origindragfloor.X - StartPositionDragFloor.X)) * 180 / Math.PI;

            distance = Math.Sqrt((origindragfloor.X - StartPositionDragFloor.X) * (origindragfloor.X - StartPositionDragFloor.X) + (origindragfloor.Y - StartPositionDragFloor.Y) * (origindragfloor.Y - StartPositionDragFloor.Y));

            x = Math.Abs(distance * multiple * Math.Cos(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.X;
            y = Math.Abs(distance * multiple * Math.Sin(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.Y;

            OriginDragFloorNoLogin = new Point(Math.Round(x), Math.Round(y));
            #endregion
        }

        /// <summary>
        /// 获取未登录时图层界面上的逃生路线转折点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Point ComputePointNoLogin(Point point)
        {
            double x, y;//定义横坐标和纵坐标
            double multiple = 1.16;//登录后图层与未登录的图层之间的倍数

            double angle = Math.Atan((point.Y - StartPositionDragFloor.Y) / (point.X - StartPositionDragFloor.X)) * 180 / Math.PI;

            double distance = Math.Sqrt((point.X - StartPositionDragFloor.X) * (point.X - StartPositionDragFloor.X) + (point.Y - StartPositionDragFloor.Y) * (point.Y - StartPositionDragFloor.Y));//登录后图层原始坐标与图标坐标的距离

            x = Math.Abs(distance * multiple * Math.Cos(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.X;
            y = Math.Abs(distance * multiple * Math.Sin(angle * Math.PI / 180)) + StartPositionDragFloorNoLogin.Y;

            Point NoLoginPoint = new Point(Math.Round(x), Math.Round(y));

            return NoLoginPoint;
        }

        /// <summary>
        /// 检查是否为编辑模式
        /// </summary>
        private bool CheckIsEditAllIcon()
        {
            if (!IsEditAllIcon)
            {
                CommonFunct.PopupWindow("编辑模式下才可以拖拽图标！\n 请点击设备设置-移动设备！");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 把拖拽EPS或者灯具图标放进图纸(图层模式编辑)
        /// </summary>
        private void DropListIconToFloorDrawingLogin()
        {
            IconSearchCode IconSearchCode = SelectIconSearchCode as IconSearchCode;
            if (IconSearchCode is IconSearchCode)
            {
                DistributionBoxInfo infoDis = IconSearchCode.Tag as DistributionBoxInfo;
                if (infoDis != null)
                {
                    DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoDis.Code);
                    CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID);
                    infoCoordinate.Location = CurrentSelectFloorLogin;
                    infoCoordinate.OriginX = OriginDragFloorLogin.X;
                    infoCoordinate.OriginY = OriginDragFloorLogin.Y;
                    infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                    infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                    infoCoordinate.TransformX = DragFloorLogin.X;
                    infoCoordinate.TransformY = DragFloorLogin.Y;
                    IconSearchCode.Tag = infoDistributionBox;
                    ObjDistributionBox.Update(infoDistributionBox);
                    ObjCoordinate.Update(infoCoordinate);
                    LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                    //LstDistributionBoxCurrentFloorLogin = LstDistributionBox.FindAll(x => x.Location == CurrentSelectFloorLogin);
                    //LstLightCurrentFloorLogin = LstLight.FindAll(x => x.Location == CurrentSelectFloorLogin);

                    Image image = GetImageLogin(IconSearchCode.Tag);
                    cvsMainWindow.Children.Add(image);
                }
                else if (IconSearchCode.Tag as LightInfo != null)
                {
                    LightInfo infolight = IconSearchCode.Tag as LightInfo;
                    LightInfo infoLight = LstLight.Find(x => x.Code == infolight.Code && x.DisBoxID == infolight.DisBoxID);
                    CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == infolight.ID);
                    infoCoordinate.Location = CurrentSelectFloorLogin;
                    infoCoordinate.OriginX = OriginDragFloorLogin.X;
                    infoCoordinate.OriginY = OriginDragFloorLogin.Y;
                    infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                    infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                    infoCoordinate.TransformX = DragFloorLogin.X;
                    infoCoordinate.TransformY = DragFloorLogin.Y;
                    IconSearchCode.Tag = infoLight;
                    ObjLight.Update(infoLight);
                    ObjCoordinate.Update(infoCoordinate);
                    LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                    //LstDistributionBoxCurrentFloorLogin = LstDistributionBox.FindAll(x => x.Location == CurrentSelectFloorLogin);
                    //LstLightCurrentFloorLogin = LstLight.FindAll(x => x.Location == CurrentSelectFloorLogin);

                    Image image = GetImageLogin(IconSearchCode.Tag);
                    cvsMainWindow.Children.Add(image);
                }
                else
                {
                    #region
                    if (BlankIcon.Tag == null)
                    {
                        BlankIcon = GetImageIcon(IconSearchCode.Tag.ToString());
                        LayerImageTag tag = new LayerImageTag
                        {
                            equipment = ((LayerImageTag)BlankIcon.Tag).equipment,
                            status = ((LayerImageTag)BlankIcon.Tag).status,
                            OriginPoint = new Point(OriginDragFloorLogin.X, OriginDragFloorLogin.Y)
                        };
                        BlankIcon.Tag = tag;
                        BlankIcon.Margin = new Thickness(DragFloorLogin.X, DragFloorLogin.Y, 0, 0);
                        cvsMainWindow.Children.Add(BlankIcon);
                    }
                    else
                    {
                        CommonFunct.PopupWindow("存在未编辑信息的图标，请先完成前一个图标信息的编辑！");
                    }
                    #endregion

                    lvIconSearchCodeList.SelectedIndex = -1;
                }
            }
            else
            {
                if (SelectIconSearchCode != null)
                {
                    PlanPartitionPointRecordInfo infoPlanPartitionPointRecord = new PlanPartitionPointRecordInfo
                    {
                        PlanPartition = 1
                    };
                    int ID = ObjPlanPartitionPointRecord.Add(infoPlanPartitionPointRecord);
                    LstPlanPartitionPointRecord.Add(infoPlanPartitionPointRecord);

                    CoordinateInfo infoCoordinate = new CoordinateInfo
                    {
                        TableName = EnumClass.TableName.PlanPartitionPointRecord.ToString(),
                        TableID = ID,
                        Location = CurrentSelectFloorLogin,
                        OriginX = OriginDragFloorLogin.X,
                        OriginY = OriginDragFloorLogin.Y,
                        NLOriginX = OriginDragFloorNoLogin.X,
                        NLOriginY = OriginDragFloorNoLogin.Y,
                        TransformX = DragFloorLogin.X,
                        TransformY = DragFloorLogin.Y
                    };
                    ObjCoordinate.Add(infoCoordinate);
                    LstCoordinate.Add(infoCoordinate);

                    PartitionPoint PartitionPoint = GetPartitionPointLogin(infoPlanPartitionPointRecord);
                    cvsMainWindow.Children.Add(PartitionPoint);
                }
                lvIconSearchCodeList.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// 一键清除
        /// </summary>
        private bool OneKeyClear()
        {
            bool isChecked = Convert.ToBoolean(radCurrentFloor.IsChecked);//radCurrentFloor 当前楼层
            if (isChecked)
            {
                MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("你是否需要清除当前楼层的配电箱以及灯具？", "提示", MessageBoxButton.YesNo);
                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    MessageBoxResult = System.Windows.MessageBox.Show("你真的确定需要清除当前楼层的配电箱以及灯具？", "警告", MessageBoxButton.YesNo);
                    if (MessageBoxResult == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("你是否需要清除所有楼层的配电箱以及灯具？"
                 , "提示", MessageBoxButton.YesNo);
                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    MessageBoxResult = System.Windows.MessageBox.Show("你真的确定需要清除所有楼层的配电箱以及灯具？", "警告", MessageBoxButton.YesNo);
                    if (MessageBoxResult == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 重新设置配电箱和灯具坐标(根据条件决定是当前楼层还是所有楼层)
        /// </summary>
        private void ResetPositionFloorLogin()
        {
            bool isChecked = Convert.ToBoolean(radCurrentFloor.IsChecked);
            if (isChecked)
            {
               
                if (LstBlankIcon != null)
                {
                    List<BlankIconInfo> LstBlankIconCurrentFloor = LstBlankIcon.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.BlankIcon.ToString() && y.TableID == x.ID).Location == CurrentSelectFloorLogin);
                    for (int i = 0; i < LstBlankIconCurrentFloor.Count; i++)
                    {
                        ObjCoordinate.Delete(LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == LstBlankIconCurrentFloor[i].ID).ID);
                        ObjBlankIcon.Delete(LstBlankIconCurrentFloor[i].ID);
                    }
                }
                
                LstCoordinate = ObjCoordinate.GetAll();
                LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

                LstCoordinateCurrentFloorLogin.ForEach(x => x.Location = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.OriginX = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.OriginY = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.NLOriginX = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.NLOriginY = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.TransformX = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.TransformY = 0);
                LstCoordinateCurrentFloorLogin.ForEach(x => x.IsAuth = 0);

                ObjCoordinate.Save(LstCoordinateCurrentFloorLogin);

                #region 旧
                //当前楼层的配电箱
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.Location = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.Address = string.Empty);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.OriginX = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.OriginY = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.NLOriginX = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.NLOriginY = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.TransformX = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.TransformY = 0);
                //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.IsAuth = 0);

                ////当前楼层的灯具
                //LstLightCurrentFloorLogin.ForEach(x => x.Location = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.Address = string.Empty);
                //LstLightCurrentFloorLogin.ForEach(x => x.OriginX = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.OriginY = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.NLOriginX = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.NLOriginY = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.TransformX = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.TransformY = 0);
                //LstLightCurrentFloorLogin.ForEach(x => x.IsAuth = 0);

                //ObjDistributionBox.Save(LstDistributionBoxCurrentFloorLogin);
                //ObjLight.Save(LstLightCurrentFloorLogin);
                #endregion
            }
            else
            {
                List<CoordinateInfo> LstCoordinateForBlankIcon = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.BlankIcon.ToString());
                for (int i = 0; i < LstCoordinateForBlankIcon.Count; i++)
                {
                    ObjCoordinate.Delete(LstCoordinateForBlankIcon[i].ID);
                }
                LstCoordinate = ObjCoordinate.GetAll();

                LstCoordinate.ForEach(x => x.Location = 0);
                LstCoordinate.ForEach(x => x.OriginX = 0);
                LstCoordinate.ForEach(x => x.OriginY = 0);
                LstCoordinate.ForEach(x => x.NLOriginX = 0);
                LstCoordinate.ForEach(x => x.NLOriginY = 0);
                LstCoordinate.ForEach(x => x.TransformX = 0);
                LstCoordinate.ForEach(x => x.TransformY = 0);
                LstCoordinate.ForEach(x => x.IsAuth = 0);

                ObjCoordinate.Save(LstCoordinate);
                ObjBlankIcon.DeleteAll();

                #region 旧
                //LstDistributionBox.ForEach(x => x.Location = 0);
                //LstDistributionBox.ForEach(x => x.Address = string.Empty);
                //LstDistributionBox.ForEach(x => x.OriginX = 0);
                //LstDistributionBox.ForEach(x => x.OriginY = 0);
                //LstDistributionBox.ForEach(x => x.NLOriginX = 0);
                //LstDistributionBox.ForEach(x => x.NLOriginY = 0);
                //LstDistributionBox.ForEach(x => x.TransformX = 0);
                //LstDistributionBox.ForEach(x => x.TransformY = 0);
                //LstDistributionBox.ForEach(x => x.IsAuth = 0);

                //LstLight.ForEach(x => x.Location = 0);
                //LstLight.ForEach(x => x.Address = string.Empty);
                //LstLight.ForEach(x => x.OriginX = 0);
                //LstLight.ForEach(x => x.OriginY = 0);
                //LstLight.ForEach(x => x.NLOriginX = 0);
                //LstLight.ForEach(x => x.NLOriginY = 0);
                //LstLight.ForEach(x => x.TransformX = 0);
                //LstLight.ForEach(x => x.TransformY = 0);
                //LstLight.ForEach(x => x.IsAuth = 0);

                //ObjDistributionBox.Save(LstDistributionBox);
                //ObjLight.Save(LstLight);
                #endregion
            }
            CommonFunct.PopupWindow("一键清除成功！");
        }

        /// <summary>
        /// 加载图标到楼层图纸上
        /// </summary>
        private void LoadIconSearchCodeNoLogin()
        {
            foreach (CoordinateInfo infoCoordinate in LstCoordinateCurrentFloorNoLogin)
            {
                object Equipment = null;
                if (infoCoordinate.TableName == EnumClass.TableName.DistributionBox.ToString())
                {
                    Equipment = LstDistributionBox.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (infoCoordinate.TableName == EnumClass.TableName.Light.ToString())
                {
                    Equipment = LstLight.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (infoCoordinate.TableName == EnumClass.TableName.BlankIcon.ToString())
                {
                    Equipment = LstBlankIcon.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (Equipment != null)
                {
                    Image image = GetImageNoLogin(Equipment);
                    cvsMainWindow.Children.Add(image);
                }
            }
        }

        /// <summary>
        /// 加载EPS和灯具图标到楼层图纸上
        /// </summary>
        private void LoadIconSearchCodeLogin()
        {
            foreach (CoordinateInfo infoCoordinate in LstCoordinateCurrentFloorLogin)
            {
                object Equipment = null;
                if (infoCoordinate.TableName == EnumClass.TableName.DistributionBox.ToString())
                {
                    Equipment = LstDistributionBox.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (infoCoordinate.TableName == EnumClass.TableName.Light.ToString())
                {
                    Equipment = LstLight.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (infoCoordinate.TableName == EnumClass.TableName.BlankIcon.ToString())
                {
                    Equipment = LstBlankIcon.Find(x => x.ID == infoCoordinate.TableID);
                }

                if (Equipment != null)
                {
                    Image image = GetImageLogin(Equipment);
                    cvsMainWindow.Children.Add(image);
                }
            }

            //foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBoxCurrentFloorLogin)
            //{
            //    Image image = GetImageLogin(infoDistributionBox);
            //    this.cvsMainWindow.Children.Add(image);
            //}

            //foreach (LightInfo infoLight in LstLightCurrentFloorLogin)
            //{
            //    Image image = GetImageLogin(infoLight);
            //    this.cvsMainWindow.Children.Add(image);
            //}
        }

        /// <summary>
        /// 加载报警点图标到图层上
        /// </summary>
        private void LoadPartitionPointLogin()
        {
            LstPartitionPointCurrentFloorLogin = LstPlanPartitionPointRecord.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && y.TableID == x.ID) != null);
            foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPartitionPointCurrentFloorLogin)
            {
                PartitionPoint PartitionPoint = GetPartitionPointLogin(infoPlanPartitionPointRecord);
                cvsMainWindow.Children.Add(PartitionPoint);
            }
        }

        /// <summary>
        /// 初始化报警点图标到图层上
        /// </summary>
        private void InitPartitionNoLogin()
        {
            LoadPartitionPointNoLogin();

        }

        /// <summary>
        /// 初始化报警点图标到图层上
        /// </summary>
        private void InitPartitionLogin()
        {
            LoadPartitionPointLogin();
            RefreshPartitionPointLogin();
        }

        /// <summary>
        /// 清除图层上的报警点图标
        /// </summary>
        private void ClearPartitionPointLogin()
        {
            for (int i = MainWindowChildCount; i < cvsMainWindow.Children.Count; i++)
            {
                PartitionPoint PartitionPoint = cvsMainWindow.Children[i] as PartitionPoint;
                if (PartitionPoint != null)
                {
                    cvsMainWindow.Children.Remove(PartitionPoint);
                    i--;
                }

                Line line = cvsMainWindow.Children[i] as Line;
                if (line != null)
                {
                    cvsMainWindow.Children.Remove(line);
                    i--;
                }

                Image img = cvsMainWindow.Children[i] as Image;
                if (img != null && !(img.Tag is LayerImageTag))
                {
                    cvsMainWindow.Children.Remove(img);
                    i--;
                }

                Label lab = cvsMainWindow.Children[i] as Label;
                if (lab != null)
                {
                    cvsMainWindow.Children.Remove(lab);
                    i--;
                }

            }
        }

        /// <summary>
        /// 刷新图纸上的图标
        /// </summary>
        private void RefreshIconSearchCodeNoLogin()
        {
            for (int i = MainWindowChildCount; i < cvsMainWindow.Children.Count; i++)
            {
                Image IconSearchCode = cvsMainWindow.Children[i] as Image;
                if (IconSearchCode == null || !(IconSearchCode.Tag is LayerImageTag))
                {
                    continue;
                }

                LayerImageTag layerImage = (LayerImageTag)IconSearchCode.Tag;
                if (layerImage.equipment is DistributionBoxInfo || layerImage.equipment is LightInfo || layerImage.equipment is BlankIconInfo)
                {
                    int ID = Convert.ToInt32(layerImage.equipment.GetType().GetProperty("ID").GetValue(layerImage.equipment));

                    CoordinateInfo infoCoordinate = new CoordinateInfo();
                    if (layerImage.equipment is DistributionBoxInfo)
                    {
                        infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                        DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.ID == ID);
                        layerImage.equipment = infoDistributionBox;
                        layerImage.status = infoDistributionBox.Status;
                    }
                    else if (layerImage.equipment is LightInfo)
                    {
                        infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == ID);
                        LightInfo infoLight = LstLight.Find(x => x.ID == ID);
                        layerImage.equipment = infoLight;
                        layerImage.status = infoLight.Status;
                    }
                    else
                    {
                        if (layerImage.equipment is BlankIconInfo)
                        {
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == ID);
                            BlankIconInfo infoBlankIcon = LstBlankIcon.Find(x => x.ID == ID);
                            layerImage.equipment = infoBlankIcon;
                            layerImage.status = 0;
                        }
                    }

                    if (infoCoordinate.TransformX <= StartPositionDragFloor.X || infoCoordinate.TransformX >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || infoCoordinate.TransformY <= StartPositionDragFloor.Y || infoCoordinate.TransformY >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
                    {
                        if (IconSearchCode.Visibility == System.Windows.Visibility.Visible)
                        {
                            IconSearchCode.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    else
                    {
                        if (IconSearchCode.Visibility == System.Windows.Visibility.Hidden)
                        {
                            IconSearchCode.Visibility = System.Windows.Visibility.Visible;
                        }
                        IconSearchCode.Width = IconSearchCode.Height = IconSearchCodeSizeNoLogin;
                        IconSearchCode.Margin = new Thickness(infoCoordinate.NLOriginX, infoCoordinate.NLOriginY, 0, 0);

                        Uri NewImageUriIconSearchCode = GetImageUriIconSearchCode(layerImage.equipment);
                        if (NewImageUriIconSearchCode != null)
                        {
                            IconSearchCode.Source = new BitmapImage(NewImageUriIconSearchCode);
                        }
                        IconSearchCode.Tag = layerImage;

                        ////灯具故障时闪烁
                        
                    }
                }
                else
                {
                    if (BlankIcon.Margin.Left <= StartPositionDragFloor.X || BlankIcon.Margin.Left >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || BlankIcon.Margin.Top <= StartPositionDragFloor.Y || BlankIcon.Margin.Top >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
                    {
                        if (BlankIcon.Visibility == System.Windows.Visibility.Visible)
                        {
                            BlankIcon.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    else
                    {
                        if (IconSearchCode.Visibility == System.Windows.Visibility.Hidden)
                        {
                            IconSearchCode.Visibility = System.Windows.Visibility.Visible;
                        }
                        BlankIcon.Width = BlankIcon.Height = IconSearchCodeSizeNoLogin;
                        //BlankIcon.Margin = new Thickness(infoCoordinate.TransformX, infoCoordinate.TransformY, 0, 0);
                    }
                }

                
            }
        }

        /// <summary>
        /// 刷新图纸上的EPS和灯具图标
        /// </summary>
        private void RefreshIconSearchCodeLogin()
        {
            for (int i = MainWindowChildCount; i < cvsMainWindow.Children.Count; i++)
            {
                if (cvsMainWindow.Children[i] as Image != null)
                {
                    Image IconSearchCode = cvsMainWindow.Children[i] as Image;


                    if (IconSearchCode == null || !(IconSearchCode.Tag is LayerImageTag))
                    {
                        continue;
                    }

                    LayerImageTag layerImage = (LayerImageTag)IconSearchCode.Tag;
                    if (layerImage.equipment is DistributionBoxInfo || layerImage.equipment is LightInfo || layerImage.equipment is BlankIconInfo)
                    {
                        int ID = Convert.ToInt32(layerImage.equipment
                                        .GetType().GetProperty("ID").GetValue(layerImage.equipment));

                        CoordinateInfo infoCoordinate = new CoordinateInfo();

                        if (layerImage.equipment is DistributionBoxInfo)
                        {
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.ID == ID);
                            if (infoDistributionBox != null)
                            {
                                layerImage.equipment = infoDistributionBox;
                                layerImage.status = infoDistributionBox.Status;
                            }

                        }
                        else if (layerImage.equipment is LightInfo)
                        {
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == ID);
                            LightInfo infoLight = LstLight.Find(x => x.ID == ID);
                            if (infoLight != null)
                            {
                                layerImage.equipment = infoLight;
                                layerImage.status = infoLight.Status;

                            }
                        }
                        else
                        {
                            if (layerImage.equipment is BlankIconInfo)
                            {
                                infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == ID);
                                BlankIconInfo infoBlankIcon = LstBlankIcon.Find(x => x.ID == ID);
                                layerImage.equipment = infoBlankIcon;
                                layerImage.status = 0;
                            }
                        }

                        if (infoCoordinate.TransformX <= StartPositionDragFloor.X || infoCoordinate.TransformX >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || infoCoordinate.TransformY <= StartPositionDragFloor.Y || infoCoordinate.TransformY >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
                        {
                            if (IconSearchCode.Visibility == System.Windows.Visibility.Visible)
                            {
                                IconSearchCode.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }

                        else
                        {
                            if (IconSearchCode.Visibility == System.Windows.Visibility.Hidden)
                            {
                                IconSearchCode.Visibility = System.Windows.Visibility.Visible;
                            }

                            IconSearchCode.Width = IconSearchCode.Height = IconSearchCodeSizeLogin;
                            IconSearchCode.Margin = new Thickness(infoCoordinate.TransformX, infoCoordinate.TransformY, 0, 0);

                            Uri NewImageUriIconSearchCode = GetImageUriIconSearchCode(layerImage.equipment);
                            if (NewImageUriIconSearchCode != null)
                            {
                                IconSearchCode.Source = new BitmapImage(NewImageUriIconSearchCode);
                            }
                            IconSearchCode.Tag = layerImage;

                        }
                    }
                    else
                    {
                        if (BlankIcon.Margin.Left <= StartPositionDragFloor.X || BlankIcon.Margin.Left >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || BlankIcon.Margin.Top <= StartPositionDragFloor.Y || BlankIcon.Margin.Top >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
                        {
                            if (BlankIcon.Visibility == System.Windows.Visibility.Visible)
                            {
                                BlankIcon.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                        else
                        {
                            if (IconSearchCode.Visibility == System.Windows.Visibility.Hidden)
                            {
                                IconSearchCode.Visibility = System.Windows.Visibility.Visible;
                            }
                            BlankIcon.Width = BlankIcon.Height = IconSearchCodeSizeLogin;
                            //BlankIcon.Margin = new Thickness(infoCoordinate.TransformX, infoCoordinate.TransformY, 0, 0);
                        }
                    }

                }
            }
        }


        /// <summary>
        /// 刷新报警点图标
        /// </summary>
        private void RefreshPartitionPointLogin()
        {
            //CurrentFloorEPSAndLightTotalCountLogin当前楼层EPS和灯具总数  LstDistributionBoxCurrentFloorLogin当前楼层配电箱  LstLightCurrentFloorLogin当前楼层的灯具
            CurrentFloorEPSAndLightTotalCountLogin = LstDistributionBoxCurrentFloorLogin.Count + LstLightCurrentFloorLogin.Count;
            for (int i = CurrentFloorEPSAndLightTotalCountLogin; i < cvsMainWindow.Children.Count; i++)
            {
                PartitionPoint PartitionPoint = cvsMainWindow.Children[i] as PartitionPoint;
                if (PartitionPoint == null)
                {
                    continue;
                }

                PlanPartitionPointRecordInfo infoPlanPartitionPointRecord = PartitionPoint.imgPartitionPoint.Tag as PlanPartitionPointRecordInfo;
                //LstPartitionPointCurrentFloorLogin当前楼层的报警点记录
                infoPlanPartitionPointRecord = LstPlanPartitionPointRecord.Find(x => x.ID == infoPlanPartitionPointRecord.ID);
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
                if (infoCoordinate.TransformX <= StartPositionDragFloor.X || infoCoordinate.TransformX >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || infoCoordinate.TransformY <= StartPositionDragFloor.Y || infoCoordinate.TransformY >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
                {
                    if (PartitionPoint.Visibility == System.Windows.Visibility.Visible)
                    {
                        PartitionPoint.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else
                {
                    if (PartitionPoint.Visibility == System.Windows.Visibility.Hidden)
                    {
                        PartitionPoint.Visibility = System.Windows.Visibility.Visible;
                    }
                    PartitionPoint.Width = PartitionPoint.Height = IconSearchCodeSizeLogin;
                    PartitionPoint.SetValue(Canvas.LeftProperty, infoCoordinate.TransformX);
                    PartitionPoint.SetValue(Canvas.TopProperty, infoCoordinate.TransformY);
                }

            }
        }

        /// <summary>
        /// 加载逃生路线在图层上
        /// </summary>
        private void LoadEscapeRoutesLogin()
        {
            List<CoordinateInfo> LstEscapeRoutesCoordinate = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == CurrentSelectFloorLogin);

            for (int i = 0; i < LstEscapeRoutesCoordinate.Count; i++)
            {
                AddTurningPointImage(LstEscapeRoutesCoordinate[i].TransformX, LstEscapeRoutesCoordinate[i].TransformY, cvsMainWindow.Children.Count + 1);
            }

            for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
            {
                printing(LstEscapeLinesCurrentFloorLogin[i]);
            }

            //for (int i = 0; i < LstEscapeRoutesCurrentFloorLogin.Count; i++)
            //{
            //    AddTurningPointImage(LstEscapeRoutesCurrentFloorLogin[i].TransformX, LstEscapeRoutesCurrentFloorLogin[i].TransformY, this.cvsMainWindow.Children.Count + 1);
            //}

        }

        /// <summary>
        /// 刷新报警点图标
        /// </summary>
        private void RefreshPartitionPointNoLogin()
        {
            //CurrentFloorEPSAndLightTotalCountNoLogin = MainWindowChildCount + LstDistributionBoxCurrentFloorNoLogin.Count + LstLightCurrentFloorNoLogin.Count;
            //for (int i = CurrentFloorEPSAndLightTotalCountNoLogin; i < this.cvsMainWindow.Children.Count; i++)
            //{
            //    FireAlarmLinkPoint FireAlarmLinkPoint = this.cvsMainWindow.Children[i] as FireAlarmLinkPoint;
            //    if (FireAlarmLinkPoint == null)
            //    {
            //        continue;
            //    }

            //    PlanPartitionPointRecordInfo infoPlanPartitionPointRecord = FireAlarmLinkPoint.Tag as PlanPartitionPointRecordInfo;
            //    infoPlanPartitionPointRecord = LstPartitionPointCurrentFloorNoLogin.Find(x => x.ID == infoPlanPartitionPointRecord.ID);

            //    if (infoPlanPartitionPointRecord.TransformX <= StartPositionDragFloor.X || infoPlanPartitionPointRecord.TransformX >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin / FixedScaleTransform || infoPlanPartitionPointRecord.TransformY <= StartPositionDragFloor.Y || infoPlanPartitionPointRecord.TransformY >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin / FixedScaleTransform)
            //    {
            //        if (FireAlarmLinkPoint.Visibility == System.Windows.Visibility.Visible)
            //        {
            //            FireAlarmLinkPoint.Visibility = System.Windows.Visibility.Hidden;
            //        }
            //    }
            //    else
            //    {
            //        if (FireAlarmLinkPoint.Visibility == System.Windows.Visibility.Hidden)
            //        {
            //            FireAlarmLinkPoint.Visibility = System.Windows.Visibility.Visible;
            //        }
            //        FireAlarmLinkPoint.Width = FireAlarmLinkPoint.Height = IconSearchCodeSizeNoLogin;
            //        FireAlarmLinkPoint.SetValue(Canvas.LeftProperty, infoPlanPartitionPointRecord.TransformX);
            //        FireAlarmLinkPoint.SetValue(Canvas.TopProperty, infoPlanPartitionPointRecord.TransformY);
            //    }
            //}
        }

        /// <summary>
        /// 刷新逃生路线转折点位置
        /// </summary>
        private void RefreshEscapeRoutesLogin()
        {
            LstEscapeRoutesCurrentFloorLoginScaling.Clear();
            List<CoordinateInfo> LstEscapeRoutesCoordinate = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == CurrentSelectFloorLogin);
            foreach (CoordinateInfo coord in LstEscapeRoutesCoordinate)
            {
                if (coord.TransformX >= 0 && coord.TransformX <= FloorDrawingPosition.X - IconSearchRouteCodeSize / FixedScaleTransform && coord.TransformY >= 0 && coord.TransformY <= FloorDrawingPosition.Y - IconSearchRouteCodeSize / FixedScaleTransform)
                {
                    LstEscapeRoutesCurrentFloorLoginScaling.Add(LstEscapeRoutes.Find(x => x.ID == coord.TableID));
                }
            }
            //foreach (EscapeRoutesInfo infoEscapeRoutes in LstEscapeRoutesCurrentFloorLogin)
            //{
            //    if (infoEscapeRoutes.TransformX >= 0 && infoEscapeRoutes.TransformX <= FloorDrawingPosition.X - IconSearchRouteCodeSize / FixedScaleTransform && infoEscapeRoutes.TransformY >= 0 && infoEscapeRoutes.TransformY <= FloorDrawingPosition.Y - IconSearchRouteCodeSize / FixedScaleTransform)
            //    {
            //        LstEscapeRoutesCurrentFloorLoginScaling.Add(infoEscapeRoutes);
            //    }
            //}

            DelLines();

            //EscapeRoutesInfo infoEscape;
            #region
            //List<EscapeRoutesInfo> LstHidPoint = new List<EscapeRoutesInfo>();
            List<CoordinateInfo> LstHidPoint = new List<CoordinateInfo>();
            for (int i = 0; i < LstEscapeRoutesCoordinate.Count; i++)
            {
                if ((LstEscapeRoutesCoordinate[i].TransformX > FloorDrawingTop.TransformX1 && LstEscapeRoutesCoordinate[i].TransformX < FloorDrawingTop.TransformX2) && (LstEscapeRoutesCoordinate[i].TransformY > FloorDrawingLeft.TransformY1 && LstEscapeRoutesCoordinate[i].TransformY < FloorDrawingLeft.TransformY2))
                {
                    AddTurningPointImage(LstEscapeRoutesCoordinate[i].TransformX, LstEscapeRoutesCoordinate[i].TransformY, cvsMainWindow.Children.Count + 1);
                }
                else
                {
                    LstHidPoint.Add(LstEscapeRoutesCoordinate[i]);
                }
            }

            if (LstHidPoint.Count == 0)
            {
                for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                {
                    printing(LstEscapeLinesCurrentFloorLogin[i]);
                }
            }
            else
            {
                List<EscapeLinesInfo> LstHidLines = new List<EscapeLinesInfo>();//放大后有部分不显示的线段
                //List<EscapeLinesInfo> LstVisLines = new List<EscapeLinesInfo>();//放大后全部都能显示的线段
                List<EscapeLinesInfo> LstPrinted = new List<EscapeLinesInfo>();
                for (int i = 0; i < LstHidPoint.Count; i++)
                {
                    for (int j = 0; j < LstEscapeLinesCurrentFloorLogin.Count; j++)
                    {
                        if (LstHidLines.Contains(LstEscapeLinesCurrentFloorLogin[j]))
                        {
                            continue;
                        }
                        else
                        {
                            if ((LstEscapeLinesCurrentFloorLogin[j].TransformX1 == LstHidPoint[i].TransformX && LstEscapeLinesCurrentFloorLogin[j].TransformY1 == LstHidPoint[i].TransformY) || (LstEscapeLinesCurrentFloorLogin[j].TransformX2 == LstHidPoint[i].TransformX && LstEscapeLinesCurrentFloorLogin[j].TransformY2 == LstHidPoint[i].TransformY))
                            {
                                LstHidLines.Add(LstEscapeLinesCurrentFloorLogin[j]);
                            }
                        }
                    }
                }
                for (int j = 0; j < LstHidLines.Count; j++)
                {
                    EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrentFloorLogin.Find(x => x.TransformX1 == LstHidLines[j].TransformX1 && x.TransformY1 == LstHidLines[j].TransformY1 && x.TransformX2 == LstHidLines[j].TransformX2 && x.TransformY2 == LstHidLines[j].TransformY2);
                    if (LstPrinted.Contains(infoEscapeLine))
                    {
                        continue;
                    }
                    else
                    {
                        Point intersection1 = new Point(9999, 9999);
                        Point intersection2 = new Point(9999, 9999);
                        if (JudgeIsintersect(LstHidLines[j], FloorDrawingLeft))
                        {
                            if (intersection1 != new Point(9999, 9999))
                            {
                                intersection2 = GetIntersection(LstHidLines[j], FloorDrawingLeft);
                            }
                            else
                            {
                                intersection1 = GetIntersection(LstHidLines[j], FloorDrawingLeft);
                            }
                        }

                        if (JudgeIsintersect(LstHidLines[j], FloorDrawingRight))
                        {
                            if (intersection1 != new Point(9999, 9999))
                            {
                                intersection2 = GetIntersection(LstHidLines[j], FloorDrawingRight);
                            }
                            else
                            {
                                intersection1 = GetIntersection(LstHidLines[j], FloorDrawingRight);
                            }
                        }

                        if (JudgeIsintersect(LstHidLines[j], FloorDrawingTop))
                        {
                            if (intersection1 != new Point(9999, 9999))
                            {
                                intersection2 = GetIntersection(LstHidLines[j], FloorDrawingTop);
                            }
                            else
                            {
                                intersection1 = GetIntersection(LstHidLines[j], FloorDrawingTop);
                            }
                        }

                        if (JudgeIsintersect(LstHidLines[j], FloorDrawingBottom))
                        {
                            if (intersection1 != new Point(9999, 9999))
                            {
                                intersection2 = GetIntersection(LstHidLines[j], FloorDrawingBottom);
                            }
                            else
                            {
                                intersection1 = GetIntersection(LstHidLines[j], FloorDrawingBottom);
                            }
                        }

                        if (intersection2 != new Point(9999, 9999))//存在两个交点
                        {
                            double a = Math.Sqrt((intersection1.X - infoEscapeLine.TransformX1) * (intersection1.X - infoEscapeLine.TransformX1) + (intersection1.Y - infoEscapeLine.TransformY1) * (intersection1.Y - infoEscapeLine.TransformY1));
                            double b = Math.Sqrt((intersection1.X - infoEscapeLine.TransformX2) * (intersection1.X - infoEscapeLine.TransformX2) + (intersection1.Y - infoEscapeLine.TransformY2) * (intersection1.Y - infoEscapeLine.TransformY2));
                            if (a < b)
                            {
                                infoEscapeLine.TransformX1 = intersection1.X;
                                infoEscapeLine.TransformY1 = intersection1.Y;
                                infoEscapeLine.TransformX2 = intersection2.X;
                                infoEscapeLine.TransformY2 = intersection2.Y;
                            }
                            else
                            {
                                infoEscapeLine.TransformX1 = intersection2.X;
                                infoEscapeLine.TransformY1 = intersection2.Y;
                                infoEscapeLine.TransformX2 = intersection1.X;
                                infoEscapeLine.TransformY2 = intersection1.Y;
                            }
                            printing(infoEscapeLine);
                            LstPrinted.Add(infoEscapeLine);
                        }
                        else//只存在一个交点
                        {
                            if (intersection1 != new Point(9999, 9999))//只存在一个交点
                            {
                                if (LstHidPoint.Find(x => x.TransformX == infoEscapeLine.TransformX1 && x.TransformY == infoEscapeLine.TransformY1) != null)
                                {
                                    if (infoEscapeLine.TransformX1 == intersection1.X && infoEscapeLine.TransformY1 == intersection1.Y)
                                    {
                                        if (LstHidPoint.Find(x => x.TransformX == infoEscapeLine.TransformX2 && x.TransformY == infoEscapeLine.TransformY2) != null)
                                        {
                                            LstPrinted.Add(infoEscapeLine);
                                            continue;
                                        }
                                    }
                                    infoEscapeLine.TransformX1 = intersection1.X;
                                    infoEscapeLine.TransformY1 = intersection1.Y;
                                }
                                else
                                {
                                    infoEscapeLine.TransformX2 = intersection1.X;
                                    infoEscapeLine.TransformY2 = intersection1.Y;
                                }
                                printing(infoEscapeLine);
                                LstPrinted.Add(infoEscapeLine);
                            }
                            else//不存在交点
                            {
                                LstPrinted.Add(infoEscapeLine);
                                continue;

                            }
                        }
                    }

                }


                for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                {
                    if (LstPrinted.Contains(LstEscapeLinesCurrentFloorLogin[i]))
                    {
                        continue;
                    }
                    else
                    {
                        printing(LstEscapeLinesCurrentFloorLogin[i]);
                        LstPrinted.Add(LstEscapeLinesCurrentFloorLogin[i]);
                    }
                }

            }
            #endregion

            if (LstEscapeLinesCurrentFloorLogin.Count != 0 && IsShowDirection)
            {
                //刷新逃生路线
                ShowDirection();
            }
        }

        /// <summary>
        /// 清除界面上的逃生路线
        /// </summary>
        private void DelLines()
        {
            for (int i = 0; i < cvsMainWindow.Children.Count; i++)
            {
                Line line = cvsMainWindow.Children[i] as Line;
                if (line != null)
                {
                    cvsMainWindow.Children.Remove(line);
                    i--;
                    continue;
                }

                Label label = cvsMainWindow.Children[i] as Label;
                if (label != null)
                {
                    cvsMainWindow.Children.Remove(label);
                    i--;
                    continue;
                }

                Image img = cvsMainWindow.Children[i] as Image;
                if (img != null)
                {
                    BitmapImage bitmapImage = img.Source as BitmapImage;
                    if (bitmapImage.UriSource == new Uri("/Pictures/TurningPoint-Normal.png", UriKind.Relative) || bitmapImage.UriSource == new Uri("/Pictures/Triangle.png", UriKind.Relative))
                    {
                        cvsMainWindow.Children.Remove(img);
                        i--;
                        continue;
                    }
                }

            }
        }

        private void RefreshEscapeRoutesNoLogin()
        {
            GenerateRoutes GR = new GenerateRoutes();
            LstEscapeLines = ObjEscapeLines.GetAll();
            LstEscapeRoutes = ObjEscapeRoutes.GetAll();
            LstPartitionPointCurrentFloorLogin = LstPlanPartitionPointRecord.FindAll(x => x.ID == LstCoordinate.Find(y => y.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && y.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1))).TableID && LstFireAlarmLinkZoneNumber.Contains(x.PlanPartition));
            List<Line> CurrentEscapeLine = new List<Line>();
            List<GenerateRoutes.AlarmPoint> FootDropLines = new List<GenerateRoutes.AlarmPoint>();//火灾报警点以及对应线段逃生路线发点
            if (LstPartitionPointCurrentFloorLogin.Count == 0)
            {
                CurrentEscapeLine = GR.GetInformation(CurrentSelectFloorLogin, null, null, null, LstPartitionPointCurrentFloorLogin, null, true, this);
            }
            else
            {
                List<CoordinateInfo> LstEscapeRoutesCoordinate = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
                //LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => x.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
                LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
                for (int i = 0; i < LstEscapeRoutesCoordinate.Count; i++)
                {
                    AddTurningPointImage(LstEscapeRoutesCoordinate[i].NLOriginX, LstEscapeRoutesCoordinate[i].NLOriginY, cvsMainWindow.Children.Count + 1);
                }

                for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                {
                    printing(LstEscapeLinesCurrentFloorLogin[i]);
                }
                if (LstPartitionPointCurrentFloorLogin.Count != 0 && LstEscapeRoutesCurrentFloorLogin.Count != 0 && LstEscapeLinesCurrentFloorLogin.Count != 0)
                {
                    FootDropLines = GR.GetFootDropLine(LstPartitionPointCurrentFloorLogin, LstEscapeRoutesCurrentFloorLogin, LstEscapeLinesCurrentFloorLogin, LstCoordinateCurrentFloorLogin, true);
                    CurrentEscapeLine = GR.GetInformation(CurrentSelectFloorLogin, LstEscapeLinesCurrentFloorLogin, LstEscapeRoutesCurrentFloorLogin, LstCoordinateCurrentFloorLogin, LstPartitionPointCurrentFloorLogin, FootDropLines, true, this);
                }
            }

            //TODO:
            if (CurrentEscapeLine.Count != 0)
            {
                FireLampChange(CurrentEscapeLine);
            }
            GC.Collect();
        }

        /// <summary>
        /// 发生火灾时灯具状态的变化
        /// </summary>
        /// <param name="LstLines">逃生路线</param>
        /// <param name="footDropLines">火灾报警点关联的线路</param>
        private void FireLampChange(List<Line> LstLines)
        {
            GenerateRoutes GR = new GenerateRoutes();
            TwoBrightLamp.Clear();
            LeftBrightLamp.Clear();
            RightBrightLamp.Clear();
            List<GenerateRoutes.AlarmPoint> footDropLines = GR.GetFootDropOriginLine(LstPartitionPointCurrentFloorLogin, LstEscapeRoutesCurrentFloorLogin, LstEscapeLinesCurrentFloorLogin, LstCoordinateCurrentFloorLogin);
            if (footDropLines.Count > 0)
            {
                Point Corner = new Point();//线段走向起点
                Point EndPoint = new Point();//线段走向终点
                List<LightInfo> ASSLineLamp = new List<LightInfo>();
                EscapeLinesInfo infoEscapeLine = new EscapeLinesInfo();
                List<LightInfo> TwoWayLamp = LstLight.FindAll(x => (x.LightClass == 2 || x.LightClass == 4) && x.ID == LstCoordinate.Find(y => y.TableName == EnumClass.TableName.Light.ToString() && y.Location == CurrentSelectFloorLogin).TableID);
                for (int i = 0; i < footDropLines.Count; i++)//首先处理存在火灾报警的线段上的灯具状态
                {
                    List<LightInfo> LeftLamp = new List<LightInfo>();//报警点左侧的灯具
                    List<LightInfo> RightLamp = new List<LightInfo>();//报警点右侧的灯具
                    List<LightInfo> twoBriLight = new List<LightInfo>();
                    infoEscapeLine = LstEscapeLines.Find(x => x.Location == CurrentSelectFloorLogin && x.Name == footDropLines[i].FootDropLine.Name);//火灾报警点关联的路线
                    Corner = footDropLines[i].Perpendicular;//逃生路线起点
                    TwoWayLamp = LstLight.FindAll(x => (x.LightClass == 2 || x.LightClass == 4) && x.ID == LstCoordinate.Find(y => y.TableName == EnumClass.TableName.Light.ToString() && y.Location == CurrentSelectFloorLogin).TableID);
                    ASSLineLamp = TwoWayLamp.FindAll(x => x.EscapeLineID == infoEscapeLine.ID);//路线上关联的灯具

                    if (ASSLineLamp.Count > 0)
                    {
                        List<CoordinateInfo> coordinateInfos = new List<CoordinateInfo>();
                        for (int j = 0; j < ASSLineLamp.Count; j++)
                        {
                            coordinateInfos.Add(LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == ASSLineLamp[j].ID));
                        }
                        List<light> Perpendiculars = GetPerpendicular(ASSLineLamp, footDropLines[i]);

                        if (footDropLines.FindAll(x => x.FootDropLine.Name == footDropLines[i].FootDropLine.Name).Count < 2)
                        {
                            #region 获取火灾联动，报警点附近的灯具，设置为双向
                            twoBriLight = GetTwoWayLamp(ASSLineLamp, footDropLines[i], Perpendiculars);//获取火灾联动，报警点附近的灯具，设置为双向

                            //TwoBrightLamp.AddRange(twoBriLight);
                            //ASSLineLamp.Clear();

                            for (int j = 0; j < twoBriLight.Count; j++)
                            {
                                // TODO 添加双向灯全亮到类成员变量
                                TwoBrightLamp.Add(twoBriLight[j]);
                                ASSLineLamp.Remove(twoBriLight[j]);
                            }
                            #endregion

                            #region 获取火灾报警点对应线上基于报警点分开两侧的灯具
                            if (footDropLines[i].FootDropLine.X1 == footDropLines[i].FootDropLine.X2)
                            {
                                for (int j = 0; j < coordinateInfos.Count; j++)
                                {
                                    if (coordinateInfos[j].OriginY < footDropLines[i].Perpendicular.Y)
                                    {
                                        LeftLamp.Add(ASSLineLamp[j]);
                                    }
                                    else
                                    {
                                        RightLamp.Add(ASSLineLamp[j]);
                                    }
                                }

                                if (footDropLines[i].FootDropLine.Y1 < footDropLines[i].FootDropLine.Y2)
                                {
                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(LeftLamp, Corner, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(RightLamp, Corner, EndPoint);
                                }
                                else
                                {
                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(LeftLamp, Corner, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(RightLamp, Corner, EndPoint);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < coordinateInfos.Count; j++)
                                {
                                    if (coordinateInfos[j].OriginX < footDropLines[i].Perpendicular.X)
                                    {
                                        LeftLamp.Add(ASSLineLamp[j]);
                                    }
                                    else
                                    {
                                        RightLamp.Add(ASSLineLamp[j]);
                                    }
                                }

                                if (footDropLines[i].FootDropLine.X1 < footDropLines[i].FootDropLine.X2)
                                {
                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(LeftLamp, Corner, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(RightLamp, Corner, EndPoint);
                                }
                                else
                                {
                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(LeftLamp, Corner, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(RightLamp, Corner, EndPoint);
                                }
                            }
                            #endregion

                        }
                        else
                        {
                            List<GenerateRoutes.AlarmPoint> FootDrops = GetMinORMaxFootDrops(footDropLines.FindAll(x => x.FootDropLine.Name == footDropLines[i].FootDropLine.Name));//获取两端报警点

                            for (int j = 0; j < FootDrops.Count; j++)
                            {
                                twoBriLight = GetTwoWayLamp(ASSLineLamp, FootDrops[j], Perpendiculars);//获取火灾联动，报警点附近的灯具，设置为双向

                                for (int h = 0; h < twoBriLight.Count; h++)
                                {
                                    // TODO 添加双向灯全亮到类成员变量
                                    if (!TwoBrightLamp.Contains(twoBriLight[h]))
                                    {
                                        TwoBrightLamp.Add(twoBriLight[h]);
                                    }
                                    ASSLineLamp.Remove(twoBriLight[h]);
                                }
                            }
                            twoBriLight = GetTwoWayLamp(FootDrops, Perpendiculars);//获取两个火灾报警点之间的灯具，改为双向显示
                            for (int h = 0; h < twoBriLight.Count; h++)
                            {
                                // TODO 添加双向灯全亮到类成员变量
                                if (!TwoBrightLamp.Contains(twoBriLight[h]))
                                {
                                    TwoBrightLamp.Add(twoBriLight[h]);
                                }
                                ASSLineLamp.Remove(twoBriLight[h]);
                            }

                            #region 获取火灾报警点对应线上基于报警点分开两侧的灯具
                            if (FootDrops[0].FootDropLine.X1 == FootDrops[0].FootDropLine.X2)
                            {
                                for (int j = 0; j < coordinateInfos.Count; j++)
                                {
                                    if (coordinateInfos[j].OriginY < FootDrops[0].Perpendicular.Y)
                                    {
                                        LeftLamp.Add(ASSLineLamp[j]);
                                    }
                                    if (coordinateInfos[j].OriginY > FootDrops[1].Perpendicular.Y)
                                    {
                                        RightLamp.Add(ASSLineLamp[j]);
                                    }
                                }

                                if (FootDrops[0].FootDropLine.Y1 < FootDrops[0].FootDropLine.Y2)
                                {
                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(LeftLamp, FootDrops[0].Perpendicular, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(RightLamp, FootDrops[1].Perpendicular, EndPoint);
                                }
                                else
                                {
                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(LeftLamp, FootDrops[1].Perpendicular, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(RightLamp, FootDrops[0].Perpendicular, EndPoint);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < coordinateInfos.Count; j++)
                                {
                                    if (coordinateInfos[j].OriginX < FootDrops[0].Perpendicular.X)
                                    {
                                        LeftLamp.Add(ASSLineLamp[j]);
                                    }
                                    if (coordinateInfos[j].OriginX > FootDrops[1].Perpendicular.X)
                                    {
                                        RightLamp.Add(ASSLineLamp[j]);
                                    }
                                }

                                if (FootDrops[0].FootDropLine.X1 < FootDrops[0].FootDropLine.X2)
                                {
                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(LeftLamp, FootDrops[0].Perpendicular, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(RightLamp, FootDrops[1].Perpendicular, EndPoint);
                                }
                                else
                                {
                                    EndPoint.X = infoEscapeLine.LineX2;
                                    EndPoint.Y = infoEscapeLine.LineY2;
                                    GetLampDirection(LeftLamp, FootDrops[1].Perpendicular, EndPoint);

                                    EndPoint.X = infoEscapeLine.LineX1;
                                    EndPoint.Y = infoEscapeLine.LineY1;
                                    GetLampDirection(RightLamp, FootDrops[0].Perpendicular, EndPoint);
                                }
                            }
                            #endregion
                        }
                    }
                    LstLines.RemoveAll(x => x.Name == footDropLines[i].FootDropLine.Name);
                }

                for (int i = 0; i < LstLines.Count; i++)
                {
                    infoEscapeLine = LstEscapeLines.Find(x => x.Location == CurrentSelectFloorLogin && x.Name == LstLines[i].Name);//火灾报警点关联的路线
                    ASSLineLamp = TwoWayLamp.FindAll(x => x.EscapeLineID == infoEscapeLine.ID);//路线上关联的灯具
                    if (LstLines[i].Tag != null)
                    {
                        if (infoEscapeLine.TransformX1 == ((Point)LstLines[i].Tag).X && infoEscapeLine.TransformY1 == ((Point)LstLines[i].Tag).Y)
                        {
                            Corner = new Point(infoEscapeLine.LineX1, infoEscapeLine.LineY1);
                            EndPoint = new Point(infoEscapeLine.LineX2, infoEscapeLine.LineY2);
                        }
                        if (infoEscapeLine.TransformX2 == ((Point)LstLines[i].Tag).X && infoEscapeLine.TransformY2 == ((Point)LstLines[i].Tag).Y)
                        {
                            Corner = new Point(infoEscapeLine.LineX2, infoEscapeLine.LineY2);
                            EndPoint = new Point(infoEscapeLine.LineX1, infoEscapeLine.LineY1);
                        }

                        if (ASSLineLamp.Count > 0)
                        {
                            GetLampDirection(ASSLineLamp, Corner, EndPoint);
                        }
                    }
                    else
                    {
                        TwoBrightLamp.AddRange(ASSLineLamp);
                    }
                }
            }
        }
        /// <summary>
        /// 求出灯具到相关线段上的映射坐标
        /// </summary>
        /// <param name="LstLineLamp"></param>
        /// <param name="footDropLine"></param>
        /// <returns></returns>
        private List<light> GetPerpendicular(List<LightInfo> LstLineLamp, GenerateRoutes.AlarmPoint footDropLine)
        {
            double k1;//line的斜率
            double x, y;
            Point perpendicular = new Point();
            List<light> Lstperpendicular = new List<light>();
            List<CoordinateInfo> LstCoordinates = new List<CoordinateInfo>();

            for (int i = 0; i < LstLineLamp.Count; i++)
            {
                LstCoordinates.Add(LstCoordinate.Find(a => a.TableName == EnumClass.TableName.Light.ToString() && a.TableID == LstLineLamp[i].ID));
            }

            for (int i = 0; i < LstCoordinates.Count; i++)
            {
                if (footDropLine.FootDropLine.X1 == footDropLine.FootDropLine.X2)
                {
                    //AddTurningPointImage(footDropLine.X1, fireAlarmPoint.Y, this.EscapeRoutes.Children.Count + 1);
                    perpendicular.X = footDropLine.FootDropLine.X1;
                    perpendicular.Y = LstCoordinates[i].OriginY;
                }
                else
                {
                    k1 = (footDropLine.FootDropLine.Y1 - footDropLine.FootDropLine.Y2) / (footDropLine.FootDropLine.X1 - footDropLine.FootDropLine.X2);
                    if (k1 == 0)
                    {
                        double perpendicularMinX = footDropLine.FootDropLine.X1 < footDropLine.FootDropLine.X2
                            ? footDropLine.FootDropLine.X1
                            : footDropLine.FootDropLine.X2;
                        double perpendicularMaxX = footDropLine.FootDropLine.X1 > footDropLine.FootDropLine.X2
                            ? footDropLine.FootDropLine.X1
                            : footDropLine.FootDropLine.X2;
                        if (LstCoordinates[i].OriginX <= perpendicularMinX)
                        {
                            perpendicular.X = perpendicularMinX;
                        }
                        else if (LstCoordinates[i].OriginX >= perpendicularMaxX)
                        {
                            perpendicular.X = perpendicularMaxX;
                        }
                        else
                        {
                            perpendicular.X = LstCoordinates[i].OriginX;
                        }
                        //AddTurningPointImage(fireAlarmPoint.X, footDropLine.Y1, this.EscapeRoutes.Children.Count + 1);
                        perpendicular.X = perpendicular.X;
                        perpendicular.Y = footDropLine.FootDropLine.Y1;
                    }
                    else
                    {
                        x = (LstCoordinates[i].OriginY - footDropLine.FootDropLine.Y1
                            + LstCoordinates[i].OriginX / k1 + footDropLine.FootDropLine.X1 * k1)
                            / (1 / k1 + k1);
                        perpendicular.X = x;
                        y = k1 * x - footDropLine.FootDropLine.X1 * k1 + footDropLine.FootDropLine.Y1;
                        perpendicular.Y = y;
                        //AddTurningPointImage(Math.Round(x), Math.Round(y), this.EscapeRoutes.Children.Count + 1);
                    }
                }
                light light = new light
                {
                    infolight = LstLineLamp[i],
                    perpendicular = perpendicular
                };
                Lstperpendicular.Add(light);
            }

            return Lstperpendicular;
        }

        /// <summary>
        /// 多点同线时获取两端的报警点
        /// </summary>
        /// <param name="FootDrops"></param>
        /// <returns></returns>
        private List<GenerateRoutes.AlarmPoint> GetMinORMaxFootDrops(List<GenerateRoutes.AlarmPoint> FootDrops)
        {
            GenerateRoutes.AlarmPoint MinFootDrop = FootDrops[0];
            GenerateRoutes.AlarmPoint MaxFootDrop = FootDrops[0];
            List<GenerateRoutes.AlarmPoint> result = new List<GenerateRoutes.AlarmPoint>();
            if (FootDrops[0].FootDropLine.X1 == FootDrops[0].FootDropLine.X2)
            {
                for (int i = 0; i < FootDrops.Count; i++)
                {
                    if (MinFootDrop.Perpendicular.Y > FootDrops[i].Perpendicular.Y)
                    {
                        MinFootDrop = FootDrops[i];
                    }
                    if (MaxFootDrop.Perpendicular.Y < FootDrops[i].Perpendicular.Y)
                    {
                        MaxFootDrop = FootDrops[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < FootDrops.Count; i++)
                {
                    if (MinFootDrop.Perpendicular.X > FootDrops[i].Perpendicular.X)
                    {
                        MinFootDrop = FootDrops[i];
                    }
                    if (MaxFootDrop.Perpendicular.X < FootDrops[i].Perpendicular.X)
                    {
                        MaxFootDrop = FootDrops[i];
                    }
                }
            }
            result.Add(MinFootDrop);
            result.Add(MaxFootDrop);
            return result;
        }



        /// <summary>
        /// 获取火灾联动，报警点附近的灯具，设置为双向
        /// </summary>
        /// <param name="LstLineLamp"></param>
        /// <param name="footDropLine"></param>
        /// <param name="perpendiculars"></param>
        /// <returns></returns>
        private List<LightInfo> GetTwoWayLamp(List<LightInfo> LstLineLamp, GenerateRoutes.AlarmPoint footDropLine, List<light> perpendiculars)
        {
            //double k1;//line的斜率
            //double x, y;
            //Point perpendicular = new Point();
            Point startPoint = footDropLine.Perpendicular;
            List<LightInfo> TwoWayLamp = new List<LightInfo>();
            List<double> LstDistance = new List<double>();
            List<light> Lstperpendicular = perpendiculars;

            for (int i = 0; i < Lstperpendicular.Count; i++)
            {
                LstDistance.Add(Math.Sqrt((Lstperpendicular[i].perpendicular.X - startPoint.X) * (Lstperpendicular[i].perpendicular.X - startPoint.X) + (Lstperpendicular[i].perpendicular.Y - startPoint.Y) * (Lstperpendicular[i].perpendicular.Y - startPoint.Y)));
            }
            try
            {
                int minIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    minIndex = GetMinDistance(LstDistance);
                    TwoWayLamp.Add(Lstperpendicular[minIndex].infolight);
                    Lstperpendicular.RemoveAt(minIndex);
                    LstDistance.RemoveAt(minIndex);
                }
            }
            catch
            {

            }

            return TwoWayLamp;
        }

        private List<LightInfo> GetTwoWayLamp(List<GenerateRoutes.AlarmPoint> footDropLines, List<light> perpendiculars)
        {
            List<LightInfo> TwoWayLamp = new List<LightInfo>();
            if (footDropLines[0].FootDropLine.X1 == footDropLines[0].FootDropLine.X2)
            {
                for (int i = 0; i < perpendiculars.Count; i++)
                {
                    if (perpendiculars[i].perpendicular.Y >= footDropLines[0].Perpendicular.Y && perpendiculars[i].perpendicular.Y <= footDropLines[1].Perpendicular.Y)
                    {
                        TwoWayLamp.Add(perpendiculars[i].infolight);
                    }
                }
            }
            else
            {
                for (int i = 0; i < perpendiculars.Count; i++)
                {
                    if (perpendiculars[i].perpendicular.X >= footDropLines[0].Perpendicular.X && perpendiculars[i].perpendicular.X <= footDropLines[1].Perpendicular.X)
                    {
                        TwoWayLamp.Add(perpendiculars[i].infolight);
                    }
                }
            }
            return TwoWayLamp;
        }

        /// <summary>
        /// 计算最短距离
        /// </summary>
        /// <param name="LstDistance"></param>
        /// <returns></returns>
        private int GetMinDistance(List<double> LstDistance)
        {
            try
            {
                double minDistance = LstDistance.FirstOrDefault();
                int minIndex = 0;
                for (int i = 1; i < LstDistance.Count; i++)
                {
                    if (LstDistance[i] < minDistance)
                    {
                        minDistance = LstDistance[i];
                        minIndex = i;
                    }
                }
                return minIndex;
            }
            catch (Exception ex)
            {
                LoggerManager.WriteDebug(ex.ToString());
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Lstlight"></param>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        private void GetLampDirection(List<LightInfo> Lstlight, Point StartPoint, Point EndPoint)
        {
            // TODO 添加左右亮灯具到类成员变量中
            double angle = 0;//灯具角度
            double LineAnagle = 0;//线段与水平线的角度
            Point CStartPoint = new Point();//转换后起点坐标
            Point CEndPoint = new Point();//转换后终点坐标
            List<CoordinateInfo> LstCoordinates = new List<CoordinateInfo>();

            for (int i = 0; i < Lstlight.Count; i++)
            {
                LstCoordinates.Add(LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == Lstlight[i].ID));
            }

            if (StartPoint.X == EndPoint.X)
            {
                if (StartPoint.Y < EndPoint.Y)
                {
                    for (int i = 0; i < LstCoordinates.Count; i++)
                    {
                        if (LstCoordinates[i].OriginX <= StartPoint.X)
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < LstCoordinates.Count; i++)
                    {
                        if (LstCoordinates[i].OriginX <= StartPoint.X)
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                    }
                }
            }
            else if (StartPoint.Y == EndPoint.Y)
            {
                if (StartPoint.X <= EndPoint.X)
                {
                    for (int i = 0; i < LstCoordinates.Count; i++)
                    {
                        if (LstCoordinates[i].OriginY < StartPoint.Y)
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < LstCoordinates.Count; i++)
                    {
                        if (LstCoordinates[i].OriginY < StartPoint.Y)
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < LstCoordinates.Count; i++)
                {
                    Point lightPoint = new Point(LstCoordinates[i].OriginX, LstCoordinates[i].OriginY);

                    if (lightPoint.Y >= StartPoint.Y && lightPoint.Y >= EndPoint.Y)
                    {
                        CStartPoint = new Point(StartPoint.X, Math.Abs(StartPoint.Y - lightPoint.Y));
                        CEndPoint = new Point(EndPoint.X, Math.Abs(EndPoint.Y - lightPoint.Y));
                        lightPoint = new Point(lightPoint.X, 0);
                    }
                    else
                    {
                        if (StartPoint.Y >= lightPoint.Y && StartPoint.Y >= EndPoint.Y)
                        {
                            CStartPoint = new Point(StartPoint.X, 0);
                            CEndPoint = new Point(EndPoint.X, Math.Abs(EndPoint.Y - StartPoint.Y));
                            lightPoint = new Point(lightPoint.X, Math.Abs(lightPoint.Y - StartPoint.Y));
                        }
                        else
                        {
                            if (EndPoint.Y >= lightPoint.Y && EndPoint.Y >= StartPoint.Y)
                            {
                                CStartPoint = new Point(StartPoint.X, Math.Abs(StartPoint.Y - EndPoint.Y));
                                CEndPoint = new Point(EndPoint.X, 0);
                                lightPoint = new Point(lightPoint.X, Math.Abs(lightPoint.Y - EndPoint.Y));
                            }
                        }
                    }
                    LineAnagle = Math.Atan((CStartPoint.Y - CEndPoint.Y) / (CStartPoint.X - CEndPoint.X)) * 180 / Math.PI;//线段与水平线的角度
                    angle = Math.Atan((CStartPoint.Y - lightPoint.Y) / (CStartPoint.X - lightPoint.X)) * 180 / Math.PI;
                    //angle = Math.Acos(((lightPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X) + (lightPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y)) / (Math.Sqrt((lightPoint.X - StartPoint.X) * (lightPoint.X - StartPoint.X) + (lightPoint.Y - StartPoint.Y) * (lightPoint.Y - StartPoint.Y)) * (Math.Sqrt((EndPoint.X - StartPoint.X) * (EndPoint.X - StartPoint.X) + (EndPoint.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y))))) * 180 / Math.PI;
                    if (LineAnagle < 0)
                    {
                        LineAnagle = LineAnagle + 180;
                    }
                    if (angle < 0)
                    {
                        if (lightPoint.Y > 0)
                        {
                            angle = angle + 180;
                        }
                        else
                        {
                            angle = angle + 360;
                        }
                    }
                    if (LineAnagle <= 180)
                    {
                        if (angle >= LineAnagle && angle <= LineAnagle + 180)
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                    }
                    else
                    {
                        if ((angle >= LineAnagle && angle <= 360) || (angle >= 0 && angle <= LineAnagle - 180))
                        {
                            RightBrightLamp.Add(Lstlight[i]);
                        }
                        else
                        {
                            LeftBrightLamp.Add(Lstlight[i]);
                        }
                    }

                }
            }

        }

        /// <summary>
        /// 判断两根线段是否相交
        /// </summary>
        /// <param name="line1">线段1</param>
        /// <param name="line2">线段2</param>
        /// <returns></returns>
        private bool JudgeIsintersect(EscapeLinesInfo line1, EscapeLinesInfo line2)
        {
            if (line1.TransformX1 == line1.TransformX2 || line1.TransformY1 == line1.TransformY2)
            {
                if (line1.TransformX1 == line1.TransformX2)
                {
                    if (line2.TransformX1 == line2.TransformX2)
                    {
                        return false;
                    }
                    else
                    {
                        if ((line1.TransformY1 <= line2.TransformY1 && line1.TransformY2 >= line2.TransformY1) || (line1.TransformY2 <= line2.TransformY1 && line1.TransformY1 >= line2.TransformY1))
                        {
                            if (line1.TransformX1 >= line2.TransformX1 && line1.TransformX2 <= line2.TransformX2)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                if (line1.TransformY1 == line1.TransformY2)
                {
                    if (line2.TransformY1 == line2.TransformY2)
                    {
                        return false;
                    }
                    else
                    {
                        if ((line1.TransformX1 <= line2.TransformX1 && line1.TransformX2 >= line2.TransformX1) || (line1.TransformX2 <= line2.TransformX1 && line1.TransformX1 >= line2.TransformX1))
                        {
                            if (line1.TransformY1 >= line2.TransformY1 && line1.TransformY2 <= line2.TransformY2)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            else
            {
                Point a = new Point(line1.TransformX1, line1.TransformY1);//线段ab
                Point b = new Point(line1.TransformX2, line1.TransformY2);
                Point c = new Point(line2.TransformX1, line2.TransformY1);//线段cd
                Point d = new Point(line2.TransformX2, line2.TransformY2);

                double ab = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));//ab线段长度
                double ac = Math.Sqrt((a.X - c.X) * (a.X - c.X) + (a.Y - c.Y) * (a.Y - c.Y));//ac线段长度
                double ad = Math.Sqrt((a.X - d.X) * (a.X - d.X) + (a.Y - d.Y) * (a.Y - d.Y));//ad线段长度
                double cd = Math.Sqrt((c.X - d.X) * (c.X - d.X) + (c.Y - d.Y) * (c.Y - d.Y));//cd线段长度
                double bc = Math.Sqrt((b.X - c.X) * (b.X - c.X) + (b.Y - c.Y) * (b.Y - c.Y));//bc线段长度

                double daac = (c.X - a.X) * (d.X - a.X) + (c.Y - a.Y) * (d.Y - a.Y);//向量ad与向量ac的数量积
                double baac = (b.X - a.X) * (c.X - a.X) + (b.Y - a.Y) * (c.Y - a.Y);//向量ab与向量ac的数量积
                double daab = (b.X - a.X) * (d.X - a.X) + (b.Y - a.Y) * (d.Y - a.Y);//向量ad与向量ab的数量积
                double accb = (a.X - c.X) * (b.X - c.X) + (a.Y - c.Y) * (b.Y - c.Y);//向量ca与向量cb的数量积
                double accd = (a.X - c.X) * (d.X - c.X) + (a.Y - c.Y) * (d.Y - c.Y);//向量ca与向量cd的数量积
                double bccd = (b.X - c.X) * (d.X - c.X) + (b.Y - c.Y) * (d.Y - c.Y);//向量cb与向量cd的数量积

                double dac = Math.Acos(daac / (ac * ad)) * 180 / Math.PI;//角dac的角度
                double bac = Math.Acos(baac / (ab * ac)) * 180 / Math.PI;//角bac的角度
                double dab = Math.Acos(daab / (ab * ad)) * 180 / Math.PI;//角dab的角度
                double acd = Math.Acos(accd / (ac * cd)) * 180 / Math.PI;//角acd的角度
                double acb = Math.Acos(accb / (ac * bc)) * 180 / Math.PI;//角acb的角度
                double bcd = Math.Acos(bccd / (bc * cd)) * 180 / Math.PI;//角bcd的角度

                if (bac <= dac && dab <= dac)
                {
                    if (acd <= acb && bcd <= acb)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取连线之间的交点
        /// </summary>
        /// <param name="line1">线段1</param>
        /// <param name="line2">线段2</param>
        /// <returns></returns>
        private Point GetIntersection(EscapeLinesInfo line1, EscapeLinesInfo line2)
        {
            Line ab = new Line();
            Line cd = new Line();
            Point intersection = new Point();
            ab.X1 = line1.TransformX1;
            ab.Y1 = line1.TransformY1;
            ab.X2 = line1.TransformX2;
            ab.Y2 = line1.TransformY2;
            cd.X1 = line2.TransformX1;
            cd.Y1 = line2.TransformY1;
            cd.X2 = line2.TransformX2;
            cd.Y2 = line2.TransformY2;

            if ((ab.X1 == ab.X2 && cd.Y1 == cd.Y2) || (ab.Y1 == ab.Y2 && cd.X1 == cd.X2))
            {
                if (ab.X1 == ab.X2 && cd.Y1 == cd.Y2)
                {
                    intersection.X = ab.X1;
                    intersection.Y = cd.Y2;
                }
                if (ab.Y1 == ab.Y2 && cd.X1 == cd.X2)
                {
                    intersection.X = cd.X2;
                    intersection.Y = ab.Y2;
                }
            }
            else
            {
                if (cd.X1 == cd.X2)
                {
                    intersection.X = cd.X1;
                    intersection.Y = ((ab.Y1 - ab.Y2) / (ab.X1 - ab.X2)) * (intersection.X - ab.X1) + ab.Y1;
                }
                else
                {
                    //线段方程 m*x+n*y+l=0
                    //ab线段：
                    double m0 = (ab.Y2 - ab.Y1) / (ab.X2 - ab.X1);
                    double n0 = -1;
                    double l0 = ab.Y1 - (((ab.Y2 - ab.Y1) * ab.X1) / (ab.X2 - ab.X1));
                    //cd线段:
                    double m1 = (cd.Y2 - cd.Y1) / (cd.X2 - cd.X1);
                    double n1 = -1;
                    double l1 = cd.Y1 - (((cd.Y2 - cd.Y1) * cd.X1) / (cd.X2 - cd.X1));


                    intersection.X = Math.Round(((n0 * l1) - (n1 * l0)) / ((m0 * n1) - (m1 * n0)));
                    intersection.Y = Math.Round(((m1 * l0) - (m0 * l1)) / ((m0 * n1) - (m1 * n0)));
                }
            }
            return intersection;
        }

        private void ScalingTurningPoint(EscapeRoutesInfo infoEscapeRoutes)
        {
            //if (infoEscapeRoutes.TransformX < 0)
            //{
            //    infoEscapeRoutes.TransformX = 0;
            //}
            //if (infoEscapeRoutes.TransformX > FloorDrawingPosition.X - IconSearchRouteCodeSize / FixedScaleTransform)
            //{
            //    infoEscapeRoutes.TransformX = 680;
            //}
            //if (infoEscapeRoutes.TransformY < 0)
            //{
            //    infoEscapeRoutes.TransformY = 0;
            //}
            //if (infoEscapeRoutes.TransformY > FloorDrawingPosition.Y - IconSearchRouteCodeSize / FixedScaleTransform)
            //{
            //    infoEscapeRoutes.TransformY = 380;
            //}
        }

        /// <summary>
        /// 刷新图形界面
        /// </summary>
        private void RefreshLayerModeNoLogin(bool isTimerRefresh, bool isScaleTransform)
        {
            if (!isTimerRefresh)
            {
                ClearShowIconSearchCodeInfoNoLogin(false);
            }
            RefreshIconSearchCodeNoLogin();
        }

        /// <summary>
        /// 刷新图形界面
        /// </summary>
        private void RefreshLayerModeLogin(bool isTimerRefresh, bool isScaleTransform)
        {
            if (!isTimerRefresh)
            {
                ClearAllIconPanelLogin();
            }

            if (isScaleTransform)
            {
                UpdateScaleTransformPositionLogin();
            }
            else
            {
                UpdateTranslateTransformPositionLogin();
            }

            RefreshIconSearchCodeLogin();
            if (IsPrintLine || IsEditAllIcon || IsShowDirection)//IsEditAllIcon|| IsShowDirection
            {
                RefreshPartitionPointLogin();
                RefreshEscapeRoutesLogin();
            }
        }

        /// <summary>
        /// 根据放大缩小图纸来修改当前楼层的图标坐标
        /// </summary>
        private void UpdateScaleTransformPositionNoLogin()
        {
            //foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBoxCurrentFloorNoLogin)
            //{
            //    Point PointDrawing = new Point(infoDistributionBox.NLOriginX - StartPositionDragFloor.X
            //    , infoDistributionBox.NLOriginY - StartPositionDragFloor.Y);
            //    PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

            //    Point PointScaleTransform = TransformGroupNoLogin.Transform(PointDrawing);
            //    PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

            //    infoDistributionBox.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //    infoDistributionBox.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //}

            //foreach (LightInfo infoLight in LstLightCurrentFloorNoLogin)
            //{
            //    Point PointDrawing = new Point(infoLight.NLOriginX - StartPositionDragFloor.X,
            //        infoLight.NLOriginY - StartPositionDragFloor.Y);
            //    PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

            //    Point PointScaleTransform = TransformGroupNoLogin.Transform(PointDrawing);
            //    PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

            //    infoLight.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //    infoLight.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //}

            //foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPartitionPointCurrentFloorNoLogin)
            //{
            //    Point PointDrawing = new Point(infoPlanPartitionPointRecord.OriginX - StartPositionDragFloor.X,
            //          infoPlanPartitionPointRecord.OriginY - StartPositionDragFloor.Y);
            //    PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

            //    Point PointScaleTransform = TransformGroupNoLogin.Transform(PointDrawing);
            //    PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

            //    infoPlanPartitionPointRecord.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //    infoPlanPartitionPointRecord.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeNoLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            //}

        }

        private object UpdateScaleTransformPositionNoLogin(object infoDisOrLight)
        {
            if (infoDisOrLight is DistributionBoxInfo)
            {
                DistributionBoxInfo infoDistributionBox = infoDisOrLight as DistributionBoxInfo;
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID);
                Point PointDrawing = new Point(infoCoordinate.NLOriginX - StartPositionDragFloor.X
                , infoCoordinate.NLOriginY - StartPositionDragFloor.Y);
                PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

                Point PointScaleTransform = TransformGroupNoLogin.Transform(PointDrawing);
                PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

                infoCoordinate.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (35 - OriginIconSearchCodeSize) / FixedScaleTransform;
                infoCoordinate.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (35 - OriginIconSearchCodeSize) / FixedScaleTransform;
                return infoDistributionBox;
            }
            else
            {
                LightInfo infoLight = infoDisOrLight as LightInfo;
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == infoLight.ID);
                Point PointDrawing = new Point(infoCoordinate.NLOriginX - StartPositionDragFloor.X,
                   infoCoordinate.NLOriginY - StartPositionDragFloor.Y);
                PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

                Point PointScaleTransform = TransformGroupNoLogin.Transform(PointDrawing);
                PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

                infoCoordinate.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (35 - OriginIconSearchCodeSize) / FixedScaleTransform;
                infoCoordinate.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (35 - OriginIconSearchCodeSize) / FixedScaleTransform;
                return infoLight;
            }
        }

        /// <summary>
        /// 根据放大缩小图纸来修改当前楼层的图标坐标
        /// </summary>
        private void UpdateScaleTransformPositionLogin()
        {
            LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
            foreach (CoordinateInfo infoCoordinate in LstCoordinateCurrentFloorLogin)
            {
                Point PointDrawing = new Point(infoCoordinate.OriginX - StartPositionDragFloor.X, infoCoordinate.OriginY - StartPositionDragFloor.Y);
                PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

                Point PointScaleTransform = TransformGroupLogin.Transform(PointDrawing);
                PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

                infoCoordinate.TransformX = PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
                infoCoordinate.TransformY = PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform;
            }
            ObjCoordinate.Save(LstCoordinateCurrentFloorLogin);
            LstCoordinate = ObjCoordinate.GetAll();
            LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

            LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);
            foreach (EscapeLinesInfo infoEscapeLine in LstEscapeLinesCurrentFloorLogin)
            {
                CoordinateInfo infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.OriginX == infoEscapeLine.LineX1 && x.OriginY == infoEscapeLine.LineY1);
                infoEscapeLine.TransformX1 = infoCoordinate.TransformX;
                infoEscapeLine.TransformY1 = infoCoordinate.TransformY;

                infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.OriginX == infoEscapeLine.LineX2 && x.OriginY == infoEscapeLine.LineY2);
                infoEscapeLine.TransformX2 = infoCoordinate.TransformX;
                infoEscapeLine.TransformY2 = infoCoordinate.TransformY;
            }

            if (BlankIcon.Tag != null)
            {
                Point PointDrawing = new Point(((LayerImageTag)BlankIcon.Tag).OriginPoint.X - StartPositionDragFloor.X, ((LayerImageTag)BlankIcon.Tag).OriginPoint.Y - StartPositionDragFloor.Y);
                PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

                Point PointScaleTransform = TransformGroupLogin.Transform(PointDrawing);
                PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

                BlankIcon.Margin = new Thickness(PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, 0, 0);
            }
        }

        /// <summary>
        /// 根据平移图纸来修改当前楼层的配电箱或者灯具坐标
        /// </summary>
        private void UpdateTranslateTransformPositionNoLogin()
        {
            //LstDistributionBoxCurrentFloorNoLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorNoLogin.X - LastDragFloorNoLogin.X);
            //LstDistributionBoxCurrentFloorNoLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorNoLogin.Y - LastDragFloorNoLogin.Y);
            //LstLightCurrentFloorNoLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorNoLogin.X - LastDragFloorNoLogin.X);
            //LstLightCurrentFloorNoLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorNoLogin.Y - LastDragFloorNoLogin.Y);
            //LstPartitionPointCurrentFloorNoLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorNoLogin.X - LastDragFloorNoLogin.X);
            //LstPartitionPointCurrentFloorNoLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorNoLogin.Y - LastDragFloorNoLogin.Y);
            //LastDragFloorNoLogin = DragFloorNoLogin;
        }

        /// <summary>
        /// 根据平移图纸来修改当前楼层的配电箱或者灯具坐标
        /// </summary>
        private void UpdateTranslateTransformPositionLogin()
        {
            #region 旧版程序
            //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorLogin.X - LastDragFloorLogin.X);
            //LstDistributionBoxCurrentFloorLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorLogin.Y - LastDragFloorLogin.Y);
            //LstLightCurrentFloorLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorLogin.X - LastDragFloorLogin.X);
            //LstLightCurrentFloorLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorLogin.Y - LastDragFloorLogin.Y);
            //LstPartitionPointCurrentFloorLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorLogin.X - LastDragFloorLogin.X);
            //LstPartitionPointCurrentFloorLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorLogin.Y - LastDragFloorLogin.Y);
            ////逃生路线转折点
            //LstEscapeRoutesCurrentFloorLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorLogin.X - LastDragFloorLogin.X);
            //LstEscapeRoutesCurrentFloorLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorLogin.Y - LastDragFloorLogin.Y);
            #endregion
            LstCoordinateCurrentFloorLogin.ForEach(x => x.TransformX = x.TransformX + DragFloorLogin.X - LastDragFloorLogin.X);
            LstCoordinateCurrentFloorLogin.ForEach(x => x.TransformY = x.TransformY + DragFloorLogin.Y - LastDragFloorLogin.Y);
            LastDragFloorLogin = DragFloorLogin;
            if (IsPrintLine || IsEditAllIcon || IsShowDirection)
            {
                UpdateScaleTransformPositionLogin();
                RefreshEscapeRoutesLogin();
            }
        }

        /// <summary>
        /// 拖动图纸上的EPS和灯具图标
        /// </summary>
        private bool MoveIconSearchCodeLogin(object sender, MouseEventArgs e)
        {
            if (MoveEquipment != null)
            {
                if (!(MoveEquipment.Tag is PlanPartitionPointRecordInfo))
                {
                    object infoDisBoxOrLight = ((LayerImageTag)MoveEquipment.Tag).equipment;
                    LayerImageTag tag = new LayerImageTag();
                    DistributionBoxInfo infoDistributionBox = new DistributionBoxInfo();
                    CoordinateInfo infoCoordinate = new CoordinateInfo();
                    LightInfo infoLight = new LightInfo();
                    BlankIconInfo infoBlankIcon = new BlankIconInfo();
                    if (infoDisBoxOrLight is DistributionBoxInfo || infoDisBoxOrLight is LightInfo || infoDisBoxOrLight is BlankIconInfo)
                    {
                        int ID = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight));
                        if (infoDisBoxOrLight is DistributionBoxInfo)
                        {
                            infoDistributionBox = LstDistributionBox.Find(x => x.ID == ID);
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID);
                            if (infoCoordinate.IsAuth == 1)
                            {
                                return false;
                            }
                        }
                        if (infoDisBoxOrLight is LightInfo)
                        {
                            infoLight = LstLight.Find(x => x.ID == ID);
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == infoLight.ID);
                            if (infoCoordinate.IsAuth == 1)
                            {
                                return false;
                            }
                        }
                        if (infoDisBoxOrLight is BlankIconInfo)
                        {
                            infoBlankIcon = LstBlankIcon.Find(x => x.ID == ID);
                            infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == infoBlankIcon.ID);
                            if (infoCoordinate.IsAuth == 1)
                            {
                                return false;
                            }
                        }
                    }

                    NewMoveAllIconLogin = e.GetPosition(ctcFloorDrawingLogin);
                    //Console.WriteLine(NewMoveAllIconLogin);
                    OriginMoveAllIconLogin = TransformGroupLogin.Inverse.Transform(NewMoveAllIconLogin);
                    NewMoveAllIconNoLogin = NewMoveAllIconLogin;
                    OriginMoveAllIconNoLogin = OriginMoveAllIconLogin;

                    #region 登录后图形图标坐标记录
                    NewMoveAllIconLogin = new Point(NewMoveAllIconLogin.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, NewMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform);
                    OriginMoveAllIconLogin = new Point(OriginMoveAllIconLogin.X + StartPositionDragFloor.X, OriginMoveAllIconLogin.Y + StartPositionDragFloor.Y);//(IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / MaxScaleTransform

                    NewMoveAllIconLogin = new Point(Math.Round(NewMoveAllIconLogin.X), Math.Round(NewMoveAllIconLogin.Y));
                    OriginMoveAllIconLogin = new Point(Math.Round(OriginMoveAllIconLogin.X), Math.Round(OriginMoveAllIconLogin.Y));
                    #endregion

                    if (OriginMoveAllIconLogin.X <= 220)
                    {
                        OriginMoveAllIconLogin.X = 220;
                    }
                    if (OriginMoveAllIconLogin.X >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.X = StartPositionDragFloor.X + FloorDrawingPosition.X - 30;
                    }
                    if (OriginMoveAllIconLogin.Y <= 140)
                    {
                        OriginMoveAllIconLogin.Y = 140;
                    }
                    if (OriginMoveAllIconLogin.Y >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.Y = StartPositionDragFloor.Y + FloorDrawingPosition.Y - 30;
                    }

                    #region 登录前图形图标坐标记录
                    ComputePointNoLogin(NewMoveAllIconLogin, OriginMoveAllIconLogin);
                    #endregion

                    if (infoDisBoxOrLight is DistributionBoxInfo)
                    {
                        UpdateScaleTransformPositionNoLogin(infoDistributionBox);
                        infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                        infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                        infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                        ObjCoordinate.Update(infoCoordinate);
                        LstCoordinate = ObjCoordinate.GetAll();
                        LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                        //ObjDistributionBox.Update(infoDistributionBox);
                        //LstDistributionBox = ObjDistributionBox.GetAll();
                        //LstDistributionBoxCurrentFloorLogin = LstDistributionBox.FindAll(x => x.Location == CurrentSelectFloorLogin);
                        tag.equipment = infoDistributionBox;
                        tag.status = infoDistributionBox.Status;
                        MoveEquipment.Tag = tag;
                    }
                    else if (infoDisBoxOrLight is LightInfo)
                    {
                        UpdateScaleTransformPositionNoLogin(infoLight);
                        infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                        infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                        infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                        ObjCoordinate.Update(infoCoordinate);
                        LstCoordinate = ObjCoordinate.GetAll();
                        LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                        //ObjLight.Update(infoLight);
                        //LstLight = ObjLight.GetAll();
                        //LstLightCurrentFloorLogin = LstLight.FindAll(x => x.Location == CurrentSelectFloorLogin);
                        tag.equipment = infoLight;
                        tag.status = infoLight.Status;
                        MoveEquipment.Tag = tag;
                    }
                    else if (infoDisBoxOrLight is BlankIconInfo)
                    {
                        infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                        infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                        infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                        ObjCoordinate.Update(infoCoordinate);
                        LstCoordinate = ObjCoordinate.GetAll();
                        LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                    }
                    else
                    {
                        tag.equipment = ((LayerImageTag)BlankIcon.Tag).equipment;
                        tag.status = ((LayerImageTag)BlankIcon.Tag).status;
                        tag.OriginPoint = new Point(OriginMoveAllIconLogin.X, OriginMoveAllIconLogin.Y);
                        BlankIcon.Tag = tag;
                        BlankIcon.Margin = new Thickness(NewMoveAllIconLogin.X, NewMoveAllIconLogin.Y, 0, 0);
                    }
                    RefreshIconSearchCodeLogin();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 拖动图纸上的EPS和灯具图标
        /// </summary>
        private bool MoveIconSearchCodeLogin(object sender, TouchEventArgs e)
        {
            if (MoveEquipment != null)
            {
                if (!(MoveEquipment.Tag is PlanPartitionPointRecordInfo))
                {
                    if (IsShowIconSearchCodePanelLogin)
                    {
                        return false;
                    }

                    object infoDisBoxOrLight = ((LayerImageTag)MoveEquipment.Tag).equipment;
                    int ID = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight));
                    int isAuth = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("IsAuth").GetValue(infoDisBoxOrLight));
                    if (isAuth == 1)
                    {
                        return false;
                    }

                    NewMoveAllIconLogin = e.GetTouchPoint(ctcFloorDrawingLogin).Position;
                    OriginMoveAllIconLogin = TransformGroupLogin.Inverse.Transform(NewMoveAllIconLogin);
                    NewMoveAllIconNoLogin = NewMoveAllIconLogin;
                    OriginMoveAllIconNoLogin = OriginMoveAllIconLogin;

                    #region 登录后图形图标坐标记录
                    NewMoveAllIconLogin = new Point(NewMoveAllIconLogin.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, NewMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform);
                    OriginMoveAllIconLogin = new Point(OriginMoveAllIconLogin.X + StartPositionDragFloor.X, OriginMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / MaxScaleTransform);

                    NewMoveAllIconLogin = new Point(Math.Round(NewMoveAllIconLogin.X), Math.Round(NewMoveAllIconLogin.Y));
                    OriginMoveAllIconLogin = new Point(Math.Round(OriginMoveAllIconLogin.X), Math.Round(OriginMoveAllIconLogin.Y));
                    #endregion

                    if (OriginMoveAllIconLogin.X <= 220)
                    {
                        OriginMoveAllIconLogin.X = 220;
                    }
                    if (OriginMoveAllIconLogin.X >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.X = StartPositionDragFloor.X + FloorDrawingPosition.X - 30;
                    }
                    if (OriginMoveAllIconLogin.Y <= 140)
                    {
                        OriginMoveAllIconLogin.Y = 140;
                    }
                    if (OriginMoveAllIconLogin.Y >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.Y = StartPositionDragFloor.Y + FloorDrawingPosition.Y - 30;
                    }


                    #region 登录前图形图标坐标记录
                    ComputePointNoLogin(NewMoveAllIconLogin, OriginMoveAllIconLogin);
                    #endregion


                    bool isDistributionBoxInfo = infoDisBoxOrLight is DistributionBoxInfo;
                    if (isDistributionBoxInfo)
                    {
                        DistributionBoxInfo infoDistributionBox = LstDistributionBoxCurrentFloorLogin.Find(x => x.ID == ID);
                        CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID);
                        infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                        infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                        infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                    }
                    else
                    {
                        LightInfo infoLight = LstLightCurrentFloorLogin.Find(x => x.ID == ID);
                        CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == infoLight.ID);
                        infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                        infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                        infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 移动报警点图标
        /// </summary>       
        private void MovePartitionPointLogin(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (MoveEquipment != null)
                {
                    if (MoveEquipment.Tag is PlanPartitionPointRecordInfo)
                    {
                        //NewMoveAllIconLogin移动图层上图标的转换坐标
                        NewMoveAllIconLogin = e.GetPosition(ctcFloorDrawingLogin);
                        //OriginMoveAllIconLogin移动图层上图标的原始坐标  TransformGroupLogin进行图纸变换的组件
                        OriginMoveAllIconLogin = TransformGroupLogin.Inverse.Transform(NewMoveAllIconLogin);

                        NewMoveAllIconLogin = new Point(NewMoveAllIconLogin.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, NewMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform);
                        OriginMoveAllIconLogin = new Point(OriginMoveAllIconLogin.X + StartPositionDragFloor.X, OriginMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / MaxScaleTransform);

                        NewMoveAllIconLogin = new Point(Math.Round(NewMoveAllIconLogin.X), Math.Round(NewMoveAllIconLogin.Y));
                        OriginMoveAllIconLogin = new Point(Math.Round(OriginMoveAllIconLogin.X), Math.Round(OriginMoveAllIconLogin.Y));

                        if (OriginMoveAllIconLogin.X <= 240)
                        {
                            OriginMoveAllIconLogin.X = 240;
                        }
                        if (OriginMoveAllIconLogin.X >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin)
                        {
                            OriginMoveAllIconLogin.X = StartPositionDragFloor.X + FloorDrawingPosition.X - 20;
                        }
                        if (OriginMoveAllIconLogin.Y <= 150)
                        {
                            OriginMoveAllIconLogin.Y = 150;
                        }
                        if (OriginMoveAllIconLogin.Y >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin)
                        {
                            OriginMoveAllIconLogin.Y = StartPositionDragFloor.Y + FloorDrawingPosition.Y - 20;
                        }

                        ComputePointNoLogin(NewMoveAllIconLogin, OriginMoveAllIconLogin);

                        PlanPartitionPointRecordInfo infoPlanPartitionPointRecord = MoveEquipment.Tag as PlanPartitionPointRecordInfo;
                        if (infoPlanPartitionPointRecord != null)
                        {
                            CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
                            infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                            infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                            infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                            infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                            infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                            infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                            ObjCoordinate.Update(infoCoordinate);
                            LstCoordinate = ObjCoordinate.GetAll();
                            //ObjPlanPartitionPointRecord.Update(infoPlanPartitionPointRecord);
                            //LstPlanPartitionPointRecord = ObjPlanPartitionPointRecord.GetAll();
                        }
                    }
                    else
                    {
                        bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                        if (isSuccess)
                        {
                            ClearAllIconPanelLogin();
                            RefreshIconSearchCodeLogin();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 移动报警点图标
        /// </summary>      
        private void MovePartitionPointLogin(object sender, TouchEventArgs e)
        {
            if (MoveEquipment != null)
            {
                if (MoveEquipment.Tag is PlanPartitionPointRecordInfo)
                {
                    NewMoveAllIconLogin = e.GetTouchPoint(ctcFloorDrawingLogin).Position;
                    OriginMoveAllIconLogin = TransformGroupLogin.Inverse.Transform(NewMoveAllIconLogin);

                    NewMoveAllIconLogin = new Point(NewMoveAllIconLogin.X + StartPositionDragFloor.X - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform, NewMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / FixedScaleTransform);
                    OriginMoveAllIconLogin = new Point(OriginMoveAllIconLogin.X + StartPositionDragFloor.X, OriginMoveAllIconLogin.Y + StartPositionDragFloor.Y - (IconSearchCodeSizeLogin - OriginIconSearchCodeSize) / MaxScaleTransform);

                    NewMoveAllIconLogin = new Point(Math.Round(NewMoveAllIconLogin.X), Math.Round(NewMoveAllIconLogin.Y));
                    OriginMoveAllIconLogin = new Point(Math.Round(OriginMoveAllIconLogin.X), Math.Round(OriginMoveAllIconLogin.Y));

                    if (OriginMoveAllIconLogin.X <= 220)
                    {
                        OriginMoveAllIconLogin.X = 220;
                    }
                    if (OriginMoveAllIconLogin.X >= StartPositionDragFloor.X + FloorDrawingPosition.X - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.X = StartPositionDragFloor.X + FloorDrawingPosition.X - 30;
                    }
                    if (OriginMoveAllIconLogin.Y <= 140)
                    {
                        OriginMoveAllIconLogin.Y = 140;
                    }
                    if (OriginMoveAllIconLogin.Y >= StartPositionDragFloor.Y + FloorDrawingPosition.Y - IconSearchCodeSizeLogin)
                    {
                        OriginMoveAllIconLogin.Y = StartPositionDragFloor.Y + FloorDrawingPosition.Y - 30;
                    }

                    ComputePointNoLogin(NewMoveAllIconLogin, OriginMoveAllIconLogin);

                    PlanPartitionPointRecordInfo infoPlanPartitionPointRecord = MoveEquipment.Tag as PlanPartitionPointRecordInfo;
                    infoPlanPartitionPointRecord = LstPartitionPointCurrentFloorLogin.Find(x => x.ID
                    == infoPlanPartitionPointRecord.ID);
                    CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.ID == infoPlanPartitionPointRecord.ID);
                    infoCoordinate.OriginX = OriginMoveAllIconLogin.X;
                    infoCoordinate.OriginY = OriginMoveAllIconLogin.Y;
                    infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                    infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                    infoCoordinate.TransformX = NewMoveAllIconLogin.X;
                    infoCoordinate.TransformY = NewMoveAllIconLogin.Y;
                }
                else
                {
                    bool isSuccess = MoveIconSearchCodeLogin(sender, e);
                    if (isSuccess)
                    {
                        ClearAllIconPanelLogin();
                        RefreshIconSearchCodeLogin();
                    }
                }
            }
        }

        /// <summary>
        /// 保存导入的图片
        /// </summary>
        /// <param name="serverImagePath">图片原路径</param>
        /// <param name="thumbnailImagePath">图片新路径</param>
        private async Task GetPicture(string serverImagePath, string thumbnailImagePath)
        {
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(serverImagePath))
            {
                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(img);
                System.Drawing.Image pricture = resizeImage(bm, new Size(740, 450));
                pricture.Save(thumbnailImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                await Task.Delay(100);
            }
                
        }

        private async Task GetPictureTFile(string serverImagePath, string thumbnailImagePath)
        {
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(serverImagePath))
            {
                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(img);
                System.Drawing.Image pricture = resizeImage(bm, new Size(740, 450));
                //File.Delete(serverImagePath);
                //File.Copy(strNewFilePath, $"{FloorDrawingPath}\\{strNewFileName}", true);
                pricture.Save($"{FloorDrawingPath}\\beizhu", System.Drawing.Imaging.ImageFormat.Jpeg);
                //img.Dispose();
                File.Delete(serverImagePath);
                File.Copy($"{FloorDrawingPath}\\beizhu", thumbnailImagePath, true);
                File.Delete($"{FloorDrawingPath}\\beizhu");
                await Task.Delay(100);
            }
                
        }

        /// <summary>
        /// 修改图片尺寸
        /// </summary>
        /// <param name="imgToResize">图片参数</param>
        /// <param name="size">图片尺寸比例</param>
        /// <returns></returns>
        private System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;
            int nPercentW = 0;
            int nPercentH = 0;
            int destWidth = 0;
            int destHeight = 0;

            nPercentH = Convert.ToInt32((sourceWidth / size.Width) * size.Height);
            nPercentW = Convert.ToInt32((sourceHeight / size.Height) * size.Width);

            if (nPercentH < sourceHeight)
            {
                destHeight = nPercentH;
                destWidth = sourceWidth;
            }
            else
            {
                destHeight = sourceHeight;
                destWidth = nPercentW;
            }

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(destWidth, destHeight);
            System.Drawing.Graphics gh = System.Drawing.Graphics.FromImage(bm);
            gh.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gh.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            gh.Dispose();
            return bm;
        }

        /// <summary>
        /// 导入图纸
        /// </summary>
        private async Task ImportFloorDrawing()
        {
            int con = LstFloorName.Count;
            PointEdit.Visibility = System.Windows.Visibility.Hidden;
            System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult DialogResult = FolderBrowserDialog.ShowDialog();
            if (DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                IsLoadPicture = false;
            }

            //ObjFloorName.DeleteAll();
            List<string> supportedFormats = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".svg" }; // 支持的图纸格式
            List<string> dbFloorNames = ObjFloorName.GetAll().Select(it => it.FloorName).ToList();  // 获取数据库楼层名称
            string strSelectedPath = FolderBrowserDialog.SelectedPath; // 选中文件的路径
            string[] files = Directory.GetFiles(strSelectedPath);
            FloorNameInfo infoFloorName = new FloorNameInfo();
            foreach (string strNewFilePath in files)
            {
                string strNewFileName = System.IO.Path.GetFileName(strNewFilePath);
                if (dbFloorNames.Contains(strNewFileName))
                {
                    continue;
                }
                // 数据库无记录时继续
                string extension = System.IO.Path.GetExtension(strNewFileName);
                if (supportedFormats.Contains(extension))
                {
                    //如果程序读取路径下不存在文件复制文件，否则直接写入数据库
                    if (strNewFilePath != $"{FloorDrawingPath}\\{strNewFileName}")
                    {
                        //File.Copy(strNewFilePath, $"{FloorDrawingPath}\\{strNewFileName}", true);
                        await GetPicture(strNewFilePath, $"{FloorDrawingPath}\\{strNewFileName}");
                    }
                    else
                    {
                        await GetPictureTFile(strNewFilePath, $"{FloorDrawingPath}\\{strNewFileName}");
                    }

                    infoFloorName.FloorName = strNewFileName;
                    LstFloorName.Add(infoFloorName);
                    ObjFloorName.Add(infoFloorName);
                }
            }

            if (LstFloorName.Count > con)
            {
                CommonFunct.PopupWindow("导入图纸成功！");
                CurrentSelectFloorLogin = 1;
                LoadFloorDrawingLogin();
            }
            else
            {
                CommonFunct.PopupWindow("导入图纸失败！");
            }
            IsLoadPicture = true;
        }

        /// <summary>
        /// 删除当前楼层图纸
        /// </summary>
        private void DelFloorDrawing()
        {
            int currentFloorNum = 0;
            LstFloorName = ObjFloorName.GetAll();
            if (CurrentSelectFloorLogin > LstFloorName.Count)
            {
                System.IO.File.Delete(string.Format("{0}\\{1}", FloorDrawingPath, LstFloorName[LstFloorName.Count - 1].FloorName));
                ObjFloorName.Delete(LstFloorName[LstFloorName.Count - 1].ID);
                LstFloorName.RemoveAt(LstFloorName.Count - 1);
                currentFloorNum = LstFloorName.Count;
            }
            else
            {
                System.IO.File.Delete(string.Format("{0}\\{1}", FloorDrawingPath, LstFloorName[CurrentSelectFloorLogin - 1].FloorName));
                ObjFloorName.Delete(LstFloorName[CurrentSelectFloorLogin - 1].ID);
                LstFloorName.RemoveAt(CurrentSelectFloorLogin - 1);
                currentFloorNum = CurrentSelectFloorLogin;
            }

            List<CoordinateInfo> LstCoordinates = LstCoordinate.FindAll(x => x.Location == currentFloorNum);
            LstCoordinates.ForEach(x => x.Location = 0);
            LstCoordinates.ForEach(x => x.NLOriginX = 0);
            LstCoordinates.ForEach(x => x.NLOriginX = x.NLOriginY = x.OriginX = x.OriginY = x.TransformX = x.TransformY = 0);

            LstCoordinate.FindAll(x => x.Location > currentFloorNum).ForEach(x => x.Location = x.Location - 1);
            ObjCoordinate.Save(LstCoordinates);
            ObjCoordinate.Save(LstCoordinate);

            LstCoordinate = ObjCoordinate.GetAll();
            CurrentSelectFloorLogin = 1;
            imgFloorDrawingLogin.Source = null;
            SwitchFloorLogin();
        }

        /// <summary>
        /// 编辑图纸上的EPS和灯具图标
        /// </summary>
        private void EditAllIcon()
        {
            LstCoordinate.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 0);
            ObjCoordinate.Save(LstCoordinate);
            //LstDistributionBox.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 0);
            //LstLight.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 0);
            //ObjDistributionBox.Save(LstDistributionBox.FindAll(x => x.Location != 0));
            //ObjLight.Save(LstLight.FindAll(x => x.Location != 0));
            CommonFunct.PopupWindow("图形界面进入编辑状态！");
        }

        /// <summary>
        /// 保存图纸上的所有图标
        /// </summary>
        private void SaveAllIcon()
        {
            LstCoordinate.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            ObjCoordinate.Save(LstCoordinate.FindAll(x => x.Location != 0));
            //LstDistributionBox.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            //LstLight.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            //ObjDistributionBox.Save(LstDistributionBox.FindAll(x => x.Location != 0));
            //ObjLight.Save(LstLight.FindAll(x => x.Location != 0));
            //ObjPlanPartitionPointRecord.Save(LstPlanPartitionPointRecord);
            //ObjEscapeRoutes.Save(LstEscapeRoutes);
            //ObjEscapeLines.Save(LstEscapeLines);
            CommonFunct.PopupWindow("图形界面保存成功！");
        }

        /// <summary>
        /// 强制保存图纸上的图标
        /// </summary>
        private void ForceSaveIconSearchCodeLogin()
        {
            LstCoordinate.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            ObjCoordinate.Save(LstCoordinate.FindAll(x => x.Location != 0));
            //LstDistributionBox.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            //LstLight.FindAll(x => x.Location != 0).ForEach(x => x.IsAuth = 1);
            //ObjDistributionBox.Save(LstDistributionBox.FindAll(x => x.Location != 0));
            //ObjLight.Save(LstLight.FindAll(x => x.Location != 0));
        }

        /// <summary>
        /// 保存图纸上的图标
        /// </summary>
        private void AuthIconSearchCode()
        {
            List<CoordinateInfo> LstCoodinateAllFloor = LstCoordinate.FindAll(x => x.Location != 0 && x.IsAuth == 0);
            if (LstCoodinateAllFloor.Count != 0)
            {
                ObjCoordinate.Save(LstCoodinateAllFloor);
                LstCoordinate = ObjCoordinate.GetAll();
            }

            //LstDistributionBoxAllFloor = LstDistributionBox.FindAll(x => x.Location != 0 && x.IsAuth == 0);
            //LstLightAllFloor = LstLight.FindAll(x => x.Location != 0 && x.IsAuth == 0);

            //if (LstDistributionBoxAllFloor.Count != 0)
            //{
            //    ObjDistributionBox.Save(LstDistributionBoxAllFloor);
            //}

            //if (LstLightAllFloor.Count != 0)
            //{
            //    ObjLight.Save(LstLightAllFloor);
            //}
        }

        /// <summary>
        /// 获取消音设置内容
        /// </summary>
        private void GetMuteSetContent()
        {
            //LstOtherFaultRecord = ObjOtherFaultRecord.GetAll();
            OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgHostFault.Tag.ToString());
            if (infoOtherFaultRecord.Disable == 1)
            {
                ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Selected.png", UriKind.Relative));
            }
            else
            {
                ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Unchecked.png", UriKind.Relative));
            }
            #region 旧（存在故障才屏蔽）
            //if (infoOtherFaultRecord.IsExist == 1)
            //{
            //    if (infoOtherFaultRecord.Disable == 1)
            //    {
            //        this.ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Selected.png", UriKind.Relative));
            //    }
            //    else
            //    {
            //        this.ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Unchecked.png", UriKind.Relative));
            //    }
            //}
            //else
            //{
            //    this.ImgHostFault.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Unchecked.png", UriKind.Relative));
            //}
            #endregion

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgEPSFault.Tag.ToString());
            if (infoOtherFaultRecord.Disable == 1)
            {
                ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Selected.png", UriKind.Relative));
            }
            else
            {
                ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Unchecked.png", UriKind.Relative));
            }

            #region 旧(存在故障才屏蔽)
            //int xx = infoOtherFaultRecord.IsExist;
            //if (infoOtherFaultRecord.IsExist == 1)
            //{
            //    if (infoOtherFaultRecord.Disable == 1)
            //    {
            //        this.ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Selected.png", UriKind.Relative));
            //    }
            //    else
            //    {
            //        this.ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Unchecked.png", UriKind.Relative));
            //    }
            //}
            //else
            //{
            //    this.ImgEPSFault.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Unchecked.png", UriKind.Relative));
            //}
            #endregion

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgLampFault.Tag.ToString());
            if (infoOtherFaultRecord.Disable == 1)
            {
                ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Selected.png", UriKind.Relative));
            }
            else
            {
                ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Unchecked.png", UriKind.Relative));
            }
            #region 旧（存在故障才屏蔽）
            //if (infoOtherFaultRecord.IsExist == 1)
            //{
            //    if (infoOtherFaultRecord.Disable == 1)
            //    {
            //        this.ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Selected.png", UriKind.Relative));
            //    }
            //    else
            //    {
            //        this.ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Unchecked.png", UriKind.Relative));
            //    }
            //}
            //else
            //{
            //    this.ImgLampFault.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Unchecked.png", UriKind.Relative));
            //}
            #endregion

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgDelaySetting.Tag.ToString());
            if (infoOtherFaultRecord.Disable == 1)
            {
                ImgDelaySetting.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Selected.png", UriKind.Relative));
            }
            else
            {
                ImgDelaySetting.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Unchecked.png", UriKind.Relative));
            }
            #region 旧（存在故障才屏蔽）
            if (infoOtherFaultRecord.IsExist == 1)
            {
                if (infoOtherFaultRecord.Disable == 1)
                {
                    ImgDelaySetting.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Selected.png", UriKind.Relative));
                }
                else
                {
                    ImgDelaySetting.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Unchecked.png", UriKind.Relative));
                }
            }
            else
            {
                ImgDelaySetting.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Unchecked.png", UriKind.Relative));
            }
            #endregion
        }

        /// <summary>
        /// 加载火灾联动信息
        /// </summary>
        private void LoadFireAlarmLinkInfo()
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsAutoFireAlarmLink");
            bool isAutoFireAlarmLink = Convert.ToBoolean(infoGblSetting.SetValue);
            if (isAutoFireAlarmLink)
            {
                AutomaticReceptionYes.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                AutomaticReceptionNo.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
            }
            else
            {
                AutomaticReceptionYes.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
                AutomaticReceptionNo.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }

            infoGblSetting = LstGblSetting.Find(x => x.Key == "IsFireAlarmLinkNormal");
            bool isFireAlarmLinkNormal = Convert.ToBoolean(infoGblSetting.SetValue);
            if (isFireAlarmLinkNormal)
            {
                LinkageUnificationYes.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                LinkageUnificationNo.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
            }
            else
            {
                LinkageUnificationYes.Source = new BitmapImage(new Uri("\\Pictures\\UnSelected.png", UriKind.Relative));
                LinkageUnificationNo.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// 加载火灾报警器数据
        /// </summary>
        private void LoadFireAlarmTypeData()
        {
            List<string> LstFireAlarmName = LstFireAlarmType.Select(x => x.FireAlarmName).ToList<string>();
            FireAlarmType.ItemsSource = null;
            FireAlarmType.ItemsSource = LstFireAlarmName;
            FireAlarmType.SelectedIndex = LstFireAlarmType.FindIndex(x => x.IsCurrentFireAlarm == 1);
        }

        /// <summary>
        /// 获取时间文本框
        /// </summary>      
        private void GetFocusTime(object sender)
        {
            if (FocusTime != null)
            {
                FocusTime.Foreground = new SolidColorBrush(Colors.White);
            }
            FocusTime = sender as TextBox;
            FocusTime.Foreground = new SolidColorBrush(Colors.Red);
        }

        /// <summary>
        /// 获取内容文本框
        /// </summary>      
        private void GetFocusTextBox(object sender)
        {
            FocusTextBox = sender as TextBox;
        }

        /// <summary>
        /// 获取内容Label
        /// </summary>
        /// <param name="sender"></param>
        private void GetFocusLabel(object sender)
        {
            FocusLabel = sender as Label;
        }

        /// <summary>
        /// 获取密码文本框
        /// </summary>       
        private void GetFocusPasswordBox(object sender)
        {
            FocusPasswordBox = sender as PasswordBox;
        }

        /// <summary>
        /// 设置是否打开图形界面
        /// </summary>      
        private void SetIsOpenLayerModeNoLogin(bool isOpen)
        {
            IsOpenLayerModeNoLogin = isOpen;
        }

        /// <summary>
        /// 设置是否打开图形界面
        /// </summary>       
        private void SetIsOpenLayerModeLogin(bool isOpen)
        {
            IsOpenLayerModeLogin = isOpen;
        }

        /// <summary>
        /// 是否显示图形界面控制面板
        /// </summary>      
        private void SetIsShowAllIconPanelLogin(bool isVisible)
        {
            IsShowIconSearchCodePanelLogin = isVisible;
            //this.stpIconSearCodeInfoLogin.Visibility = IsVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

        }

        /// <summary>
        /// 通信设置串口基础数据
        /// </summary>
        private void SetComSerialPortData()
        {

            cbxSetComFireAlarmPort.ItemsSource = null;
            cbxSetComFireAlarmBaudRate.Items.Clear();
            cbxSetComDisBoxPort.ItemsSource = null;
            cbxSetComHostBoardPort.ItemsSource = null;

            List<string> LstSerialPortNames = new List<string>();
            ObservableCollection<string> ComSerialPortNames = new ObservableCollection<string>
            {
                "--请选择--"
            };
            LstSerialPortNames.Add("--请选择--");
            string[] SerialPortNames = SerialPort.GetPortNames();
            if (SerialPortNames.Length > 0)
            {
                for (int i = 0; i < SerialPortNames.Length; i++)
                {
                    ComSerialPortNames.Add(SerialPortNames[i]);
                    LstSerialPortNames.Add(SerialPortNames[i]);
                }
            }

            cbxSetComFireAlarmPort.ItemsSource = ComSerialPortNames;
            cbxSetComDisBoxPort.ItemsSource = ComSerialPortNames;
            cbxSetComHostBoardPort.ItemsSource = ComSerialPortNames;

            EnumClass.BaudRateClass EnumBaudRate = new EnumClass.BaudRateClass();
            Array baudRateArray = Enum.GetValues(EnumBaudRate.GetType());
            foreach (int fireAlarmbaudRate in baudRateArray)
            {
                cbxSetComFireAlarmBaudRate.Items.Add(fireAlarmbaudRate.ToString());
            }


            if (LstSerialPortNames.Contains(LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue) || LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue == "")
            {
                if (LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue == "")
                {
                    cbxSetComFireAlarmPort.SelectedIndex = 0;
                }
                else
                {
                    cbxSetComFireAlarmPort.SelectedIndex = cbxSetComFireAlarmPort.Items.IndexOf(LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue);
                }
                //DJZMSL.SerialPortIsOpen = true;
                ReplaceFireSerialPort = true;
            }
            else
            {
                cbxSetComFireAlarmPort.SelectedIndex = 0;
                //DJZMSL.SerialPortIsOpen = false;
                if (ReplaceFireSerialPort)
                {
                    ReplaceFireSerialPort = false;
                }
            }

            if (LstSerialPortNames.Contains(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue) || LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue == "")
            {
                if (LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue == "")
                {
                    cbxSetComDisBoxPort.SelectedIndex = 0;
                }
                else
                {
                    cbxSetComDisBoxPort.SelectedIndex = cbxSetComDisBoxPort.Items.IndexOf(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);
                }
                //Protocol.SeialPortIsOpen = true;
                ReplaceHostSerialPort = true;
            }
            else
            {
                cbxSetComDisBoxPort.SelectedIndex = 0;
                //Protocol.SeialPortIsOpen = false;
                if (ReplaceHostSerialPort)
                {
                    ReplaceHostSerialPort = false;
                }
            }

            if (LstSerialPortNames.Contains(LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue) || LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue == "")
            {
                if (LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue == "")
                {
                    cbxSetComHostBoardPort.SelectedIndex = 0;
                }
                else
                {
                    cbxSetComHostBoardPort.SelectedIndex = cbxSetComHostBoardPort.Items.IndexOf(LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue);
                }
                //AbsFireAlarmLink.serialPortIsOpen = true;
                ReplaceDisSerialPort = true;
            }
            else
            {
                cbxSetComHostBoardPort.SelectedIndex = 0;
                //AbsFireAlarmLink.serialPortIsOpen = false;
                if (ReplaceDisSerialPort)
                {
                    ReplaceDisSerialPort = false;
                }
            }

            cbxSetComFireAlarmBaudRate.SelectedIndex = cbxSetComFireAlarmBaudRate.Items.IndexOf(LstGblSetting.Find(x => x.Key == "FireAlarmBaudRate").SetValue);

            if (!ReplaceFireSerialPort || !ReplaceHostSerialPort || !ReplaceDisSerialPort)
            {
                List<string> LstShow = new List<string>();
                if (!ReplaceFireSerialPort)
                {
                    ReplaceFireSerialPort = !ReplaceFireSerialPort;
                    LstShow.Add("火警通讯串口");
                }
                else
                {
                    LstShow.Remove("火警通讯串口");
                }

                if (!ReplaceHostSerialPort)
                {
                    ReplaceHostSerialPort = !ReplaceHostSerialPort;
                    LstShow.Add("主机通讯串口");
                }
                else
                {
                    LstShow.Remove("主机通讯串口");
                }

                if (!ReplaceDisSerialPort)
                {
                    ReplaceDisSerialPort = !ReplaceDisSerialPort;
                    LstShow.Add("配电箱通讯串口");
                }
                else
                {
                    LstShow.Remove("配电箱通讯串口");
                }

                NewShowText = string.Join("、", LstShow.ToArray());
                NewShowText += "已更新，请重新设置！";

                if (OldShowText == null)
                {
                    OldShowText = NewShowText;
                    //CommonFunct.PopupWindow(OldShowText);
                    //MessageBox.Show(OldShowText);
                    MessageBox.Show(OldShowText, "提示", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);

                }
                else
                {
                    if (OldShowText != NewShowText)
                    {
                        OldShowText = NewShowText;
                        //CommonFunct.PopupWindow(OldShowText);
                        MessageBox.Show(OldShowText, "提示", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                    }
                }
            }
        }

        /// <summary>
        /// 验证通信设置的所有串口
        /// </summary>
        private async Task<bool> CheckSetComAllSerialPort(string SerialPortType)
        {
            try
            {
                if ((cbxSetComFireAlarmPort.SelectedIndex == cbxSetComDisBoxPort.SelectedIndex && cbxSetComFireAlarmPort.SelectedIndex != 0) || (cbxSetComDisBoxPort.SelectedIndex
                == cbxSetComHostBoardPort.SelectedIndex && cbxSetComDisBoxPort.SelectedIndex != 0) || (cbxSetComFireAlarmPort.SelectedIndex == cbxSetComHostBoardPort.SelectedIndex && cbxSetComFireAlarmPort.SelectedIndex != 0))
                {
                    await Dispatcher.BeginInvoke(new Action(() => { MessageBox.Show("串口设置不能重复！", "提示！"); }));
                    return false;
                }

                if (SerialPortType == "FireAlarmPort")
                {
                    if (cbxSetComFireAlarmPort.SelectedIndex == 0)
                    {
                        LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue = null;
                    }
                    else
                    {
                        LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue = cbxSetComFireAlarmPort.SelectedValue.ToString();
                    }
                    LstGblSetting.Find(x => x.Key == "FireAlarmBaudRate").SetValue = cbxSetComFireAlarmBaudRate.SelectedValue.ToString();
                    ObjGblSetting.Save(LstGblSetting);
                    await Dispatcher.BeginInvoke(new Action(() => { CommonFunct.PopupWindow("串口通信设置成功！"); }));
                    return true;
                }

                if (SerialPortType == "DisBoxPort")
                {
                    if (AbsFireAlarmLink.IsHostSerialPort(cbxSetComDisBoxPort.SelectedValue.ToString()))
                    {
                        AbsFireAlarmLink.ReleaseHostBoardSerialPort();
                        LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue = null;
                    }
                    if ((_fireAlarm != null && _fireAlarm?.FireAlarmPortName == cbxSetComDisBoxPort.SelectedValue.ToString()) || FireAlarmLinkInterface.IsFireAlarmSerialPort(cbxSetComDisBoxPort.SelectedValue.ToString()))
                    {
                        _fireAlarm?.Disable();
                        FireAlarmLinkInterface.CloseFireAlarmSerialPort();
                        LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue = null;
                    }
                    if(cbxSetComDisBoxPort.SelectedIndex == 0)
                    {
                        Protocol.InitDisBoxSerialPort(string.Empty);
                    }
                    else
                    {
                        Protocol.InitDisBoxSerialPort(cbxSetComDisBoxPort.SelectedValue.ToString());
                    }
                    
                    if (Protocol.CheckEpsSeialPort() == SerialPortType)
                    {
                        if (cbxSetComDisBoxPort.SelectedIndex == 0)
                        {
                            LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue = null;
                        }
                        else
                        {
                            LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue = cbxSetComDisBoxPort.SelectedValue.ToString();
                        }
                        ObjGblSetting.Save(LstGblSetting);
                        await Dispatcher.BeginInvoke(new Action(() => { CommonFunct.PopupWindow("串口通信设置成功！"); }));
                        return true;
                    }
                    else
                    {
                        Protocol.InitDisBoxSerialPort(LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue);
                    }
                }

                if (SerialPortType == "HostBoardPort")
                {
                    if (Protocol.IsEPSSerialPort(cbxSetComHostBoardPort.SelectedValue.ToString()))
                    {
                        Protocol.ReleaseEPSSerialPort();
                        LstGblSetting.Find(x => x.Key == "DisBoxPort").SetValue = null;
                    }
                    if ((_fireAlarm != null && _fireAlarm?.FireAlarmPortName == cbxSetComHostBoardPort.SelectedValue.ToString()) || FireAlarmLinkInterface.IsFireAlarmSerialPort(cbxSetComHostBoardPort.SelectedValue.ToString()))
                    {
                        _fireAlarm?.Disable();
                        FireAlarmLinkInterface.CloseFireAlarmSerialPort();
                        LstGblSetting.Find(x => x.Key == "FireAlarmPort").SetValue = null;
                    }
                    if (cbxSetComHostBoardPort.SelectedIndex == 0)
                    {
                        AbsFireAlarmLink.OpenHostBoardSerialPort(string.Empty);
                    }
                    else
                    {
                        AbsFireAlarmLink.OpenHostBoardSerialPort(cbxSetComHostBoardPort.SelectedValue.ToString());
                    }
                    
                    //AbsFireAlarmLink.CheckHostSerialPort();
                    if (AbsFireAlarmLink.SerialPortType == SerialPortType)
                    {
                        if (cbxSetComHostBoardPort.SelectedIndex == 0)
                        {
                            LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue = null;
                        }
                        else
                        {
                            LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue = cbxSetComHostBoardPort.SelectedValue.ToString();
                        }
                        ObjGblSetting.Save(LstGblSetting);
                        await Dispatcher.BeginInvoke(new Action(() => { CommonFunct.PopupWindow("串口通信设置成功！"); }));
                        return true;
                    }
                    else
                    {
                        AbsFireAlarmLink.OpenHostBoardSerialPort(LstGblSetting.Find(x => x.Key == "HostBoardPort").SetValue);
                    }
                }
                await Dispatcher.BeginInvoke(new Action(() => { CommonFunct.PopupWindow("串口通信设置失败！"); }));
                return false;
            }
            catch
            {
                //MessageBox.Show(ex.ToString());
                return false;
            }

        }

        /// <summary>
        /// 单灯控制
        /// </summary>
        private void SingleLightControl(EnumClass.SingleLightControlClass SingleLightControlClass, string strLightCode, string strEPSCode)
        {
            Task task = Task.Run(async () =>
            {
                IsTimingQueryEPSOrLight = false;
                await Protocol.SetSignalLight((byte)SingleLightControlClass, strLightCode, strEPSCode);
                System.Windows.Forms.Application.DoEvents();
                IsTimingQueryEPSOrLight = true;
            });
        }

        /// <summary>
        /// 显示点击图标对应的信息
        /// </summary>
        private void ShowIconSearCodeInfoNoLogin(object infoDisBoxOrLight)
        {
            string strShowIconSearCodeInfo = string.Empty;
            bool isDistributionBoxInfo = infoDisBoxOrLight is DistributionBoxInfo;
            if (isDistributionBoxInfo)
            {
                DistributionBoxInfo infoDistributionBox = infoDisBoxOrLight as DistributionBoxInfo;
                strShowIconSearCodeInfo = string.Format("编码:{0}\r\n位置:{1}\r\n当前状态:{2}\r\n灯具数量:{3}", infoDistributionBox.Code, infoDistributionBox.Address, infoDistributionBox.Shield == 0 ? CommonFunct.GetEPSStatus(infoDistributionBox.Status) : "正常", LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(infoDistributionBox.Code)).Count);
            }
            else
            {
                DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString());
                if (((infoDistributionBox.Status & 0X07FC) & 0X07FC) == 0X07FC && infoDistributionBox.Shield == 1)
                {
                    strShowIconSearCodeInfo = string.Format("编码:{0}\r\n类型:{1}\r\n位置:{2}\r\n初始状态:{3}\r\n当前状态:{4}\r\n电池状态:{5}\r\n左预案:{6},{7},{8}\r\n右预案:{9},{10},{11}\r\n所在EPS:{12}",
                    infoDisBoxOrLight.GetType().GetProperty("Code").GetValue(infoDisBoxOrLight),
                    CommonFunct.GetLightClass(infoDisBoxOrLight as LightInfo),
                    infoDisBoxOrLight.GetType().GetProperty("Address").GetValue(infoDisBoxOrLight),
                    CommonFunct.GetLightStatus(infoDisBoxOrLight as LightInfo, LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString()), true),
                    CommonFunct.GetLightStatus(infoDisBoxOrLight as LightInfo, LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString()), false),
                    CommonFunct.GetLightBatteryStatus(LstDistributionBox.Find(x => x.Code == (infoDisBoxOrLight as LightInfo).DisBoxID.ToString()), infoDisBoxOrLight as LightInfo),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft1").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft2").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft3").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight1").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight2").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight3").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight));
                }
                else
                {
                    strShowIconSearCodeInfo = string.Format("编码:{0}\r\n类型:{1}\r\n位置:{2}\r\n初始状态:{3}\r\n当前状态:{4}\r\n电池状态:{5}\r\n左预案:{6},{7},{8}\r\n右预案:{9},{10},{11}\r\n所在EPS:{12}",
                    infoDisBoxOrLight.GetType().GetProperty("Code").GetValue(infoDisBoxOrLight),
                    CommonFunct.GetLightClass(infoDisBoxOrLight as LightInfo),
                    infoDisBoxOrLight.GetType().GetProperty("Address").GetValue(infoDisBoxOrLight),
                    CommonFunct.GetLightStatus(infoDisBoxOrLight as LightInfo, LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString()), true),
                    CommonFunct.GetLightStatus(infoDisBoxOrLight as LightInfo, LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString()), false),
                    CommonFunct.GetLightBatteryStatus(LstDistributionBox.Find(x => x.Code == (infoDisBoxOrLight as LightInfo).DisBoxID.ToString()), infoDisBoxOrLight as LightInfo),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft1").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft2").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanLeft3").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight1").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight2").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("PlanRight3").GetValue(infoDisBoxOrLight),
                    infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight));
                }
            }
            labIconSearCodeInfoNoLogin.Content = strShowIconSearCodeInfo;
        }

        /// <summary>
        /// 设置密码框
        /// </summary>
        private void SetPassword(Label Label)
        {
            if (FocusPasswordBox != null)
            {
                FocusPasswordBox.Password = string.Format("{0}{1}", FocusPasswordBox.Password, Label.Content);
            }
        }
        private void SetPassword(Button button)
        {
            if (FocusPasswordBox != null && FocusPasswordBox.Password.Count() < 6)
            {
                FocusPasswordBox.Password = $"{FocusPasswordBox.Password}{button.Content}";
            }
        }

        private void SetFocusTime1(Label Label)
        {
            if (FocusTime != null)
            {
                int selectionStart = FocusTime.SelectionStart;
                int selectionLength = FocusTime.SelectionLength;//选中该文本框的字数
                string strKeyText = FocusTime.Text.Trim();
                if (strKeyText.Length - selectionLength >= FocusTime.MaxLength)
                {
                    if (selectionLength == 0)
                    {
                        strKeyText = strKeyText.Insert(selectionStart, Label.Content.ToString());
                        selectionStart++;
                    }
                    else
                    {
                        strKeyText = strKeyText.Remove(selectionStart, selectionLength);
                        strKeyText = strKeyText.Insert(selectionStart, Label.Content.ToString());
                        selectionStart++;
                    }
                    FocusTime.Text = strKeyText;
                    FocusTime.SelectionStart = selectionStart;
                }
            }
        }

        /// <summary>
        /// 展示历史记录页数
        /// </summary>
        private void ShowHistoryPage()
        {
            //LstHistoricalEvent = ObjHistoricalEvent.GetAll();
            //LstHistoricalShow.Reverse();
            PreviousPageForHistory.Content = HistoryListCurrentPage = 1;
            if (LstHistoricalShow.Count % 12 != 0)
            {
                TotalPageForHistory.Content = LstHistoricalShow.Count / 12 + 1;
            }
            else
            {
                TotalPageForHistory.Content = LstHistoricalShow.Count / 12;
            }

        }

        /// <summary>
        /// 展示历史记录列表
        /// </summary>
        private void ShowHistoryList()
        {
            StaHistoryList.Children.Clear();
            for (int i = (HistoryListCurrentPage - 1) * 12; i < HistoryListCurrentPage * 12; i++)
            {
                if (i >= LstHistoricalShow.Count)
                {
                    break;
                }
                Label Label = GetHistoryLabel(LstHistoricalShow[i], i % 12, string.Format("" + LstHistoricalShow[i].ID + "." + LstHistoricalShow[i].EventTime + " " + LstHistoricalShow[i].EventContent));
                StaHistoryList.Children.Add(Label);
            }
        }

        private Label GetHistoryLabel(HistoricalEventInfo infoHistoricalEvent, int index, string LabContent)
        {
            Label Label = new Label
            {
                Tag = infoHistoricalEvent,
                TabIndex = index,
                Height = 37,
                Content = LabContent,
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };
            return Label;
        }

        /// <summary>
        /// 获取选中配电箱下的灯具
        /// </summary>
        private void GetLightViewByDisBoxIDNoLogin()
        {
            if (LstDistributionBox.Count != 0)
            {
                EPSViewCurrentIndexNoLogin = 0;
                SelectInfoEPSNoLogin = LstDistributionBox[EPSViewCurrentIndexNoLogin];
                LstLightViewByDisBoxIDNoLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(SelectInfoEPSNoLogin.Code));
            }
        }

        /// <summary>
        /// 获取选中配电箱下的灯具
        /// </summary>
        private void GetLightViewByDisBoxIDLogin()
        {
            if (LstDistributionBox.Count != 0)
            {
                EPSViewCurrentIndexLogin = 0;
                SelectInfoEPSLogin = LstDistributionBox[EPSViewCurrentIndexLogin];
                LstLightViewByDisBoxIDLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(SelectInfoEPSLogin.Code.ToString()));
            }
        }

        /// <summary>
        /// 显示EPS列表
        /// </summary>
        private void ShowEPSListLogin()
        {
            if (EPSData.Visibility == System.Windows.Visibility.Visible)
            {
                stpEPSListShow.Children.Clear();
                for (int i = (EPSListCurrentPageLogin - 1) * (EPSListColumnCount * EPSListMaxRowCountLogin); i < EPSListCurrentPageLogin * (EPSListColumnCount * EPSListMaxRowCountLogin); i++)
                {
                    if (i >= LstDistributionBox.Count)
                    {
                        break;
                    }
                    Label Label = GetEPSLabelLogin(LstDistributionBox[i], i);
                    stpEPSListShow.Children.Add(Label);
                }
            }
            else if (EPSCollect.Visibility == System.Windows.Visibility.Visible)
            {
                EPSCodeShow.Children.Clear();
                for (int i = (EPSListCurrentPageLogin - 1) * (7 * 4); i < EPSListCurrentPageLogin * (7 * 4); i++)
                {
                    if (i >= LstDistributionBox.Count)
                    {
                        break;
                    }
                    Label Label = GetEPSLabelNoLogin(LstDistributionBox[i], i);
                    EPSCodeShow.Children.Add(Label);
                }
            }
            else if (MasterController.Visibility == System.Windows.Visibility.Visible)
            {
                InitEPSShowNoLogin();
            }
        }

        private void ShowEPSList()
        {
            EPSLampControl.Children.Clear();
            for (int i = (EPSListCurrentPageLogin - 1) * (4 * 3); i < EPSListCurrentPageLogin * (4 * 3); i++)
            {
                if (i >= LstDistributionBox.Count)
                {
                    break;
                }
                Label Label = GetEPSLabel(LstDistributionBox[i], i);
                EPSLampControl.Children.Add(Label);
            }
        }

        /// <summary>
        /// 显示灯具列表
        /// </summary>
        private void ShowLightListLogin()
        {
            LampForEPS.Children.Clear();
            for (int i = (LightListCurrentPageLogin - 1) * 4 * 4; i < LightListCurrentPageLogin * 4 * 4; i++)
            {
                if (i >= LstLightViewByDisBoxIDLogin.Count)
                {
                    break;
                }
                Label Label = GetLightLabelLogin(LstLightViewByDisBoxIDLogin[i]);
                LampForEPS.Children.Add(Label);
            }
        }

        /// <summary>
        /// 显示灯具列表
        /// </summary>
        private void ShowLightListNoLogin()
        {
            stpLightList.Children.Clear();
            for (int i = (LightListCurrentPageNoLogin - 1) * LightListMaxRowCountNoLogin * LightListColumnCountNoLogin; i < LightListCurrentPageNoLogin * LightListMaxRowCountNoLogin * LightListColumnCountNoLogin; i++)
            {
                if (i >= LstLightViewByDisBoxIDNoLogin.Count)
                {
                    break;
                }
                Label Label = GetLightLabelNoLogin(LstLightViewByDisBoxIDNoLogin[i], i % (LightListMaxRowCountNoLogin * LightListColumnCountNoLogin));
                stpLightList.Children.Add(Label);
            }
        }

        /// <summary>
        /// 显示EPS信息
        /// </summary>
        private void ShowEPSInfoNoLogin(DistributionBoxInfo infoDistributionBox)
        {
            SelectInfoEPSNoLogin = infoDistributionBox;
            EPSCollectCode.Content = infoDistributionBox.Code;
            EPSCollectPosition.Content = infoDistributionBox.Address;
            EPSCollectFaultType.Content = infoDistributionBox.Shield == 1 ? "正常" : GetEPSStatus(infoDistributionBox.Status);
            //this.EPSCollectState.Content = infoDistributionBox.Status != (int)EnumClass.DisBoxStatusClass.正常状态 ? "故障" : "正常";
            EPSCollectMainVoltage.Content = infoDistributionBox.MainEleVoltage;//主电电压
            EPSCollectBatteryVoltage.Content = infoDistributionBox.BatteryVoltage;//电池电压
            EPSCollectDischargeCurrent.Content = infoDistributionBox.DischargeCurrent;//放电电流
            EPSCollectOutputVoltage.Content = infoDistributionBox.DischargeVoltage;//输出电压

            int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0XF000;//0X07FC
            if ((ConversionValue & 0XF000) != 0)
            {
                if (infoDistributionBox.MainEleVoltage == 0)
                {
                    if (infoDistributionBox.Status == 0)
                    {
                        EPSCollectState.Content = "跟随";
                    }
                    else
                    {
                        if ((Convert.ToInt32(infoDistributionBox.Status) & 0XF000) == 0)
                        {
                            EPSCollectState.Content = "正常";//配电箱模式
                        }
                        else
                        {
                            if (infoDistributionBox.Test == 1)
                            {
                                if (infoDistributionBox.QiangQi == 1)
                                {
                                    EPSCollectState.Content = "测试/强启";
                                }
                                else
                                {
                                    EPSCollectState.Content = "测试";
                                }
                            }
                            else
                            {
                                if (infoDistributionBox.AutoManual == 1)
                                {
                                    if (infoDistributionBox.QiangQi == 1)
                                    {
                                        EPSCollectState.Content = "手动/强启";
                                    }
                                    else
                                    {
                                        EPSCollectState.Content = "手动";
                                    }
                                }
                                else
                                {
                                    if (infoDistributionBox.QiangQi == 1)
                                    {
                                        EPSCollectState.Content = "自动/强启";
                                    }
                                    else
                                    {
                                        EPSCollectState.Content = "自动";
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if ((Convert.ToInt32(infoDistributionBox.Status) & 0XF000) == 0)
                    {
                        EPSCollectState.Content = "正常";//配电箱模式
                    }
                    else
                    {
                        if (infoDistributionBox.Test == 1)
                        {
                            if (infoDistributionBox.QiangQi == 1)
                            {
                                EPSCollectState.Content = "测试/强启";
                            }
                            else
                            {
                                EPSCollectState.Content = "测试";
                            }
                        }
                        else
                        {
                            if (infoDistributionBox.AutoManual == 1)
                            {
                                if (infoDistributionBox.QiangQi == 1)
                                {
                                    EPSCollectState.Content = "手动/强启";
                                }
                                else
                                {
                                    EPSCollectState.Content = "手动";
                                }
                            }
                            else
                            {
                                if (infoDistributionBox.QiangQi == 1)
                                {
                                    EPSCollectState.Content = "自动/强启";
                                }
                                else
                                {
                                    EPSCollectState.Content = "自动";
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                if ((ConversionValue & 0X07FC) == 0X07FC)
                {
                    if (infoDistributionBox.Shield == 1)
                    {
                        EPSCollectState.Content = "正常";
                    }
                    else
                    {
                        EPSCollectState.Content = "故障";
                    }
                }
                else
                {
                    if ((ConversionValue & 0X07FC) == 0X100)//主控板掉线故障
                    {
                        EPSCollectState.Content = "- -";
                    }
                    else
                    {
                        EPSCollectState.Content = "正常";
                    }
                }
            }
        }

        private void ShowEPSLamp(DistributionBoxInfo infoDistributionBox)
        {
            SelectInfoEPSLogin = infoDistributionBox;
            LstLightViewByDisBoxIDLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(SelectInfoEPSLogin.Code.ToString()));
            LightListCurrentPageLogin = 1;
            LightListTotalPageLogin = LstLightViewByDisBoxIDLogin.Count != 0 ? (LstLightViewByDisBoxIDLogin.Count - 1) / (4 * 4) + 1 : 1;

            LampCurrentPage.Content = LightListCurrentPageLogin;
            LampTotalPages.Content = LightListTotalPageLogin;

            ShowLightListLogin();
        }

        /// <summary>
        /// 显示EPS信息
        /// </summary>    
        private void ShowEPSInfoLogin(DistributionBoxInfo infoDistributionBox)
        {
            SelectInfoEPSLogin = infoDistributionBox;
            LstLightViewByDisBoxIDLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBox.Code));

            EPSCode.Content = infoDistributionBox.Code;
            //this.labLightCountByDisBoxLogin.Content = LstLightViewByDisBoxIDLogin.Count;
            EPSInitialPositionText.Text = infoDistributionBox.Address == string.Empty ? "安装地址未初始化" : infoDistributionBox.Address;
            CurrentState.Content = infoDistributionBox.Shield == 0 ? (GetEPSStatus(infoDistributionBox.Status) == "正常" ? "正常" : "故障") : "正常";
            //this.MainVoltage.Content = infoDistributionBox.MainEleVoltage;//主电电流

            //输出电压

            Floodlight.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count;
            TwoWaySignLamp.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count;
            DoubleHeadLamp.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count;
            BidirectionalBuriedLamp.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count;
            EXIT.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count;
            FloorIndication.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count;
            OneWaySignLamp.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count;
            OneWayBuriedLamp.Content = LstLightViewByDisBoxIDLogin.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count;
            LampTotal.Content = LstLightViewByDisBoxIDLogin.Count;
            //this.FaultLight.Content = LstLightViewByDisBoxIDLogin.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0).Count;

            if (((infoDistributionBox.Status & 0X07FC) & 0X07FC) == 0X07FC && infoDistributionBox.Shield == 1)
            {
                FaultLight.Content = 0;
            }
            else
            {
                FaultLight.Content = LstLightViewByDisBoxIDLogin.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0).Count - LstLightViewByDisBoxIDLogin.FindAll(x => ((x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0) && x.Shield == 1).Count;
            }

            MainVoltage.Content = infoDistributionBox.MainEleVoltage;
            OutputVoltage.Content = infoDistributionBox.DischargeVoltage;
            //this.labDischargeCurrentLogin.Content = infoDistributionBox.DischargeCurrent;//放电电流
            EPSReservePlan1.Text = infoDistributionBox.Plan1.ToString();
            EPSReservePlan2.Text = infoDistributionBox.Plan2.ToString();
            EPSReservePlan3.Text = infoDistributionBox.Plan3.ToString();
            EPSReservePlan4.Text = infoDistributionBox.Plan4.ToString();
            EPSReservePlan5.Text = infoDistributionBox.Plan5.ToString();
        }

        /// <summary>
        /// 显示灯具信息
        /// </summary>      
        private void ShowLightInfoLogin(LightInfo infoLight)
        {
            SelectInfoLightLogin = infoLight;
            LampEPS.Content = SelectInfoEPSLogin.Code;
            LampCode.Content = infoLight.Code;
            LampClass.Content = CommonFunct.GetLightClass(infoLight);
            LampState.Content = CommonFunct.GetLightStatus(infoLight, LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString()), false);
            LampsPositionText.Text = infoLight.Address == string.Empty ? "安装位置未初始化" : infoLight.Address;
            LeftBrightPlan1.Text = infoLight.PlanLeft1.ToString();
            LeftBrightPlan2.Text = infoLight.PlanLeft2.ToString();
            LeftBrightPlan3.Text = infoLight.PlanLeft3.ToString();
            LeftBrightPlan4.Text = infoLight.PlanLeft4.ToString();
            //this.tbxLightPlanLeft5.Text = infoLight.PlanLeft5.ToString();
            RightBrightPlan1.Text = infoLight.PlanRight1.ToString();
            RightBrightPlan2.Text = infoLight.PlanRight2.ToString();
            RightBrightPlan3.Text = infoLight.PlanRight3.ToString();
            RightBrightPlan4.Text = infoLight.PlanRight4.ToString();
            //this.tbxLightPlanRight5.Text = infoLight.PlanRight5.ToString();
        }

        /// <summary>
        /// 显示灯具信息
        /// </summary>
        private void ShowLightInfoNoLogin(LightInfo infoLight)
        {
            string strShowLightInfo = string.Format("灯具编码:{0}\r\n灯具类型:{1}\r\n灯具位置:{2}\r\n初始状态:{3}\r\n当前状态:{4}\r\n电池状态:{5}\r\n涉及左预案:{6},{7},{8},{9}\r\n涉及右预案:{10},{11},{12},{13}\r\n所在分区:{14}\r\n所在EPS:{15}\r\n ", infoLight.Code, CommonFunct.GetLightClass(infoLight), infoLight.Address, CommonFunct.GetLightStatus(infoLight, LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString()), true), CommonFunct.GetLightStatus(infoLight, LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString()), false), CommonFunct.GetLightBatteryStatus(LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString()), infoLight), infoLight.PlanLeft1, infoLight.PlanLeft2, infoLight.PlanLeft3, infoLight.PlanLeft4, infoLight.PlanRight1, infoLight.PlanRight2, infoLight.PlanRight3, infoLight.PlanRight4, "0", infoLight.DisBoxID);
            labDetailedInformation.Content = strShowLightInfo;
        }

        /// <summary>
        /// EPS列表切换上下页
        /// </summary>       
        private bool SwitchEPSListPageLogin(bool isLast)
        {
            if (isLast)
            {
                if (EPSListCurrentPageLogin == 1)
                {
                    EPSListCurrentPageLogin = EPSListTotalPageLogin;
                }
                else
                {
                    EPSListCurrentPageLogin--;
                }
            }
            else
            {
                if (EPSListCurrentPageLogin == EPSListTotalPageLogin)
                {
                    EPSListCurrentPageLogin = 1;
                }
                else
                {
                    EPSListCurrentPageLogin++;
                }
            }
            labEPSCodeListCurrentPageLogin.Content = EPSListCurrentPageLogin;
            return true;
        }

        /// <summary>
        /// 灯具列表切换上下页
        /// </summary>       
        private bool SwitchLightListPageLogin(bool isLast)
        {
            if (isLast)
            {
                if (LightListCurrentPageLogin == 1)
                {
                    LightListCurrentPageLogin = LightListTotalPageLogin;
                }
                else
                {
                    LightListCurrentPageLogin--;
                }
            }
            else
            {
                if (LightListCurrentPageLogin == LightListTotalPageLogin)
                {
                    LightListCurrentPageLogin = 1;
                }
                else
                {
                    LightListCurrentPageLogin++;
                }
            }
            return true;
        }

        /// <summary>
        /// 灯具列表切换上下页
        /// </summary>       
        private bool SwitchLightListPageNoLogin(bool isLast)
        {
            if (isLast)
            {
                if (LightListCurrentPageNoLogin == 1)
                {
                    LightListCurrentPageNoLogin = LightListTotalPageNoLogin;
                }
                else
                {
                    LightListCurrentPageNoLogin--;
                }
            }
            else
            {
                if (LightListCurrentPageNoLogin == LightListTotalPageNoLogin)
                {
                    LightListCurrentPageNoLogin = 1;
                }
                else
                {
                    LightListCurrentPageNoLogin++;
                }
            }
            LampCurrentPageByLampCollect.Content = LightListCurrentPageNoLogin;
            return true;
        }
        /// <summary>
        /// 根据灯具类型改变灯具状态控制，初始状态以及预案设置显示
        /// </summary>
        private void ChangeLightControlShow()
        {
            LampBright.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Unchecked.png", UriKind.Relative));
            LampExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Unchecked.png", UriKind.Relative));
            LampShine.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Unchecked.png", UriKind.Relative));
            LampMainEle.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Unchecked.png", UriKind.Relative));
            LampLeftOpen.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Unchecked.png", UriKind.Relative));
            LampRightOpen.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Unchecked.png", UriKind.Relative));

            switch (SelectInfoLightLogin.LightClass)
            {
                case (int)EnumClass.LightClass.照明灯:
                case (int)EnumClass.LightClass.双头灯:
                    LampShine.Visibility = LampMainEle.Visibility = LampLeftOpen.Visibility = LampRightOpen.Visibility = InitialStateLeftBright.Visibility = InitialStateRightBright.Visibility = System.Windows.Visibility.Hidden;

                    LeftBrightPlan1.Visibility = LeftBrightPlan2.Visibility
                     = LeftBrightPlan3.Visibility = LeftBrightPlan4.Visibility
                     = RightBrightPlan1.Visibility = RightBrightPlan2.Visibility
                     = RightBrightPlan3.Visibility = RightBrightPlan4.Visibility = System.Windows.Visibility.Hidden;
                    break;

                case (int)EnumClass.LightClass.双向标志灯:
                case (int)EnumClass.LightClass.双向地埋灯:
                    LampLeftOpen.Visibility = LampRightOpen.Visibility
                    = LampShine.Visibility = LampMainEle.Visibility
                    = InitialStateLeftBright.Visibility = InitialStateRightBright.Visibility
                    = System.Windows.Visibility.Visible;

                    LeftBrightPlan1.Visibility = LeftBrightPlan2.Visibility
                     = LeftBrightPlan3.Visibility = LeftBrightPlan4.Visibility
                     = RightBrightPlan1.Visibility = RightBrightPlan2.Visibility
                     = RightBrightPlan3.Visibility = RightBrightPlan4.Visibility = System.Windows.Visibility.Visible;
                    break;

                case (int)EnumClass.LightClass.安全出口灯:
                case (int)EnumClass.LightClass.楼层灯:
                case (int)EnumClass.LightClass.单向标志灯:
                case (int)EnumClass.LightClass.单向地埋灯:
                    LampShine.Visibility = LampMainEle.Visibility = System.Windows.Visibility.Visible;

                    LampLeftOpen.Visibility = LampRightOpen.Visibility = InitialStateLeftBright.Visibility = InitialStateRightBright.Visibility = System.Windows.Visibility.Hidden;

                    LeftBrightPlan1.Visibility = LeftBrightPlan2.Visibility = LeftBrightPlan3.Visibility = LeftBrightPlan4.Visibility = System.Windows.Visibility.Visible;

                    RightBrightPlan1.Visibility = RightBrightPlan2.Visibility = RightBrightPlan3.Visibility = RightBrightPlan4.Visibility = System.Windows.Visibility.Hidden;

                    break;
            }
        }

        /// <summary>
        /// 根据灯具类型确定图片
        /// </summary>
        /// <param name="infoLight"></param>
        /// <param name="image"></param>
        private void ImageShowByLightClass(LightInfo infoLight, Image image)
        {
            int status, isEmergency, beginStatus,shield;
            status = Convert.ToInt32(infoLight.GetType().GetProperty("Status").GetValue(infoLight));
            isEmergency = Convert.ToInt32(infoLight.GetType().GetProperty("IsEmergency").GetValue(infoLight));
            beginStatus = Convert.ToInt32(infoLight.GetType().GetProperty("BeginStatus").GetValue(infoLight));
            LightClass = Convert.ToInt32(infoLight.GetType().GetProperty("LightClass").GetValue(infoLight));
            shield = Convert.ToInt32(infoLight.GetType().GetProperty("Shield").GetValue(infoLight));

            if ((status & (int)EnumClass.LightFaultClass.光源故障) != 0 || (status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (status & (int)EnumClass.LightFaultClass.电池故障) != 0)
            {
                if ((status & (int)EnumClass.LightFaultClass.通信故障) != 0)
                {
                    DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString());
                    int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                    if ((ConversionValue & 0X07FC) == 0X07FC)//EPS掉线故障
                    {
                        if (infoDistributionBox.Shield == 0)
                        {
                            if (infoLight.Shield == 0)
                            {
                                switch (infoLight.LightClass)
                                {
                                    case (int)EnumClass.LightClass.照明灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双头灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双向标志灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双向地埋灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.安全出口灯:
                                        image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.楼层灯:
                                        image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.单向标志灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Fault.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.单向地埋灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Fault.png", UriKind.Relative));
                                        break;
                                }
                            }
                            else
                            {
                                switch (infoLight.LightClass)
                                {
                                    case (int)EnumClass.LightClass.照明灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双头灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双向标志灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.双向地埋灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.安全出口灯:
                                        image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.楼层灯:
                                        image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.单向标志灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                                        break;
                                    case (int)EnumClass.LightClass.单向地埋灯:
                                        image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                                        break;
                                }
                            }
                        }
                        else
                        {
                            switch (infoLight.LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if(infoLight.Shield == 0)
                        {
                            switch (infoLight.LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Fault.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Fault.png", UriKind.Relative));
                                    break;
                            }
                        }
                        else
                        {
                            switch (infoLight.LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                                    break;
                            }
                        }
                        
                    }

                }
                else
                {
                    if (infoLight.Shield == 0)
                    {
                        switch (infoLight.LightClass)
                        {
                            case (int)EnumClass.LightClass.照明灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双头灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.安全出口灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.楼层灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Fault.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Fault.png", UriKind.Relative));
                                break;
                        }
                    }
                    else
                    {
                        switch (infoLight.LightClass)
                        {
                            case (int)EnumClass.LightClass.照明灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双头灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.安全出口灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.楼层灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                                break;
                        }
                    }
                }
            }
            else
            {
                if (isEmergency == 1)
                {
                    switch (infoLight.LightClass)
                    {
                        case (int)EnumClass.LightClass.照明灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.双头灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Emergency.png", UriKind.Relative));
                            break;

                        case (int)EnumClass.LightClass.双向标志灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.双向地埋灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.安全出口灯:
                            image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.楼层灯:
                            image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.单向标志灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Emergency.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.单向地埋灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Emergency.png", UriKind.Relative));
                            break;
                    }
                }
                else if(shield == 1)
                {
                    switch (infoLight.LightClass)
                    {
                        case (int)EnumClass.LightClass.照明灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.双头灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.双向标志灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.双向地埋灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.安全出口灯:
                            image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.楼层灯:
                            image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.单向标志灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                            break;
                        case (int)EnumClass.LightClass.单向地埋灯:
                            image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                            break;
                    }
                }
                else
                {
                    if (beginStatus == 0)
                    {
                        switch (infoLight.LightClass)
                        {
                            case (int)EnumClass.LightClass.照明灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双头灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Extinguish.png", UriKind.Relative));
                                break;

                            case (int)EnumClass.LightClass.双向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.安全出口灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.楼层灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Extinguish.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Extinguish.png", UriKind.Relative));
                                break;
                        }
                    }
                    else
                    {
                        switch (infoLight.LightClass)
                        {
                            case (int)EnumClass.LightClass.照明灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/Floodlight-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双头灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/DoubleHeadLamp-Bright.png", UriKind.Relative));
                                break;

                            case (int)EnumClass.LightClass.双向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWaySignLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.双向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/TwoWayBuriedLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.安全出口灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.楼层灯:
                                image.Source = new BitmapImage(new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向标志灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWaySignLamp-Bright.png", UriKind.Relative));
                                break;
                            case (int)EnumClass.LightClass.单向地埋灯:
                                image.Source = new BitmapImage(new Uri("/Pictures/OneWayBuriedLamp-Bright.png", UriKind.Relative));
                                break;
                        }
                    }

                }
            }

        }

        /// <summary>
        /// 改变EPS码文本框的背景色
        /// </summary>
        private void ChangeEPSCodeBackgroundNoLogin(Label SelectedLabel)
        {
            foreach (Label Label in EPSCodeShow.Children)
            {
                if (Label.Content == SelectedLabel.Content)
                {
                    Label.Background = CommonFunct.GetBrush(ClickedCodeLabelBackground);
                }
                else
                {
                    Label.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        private void ChangeEPSCodeBackground(Label SelectedLabel)
        {
            foreach (Label Label in EPSLampControl.Children)
            {
                if (Label.Content == SelectedLabel.Content)
                {
                    Label.Background = CommonFunct.GetBrush(ClickedCodeLabelBackground);
                }
                else
                {
                    Label.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }
        /// <summary>
        /// 改变配电箱码文本框的背景色
        /// </summary>    
        private void ChangeDisBoxCodeBackgroundLogin(Label SelectedLabel)
        {
            foreach (Label Label in stpEPSListShow.Children)
            {
                if (Label.Content == SelectedLabel.Content)
                {
                    Label.Background = CommonFunct.GetBrush(ClickedCodeLabelBackground);
                }
                else
                {
                    Label.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        /// <summary>
        /// 改变灯码文本框的背景色
        /// </summary>
        private void ChangeLightCodeBackgroundNoLogin(Label Label, bool isLast)
        {
            if (isLast)
            {
                Label.Background = CommonFunct.GetBrush(LabelBackground);
            }
            else
            {
                Label.Background = CommonFunct.GetBrush(ClickedCodeLabelBackground);
            }
            stpInformationDisplay.Visibility = isLast ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 重置灯码文本框背景色
        /// </summary>
        private void ResetLightCodeBackgroundLogin(Label Label)
        {
            foreach (Label UIElement in LampForEPS.Children)
            {
                Label LabelChild = UIElement;
                if (LabelChild.Content == Label.Content)
                {
                    LabelChild.Background = CommonFunct.GetBrush(ClickedCodeLabelBackground);
                }
                else
                {
                    LabelChild.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        /// <summary>
        /// 切换EPS展示页面
        /// </summary>
        /// <param name="isLast"></param>
        /// <returns></returns>
        private bool SwitchEPSImageDisplay(bool isLast)
        {
            if (isLast)
            {
                if (EPSImageDisplayCurrentPage == 1)
                {
                    EPSImageDisplayCurrentPage = EPSImageDisplayTotalPage;
                }
                else
                {
                    EPSImageDisplayCurrentPage--;
                }
            }
            else
            {
                if (EPSImageDisplayCurrentPage == EPSImageDisplayTotalPage)
                {
                    EPSImageDisplayCurrentPage = 1;
                }
                else
                {
                    EPSImageDisplayCurrentPage++;
                }
            }
            EPSShowCurrentPage.Content = EPSImageDisplayCurrentPage;
            return true;
        }

        /// <summary>
        /// 单灯替换
        /// </summary>
        private bool ReplaceSingleLight()
        {
            string strEPSCode = LampReplaceEPSCode.Content.ToString();
            string strOldLightCode = OldLampCode.Content.ToString();
            string strNewLightCode = NewLampCode.Content.ToString();
            if (strEPSCode == string.Empty || strOldLightCode == string.Empty || strNewLightCode == string.Empty)
            {
                MessageBox.Show("EPS码或者原灯码或者现灯码不能为空！", "提示");
                return false;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strEPSCode);
            if (infoDistributionBox == null)
            {
                MessageBox.Show(string.Format("EPS{0}不存在！", strEPSCode), "提示");
                return false;
            }

            LightInfo infoOldLightCode = LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code).Find(x => x.Code == strOldLightCode);
            if (infoOldLightCode == null)
            {
                MessageBox.Show(string.Format("原灯码{0}不存在！", strOldLightCode), "提示");
                return false;
            }

            LightInfo infoNewLightCode = LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code).Find(x => x.Code == strNewLightCode);
            if (infoNewLightCode != null)
            {
                MessageBox.Show(string.Format("现灯码{0}已存在！", strNewLightCode), "提示");
                return false;
            }

            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            bool isReplaceSingleOldLight = Protocol.ReplaceSingleOldLight(strOldLightCode, infoDistributionBox.Code);
            if (!isReplaceSingleOldLight)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show(string.Format("灯码{0}替换失败！", strNewLightCode), "提示");
                return false;
            }

            bool isReplaceSingleNewLight = Protocol.ReplaceSingleNewLight(strNewLightCode, infoDistributionBox.Code);
            if (!isReplaceSingleNewLight)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show(string.Format("灯码{0}替换失败！", strNewLightCode), "提示");
                return false;
            }

            infoOldLightCode.Code = strNewLightCode;
            infoOldLightCode.Address = string.Empty;
            infoOldLightCode.LightClass = Convert.ToInt32(strNewLightCode.Substring(0, 1));
            infoOldLightCode.BeginStatus = SelectInfoLightLogin.Status = GetLightInitStatus(infoOldLightCode.LightClass);
            infoOldLightCode.PlanLeft1 = infoOldLightCode.PlanLeft2 = infoOldLightCode.PlanLeft3
            = infoOldLightCode.PlanLeft4 = infoOldLightCode.PlanLeft5 = infoOldLightCode.PlanRight1
            = infoOldLightCode.PlanRight2 = infoOldLightCode.PlanRight3 = infoOldLightCode.PlanRight4
            = infoOldLightCode.PlanRight5 = 0;
            ObjLight.Update(infoOldLightCode);
            LstLight.Sort();

            IsTimingQueryEPSOrLight = true;
            MessageBox.Show(string.Format("灯码{0}替换成功！", strNewLightCode), "提示");
            return true;
        }

        /// <summary>
        /// 单灯添加
        /// </summary>
        private bool AddSingleLight()
        {
            string strEPSCode = LampAddEPSCode.Content.ToString();
            string strLightCode = LampAddCode.Content.ToString();
            if (strEPSCode == string.Empty || strLightCode == string.Empty)
            {
                MessageBox.Show("EPS码或者灯码不能为空！", "提示");
                return false;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strEPSCode);
            if (infoDistributionBox == null)
            {
                MessageBox.Show(string.Format("EPS{0}不存在！", strEPSCode), "提示");
                return false;
            }

            LightInfo infoLight = LstLight.Find(x => x.Code == strLightCode);
            if (infoLight != null)
            {
                MessageBox.Show(string.Format("灯码{0}已存在！", strLightCode), "提示");
                return false;
            }

            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            bool isSuccess = Protocol.QuerySingleLight(strLightCode, infoDistributionBox.Code);//查询该EPS下是否有此灯具
            if (!isSuccess)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show(string.Format("灯具{0}添加失败！", strLightCode), "提示");
                return false;
            }

            Protocol.AddSingleLight(strLightCode, infoDistributionBox.Code);//单灯添加

            infoLight = new LightInfo
            {
                Code = strLightCode,
                Address = string.Empty,
                ErrorTime = string.Empty,
                PlanLeft1 = 0,
                PlanLeft2 = 0,
                PlanLeft3 = 0,
                PlanLeft4 = 0,
                PlanLeft5 = 0,
                PlanRight1 = 0,
                PlanRight2 = 0,
                PlanRight3 = 0,
                PlanRight4 = 0,
                PlanRight5 = 0,
                LightClass = Convert.ToInt32(strLightCode.Substring(0, 1)),
                DisBoxID = int.Parse(infoDistributionBox.Code),
                LightIndex = LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code).Count,
                Shield = 0
            };
            infoLight.BeginStatus = infoLight.Status = GetLightInitStatus(infoLight.LightClass);

            if (infoLight.LightClass == 5 && int.Parse(infoLight.Code.Substring(1, 1)) >= 5)
            {
                infoLight.LightClass = 6;
            }

            ObjLight.Add(infoLight);
            LstLight.Add(infoLight);
            LstLight.Sort();

            IsTimingQueryEPSOrLight = true;
            MessageBox.Show(string.Format("灯具{0}添加成功！", strLightCode), "提示");
            return true;
        }

        /// <summary>
        /// 单灯删除
        /// </summary>
        private bool DeleteSingleLight()
        {
            string strEPSCode = LampAddEPSCode.Content.ToString();
            string strLightCode = LampAddCode.Content.ToString();
            if (strEPSCode == string.Empty || strLightCode == string.Empty)
            {
                MessageBox.Show("EPS码或者灯码不能为空！", "提示");
                return false;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strEPSCode);
            if (infoDistributionBox == null)
            {
                MessageBox.Show(string.Format("EPS{0}不存在！", strEPSCode), "提示");
                return false;
            }

            LightInfo infoLight = LstLight.Find(x => x.Code == strLightCode);
            if (infoLight == null)
            {
                MessageBox.Show(string.Format("灯码{0}不存在！", strLightCode), "提示");
                return false;
            }

            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            Protocol.DeleteSingleLight(strLightCode, infoDistributionBox.Code);
            //isSuccess = Protocol.DeleteSingleLight(strLightCode, infoDistributionBox.Code);
            //if (!isSuccess)
            //{
            //    IsTimingQueryEPSOrLight = true;
            //    MessageBox.Show(string.Format("灯具{0}删除失败！", strLightCode), "提示");
            //    return false;
            //}

            LstLightQueryByEPSID = LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code);
            for (int i = infoLight.LightIndex + 1; i < LstLightQueryByEPSID.Count; i++)
            {
                LightInfo infoLightQueryByEPSID = LstLightQueryByEPSID[i];
                infoLightQueryByEPSID.LightIndex = infoLightQueryByEPSID.LightIndex - 1;
            }

            ObjLight.Save(LstLightQueryByEPSID);
            ObjLight.Delete(infoLight.ID);
            LstLight.Remove(infoLight);
            LstLight.Sort();

            IsTimingQueryEPSOrLight = true;
            MessageBox.Show(string.Format("灯具{0}删除成功！", strLightCode), "提示");
            return true;
        }

        /// <summary>
        /// 添加EPS
        /// </summary>
        /// <returns></returns>
        private bool AddSingleEPS()
        {
            string strDisCode = AddEPSCode.Content.ToString();
            if (string.IsNullOrEmpty(strDisCode))
            {
                MessageBox.Show("EPS码不能为空！", "提示");
                return false;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strDisCode);
            if (infoDistributionBox != null)
            {
                MessageBox.Show("此EPS已存在,无需添加！", "提示");
                return false;
            }

            IsTimingQueryEPSOrLight = false;
            IsQueryEPSAndLight = false;
            IsQueryLight = false;

            bool isSuccess = Protocol.FindEPS(strDisCode);
            if (!isSuccess)
            {
                MessageBox.Show("未查询到此EPS，请检查该设备是否安装！", "提示");
                return false;
            }

            AddEPS(strDisCode);
            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show(string.Format("配电箱{0}添加成功,是否现在开始搜灯？", strDisCode), "提示", MessageBoxButton.YesNo);

            if (MessageBoxResult == MessageBoxResult.Yes)
            {
                FindLight(strDisCode);
                MessageBox.Show("搜灯完成！", "提示");
            }

            IsTimingQueryEPSOrLight = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
            return true;
        }

        /// <summary>
        /// 单个EPS搜灯
        /// </summary>
        /// <param name="strDisCode">EPS码</param>
        private void FindLight(string strDisCode)
        {
            IsTimingQueryEPSOrLight = false;
            IsQueryEPSAndLight = false;
            IsQueryLight = false;
            InitialTips("搜灯中！请勿操作！", true);
            SetEnableMainWindow(false);
            Protocol.ADDEPSLamp(strDisCode);//指定EPS快速搜灯

            LightCodeByEPS = Protocol.QueryLightByEPSFastly(strDisCode);

            if (LightCodeByEPS != null)
            {
                for (int j = 0; j < LightCodeByEPS.Length; j++)
                {
                    string strLightCode = LightCodeByEPS[j];
                    LightInfo infoLight = LstLight.Find(x => x.Code == strLightCode && x.DisBoxID == int.Parse(strDisCode));
                    if (Convert.ToInt32(strLightCode) != 0 && strLightCode.Length == LightCodeLength && infoLight == null)
                    {
                        ObjLight.Add(AddLight(strLightCode, int.Parse(strDisCode), j));
                    }
                }
            }
            Protocol.AllMainEle();
            SetEnableMainWindow(true);
            InitialTips(null, false);
            IsTimingQueryEPSOrLight = true;
            IsQueryEPSAndLight = true;
            IsQueryLight = true;
        }

        private bool DeleteSingleEPS()
        {
            string strDisCode = AddEPSCode.Content.ToString();
            if (string.IsNullOrEmpty(strDisCode))
            {
                MessageBox.Show("EPS码不能为空！", "提示");
                return false;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strDisCode);
            if (infoDistributionBox == null)
            {
                MessageBox.Show("未找到此配电箱！", "提示");
                return false;
            }

            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("是否要删除该配电箱？", "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.Yes)
            {
                ObjDistributionBox.Delete(infoDistributionBox.ID);
                List<LightInfo> LstLamp = LstLight.FindAll(x => x.DisBoxID == int.Parse(strDisCode));
                foreach (LightInfo infoLight in LstLamp)
                {
                    ObjLight.Delete(infoLight.ID);
                }

                List<FaultRecordInfo> LstFault = LstFaultRecord.FindAll(x => x.Subject == strDisCode);
                foreach (FaultRecordInfo infoFault in LstFault)
                {
                    ObjFaultRecord.Delete(infoFault.ID);
                }

                List<HistoricalEventInfo> LstHistorical = LstHistoricalEvent.FindAll(x => x.EventContent.Contains(strDisCode));
                foreach (HistoricalEventInfo infoHistoricalEvent in LstHistoricalEvent)
                {
                    ObjHistoricalEvent.Delete(infoHistoricalEvent.ID);
                }

                LstDistributionBox = ObjDistributionBox.GetAll();
                LstLight = ObjLight.GetAll();
                LstFaultRecord = ObjFaultRecord.GetAll();
                LstHistoricalEvent = ObjHistoricalEvent.GetAll();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 替换EPS
        /// </summary>
        private void ReplaceEPS()
        {
            string strOldEPSCode = OldEPSCode.Content.ToString();
            string strNewEPSCode = NewEPSCode.Content.ToString();
            if (strOldEPSCode == string.Empty || strNewEPSCode == string.Empty)
            {
                MessageBox.Show("原EPS码或者现EPS码不能为空！", "提示");
                return;
            }

            DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strNewEPSCode);
            if (infoDistributionBox != null)
            {
                MessageBox.Show(string.Format("现EPS码{0}已存在！", strNewEPSCode), "提示");
                return;
            }

            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            bool isSuccess = Protocol.FindEPS(strNewEPSCode);
            if (!isSuccess)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show(string.Format("现EPS码{0}通信失败！", strNewEPSCode), "提示");
                return;
            }

            Protocol.TransAllLightCodeByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == strOldEPSCode)
            , strNewEPSCode);
            Protocol.TransLightLeftPlanByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == strOldEPSCode)
            , strNewEPSCode);
            Protocol.TransLightRightPlanByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == strOldEPSCode)
            , strNewEPSCode);
            Protocol.TransLightStateByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == strOldEPSCode)
             , strNewEPSCode);
            Protocol.TransEPSPlan(LstDistributionBox.Find(x => x.Code == strOldEPSCode), strNewEPSCode);

            isSuccess = Protocol.FindEPS(strNewEPSCode);
            if (!isSuccess)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show(string.Format("现EPS码{0}通信失败！", strNewEPSCode), "提示");
                return;
            }

            LightCodeByEPS = Protocol.QueryLightByEPSFastly(strNewEPSCode);
            if (LightCodeByEPS.Length != LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(strOldEPSCode)).Count)
            {
                IsTimingQueryEPSOrLight = true;
                MessageBox.Show("EPS替换失败！", "提示");
                return;
            }

            infoDistributionBox = LstDistributionBox.Find(x => x.Code == strOldEPSCode);
            LstLight.FindAll(x => x.DisBoxID == Convert.ToInt32(infoDistributionBox.Code))
                .ForEach(x => x.DisBoxID = Convert.ToInt32(strNewEPSCode));
            infoDistributionBox.Code = strNewEPSCode;
            ObjDistributionBox.Update(infoDistributionBox);
            ObjLight.Save(LstLight);

            LstDistributionBox.Sort();

            IsTimingQueryEPSOrLight = true;
            MessageBox.Show(string.Format("配电箱{0}修改成功！", strNewEPSCode), "提示");
        }

        /// <summary>
        /// 取消替换EPS
        /// </summary>
        private void ReplaceEPSCancel()
        {
            OldEPSCode.Content = string.Empty;
            NewEPSCode.Content = string.Empty;
        }

        /// <summary>
        /// 打开复位登录系统页面
        /// </summary>
        private bool OpenLoginResetSystemPage()
        {
            Login1 Login = new Login1(LstGblSetting, (int)EnumClass.VerifyClass.复位验证);
            Login.ShowDialog();
            return Login.IsSuccessCheck;
        }

        /// <summary>
        /// 打开登录应急界面
        /// </summary>
        private bool OpenLoginEmergencyPage()
        {
            Login1 Login = new Login1(LstGblSetting, (int)EnumClass.VerifyClass.权限验证);
            Login.ShowDialog();
            return Login.IsSuccessCheck;
        }

        /// <summary>
        /// 系统复位
        /// </summary>
        private void ResetSystemNoLogin()
        {
            SetIsEnterResetSystem(true);
            Protocol.AllMainEle();
            AbsFireAlarmLink.SendHostBoardData(0X01);
            if (IsRealFireAlarmLink)
            {
                ClearFireAlarmLinkZoneNumberStat();
                //SetRealFireAlarmLinkTimer(false);
                SetIsRealFireAlarmLink(false);
                SetAllLightStatus(false);
                SetAllEPSStatus(true);
                SetAllLightStatus(true);
                RecordRealFireAlarmLinkCalcuTotalTime();
                HideRealFireAlarmLinkInfoPanel();
            }
            else
            {
                RecordAllMainEleInfo();
            }
            Protocol.AllMainEle();
            AbsFireAlarmLink.SendHostBoardData(0X01);
            SetAllEPSStatus(false);
            SetAllLightStatus(false);
            SetAllMainEleTimer(true);
            //等待EPS复位
            while (CurrentExeInstructTime < ResetWaitTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(90);
                CurrentExeInstructTime += 90;
            }
            CurrentExeInstructTime = 0;
            if (IsOpenLayerModeNoLogin)
            {
                InitLayerModeNoLogin();
            }
            if (stpLayerModeNoLogin.Visibility == System.Windows.Visibility.Visible)
            {
                ShowTotalFloorNoLogin();
                SetIsOpenLayerModeNoLogin(true);
                SwitchFloorNoLogin();
            }
            SetIsEnterResetSystem(false);
            //LstOldZoneNumber.Clear();
            //if (_fireAlarmType == "GST5000H")
            //{
            //    //_fireAlarmSerialMonitor.OnEnable();
            //    _fireAlarm.OnMonitor(1000);
            //}
        }

        private void ResetSystemButton()
        {
            App.Restartup();
        }

        /// <summary>
        /// 退出系统
        /// </summary>
        private void ExitSystem()
        {
            string strFillPassWord = CommonFunct.Md5Encrypt(FocusTextBox.Text);
            string strAdminPassWord = LstGblSetting.Find(x => x.Key == "AdminPassWord").SetValue;
            if (strFillPassWord != strAdminPassWord)
            {
                CommonFunct.PopupWindow("退出系统密码填写错误，请重新填写！");
                return;
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// 一般全体应急系统复位
        /// </summary>
        private void CommonResetSystem()
        {
            SetAllMainEleTimer(false);
            SetIsAllMainEle(true);
            SetIsEnterResetSystem(false);
            AllMainEle();
        }

        private void OldCommonResetSystem()
        {
            IsEmergency = false;
            IsComEmergency = false;
            AllEmergencyTotalTimer.Enabled = false;

            LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            LstLight.ForEach(x => x.IsEmergency = 0);
            ObjDistributionBox.Save(LstDistributionBox);
            ObjLight.Save(LstLight);
            RefreshIconSearchCodeLogin();

            ResettingProgressBar = pgbResetSystem;
            AllMainEle();
            GetEdition(IsCommodity);
            
            StopRefreshProgressBarValueTimer();
            RefreshProgressBarValue();

            AllEmergencyTotalTimer.Enabled = false;

            CurrentProgressBarValue = 0.0;
            RefreshProgressBarValueTimer.Enabled = true;

        }



        /// <summary>
        /// 系统应急
        /// </summary>
        private void AllEmergencySystem()
        {
            SetIsAllMainEle(false);
            AllEmergency();
            //OldEmergency();
        }
        /// <summary>
        /// 移动灯具显示信息面板
        /// </summary>      
        private void MoveShowLightInfoPanel(int tabIndex)
        {
            if (IsLogin)
            {
                double columnDistance;
                double rowDistance;

                if (tabIndex % LightListColumnCountNoLogin > LightListMiddleColumnIndex)
                {
                    columnDistance = (tabIndex % LightListColumnCountNoLogin - LightListMiddleColumnIndex) * 130 + LightLabelLeftOffsetNoLogin;
                }
                else
                {
                    columnDistance = tabIndex % LightListColumnCountNoLogin * 130;
                }

                if (tabIndex / LightListColumnCountNoLogin > LightListMiddleRowIndex)
                {
                    rowDistance = (tabIndex / LightListColumnCountNoLogin - LightListMiddleRowIndex)
                        * (LabelHeight + MarginTop) + LightLabelTopOffsetNoLogin;
                }
                else
                {
                    rowDistance = tabIndex / LightListColumnCountNoLogin * (LabelHeight + MarginTop);
                }


                stpInformationDisplay.SetValue(Canvas.LeftProperty, LightInfoPanelMargin.X + columnDistance);
                stpInformationDisplay.SetValue(Canvas.TopProperty, LightInfoPanelMargin.Y + rowDistance);
                stpInformationDisplay.Height = stpInformationDisplay.Width = labDetailedInformation.Height = labDetailedInformation.Width = 150;
                labDetailedInformation.FontSize = 10;
            }
            else
            {
                double columnDistance;
                double rowDistance;

                double leftlistDistance = 210;//整体列表到界面左侧距离
                double toplistDistance = 20;//整体列表到界面上侧距离
                double leftImageDistance = 17;//图片左侧到StackPanel左侧距离
                double rightImageDistance = 87;//图片右侧到StackPanel右侧距离
                double topImageDistance = 15;//图片上端到StackPanel上端距离
                double bottomImageDistance = 60; //图片底部到StackPanel底部距离
                double ImageWidth = 90;//图片宽度
                double ImageHeight = 60;//图片高度

                if (tabIndex % 4 < 3)
                {
                    columnDistance = leftlistDistance + (leftImageDistance + ImageWidth) * (tabIndex % 4 + 1) + rightImageDistance * (tabIndex % 4);
                }
                else
                {
                    columnDistance = leftlistDistance + (leftImageDistance + ImageWidth + rightImageDistance) * 3 + leftImageDistance - stpInformationDisplay.Width;
                }

                if (((tabIndex / 4) + 1) < 4)
                {
                    rowDistance = toplistDistance + (topImageDistance + ImageHeight) * ((tabIndex / 4) + 1) + bottomImageDistance * (tabIndex / 4);
                }
                else
                {
                    rowDistance = toplistDistance + (topImageDistance + ImageHeight) * ((tabIndex / 4) + 1) + bottomImageDistance * (tabIndex / 4) - stpInformationDisplay.Height;
                }

                stpInformationDisplay.SetValue(Canvas.LeftProperty, columnDistance);
                stpInformationDisplay.SetValue(Canvas.TopProperty, rowDistance);
                stpInformationDisplay.Height = stpInformationDisplay.Width = labDetailedInformation.Height = labDetailedInformation.Width = 150;
                labDetailedInformation.FontSize = 10;
            }
        }

        /// <summary>
        /// 显示主控器信息
        /// </summary>
        private void ShowControllerPanel()
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Visible;
            stpInformationDisplay.SetValue(Canvas.LeftProperty, LightInfoPanelMargin.X + 30);
            stpInformationDisplay.SetValue(Canvas.TopProperty, LightInfoPanelMargin.Y - 100);

            string strShowLightInfo = string.Format("当前状态:{0}\r\nEPS总数:{1}\r\n灯具总数:{2}\r\n照明灯总数:{3}\r\n双向标志灯总数:{4}\r\n双头灯总数:{5}\r\n双向地埋灯总数:{6}\r\n安全出口灯总数:{7}\r\n单向标志灯总数:{8}\r\n单向地埋灯总数:{9}\r\n楼层灯总数:{10}\r\n", MasterControllerStatus, LstDistributionBox.Count, LstLight.Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count, LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count);
            labDetailedInformation.Content = strShowLightInfo;

            stpInformationDisplay.Height = labDetailedInformation.Height = 250;
            stpInformationDisplay.Width = labDetailedInformation.Width = 200;
            labDetailedInformation.FontSize = 14;
        }


        /// <summary>
        /// 返回一个EPS文本控件(需要根据EPS索引来决定位置)
        /// </summary>
        private Label GetEPSLabelNoLogin(DistributionBoxInfo infoDistributionBox, int index)
        {
            Label Label = new Label
            {
                Tag = infoDistributionBox,
                Height = 55,
                Width = 140,
                Content = infoDistributionBox.Code,
                FontSize = CodeFontSize,
                Foreground = GetEPSOrLampTypeFaceColor(infoDistributionBox, null),
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,

            };

            if (index % 7 == 0)
            {
                Label.Margin = new Thickness(0, 0, 0, 0);
                //Label.Margin = new Thickness(5, MarginTop-5, 0, 0);
            }
            else
            {
                Label.Margin = new Thickness((index % 7) * 138, -(Label.Height), 0, 0);
            }
            Label.MouseDown += EPSCodeNoLogin_MouseDown;
            return Label;
        }

        private Label GetEPSLabel(DistributionBoxInfo infoDistributionBox, int index)
        {
            Label Label = new Label
            {
                Tag = infoDistributionBox,
                Height = 60,
                Width = 100,
                Content = infoDistributionBox.Code,
                FontSize = CodeFontSize,
                Foreground = CommonFunct.GetBrush(CodeLabelForeground),
                Background = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            if (index % 4 == 0)
            {
                Label.Margin = new Thickness(15, 12 - 5, 0, 0);
            }
            else
            {
                Label.Margin = new Thickness((index % 4) * 110 + 15, -(Label.Height), 0, 0);
            }
            Label.MouseDown += EPSCode_MouseDown;
            return Label;
        }

        /// <summary>
        /// 返回一个EPS文本控件(需要根据配电箱索引来决定位置)
        /// </summary>       
        private Label GetEPSLabelLogin(DistributionBoxInfo infoDistributionBox, int index)
        {
            Label Label = new Label
            {
                Tag = infoDistributionBox,
                Height = LabelHeight,
                Width = LabelWidth,
                Content = infoDistributionBox.Code,
                FontSize = CodeFontSize,
                Foreground = CommonFunct.GetBrush(CodeLabelForeground),
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            if (index % EPSListColumnCount == 0)
            {
                Label.Margin = new Thickness(0, 3, 0, 0);
            }
            else if (index % EPSListColumnCount == 1)
            {
                Label.Margin = new Thickness(153, -(Label.Height), 0, 0);
            }
            else
            {
                Label.Margin = new Thickness(305, -(Label.Height), 0, 0);
            }
            Label.MouseDown += EPSCodeLogin_MouseDown;
            return Label;
        }

        /// <summary>
        /// 返回灯具文本控件(需要根据灯具索引来决定位置)
        /// </summary>        
        private Label GetLightLabelLogin(LightInfo infoLight)
        {
            Label Label = new Label
            {
                Tag = infoLight,
                Height = 45,
                Width = 100,
                Content = infoLight.Code,
                FontSize = CodeFontSize,
                Foreground = CommonFunct.GetBrush(CodeLabelForeground),
                Background = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            int index = LstLightViewByDisBoxIDLogin.FindIndex(x => x == infoLight);

            if (index % 4 == 0)
            {
                Label.Margin = new Thickness(15, MarginTop, 0, 0);
            }
            else
            {
                Label.Margin = new Thickness((index % 4) * 110 + 15, -(Label.Height), 0, 0);
            }
            Label.MouseDown += LightCodeLogin_MouseDown;
            return Label;
        }

        /// <summary>
        /// 返回灯具文本控件(需要根据灯具索引来决定位置)
        /// </summary>
        private Label GetLightLabelNoLogin(LightInfo infoLight, int index)
        {
            Label Label = new Label
            {
                Tag = infoLight,
                TabIndex = index,
                Height = 48,
                Width = 130,
                Content = infoLight.Code,
                FontSize = CodeFontSize,
                Foreground = GetEPSOrLampTypeFaceColor(null, infoLight),
                Background = CommonFunct.GetBrush(LabelBackground),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            if (index % LightListColumnCountNoLogin == 0)
            {
                Label.Margin = new Thickness(5, MarginTop, 0, 0);
            }
            else
            {
                Label.Margin = new Thickness((index % LightListColumnCountNoLogin) * 140 + 5, -(Label.Height), 0, 0);
            }

            Label.MouseDown += LightCodeNoLogin_MouseDown;
            Label.MouseUp += LightCodeNoLogin_MouseUp;
            Label.TouchDown += LightCodeNoLogin_TouchDown;
            Label.TouchUp += LightCodeNoLogin_TouchUp;
            return Label;
        }

        private StackPanel GetEPSStackPanel(DistributionBoxInfo infoDistributionBox, int index)
        {
            StackPanel stackpanel = new StackPanel
            {
                Tag = infoDistributionBox,
                Height = 123,
                Width = 190,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Image image = new Image
            {
                Height = 70,
                Width = 65,
                Margin = new Thickness(-80, 15, 0, 0),
                Source = InspectGetEPSStateIamge(infoDistributionBox),
                Tag = infoDistributionBox.Code
            };
            image.MouseDown += EPSImage_MouseDown;
            stackpanel.Children.Add(image);

            Label label1 = new Label
            {
                Content = infoDistributionBox.Code,
                Height = 27,
                Width = 55,
                Margin = new Thickness(50, -110, 0, 0),
                Foreground = CommonFunct.GetBrush("#8CB3D9"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label1);

            Label label2 = new Label
            {
                Content = "状态:",
                Height = 25,
                Width = 40,
                Margin = new Thickness(35, -70, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label2);

            Label label3 = new Label
            {
                Tag = "状态",
                Content = InspectGainEPSStatus(infoDistributionBox),
                Height = 25,
                Width = 60,
                Margin = new Thickness(130, -70, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label3);

            Label label4 = new Label
            {
                Content = "灯具数:",
                Height = 25,
                Width = 50,
                Margin = new Thickness(50, -30, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label4);

            Label label5 = new Label
            {
                Content = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBox.Code)).Count,
                Height = 25,
                Width = 40,
                Margin = new Thickness(130, -30, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label5);

            Label label6 = new Label
            {
                Content = infoDistributionBox.Address == string.Empty ? "安装位置未初始化" : infoDistributionBox.Address,
                Height = 25,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label6);

            if (index % EPSImageDisplayColumnCount == 0)
            {
                stackpanel.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                //int x = index / EPSImageDisplayColumnCount;
                stackpanel.Margin = new Thickness((index % EPSImageDisplayColumnCount) * (stackpanel.Width + 10), -stackpanel.Height, 0, 0);
            }
            return stackpanel;

        }

        private StackPanel GetLampStackPanel(LightInfo infoLight, int index)
        {
            LampNoLoginTag lampNoLoginTag = new LampNoLoginTag
            {
                infoLight = infoLight,
                tabIndex = index
            };

            StackPanel stackpanel = new StackPanel
            {
                Tag = infoLight,

                Height = 135,
                Width = 190,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Image image = new Image
            {
                Tag = lampNoLoginTag,
                Height = 60,
                Width = 90,
                Margin = new Thickness(-70, 15, 0, 0)
            };
            ImageShowByLightClass(infoLight, image);
            image.MouseLeftButtonDown += LampNoLogin_MouseDown;
            image.MouseLeftButtonUp += LampNoLogin_MouseUp;
            image.TouchDown += LampNoLogin_TouchDown;
            image.TouchUp += LampNoLogin_TouchUp;
            stackpanel.Children.Add(image);

            Label label1 = new Label
            {
                Content = infoLight.Code,
                Height = 35,
                Width = 70,
                Margin = new Thickness(100, -60, 0, 0),
                Foreground = CommonFunct.GetBrush("#8CB3D9"),
                FontSize = 16,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label1);

            Label label2 = new Label
            {
                Content = infoLight.Address == string.Empty ? "安装位置未初始化" : infoLight.Address,
                Height = 35,
                Margin = new Thickness(0, 14, 0, 0),
                Foreground = CommonFunct.GetBrush("#FFFFFF"),
                FontSize = 16,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            stackpanel.Children.Add(label2);

            if (index % LampImageDisplayColumnCount == 0)
            {
                stackpanel.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                stackpanel.Margin = new Thickness((index % LampImageDisplayColumnCount) * (stackpanel.Width + 10), -(stackpanel.Height), 0, 0);
            }
            return stackpanel;
        }

        /// <summary>
        /// 未登录查看灯具状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LampNoLogin_MouseDown(object sender, MouseEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Visible;
            MoveShowLightInfoPanel(((LampNoLoginTag)((sender as Image).Tag)).tabIndex);
            ShowLightInfoNoLogin(((LampNoLoginTag)((sender as Image).Tag)).infoLight);
        }

        private void LampNoLogin_MouseUp(object sender, MouseEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Hidden;
        }

        private void LampNoLogin_TouchDown(object sender, TouchEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Visible;
            MoveShowLightInfoPanel(((LampNoLoginTag)((sender as Image).Tag)).tabIndex);
            ShowLightInfoNoLogin(((LampNoLoginTag)((sender as Image).Tag)).infoLight);
        }

        private void LampNoLogin_TouchUp(object sender, TouchEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 根据EPS和灯具的状态改变字体颜色
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <param name="infoLight"></param>
        /// <returns></returns>
        private Brush GetEPSOrLampTypeFaceColor(DistributionBoxInfo infoDistributionBox, LightInfo infoLight)
        {
            if (infoDistributionBox != null)
            {
                if (infoDistributionBox.IsEmergency == 1)
                {
                    return CommonFunct.GetBrush(CodeLabelEmergencyForeground);
                }
                int ConversionValue = infoDistributionBox.Status & 0X07FC;
                if ((ConversionValue & 0X07FC) != (int)EnumClass.DisBoxStatusClass.正常状态)
                {
                    if (infoDistributionBox.Shield == 1)
                    {
                        return CommonFunct.GetBrush(CodeLabelForeground);
                    }
                    else
                    {
                        return CommonFunct.GetBrush(CodeLabelFaultForeground);
                    }
                }
                else
                {
                    return CommonFunct.GetBrush(CodeLabelForeground);
                }
            }
            if (infoLight != null)
            {
                if (infoLight.IsEmergency == 1)
                {
                    return CommonFunct.GetBrush(CodeLabelEmergencyForeground);
                }
                if (infoLight.Status == (int)EnumClass.LightFaultClass.光源故障 || infoLight.Status == (int)EnumClass.LightFaultClass.通信故障)
                {
                    DistributionBoxInfo infoDis = LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString());
                    if (infoLight.Status == (int)EnumClass.LightFaultClass.通信故障 && ((infoDis.Status & 0X07FC) & 0X07FC) == 0X07FC && infoDis.Shield == 1)
                    {
                        return CommonFunct.GetBrush(CodeLabelForeground);
                    }
                    else
                    {
                        if (infoLight.Shield == 1)
                        {
                            return CommonFunct.GetBrush(CodeLabelForeground);
                        }
                        else
                        {
                            return CommonFunct.GetBrush(CodeLabelFaultForeground);
                        }
                    }
                }
                else
                {
                    return CommonFunct.GetBrush(CodeLabelForeground);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据EPS状态改变EPS图标
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <returns></returns>
        private ImageSource GetEPSStateIamge(DistributionBoxInfo infoDistributionBox)
        {
            if (infoDistributionBox.IsEmergency == 1)
            {
                return new BitmapImage(new Uri("/Pictures/EPS-Emergency.png", UriKind.Relative));
            }
            else
            {
                int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                if ((ConversionValue & 0X07FC) != 0)
                {
                    if (infoDistributionBox.Shield == 0)
                    {
                        return new BitmapImage(new Uri("/Pictures/EPS-Fault.png", UriKind.Relative));
                    }
                    return new BitmapImage(new Uri("/Pictures/EPS-Normal.png", UriKind.Relative));
                }
                else
                {
                    return new BitmapImage(new Uri("/Pictures/EPS-Normal.png", UriKind.Relative));
                }
            }
        }

        private ImageSource InspectGetEPSStateIamge(DistributionBoxInfo infoDistributionBox)
        {
            
            if (infoDistributionBox.IsEmergency == 1)
            {
                return new BitmapImage(new Uri("/Pictures/EPS-Emergency.png", UriKind.Relative));
            }
            else
            {
                
                int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                if ((ConversionValue & 0X07FC) != 0 )
                {
                    if (infoDistributionBox.Shield == 0)
                    {
                        return new BitmapImage(new Uri("/Pictures/EPS-Fault.png", UriKind.Relative));
                    }
                    return new BitmapImage(new Uri("/Pictures/EPS-Normal.png", UriKind.Relative));
                }
                else
                {
                    int LstLightCount = LstLight.FindAll(x => Convert.ToString(x.DisBoxID) == infoDistributionBox.Code).Count;
                    if (FaultLightCount[infoDistributionBox.Code] > LstLightCount/2 && infoDistributionBox.Shield == 0)
                    {
                        return new BitmapImage(new Uri("/Pictures/EPS-Fault.png", UriKind.Relative));
                    }
                    return new BitmapImage(new Uri("/Pictures/EPS-Normal.png", UriKind.Relative));
                }
            }
        }
        

        private void EPSImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            QueryEPSDetailPageNoLogin((sender as Image).Tag.ToString());
            MasterController.Visibility = System.Windows.Visibility.Hidden;
            EPSDetailPageNoLogin.Visibility = System.Windows.Visibility.Visible;
        }

        private void QueryEPSDetailPageNoLogin(string infoDistributionBoxCode)
        {
            EPSSelected.Content = infoDistributionBoxCode;
            SelectInfoEPSNoLogin = LstDistributionBox.Find(x => x.Code == infoDistributionBoxCode);
            LstLightViewByDisBoxIDNoLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(infoDistributionBoxCode));
            LampNumByEPSSelected.Content = LstLightViewByDisBoxIDNoLogin.Count;
            if (EPSDetailPageNoLogin.Visibility == System.Windows.Visibility.Hidden)
            {
                LampImageDisplayCurrentPage = 1;
            }
            LampImageDisplayTotalPage = LstLightViewByDisBoxIDNoLogin.Count != 0 ? (LstLightViewByDisBoxIDNoLogin.Count - 1) / (4 * 4) + 1 : 1;
            LampByEPSCurrentPage.Content = LampImageDisplayCurrentPage;
            LampByEPSTotalPage.Content = LampImageDisplayTotalPage;
            InitLampShowNologin();
        }





        private string GainEPSStatus(DistributionBoxInfo infoDistributionBox)
        {
            if (infoDistributionBox.IsEmergency == 1)
            {
                return "应急中";
            }
            else
            {
                int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                if ((ConversionValue & 0X07FC) != 0)
                {
                    return infoDistributionBox.Shield == 0 ? "故障" : "正常";
                }
                else
                {
                    return "正常";
                }
            }
        }

        private string InspectGainEPSStatus(DistributionBoxInfo infoDistributionBox)
        {
            if (infoDistributionBox.IsEmergency == 1)
            {
                return "应急中";
            }
            else
            {
                int ConversionValue = Convert.ToInt32(infoDistributionBox.Status) & 0X07FC;
                if ((ConversionValue & 0X07FC) != 0)
                {
                    return infoDistributionBox.Shield == 0 ? "故障" : "正常";
                }
                else
                {
                    int LstLightCount = LstLight.FindAll(x => Convert.ToString(x.DisBoxID) == infoDistributionBox.Code).Count;
                    if (FaultLightCount[infoDistributionBox.Code] > LstLightCount/2)
                    {
                        return "故障";
                    }
                    return "正常";
                }
            }
        }

        /// <summary>
        /// 数据备份
        /// </summary>
        private void BackUpData()
        {
            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            DBHelperSQLite.ConnectionString = ConfigurationManager.AppSettings["ConnectionStringBackUp"].ToString();
            ObjDistributionBox.DeleteAll();
            ObjLight.DeleteAll();
            ObjGblSetting.DeleteAll();
            ObjFireAlarmType.DeleteAll();

            ObjDistributionBox.RestoreOrBackUpData(LstDistributionBox);
            ObjLight.RestoreOrBackUpData(LstLight);
            ObjGblSetting.RestoreOrBackUpData(LstGblSetting);
            ObjFireAlarmType.RestoreOrBackUpData(LstFireAlarmType);
            DBHelperSQLite.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            CommonFunct.PopupWindow("数据备份完成！");

            IsTimingQueryEPSOrLight = true;
        }

        public static void NewBackUpData()
        {
            string appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string dbFilePath = appStartupPath + @"\PCSmallHost.db";
            string dbNewFilePath = appStartupPath + @"\backup\PCSmallHostBackUp" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".db";
            string dirPath = appStartupPath + @"\backup";

            try
            {
                if (!Directory.Exists(dirPath))
                {
                    //创建backup文件夹
                    Directory.CreateDirectory(dirPath);
                }
                File.Copy(dbFilePath, dbNewFilePath);//拷贝数据库文件
                if (File.Exists(dbNewFilePath))
                {
                    CommonFunct.PopupWindow("数据备份完成！\n 文件路径：\n" + dirPath);
                }
            }
            catch
            {
                CommonFunct.PopupWindow("数据备份失败！");
            }
        }

        /// <summary>
        /// 备份数据恢复
        /// </summary>
        private void RestoreBackUpData()
        {
            IsTimingQueryEPSOrLight = false;
            while (!IsFinishQueryEPSOrLight && CurrentExeInstructTime < MaxExeInstructTime)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(ExeCommonInstructSleepTime);
                CurrentExeInstructTime += ExeCommonInstructSleepTime;
            }
            CurrentExeInstructTime = 0;

            ObjDistributionBox.DeleteAll();
            ObjLight.DeleteAll();
            ObjGblSetting.DeleteAll();

            DBHelperSQLite.ConnectionString = ConfigurationManager.AppSettings["ConnectionStringBackUp"].ToString();
            LstDistributionBox = ObjDistributionBox.GetAll();
            LstLight = ObjLight.GetAll();
            LstGblSetting = ObjGblSetting.GetAll();

            DBHelperSQLite.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            ObjDistributionBox.RestoreOrBackUpData(LstDistributionBox);
            ObjLight.RestoreOrBackUpData(LstLight);
            ObjGblSetting.RestoreOrBackUpData(LstGblSetting);
            ObjFireAlarmType.RestoreOrBackUpData(LstFireAlarmType);
            CommonFunct.PopupWindow("恢复备份数据完成！");

            IsTimingQueryEPSOrLight = true;
        }

        private void NewRestoreBackUpData()
        {
            string appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string backUpDir = appStartupPath + @"\backup";
            string dbFilePath = appStartupPath + @"\PCSmallHost.db";
            //选择文件
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "db文件|*.db",//筛选文件类型
                InitialDirectory = backUpDir
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //恢复（覆盖）文件
                File.Copy(dialog.FileName, dbFilePath, true);//拷贝文件，存在则覆盖
                if (File.GetLastWriteTime(dialog.FileName).ToString("yyyyMMddHHmmss") == File.GetLastWriteTime(dbFilePath).ToString("yyyyMMddHHmmss"))
                {
                    //通过比较2个文件的修改日期，进行判断
                    CommonFunct.PopupWindow("恢复备份数据完成！");
                    ResetSystemButton();
                }
                else
                {
                    CommonFunct.PopupWindow("恢复备份数据失败！\n 请手动拷贝文件进行恢复！");
                }
            }
        }

        /// <summary>
        /// 消音设置确定
        /// </summary>
        private void MuteSetConfirm1()
        {
            OtherFaultRecordInfo infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgHostFault.Tag.ToString());
            infoOtherFaultRecord.Disable = ImgHostFault.Source.ToString().Contains("Unchecked") ? 0 : 1;
            ObjOtherFaultRecord.Save(LstOtherFaultRecord);

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgEPSFault.Tag.ToString());
            infoOtherFaultRecord.Disable = ImgEPSFault.Source.ToString().Contains("Unchecked") ? 0 : 1;
            ObjOtherFaultRecord.Save(LstOtherFaultRecord);

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgLampFault.Tag.ToString());
            infoOtherFaultRecord.Disable = ImgLampFault.Source.ToString().Contains("Unchecked") ? 0 : 1;
            ObjOtherFaultRecord.Save(LstOtherFaultRecord);

            infoOtherFaultRecord = LstOtherFaultRecord.Find(x => x.Description == ImgDelaySetting.Tag.ToString());
            infoOtherFaultRecord.Disable = ImgDelaySetting.Source.ToString().Contains("Unchecked") ? 0 : 1;
            ObjOtherFaultRecord.Save(LstOtherFaultRecord);

            CommonFunct.PopupWindow("消音设置成功！");
        }

        /// <summary>
        /// 刷新故障图标
        /// </summary>
        private void ShowALLFault()
        {
            if (LstOtherFaultRecord.FindAll(x => x.IsExist == 1 && x.Disable == 0).Count != 0)
            {
                ALLFault.Visibility = HpALLFault.Visibility = System.Windows.Visibility.Visible;
                LayerState.Content = "故障";
                HostStatusLogin.Content = "故障";
            }
            else
            {
                ALLFault.Visibility = HpALLFault.Visibility = System.Windows.Visibility.Hidden;
                LayerState.Content = "正常";
                HostStatusLogin.Content = "正常";
            }
        }

        /// <summary>
        /// 清除配电箱信息
        /// </summary>
        private void ClearEPSDataLogin()
        {
            // 清除EPS数据页面
            EPSCode.Content = MainVoltage.Content = BatteryVoltage.Content = OutputVoltage.Content = DischargeCurrent.Content = TwoWaySignLamp.Content = DoubleHeadLamp.Content = BidirectionalBuriedLamp.Content = Floodlight.Content = OneWaySignLamp.Content = EXIT.Content = OneWayBuriedLamp.Content = FloorIndication.Content = FaultLight.Content = LampTotal.Content = 0;

            CurrentState.Content = "正常";

            EPSInitialPositionText.Text = EPSReservePlan1.Text = EPSReservePlan2.Text = EPSReservePlan3.Text
            = EPSReservePlan4.Text = EPSReservePlan5.Text = string.Empty;

            // 清除EPS汇总页面
            EPSCollectCode.Content = EPSCollectMainVoltage.Content = EPSCollectOutputVoltage.Content = EPSCollectBatteryVoltage.Content = EPSCollectDischargeCurrent.Content = EPSCollectSum.Content = EPSCollectFaultSum.Content = EPSCollectLampSum.Content = EPSCollectLampFaultSum.Content = 0;

            EPSCollectState.Content = "--";
            EPSCollectFaultType.Content = EPSCollectPosition.Content = string.Empty;
        }

        /// <summary>
        /// 清除选中配电箱信息
        /// </summary>
        private void ClearSelectInfoEPSNoLogin()
        {
            SelectInfoEPSNoLogin = new DistributionBoxInfo();
        }

        /// <summary>
        /// 清除选中配电箱和灯具信息
        /// </summary>
        private void ClearAllSelectInfoLogin()
        {
            SelectInfoEPSLogin = new DistributionBoxInfo();
            SelectInfoLightLogin = new LightInfo();
        }

        /// <summary>
        /// 清空火灾联动分区统计
        /// </summary>
        private void ClearFireAlarmLinkZoneNumberStat()
        {
            LstFireAlarmZone.ForEach(x => x.FireAlarmLinkStatCount = 0);
            LstFireAlarmZone.ForEach(x => x.IsFireAlarmLinkNow = false);
        }

        /// <summary>
        /// 清除时间文本框内容
        /// </summary>
        private void ClearFocusTime()
        {
            if (FocusTime != null)
            {
                int selectionStart = FocusTime.SelectionStart;
                int selectionLength = FocusTime.SelectionLength;
                string strKeyText = FocusTime.Text.Trim();
                if (strKeyText.Length != 0)
                {
                    if (selectionLength != 0)
                    {
                        strKeyText = strKeyText.Remove(selectionStart, selectionLength);
                    }
                    else
                    {
                        selectionStart--;
                        if (selectionStart != -1)
                        {
                            strKeyText = strKeyText.Remove(selectionStart, 1);
                        }
                    }
                }
                FocusTime.Text = strKeyText;
                if (selectionStart != -1)
                {
                    FocusTime.SelectionStart = selectionStart;
                }
            }
        }

        /// <summary>
        /// 清除所有加载到当前楼层的图标
        /// </summary>
        private void ClearIconSearchCode()
        {
            if (cvsMainWindow.Children.Count != MainWindowChildCount)//MainWindowChildCount
            {
                cvsMainWindow.Children.RemoveRange(MainWindowChildCount, cvsMainWindow.Children.Count - MainWindowChildCount);
            }
        }

        /// <summary>
        /// 刷新所有楼层
        /// </summary>
        private void RefreshAllIcon()
        {
            if (IsOpenLayerModeNoLogin)
            {
                RefreshLayerModeNoLogin(true, true);
            }

            if (IsOpenLayerModeLogin)
            {
                RefreshLayerModeLogin(true, true);
            }
        }

        /// <summary>
        /// 恢复所有图层图标
        /// </summary>
        private void RestoreAllIcon()
        {
            ClearIconSearchCode();
            if (IsOpenLayerModeNoLogin)
            {
                LoadIconSearchCodeNoLogin();
                //RefreshIconSearchCodeNoLogin();
                if (IsRealFireAlarmLink)
                {
                    InitPartitionNoLogin();
                    RefreshEscapeRoutesNoLogin();
                }
            }
            if (IsOpenLayerModeLogin)
            {
                UpdateScaleTransformPositionLogin();
                LoadIconSearchCodeLogin();
                RefreshIconSearchCodeLogin();
                if (IsPrintLine || IsEditAllIcon || IsShowDirection)
                {
                    InitPartitionLogin();
                    RefreshEscapeRoutesLogin();
                }
            }
        }

        /// <summary>
        /// 切换楼层
        /// </summary>
        private void SwitchFloorLogin()
        {
            ShowCurrentPageFloorLogin();
            LoadFloorDrawingLogin();
            GetDataCurrentFloorLogin();
            ClearAllIconPanelLogin();
            RestoreAllIcon();
        }

        /// <summary>
        /// 切换楼层
        /// </summary>
        private void SwitchFloorNoLogin()
        {
            LoadFloorDrawingNoLogin();
            GetDataCurrentFloorNoLogin();
            RestoreAllIcon();
        }

        /// <summary>
        /// 增加图标进入列表
        /// </summary>
        private void AddIconSearchCodeListLogin(object infoDisBoxOrLight)
        {
            string disCode = comIconSearchCode.SelectedItem.ToString();
            DistributionBoxInfo infoDistributionBox = infoDisBoxOrLight as DistributionBoxInfo;
            if (infoDistributionBox != null)
            {
                if (infoDistributionBox.Code == disCode)
                {
                    IconSearchCode IconSearchCode = GetIconSearchCodeByListLogin(infoDisBoxOrLight);
                    lvIconSearchCodeList.Items.Insert(0, IconSearchCode);
                }
            }
            if (infoDisBoxOrLight is LightInfo)
            {
                LightInfo infoLight = infoDisBoxOrLight as LightInfo;
                if (infoLight.DisBoxID == int.Parse(disCode))
                {
                    IconSearchCode IconSearchCode = GetIconSearchCodeByListLogin(infoDisBoxOrLight);
                    lvIconSearchCodeList.Items.Insert(GetInsertIndex(infoLight), IconSearchCode);
                }
            }
        }

        /// <summary>
        /// 获取指定灯具图标在列表中的位置
        /// </summary>
        /// <param name="infoLight">指定灯具数据-</param>
        /// <returns></returns>
        private int GetInsertIndex(LightInfo infoLight)
        {
            List<int> lightIndex = new List<int>();
            int Number = 0;
            for (int i = 0; i < lvIconSearchCodeList.Items.Count; i++)
            {
                IconSearchCode iconSearchCode = lvIconSearchCodeList.Items[i] as IconSearchCode;
                if (iconSearchCode != null && iconSearchCode.Tag is LightInfo)
                {
                    lightIndex.Add(((lvIconSearchCodeList.Items[i] as IconSearchCode).Tag as LightInfo).LightIndex);
                }
            }

            if (lightIndex.Count == 0)
            {
                return Number = 0;
            }

            if (lightIndex.Count == 1)
            {
                return Number = infoLight.LightIndex < lightIndex[0] ? 0 : 1;
            }

            if (lightIndex.Count > 1)
            {
                for (int i = 0; i < lightIndex.Count; i++)
                {
                    if (infoLight.LightIndex > lightIndex[i] && infoLight.LightIndex < lightIndex[i + 1])
                    {
                        Number = i + 1;
                        break;
                    }
                }

                IconSearchCode icon = lvIconSearchCodeList.Items[0] as IconSearchCode;
                if (icon != null && icon.Tag is DistributionBoxInfo)
                {
                    Number++;
                }
                return Number;
            }
            return 0;
        }

        /// <summary>
        /// 移除图标列表中的图标
        /// </summary>
        private void RemoveIconSearchCodeListLogin()
        {
            IconSearchCode IconSearchCode = SelectIconSearchCode as IconSearchCode;
            if (IconSearchCode != null && (IconSearchCode.Tag as DistributionBoxInfo != null || IconSearchCode.Tag as LightInfo != null))
            {
                lvIconSearchCodeList.Items.Remove(IconSearchCode);
            }
        }

        /// <summary>
        /// 初始化图标列表（已有数据）
        /// </summary>
        private void InitIconSearchCodeListLogin()
        {
            string strIconSearchCode = null;

            ObservableCollection<string> ComDistributionBox = new ObservableCollection<string>();
            List<CoordinateInfo> LstCinLocation = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.Location != 0);
            List<DistributionBoxInfo> LstDisLocation = LstDistributionBox.FindAll(x => LstCinLocation.Find(y => y.TableID == x.ID) == null);//已经设置楼层的配电箱
            List<DistributionBoxInfo> LstDisNoLocation = new List<DistributionBoxInfo>();//未设置楼层的配电箱
            List<LightInfo> LstLightByDis = new List<LightInfo>();

            for (int i = 0; i < LstDisLocation.Count; i++)
            {
                LstLightByDis = LstLight.FindAll(x => x.DisBoxID == int.Parse(LstDisLocation[i].Code));
                if (LstLightByDis.Any(x => LstCoordinate.Find(y => y.TableName == EnumClass.TableName.Light.ToString() && y.TableID == x.ID).Location == 0))
                {
                    LstDisNoLocation.Add(LstDisLocation[i]);
                }
            }
            LstDisNoLocation.Sort();

            foreach (DistributionBoxInfo disBox in LstDistributionBox)
            {
                ComDistributionBox.Add(disBox.Code);
            }

            comIconSearchCode.ItemsSource = ComDistributionBox;
            comIconSearchCode.IsEnabled = true;
            comIconSearchCode.SelectedIndex = 0;
            lvIconSearchCodeList.Items.Clear();

            if (comIconSearchCode.SelectedItem != null)
            {
                strIconSearchCode = comIconSearchCode.SelectedItem.ToString();
            }

            if (strIconSearchCode != null)
            {
                IconSearchCode iconSearchCode;
                DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == strIconSearchCode);
                if (LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID).Location == 0)
                {
                    iconSearchCode = GetIconSearchCodeByListLogin(infoDistributionBox);
                    lvIconSearchCodeList.Items.Add(iconSearchCode);
                }

                LstLightByDis.Clear();
                LstLightIconSearchCodeList = LstLight.FindAll(x => x.DisBoxID == int.Parse(strIconSearchCode));
                LstLightByDis.AddRange(LstLightIconSearchCodeList.Where(x => LstCoordinate.Find(y => y.TableName == EnumClass.TableName.Light.ToString() && y.TableID == x.ID).Location == 0));

                foreach (LightInfo infoLight in LstLightByDis)
                {
                    iconSearchCode = GetIconSearchCodeByListLogin(infoLight);
                    lvIconSearchCodeList.Items.Add(iconSearchCode);
                }
            }

            lvIconSearchCodeList.Items.Add(new PartitionPoint());
        }


        /// <summary>
        /// 初始化图标列表（未有数据）
        /// </summary>
        private void InitIconSearchCodeList()
        {
            btnEditAllIcon_Click(null, null);
            comIconSearchCode.IsEnabled = false;//禁用EPS选择框
            lvIconSearchCodeList.Items.Clear();
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("配电箱"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("照明灯"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("双头灯"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("楼层指示"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("安全出口"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("单向左向"));
            //this.lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("单向右向"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("双向标志灯"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("单向地埋灯"));
            lvIconSearchCodeList.Items.Add(GetIconSearchCodeByListLogin("双向地埋灯"));
            lvIconSearchCodeList.Items.Add(new PartitionPoint());

            SetIsEditAllIcon(true);
        }

        /// <summary>
        /// 点击当前楼层
        /// </summary>
        private void ClickCurrentFloor(object sender)
        {
            string strCurrentFloor = (sender as System.Windows.Controls.Label).Content.ToString();
            //foreach (System.Windows.Controls.Control Control in this.stpCurrentPageFloorNoLogin.Children)
            //{
            //    System.Windows.Controls.Label LabelPageFloorNoLogin = Control as System.Windows.Controls.Label;
            //    if (strCurrentFloor == LabelPageFloorNoLogin.Content.ToString())
            //    {
            //        CurrentSelectFloorNoLogin = LabelPageFloorNoLogin.Content.ToString();
            //        LabelPageFloorNoLogin.Foreground = CommonFunct.GetBrush(SelectFloorForeground);
            //    }
            //    else
            //    {
            //        LabelPageFloorNoLogin.Foreground = CommonFunct.GetBrush(UnSelectFloorForeground);
            //    }
            //}
        }

        /// <summary>
        /// 按下图纸
        /// </summary>       
        private void ClickDownFloorDrawingNoLogin(MouseButtonEventArgs e)
        {
            DragFloorNoLogin = e.GetPosition(ctcFloorDrawingNoLogin);
            DragFloorNoLogin = new Point(Math.Round(DragFloorNoLogin.X, 2), Math.Round(DragFloorNoLogin.Y, 2));
            LastDragFloorNoLogin = DragFloorNoLogin;
        }

        /// <summary>
        /// 按下图纸
        /// </summary>
        private void ClickDownFloorDrawingNoLogin(TouchEventArgs e)
        {
            DragFloorNoLogin = e.GetTouchPoint(ctcFloorDrawingNoLogin).Position;
            DragFloorNoLogin = new Point(Math.Round(DragFloorNoLogin.X), Math.Round(DragFloorNoLogin.Y));
            LastDragFloorNoLogin = DragFloorNoLogin;
            imgFloorDrawingNoLogin.CaptureTouch(e.TouchDevice);
        }

        /// <summary>
        /// 按下图纸
        /// </summary>       
        private void ClickDownFloorDrawingLogin(MouseButtonEventArgs e)
        {
            DragFloorLogin = e.GetPosition(ctcFloorDrawingLogin);
            DragFloorLogin = new Point(Math.Round(DragFloorLogin.X), Math.Round(DragFloorLogin.Y));
            LastDragFloorLogin = DragFloorLogin;
        }

        /// <summary>
        /// 按下图纸
        /// </summary>       
        private void ClickDownFloorDrawingLogin(TouchEventArgs e)
        {
            DragFloorLogin = e.GetTouchPoint(ctcFloorDrawingLogin).Position;
            DragFloorLogin = new Point(Math.Round(DragFloorLogin.X), Math.Round(DragFloorLogin.Y));
            LastDragFloorLogin = DragFloorLogin;
            imgFloorDrawingLogin.CaptureTouch(e.TouchDevice);
        }

        /// <summary>
        /// 松开图纸
        /// </summary>      
        private void ClickUpFloorDrawingNoLogin()
        {
            LastDragFloorNoLogin = new Point(0, 0);
        }

        /// <summary>
        /// 松开图纸
        /// </summary>
        private void ClickUpFloorDrawingNoLogin(TouchEventArgs e)
        {
            LastDragFloorNoLogin = new Point(0, 0);
            imgFloorDrawingNoLogin.ReleaseTouchCapture(e.TouchDevice);
        }

        /// <summary>
        /// 松开图纸
        /// </summary>
        private void ClickUpFloorDrawingLogin()
        {
            LastDragFloorLogin = new Point(0, 0);
        }

        /// <summary>
        /// 松开图纸
        /// </summary>      
        private void ClickUpFloorDrawingLogin(TouchEventArgs e)
        {
            LastDragFloorLogin = new Point(0, 0);
            imgFloorDrawingLogin.ReleaseTouchCapture(e.TouchDevice);
        }

        /// <summary>
        /// 根据勾选决定消音设置
        /// </summary>
        /// <param name="Image"></param>
        private void ImageMuteSet1(Image Image)
        {
            string path = Image.Source.ToString();
            string tag = Image.Tag.ToString();
            if (path.Contains("Unchecked"))
            {
                switch (tag)
                {
                    case "主机故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Selected.png", UriKind.Relative));
                        break;
                    case "EPS故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Selected.png", UriKind.Relative));
                        break;
                    case "灯具故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Selected.png", UriKind.Relative));
                        break;
                    case "延时设置":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Selected.png", UriKind.Relative));
                        break;
                }
            }
            else
            {
                switch (tag)
                {
                    case "主机故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\HostFault-Unchecked.png", UriKind.Relative));
                        break;
                    case "EPS故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\EPSFault-Unchecked.png", UriKind.Relative));
                        break;
                    case "灯具故障":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\LampFault-Unchecked.png", UriKind.Relative));
                        break;
                    case "延时设置":
                        Image.Source = new BitmapImage(new Uri("\\Pictures\\DelaySetting-Unchecked.png", UriKind.Relative));
                        break;
                }
            }
        }

        private void GetImageLightCurrentState()
        {
            LampBright.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Unchecked.png", UriKind.Relative));
            LampExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Unchecked.png", UriKind.Relative));
            LampShine.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Unchecked.png", UriKind.Relative));
            LampMainEle.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Unchecked.png", UriKind.Relative));
            LampLeftOpen.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Unchecked.png", UriKind.Relative));
            LampRightOpen.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Unchecked.png", UriKind.Relative));

            switch (SelectInfoLightLogin.CurrentState)
            {
                case (int)EnumClass.SingleLightCurrentState.全灭:
                    LampExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Selected.png", UriKind.Relative));
                    break;
                case (int)EnumClass.SingleLightCurrentState.全亮:
                    LampBright.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Selected.png", UriKind.Relative));
                    break;
                case (int)EnumClass.SingleLightCurrentState.闪:
                    LampShine.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Selected.png", UriKind.Relative));
                    break;
                case (int)EnumClass.SingleLightCurrentState.主电:
                    LampMainEle.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Selected.png", UriKind.Relative));
                    break;
                case (int)EnumClass.SingleLightCurrentState.左亮:
                    LampLeftOpen.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Selected.png", UriKind.Relative));
                    break;
                case (int)EnumClass.SingleLightCurrentState.右亮:
                    LampRightOpen.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Selected.png", UriKind.Relative));
                    break;
            }
        }

        /// <summary>
        /// 获取灯具初始状态 
        /// </summary>       
        private void GetImageLightBeginStatus()
        {
            switch (SelectInfoLightLogin.BeginStatus)
            {
                case (int)EnumClass.LightStatusClass.双向标志灯全亮:
                case (int)EnumClass.LightStatusClass.双向地埋灯全亮:
                case (int)EnumClass.LightStatusClass.其它全亮:
                    InitialStateBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusOpenClicked.png", UriKind.Relative));
                    InitialStateRightBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusRightOpenUnClicked.png", UriKind.Relative));
                    InitialStateLeftBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
                    InitialStateExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusCloseUnClicked.png", UriKind.Relative));
                    break;
                case (int)EnumClass.LightStatusClass.右亮:
                    InitialStateBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusOpenUnClicked.png", UriKind.Relative));
                    InitialStateRightBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusRightOpenClicked.png", UriKind.Relative));
                    InitialStateLeftBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
                    InitialStateExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusCloseUnClicked.png", UriKind.Relative));
                    break;
                case (int)EnumClass.LightStatusClass.左亮:
                    InitialStateBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusOpenUnClicked.png", UriKind.Relative));
                    InitialStateRightBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusRightOpenUnClicked.png", UriKind.Relative));
                    InitialStateLeftBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusLeftOpenClicked.png", UriKind.Relative));
                    InitialStateExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusCloseUnClicked.png", UriKind.Relative));
                    break;
                case (int)EnumClass.LightStatusClass.全灭:
                    InitialStateBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusOpenUnClicked.png", UriKind.Relative));
                    InitialStateRightBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusRightOpenUnClicked.png", UriKind.Relative));
                    InitialStateLeftBright.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
                    InitialStateExtinguish.Source = new BitmapImage(new Uri("\\Pictures\\BeginStatusCloseClicked.png", UriKind.Relative));
                    break;
            }
        }

        /// <summary>
        /// 获取灯具初始状态
        /// </summary>      
        private int GetDataLightBeginStatus()
        {
            if (!InitialStateBright.Source.ToString().Contains("UnClicked"))
            {
                if (SelectInfoLightLogin.LightClass == (int)EnumClass.LightClass.双向标志灯 || SelectInfoLightLogin.LightClass == (int)EnumClass.LightClass.双向地埋灯)
                {
                    if (SelectInfoLightLogin.LightClass == (int)EnumClass.LightClass.双向标志灯)
                    {
                        return (int)EnumClass.LightStatusClass.双向标志灯全亮;
                    }
                    else
                    {
                        return (int)EnumClass.LightStatusClass.双向地埋灯全亮;
                    }
                }
                else
                {
                    return (int)EnumClass.LightStatusClass.其它全亮;
                }
            }
            else if (!InitialStateLeftBright.Source.ToString().Contains("UnClicked"))
            {
                return (int)EnumClass.LightStatusClass.左亮;
            }
            else if (!InitialStateRightBright.Source.ToString().Contains("UnClicked"))
            {
                return (int)EnumClass.LightStatusClass.右亮;
            }
            else
            {
                return (int)EnumClass.LightStatusClass.全灭;
            }
        }

        /// <summary>
        /// 获取灯具初始化状态
        /// </summary>       
        private int GetLightInitStatus(int lightClass)
        {
            switch (lightClass)
            {
                case (int)EnumClass.LightClass.照明灯:
                case (int)EnumClass.LightClass.双头灯:
                    return (int)EnumClass.LightStatusClass.全灭;
                case (int)EnumClass.LightClass.双向标志灯:
                    return (int)EnumClass.LightStatusClass.双向标志灯全亮;
                case (int)EnumClass.LightClass.双向地埋灯:
                    return (int)EnumClass.LightStatusClass.双向地埋灯全亮;
                case (int)EnumClass.LightClass.安全出口灯:
                case (int)EnumClass.LightClass.楼层灯:
                case (int)EnumClass.LightClass.单向标志灯:
                case (int)EnumClass.LightClass.单向地埋灯:
                    return (int)EnumClass.LightStatusClass.其它全亮;
            }
            return -1;
        }



        /// <summary>
        /// 获取当前楼层所有的配电箱以及灯具
        /// </summary>
        private void GetDataCurrentFloorNoLogin()
        {
            LstCoordinateCurrentFloorNoLogin = LstCoordinate.FindAll(x => x.Location == Convert.ToInt32(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));

            LstEscapeLines = ObjEscapeLines.GetAll();
            LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
            //LstDistributionBoxCurrentFloorNoLogin = LstDistributionBox.FindAll(x => x.Location == Convert.ToInt32(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
            //LstLightCurrentFloorNoLogin = LstLight.FindAll(x => x.Location == Convert.ToInt32(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
            //LstPartitionPointCurrentFloorNoLogin = LstPlanPartitionPointRecord.FindAll(x => x.Location == Convert.ToInt32(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
            //LstEscapeRoutes = ObjEscapeRoutes.GetAll();
            //LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => x.Location == int.Parse(CurrentSelectFloorNoLogin.Substring(0, CurrentSelectFloorNoLogin.Length - 1)));
        }

        /// <summary>
        /// 获取当前楼层所有的配电箱以及灯具
        /// </summary>
        private void GetDataCurrentFloorLogin()
        {
            //LstEscapeRoutes = ObjEscapeRoutes.GetAll();
            //LstPlanPartitionPointRecord = ObjPlanPartitionPointRecord.GetAll();
            //LstDistributionBoxCurrentFloorLogin = LstDistributionBox.FindAll(x => x.Location == CurrentSelectFloorLogin);
            //LstLightCurrentFloorLogin = LstLight.FindAll(x => x.Location == CurrentSelectFloorLogin);
            //LstPartitionPointCurrentFloorLogin = LstPlanPartitionPointRecord.FindAll(x => x.Location == CurrentSelectFloorLogin);
            //LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => x.Location == CurrentSelectFloorLogin);
            LstEscapeLines = ObjEscapeLines.GetAll();
            LstCoordinate = ObjCoordinate.GetAll(); ;
            LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);
            LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

        }

        /// <summary>
        /// 清除点击图标对应的信息面板
        /// </summary>
        private void ClearShowIconSearchCodeInfoNoLogin(bool isVisible)
        {
            stpIconSearCodeInfoNoLogin.Visibility = isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 清除点击图层图标的面板
        /// </summary>      
        private void ClearAllIconPanelLogin()
        {
            stpIconSearCodeInfoLogin.Children.Clear();
            stpPartitionPointInfoLogin.Children.Clear();
        }

        /// <summary>
        /// 清除图形界面一切元素
        /// </summary>
        private void ClearAllElementLogin()
        {
            if (stpLayerModeLogin.Visibility != System.Windows.Visibility.Visible)
            {
                SetIsOpenLayerModeLogin(false);
            }
            ClearAllSelectInfoLogin();
            ClearIconSearchCode();
            ClearAllIconPanelLogin();
        }

        /// <summary>
        /// 清除图形界面一切元素
        /// </summary>
        private void ClearAllElementNoLogin()
        {
            ClearIconSearchCode();
            ClearShowIconSearchCodeInfoNoLogin(false);
            ClearSelectInfoEPSNoLogin();
        }

        /// <summary>
        /// 显示点击图标面板
        /// </summary>       
        private void ShowIconSearchCodePanelLogin(object infoDisBoxOrLight, Image image)
        {
            stpIconSearCodeInfoLogin.Visibility = System.Windows.Visibility.Visible;
            DistributionBoxInfo infoDistributionBox = infoDisBoxOrLight as DistributionBoxInfo;
            if (infoDistributionBox != null || infoDisBoxOrLight.ToString() == "配电箱")
            {
                stpIconSearCodeInfoLogin.Children.Add(new ControlIconSearchCode(infoDistributionBox, null, infoDisBoxOrLight, image));
            }
            else
            {
                LightInfo infoLight = infoDisBoxOrLight as LightInfo;
                if (infoLight != null)
                {
                    infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoLight.DisBoxID.ToString());
                }
                stpIconSearCodeInfoLogin.Children.Add(new ControlIconSearchCode(infoDistributionBox, infoLight, infoDisBoxOrLight, image));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <param name="infoLight"></param>
        public void UpdateFacPlanInLayer(DistributionBoxInfo infoDistributionBox, LightInfo infoLight, object infoDisBoxOrLight)
        {
            if (infoLight != null)
            {
                EPSComboBox.Visibility = System.Windows.Visibility.Hidden;
                LampNewCode.Visibility = System.Windows.Visibility.Hidden;
                IDExegesis.Visibility = System.Windows.Visibility.Hidden;
                BlankIconID.Visibility = System.Windows.Visibility.Hidden;
                LampForEPSNum.Visibility = System.Windows.Visibility.Visible;
                DisCode.Visibility = System.Windows.Visibility.Visible;
                Number.Visibility = System.Windows.Visibility.Visible;
                LampForEPSNum.Content = "所在EPS：";
                EPSOrLampCode.Content = "编 码：";
                Number.Content = infoLight.Code;
                DisCode.Content = infoDistributionBox.Code;
                Type.Content = CommonFunct.GetLightClass(infoLight);
                FacilityAddress.Text = infoLight.Address;

                LampLeftPlan1.Text = infoLight.PlanLeft1.ToString();
                LampLeftPlan2.Text = infoLight.PlanLeft2.ToString();
                LampLeftPlan3.Text = infoLight.PlanLeft3.ToString();
                LampLeftPlan4.Text = infoLight.PlanLeft4.ToString();


                if (infoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯 || infoLight.LightClass == (int)EnumClass.LightClass.双向标志灯)
                {
                    LampRightPlan1.Text = infoLight.PlanRight1.ToString();
                    LampRightPlan2.Text = infoLight.PlanRight2.ToString();
                    LampRightPlan3.Text = infoLight.PlanRight3.ToString();
                    LampRightPlan4.Text = infoLight.PlanRight4.ToString();
                    LampRightPlan.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    LampRightPlan.Visibility = System.Windows.Visibility.Hidden;
                }
                EPSPlan.Visibility = System.Windows.Visibility.Hidden;
                LampPlan.Visibility = System.Windows.Visibility.Visible;
                if (infoLight.Shield == 1)
                {
                    Shield.Content = "屏蔽中";
                    Shield.IsEnabled = false;
                    Unblock.IsEnabled = true;
                }
                else
                {
                    Shield.Content = "屏蔽";
                    Shield.IsEnabled = true;
                    Unblock.IsEnabled = false;
                }
                Shield.Visibility = System.Windows.Visibility.Visible;
                Unblock.Visibility = System.Windows.Visibility.Visible;
            }
            else if (infoDistributionBox != null)
            {
                EPSComboBox.Visibility = System.Windows.Visibility.Hidden;
                IDExegesis.Visibility = System.Windows.Visibility.Hidden;
                BlankIconID.Visibility = System.Windows.Visibility.Hidden;
                LampForEPSNum.Visibility = LampNewCode.Visibility = System.Windows.Visibility.Hidden;
                DisCode.Visibility = System.Windows.Visibility.Hidden;
                Number.Visibility = System.Windows.Visibility.Visible;
                EPSOrLampCode.Content = "编 码：";
                Number.Content = infoDistributionBox.Code;
                Type.Content = "配电箱";
                FacilityAddress.Text = infoDistributionBox.Address;

                EPSPlan1.Text = infoDistributionBox.Plan1.ToString();
                EPSPlan2.Text = infoDistributionBox.Plan2.ToString();
                EPSPlan3.Text = infoDistributionBox.Plan3.ToString();
                EPSPlan4.Text = infoDistributionBox.Plan4.ToString();
                EPSPlan5.Text = infoDistributionBox.Plan5.ToString();

                LampPlan.Visibility = System.Windows.Visibility.Hidden;
                EPSPlan.Visibility = System.Windows.Visibility.Visible;
                Shield.Visibility = System.Windows.Visibility.Visible;
                Unblock.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ObservableCollection<string> ComDistributionBox = new ObservableCollection<string>();//记录已经连接的转折点的下标
                EPSOrLampCode.Content = "EPS码:";
                LampForEPSNum.Content = "灯码:";
                for (int i = 0; i < LstDistributionBox.Count; i++)
                {
                    ComDistributionBox.Add(LstDistributionBox[i].Code);
                }
                EPSComboBox.ItemsSource = null;
                EPSComboBox.ItemsSource = ComDistributionBox;
                EPSComboBox.SelectedIndex = 0;
                DisCode.Visibility = System.Windows.Visibility.Hidden;
                EPSComboBox.Visibility = System.Windows.Visibility.Visible;
                LampForEPSNum.Visibility = System.Windows.Visibility.Visible;
                LampNewCode.Visibility = System.Windows.Visibility.Visible;
                if (infoDisBoxOrLight is BlankIconInfo)
                {
                    IDExegesis.Visibility = System.Windows.Visibility.Visible;
                    BlankIconID.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    IDExegesis.Visibility = System.Windows.Visibility.Hidden;
                    BlankIconID.Visibility = System.Windows.Visibility.Hidden;
                }

                if (infoDisBoxOrLight.ToString() == "配电箱")
                {
                    Number.Content = 0;
                    Number.Visibility = System.Windows.Visibility.Hidden;
                    LampForEPSNum.Visibility = LampNewCode.Visibility = System.Windows.Visibility.Hidden;

                    Type.Content = "配电箱";
                    FacilityAddress.Text = "安装位置未初始化";

                    EPSPlan1.Text = EPSPlan2.Text = EPSPlan3.Text = EPSPlan4.Text = EPSPlan5.Text = "0";

                    LampPlan.Visibility = System.Windows.Visibility.Hidden;
                    EPSPlan.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    Number.Content = 0;
                    Number.Visibility = System.Windows.Visibility.Hidden;
                    LampForEPSNum.Visibility = LampNewCode.Visibility = System.Windows.Visibility.Visible;
                    if (infoDisBoxOrLight is BlankIconInfo)
                    {
                        BlankIconID.Content = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight));
                    }
                    Type.Content = CommonFunct.GetLightClass(infoDisBoxOrLight);
                    FacilityAddress.Text = "安装位置未初始化";
                    LampLeftPlan1.Text = LampLeftPlan2.Text = LampLeftPlan3.Text = LampLeftPlan4.Text = "0";
                    if (infoDisBoxOrLight.ToString() == "双向标志灯" || infoDisBoxOrLight.ToString() == "双向地埋灯")
                    {
                        LampRightPlan1.Text = LampRightPlan2.Text = LampRightPlan3.Text = LampRightPlan4.Text = "0";
                        LampRightPlan.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        LampRightPlan.Visibility = System.Windows.Visibility.Hidden;
                    }
                    EPSPlan.Visibility = System.Windows.Visibility.Hidden;
                    LampPlan.Visibility = System.Windows.Visibility.Visible;
                }
                Shield.Visibility = System.Windows.Visibility.Hidden;
                Unblock.Visibility = System.Windows.Visibility.Hidden;
            }

            stpLayerEdit.Visibility = System.Windows.Visibility.Visible;
            stpIconSearCodeInfoLogin.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// 设置灯具设备与逃生路线的联系
        /// </summary>
        /// <param name="infoLight"></param>
        public void LampRelationLines(LightInfo infoLight, DistributionBoxInfo infoDistriButionBox)
        {
            ObservableCollection<string> ComEscapeLines = new ObservableCollection<string>();//记录已经连接的转折点的下标
            List<EscapeLinesInfo> LstEscapeLines = ObjEscapeLines.GetAll().FindAll(x => x.Location == CurrentSelectFloorLogin);

            for (int i = 0; i < LstEscapeLines.Count; i++)
            {
                ComEscapeLines.Add(LstEscapeLines[i].Name);
            }

            LineName.ItemsSource = ComEscapeLines;
            if (infoLight.EscapeLineID != null && infoLight.EscapeLineID != -1)
            {
                LineName.SelectedItem = LstEscapeLines.Find(x => x.ID == infoLight.EscapeLineID).Name;
            }
            else
            {
                LineName.SelectedIndex = 0;
            }

            RelationLampCode.Content = infoLight.Code;
            RelationEPSCode.Content = infoDistriButionBox.Code;
            LampRelationLine.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 设备图标顺时针旋转90度
        /// </summary>
        /// <param name="infoDistributionBox"></param>
        /// <param name="infoLight"></param>
        public void StartDeviceIconRotate(DistributionBoxInfo infoDistributionBox, LightInfo infoLight)
        {
            //List<LightInfo> lstlight = ObjLight.GetAll().FindAll(x => x.Location == CurrentSelectFloorLogin);
            if (infoLight != null && infoLight.Code.Substring(0, 1) != "1" && infoLight.Code.Substring(0, 1) != "3" && infoLight.Code.Substring(0, 1) != "5")
            {
                infoLight.RtnDirection += 90;
                if (infoLight.RtnDirection == 360)
                {
                    infoLight.RtnDirection = 0;
                }
                ObjLight.Update(infoLight);
                LstLight = ObjLight.GetAll();

                ClearAllElementLogin();
                GetDataCurrentFloorLogin();
                UpdateScaleTransformPositionLogin();
                LoadIconSearchCodeLogin();
                RefreshIconSearchCodeLogin();
            }
        }

        /// <summary>
        /// 显示出选中的逃生路线线段
        /// </summary>
        public void ShowSelectEscapeLine(string LineName)
        {
            EscapeLinesInfo infoEscapeLine = LstEscapeLines.Find(x => x.Location == CurrentSelectFloorLogin && x.Name == LineName);
            for (int i = 0; i < cvsMainWindow.Children.Count; i++)
            {
                Line line = cvsMainWindow.Children[i] as Line;
                if (line != null)
                {
                    line.Stroke = System.Windows.Media.Brushes.LightSkyBlue;
                }
            }

            for (int i = 0; i < cvsMainWindow.Children.Count; i++)
            {
                Line line = cvsMainWindow.Children[i] as Line;
                if (line == null)
                {
                    continue;
                }
                else
                {
                    if (line.Name == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (line.X1 == infoEscapeLine.LineX1 && line.X2 == infoEscapeLine.LineX2 && line.Y1 == infoEscapeLine.LineY1 && line.Y2 == infoEscapeLine.LineY2)
                        {
                            SelectedLine = infoEscapeLine;
                            line.Stroke = System.Windows.Media.Brushes.Red;
                        }
                    }
                }
            }
        }

        public void RemoveSelectedEscapeLine()
        {
            SelectedLine = new EscapeLinesInfo();
        }

        /// <summary>
        /// 显示点击图标面板
        /// </summary>
        private void ShowPartitionPanelLogin(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            List<EscapeLinesInfo> LstCurrentEscapeLine = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);
            ControlPartitionPoint ControlPartitionPoint = new ControlPartitionPoint(infoPlanPartitionPointRecord, LstFireAlarmPartitionSet, LstCurrentEscapeLine);
            ControlPartitionPoint.ShowDialog();
        }

        /// <summary>
        /// 移除EPS和灯具图标
        /// </summary>
        private void RemoveIconSearchCode(object infoDisBoxOrLight)
        {
            ClearIconSearchCodePos(infoDisBoxOrLight);
            ClearAllElementLogin();
            GetDataCurrentFloorLogin();
            LoadIconSearchCodeLogin();
            RefreshIconSearchCodeLogin();
            AddIconSearchCodeListLogin(infoDisBoxOrLight);
        }

        /// <summary>
        /// 移除报警点图标
        /// </summary>      
        private void RemovePartitionPoint(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            DeletePartitionPoint(infoPlanPartitionPointRecord);
            ClearAllIconPanelLogin();
            GetDataCurrentFloorLogin();
            for (int i = MainWindowChildCount; i < cvsMainWindow.Children.Count; i++)
            {
                PartitionPoint PartitionPoint = cvsMainWindow.Children[i] as PartitionPoint;
                if (PartitionPoint != null)
                {
                    cvsMainWindow.Children.Remove(PartitionPoint);
                    i--;
                }
            }
            InitPartitionLogin();
        }

        /// <summary>
        /// 清空选中EPS和灯具图标的坐标
        /// </summary>
        private void ClearIconSearchCodePos(object infoDisBoxOrLight)
        {
            if (infoDisBoxOrLight is DistributionBoxInfo || infoDisBoxOrLight is LightInfo || infoDisBoxOrLight is BlankIconInfo)
            {
                CoordinateInfo infoCoordinate = new CoordinateInfo();
                int ID = (int)infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight);
                if (infoDisBoxOrLight is DistributionBoxInfo)
                {
                    infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                }
                else if (infoDisBoxOrLight is LightInfo)
                {
                    infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == ID);
                }
                else
                {
                    infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == ID);
                    ObjBlankIcon.Delete(ID);
                    LstBlankIcon = ObjBlankIcon.GetAll();
                }
                infoCoordinate.Location = infoCoordinate.IsAuth = 0;
                infoCoordinate.OriginX = infoCoordinate.OriginY = infoCoordinate.TransformX = infoCoordinate.TransformY = infoCoordinate.NLOriginX = infoCoordinate.NLOriginY = 0;
                ObjCoordinate.Update(infoCoordinate);
            }
            else
            {
                BlankIcon = new Image();
            }
        }

        /// <summary>
        /// 删除报警点图标
        /// </summary>      
        private void DeletePartitionPoint(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            ObjPlanPartitionPointRecord.Delete(infoPlanPartitionPointRecord.ID);
            LstPlanPartitionPointRecord.Remove(infoPlanPartitionPointRecord);
        }

        /// <summary>
        /// 移动点击图标对应的信息
        /// </summary>
        private void MoveIconSearchCodeInfoPanelNoLogin(object infoDisBoxOrLight)
        {
            CoordinateInfo coordinate = LstCoordinate.Find(x => x.TableID == Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight)));
            IconSearchCodeInfoPanelPosNoLogin.X = Convert.ToInt32(coordinate.GetType().GetProperty("NLOriginX").GetValue(coordinate));
            IconSearchCodeInfoPanelPosNoLogin.Y = Convert.ToInt32(coordinate.GetType().GetProperty("NLOriginY").GetValue(coordinate));

            if (IconSearchCodeInfoPanelPosNoLogin.X + IconSearchCodeInfoPanelAreaNoLogin.X > StartPositionDragFloorNoLogin.X + FloorDrawingPositionNoLogin.X)
            {
                IconSearchCodeInfoPanelPosNoLogin.X = IconSearchCodeInfoPanelPosNoLogin.X - IconSearchCodeInfoPanelAreaNoLogin.X;
            }
            else
            {
                IconSearchCodeInfoPanelPosNoLogin.X = IconSearchCodeInfoPanelPosNoLogin.X + PanelLeftOffset;
            }

            if (IconSearchCodeInfoPanelPosNoLogin.Y + IconSearchCodeInfoPanelAreaNoLogin.Y > StartPositionDragFloorNoLogin.Y + FloorDrawingPositionNoLogin.Y)
            {
                IconSearchCodeInfoPanelPosNoLogin.Y = IconSearchCodeInfoPanelPosNoLogin.Y - IconSearchCodeInfoPanelAreaNoLogin.Y
                + PanelTopOffset;
            }

            stpIconSearCodeInfoNoLogin.SetValue(Canvas.LeftProperty, IconSearchCodeInfoPanelPosNoLogin.X);
            stpIconSearCodeInfoNoLogin.SetValue(Canvas.TopProperty, IconSearchCodeInfoPanelPosNoLogin.Y);
        }

        /// <summary>
        /// 移动EPS和灯具图标对应的信息面板
        /// </summary>      
        private void MoveIconSearchCodeInfoPanelLogin(object infoDisBoxOrLight)
        {
            if (infoDisBoxOrLight is DistributionBoxInfo || infoDisBoxOrLight is LightInfo || infoDisBoxOrLight is BlankIconInfo)
            {
                CoordinateInfo infoCoordinate = new CoordinateInfo();
                int ID = (int)infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight);
                if (infoDisBoxOrLight is DistributionBoxInfo)
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                }
                else if (infoDisBoxOrLight is LightInfo)
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == ID);
                }
                else
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.BlankIcon.ToString() && x.TableID == ID);
                }

                IconSearchCodeInfoPanelPosLogin.X = infoCoordinate.TransformX;
                IconSearchCodeInfoPanelPosLogin.Y = infoCoordinate.TransformY;
            }
            else
            {
                IconSearchCodeInfoPanelPosLogin.X = BlankIcon.Margin.Left;
                IconSearchCodeInfoPanelPosLogin.Y = BlankIcon.Margin.Top;
            }

            if (IconSearchCodeInfoPanelPosLogin.X + IconSearchCodeInfoPanelAreaLogin.X > StartPositionDragFloor.X + FloorDrawingPosition.X)
            {
                IconSearchCodeInfoPanelPosLogin.X = IconSearchCodeInfoPanelPosLogin.X - IconSearchCodeInfoPanelAreaLogin.X
                + PanelRightOffset;
            }
            else
            {
                IconSearchCodeInfoPanelPosLogin.X = IconSearchCodeInfoPanelPosLogin.X + PanelLeftOffset;//PanelLeftOffset
            }

            if (IconSearchCodeInfoPanelPosLogin.Y + IconSearchCodeInfoPanelAreaLogin.Y > StartPositionDragFloor.Y + FloorDrawingPosition.Y)
            {
                IconSearchCodeInfoPanelPosLogin.Y = IconSearchCodeInfoPanelPosLogin.Y - IconSearchCodeInfoPanelAreaLogin.Y
                + PanelTopOffset;
            }

            stpIconSearCodeInfoLogin.SetValue(Canvas.LeftProperty, IconSearchCodeInfoPanelPosLogin.X);
            stpIconSearCodeInfoLogin.SetValue(Canvas.TopProperty, IconSearchCodeInfoPanelPosLogin.Y);
        }

        /// <summary>
        /// 移动报警点图标对应的信息面板
        /// </summary>
        private void MovePartitionInfoPanelLogin(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            //PartitionPointInfoPanelPosLogin报警点信息面板的坐标    PartitionPointInfoPanelAreaLogin报警点信息面板的高度宽度
            //StartPositionDragFloor记录配电箱或者灯具拖拽的起始坐标   FloorDrawingPosition图纸的高度宽度
            CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
            PartitionPointInfoPanelPosLogin.X = infoCoordinate.TransformX;
            PartitionPointInfoPanelPosLogin.Y = infoCoordinate.TransformY;
            if (PartitionPointInfoPanelPosLogin.X + PartitionPointInfoPanelAreaLogin.X > StartPositionDragFloor.X + FloorDrawingPosition.X)
            {
                PartitionPointInfoPanelPosLogin.X -= PartitionPointInfoPanelAreaLogin.X;
            }
            if (PartitionPointInfoPanelPosLogin.Y + PartitionPointInfoPanelAreaLogin.Y > StartPositionDragFloor.Y + FloorDrawingPosition.Y)
            {
                PartitionPointInfoPanelPosLogin.Y -= PartitionPointInfoPanelAreaLogin.Y;
            }
            stpPartitionPointInfoLogin.SetValue(Canvas.LeftProperty, PartitionPointInfoPanelPosLogin.X);
            stpPartitionPointInfoLogin.SetValue(Canvas.TopProperty, PartitionPointInfoPanelPosLogin.Y);
        }

        /// <summary>
        /// 根据配电箱或者灯具类型返回对应的图标
        /// </summary>      
        private Uri GetImageIconSearchCodeByList(object infoDisBoxOrLight)
        {
            Uri BitmapImageBaseUri = null;
            bool isDistributionBoxInfo = infoDisBoxOrLight is DistributionBoxInfo;
            if (isDistributionBoxInfo)
            {
                BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxOpen.png", UriKind.Relative);
            }
            else
            {
                LightClass = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("LightClass").GetValue(infoDisBoxOrLight));
                switch (LightClass)
                {
                    case (int)EnumClass.LightClass.照明灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\FloodLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.双向标志灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\DoubleMarkerLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.双头灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.双向地埋灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\DoubleBuriedLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.安全出口灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\ExitLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.楼层灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\FloorLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.单向标志灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\MarkerLightOpen.png", UriKind.Relative);
                        break;
                    case (int)EnumClass.LightClass.单向地埋灯:
                        BitmapImageBaseUri = new Uri("\\Pictures\\BuriedLightOpen.png", UriKind.Relative);
                        break;
                }
            }
            return BitmapImageBaseUri;
        }

        /// <summary>
        /// 根据配电箱或者灯具类型返回对应的Uri
        /// </summary>       
        private Uri GetImageUriIconSearchCode(object infoDisBoxOrLight)
        {
            try
            {
                Uri BitmapImageBaseUri = null;
                int status, isEmergency, shield;

                bool isDistributionBoxInfo = infoDisBoxOrLight is DistributionBoxInfo;
                if (isDistributionBoxInfo)
                {
                    status = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("Status").GetValue(infoDisBoxOrLight));
                    isEmergency = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("IsEmergency").GetValue(infoDisBoxOrLight));
                    shield = Convert.ToInt32(infoDisBoxOrLight?.GetType().GetProperty("Shield").GetValue(infoDisBoxOrLight));
                    int convert = status & 0X07FC;
                    if (isEmergency == 1)
                    {
                        BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxEmergency.png", UriKind.Relative);
                    }
                    else
                    {
                        if (convert != 0 && shield == 0)
                        {
                            BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxFault.png", UriKind.Relative);
                        }
                        else
                        {
                            BitmapImageBaseUri = isEmergency == 1
                                ? new Uri("\\Pictures\\DisBoxEmergency.png", UriKind.Relative)
                                : new Uri("\\Pictures\\DisBoxOpen.png", UriKind.Relative);
                        }
                    }

                    convert = status & 0XF000;
                    if (convert != 0 && (convert & 0X2000) != 0X2000)
                    {
                        BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxEmergency.png", UriKind.Relative);
                    }
                }
                else if (infoDisBoxOrLight is LightInfo)
                {
                    bool leftOpen = false;
                    bool rightOpen = false;
                    double rtnDirection = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("RtnDirection").GetValue(infoDisBoxOrLight));
                    status = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("Status").GetValue(infoDisBoxOrLight));
                    isEmergency = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("IsEmergency").GetValue(infoDisBoxOrLight));
                    LightClass = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("LightClass").GetValue(infoDisBoxOrLight));
                    shield = Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("Shield").GetValue(infoDisBoxOrLight));
                    int convert = status & 0X07;

                    if ((status & (int)EnumClass.LightFaultClass.光源故障) != 0 || (status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (status & (int)EnumClass.LightFaultClass.电池故障) != 0)
                    {
                        DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == infoDisBoxOrLight.GetType().GetProperty("DisBoxID").GetValue(infoDisBoxOrLight).ToString());
                        if (((infoDistributionBox.Status & 0X07FC) & 0X07FC) == 0X07FC && infoDistributionBox.Shield == 1)
                        {
                            switch (LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Extinguish.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Extinguish.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    if (rtnDirection == 180)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright-R.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright.png", UriKind.Relative);
                                    }
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Bright.png", UriKind.Relative);
                                    break;
                            }
                        }
                        else
                        {
                            if (shield == 0)
                            {
                                switch (LightClass)
                                {
                                    case (int)EnumClass.LightClass.照明灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双向标志灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双头灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双向地埋灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.安全出口灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.楼层灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Fault.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.单向标志灯:
                                        if (rtnDirection == 180)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Fault-R.png", UriKind.Relative);
                                        }
                                        else
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Fault.png", UriKind.Relative);
                                        }
                                        break;
                                    case (int)EnumClass.LightClass.单向地埋灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Fault.png", UriKind.Relative);
                                        break;
                                }
                            }
                            else
                            {
                                switch (LightClass)
                                {
                                    case (int)EnumClass.LightClass.照明灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Extinguish.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双向标志灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双头灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Extinguish.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.双向地埋灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.安全出口灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.楼层灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative);
                                        break;
                                    case (int)EnumClass.LightClass.单向标志灯:
                                        if (rtnDirection == 180)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright-R.png", UriKind.Relative);
                                        }
                                        else
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright.png", UriKind.Relative);
                                        }
                                        break;
                                    case (int)EnumClass.LightClass.单向地埋灯:
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Bright.png", UriKind.Relative);
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isEmergency == 1)
                        {
                            switch (LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Emergency.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Emergency.png", UriKind.Relative);
                                    //if ((convert & 0X04) == 0X04)
                                    //{
                                    //    leftOpen = true;
                                    //}
                                    //if ((convert & 0X01) == 0X01)
                                    //{
                                    //    rightOpen = true;
                                    //}
                                    //if (leftOpen && rightOpen)
                                    //{
                                    //    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Emergency.png", UriKind.Relative);
                                    //}
                                    //else
                                    //{
                                    //    if (leftOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Emergency-L.png", UriKind.Relative);
                                    //    }
                                    //    if (rightOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Emergency-R.png", UriKind.Relative);
                                    //    }
                                    //    if (!leftOpen && !rightOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Extinguish.png", UriKind.Relative);
                                    //    }

                                    //}
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Emergency.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    if ((convert & 0X04) == 0X04)
                                    {
                                        leftOpen = true;
                                    }
                                    if ((convert & 0X01) == 0X01)
                                    {
                                        rightOpen = true;
                                    }
                                    if (leftOpen && rightOpen)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Emergency.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        if (leftOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Emergency-L.png", UriKind.Relative);
                                        }
                                        if (rightOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Emergency-R.png", UriKind.Relative);
                                        }
                                        if (!leftOpen && !rightOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Extinguish.png", UriKind.Relative);
                                        }

                                    }
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Emergency.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Emergency.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    if (rtnDirection == 180)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Emergency-R.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Emergency.png", UriKind.Relative);
                                    }
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Emergency.png", UriKind.Relative);
                                    break;
                            }
                        }
                        else if (shield == 1)
                        {
                            switch (LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Extinguish.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Extinguish.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    if (rtnDirection == 180)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright-R.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright.png", UriKind.Relative);
                                    }
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Bright.png", UriKind.Relative);
                                    break;
                            }
                        }
                        else
                        {
                            switch (LightClass)
                            {
                                case (int)EnumClass.LightClass.照明灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向标志灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                                    //if ((convert & 0X04) == 0X04)
                                    //{
                                    //    leftOpen = true;
                                    //}
                                    //if ((convert & 0X01) == 0X01)
                                    //{
                                    //    rightOpen = true;
                                    //}
                                    //if (leftOpen && rightOpen)
                                    //{
                                    //    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                                    //}
                                    //else
                                    //{
                                    //    if (leftOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright-L.png", UriKind.Relative);
                                    //    }
                                    //    if (rightOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright-R.png", UriKind.Relative);
                                    //    }
                                    //    if (!leftOpen && !rightOpen)
                                    //    {
                                    //        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Extinguish.png", UriKind.Relative);
                                    //    }
                                    //}
                                    break;
                                case (int)EnumClass.LightClass.双头灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.双向地埋灯:
                                    if ((convert & 0X04) == 0X04)
                                    {
                                        leftOpen = true;
                                    }
                                    if ((convert & 0X01) == 0X01)
                                    {
                                        rightOpen = true;
                                    }
                                    if (leftOpen && rightOpen)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        if (leftOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright-L.png", UriKind.Relative);
                                        }
                                        if (rightOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright-R.png", UriKind.Relative);
                                        }
                                        if (!leftOpen && !rightOpen)
                                        {
                                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Extinguish.png", UriKind.Relative);
                                        }
                                    }
                                    break;
                                case (int)EnumClass.LightClass.安全出口灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.楼层灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative);
                                    break;
                                case (int)EnumClass.LightClass.单向标志灯:
                                    if (rtnDirection == 180)
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright-R.png", UriKind.Relative);
                                    }
                                    else
                                    {
                                        BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright.png", UriKind.Relative);
                                    }
                                    break;
                                case (int)EnumClass.LightClass.单向地埋灯:
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Bright.png", UriKind.Relative);
                                    break;
                            }
                        }
                    }

                    if ((status & (int)EnumClass.LightFaultClass.光源故障) == 0 && (status & (int)EnumClass.LightFaultClass.通信故障) == 0 && (status & (int)EnumClass.LightFaultClass.电池故障) == 0 && convert == 0 && isEmergency == 0)
                    {
                        switch (LightClass)
                        {
                            case (int)EnumClass.LightClass.照明灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.双向标志灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.双头灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.双向地埋灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.安全出口灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.楼层灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Extinguish.png", UriKind.Relative);
                                break;
                            case (int)EnumClass.LightClass.单向标志灯:
                                if (rtnDirection == 180)
                                {
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Extinguish-R.png", UriKind.Relative);
                                }
                                else
                                {
                                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Extinguish.png", UriKind.Relative);
                                }
                                break;
                            case (int)EnumClass.LightClass.单向地埋灯:
                                BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Extinguish.png", UriKind.Relative);
                                break;
                        }
                    }
                }
                else
                {
                    //string type = infoDisBoxOrLight.GetType().GetProperty("Type").GetValue(infoDisBoxOrLight).ToString();
                    switch (infoDisBoxOrLight.GetType().GetProperty("Type").GetValue(infoDisBoxOrLight).ToString())
                    {
                        case "照明灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Fault.png", UriKind.Relative);
                            break;
                        case "双向标志灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Fault.png", UriKind.Relative);
                            break;
                        case "双头灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Fault.png", UriKind.Relative);
                            break;
                        case "双向地埋灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Fault.png", UriKind.Relative);
                            break;
                        case "安全出口灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Fault.png", UriKind.Relative);
                            break;
                        case "楼层灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Fault.png", UriKind.Relative);
                            break;
                        case "单向标志灯":
                            if (Convert.ToInt32(infoDisBoxOrLight.GetType().GetProperty("RtnDirection").GetValue(infoDisBoxOrLight)) == 180)
                            {
                                BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Fault-R.png", UriKind.Relative);
                            }
                            else
                            {
                                BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Fault.png", UriKind.Relative);
                            }
                            break;
                        case "单向地埋灯":
                            BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Fault.png", UriKind.Relative);
                            break;
                    }
                }
                return BitmapImageBaseUri;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取配电箱或者灯具列表图标
        /// </summary>      
        private IconSearchCode GetIconSearchCodeByListLogin(object infoDisBoxOrLight)
        {
            IconSearchCode IconSearchCode = new IconSearchCode
            {
                Tag = infoDisBoxOrLight
            };

            IconSearchCode.labIconSearchCode.Content = infoDisBoxOrLight.GetType().GetProperty("Code").GetValue(infoDisBoxOrLight);
            IconSearchCode.imgIconSearchCode.Source = new BitmapImage(GetImageIconSearchCodeByList(infoDisBoxOrLight));
            return IconSearchCode;
        }

        private IconSearchCode GetIconSearchCodeByListLogin(string infoDisBoxOrLight)
        {
            IconSearchCode iconSearchCode = new IconSearchCode
            {
                Tag = infoDisBoxOrLight
            };
            iconSearchCode.labIconSearchCode.Content = infoDisBoxOrLight;
            iconSearchCode.imgIconSearchCode.Source = new BitmapImage(IconSource(infoDisBoxOrLight));
            return iconSearchCode;
        }

        private Uri IconSource(string infoDisBoxOrLight)
        {
            Uri BitmapImageBaseUri = null;
            switch (infoDisBoxOrLight)
            {
                case "配电箱":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxOpen.png", UriKind.Relative);
                    break;
                case "照明灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\FloodLightOpen.png", UriKind.Relative);
                    break;
                case "双头灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLightOpen.png", UriKind.Relative);
                    break;
                case "楼层指示":
                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorLightOpen.png", UriKind.Relative);
                    break;
                case "安全出口":
                    BitmapImageBaseUri = new Uri("\\Pictures\\ExitLightOpen.png", UriKind.Relative);
                    break;
                case "单向左向":
                    BitmapImageBaseUri = new Uri("\\Pictures\\MarkerLightOpen.png", UriKind.Relative);
                    break;
                case "单向右向":
                    BitmapImageBaseUri = new Uri("\\Pictures\\MarkerLightOpen-R.png", UriKind.Relative);
                    break;
                case "双向标志灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleMarkerLightOpen.png", UriKind.Relative);
                    break;
                case "单向地埋灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\BuriedLightOpen.png", UriKind.Relative);
                    break;
                case "双向地埋灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleBuriedLightOpen.png", UriKind.Relative);
                    break;
            }

            return BitmapImageBaseUri;
        }

        private Uri IconSourceLayer(string infoDisBoxOrLight)
        {
            Uri BitmapImageBaseUri = null;
            switch (infoDisBoxOrLight)
            {
                case "配电箱":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DisBoxOpen.png", UriKind.Relative);
                    break;
                case "照明灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\Floodlight-Bright.png", UriKind.Relative);
                    break;
                case "双头灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\DoubleHeadLamp-Bright.png", UriKind.Relative);
                    break;
                case "楼层指示":
                    BitmapImageBaseUri = new Uri("\\Pictures\\FloorIndication-Bright.png", UriKind.Relative);
                    break;
                case "安全出口":
                    BitmapImageBaseUri = new Uri("\\Pictures\\EXIT-Bright.png", UriKind.Relative);
                    break;
                case "单向左向":
                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright.png", UriKind.Relative);
                    break;
                case "单向右向":
                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWaySignLamp-Bright-R.png", UriKind.Relative);
                    break;
                case "双向标志灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWaySignLamp-Bright.png", UriKind.Relative);
                    break;
                case "单向地埋灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\OneWayBuriedLamp-Bright.png", UriKind.Relative);
                    break;
                case "双向地埋灯":
                    BitmapImageBaseUri = new Uri("\\Pictures\\TwoWayBuriedLamp-Bright.png", UriKind.Relative);
                    break;
            }

            return BitmapImageBaseUri;
        }

        /// <summary>
        /// 获取图形界面的图标对象(未登录)
        /// </summary>
        /// <param name="infoDisBoxOrLight"></param>
        /// <returns></returns>
        private Image GetImageNoLogin(object infoDisBoxOrLight)
        {
            if (infoDisBoxOrLight as DistributionBoxInfo != null || infoDisBoxOrLight as LightInfo != null || infoDisBoxOrLight as BlankIconInfo != null)
            {
                LayerImageTag layerimagetag = new LayerImageTag
                {
                    equipment = infoDisBoxOrLight,
                    status = infoDisBoxOrLight as BlankIconInfo != null ? 0 : (int)infoDisBoxOrLight.GetType().GetProperty("Status").GetValue(infoDisBoxOrLight)
                };
                Uri uri = GetImageUriIconSearchCode(infoDisBoxOrLight);
                Image image = new Image
                {
                    Tag = layerimagetag,
                    Width = IconSearchCodeSizeNoLogin,
                    Height = IconSearchCodeSizeNoLogin,
                    Source = new BitmapImage(uri)
                };



                        


                CoordinateInfo infoCoordinate = new CoordinateInfo();
                int ID = (int)infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight);
                if (infoDisBoxOrLight is LightInfo || infoDisBoxOrLight is BlankIconInfo)
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == (infoDisBoxOrLight is LightInfo ? EnumClass.TableName.Light.ToString() : EnumClass.TableName.BlankIcon.ToString()) && x.TableID == ID);
                    double rotate = (double)infoDisBoxOrLight.GetType().GetProperty("RtnDirection").GetValue(infoDisBoxOrLight);
                    if (rotate != 180)
                    {
                        RotateTransform rotateTransform = new RotateTransform(rotate);
                        image.RenderTransformOrigin = new Point(0.5, 0.5);
                        image.RenderTransform = rotateTransform;
                    }
                }
                else
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                }
                image.Margin = new Thickness(infoCoordinate.NLOriginX, infoCoordinate.NLOriginY, 0, 0);

                image.MouseEnter += imgIconSearchCodeNoLogin_MouseEnter;
                image.MouseLeave += imgIconSearchCodeNoLogin_MouseLeave;
                image.TouchDown += imgIconSearchCodeNoLogin_TouchDown;

                return image;
            }
            return null;
        }

        private Image GetImageLogin(object infoDisBoxOrLight)
        {
            if (infoDisBoxOrLight as DistributionBoxInfo != null || infoDisBoxOrLight as LightInfo != null || infoDisBoxOrLight as BlankIconInfo != null)
            {
                LayerImageTag layerimagetag = new LayerImageTag
                {
                    equipment = infoDisBoxOrLight,
                    status = infoDisBoxOrLight as BlankIconInfo != null ? 0 : (int)infoDisBoxOrLight.GetType().GetProperty("Status").GetValue(infoDisBoxOrLight)
                };
                Uri uri = GetImageUriIconSearchCode(infoDisBoxOrLight);
                Image image = new Image
                {
                    Tag = layerimagetag,
                    Width = IconSearchCodeSizeLogin,
                    Height = IconSearchCodeSizeLogin,
                    Source = new BitmapImage(uri)
                };


                // 创建闪烁动画
                Storyboard blinkAnimation = new Storyboard();
                ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames();
                animation.Duration = TimeSpan.FromSeconds(1);
                animation.RepeatBehavior = RepeatBehavior.Forever;
                DiscreteObjectKeyFrame visibleKeyFrame = new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
                DiscreteObjectKeyFrame hiddenKeyFrame = new DiscreteObjectKeyFrame(Visibility.Hidden, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)));
                animation.KeyFrames.Add(visibleKeyFrame);
                animation.KeyFrames.Add(hiddenKeyFrame);


                // 启动闪烁动画
                Storyboard.SetTarget(animation, image);
                Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.VisibilityProperty));
                blinkAnimation.Children.Add(animation);

                CoordinateInfo infoCoordinate = new CoordinateInfo();
                int ID = (int)infoDisBoxOrLight.GetType().GetProperty("ID").GetValue(infoDisBoxOrLight);
                if (infoDisBoxOrLight is LightInfo || infoDisBoxOrLight is BlankIconInfo)
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == (infoDisBoxOrLight is LightInfo ? EnumClass.TableName.Light.ToString() : EnumClass.TableName.BlankIcon.ToString()) && x.TableID == ID);
                    double rotate = (double)infoDisBoxOrLight.GetType().GetProperty("RtnDirection").GetValue(infoDisBoxOrLight);
                    if (rotate != 180)
                    {
                        RotateTransform rotateTransform = new RotateTransform(rotate);
                        image.RenderTransformOrigin = new Point(0.5, 0.5);
                        image.RenderTransform = rotateTransform;
                    }
                }
                else
                {
                    infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == ID);
                }
                image.Margin = new Thickness(infoCoordinate.TransformX, infoCoordinate.TransformY, 0, 0);
                image.MouseDown += imgIconSearchCodeLogin_MouseDown;
                image.MouseMove += imgIconSearchCodeLogin_MouseMove;
                image.TouchMove += imgIconSearchCodeLogin_TouchMove;

                return image;
            }
            return null;
        }

        private Image GetImageIcon(string infoDisboxOrLight)
        {
            LayerImageTag layerimagetag = new LayerImageTag
            {
                equipment = infoDisboxOrLight
            };
            if (infoDisboxOrLight == "配电箱" || infoDisboxOrLight == "照明灯" || infoDisboxOrLight == "双头灯")
            {
                layerimagetag.status = 0;
            }
            else
            {
                layerimagetag.status = 2;
            }
            Uri uri = IconSourceLayer(infoDisboxOrLight);
            Image image = new Image
            {
                Tag = layerimagetag,
                Width = IconSearchCodeSizeLogin,
                Height = IconSearchCodeSizeLogin,
                Source = new BitmapImage(uri),
                Margin = new Thickness((double)BlankIcon.Margin.Left, (double)BlankIcon.Margin.Top, 0, 0)
            };
            image.MouseDown += imgIconSearchCodeLogin_MouseDown;
            image.MouseMove += imgIconSearchCodeLogin_MouseMove;
            image.TouchMove += imgIconSearchCodeLogin_TouchMove;

            return image;
        }

        /// <summary>
        /// 获取报警点图形界面图标
        /// </summary>      
        private PartitionPoint GetPartitionPointLogin(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            PartitionPoint PartitionPoint = new PartitionPoint();
            CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
            PartitionPoint.imgPartitionPoint.Tag = infoPlanPartitionPointRecord;
            PartitionPoint.Width = PartitionPoint.Height = IconSearchCodeSizeLogin;
            PartitionPoint.SetValue(Canvas.LeftProperty, infoCoordinate.TransformX);
            PartitionPoint.SetValue(Canvas.TopProperty, infoCoordinate.TransformY);
            PartitionPoint.imgPartitionPoint.MouseDown += imgPartitionPoint_MouseDown;
            PartitionPoint.imgPartitionPoint.MouseMove += imgPartitionPoint_MouseMove;//鼠标移动
            PartitionPoint.imgPartitionPoint.TouchMove += imgPartitionPoint_TouchMove;//手指触摸移动
            return PartitionPoint;
        }

        /// <summary>
        /// 获取火灾联动点图形界面图标(未登录)
        /// </summary>
        private FireAlarmLinkPoint GetPartitionPointNoLogin(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            FireAlarmLinkPoint FireAlarmLinkPoint = new FireAlarmLinkPoint();
            CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && x.TableID == infoPlanPartitionPointRecord.ID);
            FireAlarmLinkPoint.Tag = infoPlanPartitionPointRecord;
            //FireAlarmLinkPoint.Width = FireAlarmLinkPoint.Height = IconSearchCodeSizeNoLogin;
            //FireAlarmLinkPoint.Margin = new Thickness(infoPlanPartitionPointRecord.NLOriginX, infoPlanPartitionPointRecord.NLOriginY, 0, 0);
            FireAlarmLinkPoint.SetValue(Canvas.LeftProperty, infoCoordinate.NLOriginX);
            FireAlarmLinkPoint.SetValue(Canvas.TopProperty, infoCoordinate.NLOriginY);
            return FireAlarmLinkPoint;
        }

        /// <summary>
        /// 获取点击选中的列表图标
        /// </summary>       
        private void GetSelectIconSearchCode(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Controls.ListView ListView = sender as System.Windows.Controls.ListView;
                SelectIconSearchCode = ListView.SelectedItem;
                if (SelectIconSearchCode != null)
                {
                    DragDrop.DoDragDrop(ListView, SelectIconSearchCode, System.Windows.DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// 获取点击选中的列表图标
        /// </summary>       
        private void GetSelectIconSearchCode(object sender)
        {
            System.Windows.Controls.ListView ListView = sender as System.Windows.Controls.ListView;
            SelectIconSearchCode = ListView.SelectedItem;
            if (SelectIconSearchCode != null)
            {
                DragDrop.DoDragDrop(ListView, SelectIconSearchCode, System.Windows.DragDropEffects.Move);
            }
        }

        private void StopRefreshProgressBarValueTimer()
        {
            RefreshProgressBarValueTimer.Enabled = false;
        }

        /// <summary>
        /// 刷新复位进度条值
        /// </summary>
        private void RefreshProgressBarValue()
        {
            while (CurrentProgressBarValue < ResettingProgressBar.Maximum)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(ExeInstructSleepTime);

                CurrentProgressBarValue++;
                ResettingProgressBar.Value = CurrentProgressBarValue;
                IsSimulationLinkage = false;
            }
        }

        private void RefreshProgressBarValueTimer_Tick(object sender, EventArgs e)
        {
            AllMainEle();
            GetEdition(IsCommodity);
            StopRefreshProgressBarValueTimer();
            RefreshProgressBarValue();

            AllEmergencyTotalTimer.Enabled = false;


            CurrentProgressBarValue = 0.0;
            RefreshProgressBarValueTimer.Enabled = true;
            ResettingProgressBar.Value = CurrentProgressBarValue;
        }


        private void SignIn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SignIn.Source = new BitmapImage(new Uri("\\Pictures\\SignIn-Unchecked.png", UriKind.Relative));
            Login();
            //btnAdvancedSettings_Click(null, null);
        }

        private void LoginInLayer_Click(object sender, RoutedEventArgs e)
        {
            Login();
            //List<int> LstIndex = new List<int>();
            //LstIndex.Add(1);
            //StartRealFireAlarmLink(LstIndex);
        }

        private void Login()
        {
            bool isSuccess = CheckIsRealFireAlarmLinkNow();
            if (isSuccess)
            {
                //this.FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                //this.Homepage.Visibility = System.Windows.Visibility.Visible;
                isSuccess = ShowLogin();
                if (isSuccess)
                {
                    ShowMainWindow();
                    AddHistoricalEvent("登录系统");

                    SetIsLogin(true);
                    SetIsOpenLayerModeNoLogin(false);
                    ClearAllElementNoLogin();
                    stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    if (stpLayerModeNoLogin.Visibility == System.Windows.Visibility.Hidden)
                    {
                        MasterController.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FocusReturn = sender as Image;
            FocusReturn.Source = new BitmapImage(new Uri("\\Pictures\\PageReturn-Selected.png", UriKind.Relative));
        }

        private void Return_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FocusReturn.Source = new BitmapImage(new Uri("\\Pictures\\PageReturn.png", UriKind.Relative));
            if (FocusReturn != null && !IsRealFireAlarmLink)
            {
                string tag = (sender as Image).Tag.ToString();
                switch (tag)
                {
                    case "消音设置":
                        Silencing.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "密码修改":
                        PassworkModify.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "月检设置":
                        MonthlyInspection.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "年检设置":
                        YearlyInspection.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "EPS数据":
                        EPSData.Visibility = System.Windows.Visibility.Hidden;
                        ClearSelect();
                        break;
                    case "EPS汇总":
                        EPSCollect.Visibility = System.Windows.Visibility.Hidden;
                        ClearSelect();
                        break;
                    case "单灯控制":
                        SingleLampControl.Visibility = System.Windows.Visibility.Hidden;
                        ClearSelect();
                        break;
                    case "初始化":
                        Initialization.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "历史记录":
                        History.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "联动":
                        Linkage.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "灯具汇总":
                        LampCollect.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "自动检测":
                        SelfInspection.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "通讯设置":
                        stpSetCommunication.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "EPS详细页":
                        EPSDetailPageNoLogin.Visibility = System.Windows.Visibility.Hidden;
                        ClearSelect();
                        InitEPSShowNoLogin();
                        break;
                    case "图形界面":
                        stpLayerModeLogin.Visibility = System.Windows.Visibility.Hidden;
                        ClearAllElementLogin();
                        break;
                    case "未登录图形界面":
                        stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Hidden;
                        btnCloseGuiNoLogin_Click(null, null);
                        break;
                }
                if (tag == "EPS详细页" || tag == "未登录图形界面")
                {
                    MasterController.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                    Homepage.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void Silencing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SilencingMenu.Source = new BitmapImage(new Uri("\\Pictures\\SilencingSettings-Selected.png", UriKind.Relative));
        }

        private void Silencing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SilencingMenu.Source = new BitmapImage(new Uri("\\Pictures\\SilencingSettings-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = Silencing.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            GetMuteSetContent();
        }

        private void btnText_Click1(object sender, MouseButtonEventArgs e)
        {
            SetFocusTime1(sender as Label);
        }

        private void DelFocusTime(object sender, MouseButtonEventArgs e)
        {
            ClearFocusTime();
        }

        private void GetFocusPassword(object sender, RoutedEventArgs e)
        {
            GetFocusPasswordBox(sender);
        }

        private void setPassword(object sender, MouseButtonEventArgs e)
        {
            SetPassword(sender as Label);
        }

        private void DelPassword(object sender, MouseButtonEventArgs e)
        {
            if (FocusPasswordBox != null)
            {
                FocusPasswordBox.Clear();
            }
        }

        public void DetermineModify(object sender, MouseButtonEventArgs e)
        {
            if (OldPassword.Password == string.Empty)
            {
                OldPassword.Focus();
                MessageBox.Show("请输入原始密码！", "提示");
                OldPassword.Focus();

            }
            else if (NewPassword.Password == string.Empty)
            {
                NewPassword.Focus();
                MessageBox.Show("请输入新密码！", "提示");
                NewPassword.Focus();
            }
            else
            {
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "UserPassWord");
                if (infoGblSetting.SetValue != CommonFunct.Md5Encrypt(OldPassword.Password))
                {
                    MessageBox.Show("原始密码输入不正确，请重新输入！", "提示");
                    OldPassword.Focus();
                    //FocusPasswordBox = sender as PasswordBox;
                    return;
                }

                infoGblSetting.SetValue = CommonFunct.Md5Encrypt(NewPassword.Password);
                ObjGblSetting.Update(infoGblSetting);
                MessageBox.Show("密码修改成功！", "提示");
            }
        }

        private void CommunicationSetting_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommunicationSetting.Source = new BitmapImage(new Uri("\\Pictures\\TimeSettings-Selected.png", UriKind.Relative));
        }

        private void CommunicationSetting_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CommunicationSetting.Source = new BitmapImage(new Uri("\\Pictures\\TimeSettings-Unchecked.png", UriKind.Relative));
            SetComSerialPortData();
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = stpSetCommunication.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void PasswordModification_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PasswordModification.Source = new BitmapImage(new Uri("\\Pictures\\PasswordSettings-Selected.png", UriKind.Relative));
        }

        private void PasswordModification_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OldPassword.Clear();
            NewPassword.Clear();
            PasswordModification.Source = new BitmapImage(new Uri("\\Pictures\\PasswordSettings-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = PassworkModify.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void IsMonthlyInspection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsMonthlyInspection");
            BitmapImage BitmapImage = (sender as Image).Source as BitmapImage;
            if (BitmapImage.UriSource == new Uri("\\Pictures\\Unselected.png", UriKind.Relative))
            {
                (sender as Image).Tag = "Selected";
                (sender as Image).Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                //IsMonthlyInspection = true;
                infoGblSetting.SetValue = "true";
                ExecuteMonthCheckTimer.Enabled = true;//启动自动月检计时
                MonthOrSeasonCheckFault.Enabled = false;//关闭手动月检计时
            }
            else
            {
                (sender as Image).Tag = "Unselected";
                (sender as Image).Source = new BitmapImage(new Uri("\\Pictures\\Unselected.png", UriKind.Relative));
                //IsMonthlyInspection = false;
                infoGblSetting.SetValue = "false";
                ExecuteMonthCheckTimer.Enabled = false;//关闭自动月检计时
                MonthOrSeasonCheckFault.Enabled = true;//启动手动月检计时
            }
            ObjGblSetting.Update(infoGblSetting);
        }

        private void MonthlyInspectionOK(object sender, MouseButtonEventArgs e)
        {
            MonthlyInspectionSetOK();
        }

        private void MonthlyTesting_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MonthlyTesting.Source = new BitmapImage(new Uri("\\Pictures\\MonthCheckSettings-Selected.png", UriKind.Relative));
        }

        private void MonthlyTesting_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MonthlyTesting.Source = new BitmapImage(new Uri("\\Pictures\\MonthCheckSettings-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextMonthCheckTime");
            DateTime dtMonthNextTime = Convert.ToDateTime(infoGblSetting.SetValue);
            MonthlyDay.Text = dtMonthNextTime.Day.ToString();
            MonthlyHour.Text = dtMonthNextTime.Hour.ToString();
            MonthlyMinute.Text = dtMonthNextTime.Minute.ToString();
            MonthlySecond.Text = dtMonthNextTime.Second.ToString();
            if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsMonthlyInspection").SetValue))
            {
                IsAutomaticMonthlyInspection.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }
            else
            {
                IsAutomaticMonthlyInspection.Source = new BitmapImage(new Uri("\\Pictures\\Unselected.png", UriKind.Relative));
            }
            TimeAndState.Visibility = MonthlyInspection.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void IsSeasonlyInspection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection");
            BitmapImage BitmapImage = (sender as Image).Source as BitmapImage;
            if (BitmapImage.UriSource == new Uri("\\Pictures\\Unselected.png", UriKind.Relative))
            {
                (sender as Image).Tag = "Selected";
                (sender as Image).Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
                infoGblSetting.SetValue = "true";
                ExecuteSeasonCheckTimer.Enabled = true;//启动自动年检计时
                MonthOrSeasonCheckFault.Enabled = false;//关闭手动年检计时
            }
            else
            {
                (sender as Image).Tag = "Unselected";
                (sender as Image).Source = new BitmapImage(new Uri("\\Pictures\\Unselected.png", UriKind.Relative));
                infoGblSetting.SetValue = "false";
                ExecuteSeasonCheckTimer.Enabled = false;//关闭自动年检计时
                MonthOrSeasonCheckFault.Enabled = true;//启动手动年检计时
            }
            ObjGblSetting.Update(infoGblSetting);
        }

        private void YearlyInspectionOK(object sender, MouseButtonEventArgs e)
        {
            YearlyInspectionSetOK();
        }

        private void YearlyTeating_MouseDown(object sender, MouseButtonEventArgs e)
        {
            YearlyTeating.Source = new BitmapImage(new Uri("\\Pictures\\YearCheckSettings-Selected.png", UriKind.Relative));
        }

        private void YearlyTeating_MouseUp(object sender, MouseButtonEventArgs e)
        {
            YearlyTeating.Source = new BitmapImage(new Uri("\\Pictures\\YearCheckSettings-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
            DateTime dtSeasonNextTime = Convert.ToDateTime(infoGblSetting.SetValue);
            YearlyMonth.Text = dtSeasonNextTime.Month.ToString();
            YearlyDay.Text = dtSeasonNextTime.Day.ToString();
            YearlyHour.Text = dtSeasonNextTime.Hour.ToString();
            YearlyMinute.Text = dtSeasonNextTime.Minute.ToString();
            YearlySecond.Text = dtSeasonNextTime.Second.ToString();
            if (Convert.ToBoolean(LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection").SetValue))
            {
                IsAutomaticQuarterlyInspection.Source = new BitmapImage(new Uri("\\Pictures\\Selected.png", UriKind.Relative));
            }
            else
            {
                IsAutomaticQuarterlyInspection.Source = new BitmapImage(new Uri("\\Pictures\\Unselected.png", UriKind.Relative));
            }
            TimeAndState.Visibility = YearlyInspection.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void EPSInformation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EPSInformation.Source = new BitmapImage(new Uri("\\Pictures\\EPS-Data-Selected.png", UriKind.Relative));
        }

        private void EPSInformation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ClearEPSDataLogin();
            LstDistributionBox = ObjDistributionBox.GetAll();
            EPSListCurrentPageLogin = 1;
            EPSListTotalPageLogin = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (EPSListColumnCount * EPSListMaxRowCountLogin) + 1 : 1;

            labEPSCodeListCurrentPageLogin.Content = EPSListCurrentPageLogin;//当前页数
            labEPSCodeListTotalPageLogin.Content = EPSListTotalPageLogin;//总页数
            EPSInformation.Source = new BitmapImage(new Uri("\\Pictures\\EPS-Data-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = EPSData.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            ShowEPSListLogin();
            if (LstDistributionBox.Count != 0)
            {
                ShowEPSInfoLogin(LstDistributionBox[0]);
            }
        }

        private void EPSPool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EPSPool.Source = new BitmapImage(new Uri("\\Pictures\\EPS-Summary-Selected.png", UriKind.Relative));
        }

        private void EPSPool_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ClearEPSDataLogin();
            EPSListCurrentPageLogin = 1;
            EPSListTotalPageLogin = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (7 * 4) + 1 : 1;

            labEPSCollectCurrentPage.Content = EPSListCurrentPageLogin;//当前页数
            labEPSCollectTotalPage.Content = EPSListTotalPageLogin;//总页数
            EPSPool.Source = new BitmapImage(new Uri("\\Pictures\\EPS-Summary-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = EPSCollect.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;


            EPSCollectSum.Content = LstDistributionBox.Count;
            EPSCollectFaultSum.Content = LstDistributionBox.FindAll(x => (x.Status & 0X07FC) != (int)EnumClass.DisBoxStatusClass.正常状态).Count;
            EPSCollectLampSum.Content = LstLight.Count;
            EPSCollectLampFaultSum.Content = LstLight.FindAll(x => (x.Status & (int)EnumClass.LightFaultClass.通信故障) != 0 || (x.Status & (int)EnumClass.LightFaultClass.光源故障) != 0).Count;
            ShowEPSListLogin();
            if (LstDistributionBox.Count > 0)
            {
                ShowEPSInfoNoLogin(LstDistributionBox[0]);
            }

        }

        private void LampControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LampControl.Source = new BitmapImage(new Uri("\\Pictures\\LampControl-Selected.png", UriKind.Relative));
        }

        private void LampControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GetLightViewByDisBoxIDLogin();
            OpenSingleLightControlPage();
            ShowEPSList();
            ShowLightListLogin();

            //ClearLightDataLogin();
            LampControl.Source = new BitmapImage(new Uri("\\Pictures\\LampControl-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = SingleLampControl.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void Init_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Init.Source = new BitmapImage(new Uri("\\Pictures\\Initialization-Selected.png", UriKind.Relative));
        }

        private void Init_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Init.Source = new BitmapImage(new Uri("\\Pictures\\Initialization-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = Initialization.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            EPSAndLightStatCount();
        }

        private void Chronicle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Chronicle.Source = new BitmapImage(new Uri("\\Pictures\\Records-Selected.png", UriKind.Relative));
        }

        private void Chronicle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Chronicle.Source = new BitmapImage(new Uri("\\Pictures\\Records-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = History.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            LstHistoricalShow.Clear();
            for (int i = 0; i < LstHistoricalEvent.Count; i++)
            {
                LstHistoricalShow.Add(LstHistoricalEvent[i]);
            }
            LstHistoricalShow.Reverse();
            ShowHistoryPage();
            ShowHistoryList();
        }

        private void LinkageIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LinkageIcon.Source = new BitmapImage(new Uri("\\Pictures\\Linkage-Selected.png", UriKind.Relative));
        }

        private void LinkageIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LinkageIcon.Source = new BitmapImage(new Uri("\\Pictures\\Linkage-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = Linkage.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
            LoadFireAlarmLinkInfo();
            LoadFireAlarmTypeData();
        }

        private void LuminairesSummary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LuminairesSummary.Source = new BitmapImage(new Uri("\\Pictures\\Light-Summary-Selected.png", UriKind.Relative));
        }

        private void LuminairesSummary_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LuminairesSummary.Source = new BitmapImage(new Uri("\\Pictures\\Light-Summary-Unchecked.png", UriKind.Relative));
            GetLightViewByDisBoxIDNoLogin();
            OpenLightSummaryNoLoginPage();
            ShowLightListNoLogin();

            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = LampCollect.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void Check_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Check.Source = new BitmapImage(new Uri("\\Pictures\\SelfInspection-Selected.png", UriKind.Relative));
        }

        private void Check_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Check.Source = new BitmapImage(new Uri("\\Pictures\\SelfInspection-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            TimeAndState.Visibility = SelfInspection.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void SilencingSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SilencingSelection.Source = new BitmapImage(new Uri("\\Pictures\\Silencing-Selected.png", UriKind.Relative));
        }

        private void SilencingSelection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SilencingSelection.Source = new BitmapImage(new Uri("\\Pictures\\Silencing-Unchecked.png", UriKind.Relative));

            SetIsMute();
        }

        private void CompulsoryEmergency_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CompulsoryEmergency.Source = new BitmapImage(new Uri("\\Pictures\\Compulsory emergency-Unchecked.png", UriKind.Relative));
            Homepage.Visibility = System.Windows.Visibility.Hidden;
            CompulsoryEmergencyLogin.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
        }

        private void CompulsoryEmergency_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CompulsoryEmergency.Source = new BitmapImage(new Uri("\\Pictures\\Compulsory emergency-Selected.png", UriKind.Relative));
        }

        private void btnCancelOrReset_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "取消")
            {
                CompulsoryEmergencyLogin.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                if (IsLogin)
                {
                    Homepage.Visibility = System.Windows.Visibility.Visible;
                    //InitLayerModeNoLogin();
                }
                else
                {
                    TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
                }
                Homepage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                //MonthlyCheckTime = 0;//应急复位后月检次数清零
                IsComEmergency = false;
                AllEmergencyTotalTimer.Enabled = false;
                ResetProgress.Visibility = System.Windows.Visibility.Visible;
                ResettingProgressBar = pgbResetSystem;
                RefreshProgressBarValueTimer_Tick(sender, e);
                EmergencyLoginTime.Content = "00:00:00";
                ResetProgress.Visibility = EmergencyTip.Visibility = System.Windows.Visibility.Hidden;
                EmergencyTiming.Foreground = EmergencyLoginTime.Foreground = CommonFunct.GetBrush("#8CB3D9");

                CompulsoryEmergencyLogin.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Hidden;
                if (IsLogin)
                {
                    Homepage.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
                }
                CancelOrReset.Tag = "取消";
                CancelOrReset.Content = "取消";
                //AllMainEleTimer.Enabled = true;
            }
            LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            LstLight.ForEach(x => x.IsEmergency = 0);

            ObjDistributionBox.Save(LstDistributionBox);
            ObjLight.Save(LstLight);
        }

        private void btnDetermineEmergency_Click(object sender, RoutedEventArgs e)
        {
            AllEmergencyTotalTime = 0;
            DetEmergency();
            IndicatorLight();
        }

        private void DetEmergency()
        {
            IsComEmergency = true;
            AllMainEleTimer.Enabled = false;
            AllEmergencyTotalTimer.Enabled = true;
            CancelOrReset.Tag = "复位";
            CancelOrReset.Content = "复位";
            EmergencyTip.Visibility = System.Windows.Visibility.Visible;
            EmergencyTiming.Foreground = EmergencyLoginTime.Foreground = CommonFunct.GetBrush("#FFFFFF");
            RecordAllEmergencyInfo();
            AllEmergencySystem();
            LstDistributionBox.ForEach(x => x.IsEmergency = 1);
            LstLight.ForEach(x => x.IsEmergency = 1);
            ObjDistributionBox.Save(LstDistributionBox);
            ObjLight.Save(LstLight);
        }

        public void IndicatorLight()
        {
            if (LstFaultRecord.Count == 0)
            {
                if ((AbsFireAlarmLink.HostBoardReturnStatus & (int)EnumClass.HostBoardStatus.电池充电) != 0)
                {
                    AbsFireAlarmLink.SendHostBoardData(0X72);
                    AbsFireAlarmLink.HostBoardSendStatus = 0X72;
                }
                else
                {
                    AbsFireAlarmLink.SendHostBoardData(0X52);
                    AbsFireAlarmLink.HostBoardSendStatus = 0X52;
                }
            }
            else
            {
                if ((AbsFireAlarmLink.HostBoardReturnStatus & (int)EnumClass.HostBoardStatus.电池故障) != 0 || (AbsFireAlarmLink.HostBoardReturnStatus & (int)EnumClass.HostBoardStatus.电池短路) != 0)
                {
                    AbsFireAlarmLink.SendHostBoardData(0X46);
                    AbsFireAlarmLink.HostBoardSendStatus = 0X46;
                }
                else
                {
                    if ((AbsFireAlarmLink.HostBoardReturnStatus & (int)EnumClass.HostBoardStatus.电池充电) != 0)
                    {
                        AbsFireAlarmLink.SendHostBoardData(0X76);
                        AbsFireAlarmLink.HostBoardSendStatus = 0X76;
                    }
                    else
                    {
                        AbsFireAlarmLink.SendHostBoardData(0X56);
                        AbsFireAlarmLink.HostBoardSendStatus = 0X56;
                    }
                }
            }
        }

        private void Emergency_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Emergency.Source = new BitmapImage(new Uri("\\Pictures\\Compulsory emergency-Selected.png", UriKind.Relative));
        }

        private void Emergency_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Emergency.Source = new BitmapImage(new Uri("\\Pictures\\Compulsory emergency-Unchecked.png", UriKind.Relative));
            AllEmergencyNoLogin();
        }

        private void AllEmergencyNoLogin()
        {
            bool isSuccess = CheckIsRealFireAlarmLinkNow();
            if (isSuccess)
            {
                isSuccess = OpenLoginEmergencyPage();
                if (isSuccess)
                {
                    if (stpLayerModeNoLogin.Visibility == System.Windows.Visibility.Visible)
                    {
                        LstDistributionBox.ForEach(x => x.IsEmergency = 1);
                        LstLight.ForEach(x => x.IsEmergency = 1);
                        ObjDistributionBox.Save(LstDistributionBox);
                        ObjLight.Save(LstLight);
                        RefreshIconSearchCodeNoLogin();
                    }
                    else
                    {
                        ClearAllElementNoLogin();
                        MasterController.Visibility = System.Windows.Visibility.Hidden;
                        OpenAllEmergencyPage();
                    }
                    //IndicatorLight();
                    //SetAllEmergencyPage(true);
                    //AllEmergencyTotalTime = 0;
                    //SetAllMainEleTimer(false);
                    //AllMainEleTimer.Enabled = false;
                    //AllEmergencyTotalTimer.Enabled = true;
                    //RecordAllEmergencyInfo();
                    //AllEmergencySystem();
                }
                else
                {
                    TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
                    //OpenRecordLogPage();
                    //InitLayerModeNoLogin();
                }
            }
        }

        private void SetAllEmergencyPage(bool v)
        {
            IsComEmergency = v;
        }

        private void SignIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SignIn.Source = new BitmapImage(new Uri("\\Pictures\\SignIn-Selected.png", UriKind.Relative));
        }

        private void ResetNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetNoLogin.Source = new BitmapImage(new Uri("\\Pictures\\Reset-Selected.png", UriKind.Relative));
        }

        private void ResetNoLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ResetNoLogin.Source = new BitmapImage(new Uri("\\Pictures\\Reset-Unchecked.png", UriKind.Relative));
            RSNoLogin();
        }

        /// <summary>
        /// 未登录时复位按钮功能
        /// </summary>
        private void RSNoLogin()
        {
            bool isSuccess = OpenLoginResetSystemPage();
            if (isSuccess)
            {
                ClearAllElementNoLogin();
                SimulateFireAlarmLinkExePlanTimer.Stop();
                SetSimulateFireAlarmLinkExePlanTimer(false);
                ResetSystemNoLogin();
                btnAllEmergencyInLayer.IsEnabled = LoginInLayer.IsEnabled = true;
                AbsFireAlarmLink.LstOldZoneNumber.Clear();
                FireAlarmLinkInterface.LstOldNumber.Clear();
            }
            else
            {
                if (IsRealFireAlarmLink)
                {
                    RestoreAllIcon();
                    btnAllEmergencyInLayer.IsEnabled = LoginInLayer.IsEnabled = false;
                }
                else
                {
                    TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
                }
            }
            SimulateFireAlarmLinkExePlanTimer.Stop();
            SetSimulateFireAlarmLinkExePlanTimer(false);
            IsTimingQueryEPSOrLight = true;
            IsQueryEPSAndLight = true;
        }

        private void SilencingNoLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SilencingNoLogin.Source = new BitmapImage(new Uri("\\Pictures\\Silencing-Selected.png", UriKind.Relative));
        }

        private void SilencingNoLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SilencingNoLogin.Source = new BitmapImage(new Uri("\\Pictures\\Silencing-Unchecked.png", UriKind.Relative));
            SetIsMute();
        }

        private void btnMuteSetConfirm_Click1(object sender, RoutedEventArgs e)
        {
            if (int.Parse(EmergencyTimeSet.Content.ToString()) <= 30)
            {
                MuteSetConfirm1();
                ShowALLFault();
                //EmDisable();
                //HostLampControl();
                //NewHostLampControl();
                GetEdition(IsCommodity);
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "EmergencyTime");
                infoGblSetting.SetValue = EmergencyTimeSet.Content.ToString();
                ObjGblSetting.Update(infoGblSetting);
            }
            else
            {
                CommonFunct.PopupWindow("断主电应急时间不能超过30分钟！");
            }
        }

        private void btnEPSInitialPositionModify_Click(object sender, RoutedEventArgs e)
        {
            EPSInitialPositionText.GotFocus -= tbxEPSInstall_GotFocus;
            EPSInitialPositionText.GotFocus += tbxEPSInstall_GotFocus;
        }

        private void btnEPSInitialPositionSave_Click(object sender, RoutedEventArgs e)
        {
            EPSInitialPositionText.Background = new SolidColorBrush(Colors.Transparent);
            EPSInitialPositionText.GotFocus -= tbxEPSInstall_GotFocus;
            SaveEPSInstallPos();
        }

        private void btnEPSInitialPositionCancel_Click(object sender, RoutedEventArgs e)
        {
            EPSInitialPositionText.Background = new SolidColorBrush(Colors.Transparent);
            EPSInitialPositionText.GotFocus -= tbxEPSInstall_GotFocus;
        }

        private void btnEPSReservePlanModify_Click(object sender, RoutedEventArgs e)
        {
            //将事件清空后再添加事件，防止出现事件叠加现象
            EPSReservePlan1.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan2.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan3.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan4.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan5.GotFocus -= FocusTextBox_GotFocus;

            EPSReservePlan1.GotFocus += FocusTextBox_GotFocus;
            EPSReservePlan2.GotFocus += FocusTextBox_GotFocus;
            EPSReservePlan3.GotFocus += FocusTextBox_GotFocus;
            EPSReservePlan4.GotFocus += FocusTextBox_GotFocus;
            EPSReservePlan5.GotFocus += FocusTextBox_GotFocus;
        }

        private void btnEPSReservePlanSave_Click(object sender, RoutedEventArgs e)
        {
            EPSReservePlan1.Background = EPSReservePlan2.Background = EPSReservePlan3.Background = EPSReservePlan4.Background = EPSReservePlan5.Background = new SolidColorBrush(Colors.Transparent);
            SaveEPSPlan(SelectInfoEPSLogin, EPSReservePlan1.Text, EPSReservePlan2.Text, EPSReservePlan3.Text, EPSReservePlan4.Text, EPSReservePlan5.Text);
            CommonFunct.PopupWindow("修改EPS预案成功！");
        }

        private void btnEPSReservePlanCancel_Click(object sender, RoutedEventArgs e)
        {
            EPSReservePlan1.Background = EPSReservePlan2.Background = EPSReservePlan3.Background = EPSReservePlan4.Background = EPSReservePlan5.Background = new SolidColorBrush(Colors.Transparent);
            EPSReservePlan1.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan2.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan3.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan4.GotFocus -= FocusTextBox_GotFocus;
            EPSReservePlan5.GotFocus -= FocusTextBox_GotFocus;
        }

        private void btnEPSMainPower_Click(object sender, RoutedEventArgs e)
        {
            EmergencyOrMainEleByEPS(false, SelectInfoEPSLogin.Code);
        }

        private void btnEPSeEmergency_Click(object sender, RoutedEventArgs e)
        {
            EmergencyOrMainEleByEPS(true, SelectInfoEPSLogin.Code);
        }

        private void btnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchEPSListPageLogin(true);
            if (isSuccess)
            {
                labEPSCollectCurrentPage.Content = EPSListCurrentPageLogin;
                ShowEPSListLogin();
                //ClearEPSDataLogin();
                ClearAllSelectInfoLogin();
            }
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchEPSListPageLogin(false);
            if (isSuccess)
            {
                labEPSCollectCurrentPage.Content = EPSListCurrentPageLogin;
                ShowEPSListLogin();
                //ClearEPSDataLogin();
                ClearAllSelectInfoLogin();
            }
        }

        private void LightControl(object sender, MouseButtonEventArgs e)
        {
            if (LampStatus != null)
            {
                switch (LampStatus.Tag.ToString())
                {
                    case "亮":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Unchecked.png", UriKind.Relative));
                        break;
                    case "灭":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Unchecked.png", UriKind.Relative));
                        break;
                    case "闪":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Unchecked.png", UriKind.Relative));
                        break;
                    case "主电":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Unchecked.png", UriKind.Relative));
                        break;
                    case "左亮":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Unchecked.png", UriKind.Relative));
                        break;
                    case "右亮":
                        LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Unchecked.png", UriKind.Relative));
                        break;
                }
            }
            LampStatus = sender as Image;

            switch (LampStatus.Tag.ToString())
            {
                case "亮":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Bright-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.全亮, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
                case "灭":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Extinguish-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.全灭, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
                case "闪":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\Flash-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.闪, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
                case "主电":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\MainPower-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.主电, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
                case "左亮":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\LeftBright-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.左亮, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
                case "右亮":
                    LampStatus.Source = new BitmapImage(new Uri("\\Pictures\\RightBright-Selected.png", UriKind.Relative));
                    SingleLightControl(EnumClass.SingleLightControlClass.右亮, SelectInfoLightLogin.Code, SelectInfoEPSLogin.Code);
                    break;
            }
        }

        /// <summary>
        /// 控制灯具状态
        /// </summary>
        /// <param name="SingleLightControlClass"></param>
        /// <param name="infoLight"></param>
        /// <param name="infoDistributionBox"></param>
        private void CollectLampStatus(EnumClass.SingleLightControlClass SingleLightControlClass, LightInfo infoLight, DistributionBoxInfo infoDistributionBox)
        {
            if ((infoLight.Status & (int)EnumClass.LightFaultClass.光源故障) == 0 && (infoLight.Status & (int)EnumClass.LightFaultClass.通信故障) == 0)
            {
                if ((infoLight.Status & 0X20) == 0 && infoLight.IsEmergency != 1)
                {
                    SingleLightControl(SingleLightControlClass, infoLight.Code, infoDistributionBox.Code);
                }
            }
        }

        private void btnLampInitialPositionModify_Click(object sender, RoutedEventArgs e)
        {
            LampsPositionText.GotFocus -= tbxLightInstallPos_GotFocus;
            LampsPositionText.GotFocus += tbxLightInstallPos_GotFocus;
        }

        private void btnLampInitialPositionSave_Click(object sender, RoutedEventArgs e)
        {
            LampsPositionText.Background = new SolidColorBrush(Colors.Transparent);
            LampsPositionText.GotFocus -= tbxLightInstallPos_GotFocus;
            SaveLightInstallPos();
        }

        private void btnLampInitialPositionCancel_Click(object sender, RoutedEventArgs e)
        {
            LampsPositionText.Background = new SolidColorBrush(Colors.Transparent);
            LampsPositionText.GotFocus -= tbxLightInstallPos_GotFocus;
        }

        private void LightInitialState(object sender, RoutedEventArgs e)
        {
            if (SelectInfoLightLogin.ID != 0)
            {
                if (SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.双向标志灯全亮 || SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.双向地埋灯全亮 || SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.其它全亮)
                {
                    InitialStateBright.Source = new BitmapImage(new Uri("/Pictures/BeginStatusOpenUnClicked.png", UriKind.Relative));
                }
                if (SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.左亮)
                {
                    InitialStateLeftBright.Source = new BitmapImage(new Uri("/Pictures/BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
                }
                if (SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.右亮)
                {
                    InitialStateRightBright.Source = new BitmapImage(new Uri("/Pictures/BeginStatusRightOpenUnClicked.png", UriKind.Relative));
                }
                if (SelectInfoLightLogin.BeginStatus == (int)EnumClass.LightStatusClass.全灭)
                {
                    InitialStateExtinguish.Source = new BitmapImage(new Uri("/Pictures/BeginStatusCloseUnClicked.png", UriKind.Relative));
                }
            }
            if (LampInitialState != null)
            {
                switch (LampInitialState.Tag.ToString())
                {
                    case "亮":
                        LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusOpenUnClicked.png", UriKind.Relative));
                        break;
                    case "左亮":
                        LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusLeftOpenUnClicked.png", UriKind.Relative));
                        break;
                    case "右亮":
                        LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusRightOpenUnClicked.png", UriKind.Relative));
                        break;
                    case "灭":
                        LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusCloseUnClicked.png", UriKind.Relative));
                        break;
                }
            }
            LampInitialState = sender as Image;

            switch (LampInitialState.Tag.ToString())
            {
                case "亮":
                    LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusOpenClicked.png", UriKind.Relative));
                    break;
                case "左亮":
                    LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusLeftOpenClicked.png", UriKind.Relative));
                    break;
                case "右亮":
                    LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusRightOpenClicked.png", UriKind.Relative));
                    break;
                case "灭":
                    LampInitialState.Source = new BitmapImage(new Uri("/Pictures/BeginStatusCloseClicked.png", UriKind.Relative));
                    break;
            }

        }

        private void btnLightInitialStateModify_Click(object sender, RoutedEventArgs e)
        {
            InitialStateBright.MouseDown -= LightInitialState;
            InitialStateLeftBright.MouseDown -= LightInitialState;
            InitialStateRightBright.MouseDown -= LightInitialState;
            InitialStateExtinguish.MouseDown -= LightInitialState;

            InitialStateBright.MouseDown += LightInitialState;
            InitialStateLeftBright.MouseDown += LightInitialState;
            InitialStateRightBright.MouseDown += LightInitialState;
            InitialStateExtinguish.MouseDown += LightInitialState;
        }

        private void btnLightInitialStateSave_Click(object sender, RoutedEventArgs e)
        {
            btnLightInitialStateCancel_Click(sender, e);
            SaveLightBeginStatus();
        }

        private void btnLightInitialStateCancel_Click(object sender, RoutedEventArgs e)
        {
            InitialStateBright.MouseDown -= LightInitialState;
            InitialStateLeftBright.MouseDown -= LightInitialState;
            InitialStateRightBright.MouseDown -= LightInitialState;
            InitialStateExtinguish.MouseDown -= LightInitialState;
        }

        private void btnLightPlanModify_Click(object sender, RoutedEventArgs e)
        {
            LeftBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan4.GotFocus -= FocusTextBox_GotFocus;

            LeftBrightPlan1.GotFocus += FocusTextBox_GotFocus;
            LeftBrightPlan2.GotFocus += FocusTextBox_GotFocus;
            LeftBrightPlan3.GotFocus += FocusTextBox_GotFocus;
            LeftBrightPlan4.GotFocus += FocusTextBox_GotFocus;
            RightBrightPlan1.GotFocus += FocusTextBox_GotFocus;
            RightBrightPlan2.GotFocus += FocusTextBox_GotFocus;
            RightBrightPlan3.GotFocus += FocusTextBox_GotFocus;
            RightBrightPlan4.GotFocus += FocusTextBox_GotFocus;
        }

        private void btnLightPlanCancel_Click(object sender, RoutedEventArgs e)
        {
            LeftBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan1.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan2.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan3.GotFocus -= FocusTextBox_GotFocus;
            RightBrightPlan4.GotFocus -= FocusTextBox_GotFocus;
            LeftBrightPlan1.Background = LeftBrightPlan2.Background = LeftBrightPlan3.Background = LeftBrightPlan4.Background = RightBrightPlan1.Background = RightBrightPlan2.Background = RightBrightPlan3.Background = RightBrightPlan4.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void SearchLamp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchLamp.Source = new BitmapImage(new Uri("/Pictures/SearchLights-checked.png", UriKind.Relative));
        }

        private void SearchLamp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchLamp.Source = new BitmapImage(new Uri("/Pictures/SearchLights-Unchecked.png", UriKind.Relative));
            Searchlamp();
            //ShowInitialLightTips();
            //SetEnableMainWindow(false);
            //SearchLight();
            //SetEnableMainWindow(true);
            //HideInitialLightTips();
            //ShowInitialLightInfo();
            //SetAllMainEleTimer(true);
            //AddHistoricalEvent("初始化灯具");
        }

        private void SearchLampFastly_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchLampFastly.Source = new BitmapImage(new Uri("/Pictures/SearchLightsQuickly-checked.png", UriKind.Relative));
        }

        private void SearchLampFastly_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchLampFastly.Source = new BitmapImage(new Uri("/Pictures/SearchLightsQuickly-Unchecked.png", UriKind.Relative));
            Searchlamp();
        }

        private void Searchlamp()
        {
            ObjLight.DeleteAll();
            ObjBlankIcon.DeleteAll();
            List<CoordinateInfo> LstCoordinateByLamp = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.Light.ToString() || x.TableName == EnumClass.TableName.BlankIcon.ToString());
            for (int i = 0; i < LstCoordinateByLamp.Count; i++)
            {
                ObjCoordinate.Delete(LstCoordinateByLamp[i].ID);
            }
            LstBlankIcon?.Clear();
            LstCoordinate = ObjCoordinate.GetAll();
            LstLight?.Clear();
            LightStatusByEPSArray = null;
            LstLightQueryByEPSID?.Clear();
            InitialTips("搜灯中！请勿操作！", true);
            SetEnableMainWindow(false);
            SearchLightFastly();
            SetEnableMainWindow(true);
            InitialTips(null, false);
            ShowInitialLightInfo();
            SetAllMainEleTimer(true);
            AddHistoricalEvent("初始化灯具");
        }

        private void SearchEPSCode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchEPSCode.Source = new BitmapImage(new Uri("/Pictures/SearchEPS-checked.png", UriKind.Relative));
        }

        private void SearchEPSCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchEPSCode.Source = new BitmapImage(new Uri("/Pictures/SearchEPS-Unchecked.png", UriKind.Relative));
            InitialTips("搜EPS中！请勿操作！", true);
            DeleteBusinessData();
            SetEnableMainWindow(false);
            SearchEPSFastly();
            SetEnableMainWindow(true);
            InitialTips(null, false);
            ShowInitialEPSInfo();
            EPSQuantitySearched.Content = EPSTotalQuantity.Content = "0";
            pgbSearchEPS1.Value = 0;
            AddHistoricalEvent("初始化EPS");
            ObjFaultRecord.DeleteAll();
            LstFaultRecord.Clear();
            #region 旧版慢搜EPS
            //this.EPSTotalQuantity.Content = 501;
            //ShowInitialEPSTips();
            //DeleteBusinessData();
            //SetEnableMainWindow(false);
            //SearchEPS();
            //SetEnableMainWindow(true);
            //HideInitialEPSTips();
            //ShowInitialEPSInfo();
            //AddHistoricalEvent("初始化EPS");
            //ObjFaultRecord.DeleteAll();
            //LstFaultRecord.Clear();
            #endregion
        }

        private void SearchEPSCodeFastly_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchEPSCodeFastly.Source = new BitmapImage(new Uri("/Pictures/SearchEPSQuickly-checked.png", UriKind.Relative));
        }

        private void SearchEPSCodeFastly_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SearchEPSCodeFastly.Source = new BitmapImage(new Uri("/Pictures/SearchEPSQuickly-Unchecked.png", UriKind.Relative));
            InitialTips("搜EPS中！请勿操作！", true);
            DeleteBusinessData();
            SetEnableMainWindow(false);
            SearchEPSFastly();
            SetEnableMainWindow(true);
            InitialTips(null, false);
            ShowInitialEPSInfo();
            EPSQuantitySearched.Content = EPSTotalQuantity.Content = "0";
            pgbSearchEPS1.Value = 0;
            AddHistoricalEvent("初始化EPS");
            ObjFaultRecord.DeleteAll();
            LstFaultRecord?.Clear();
        }

        private void btnAddLamp_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = AddSingleLight();
            if (isSuccess)
            {
                EPSAndLightStatCount();
            }
            LampAddEPSCode.Content = string.Empty;
            LampAddCode.Content = string.Empty;
        }

        private void btnDeleteLamp_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = DeleteSingleLight();
            if (isSuccess)
            {
                EPSAndLightStatCount();
            }
            LampAddEPSCode.Content = string.Empty;
            LampAddCode.Content = string.Empty;
        }

        private void btnLampReplace_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = ReplaceSingleLight();
            if (isSuccess)
            {
                EPSAndLightStatCount();
            }
        }

        private void btnLampReplaceCancel_Click(object sender, RoutedEventArgs e)
        {
            LampReplaceEPSCode.Content = string.Empty;
            OldLampCode.Content = string.Empty;
            NewLampCode.Content = string.Empty;
        }

        private void FocusLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetFocusLabel(sender);
            ShowSystemKeyBoardLabel();
        }

        private void GraphicalInterface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GraphicalInterface.Source = new BitmapImage(new Uri("/Pictures/GraphicalInterface-checked.png", UriKind.Relative));
        }

        private void GraphicalInterface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GraphicalInterface.Source = new BitmapImage(new Uri("/Pictures/GraphicalInterface-Unchecked.png", UriKind.Relative));
            ShowFunButton();
            InitLayerModeLogin();
        }

        /// <summary>
        /// 显示图形界面的功能按钮
        /// </summary>
        private void ShowFunButton()
        {
            Menu.Children.Clear();
            List<SubItem> menuImport = new List<SubItem>
            {
                new SubItem("设置总楼层数"),
                new SubItem("导入图纸"),
                new SubItem("删除图纸")
            };
            ItemMenu item = new ItemMenu("楼层设置", menuImport, PackIconKind.Download);

            List<SubItem> menuSetUp = new List<SubItem>
            {
                new SubItem("智能添加"),
                new SubItem("移动设备"),
                new SubItem("一键清除"),
                new SubItem("保存设置")
            };
            ItemMenu item1 = new ItemMenu("设备设置", menuSetUp, PackIconKind.Gear);

            List<SubItem> menuLine = new List<SubItem>
            {
                new SubItem("放置转折点"),
                new SubItem("连线"),
                new SubItem("保存路线")
            };
            ItemMenu printLine = new ItemMenu("设置路线", menuLine, PackIconKind.ExitRun);

            List<SubItem> menuEmergency = new List<SubItem>
            {
                new SubItem("全体应急")
            };
            ItemMenu printEmergency = new ItemMenu("全体应急", menuEmergency, PackIconKind.AlarmLight);

            List<SubItem> menuLinkage = new List<SubItem>
            {
                new SubItem("模拟联动"),
                new SubItem("取消联动")
            };
            ItemMenu printLinkage = new ItemMenu("模拟联动", menuLinkage, PackIconKind.LinkBox);

            List<SubItem> menuReset = new List<SubItem>
            {
                new SubItem("复位")
            };
            ItemMenu printReset = new ItemMenu("复位", menuReset, PackIconKind.AlarmLightOutline);

            List<SubItem> menuMute = new List<SubItem>
            {
                new SubItem("消音")
            };
            ItemMenu printMute = new ItemMenu("消音", menuMute, PackIconKind.Mute);

            List<SubItem> menuFeedBack = new List<SubItem>
            {
                new SubItem("反馈数据")
            };
            ItemMenu printFeedBack = new ItemMenu("反馈数据", menuFeedBack, PackIconKind.CompareVertical);

            Menu.Children.Add(new UserControlMenuItem(item, this));
            Menu.Children.Add(new UserControlMenuItem(item1, this));
            Menu.Children.Add(new UserControlMenuItem(printLine, this));
            Menu.Children.Add(new UserControlMenuItem(printEmergency, this));
            Menu.Children.Add(new UserControlMenuItem(printLinkage, this));
            Menu.Children.Add(new UserControlMenuItem(printReset, this));
            Menu.Children.Add(new UserControlMenuItem(printMute, this));
            Menu.Children.Add(new UserControlMenuItem(printFeedBack, this));

        }

        internal async Task SwitchScreen(object sender)
        {
            switch (sender.ToString())
            {
                case "设置总楼层数":
                    ShowFloorTotalSetUp();
                    break;
                case "导入图纸":
                    await ImportFloorDrawing();
                    break;
                case "删除图纸":
                    DelFloorDrawing();
                    break;
                case "智能添加":
                    InitIconSearchCodeList();
                    break;
                case "移动设备":
                    btnEditAllIcon_Click(null, null);
                    break;
                case "一键清除":
                    btnOneKeyClear_Click(null, null);
                    break;
                case "保存设置":
                    btnSaveAllIcon_Click(null, null);
                    break;
                case "放置转折点":
                    btnSetEscapeRoutes_Click(null, null);
                    break;
                case "连线":
                    PrintLines();
                    break;
                case "保存路线":
                    SaveLines();
                    break;

                case "全体应急":
                    OldEmergency();
                    break;
                case "模拟联动":
                    SimulateFireAlarmLinkInLayer();
                    break;
                case "取消联动":
                    SimulateCancelLinkage();
                    break;

                case "复位":
                    OldCommonResetSystem();
                    break;
                case "消音":
                    SetIsMute();
                    break;
                case "反馈数据":
                    FeedbackData();
                    break;
            }
        }

        /// <summary>
        /// 绘画箭头(逃生路线方向的展示)
        /// </summary>
        /// <param name="line">箭头绘画在此线段上</param>
        /// <param name="pointx">箭头的横坐标</param>
        /// <param name="pointy">箭头的纵坐标</param>
        /// <param name="angle">箭头需要旋转的角度</param>
        public void PrintArrow(Line line, double pointx, double pointy, double angle)
        {
            Image img = new Image
            {
                Tag = line,
                Height = 10,
                Width = 8,
                Source = new BitmapImage(new Uri("/Pictures/Triangle.png", UriKind.Relative)),
                Margin = new Thickness(pointx - 5, pointy - 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            RotateTransform rotateTransform = new RotateTransform(angle);
            img.RenderTransformOrigin = new Point(0.5, 0.5);
            img.RenderTransform = rotateTransform;
            cvsMainWindow.Children.Add(img);
        }

        /// <summary>
        /// 删除相关线段上的箭头
        /// </summary>
        /// <param name="alarmpoint">需要删除箭头的线段信息</param>
        public void DeleteOldImage(GenerateRoutes.AlarmPoint alarmpoint)
        {
            List<Image> OldImages = new List<Image>();
            //找出新火灾点对应的线路上的旧箭头
            for (int i = 0; i < cvsMainWindow.Children.Count; i++)
            {
                Image OldImage = cvsMainWindow.Children[i] as Image;
                if (OldImage == null)
                {
                    continue;
                }
                else
                {
                    if (OldImage.Tag is Line && (OldImage.Tag as Line).X1 == alarmpoint.FootDropLine.X1 && (OldImage.Tag as Line).X2 == alarmpoint.FootDropLine.X2 && (OldImage.Tag as Line).Y1 == alarmpoint.FootDropLine.Y1 && (OldImage.Tag as Line).Y2 == alarmpoint.FootDropLine.Y2)
                    {
                        OldImages.Add(OldImage);
                    }
                }
            }
            //删除新火灾点对应的线路上的旧箭头
            for (int i = 0; i < OldImages.Count; i++)
            {
                cvsMainWindow.Children.Remove(OldImages[i]);
            }
        }

        /// <summary>
        /// 删除相关线段上的部分箭头
        /// </summary>
        /// <param name="alarmpoint">需要删除箭头的线段信息</param>
        /// <param name="point">需要删除的箭头的条件</param>
        public void DeleteOldImage(GenerateRoutes.AlarmPoint alarmpoint, Point point)
        {
            List<Image> OldImages = new List<Image>();
            bool DeleCondition = false;

            if (alarmpoint.FootDropLine.X1 == alarmpoint.FootDropLine.X2)
            {
                if (point.Y < alarmpoint.Perpendicular.Y)
                {
                    DeleCondition = false;
                }
                else
                {
                    DeleCondition = true;
                }
                //找出新火灾点对应的线路上的旧箭头
                for (int i = 0; i < cvsMainWindow.Children.Count; i++)
                {
                    Image OldImage = cvsMainWindow.Children[i] as Image;
                    if (OldImage == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (DeleCondition)
                        {
                            //10为箭头展示坐标与真实坐标的差距
                            if (OldImage.Tag == alarmpoint.FootDropLine && OldImage.Margin.Top + 10 > alarmpoint.Perpendicular.Y)
                            {
                                OldImages.Add(OldImage);
                            }
                        }
                        else
                        {
                            if (OldImage.Tag == alarmpoint.FootDropLine && OldImage.Margin.Top + 10 < alarmpoint.Perpendicular.Y)
                            {
                                OldImages.Add(OldImage);
                            }
                        }
                    }
                }
            }
            else
            {
                if (point.X < alarmpoint.Perpendicular.X)
                {
                    DeleCondition = false;
                }
                else
                {
                    DeleCondition = true;
                }
                //找出新火灾点对应的线路上的旧箭头
                for (int i = 0; i < cvsMainWindow.Children.Count; i++)
                {
                    Image OldImage = cvsMainWindow.Children[i] as Image;
                    if (OldImage == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (DeleCondition)
                        {
                            //12为箭头展示坐标与真实坐标的差距
                            if (OldImage.Tag == alarmpoint.FootDropLine && OldImage.Margin.Left + 12 > alarmpoint.Perpendicular.X)
                            {
                                OldImages.Add(OldImage);
                            }
                        }
                        else
                        {
                            if (OldImage.Tag == alarmpoint.FootDropLine && OldImage.Margin.Left + 12 < alarmpoint.Perpendicular.X)
                            {
                                OldImages.Add(OldImage);
                            }
                        }
                    }
                }
            }
            //删除新火灾点对应的线路上的旧箭头
            for (int i = 0; i < OldImages.Count; i++)
            {
                cvsMainWindow.Children.Remove(OldImages[i]);
            }
        }

        private void btnSimulationLinkage_Click(object sender, RoutedEventArgs e)
        {
            IsSimulationLinkage = true;
            IsLinkageTiming = true;
            SetSimulateFireAlarmLinkExePlanTimer(true);
            //SetSimulateFireAlarmLinkTimer(true);
            SetIsAllMainEle(false);
            GetSimulateFireAlarmLinkZoneNumber();//获取预案号
            SetSimulateFireAlarmLinkPage(true);
            RecordSimulateFireAlarmLinkHistory();
            ClearAllEmergencyCalcuTime();
            SimulateFireAlarmLinkExePlan();
            //HostLampControl();
            //NewHostLampControl();
            GetEdition(IsCommodity);
        }

        private void btnCancelLinkage_Click(object sender, RoutedEventArgs e)
        {
            SetSimulateFireAlarmLinkExePlanTimer(false);
            IsRefreshProgressBar = true;//刷新进度条
            IsLinkageTiming = false;//停止计时
            //SetSimulateFireAlarmLinkTimer(false);
            SetIsEnterResetSystem(true);//进入复位
            AbsFireAlarmLink.SendHostBoardData(0X01);
            AbsFireAlarmLink.HostBoardSendStatus = 0X01;
            SetAllMainEleTimer(true);
            RecordSimulateFireAlarmLinkInfo();
            Resetting.Visibility = System.Windows.Visibility.Visible;
            ResettingProgressBar = ResetSystem;
            RefreshProgressBarValueTimer_Tick(sender, e);
            labSimulateLinkEmergencyTime.Content = "00:00:00";
            SetSimulateFireAlarmLinkPage(false);
            IsRefreshProgressBar = false;
            SetIsEnterResetSystem(false);//退出复位
            //Thread.Sleep(9000);
            IsSimulationLinkage = false;
        }

        private void AutomaticReceptionYes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetIsAutoFireAlarmLink(true);
        }

        private void AutomaticReceptionNo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetIsAutoFireAlarmLink(false);
        }

        private void btnIsAutoFireAlarmLinkConfirm_Click(object sender, RoutedEventArgs e)
        {
            SetIsAutoFireAlarmLink();
        }

        private void btnLinkageUnificationYes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetIsFireAlarmLinkNormal(true);
        }

        private void btnLinkageUnificationNo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetIsFireAlarmLinkNormal(false);
        }

        private void btnIsFireAlarmLinkNormalConfirm_Click(object sender, RoutedEventArgs e)
        {
            SetIsFireAlarmLinkNormal();
        }

        private void btnFireAlarmTypeConfirm_Click(object sender, RoutedEventArgs e)
        {
            SetFireAlarmType();
        }

        private void PreviousEPSCode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviousEPSCode.Source = new BitmapImage(new Uri("/Pictures/LastDown.jpg", UriKind.Relative));
        }

        private void PreviousEPSCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviousEPSCode.Source = new BitmapImage(new Uri("/Pictures/LastUp.jpg", UriKind.Relative));
            if (int.Parse(EPSCurrentByLampCollect.Content.ToString()) > 1)
            {
                EPSCurrentByLampCollect.Content = int.Parse(EPSCurrentByLampCollect.Content.ToString()) - 1;
            }
            else
            {
                EPSCurrentByLampCollect.Content = EPSTotalByLampCollect.Content;
            }
            if (LstDistributionBox.Count != 0)
            {
                SelectInfoEPSNoLogin = LstDistributionBox[int.Parse(EPSCurrentByLampCollect.Content.ToString()) - 1];
                LstLightViewByDisBoxIDNoLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(SelectInfoEPSNoLogin.Code));
                EPSCodeByLampCollect.Content = SelectInfoEPSNoLogin.Code;

                OpenLightSummaryNoLoginPage();
                ShowLightListNoLogin();
            }
        }

        private void NextEPSCode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NextEPSCode.Source = new BitmapImage(new Uri("/Pictures/NextDown.jpg", UriKind.Relative));
        }

        private void NextEPSCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NextEPSCode.Source = new BitmapImage(new Uri("Pictures/NextUp.jpg", UriKind.Relative));
            if (int.Parse(EPSCurrentByLampCollect.Content.ToString()) < int.Parse(EPSTotalByLampCollect.Content.ToString()))
            {
                EPSCurrentByLampCollect.Content = int.Parse(EPSCurrentByLampCollect.Content.ToString()) + 1;
            }
            else
            {
                EPSCurrentByLampCollect.Content = 1;
            }
            if (LstDistributionBox.Count != 0)
            {
                SelectInfoEPSNoLogin = LstDistributionBox[int.Parse(EPSCurrentByLampCollect.Content.ToString()) - 1];
                LstLightViewByDisBoxIDNoLogin = LstLight.FindAll(x => x.DisBoxID == int.Parse(SelectInfoEPSNoLogin.Code));
                EPSCodeByLampCollect.Content = SelectInfoEPSNoLogin.Code;

                OpenLightSummaryNoLoginPage();
                ShowLightListNoLogin();
            }
        }

        private void btnPreviousPageByLamp_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchLightListPageNoLogin(true);
            if (isSuccess)
            {
                ShowLightListNoLogin();
            }
        }

        private void btnNextPageByLamp_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchLightListPageNoLogin(false);
            if (isSuccess)
            {
                ShowLightListNoLogin();
            }
        }

        private void btnMonthlyInspectionByHand_Click(object sender, RoutedEventArgs e)
        {
            if (IsAllMainEle)
            {
                if (LstFaultRecord.Count == 0)
                {
                    LatelyFaultRecord = null;
                }
                else
                {
                    LatelyFaultRecord = LstFaultRecord[LstFaultRecord.Count - 1];
                }
                DateTime MonthCheckTime = DateTime.Now;
                SelfCheckingTitle.Content = "正在进行手动月检...";
                DisableButton(true);
                ExeMonthOrSeasonCheck(true);

                ExecuteMonthCheckTimer.Enabled = false;
                MonthOrSeasonCheckFault.Enabled = true;
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsMonthlyInspection");
                infoGblSetting.SetValue = "false";//自动月检改成手动月检
                ObjGblSetting.Update(infoGblSetting);
                infoGblSetting = LstGblSetting.Find(x => x.Key == "NextMonthCheckTime");
                infoGblSetting.SetValue = MonthCheckTime.AddMonths(1).ToString();//更新下一次月检的时间
                ObjGblSetting.Update(infoGblSetting);
                DisableButton(false);
            }
        }

        private void btnSelfCheckingReset_Click(object sender, RoutedEventArgs e)
        {
            

            IsEmergency = false;
            SelfCheckingReset.IsEnabled = false;
            SetIsAllMainEle(true);
            IsOpenEmTime = false;
            IsOpenMainTime = false;
            IsComEmergency = false;
            IsAccelerateDetection = false;

            LstDistributionBox.ForEach(x => x.IsEmergency = 0);
            LstLight.ForEach(x => x.IsEmergency = 0);
            ObjDistributionBox.Save(LstDistributionBox);
            ObjLight.Save(LstLight);

            AllMainEle();
            SelfCheckPrompt.Visibility = System.Windows.Visibility.Hidden;
            TotalExecuteSecond = 0;
            SelfCheckingHour.Content = SelfCheckingMinute.Content = SelfCheckingSecond.Content = SelfCheckingFrequency.Content = 0;
            SelfCheckingTime.Foreground = SelfCheckingHour.Foreground = SelfCheckingMinute.Foreground = SelfCheckingSecond.Foreground = BetweenHourAndMinute.Foreground = BetweenMinuteAndSecond.Foreground = CommonFunct.GetBrush("#8CB3D9");
            SelfCheckingResetShow.Visibility = System.Windows.Visibility.Visible;
            SelfCheckingFrequency.Visibility = SelfCheckingFrequencyUnit.Visibility = System.Windows.Visibility.Hidden;
            for (int i = 1; i <= SelfCheckingResetSystem.Maximum; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(50);
                SelfCheckingResetSystem.Value = i;
            }
            if (SelfCheckingResetSystem.Value == SelfCheckingResetSystem.Maximum)
            {
                SelfCheckingResetSystem.Value = 0;
                SelfCheckingResetShow.Visibility = System.Windows.Visibility.Hidden;
            }
            SelfCheckingReset.IsEnabled = true;
            
        }

        private void btnSeasonlyInspectionByHand_Click(object sender, RoutedEventArgs e)
        {
            if (IsAllMainEle)
            {
                if(LstFaultRecord.Count> 0 && LstFaultRecord != null) {
                    LatelyFaultRecord = LstFaultRecord[LstFaultRecord.Count - 1];
                }
                DateTime MonthCheckTime = DateTime.Now;

                SelfCheckingTitle.Content = "正在进行手动年检...";
                DisableButton(true);
                ExeMonthOrSeasonCheck(false);

                ExecuteSeasonCheckTimer.Enabled = false;
                MonthOrSeasonCheckFault.Enabled = true;
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "IsSeasonlyInspection");
                infoGblSetting.SetValue = "false";//自动月检改成手动月检
                ObjGblSetting.Update(infoGblSetting);
                infoGblSetting = LstGblSetting.Find(x => x.Key == "NextSeasonCheckTime");
                infoGblSetting.SetValue = MonthCheckTime.AddMonths(12).ToString();//更新下一次年检的时间
                ObjGblSetting.Update(infoGblSetting);
                DisableButton(false);
            }
        }

        /// <summary>
        /// 月年检按键启动
        /// </summary>
        private void KeyMonthlyAndYearLyCheck()
        {
            if (!IsKeyAccelerateCheck)
            {
                IsKeyAccelerateCheck = true;
                MonthlyAndYearlyCheckKey.Visibility = System.Windows.Visibility.Visible;
                btnMonthAndYearCheckSpeedUp_Click(null, null);
                IsKeyAccelerateCheck = false;
            }
        }

        private void btnMonthAndYearCheckSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            if (IsAllMainEle)
            {
                if (LstFaultRecord.Count == 0)
                {
                    LatelyFaultRecord = null;
                }
                else
                {
                    LatelyFaultRecord = LstFaultRecord[LstFaultRecord.Count - 1];//记录月年检开始前的一次故障
                }
                StartMonthAndSeasonCheckSpeedUp();
                DisableButton(true);
                ExeMonthAndSeasonCheck();
                DisableButton(false);
            }
        }

        private void EPSImageDisplayPreviousPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EPSImageDisplayPreviousPage.Source = new BitmapImage(new Uri("/Pictures/HomeLastUp-Selected.png", UriKind.Relative));
        }

        private void EPSImageDisplayPreviousPage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EPSImageDisplayPreviousPage.Source = new BitmapImage(new Uri("/Pictures/HomeLastUp.png", UriKind.Relative));
            bool isSuccess = SwitchEPSImageDisplay(true);
            if (isSuccess)
            {
                InitEPSShowNoLogin();
            }
        }

        private void EPSImageDisplayNextPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EPSImageDisplayNextPage.Source = new BitmapImage(new Uri("/Pictures/HomeNextUp-Selected.png", UriKind.Relative));
        }

        private void EPSImageDisplayNextPage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EPSImageDisplayNextPage.Source = new BitmapImage(new Uri("/Pictures/HomeNextUp.png", UriKind.Relative));
            bool isSuccess = SwitchEPSImageDisplay(false);
            if (isSuccess)
            {
                InitEPSShowNoLogin();
            }
        }

        private void ControllerImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Visible;
            ShowControllerPanel();
        }

        private void ControllerImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ControllerImage_TouchDown(object sender, TouchEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Visible;
            ShowControllerPanel();
        }

        private void ControllerImage_TouchUp(object sender, TouchEventArgs e)
        {
            stpInformationDisplay.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnGraphicalNoLogin_Click(object sender, RoutedEventArgs e)
        {
            stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
            //this.TimeAndState.IsEnabled = this.MasterController.IsEnabled = false;
            ShowTotalFloorNoLogin();
            SetIsOpenLayerModeNoLogin(true);
            SwitchFloorNoLogin();
        }

        private void btnCloseGuiNoLogin_Click(object sender, RoutedEventArgs e)
        {
            stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Hidden;
            ClearAllElementLogin();
            MenuCancel();

            EPSTotalNumber.Content = LstDistributionBox.Count;
            LampTotalNumber.Content = LstLight.Count;

            EPSShowCurrentPage.Content = 1;
            EPSShowTotalPage.Content = LstDistributionBox.Count % 12 == 0 ? LstDistributionBox.Count / 12 : LstDistributionBox.Count / 12 + 1;
            EPSImageDisplayTotalPage = LstDistributionBox.Count != 0 ? (LstDistributionBox.Count - 1) / (EPSImageDisplayColumnCount * EPSImageDisplayMaxRowCount) + 1 : 1;


            InitEPSShowNoLogin();
            ShowHistoryEventRecordLog();
            TimeAndState.IsEnabled = MasterController.IsEnabled = true;
            IsOpenLayerModeNoLogin = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ClearAllElementNoLogin();

            bool isSuccess = OpenLoginResetSystemPage();
            if (isSuccess)
            {
                ResetSystemNoLogin();
                stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Hidden;
                ClearAllElementLogin();
                //QueryEPSAndLightTimer.Enabled = true;
                IsQueryEPSAndLight = true;
                TimeAndState.IsEnabled = MasterController.IsEnabled = true;
            }
            else
            {
                if (IsRealFireAlarmLink)
                {
                    RestoreAllIcon();
                }
                else
                {
                    TimeAndState.Visibility = MasterController.Visibility = FunctionPage.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void btnPreviousPageForHistory_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryListCurrentPage > 1)
            {
                HistoryListCurrentPage--;
            }
            PreviousPageForHistory.Content = HistoryListCurrentPage;
            ShowHistoryList();
        }

        private void btnNextPageForHistory_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryListCurrentPage < int.Parse(TotalPageForHistory.Content.ToString()))
            {
                HistoryListCurrentPage++;
            }
            PreviousPageForHistory.Content = HistoryListCurrentPage;
            ShowHistoryList();
        }

        /// <summary>
        /// 进入设置转折点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetEscapeRoutes_Click(object sender, RoutedEventArgs e)
        {
            IsPrintLine = true;
            ObservableCollection<int> PointIndex = new ObservableCollection<int>();
            ObservableCollection<int> ExitIndex = new ObservableCollection<int>();
            for (int i = 1; i <= LstEscapeRoutesCurrentFloorLogin.Count; i++)
            {
                PointIndex.Add(i);
                if (LstEscapeRoutesCurrentFloorLogin[i - 1].EndPoint == 1)
                {
                    ExitIndex.Add(i);
                }
            }
            PointNum.ItemsSource = PointIndex;
            PointNum.SelectedIndex = 0;
            EXITIndex.ItemsSource = ExitIndex;
            EXITIndex.SelectedIndex = 0;

            PointEdit.Visibility = PointDel.Visibility = PrintPoint.Visibility = System.Windows.Visibility.Visible;
            PointOK.Visibility = Connect.Visibility = AnalogLinkage.Visibility = System.Windows.Visibility.Hidden;

            ctcFloorDrawingLogin.MouseDown -= ctcFloorDrawingLogin_MouseDown;
            ctcFloorDrawingLogin.MouseUp -= ctcFloorDrawingLogin_MouseUp;
            ctcFloorDrawingLogin.MouseMove -= ctcFloorDrawingLogin_MouseMove;
            ctcFloorDrawingLogin.TouchDown -= ctcFloorDrawingLogin_TouchDown;
            ctcFloorDrawingLogin.TouchUp -= ctcFloorDrawingLogin_TouchUp;
            ctcFloorDrawingLogin.TouchMove -= ctcFloorDrawingLogin_TouchMove;
            ctcFloorDrawingLogin.DragOver -= ctcFloorDrawingLogin_DragOver;
            ctcFloorDrawingLogin.Drop -= ctcFloorDrawingLogin_Drop;
            //先去掉事件后添加事件，防止事件叠加
            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
            ctcFloorDrawingLogin.MouseLeftButtonDown += GetCoordinates_MouseDown;

            InitPartitionLogin();
            LoadEscapeRoutesLogin();
            RefreshEscapeRoutesLogin();
        }

        /// <summary>
        /// 进入连线步骤
        /// </summary>
        private void PrintLines()
        {
            PointEdit.Visibility = PointOK.Visibility = Connect.Visibility = System.Windows.Visibility.Visible;
            PointDel.Visibility = PrintPoint.Visibility = AnalogLinkage.Visibility = System.Windows.Visibility.Hidden;

            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
        }

        private void SaveLines()
        {
            IsPrintLine = false;
            PointEdit.Visibility = PointDel.Visibility = PointOK.Visibility = Connect.Visibility = AnalogLinkage.Visibility = System.Windows.Visibility.Hidden;

            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
        }

        /// <summary>
        /// 反馈数据
        /// </summary>
        private void FeedbackData()
        {
            InitialTips("反馈数据中！请勿操作！", true);
            SetEnableMainWindow(false);
            LstLight = ObjLight.GetAll();
            LstDistributionBox = ObjDistributionBox.GetAll();
            IsTimingQueryEPSOrLight = false;
            foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBox)
            {
                if (Protocol.FindEPS(infoDistributionBox.Code))
                {
                    Protocol.TransAllLightCodeByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code), infoDistributionBox.Code);
                    Protocol.TransLightLeftPlanByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code), infoDistributionBox.Code);
                    Protocol.TransLightRightPlanByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code), infoDistributionBox.Code);
                    Protocol.TransLightStateByEPS(LstLight.FindAll(x => x.DisBoxID.ToString() == infoDistributionBox.Code), infoDistributionBox.Code);
                    Protocol.TransEPSPlan(LstDistributionBox.Find(x => x.Code == infoDistributionBox.Code), infoDistributionBox.Code);
                }
                else
                {
                    MessageBox.Show("不存在" + infoDistributionBox.Code);
                }
            }
            IsTimingQueryEPSOrLight = true;
            InitialTips(null, false);
            SetEnableMainWindow(true);
        }

        private void btnEPSArrayPrePage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchEPSListPageLogin(true);
            if (isSuccess)
            {
                EPSCurrentPage.Content = EPSListCurrentPageLogin;
                ShowEPSList();
                ShowLightListLogin();
            }
        }

        private void btnEPSArrayNextPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchEPSListPageLogin(false);
            if (isSuccess)
            {
                EPSCurrentPage.Content = EPSListCurrentPageLogin;
                ShowEPSList();
                ShowLightListLogin();
            }
        }

        private void btnLampByEPSPrePage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchLightListPageLogin(true);
            if (isSuccess)
            {
                LampCurrentPage.Content = LightListCurrentPageLogin;
                //this.LampCurrentPage.Content = EPSListCurrentPageLogin;
                ShowLightListLogin();
            }
        }

        private void btnLampByEPSNextPage_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = SwitchLightListPageLogin(false);
            if (isSuccess)
            {
                LampCurrentPage.Content = LightListCurrentPageLogin;
                ShowLightListLogin();
            }
        }

        private void btnPartitionSet_Click(object sender, RoutedEventArgs e)
        {
            OpenPartitionSetPage();
            InitFireAlarmPartitionSet();
        }

        private void InitIconSearchCodeListLogin(object sender, SelectionChangedEventArgs e)
        {
            if (comIconSearchCode.SelectedIndex == -1)
            {
                comIconSearchCode.SelectedIndex = 0;
            }
            lvIconSearchCodeList.Items.Clear();
            string strIconSearchCode = comIconSearchCode.SelectedItem.ToString();
            if (strIconSearchCode != null)
            {
                IconSearchCode IconSearchCode;
                DistributionBoxInfo infoDistrbutiobBox = LstDistributionBox.Find(x => x.Code == strIconSearchCode);
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistrbutiobBox.ID);
                if (infoCoordinate.Location == 0 && infoDistrbutiobBox.Code != null)
                {
                    IconSearchCode = GetIconSearchCodeByListLogin(infoDistrbutiobBox);
                    lvIconSearchCodeList.Items.Add(IconSearchCode);
                    
                    
                }



                LstLightIconSearchCodeList = LstLight.FindAll(x => x.DisBoxID == int.Parse(strIconSearchCode) && LstCoordinate.Find(y => y.TableName == EnumClass.TableName.Light.ToString() && y.TableID == x.ID).Location == 0);
                foreach (LightInfo infoLight in LstLightIconSearchCodeList)
                {
                    if(infoLight.Code != null)
                    {
                        IconSearchCode = GetIconSearchCodeByListLogin(infoLight);
                        lvIconSearchCodeList.Items.Add(IconSearchCode);
                    }
                    
                }
            }

            lvIconSearchCodeList.Items.Add(new PartitionPoint());
        }

        private void btnOneKeySearch_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("“一键组网”将清除所有数据，是否继续？", "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.Yes)
            {
                SetEnableMainWindow(false);
                OneKeySearchShow.Visibility = System.Windows.Visibility.Visible;
                //QueryEPSAndLightTimer.Enabled = false;
                IsQueryEPSAndLight = false;
                IsOneKey = true;

                //初始化数据
                OneKeyInitialize.Visibility = System.Windows.Visibility.Visible;
                DeleteBusinessData();

                //快速搜EPS
                SearchEPSShow.Visibility = System.Windows.Visibility.Visible;
                SearchEPSFastly();
                SearchedNum.Visibility = TotalSearch.Visibility = OneKeySearchSpeed.Visibility = System.Windows.Visibility.Visible;
                SearchedNum.Content = TotalSearch.Content = OneKeySearchSpeed.Value = 0;

                //显示EPS数量
                EPSNumByOneKey.Content = LstDistributionBox.Count;
                EPSTotalShow.Visibility = EPSNumByOneKey.Visibility = System.Windows.Visibility.Visible;

                //快速搜灯
                SearchLampShow.Visibility = System.Windows.Visibility.Visible;
                TotalSearch.Content = EPSNumByOneKey.Content;
                SearchLightFastly();

                //显示灯具数量
                LampNumByOneKey.Content = LstLight.Count;
                LampTotalShow.Visibility = LampNumByOneKey.Visibility = System.Windows.Visibility.Visible;

                //显示每个类型灯的数量
                FloodlightNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.照明灯).Count;
                TwoWaySignNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向标志灯).Count;
                TwoBurnerNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双头灯).Count;
                TwoBuriedNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.双向地埋灯).Count;
                EXITNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.安全出口灯).Count;
                FloorIndicatorNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.楼层灯).Count;
                OneWaySignNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向标志灯).Count;
                OneBuriedNumByOneKey.Content = LstLight.FindAll(x => x.LightClass == (int)EnumClass.LightClass.单向地埋灯).Count;
                LampTypeNum.Visibility = System.Windows.Visibility.Visible;

                //系统复位
                SystemResert.Visibility = FinallyShow.Visibility = System.Windows.Visibility.Visible;
                Protocol.AllMainEle();
                //清除故障重新巡检
                ObjFaultRecord.DeleteAll();
                LstFaultRecord.Clear();
                //QueryEPSAndLightTimer.Enabled = true;
                IsQueryEPSAndLight = true;
                CloseWin.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            SetEnableMainWindow(true);
            IsQueryEPSAndLight = true;
            EPSNumByOneKey.Content = LampNumByOneKey.Content = FloodlightNumByOneKey.Content = TwoWaySignNumByOneKey.Content = TwoBurnerNumByOneKey.Content = TwoBuriedNumByOneKey.Content = EXITNumByOneKey.Content = FloorIndicatorNumByOneKey.Content = OneWaySignNumByOneKey.Content = OneBuriedNumByOneKey.Content = SearchedNum.Content = TotalSearch.Content = OneKeySearchSpeed.Value = 0;

            OneKeyInitialize.Visibility = SearchEPSShow.Visibility = EPSTotalShow.Visibility = EPSNumByOneKey.Visibility = SearchLampShow.Visibility = LampTotalShow.Visibility = LampNumByOneKey.Visibility = LampTypeNum.Visibility = SystemResert.Visibility = FinallyShow.Visibility = System.Windows.Visibility.Hidden;

            SearchedNum.Visibility = TotalSearch.Visibility = OneKeySearchSpeed.Visibility = System.Windows.Visibility.Hidden;

            OneKeySearchShow.Visibility = System.Windows.Visibility.Hidden;
            CloseWin.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnLayerEdit_Click(object sender, RoutedEventArgs e)
        {
            stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;

        }

        private void btnLayerEditOK_Click(object sender, RoutedEventArgs e)
        {
            if (Number.Content.ToString() != "0")
            {
                if (Number.Content.ToString().Substring(0, 1) == "6")
                {
                    if (!CommonFunct.IsNumeric(EPSPlan1.Text) || !CommonFunct.IsNumeric(EPSPlan2.Text) || !CommonFunct.IsNumeric(EPSPlan3.Text) || !CommonFunct.IsNumeric(EPSPlan4.Text) || !CommonFunct.IsNumeric(EPSPlan5.Text))
                    {
                        MessageBox.Show("输入格式不正确", "提示");
                    }
                    else
                    {
                        DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == Number.Content.ToString());
                        infoDistributionBox.Address = FacilityAddress.Text;
                        infoDistributionBox.Plan1 = int.Parse(EPSPlan1.Text);
                        infoDistributionBox.Plan2 = int.Parse(EPSPlan2.Text);
                        infoDistributionBox.Plan3 = int.Parse(EPSPlan3.Text);
                        infoDistributionBox.Plan4 = int.Parse(EPSPlan4.Text);
                        infoDistributionBox.Plan5 = int.Parse(EPSPlan5.Text);
                        SaveEPSPlan(infoDistributionBox, EPSPlan1.Text, EPSPlan2.Text, EPSPlan3.Text, EPSPlan4.Text, EPSPlan5.Text);
                        ObjDistributionBox.Update(infoDistributionBox);
                        CommonFunct.PopupWindow("设备信息修改成功！");
                        stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
                else
                {
                    if (!CommonFunct.IsNumeric(LampLeftPlan1.Text) || !CommonFunct.IsNumeric(LampLeftPlan2.Text) || !CommonFunct.IsNumeric(LampLeftPlan3.Text) || !CommonFunct.IsNumeric(LampLeftPlan4.Text) || !CommonFunct.IsNumeric(LampRightPlan1.Text) || !CommonFunct.IsNumeric(LampRightPlan2.Text) || !CommonFunct.IsNumeric(LampRightPlan3.Text) || !CommonFunct.IsNumeric(LampRightPlan4.Text))
                    {
                        MessageBox.Show("输入格式不正确", "提示");
                    }
                    else
                    {
                        LightInfo infoLight = LstLight.Find(x => x.Code == Number.Content.ToString() && x.DisBoxID == int.Parse(DisCode.Content.ToString()));
                        infoLight.Address = FacilityAddress.Text;

                        infoLight.PlanLeft1 = int.Parse(LampLeftPlan1.Text);
                        infoLight.PlanLeft2 = int.Parse(LampLeftPlan2.Text);
                        infoLight.PlanLeft3 = int.Parse(LampLeftPlan3.Text);
                        infoLight.PlanLeft4 = int.Parse(LampLeftPlan4.Text);

                        if (infoLight.LightClass == (int)EnumClass.LightClass.双向地埋灯 || infoLight.LightClass == (int)EnumClass.LightClass.双向标志灯)
                        {
                            infoLight.PlanRight1 = int.Parse(LampRightPlan1.Text);
                            infoLight.PlanRight2 = int.Parse(LampRightPlan2.Text);
                            infoLight.PlanRight3 = int.Parse(LampRightPlan3.Text);
                            infoLight.PlanRight4 = int.Parse(LampRightPlan4.Text);
                        }
                        SaveLightPlan(infoLight, LampLeftPlan1.Text, LampLeftPlan2.Text, LampLeftPlan3.Text, LampLeftPlan4.Text, LampRightPlan1.Text, LampRightPlan2.Text, LampRightPlan3.Text, LampRightPlan4.Text);
                        ObjLight.Update(infoLight);
                        CommonFunct.PopupWindow("设备信息修改成功！");
                        stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                    }

                }
            }
            else
            {
                string EPSCode = EPSComboBox.SelectedItem.ToString();
                if (LampNewCode.Visibility == System.Windows.Visibility.Hidden)
                {
                    DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == EPSCode);
                    if (infoDistributionBox != null)
                    {
                        CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.DistributionBox.ToString() && x.TableID == infoDistributionBox.ID);
                        infoCoordinate.OriginX = OriginDragFloorLogin.X;
                        infoCoordinate.OriginY = OriginDragFloorLogin.Y;
                        infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                        infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                        infoCoordinate.TransformX = DragFloorLogin.X;
                        infoCoordinate.TransformY = DragFloorLogin.Y;
                        infoCoordinate.Location = CurrentSelectFloorLogin;
                        infoDistributionBox.Address = FacilityAddress.Text;
                        infoDistributionBox.Plan1 = int.Parse(EPSPlan1.Text);
                        infoDistributionBox.Plan2 = int.Parse(EPSPlan2.Text);
                        infoDistributionBox.Plan3 = int.Parse(EPSPlan3.Text);
                        infoDistributionBox.Plan4 = int.Parse(EPSPlan4.Text);
                        infoDistributionBox.Plan5 = int.Parse(EPSPlan5.Text);

                        SaveEPSPlan(infoDistributionBox, EPSPlan1.Text, EPSPlan2.Text, EPSPlan3.Text, EPSPlan4.Text, EPSPlan5.Text);
                        ObjDistributionBox.Update(infoDistributionBox);
                        ObjCoordinate.Update(infoCoordinate);
                        CommonFunct.PopupWindow("设备信息保存成功！");
                        BlankIcon = new Image();
                        ClearAllElementLogin();
                        GetDataCurrentFloorLogin();
                        LoadIconSearchCodeLogin();
                        RefreshIconSearchCodeLogin();
                        stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                        BlankIcon = new Image();
                    }
                    else
                    {
                        CommonFunct.PopupWindow("不存在该配电箱！");
                    }
                }
                else
                {
                    string LampCode = LampNewCode.Text;
                    bool isNumeric = CommonFunct.IsNumeric(LampCode);
                    if (!isNumeric || !(int.Parse(LampCode) >= 100000 && int.Parse(LampCode) <= 899999 && LampCode.Substring(0, 1) != "6"))//设定空白控件，存入数据库
                    {
                        //CommonFunct.PopupWindow("请输入灯码");
                        BlankIconInfo infoBlankIcon = new BlankIconInfo
                        {
                            Type = Type.Content.ToString(),
                            DisboxCode = EPSCode,
                            RtnDirection = 0,
                            EscapeLineID = -1
                        };

                        CoordinateInfo infoCoordinate = new CoordinateInfo
                        {
                            TableID = ObjBlankIcon.Add(infoBlankIcon),
                            TableName = EnumClass.TableName.BlankIcon.ToString(),
                            Location = CurrentSelectFloorLogin,
                            OriginX = OriginDragFloorLogin.X,
                            OriginY = OriginDragFloorLogin.Y,
                            NLOriginX = OriginDragFloorNoLogin.X,
                            NLOriginY = OriginDragFloorNoLogin.Y,
                            TransformX = NewMoveAllIconLogin.X,
                            TransformY = NewMoveAllIconLogin.Y,
                            IsAuth = 0
                        };
                        ObjCoordinate.Add(infoCoordinate);
                        LstCoordinate = ObjCoordinate.GetAll();
                        LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

                        LstBlankIcon = ObjBlankIcon.GetAll();

                        CommonFunct.PopupWindow("设备信息保存成功！");
                        BlankIcon = new Image();
                        ClearAllElementLogin();
                        GetDataCurrentFloorLogin();
                        LoadIconSearchCodeLogin();
                        RefreshIconSearchCodeLogin();
                        stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                        BlankIcon = new Image();
                    }
                    else
                    {
                        if (LstLight.Find(x => x.Code == LampCode && x.DisBoxID.ToString() == EPSCode) != null)//已经存在此灯具，记录灯具的坐标信息
                        {
                            LightInfo infoLight = LstLight.Find(x => x.Code == LampCode && x.DisBoxID.ToString() == EPSCode);
                            CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.Light.ToString() && x.TableID == infoLight.ID);
                            infoLight.Address = FacilityAddress.Text;
                            infoLight.PlanLeft1 = int.Parse(LampLeftPlan1.Text);
                            infoLight.PlanLeft2 = int.Parse(LampLeftPlan2.Text);
                            infoLight.PlanLeft3 = int.Parse(LampLeftPlan3.Text);
                            infoLight.PlanLeft4 = int.Parse(LampLeftPlan4.Text);
                            infoLight.PlanRight1 = int.Parse(LampRightPlan1.Text);
                            infoLight.PlanRight2 = int.Parse(LampRightPlan2.Text);
                            infoLight.PlanRight3 = int.Parse(LampRightPlan3.Text);
                            infoLight.PlanRight4 = int.Parse(LampRightPlan4.Text);
                            infoCoordinate.OriginX = OriginDragFloorLogin.X;
                            infoCoordinate.OriginY = OriginDragFloorLogin.Y;
                            infoCoordinate.NLOriginX = OriginDragFloorNoLogin.X;
                            infoCoordinate.NLOriginY = OriginDragFloorNoLogin.Y;
                            infoCoordinate.TransformX = DragFloorLogin.X;
                            infoCoordinate.TransformY = DragFloorLogin.Y;
                            infoCoordinate.Location = CurrentSelectFloorLogin;

                            SaveLightPlan(infoLight, LampLeftPlan1.Text, LampLeftPlan2.Text, LampLeftPlan3.Text, LampLeftPlan4.Text, LampRightPlan1.Text, LampRightPlan2.Text, LampRightPlan3.Text, LampRightPlan4.Text);

                            ObjLight.Update(infoLight);
                            ObjCoordinate.Update(infoCoordinate);
                            CommonFunct.PopupWindow("设备信息保存成功！");
                            BlankIcon = new Image();
                            ClearAllElementLogin();
                            GetDataCurrentFloorLogin();
                            LoadIconSearchCodeLogin();
                            RefreshIconSearchCodeLogin();
                            stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                        }
                        else
                        {
                            //bool isSuccess = Protocol.QuerySingleLight(LampCode, EPSCode);//查询该EPS下是否有此灯具
                            //if (!isSuccess)
                            //{
                            //    CommonFunct.PopupWindow(string.Format("{0}配电箱下不存在{1}灯具", EPSCode, LampCode));
                            //}
                            //else
                            //{
                            //    Protocol.AddSingleLight(LampCode, EPSCode);//单灯添加

                            //    if (LstLight.FindAll(x => x.DisBoxID.ToString() == EPSCode).Count < 200)
                            //    {
                            LightInfo infoLight = new LightInfo
                            {
                                Code = LampCode,
                                Address = FacilityAddress.Text,
                                ErrorTime = string.Empty,
                                PlanLeft1 = int.Parse(LampLeftPlan1.Text),
                                PlanLeft2 = int.Parse(LampLeftPlan2.Text),
                                PlanLeft3 = int.Parse(LampLeftPlan3.Text),
                                PlanLeft4 = int.Parse(LampLeftPlan4.Text),
                                PlanLeft5 = 0,
                                PlanRight1 = int.Parse(LampRightPlan1.Text),
                                PlanRight2 = int.Parse(LampRightPlan2.Text),
                                PlanRight3 = int.Parse(LampRightPlan3.Text),
                                PlanRight4 = int.Parse(LampRightPlan4.Text),
                                PlanRight5 = 0,
                                LightClass = Convert.ToInt32(LampCode.Substring(0, 1)),
                                DisBoxID = int.Parse(EPSCode),
                                LightIndex = LstLight.FindAll(x => x.DisBoxID == int.Parse(EPSCode)).Count,
                                Shield = 0
                            };

                            if (Convert.ToInt32(infoLight.Code.Substring(0, 1)) == 5)
                            {
                                if (Convert.ToInt32(infoLight.Code.Substring(1, 1)) >= 5)
                                {
                                    infoLight.LightClass = 6;
                                }
                                else
                                {
                                    infoLight.LightClass = 5;
                                }
                            }

                            if (infoLight.LightClass == 1 || infoLight.LightClass == 3)
                            {
                                infoLight.CurrentState = (int)EnumClass.SingleLightCurrentState.全灭;
                            }
                            else
                            {
                                infoLight.CurrentState = (int)EnumClass.SingleLightCurrentState.全亮;
                            }
                            infoLight.BeginStatus = infoLight.Status = GetLightInitStatus(infoLight.LightClass);

                            CoordinateInfo infoCoordinate = new CoordinateInfo
                            {
                                TableName = EnumClass.TableName.Light.ToString(),
                                TableID = ObjLight.Add(infoLight),
                                Location = CurrentSelectFloorLogin,
                                OriginX = OriginDragFloorLogin.X,
                                OriginY = OriginDragFloorLogin.Y,
                                NLOriginX = OriginDragFloorNoLogin.X,
                                NLOriginY = OriginDragFloorNoLogin.Y,
                                TransformX = DragFloorLogin.X,
                                TransformY = DragFloorLogin.Y,
                                IsAuth = 0
                            };
                            //SaveLightPlan(infoLight, this.LampLeftPlan1.Text, this.LampLeftPlan2.Text, this.LampLeftPlan3.Text, this.LampLeftPlan4.Text, this.LampRightPlan1.Text, this.LampRightPlan2.Text, this.LampRightPlan3.Text, this.LampRightPlan4.Text);
                            ObjCoordinate.Add(infoCoordinate);
                            LstCoordinate = ObjCoordinate.GetAll();
                            LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

                            ObjBlankIcon.Delete(Convert.ToInt32(BlankIconID.Content.ToString()));
                            LstBlankIcon = ObjBlankIcon.GetAll();

                            LstLight = ObjLight.GetAll();
                            LstLight.Sort();
                            CommonFunct.PopupWindow("设备信息保存成功！");
                            BlankIcon = new Image();
                            ClearAllElementLogin();
                            GetDataCurrentFloorLogin();
                            LoadIconSearchCodeLogin();
                            RefreshIconSearchCodeLogin();
                            //}
                            //else
                            //{
                            //    CommonFunct.PopupWindow("该配电箱下灯具已超200！！");
                            //}
                            stpLayerEdit.Visibility = System.Windows.Visibility.Hidden;
                            //}
                        }
                    }
                }
            }
        }
        private void CurrentFloorCho_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radCurrentFloor.IsChecked = true;
        }

        private void AllFloorCho_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radAllFloor.IsChecked = true;
        }

        /// <summary>
        /// 显示总楼层设置页面
        /// </summary>
        private void ShowFloorTotalSetUp()
        {
            PointEdit.Visibility = System.Windows.Visibility.Hidden;
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "ConstructionFloor");
            FloorTotalNum.Text = infoGblSetting.SetValue;
            GraphicalTotalFloorNum.Visibility = System.Windows.Visibility.Visible;

            ctcFloorDrawingLogin.MouseLeftButtonDown -= GetCoordinates_MouseDown;
        }

        private void btnTotalFloorCancel_Click(object sender, RoutedEventArgs e)
        {
            GraphicalTotalFloorNum.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnKeyBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FocusTextBox = FloorTotalNum;
            ShowSystemKeyBoard();
        }

        private void btnTotalFloorOK_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = ModifyTotalFloor();
            if (isSuccess)
            {
                GraphicalTotalFloorNum.Visibility = System.Windows.Visibility.Hidden;
                InitCurrengPageFloorNoLogin();
                SwitchFloorLogin();
            }
        }

        private void imgUpLogin_MouseDown(object sender, RoutedEventArgs e)
        {
            bool isSuccess = LastPageFloorLogin();
            if (isSuccess)
            {
                SwitchFloorLogin();
            }
        }

        private void imgDownLogin_MouseDown(object sender, RoutedEventArgs e)
        {
            bool isSuccess = NextPageFloorLogin();
            if (isSuccess)
            {
                SwitchFloorLogin();
            }
        }

        private void btnPageFirst_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectFloorLogin = 1;
            SwitchFloorLogin();
        }

        private void btnPageLast_Click(object sender, RoutedEventArgs e)
        {
            CurrentSelectFloorLogin = TotalFloor;
            SwitchFloorLogin();
        }

        private void btnKeyBoardNoLogin_Click(object sender, RoutedEventArgs e)
        {
            FocusTextBox = tbxJumpSelectFloorNoLogin;
            ShowSystemKeyBoard();
        }

        private void btnLayerNextPageNoLogin_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = NextPageFloorNoLogin();
            if (isSuccess)
            {
                FloorPage.Content = CurrentSelectFloorNoLogin;
                SwitchFloorNoLogin();
            }
        }

        private void btnLayerBeforePageNoLogin_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = LastPageFloorNoLogin();
            if (isSuccess)
            {
                FloorPage.Content = CurrentSelectFloorNoLogin;
                SwitchFloorNoLogin();
            }
        }

        private void btnPageLastNoLogin_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageFloorNoLogin = int.Parse(FloorTotalNoLogin.Content.ToString());
            CurrentSelectFloorNoLogin = CurrentPageFloorNoLogin + "层";
            FloorPage.Content = CurrentSelectFloorNoLogin;
            SwitchFloorNoLogin();
        }

        private void btnPageFirstNoLogin_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageFloorNoLogin = 1;
            CurrentSelectFloorNoLogin = CurrentPageFloorNoLogin + "层";
            FloorPage.Content = CurrentSelectFloorNoLogin;
            SwitchFloorNoLogin();
        }

        private void btnPointDel_Click(object sender, RoutedEventArgs e)
        {
            int index = PointNum.SelectedIndex;

            if (index >= 0)
            {
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes.Find(y => LstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == y.ID).Location == CurrentSelectFloorLogin && y.TurnIndex == index).ID);
                for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                {
                    if ((LstEscapeLinesCurrentFloorLogin[i].LineX1 == infoCoordinate.OriginX && LstEscapeLinesCurrentFloorLogin[i].LineY1 == infoCoordinate.OriginY) || (LstEscapeLinesCurrentFloorLogin[i].LineX2 == infoCoordinate.OriginX && LstEscapeLinesCurrentFloorLogin[i].LineY2 == infoCoordinate.OriginY))
                    {
                        ObjEscapeLines.Delete(LstEscapeLinesCurrentFloorLogin[i].ID);
                    }
                }
                LstEscapeLines = ObjEscapeLines.GetAll();
                LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);

                ObjEscapeRoutes.Delete(infoCoordinate.TableID);
                LstEscapeRoutes = ObjEscapeRoutes.GetAll();

                ObjCoordinate.Delete(infoCoordinate.ID);
                LstCoordinate = ObjCoordinate.GetAll();
                LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                ShowEscapePointIndex();
                RefreshLayerModeLogin(true, true);
            }
            else
            {
                CommonFunct.PopupWindow("不存在逃生路线转折点！");
            }
        }

        private void btnPointDelAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult MessageBoxResult = System.Windows.MessageBox.Show("你是否需要清除当前楼层的路线？"
                , "提示", MessageBoxButton.YesNo);
            if (MessageBoxResult == MessageBoxResult.Yes)
            {
                PointNum.ItemsSource = null;
                List<CoordinateInfo> LstCoordinateER = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == CurrentSelectFloorLogin);
                for (int i = 0; i < LstCoordinateER.Count; i++)
                {
                    ObjEscapeRoutes.Delete(LstCoordinateER[i].TableID);
                    ObjCoordinate.Delete(LstCoordinateER[i].ID);
                }
                for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                {
                    ObjEscapeLines.Delete(LstEscapeLinesCurrentFloorLogin[i].ID);
                }
                LstEscapeRoutes = ObjEscapeRoutes.GetAll();

                LstEscapeLines = ObjEscapeLines.GetAll();
                LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);

                LstCoordinate = ObjCoordinate.GetAll();
                LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);
                RefreshEscapeRoutesLogin();
            }
        }

        private void btnPointOK_Click(object sender, RoutedEventArgs e)
        {
            if (PointNum.SelectedIndex >= 0)
            {
                ObservableCollection<int> ImgIndex = new ObservableCollection<int>();//记录已经连接的转折点的下标
                ObservableCollection<int> UnImgIndex = new ObservableCollection<int>();//记录未连线的转折点的下标
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes.Find(y => LstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == y.ID).Location == CurrentSelectFloorLogin && y.TurnIndex == (int)PointNum.SelectedValue).ID);
                CurrentChoPoint.X = infoCoordinate.OriginX;
                CurrentChoPoint.Y = infoCoordinate.OriginY;

                Connected.ItemsSource = null;
                Offline.ItemsSource = null;

                ShowEscapeRoutes(CurrentChoPoint);
            }
            else
            {
                CommonFunct.PopupWindow("不存在逃生路线转折点！");
            }

        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            if (Offline.SelectedItem != null)
            {
                int index = (int)Offline.SelectedItem;
                Point CurrentPoint = CurrentChoPoint;
                Point OtherOldPoint = new Point();
                Point OtherNewPoint = new Point();
                List<EscapeLinesInfo> LstChangeLines = new List<EscapeLinesInfo>();
                List<EscapeLinesInfo> LstGivenLines = LstEscapeLinesCurrentFloorLogin.FindAll(x => (x.LineX1 == CurrentChoPoint.X && x.LineY1 == CurrentChoPoint.Y) || (x.LineX2 == CurrentChoPoint.X && x.LineY2 == CurrentChoPoint.Y));//获取与所选的转折点相关的线段
                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes.Find(y => LstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == y.ID).Location == CurrentSelectFloorLogin && y.TurnIndex == (int)Offline.SelectedItem).ID);
                OtherOldPoint.X = infoCoordinate.OriginX;
                OtherOldPoint.Y = infoCoordinate.OriginY;
                OtherNewPoint = OtherOldPoint;

                double distanceX = CurrentPoint.X - OtherOldPoint.X;
                double distanceY = CurrentPoint.Y - OtherOldPoint.Y;
                if (System.Math.Abs(distanceX) < System.Math.Abs(distanceY))
                {
                    if (Math.Abs(distanceX) < 10)
                    {
                        //所选的转折点未连线，或者所选转折点所连的线段与将要连线的线段互相垂直（不影响横或竖线） ，直接修改所选转折点的坐标
                        if (LstGivenLines.Count == 0 || LstGivenLines.FindAll(x => x.LineX1 == x.LineX2).Count == 0)
                        {
                            CurrentPoint.X = OtherOldPoint.X;
                            CurrentPoint.Y = CurrentChoPoint.Y;
                        }
                        if (LstGivenLines.FindAll(x => x.LineX1 == x.LineX2).Count != 0)//所选的转折点所连的线段中存在与将要连线的线段平行的线段 ,改变将要连线的另外一个点的坐标
                        {
                            OtherNewPoint.X = CurrentChoPoint.X;
                            OtherNewPoint.Y = OtherOldPoint.Y;

                            //EscapeRoutesInfo infoOldEscapeRoute = new EscapeRoutesInfo();
                            //EscapeRoutesInfo infoNewEscapeRoute = new EscapeRoutesInfo();

                            //infoOldEscapeRoute.CoordinateX = OtherOldPoint.X;
                            //infoOldEscapeRoute.CoordinateY = OtherOldPoint.Y;
                            //infoNewEscapeRoute.CoordinateX = OtherNewPoint.X;
                            //infoNewEscapeRoute.CoordinateY = OtherNewPoint.Y;

                            CoordinateInfo infoOldCoordinate = new CoordinateInfo();
                            CoordinateInfo infoNewCoordinate = new CoordinateInfo();

                            infoOldCoordinate.OriginX = OtherOldPoint.X;
                            infoOldCoordinate.OriginY = OtherOldPoint.Y;
                            infoNewCoordinate.OriginX = OtherNewPoint.X;
                            infoNewCoordinate.OriginY = OtherNewPoint.Y;

                            ChangeEscapeRoutes(infoOldCoordinate, infoNewCoordinate);
                        }
                    }
                }
                else
                {
                    if (Math.Abs(distanceY) < 10)
                    {
                        if (LstGivenLines.Count == 0 || LstGivenLines.FindAll(x => x.LineY1 == x.LineY2).Count == 0)
                        {
                            CurrentPoint.X = CurrentChoPoint.X;
                            CurrentPoint.Y = OtherOldPoint.Y;
                        }
                        if (LstGivenLines.FindAll(x => x.LineY1 == x.LineY2).Count != 0)
                        {
                            OtherNewPoint.X = OtherOldPoint.X;
                            OtherNewPoint.Y = CurrentChoPoint.Y;

                            //EscapeRoutesInfo infoOldEscapeRoute = new EscapeRoutesInfo();
                            //EscapeRoutesInfo infoNewEscapeRoute = new EscapeRoutesInfo();

                            //infoOldEscapeRoute.CoordinateX = OtherOldPoint.X;
                            //infoOldEscapeRoute.CoordinateY = OtherOldPoint.Y;
                            //infoNewEscapeRoute.CoordinateX = OtherNewPoint.X;
                            //infoNewEscapeRoute.CoordinateY = OtherNewPoint.Y;

                            CoordinateInfo infoOldCoordinate = new CoordinateInfo();
                            CoordinateInfo infoNewCoordinate = new CoordinateInfo();

                            infoOldCoordinate.OriginX = OtherOldPoint.X;
                            infoOldCoordinate.OriginY = OtherOldPoint.Y;
                            infoNewCoordinate.OriginX = OtherNewPoint.X;
                            infoNewCoordinate.OriginY = OtherNewPoint.Y;

                            ChangeEscapeRoutes(infoOldCoordinate, infoNewCoordinate);
                        }
                    }
                }

                if (CurrentChoPoint != CurrentPoint)
                {
                    Point NLPoint = ComputePointNoLogin(CurrentPoint);
                    //EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutesCurrentFloorLogin.Find(x => x.CoordinateX == CurrentChoPoint.X && x.CoordinateY == CurrentChoPoint.Y);
                    CoordinateInfo infoCod = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == CurrentSelectFloorLogin && x.OriginX == CurrentChoPoint.X && x.OriginY == CurrentChoPoint.Y);
                    if (infoCod != null)
                    {
                        infoCod.OriginX = CurrentPoint.X;
                        infoCod.OriginY = CurrentPoint.Y;
                        infoCod.TransformX = infoCod.OriginX;
                        infoCod.TransformY = infoCod.OriginY;
                        infoCod.NLOriginX = NLPoint.X;
                        infoCod.NLOriginY = NLPoint.Y;
                        //ObjEscapeRoutes.Update(infoEscapeRoutes);
                        ObjCoordinate.Update(infoCod);
                    }

                    for (int i = 0; i < LstGivenLines.Count; i++)
                    {
                        if (LstGivenLines[i].LineX1 == CurrentChoPoint.X && LstGivenLines[i].LineY1 == CurrentChoPoint.Y)
                        {
                            LstGivenLines[i].LineX1 = CurrentPoint.X;
                            LstGivenLines[i].LineY1 = CurrentPoint.Y;
                            LstGivenLines[i].NLineX1 = NLPoint.X;
                            LstGivenLines[i].NLineY1 = NLPoint.Y;
                        }
                        if (LstGivenLines[i].LineX2 == CurrentChoPoint.X && LstGivenLines[i].LineY2 == CurrentChoPoint.Y)
                        {
                            LstGivenLines[i].LineX2 = CurrentPoint.X;
                            LstGivenLines[i].LineY2 = CurrentPoint.Y;
                            LstGivenLines[i].NLineX2 = NLPoint.X;
                            LstGivenLines[i].NLineY2 = NLPoint.Y;
                        }
                        ObjEscapeLines.Update(LstGivenLines[i]);
                    }
                    CurrentChoPoint = CurrentPoint;
                }
                if (OtherNewPoint != OtherOldPoint)
                {
                    Point NLPoint = ComputePointNoLogin(OtherNewPoint);
                    //EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutesCurrentFloorLogin.Find(x => x.CoordinateX == OtherOldPoint.X && x.CoordinateY == OtherOldPoint.Y);
                    CoordinateInfo infoCord = LstCoordinate.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.OriginX == OtherOldPoint.X && x.OriginY == OtherOldPoint.Y);
                    LstGivenLines = LstEscapeLinesCurrentFloorLogin.FindAll(x => (x.LineX1 == infoCord.OriginX && x.LineY1 == infoCord.OriginY) || (x.LineX2 == infoCord.OriginX && x.LineY2 == infoCord.OriginY));

                    if (infoCord != null)
                    {
                        infoCord.OriginX = OtherNewPoint.X;
                        infoCord.OriginY = OtherNewPoint.Y;
                        infoCord.TransformX = infoCord.OriginX;
                        infoCord.TransformY = infoCord.OriginY;
                        infoCord.NLOriginX = NLPoint.X;
                        infoCord.NLOriginY = NLPoint.Y;
                        //ObjEscapeRoutes.Update(infoEscapeRoutes);
                        ObjCoordinate.Update(infoCord);
                    }

                    for (int i = 0; i < LstGivenLines.Count; i++)
                    {
                        if (LstGivenLines[i].LineX1 == infoCord.OriginX && LstGivenLines[i].LineY1 == infoCord.OriginY)
                        {
                            LstGivenLines[i].LineX1 = infoCord.OriginX;
                            LstGivenLines[i].LineY1 = infoCord.OriginY;
                            LstGivenLines[i].NLineX1 = NLPoint.X;
                            LstGivenLines[i].NLineY1 = NLPoint.Y;
                        }
                        if (LstGivenLines[i].LineX2 == CurrentChoPoint.X && LstGivenLines[i].LineY2 == CurrentChoPoint.Y)
                        {
                            LstGivenLines[i].LineX2 = infoCord.OriginX;
                            LstGivenLines[i].LineY2 = infoCord.OriginY;
                            LstGivenLines[i].NLineX2 = NLPoint.X;
                            LstGivenLines[i].NLineY2 = NLPoint.Y;
                        }
                        ObjEscapeLines.Update(LstGivenLines[i]);
                    }

                }

                int subscript;//路线线段的序号
                EscapeLinesInfo infoEscapeLines = new EscapeLinesInfo();
                if (LstEscapeLinesCurrentFloorLogin.Count != 0)
                {
                    //subscript = int.Parse(LstEscapeLinesCurrentFloorLogin[LstEscapeLinesCurrentFloorLogin.Count - 1].Name.Substring(LstEscapeLinesCurrentFloorLogin[LstEscapeLinesCurrentFloorLogin.Count - 1].Name.Length - 1, 1)) + 1;
                    subscript = int.Parse(System.Text.RegularExpressions.Regex.Replace(LstEscapeLinesCurrentFloorLogin[LstEscapeLinesCurrentFloorLogin.Count - 1].Name, @"[^0-9]+", "")) + 1;
                }
                else
                {
                    subscript = 1;
                }

                Point NLPoint1 = ComputePointNoLogin(OtherNewPoint);
                Point NLPoint2 = ComputePointNoLogin(CurrentPoint);
                infoEscapeLines.Name = "Line" + subscript.ToString();
                infoEscapeLines.Location = CurrentSelectFloorLogin;
                infoEscapeLines.LineX1 = OtherNewPoint.X;
                infoEscapeLines.LineY1 = OtherNewPoint.Y;
                infoEscapeLines.LineX2 = CurrentPoint.X;
                infoEscapeLines.LineY2 = CurrentPoint.Y;
                infoEscapeLines.NLineX1 = NLPoint1.X;
                infoEscapeLines.NLineY1 = NLPoint1.Y;
                infoEscapeLines.NLineX2 = NLPoint2.X;
                infoEscapeLines.NLineY2 = NLPoint2.Y;
                infoEscapeLines.TransformX1 = infoEscapeLines.LineX1;
                infoEscapeLines.TransformX2 = infoEscapeLines.LineX2;
                infoEscapeLines.TransformY1 = infoEscapeLines.LineY1;
                infoEscapeLines.TransformY2 = infoEscapeLines.LineY2;
                ObjEscapeLines.Add(infoEscapeLines);
                LstEscapeLines = ObjEscapeLines.GetAll();
                LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);

                LstEscapeRoutes = ObjEscapeRoutes.GetAll();
                //LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => x.Location == CurrentSelectFloorLogin);

                LstCoordinate = ObjCoordinate.GetAll();
                LstCoordinateCurrentFloorLogin = LstCoordinate.FindAll(x => x.Location == CurrentSelectFloorLogin);

                Point Transform1 = GetPointTransform(OtherNewPoint);
                Point Transform2 = GetPointTransform(CurrentPoint);

                printing(Transform1.X, Transform2.X, Transform1.Y, Transform2.Y);


                Connected.ItemsSource = null;
                Offline.ItemsSource = null;
                UpdateScaleTransformPositionLogin();
                RefreshEscapeRoutesLogin();
                ShowEscapeRoutes(CurrentChoPoint);
            }
            else
            {
                CommonFunct.PopupWindow("请选择需要连线的转折点！");
            }
        }

        private Point GetPointTransform(Point infoEscapeRoute)
        {
            Point PointDrawing = new Point(infoEscapeRoute.X - StartPositionDragFloor.X, infoEscapeRoute.Y - StartPositionDragFloor.Y);
            PointDrawing = new Point(Math.Round(PointDrawing.X), Math.Round(PointDrawing.Y));

            Point PointScaleTransform = TransformGroupLogin.Transform(PointDrawing);
            PointScaleTransform = new Point(Math.Round(PointScaleTransform.X), Math.Round(PointScaleTransform.Y));

            Point Transform = new Point
            {
                X = PointScaleTransform.X + StartPositionDragFloor.X - (IconSearchRouteCodeSize - OriginIconSearchRouteCodeSize) / FixedScaleTransform,
                Y = PointScaleTransform.Y + StartPositionDragFloor.Y - (IconSearchRouteCodeSize - OriginIconSearchRouteCodeSize) / FixedScaleTransform
            };

            return Transform;
        }

        private void ChangeEscapeRoutes(CoordinateInfo OldinfoEscapeRoutes, CoordinateInfo NewinfoEscapeRoutes)
        {
            List<EscapeLinesInfo> LstEscapeLine = new List<EscapeLinesInfo>();//转折点相应的线段


            LstEscapeLine = LstEscapeLinesCurrentFloorLogin.FindAll(x => (x.LineX1 == OldinfoEscapeRoutes.OriginX && x.LineY1 == OldinfoEscapeRoutes.OriginY) || (x.LineX2 == OldinfoEscapeRoutes.OriginX && x.LineY2 == OldinfoEscapeRoutes.OriginY));

            Point oldCurrPoint = new Point();
            Point NewCurrPoint = new Point();
            for (int i = 0; i < LstEscapeLine.Count; i++)
            {
                //获取线段的另一个端点oldCurrPoint
                if (LstEscapeLine[i].LineX1 == OldinfoEscapeRoutes.OriginX && LstEscapeLine[i].LineY1 == OldinfoEscapeRoutes.OriginY)
                {
                    oldCurrPoint.X = LstEscapeLine[i].LineX2;
                    oldCurrPoint.Y = LstEscapeLine[i].LineY2;
                }
                else
                {
                    oldCurrPoint.X = LstEscapeLine[i].LineX1;
                    oldCurrPoint.Y = LstEscapeLine[i].LineY1;
                }

                List<EscapeLinesInfo> LstOldLines = LstEscapeLinesCurrentFloorLogin.FindAll(x => (x.LineX1 == oldCurrPoint.X && x.LineY1 == oldCurrPoint.Y) || (x.LineX2 == oldCurrPoint.X && x.LineY2 == oldCurrPoint.Y));

                double distanceX = NewinfoEscapeRoutes.OriginX - oldCurrPoint.X;
                double distanceY = NewinfoEscapeRoutes.OriginY - oldCurrPoint.Y;
                if (System.Math.Abs(distanceX) < System.Math.Abs(distanceY))
                {
                    //判断前一个点是否跟其他转折点连上线段，存在两条线段以上则改变当前转折点的位置与前一个点达成横线或者竖线

                    if (Math.Abs(distanceX) < 10)
                    {
                        NewCurrPoint.X = NewinfoEscapeRoutes.OriginX;
                        NewCurrPoint.Y = oldCurrPoint.Y;
                    }
                }
                else
                {
                    if (Math.Abs(distanceY) < 10)
                    {
                        NewCurrPoint.X = oldCurrPoint.X;
                        NewCurrPoint.Y = NewinfoEscapeRoutes.OriginY;
                    }
                }

                //EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutesCurrentFloorLogin.Find(x => x.CoordinateX == oldCurrPoint.X && x.CoordinateY == oldCurrPoint.Y);
                //EscapeRoutesInfo infoNewEscapeRoute = infoEscapeRoutes;

                CoordinateInfo infoCoordinate = LstCoordinate.Find(x => x.Location == CurrentSelectFloorLogin && x.OriginX == oldCurrPoint.X && x.OriginY == oldCurrPoint.Y);
                CoordinateInfo infoNewCoordinate = infoCoordinate;

                if (infoCoordinate != null)
                {
                    infoNewCoordinate.OriginX = NewCurrPoint.X;
                    infoNewCoordinate.OriginY = NewCurrPoint.Y;
                    //ObjEscapeRoutes.Update(infoNewEscapeRoute);
                    ObjCoordinate.Update(infoNewCoordinate);

                    if (LstOldLines.FindAll(x => (x.LineX1 != OldinfoEscapeRoutes.OriginX || x.LineY1 != OldinfoEscapeRoutes.OriginY) && (x.LineX2 != OldinfoEscapeRoutes.OriginX || x.LineY2 != OldinfoEscapeRoutes.OriginY)).Count != 0)
                    {
                        //ChangeEscapeRoutes(infoEscapeRoutes, infoNewEscapeRoute);
                        ChangeEscapeRoutes(infoCoordinate, infoNewCoordinate);
                    }
                }

                if (LstEscapeLine[i].LineX1 == OldinfoEscapeRoutes.OriginX && LstEscapeLine[i].LineY1 == OldinfoEscapeRoutes.OriginY)
                {
                    LstEscapeLine[i].LineX1 = NewinfoEscapeRoutes.OriginX;
                    LstEscapeLine[i].LineY1 = NewinfoEscapeRoutes.OriginY;
                    LstEscapeLine[i].LineX2 = NewCurrPoint.X;
                    LstEscapeLine[i].LineY2 = NewCurrPoint.Y;
                }
                if (LstEscapeLine[i].LineX2 == OldinfoEscapeRoutes.OriginX && LstEscapeLine[i].LineY2 == OldinfoEscapeRoutes.OriginY)
                {
                    LstEscapeLine[i].LineX1 = NewCurrPoint.X;
                    LstEscapeLine[i].LineY1 = NewCurrPoint.Y;
                    LstEscapeLine[i].LineX2 = NewinfoEscapeRoutes.OriginX;
                    LstEscapeLine[i].LineY2 = NewinfoEscapeRoutes.OriginY;
                }
                ObjEscapeLines.Update(LstEscapeLine[i]);
            }

        }

        private void ShowEscapeRoutes(Point point)
        {
            ObservableCollection<int> ImgIndex = new ObservableCollection<int>();//记录已经连接的转折点的下标
            ObservableCollection<int> UnImgIndex = new ObservableCollection<int>();//记录未连线的转折点的下标
            LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);

            int LinesCount = LstEscapeLinesCurrentFloorLogin.FindAll(x => (x.LineX1 == point.X && x.LineY1 == point.Y) || (x.LineX2 == point.X && x.LineY2 == point.Y)).Count;
            List<CoordinateInfo> LstCordEscape = LstCoordinate.FindAll(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.Location == CurrentSelectFloorLogin);
            if (LinesCount == 0)
            {
                Connected.IsEnabled = false;
                for (int i = 0; i < LstCordEscape.Count; i++)
                {
                    if (LstCordEscape[i].OriginX != point.X || LstCordEscape[i].OriginY != point.Y)
                    {
                        UnImgIndex.Add(i + 1);
                    }
                }
                Offline.ItemsSource = UnImgIndex;
                Offline.SelectedIndex = 0;
                Offline.IsEnabled = true;
            }
            else
            {
                Connected.IsEnabled = true;
                if (LinesCount == LstCordEscape.Count)
                {
                    for (int i = 0; i < LstCordEscape.Count; i++)
                    {
                        if (LstCordEscape[i].OriginX != point.X || LstCordEscape[i].OriginY != point.Y)
                        {
                            ImgIndex.Add(i + 1);
                        }
                    }
                    Connected.ItemsSource = ImgIndex;
                    Connected.SelectedIndex = 0;
                    Offline.IsEnabled = false;
                }
                else
                {
                    Point OldPoint = new Point();
                    List<Point> PointIndex = new List<Point>();
                    for (int i = 0; i < LstEscapeLinesCurrentFloorLogin.Count; i++)
                    {
                        if (LstEscapeLinesCurrentFloorLogin[i].LineX1 == point.X && LstEscapeLinesCurrentFloorLogin[i].LineY1 == point.Y)
                        {
                            OldPoint.X = LstEscapeLinesCurrentFloorLogin[i].LineX2;
                            OldPoint.Y = LstEscapeLinesCurrentFloorLogin[i].LineY2;
                            PointIndex.Add(OldPoint);
                        }
                        if (LstEscapeLinesCurrentFloorLogin[i].LineX2 == point.X && LstEscapeLinesCurrentFloorLogin[i].LineY2 == point.Y)
                        {
                            OldPoint.X = LstEscapeLinesCurrentFloorLogin[i].LineX1;
                            OldPoint.Y = LstEscapeLinesCurrentFloorLogin[i].LineY1;
                            PointIndex.Add(OldPoint);
                        }
                    }

                    for (int i = 0; i < LstCordEscape.Count; i++)
                    {
                        OldPoint.X = LstCordEscape[i].OriginX;
                        OldPoint.Y = LstCordEscape[i].OriginY;
                        if (PointIndex.Contains(OldPoint))
                        {
                            ImgIndex.Add(i + 1);
                        }
                        else
                        {
                            if (LstCordEscape[i].OriginX != point.X || LstCordEscape[i].OriginY != point.Y)
                            {
                                UnImgIndex.Add(i + 1);
                            }
                        }
                    }

                    Connected.ItemsSource = ImgIndex;
                    Connected.SelectedIndex = 0;
                    Offline.ItemsSource = UnImgIndex;
                    Offline.SelectedIndex = 0;
                    Offline.IsEnabled = true;
                }
            }
            //this.EditLine.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnDeleteLine_Click(object sender, RoutedEventArgs e)
        {
            if (Connected.SelectedItem != null)
            {
                if (int.Parse(Connected.SelectedItem.ToString()) > 0)
                {
                    //EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutesCurrentFloorLogin[int.Parse(this.Connected.SelectedItem.ToString()) - 1];
                    CoordinateInfo infoCoordinate = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes.Find(y => LstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == y.ID).Location == CurrentSelectFloorLogin && y.TurnIndex == int.Parse(Connected.SelectedItem.ToString()) - 1).ID);
                    //EscapeRoutesInfo infoChangePoint = LstEscapeRoutesCurrentFloorLogin[this.PointNum.SelectedIndex];
                    CoordinateInfo infoChangePoint = LstCoordinateCurrentFloorLogin.Find(x => x.TableName == EnumClass.TableName.EscapeRoutes.ToString() && x.TableID == LstEscapeRoutes.Find(y => LstCoordinate.Find(z => z.TableName == EnumClass.TableName.EscapeRoutes.ToString() && z.TableID == y.ID).Location == CurrentSelectFloorLogin && y.TurnIndex == PointNum.SelectedIndex).ID);

                    EscapeLinesInfo infoEscapeLine = LstEscapeLinesCurrentFloorLogin.Find(x => (x.LineX1 == infoChangePoint.OriginX && x.LineY1 == infoChangePoint.OriginY && x.LineX2 == infoCoordinate.OriginX && x.LineY2 == infoCoordinate.OriginY) || (x.LineX1 == infoCoordinate.OriginX && x.LineY1 == infoCoordinate.OriginY && x.LineX2 == infoChangePoint.OriginX && x.LineY2 == infoChangePoint.OriginY));

                    LstEscapeLines.Remove(infoEscapeLine);
                    LstEscapeLinesCurrentFloorLogin.Remove(infoEscapeLine);
                    ObjEscapeLines.Delete(infoEscapeLine.ID);
                    RefreshEscapeRoutesLogin();

                    Connected.ItemsSource = null;
                    Offline.ItemsSource = null;
                    ShowEscapeRoutes(CurrentChoPoint);
                }
                else
                {
                    CommonFunct.PopupWindow("不存在此路线！");
                }
            }
            else
            {
                CommonFunct.PopupWindow("不存在此路线！");
            }
        }

        private void btnAddExitPoint_Click(object sender, RoutedEventArgs e)
        {
            if (PointNum.ItemsSource != null)
            {
                EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutes.Find(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.EscapeRoutes.ToString() && y.TableID == x.ID) != null && x.TurnIndex == (int)PointNum.SelectedValue);
                if (infoEscapeRoutes != null)
                {
                    infoEscapeRoutes.EndPoint = 1;
                    ObjEscapeRoutes.Update(infoEscapeRoutes);
                    LstEscapeRoutes = ObjEscapeRoutes.GetAll();

                    ObservableCollection<int> PointIndex = new ObservableCollection<int>();
                    LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.EscapeRoutes.ToString() && y.TableID == x.ID) != null);
                    for (int i = 0; i < LstEscapeRoutesCurrentFloorLogin.Count; i++)
                    {
                        if (LstEscapeRoutesCurrentFloorLogin[i].EndPoint == 1)
                        {
                            PointIndex.Add(i + 1);
                        }
                    }
                    EXITIndex.ItemsSource = PointIndex;
                    EXITIndex.SelectedIndex = 0;
                }
            }
            else
            {

            }
        }

        private void btnDelExitPoint_Click(object sender, RoutedEventArgs e)
        {
            if (EXITIndex.SelectedItem != null)
            {
                EscapeRoutesInfo infoEscapeRoutes = LstEscapeRoutes.Find(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.EscapeRoutes.ToString() && y.TableID == x.ID) != null && x.TurnIndex == (int)EXITIndex.SelectedItem - 1);

                if (infoEscapeRoutes != null)
                {
                    infoEscapeRoutes.EndPoint = 0;
                    ObjEscapeRoutes.Update(infoEscapeRoutes);
                    LstEscapeRoutes = ObjEscapeRoutes.GetAll();

                    ObservableCollection<int> PointIndex = new ObservableCollection<int>();
                    for (int i = 0; i < LstEscapeRoutesCurrentFloorLogin.Count; i++)
                    {
                        if (LstEscapeRoutesCurrentFloorLogin[i].EndPoint == 1)
                        {
                            PointIndex.Add(i + 1);
                        }
                    }
                    EXITIndex.ItemsSource = PointIndex;
                    EXITIndex.SelectedIndex = 0;
                }
            }
        }

        private void btnAnalogLinkage_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentProgressBarValue == 0.0)
            {
                IsShowDirection = true;
                SliderControlLogin.sdFloorDrawing.Value = 1;
                SliderControlLogin.IsEnabled = false;
                //RefreshLayerModeLogin(true, true);//刷新图层楼层
                InitPartitionLogin();
                LoadEscapeRoutesLogin();
                ShowDirection();

                IsSimulationLinkage = true;
                IsLinkageTiming = true;

                LstDistributionBox.ForEach(x => x.IsEmergency = 1);
                LstLight.ForEach(x => x.IsEmergency = 1);
                ObjDistributionBox.Save(LstDistributionBox);
                ObjLight.Save(LstLight);
                SetSimulateFireAlarmLinkExePlanTimer(true);
                SetIsAllMainEle(false);
                SimulateFireAlarmLinkZoneNumber = 5;
                RecordSimulateFireAlarmLinkHistory();
                ClearAllEmergencyCalcuTime();
                SimulateFireAlarmLinkExePlan();
            }
            else
            {
                CommonFunct.PopupWindow("系统复位未结束，请稍后再重试");
                Thread.Sleep(3000);
            }
            
            //Thread.Sleep(9000);
            //GetEdition(IsCommodity);
        }

        private void ShowDirection()
        {
            if (ResPlanNum.SelectedItem != null)
            {
                string item = ResPlanNum.SelectedItem.ToString();

                if (item == "主联动")
                {
                    LstPartitionPointCurrentFloorLogin = LstPlanPartitionPointRecord.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && y.TableID == x.ID) != null);
                }
                else
                {
                    LstPartitionPointCurrentFloorLogin = LstPlanPartitionPointRecord.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.PlanPartitionPointRecord.ToString() && y.TableID == x.ID) != null && x.PlanPartition == int.Parse(item));
                }
                LstEscapeLinesCurrentFloorLogin = LstEscapeLines.FindAll(x => x.Location == CurrentSelectFloorLogin);
                LstEscapeRoutesCurrentFloorLogin = LstEscapeRoutes.FindAll(x => LstCoordinateCurrentFloorLogin.Find(y => y.TableName == EnumClass.TableName.EscapeRoutes.ToString() && y.TableID == x.ID) != null);
                int h = cvsMainWindow.Children.Count;

                if (LstPartitionPointCurrentFloorLogin.Count != 0 && LstEscapeRoutesCurrentFloorLogin.Count != 0 && LstEscapeLinesCurrentFloorLogin.Count != 0)
                {
                    GenerateRoutes GR = new GenerateRoutes();
                    List<GenerateRoutes.AlarmPoint> FootDropLines = new List<GenerateRoutes.AlarmPoint>();
                    FootDropLines = GR.GetFootDropLine(LstPartitionPointCurrentFloorLogin, LstEscapeRoutesCurrentFloorLogin, LstEscapeLinesCurrentFloorLogin, LstCoordinateCurrentFloorLogin, false);
                    GR.GetInformation(CurrentSelectFloorLogin, LstEscapeLinesCurrentFloorLogin, LstEscapeRoutesCurrentFloorLogin, LstCoordinateCurrentFloorLogin, LstPartitionPointCurrentFloorLogin, FootDropLines, false, this);
                    GC.Collect();
                }
            }

        }

        private void btnResetSystem_Click(object sender, RoutedEventArgs e)
        {
            RSNoLogin();
            //LstOldZoneNumber.Clear();
        }

        private void btnAllEmergencyInLayer_Click(object sender, RoutedEventArgs e)
        {
            AllEmergencyNoLogin();
        }

        private void btnExitSystem_Click(object sender, MouseButtonEventArgs e)
        {
            bool isSuccess = ShowExit();
            if (isSuccess)
            {
                ObjFaultRecord.DeleteAll();//清除系统所有故障记录
                Environment.Exit(0);
            }
            else
            {
                if (stpLayerModeNoLogin.Visibility == System.Windows.Visibility.Hidden)
                {
                    MasterController.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    stpLayerModeNoLogin.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void SetPassword(object sender, RoutedEventArgs e)
        {
            SetPassword(sender as Button);
        }

        private void DelPassword(object sender, RoutedEventArgs e)
        {
            if (FocusPasswordBox != null)
            {
                FocusPasswordBox.Clear();
            }
        }

        private void DetermineModify(object sender, RoutedEventArgs e)
        {
            if (OldPassword.Password == string.Empty)
            {
                OldPassword.Focus();
                MessageBox.Show("请输入原始密码！", "提示");
            }
            else if (NewPassword.Password == string.Empty)
            {
                NewPassword.Focus();
                MessageBox.Show("请输入新密码！", "提示");
            }
            else
            {
                GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "UserPassWord");
                if (infoGblSetting.SetValue != CommonFunct.Md5Encrypt(OldPassword.Password))
                {
                    OldPassword.Focus();
                    MessageBox.Show("原始密码输入不正确，请重新输入！", "提示");
                    return;
                }

                infoGblSetting.SetValue = CommonFunct.Md5Encrypt(NewPassword.Password);
                ObjGblSetting.Update(infoGblSetting);
                MessageBox.Show("密码修改成功！", "提示");
            }
        }

        private void btnKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            tbxEPSInstall_GotFocus(FacilityAddress, null);
        }

        private void btnLampRelationLine_Click(object sender, RoutedEventArgs e)
        {
            if (LineName.SelectedItem != null && LineName.SelectedIndex >= 0)
            {
                EscapeLinesInfo infoEsacpeLines = LstEscapeLines.Find(x => x.Location == CurrentSelectFloorLogin && x.Name == LineName.SelectedItem.ToString());
                LightInfo infoLight = LstLight.Find(x => x.Code == RelationLampCode.Content.ToString() && x.DisBoxID == int.Parse(RelationEPSCode.Content.ToString()));

                infoLight.EscapeLineID = infoEsacpeLines.ID;

                if (ObjLight.Update(infoLight))
                {
                    RemoveSelectedEscapeLine();
                    CommonFunct.PopupWindow("绑定成功！");
                }
                else
                {
                    CommonFunct.PopupWindow("绑定失败!");
                }
            }
            else
            {
                CommonFunct.PopupWindow("数据错误!");
            }
            LampRelationLine.Visibility = System.Windows.Visibility.Hidden;
            stpIconSearCodeInfoLogin.Children.Clear();
        }

        private void LineName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Linename = LineName.SelectedItem.ToString();
            //this.LampRelationLine.Opacity = 0.2;
            ShowSelectEscapeLine(Linename);
        }

        private void btnEPSReplace_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEPS();
        }

        private void btnEPSReplaceCancel_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEPSCancel();
        }

        private void Windows_MouseEnter(object sender, MouseEventArgs e)
        {
            GblSettingInfo infoGblSetting = LstGblSetting.Find(x => x.Key == "LicenseDate");
            if (DateTime.Now > Convert.ToDateTime(infoGblSetting.SetValue))
            {
                Register register = new Register();
                register.ShowDialog();
            }
        }

        private void EventLog_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void Shield_Click(object sender, RoutedEventArgs e)
        {
            if (DisCode.Visibility == System.Windows.Visibility.Hidden)
            {
                LampShield(null, Number.Content.ToString(), true);
            }
            else
            {
                LampShield(DisCode.Content.ToString(), Number.Content.ToString(), true);
            }
            Shield.Content = "屏蔽中";
            Shield.IsEnabled = false;
            Unblock.IsEnabled = true;
        }

        private void Unblock_Click(object sender, RoutedEventArgs e)
        {
            if (DisCode.Visibility == System.Windows.Visibility.Hidden)
            {
                LampShield(null, Number.Content.ToString(), false);
            }
            else
            {
                LampShield(DisCode.Content.ToString(), Number.Content.ToString(), false);
            }
            Shield.Content = "屏蔽";
            Shield.IsEnabled = true;
            Unblock.IsEnabled = false;
        }

        /// <summary>
        /// 灯具屏蔽
        /// </summary>
        /// <param name="epsCode"></param>
        /// <param name="lightCode"></param>
        /// <param name="shield"></param>
        private void LampShield(string epsCode, string lightCode, bool shield)
        {
            if (lightCode.Substring(0, 1) != "6")
            {
                LightInfo infoLight = LstLight.Find(x => x.Code == lightCode && x.DisBoxID == int.Parse(epsCode));
                if (shield)
                {
                    infoLight.Shield = 1;
                }
                else
                {
                    infoLight.Shield = 0;
                }
                ObjLight.Update(infoLight);
                LstLight = ObjLight.GetAll();
            }
            else
            {
                DistributionBoxInfo infoDistributionBox = LstDistributionBox.Find(x => x.Code == lightCode);
                if (shield)
                {
                    infoDistributionBox.Shield = 1;
                }
                else
                {
                    infoDistributionBox.Shield = 0;
                }
                ObjDistributionBox.Update(infoDistributionBox);
                LstDistributionBox = ObjDistributionBox.GetAll();
            }
        }

        private void btnAddEps_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = AddSingleEPS();
            if (isSuccess)
            {
                EPSAndLightStatCount();
            }
            AddEPSCode.Content = string.Empty;
        }

        private void btnDeleteEps_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = DeleteSingleEPS();
            if (isSuccess)
            {
                EPSAndLightStatCount();
            }
            AddEPSCode.Content = string.Empty;
        }

        private void btnFindLamp_Click(object sender, RoutedEventArgs e)
        {
            FindLight(AddEPSCode.Content.ToString());
        }

        private void btnEPSCancel_Click(object sender, RoutedEventArgs e)
        {
            AddEPSCode.Content = string.Empty;
        }
        /// <summary>
        /// 打印设置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrintSet_Click(object sender, RoutedEventArgs e)
        {
            PrintSetOptionView view = new PrintSetOptionView();
            view.ShowDialog();
        }
        /// <summary>
        /// 获取故障打印状态
        /// </summary>
        /// <param name="FaultType"></param>
        /// <returns></returns>
        private bool IsPrint(int FaultType)
        {
            CFaultPrintSetting setting = new CFaultPrintSetting();
            List<FaultPrintSettingInfo> faultPrintSettingInfos = setting.GetAll();
            return faultPrintSettingInfos.Find(x => x.FaultType == FaultType).IsPrint == 0 ? false : true;
        }
        /// <summary>
        /// 故障信息打印
        /// </summary>
        /// <param name="faultInfo"></param>
        private void FaultPrint(int faultType, string faultInfo)
        {
            //if (IsPrint(faultType))
            //{
            //    Printer.print(faultInfo);
            //}
        }
        /// <summary>
        /// 主机数据下传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHostdataUpload_Click(object sender, RoutedEventArgs e)
        {
            CommonFunct.PopupWindow("开始发送");
            DataUploadProgress.Visibility = System.Windows.Visibility.Visible;
            LoadProgressBar progressBar = new LoadProgressBar();
            progressBar.FilesCount = LstLight.Count;
            progressBar.ImportedFilesCount = 0;
            DataUploadProgress.DataContext = progressBar;
            Protocol.HostDataUpload(progressBar);
            ///CommonFunct.PopupWindow("发送完成");
        }
    }
}
