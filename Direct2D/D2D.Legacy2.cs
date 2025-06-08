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

namespace REWD.D2D.Legacy2;

public class Direct2D
{
    Texture2D tex;
    RenderTarget rt;
    DeviceContext d2dContext;
    SharpDX.Direct2D1.Device device;

    public Direct2D()
    {
        var form = new RenderForm("SharpDX Render Window");
		form.ClientSize = new System.Drawing.Size(800, 600);


        // Initialize Direct2D Factory
        var factory = new SharpDX.Direct2D1.Factory1();

        // Create a WIC Imaging Factory (optional, for image handling)
        var wicFactory = new ImagingFactory();

        // Create a Direct3D Device and SwapChain
        var swapChainDescription = new SwapChainDescription()
		{
			BufferCount = 1,
			ModeDescription = new ModeDescription(800, 600, new Rational(60, 1), Format.B8G8R8A8_UNorm),
			Usage = Usage.RenderTargetOutput,
			OutputHandle = form.Handle,
			SampleDescription = new SampleDescription(1, 0),
			SwapEffect = SwapEffect.Discard,
			IsWindowed = true
		};

        var factoryDxgi = new SharpDX.DXGI.Factory1();
		var d3dDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
		var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>();
			
        device = new SharpDX.Direct2D1.Device(factory, dxgiDevice);
        SwapChain swapChain = new SwapChain(factoryDxgi, d3dDevice, swapChainDescription);
                            
        // Get the back buffer from the swap chain
        tex = swapChain.GetBackBuffer<Texture2D>(0);

        d2dContext = new SharpDX.Direct2D1.DeviceContext(device, SharpDX.Direct2D1.DeviceContextOptions.None);
        
        // Create a Direct2D Render Target
        var renderTargetProperties = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
        rt = new RenderTarget(factory, tex.QueryInterface<Surface>(), renderTargetProperties);

        RenderLoop.Run(form, () =>
	    {
		    // Begin drawing
            rt.BeginDraw();

            // Clear the render target with a solid color
            rt.Clear(new RawColor4(0.1f, 0.2f, 0.3f, 1.0f));

            // Draw a rectangle
            //var brush = new SolidColorBrush(renderTarget, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
            //renderTarget.FillRectangle(new RawRectangleF(100, 100, 300, 300), brush);
            this.Draw(rt);
            
            // End drawing
            rt.EndDraw();

            // Present the frame
            swapChain.Present(1, PresentFlags.None);
	    });

        // Dispose resources
        rt.Dispose();
        tex.Dispose();
        swapChain.Dispose();
        device.Dispose();
        factory.Dispose();
        wicFactory.Dispose();
        d2dContext.Dispose();
    }

    public virtual void Draw(RenderTarget rt)
    {
    }

    public void DrawBitmap(System.Drawing.Bitmap bitmap)
    {
        // Create Direct2D device context
        rt.DrawBitmap(ConvertBitmap(bitmap, d2dContext), 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
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

public class Game : Direct2D
{
    public Game()
    {
        new Direct2D();
    }
	public override void Draw(RenderTarget rt)
	{
		DrawBitmap(new System.Drawing.Bitmap(20, 20));
	}
}