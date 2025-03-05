using Hexa.NET.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using System.IO;
using System;
using System.Runtime.InteropServices;
using TeamSpettro.SettingsSystem;
using HekonrayBase;
using HekonrayBase.Settings;


namespace Kunai
{
    public class MainWindow : HekonrayWindow
    {
        private IntPtr m_IniName;
        private KunaiProject KunaiProject => (KunaiProject)Project;
        public static ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;

        public MainWindow(Version in_OpenGLVersion, Vector2 in_WindowSize) : base(in_OpenGLVersion, in_WindowSize)
        {
            ApplicationName = "Kunai";
            Title = ApplicationName;
        }

        protected override void OnLoad()
        {
            Project = KunaiProject.Instance;
            KunaiProject.SetWindowParameters(this, new System.Numerics.Vector2(ClientSize.X, ClientSize.Y));
            OnActionWithArgs = LoadFromArgs;
            TeamSpettro.Resources.Initialize(Path.Combine(Program.Path, "config.json"));
            
            base.OnLoad();

            ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
            // Example #10000 for why ImGui.NET is kinda bad
            // This is to avoid having imgui.ini files in every folder that the program accesses
            unsafe
            {
                m_IniName = Marshal.StringToHGlobalAnsi(Path.Combine(Program.Path, "imgui.ini"));
                ImGuiIOPtr io = ImGui.GetIO();
                io.IniFilename = (byte*)m_IniName;
            }
            Windows.Add(ModalHandler.Instance);
            Windows.Add(new Window.MenuBarWindow());
            Windows.Add(new AnimationsWindow());
            Windows.Add(new HierarchyWindow());
            Windows.Add(new InspectorWindow());
            Windows.Add(new ViewportWindow());
            Windows.Add(new CropEditor());
            Windows.Add(new SettingsWindow());
        }

        private void LoadFromArgs(string[] args)
        {
            KunaiProject.LoadFile(args[0]);
        }

        protected override void OnResize(ResizeEventArgs in_E)
        {
            base.OnResize(in_E);
            if(KunaiProject != null)
                KunaiProject.ScreenSize = new System.Numerics.Vector2(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs in_E)
        {
            if(ShouldRender())
            {
                base.OnRenderFrame(in_E);

                float deltaTime = (float)(in_E.Time);
                KunaiProject.Render(KunaiProject.WorkProjectCsd, (float)deltaTime);

                if (KunaiProject.WorkProjectCsd != null)
                    Title = ApplicationName + $" - [{KunaiProject.Config.WorkFilePath}]";
            }
        }
    }
}
