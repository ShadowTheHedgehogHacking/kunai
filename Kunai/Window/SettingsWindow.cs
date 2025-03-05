using HekonrayBase;
using HekonrayBase.Base;
using HekonrayBase.Settings;
using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Numerics;
using TeamSpettro.SettingsSystem;

namespace Kunai.Window
{
    public class SettingsWindow : Singleton<CropEditor>, IWindow
    {
        public static bool Enabled = false;
        bool _themeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");

        public void OnReset(IProgramProject in_Renderer)
        {
            throw new NotImplementedException();
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            if (Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Settings"))
                {
                    int currentTheme = _themeIsDark ? 1 : 0;
                    var color = renderer.ViewportColor;
                    if (ImGui.Combo("Theme", ref currentTheme, ["Light", "Dark"], 2))
                    {
                        _themeIsDark = currentTheme == 1;
                        SettingsManager.SetBool("IsDarkThemeEnabled", _themeIsDark);
                        ImGuiThemeManager.SetTheme(_themeIsDark);
                    }
                    if (ImGui.ColorEdit3("Viewport Color", ref color))
                    {
                        renderer.ViewportColor = color;
                    }
                    ImGui.End();
                }
            }
        }
    }
}
