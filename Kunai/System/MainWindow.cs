using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using System.IO;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using TeamSpettro.SettingsSystem;
using Kunai.Settings;
using System.Drawing.Imaging;
using System.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;


namespace Kunai
{
    public class MainWindow : GameWindow
    {
        public static bool IsMouseLeftDown;
        public static readonly string ApplicationName = "Kunai";
        private ImGuiController _controller;
        private IntPtr iniName;
        public static KunaiProject Renderer;
        public byte[] _iconData;
        public static ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
        {
            MemoryStream ms = new MemoryStream();
            Title = ApplicationName;
            GetIcon();
        }

        void GetIcon()
        {
            // TODO: eventually replace with program's own embedded icon?
            using SixLabors.ImageSharp.Image<Rgba32> newDDS = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine(Program.Path, "Resources", "Icons", "ico.png"));
            _iconData = new byte[newDDS.Width * newDDS.Height * Unsafe.SizeOf<Rgba32>()];
            newDDS.CopyPixelDataTo(_iconData);

            OpenTK.Windowing.Common.Input.Image windowIcon = new OpenTK.Windowing.Common.Input.Image(newDDS.Width, newDDS.Height, _iconData);
            Icon = new OpenTK.Windowing.Common.Input.WindowIcon(windowIcon);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            TeamSpettro.Resources.Initialize(Path.Combine(Program.Path, "config.json"));
            Renderer = new KunaiProject(this, new System.Numerics.Vector2(1280, 720), new System.Numerics.Vector2(ClientSize.X, ClientSize.Y));
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

            ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
            // Example #10000 for why ImGui.NET is kinda bad
            // This is to avoid having imgui.ini files in every folder that the program accesses
            unsafe
            {
                iniName = Marshal.StringToHGlobalAnsi(Path.Combine(Program.Path, "imgui.ini"));
                ImGuiIOPtr io = ImGui.GetIO();
                io.IniFilename = (byte*)iniName;
            }
            unsafe
            {
            }
            Renderer.Windows.Add(new MenuBarWindow());
            Renderer.Windows.Add(new AnimationsWindow());
            Renderer.Windows.Add(new HierarchyWindow());
            Renderer.Windows.Add(new InspectorWindow());
            Renderer.Windows.Add(new ViewportWindow());
            Renderer.Windows.Add(new CropEditor());
            Renderer.Windows.Add(new SettingsWindow());

            if (Program.Arguments.Length > 0)
            {
                Renderer.LoadFile(Program.Arguments[0]);
            }
        }
        protected override void OnResize(ResizeEventArgs in_E)
        {
            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            base.OnResize(in_E);

            Renderer.ScreenSize = new System.Numerics.Vector2(ClientSize.X, ClientSize.Y);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        //For whatever fucking stupid reason, Imgui.Net has no "IsMouseDown" function
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                IsMouseLeftDown = true; // Left mouse button pressed
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left)
            {
                IsMouseLeftDown = false; // Left mouse button released
            }
        }
        protected override void OnRenderFrame(FrameEventArgs in_E)
        {
            if (Renderer.ScreenSize.X != 0 && Renderer.ScreenSize.Y != 0)
            {
                if (IsFocused)
                {
                    base.OnRenderFrame(in_E);


                    _controller.Update(this, (float)in_E.Time);

                    GL.ClearColor(new Color4(0, 0, 0, 255));
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.Blend);
                    GL.Disable(EnableCap.CullFace);
                    GL.BlendEquation(BlendEquationMode.FuncAdd);
                    float deltaTime = (float)(in_E.Time);
                    Renderer.Render(Renderer.WorkProjectCsd, (float)deltaTime);
                    //ImGui.ShowMetricsWindow();

                    ImGui.SetNextWindowSize(Renderer.ScreenSize);
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));

                    _controller.Render();

                    if (Renderer.WorkProjectCsd != null)
                        Title = ApplicationName + $" - [{Renderer.Config.WorkFilePath}]";

                    ImGuiController.CheckGlError("End of frame");
                }
            }
            SwapBuffers();
        }
        protected override void OnTextInput(TextInputEventArgs in_E)
        {
            base.OnTextInput(in_E);


            _controller.PressChar((char)in_E.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs in_E)
        {
            base.OnMouseWheel(in_E);

            _controller.MouseScroll(in_E.Offset);
        }
    }
}
