using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows;
using REWD.FoundationR;
using static System.Windows.Forms.AxHost;
using System.Windows.Media.Media3D;
using Color = System.Drawing.Color;

namespace REWD
{
	internal class Program
	{
		static void Main(string[] args)
		{
			new Foundation(0, 0, 800, 600, "Demo", 32);
		}
	}

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
		private byte[]? backBuffer;
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
		public void Draw(REW image, int x, int y)
		{
			if (x > width)  return;
            if (y > height) return;
            CompositeImage(backBuffer, width, height, image.GetPixels(), image.Width, image.Height, x, y);
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
		public virtual void CompositeImage(byte[] buffer, int bufferWidth, int bufferHeight, byte[] image, int imageWidth, int imageHeight, int x, int y, bool text = false)
        {
            Parallel.For(0, imageHeight, i =>
            {
                for (int j = 0; j < imageWidth; j++)
                {
                    if (j > bufferWidth)
                    {
                        return;
                    }
                    if (i > bufferHeight)
                    {
                        return;
                    }

                    int index = Math.Min((i * imageWidth + j) * 4, image.Length - 4);
                    int bufferIndex = ((y + i) * bufferWidth + (x + j)) * 4;

                    if (bufferIndex < 0 || bufferIndex >= buffer.Length - 4)
                        return;
                    Pixel back = new Pixel(
                        buffer[bufferIndex + 3],
                        buffer[bufferIndex + 2],
                        buffer[bufferIndex + 1],
                        buffer[bufferIndex]
                    );
                    Pixel fore = new Pixel(
                        image[index],
                        image[index + 1],
                        image[index + 2],
                        image[index + 3]
                    );
                    if (back.A == 0 && fore.A == 0)
                        continue;

                    if (fore.A < 255 && !text)
                    {
                        Color blend = fore.color.Blend(back.color, 0.15d);
                        buffer[bufferIndex] = blend.B;
                        buffer[bufferIndex + 1] = blend.G;
                        buffer[bufferIndex + 2] = blend.R;

                        if (back.A == 255) buffer[bufferIndex + 3] = 255;
                        else buffer[bufferIndex + 3] = blend.A;
                    }
                    else
                    {
                        buffer[bufferIndex] = fore.color.B;
                        buffer[bufferIndex + 1] = fore.color.G;
                        buffer[bufferIndex + 2] = fore.color.R;
                        buffer[bufferIndex + 3] = 255;
                    }
                }
            });
        }                 
	}
}
