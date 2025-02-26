using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SharpNeedle.Framework.Ninja.Csd;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using System.Windows;
using Hexa.NET.ImPlot;
using OpenTK.Windowing.Common.Input;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace Kunai
{
    public class MainWindow : GameWindow
    {
        public static bool IsMouseLeftDown;
        public static readonly string ApplicationName = "Kunai";
        private static MemoryStream _iconData;
        private float _test = 1;
        private ImGuiController _controller;
        public static KunaiProject Renderer;
        public static ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings(){ Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
        { }
        protected override void OnLoad()
        {
            base.OnLoad();
            Title = ApplicationName;
            Renderer = new KunaiProject(this, new System.Numerics.Vector2(1280, 720), new System.Numerics.Vector2(ClientSize.X, ClientSize.Y));
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

            Renderer.Windows.Add(new MenuBarWindow());
            Renderer.Windows.Add(new AnimationsWindow());
            Renderer.Windows.Add(new HierarchyWindow());
            Renderer.Windows.Add(new InspectorWindow());
            Renderer.Windows.Add(new ViewportWindow());
            Renderer.Windows.Add(new CropEditor());
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
