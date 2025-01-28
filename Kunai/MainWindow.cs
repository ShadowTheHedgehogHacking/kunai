using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SharpNeedle.Ninja.Csd;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using System.Windows;
using Hexa.NET.ImPlot;
using OpenTK.Windowing.Common.Input;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;


namespace Kunai
{
    public class MainWindow : GameWindow
    {
        public static readonly string applicationName = "Kunai";
        private static MemoryStream iconData;
        float test = 1;
        ImGuiController _controller;
        public static ShurikenRenderHelper renderer;
        public static ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings(){ Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
        { }
        protected override void OnLoad()
        {
            base.OnLoad();
            Title = applicationName;
            renderer = new ShurikenRenderHelper(this, new ShurikenRenderer.Vector2(1280, 720), new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y));
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

            renderer.windows.Add(new MenuBarWindow());
            renderer.windows.Add(new AnimationsWindow());
            renderer.windows.Add(new HierarchyWindow());
            renderer.windows.Add(new InspectorWindow());
            renderer.windows.Add(new ViewportWindow());
            if (Program.arguments.Length > 0)
            {
                renderer.LoadFile(Program.arguments[0]);
            }
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            base.OnResize(e);

            renderer.screenSize = new ShurikenRenderer.Vector2(ClientSize.X, ClientSize.Y);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _controller.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 0, 0, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            float deltaTime = (float)(e.Time);
            renderer.Render(renderer.WorkProjectCsd, (float)deltaTime);
            _controller.Render();
            if (renderer.WorkProjectCsd != null)            
                Title = applicationName + $" - [{renderer.config.WorkFilePath}]";

            ImGuiController.CheckGLError("End of frame");
            SwapBuffers();
        }
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            
            
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            
            _controller.MouseScroll(e.Offset);
        }
    }
}
