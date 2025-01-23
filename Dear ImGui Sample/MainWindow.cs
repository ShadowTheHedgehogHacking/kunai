using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SharpNeedle.Ninja.Csd;
using Kunai.ShurikenRenderer;
using Kunai.Window;


namespace Kunai
{
    public class MainWindow : GameWindow
    {
        ImGuiController _controller;
        public static ShurikenRenderHelper renderer;
        public MainWindow() : base(GameWindowSettings.Default, new NativeWindowSettings(){ Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            renderer = new ShurikenRenderHelper(new ShurikenRenderer.Vector2(1280, 720));

            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);

            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        }
        
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _controller.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            // Enable Docking
            ImGui.DockSpaceOverViewport();

           ImGui.ShowDemoWindow();

            if(ImGui.Begin("Test"))
            {
                if(ImGui.Button("Trigger load prompt"))
                {
                    var testdial = NativeFileDialogSharp.Dialog.FileOpen();
                    if (testdial.IsOk)
                    {
                        renderer.LoadFile(@testdial.Path);
                        //else
                        //{
                        //    //GNCP/SNCP requires TXDs
                        //    if (extension == ".gncp" || extension == ".sncp")
                        //    {
                        //        GSncpImportWindow windowImport = new GSncpImportWindow();
                        //        windowImport.ShowDialog();
                        //    }
                        //    else
                        //        MessageBox.Show("The loaded UI file has an invalid texture list, textures will not load.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //}
                    }
                }

                if (renderer.WorkProjectCsd != null)
                {
                    HierarchyWindow.Render(renderer.WorkProjectCsd);
                    InspectorWindow.Render(renderer.WorkProjectCsd);
                   //GL.BlendEquation(BlendEquationMode.FuncAdd);
                   //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                   //GL.Enable(EnableCap.FramebufferSrgb);
                    
                }
            }
            ImGui.End();

            _controller.Render();
            if (renderer.WorkProjectCsd != null)
            {
                renderer.Render(renderer.WorkProjectCsd, (float)e.Time);
            }

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
