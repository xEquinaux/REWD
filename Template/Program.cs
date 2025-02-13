using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;                      
using System.Threading.Tasks;
using System.Windows.Diagnostics;
using System.Windows.Forms;
using REWD.FoundationR;
using Microsoft.Win32;

namespace REWD.Foundation_GameTemplate
{
    public class Program
    {
        static int StartX => 0;
        static int StartY => 0;
        static int Width => 640;
        static int Height => 480;
        static int BitsPerPixel => 32;
        static string Title = "Foundation_GameTemplate";
        public static Main m;
        static void Main(string[] args)
        {
            Thread t = new Thread(() => { (m = new Main()).Run(SurfaceType.WindowHandle_Loop, new FoundationR.Surface(StartX, StartY, Width, Height, Title, BitsPerPixel)); });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (Console.ReadLine() != "exit");
            t.Abort();
            Environment.Exit(0);
        }
        public static void Run(bool noBorder = false)
        {
            Thread t = new Thread(() => { (m = new Main()).Run(noBorder ? SurfaceType.WindowHandle_Loop_NoBorder : SurfaceType.WindowHandle_Loop, new FoundationR.Surface(StartX, StartY, Width, Height, Title, BitsPerPixel)); });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (Console.ReadLine() != "exit");
            t.Abort();
            Environment.Exit(0);
        }
    }
    public class Main : Foundation
    {
        System.Drawing.Point mouse;
        WindowUtils.RECT window_frame;
        REW pane;
        REW tile;
        REW cans;
        REW solidColor;
        internal Main()
        {
        }

        public override void RegisterHooks()
        {
            Foundation.UpdateEvent += Update;
            Foundation.ResizeEvent += Resize;
            Foundation.InputEvent += Input;
            Foundation.DrawEvent += Draw;
            Foundation.InitializeEvent += Initialize;
            Foundation.LoadResourcesEvent += LoadResources;
            Foundation.MainMenuEvent += MainMenu;
            Foundation.PreDrawEvent += PreDraw;
            Foundation.CameraEvent += Camera;
        }

        protected void Camera(CameraArgs e)
        {
        }

        protected void PreDraw(PreDrawArgs e)
        {
        }

        protected void MainMenu(DrawingArgs e)
        {
        }

        protected void LoadResources()
        {
            Asset.LoadFromFile(@".\Textures\bluepane.rew", out pane);
            Asset.LoadFromFile(@".\Textures\background.rew", out tile);
            Asset.LoadFromFile(@".\Textures\cans.rew", out cans);
        }

        protected void Initialize(InitializeArgs e)
        {
        }

        protected void Draw(DrawingArgs e)
        {
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    e.rewBatch.Draw(tile, i * 50, j * 50);
            e.rewBatch.Draw(tile, mouse.X, mouse.Y);
            e.rewBatch.Draw(REW.Create(50, 50, Color.White, Ext.GetFormat(4)), 0, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Red, Ext.GetFormat(4)), 50, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Green, Ext.GetFormat(4)), 100, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Blue, Ext.GetFormat(4)), 150, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Gray, Ext.GetFormat(4)), 200, 0);
            e.rewBatch.Draw(REW.Create(50, 50, Color.Black, Ext.GetFormat(4)), 250, 0);
            e.rewBatch.DrawString("Arial", "Test_value_01", 50, 50, 200, 100);
        }

        protected void Input(InputArgs e)
        {
            mouse = e.mouse;
        }

        protected void Update(UpdateArgs e)
        {
        }
        
        protected new bool Resize()
        {
            return false;
        }
    }
}
