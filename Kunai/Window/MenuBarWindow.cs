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
        public static float menuBarHeight = 32;
        private static readonly string filters = "xncp,yncp,gncp;xncp;yncp;gncp";

        public static string AddQuotesIfRequired(string path)
        {
            return !string.IsNullOrWhiteSpace(path) ?
                path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
                    "\"" + path + "\"" : path :
                    string.Empty;
        }
        public static void ExecuteAsAdmin(string fileName)
        {
            fileName = AddQuotesIfRequired(fileName);
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }
        public override void Update(ShurikenRenderHelper in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBarWindow.menuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open CSD Project..."))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileOpen(filters);
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
                        ExecuteAsAdmin(@Path.Combine(Directory.GetParent(@Program.path).FullName, "FileTypeRegisterService.exe"));
                    }

                    ImGui.EndMenu();

                }


            }
            ImGui.EndMainMenuBar();
        }
    }
}
