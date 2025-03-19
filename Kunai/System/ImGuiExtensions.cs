using Hexa.NET.ImGui;


public static class ImGuiE
{
    public static bool IsMouseClicked(int mouseIndex)
    {
        return ImGui.GetIO().MouseClicked[mouseIndex];
    }
}