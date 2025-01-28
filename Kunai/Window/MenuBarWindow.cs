using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public class MenuBarWindow : WindowBase
    {
        public static float MenuBarHeight = 32;
        private static readonly string Filters = "xncp,yncp,gncp;xncp;yncp;gncp";

        public static string AddQuotesIfRequired(string in_Path)
        {
            return !string.IsNullOrWhiteSpace(in_Path) ?
                in_Path.Contains(" ") && (!in_Path.StartsWith("\"") && !in_Path.EndsWith("\"")) ?
                    "\"" + in_Path + "\"" : in_Path :
                    string.Empty;
        }
        public static void ExecuteAsAdmin(string in_FileName)
        {
            in_FileName = AddQuotesIfRequired(in_FileName);
            Process proc = new Process();
            proc.StartInfo.FileName = in_FileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }
        public override void Update(ShurikenRenderHelper in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBarWindow.MenuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open CSD Project..."))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileOpen(Filters);
                        if (testdial.IsOk)
                        {
                            in_Renderer.LoadFile(@testdial.Path);
                        }
                    }
                    if (ImGui.MenuItem("Save", "Ctrl + S"))
                    {
                        in_Renderer.SaveCurrentFile(null);
                    }
                    ImGui.EndMenu();
                }
                if(ImGui.BeginMenu("Edit"))
                {
                    if(ImGui.MenuItem("Associate extensions"))
                    {
                        ExecuteAsAdmin(@Path.Combine(Directory.GetParent(@Program.Path).FullName, "FileTypeRegisterService.exe"));
                    }

                    ImGui.EndMenu();

                }


            }
            ImGui.EndMainMenuBar();
        }
    }
}
