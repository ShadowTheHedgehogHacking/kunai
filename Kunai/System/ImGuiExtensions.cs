using HekonrayBase;
using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using OpenTK.Windowing.GraphicsLibraryFramework;


public static class ImGuiE
{
    public static bool IsMouseClicked(ImGuiMouseButton in_MouseIndex) => IsMouseClicked(in_MouseIndex);
    public static bool IsMouseClicked(int in_MouseIndex)
    {
        return ImGui.GetIO().MouseClicked[in_MouseIndex];
    }
    public static bool IsKeyDown(Keys in_KeyData)
    {
        return KunaiProject.Instance.KeyboardState.IsKeyDown(in_KeyData);
    }
    public static bool IsKeyTapped(Keys in_KeyData)
    {
        return KunaiProject.Instance.KeyboardState.IsKeyPressed(in_KeyData);
    }
}