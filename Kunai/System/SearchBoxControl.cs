using Hexa.NET.ImGui;

namespace Kunai
{
    public class TempSearchBox
    {
        public string SearchTxt = "";
        public string MComparisonString = "";
        public string MSearchTxtCopy = "";

        //Call 1st
        public void Render()
        {
            ImGui.TextUnformatted("Search  ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##Search", ref SearchTxt, 256);
            ImGui.Separator();
        }

        //Call 2nd
        public void Update(string in_Str)
        {
            MComparisonString = in_Str.ToLower();
            MSearchTxtCopy = SearchTxt.ToLower();
        }

        //Call where result is needed
        public bool MatchResult()
        {
            return string.IsNullOrEmpty(MSearchTxtCopy) ? true : MComparisonString.Contains(MSearchTxtCopy);
        }

    }
}