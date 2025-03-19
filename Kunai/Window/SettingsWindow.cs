using HekonrayBase;
using HekonrayBase.Base;
using HekonrayBase.Settings;
using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TeamSpettro.SettingsSystem;

namespace Kunai.Window
{
    public class SettingsWindow : Singleton<CropEditor>, IWindow
    {
        public static bool Enabled = false;
        bool m_ThemeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");
        public static string AddQuotesIfRequired(string in_Path)
        {
            return !string.IsNullOrWhiteSpace(in_Path) ?
                in_Path.Contains(" ") && (!in_Path.StartsWith("\"") && !in_Path.EndsWith("\"")) ?
                    "\"" + in_Path + "\"" : in_Path :
                    string.Empty;
        }
        public static void ExecuteAsAdmin(string in_FileName)
        {
            //If the user cancels the UAC prompt, it'll throw an exception
            //so just do nothing in case that happens
            try
            {

                in_FileName = AddQuotesIfRequired(in_FileName);
                Process proc = new Process();
                proc.StartInfo.FileName = in_FileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch(Exception e)
            {

            }
        }
        public void OnReset(IProgramProject in_Renderer)
        {
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            if (Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Settings", ref Enabled))
                {
                    int currentTheme = m_ThemeIsDark ? 1 : 0;
                    var color = renderer.ViewportColor;
                    if (ImGui.Combo("Theme", ref currentTheme, ["Light", "Dark"], 2))
                    {
                        m_ThemeIsDark = currentTheme == 1;
                        SettingsManager.SetBool("IsDarkThemeEnabled", m_ThemeIsDark);
                        ImGuiThemeManager.SetTheme(m_ThemeIsDark);
                    }
                    if (ImGui.ColorEdit3("Viewport Color", ref color))
                    {
                        renderer.SetViewportColor(color);
                    }

                    if(ImGui.Button("Associate file extensions"))
                        ExecuteAsAdmin(@Path.Combine(@Program.Path, "FileTypeRegisterService.exe"));
                    ImGui.End();
                }
            }
        }
    }
}
