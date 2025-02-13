﻿using REWD.Foundation_GameTemplate;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace REWD.FoundationR
{
    public partial class Foundation
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, uint flags);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        public Foundation(int sx, int sy, int w, int y, string title, int bpp)
        {
            Start(new Surface(sx, sy, w, y, title, bpp));
        }

        bool flag = true, flag2 = true, init, init2;
        public static int offX, offY;
        public static Rectangle bounds;
        public static Camera viewport;
        protected static RewBatch _rewBatch;
        Stopwatch watch1 = new Stopwatch();
        public static Stopwatch GameTime = new Stopwatch();
        public static TimeSpan DrawTime;
        public static TimeSpan UpdateTime;
        public static IntPtr HDC;

        internal class SurfaceForm : Form
        {
            internal SurfaceForm(Surface surface)
            {
                //form.TransparencyKey = System.Drawing.Color.CornflowerBlue;
                BackColor = System.Drawing.Color.CornflowerBlue;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                Width = surface.Width;
                Height = surface.Height;
                Location = new Point(surface.X, surface.Y);
                Text = surface.Title;
                Name = surface.Title;
                DoubleBuffered = true;
                UseWaitCursor = false;
                BringToFront();
            }
        }

        public void Start(Surface window, bool noBorder = false)
        {
            this.RegisterHooks();
            window.form = new SurfaceForm(window);
            if (noBorder)
			    window.form.FormBorderStyle = FormBorderStyle.None;
            _rewBatch = new RewBatch(window.Width, window.Height, window.BitsPerPixel);
            LoadResourcesEvent?.Invoke();
            InitializeEvent?.Invoke(new InitializeArgs());
            HDC = FindWindowByCaption(IntPtr.Zero, window.Title);
            Task t = new Task(() => draw(ref flag, window));
            Task t2 = new Task(() => update(ref flag2));
            t.Start();
            t2.Start();
            GameTime.Start();
            void draw(ref bool taskDone, Surface surface)
            {
                int width = (int)surface.Width;
                int height = (int)surface.Height;
                while (true)
                {
                    if (taskDone)
                    {
                        taskDone = false;
                        DrawTime = watch1.Elapsed;
                        watch1.Restart();
                        try
                        { 
                            window.form?.Invoke(() =>
                            {
                                InputEvent?.Invoke(new InputArgs() { mouse = window.form.PointToClient(System.Windows.Forms.Cursor.Position) });
                            });
                        }
                        catch { }
                        finally
                        { 
                            {
                                InternalBegin(window);
                                //if ((bool)ResizeEvent?.Invoke())
                                //{
                                //    _rewBatch = new RewBatch(width, height, window.BitsPerPixel);
                                //}
                                MainMenuEvent?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                                PreDrawEvent?.Invoke(new PreDrawArgs() { rewBatch = _rewBatch });
                                DrawEvent?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                                CameraEvent?.Invoke(new CameraArgs() { rewBatch = _rewBatch, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                                InternalEnd();
                            }
                        }
                        taskDone = true;
                    }
                }
            }
            void update(ref bool taskDone)
            {
                while (true)
                {
                    if (taskDone)
                    {
                        taskDone = false;
                        UpdateEvent?.Invoke(new UpdateArgs());
                        taskDone = true;
                    }
                }
            }
            window.form.ShowDialog();
        }

		public virtual void FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		bool UpdateLimiter()
        {
            double deltaTime = 0; // Initialize the time accumulator
            double accumulator = 0; // Accumulated time
            double targetFrameTime = 1.0 / 60.0; // Target frame time (1/60 seconds)
            double oldTime = 0;

            double currentTime = watch1.Elapsed.Milliseconds; // Get current time
            deltaTime = currentTime - oldTime; // Calculate time since last frame
            oldTime = currentTime; // Update old time

            accumulator += deltaTime; // Accumulate time

            // Update when the accumulated time exceeds the target frame time
            while (accumulator >= targetFrameTime)
            {
                watch1.Restart();
                accumulator -= targetFrameTime; // Subtract the frame time
                return true;
            }
            return false;
        }

        public static class WindowUtils
        {
            [DllImport("dwmapi.dll")]
            static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            public static RECT GetWindowRectangleWithoutShadows(IntPtr handle)
            {
                RECT rect;
                DwmGetWindowAttribute(handle, 9 /* DWMWA_EXTENDED_FRAME_BOUNDS */, out rect, Marshal.SizeOf(typeof(RECT)));
                return rect;
            }
        }

        private void InternalBegin(Surface window)
        {
            _rewBatch.Begin(GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403));
        }
        private void InternalBegin(IntPtr hdc)
        {
            _rewBatch.Begin(hdc);
        }
        private void InternalEnd()
        {
            _rewBatch.End();
        }
        #region events
        public delegate void Event<T>(T e);
        public delegate void Event();
        public delegate bool Resize();
        public static event Resize ResizeEvent;
        public static event Event<InitializeArgs> InitializeEvent;
        public static event Event<InputArgs> InputEvent;
        public static event Event LoadResourcesEvent;
        public static event Event<DrawingArgs> MainMenuEvent;
        public static event Event<PreDrawArgs> PreDrawEvent;
        public static event Event<DrawingArgs> DrawEvent;
        public static event Event<UpdateArgs> UpdateEvent;
        public static event Event<CameraArgs> CameraEvent;
        public interface IArgs
        {
        }
        public class ResizeArgs : IArgs
        {
            public Surface window;
        }
        public class DrawingArgs : IArgs
        {
            public RewBatch rewBatch;
        }
        public class PreDrawArgs : IArgs
        {
            public RewBatch rewBatch;
        }
        public class UpdateArgs : IArgs
        {
        }
        public class CameraArgs : IArgs
        {
            public RewBatch rewBatch;
            public Camera CAMERA;
            public Rectangle screen;
            public int offX, offY;
        }
        public class InitializeArgs : IArgs
        {
        }
        public class InputArgs : IArgs
        {
            public Point mouse;
        }
        #endregion

        Point mouse;
        WindowUtils.RECT window_frame;
        REW pane;
        REW tile;
        REW cans;
        REW solidColor;
        REW background;
        REW glass;
        REW properties;

        public virtual void RegisterHooks()
        {
            Foundation.UpdateEvent += Update;
            Foundation.InputEvent += Input;
            Foundation.DrawEvent += Draw;
            Foundation.InitializeEvent += Initialize;
            Foundation.LoadResourcesEvent += LoadResources;
            Foundation.MainMenuEvent += MainMenu;
            Foundation.PreDrawEvent += PreDraw;
            Foundation.CameraEvent += Camera;
        }

        protected virtual void Camera(CameraArgs e)
        {
        }

        protected virtual void PreDraw(PreDrawArgs e)
        {
        }

        protected virtual void MainMenu(DrawingArgs e)
        {
        }

        protected virtual void LoadResources()
        {
            Asset.LoadFromFile(@".\Textures\properties.rew", out properties);
            Asset.LoadFromFile(@".\Textures\glass.rew", out glass);
            Asset.LoadFromFile(@".\Textures\sky_1280x1024.rew", out background);
            Asset.LoadFromFile(@".\Textures\bluepane.rew", out pane);
            Asset.LoadFromFile(@".\Textures\background.rew", out tile);
            Asset.LoadFromFile(@".\Textures\cans.rew", out cans);
        }

        protected virtual void Initialize(InitializeArgs e)
        {
        }

        protected virtual void Draw(DrawingArgs e)
        {
            e.rewBatch.Draw(background, 0, 0);
            e.rewBatch.Draw(tile, mouse.X, mouse.Y);
            e.rewBatch.Draw(REW.Create(50, 50, Color.FromArgb(255, 255, 255, 255), Ext.GetFormat(4)), 0, 50);
            e.rewBatch.Draw(glass, 200, 200);
            e.rewBatch.Draw(glass, 400, 520);
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    e.rewBatch.Draw(tile, i * 50, j * 50);
            e.rewBatch.Draw(REW.Create(50, 50, Color.White, Ext.GetFormat(4)), 0, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Red, Ext.GetFormat(4)), 50, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Green, Ext.GetFormat(4)), 100, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Blue, Ext.GetFormat(4)), 150, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Gray, Ext.GetFormat(4)), 200, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Black, Ext.GetFormat(4)), 250, 0);
            e.rewBatch.Draw(pane, 0, 0);
            e.rewBatch.Draw(properties, 200, 300);
            e.rewBatch.DrawString("Arial", "Test_value_01", 50, 50, 200, 100);
        }

        protected virtual void Input(InputArgs e)
        {
            mouse = e.mouse;
        }

        protected virtual void Update(UpdateArgs e)
        {
        }
    }
    public struct Surface
    {
        public Surface(int x, int y, int width, int height, string windowTitle, int bitsPerPixel)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Title = windowTitle;
            this.BitsPerPixel = bitsPerPixel;
            form = default;
        }
        public string? Title;
        public int Width, Height;
        public int X, Y;
        public int BitsPerPixel;
        public Form form;
    }
    public class Camera
    {
        public Vector2 oldPosition;
        public Vector2 position;
        public Vector2 velocity;
        public virtual bool isMoving => velocity != Vector2.Zero || oldPosition != position;
        public bool follow = false;
        public bool active = false;
    }
}
