using Hexa.NET.ImGui;


public static class ImGuiE
{
    public static bool IsMouseClicked(int in_MouseIndex)
    {
        return ImGui.GetIO().MouseClicked[in_MouseIndex];
    }
}