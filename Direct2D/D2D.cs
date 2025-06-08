using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using SharpDX.Windows;
using System;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;

namespace REWD.D2D;

public class Direct2D
{
    public SharpDX.Direct2D1.DeviceContext deviceContext;

    public Direct2D(int width, int height)
    {
        var form = new RenderForm("SharpDX Render Window");
		form.ClientSize = new System.Drawing.Size(width, height);


        // Initialize Direct2D Factory
        var factory = new SharpDX.Direct2D1.Factory1();

        // Create a WIC Imaging Factory (optional, for image handling)
        var wicFactory = new ImagingFactory();

        // Create a Direct3D Device and SwapChain
        var swapChainDescription = new SwapChainDescription()
		{
			BufferCount = 1,
			ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
			Usage = Usage.RenderTargetOutput,
			OutputHandle = form.Handle,
			SampleDescription = new SampleDescription(1, 0),
			IsWindowed = true
		};

        var factoryDxgi = new SharpDX.DXGI.Factory1();
		var d3dDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
		var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
			
        var device = new SharpDX.Direct2D1.Device(factory, dxgiDevice);
        var swapChain = new SwapChain(factoryDxgi, d3dDevice, swapChainDescription);
                            
        // Get the back buffer from the swap chain
        var backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        if (backBuffer == null)
        {
            throw new Exception("Back Buffer if null");
        }

        // Create a Direct2D Render Target
        var renderTargetProperties = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
        var renderTarget = new RenderTarget(factory, backBuffer.QueryInterface<Surface>(), renderTargetProperties);

        var bitmapProperties = new BitmapProperties1(
            new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore),
            96, 96,
            BitmapOptions.Target | BitmapOptions.CannotDraw
        );

        // Initialize device context
        deviceContext = new SharpDX.Direct2D1.DeviceContext(device, SharpDX.Direct2D1.DeviceContextOptions.None);

        // Create the Direct2D render target from the DXGI Surface
        var renderTarget2 = new Bitmap1(deviceContext, backBuffer.QueryInterface<Surface>(), bitmapProperties);
        
        // Set it as the active target
        deviceContext.Target = renderTarget2;

        RenderLoop.Run(form, () =>
	    {
		    // Begin drawing
            deviceContext.BeginDraw();

            // Clear the render target with a solid color
            deviceContext.Clear(new RawColor4(0.1f, 0.2f, 0.3f, 1.0f));

            // Draw a rectangle
            //var brush = new SolidColorBrush(renderTarget, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
            //renderTarget.FillRectangle(new RawRectangleF(100, 100, 300, 300), brush);
            this.Draw(deviceContext);

            // End drawing
            deviceContext.EndDraw();

            // Present the frame
            swapChain.Present(1, PresentFlags.None);
	    });

        // Dispose resources
        renderTarget.Dispose();
        backBuffer.Dispose();
        swapChain.Dispose();
        device.Dispose();
        factory.Dispose();
        wicFactory.Dispose();
    }

    public virtual void Draw(DeviceContext rt)
    {
    }
}

public class Game : Direct2D
{
    public Game() : base (800, 600)
    {
    }

	public override void Draw(DeviceContext rt)
	{
        using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(100, 100))
        { 
            using (System.Drawing.Graphics g = Graphics.FromImage(bmp))
            { 
                g.FillRectangle(System.Drawing.Brushes.Red, new Rectangle(100, 100, 100, 100));
                var image = ConvertBitmap(bmp, deviceContext);
		        rt.DrawBitmap(image, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                image.Dispose();
            }
        }
	}

    private SharpDX.Direct2D1.Bitmap ConvertBitmap(System.Drawing.Bitmap bitmap, SharpDX.Direct2D1.DeviceContext deviceContext)
    {
        var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties(
            new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));

        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        using (var dataStream = new SharpDX.DataStream(bitmapData.Scan0, bitmapData.Stride * bitmap.Height, true, false))
        {
            var sharpDxBitmap = new SharpDX.Direct2D1.Bitmap(deviceContext, new Size2(bitmap.Width, bitmap.Height), dataStream, bitmapData.Stride, bitmapProperties);
            bitmap.UnlockBits(bitmapData);
            return sharpDxBitmap;
        }
    }
}
