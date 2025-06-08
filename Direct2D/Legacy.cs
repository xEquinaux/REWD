namespace REWD.D2D.Legacy
{
	using SharpDX;
	using SharpDX.Direct2D1;
	using SharpDX.Direct3D;
	using SharpDX.Direct3D11;
	using SharpDX.DXGI;
	using SharpDX.Mathematics.Interop;
	using SharpDX.WIC;
	using SharpDX.Windows;
	using System.Windows.Forms;

	public class Program : Direct2D
	{
		public Program(int width, int height)
		{
			new Direct2D(width, height);
		}
		public override void Draw(SharpDX.Direct2D1.DeviceContext context, RawColor4 color)
		{
			base.Draw(context, color);
		}
	}

	public class Direct2D
	{
		public Direct2D(int width = 800, int height = 600)
		{
			var form = new RenderForm("SharpDX Render Window");
			form.ClientSize = new System.Drawing.Size(width, height);

			var factory = new SharpDX.Direct2D1.Factory1(FactoryType.SingleThreaded);
			var d3dDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
			var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>();
			var d2dDevice = new SharpDX.Direct2D1.Device(factory, dxgiDevice);
			var d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.None);

			var renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Unknown), 0, 0, RenderTargetUsage.None, SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT)
			{
				Type = RenderTargetType.Hardware
			};

			// Create the render target
			var bitmapProperties = new BitmapProperties1(
				new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore),
				96, 96,
				BitmapOptions.Target | BitmapOptions.CannotDraw // Ensure it's a valid render target
			);
			var bitmap = new Bitmap1(d2dContext, new Size2(width, height), bitmapProperties);

			d2dContext.Target = bitmap;

			var swapChainDescription = new SwapChainDescription()
			{
				BufferCount = 1,
				ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
				Usage = Usage.RenderTargetOutput,
				OutputHandle = form.Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				IsWindowed = true
			};


			var wicFactory = new ImagingFactory();
            var d2dFactory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded);

            var wicBitmap = new SharpDX.WIC.Bitmap(wicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppBGRA, BitmapCreateCacheOption.CacheOnLoad);
			var d2dRenderTarget = new WicRenderTarget(d2dFactory, wicBitmap, renderTargetProperties);
			var solidColorBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0, 0, 0, 1));

            //d2dRenderTarget.BeginDraw();
            //d2dRenderTarget.Clear(Color.Black);
            //d2dRenderTarget.FillGeometry(rectangleGeometry, solidColorBrush, null);
            //d2dRenderTarget.EndDraw();

			var factoryDxgi = new SharpDX.DXGI.Factory1();
			var swapChain = new SwapChain(factoryDxgi, d3dDevice, swapChainDescription);

			float r = System.Drawing.Color.CornflowerBlue.R / 255f;
			float g = System.Drawing.Color.CornflowerBlue.G / 255f;
			float b = System.Drawing.Color.CornflowerBlue.B / 255f;
			var color = new RawColor4(r, g, b, 1);
			RenderLoop.Run(form, () =>
			{
				d2dRenderTarget.BeginDraw();
				d2dRenderTarget.Clear(color);
				d2dRenderTarget.EndDraw();

				//BeginDraw(d2dContext);
				//Draw(d2dContext, color);
				//EndDraw(d2dContext);
				//swapChain.Present(1, PresentFlags.None); // Present the frame
			});

			bitmap.Dispose();
			d2dContext.Dispose();
			d2dDevice.Dispose();
			dxgiDevice.Dispose();
			factory.Dispose();
			factoryDxgi.Dispose();
			swapChain.Dispose();
		}

		public virtual void BeginDraw(SharpDX.Direct2D1.DeviceContext context)
		{
			context.BeginDraw();
		}
		public virtual void EndDraw(SharpDX.Direct2D1.DeviceContext context)
		{
			context.EndDraw();
		}
		public virtual void Draw(SharpDX.Direct2D1.DeviceContext context, RawColor4 color)
		{
			context.Clear(color); // Clear the screen
		}
	}
}
