using Hexa.NET.ImGui;
using IconFonts;
using Kunai.Settings;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Numerics;
using TeamSpettro.SettingsSystem;

namespace Kunai.Window
{
    public class SettingsWindow : WindowBase
    {
        public static bool Enabled = false;
        bool _themeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");

        public override void Update(KunaiProject in_Renderer)
        {
            if (Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Settings"))
                {
                    int currentTheme = _themeIsDark ? 1 : 0;
                    var color = in_Renderer.ViewportColor;
                    if (ImGui.Combo("Theme", ref currentTheme, ["Light", "Dark"], 2))
                    {
                        _themeIsDark = currentTheme == 1;
                        SettingsManager.SetBool("IsDarkThemeEnabled", _themeIsDark);
                        ImGuiThemeManager.SetTheme(_themeIsDark);
                    }
                    if (ImGui.ColorEdit3("Viewport Color", ref color))
                    {
                        in_Renderer.ViewportColor = color;
                    }
                    ImGui.End();
                }
            }
        }
    }
}
