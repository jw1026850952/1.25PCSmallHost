using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SunFo.OS_SmallHost_.Util
{
    public class GifImage : System.Windows.Controls.Image
    {
        private Bitmap GifBitmap;
        private BitmapSource BitmapSource;

        public GifImage(string strGifImagePath)
        {
            this.GifBitmap = new Bitmap(strGifImagePath);

        }

        private BitmapSource GetBitmapSource()
        {
            IntPtr handle = IntPtr.Zero;
            handle = this.GifBitmap.GetHbitmap();
            this.BitmapSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            if (handle != IntPtr.Zero)
            {
                DeleteObject(handle);
            }
            return this.BitmapSource;
        }

        public void StartAnimate()
        {
            ImageAnimator.StopAnimate(this.GifBitmap, this.OnFrameChanged);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr handle);

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ImageAnimator.UpdateFrames();
                if (this.BitmapSource != null)
                {
                    this.BitmapSource.Freeze();
                }

                this.BitmapSource = this.GetBitmapSource();
                Source = this.BitmapSource;
                this.InvalidateVisual();
            }));
        }
    }
}
