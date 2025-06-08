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

namespace REWD.D2D;

public class Direct2D
{
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
			
        SharpDX.Direct2D1.Device device = new SharpDX.Direct2D1.Device(factory, dxgiDevice);;
        SwapChain swapChain = new SwapChain(factoryDxgi, d3dDevice, swapChainDescription);;
                            
        // Get the back buffer from the swap chain
        var backBuffer = swapChain.GetBackBuffer<Texture2D>(0);

        // Create a Direct2D Render Target
        var renderTargetProperties = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
        var renderTarget = new RenderTarget(factory, backBuffer.QueryInterface<Surface>(), renderTargetProperties);

        RenderLoop.Run(form, () =>
	    {
		    // Begin drawing
            renderTarget.BeginDraw();

            // Clear the render target with a solid color
            renderTarget.Clear(new RawColor4(0.1f, 0.2f, 0.3f, 1.0f));

            // Draw a rectangle
            var brush = new SolidColorBrush(renderTarget, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
            renderTarget.FillRectangle(new RawRectangleF(100, 100, 300, 300), brush);

            // End drawing
            renderTarget.EndDraw();

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
}
