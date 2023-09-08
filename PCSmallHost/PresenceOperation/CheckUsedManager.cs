using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PCSmallHost.PresenceOperation
{
    public class CheckUsedManager
    {
        public static event EventHandler TimeToFinishedEvebt = null;

        private static DispatcherTimer checkUsedTimer = new DispatcherTimer();

        private static Point mousePosition = GetMousePoint();

        static CheckUsedManager()
        {
            checkUsedTimer.Interval = TimeSpan.FromSeconds(1800);

            checkUsedTimer.Tick += new EventHandler(checkUserTimer_Tick);

            checkUsedTimer.Start();
        }

        static void checkUserTimer_Tick(object sender, EventArgs e)
        {
            if (!HaveUsedTo())
            {
                if (TimeToFinishedEvebt != null)
                {
                    TimeToFinishedEvebt(null, null);
                }
            }
        }

        private static bool HaveUsedTo()
        {
            Point point = GetMousePoint();

            if (point == mousePosition)
            {
                return false;
            }

            mousePosition = point;

            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MPoint
        {
            public int x;
            public int y;
            public MPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(out MPoint mpt);

        /// <summary>
        /// 获取当前屏幕鼠标位置
        /// </summary>
        /// <returns></returns>
        public static Point GetMousePoint()
        {
            MPoint mpt = new MPoint();

            GetCursorPos(out mpt);

            Point p = new Point(mpt.x, mpt.y);

            return p;
        }
    }
}

