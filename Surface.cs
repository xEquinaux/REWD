using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace REWD
{
	public class RewBatch
    {
        [DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
        static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitmapInfo pbmi, uint pila, out IntPtr ppbBits, IntPtr hSection, uint dwOffset);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, byte[] lpBits);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        static extern int SetDIBitsToDevice(IntPtr hdc, int xDest, int yDest, int w, int h, int xSrc, int ySrc, int startScan, int scanLines, IntPtr bits, IntPtr bmih, uint colorUse);
        [DllImport("gdi32.dll")]
        static extern int SetDIBitsToDevice(IntPtr hdc, int xDest, int yDest, int w, int h, int xSrc, int ySrc, int startScan, int scanLines, byte[] bits, BitmapInfoHeader bmih, uint colorUse);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hbdiobj);

        private int stride => width * ((PixelFormats.Bgr24.BitsPerPixel + 7) / 8);
        private int width, height;
        private int oldWidth, oldHeight;
        public short BitsPerPixel { get; private set; }
        private byte[] backBuffer;
        private Int32Rect backBufferRect => new Int32Rect(0, 0, width, height);
        IntPtr hdc;
        public RewBatch(int width, int height, int bitsPerPixel = 32)
        {
            Initialize(width, height);
            BitsPerPixel = (short)bitsPerPixel;
        }
        void Initialize(int width, int height)
        {
            this.width = width;
            this.height = height;
            backBuffer = new byte[width * height * (BitsPerPixel / 8)];
        }
        public bool Resize(int width, int height)
        {
            if (oldWidth != width || oldHeight != height)
            {
                this.width = width;
                this.oldWidth = width;
                this.height = height;
                this.oldHeight = height;
                backBuffer = new byte[width * height * (BitsPerPixel / 8)];
                return true;
            }
            return false;
        }
        public void Begin(IntPtr hdc)
        {
            backBuffer = new byte[width * height * (BitsPerPixel / 8)];
            this.hdc = hdc;
        }
        public void Draw(int x, int y)
        {
        }
        public void End()
        {
            BitmapInfoHeader bmih = new BitmapInfoHeader()
            {
                Size = 40,
                Width = this.width,
                Height = this.height,
                Planes = 1,
                BitCount = 32,
                Compression = (uint)BitmapCompressionMode.BI_BITFIELDS,
                SizeImage = (uint)(this.width * this.height * (BitsPerPixel / 8)),
                XPelsPerMeter = 96,
                YPelsPerMeter = 96,
                RedMask = 0x00FF0000,
                GreenMask = 0x0000FF00,
                BlueMask = 0x000000FF,
                AlphaMask = 0xFF000000,
                CSType = BitConverter.ToUInt32(new byte[] { 32, 110, 106, 87 }, 0)
            };
            GCHandle h = GCHandle.Alloc(bmih, GCHandleType.Pinned);
            GCHandle h2 = GCHandle.Alloc(backBuffer, GCHandleType.Pinned);
            SetDIBitsToDevice(hdc, 0, 0, this.width, this.height, 0, 0, 0, this.height, h2.AddrOfPinnedObject(), h.AddrOfPinnedObject(), 0);
            h.Free();
            h2.Free();
            ReleaseDC(IntPtr.Zero, hdc);
            backBuffer = null;
        }
    }
}
