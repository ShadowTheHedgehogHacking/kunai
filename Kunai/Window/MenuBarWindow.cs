using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public class MenuBarWindow : WindowBase
    {
        public static float MenuBarHeight = 32;
        private static readonly string FiltersOpen = "xncp,yncp,gncp,sncp";
        private static readonly string Filters = "xncp;yncp;gncp;sncp";

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
        //https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        public override void Update(KunaiProject in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBarWindow.MenuBarHeight = ImGui.GetWindowSize().Y;
                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open File..."))
                    {
                        var dialog = NativeFileDialogSharp.Dialog.FileOpen(FiltersOpen);
                        if (dialog.IsOk)
                        {
                            in_Renderer.LoadFile(dialog.Path);
                        }
                    }
                    if (ImGui.MenuItem("Reload File"))
                    {
                        in_Renderer.WorkProjectCsd = null;
                        in_Renderer.LoadFile(in_Renderer.Config.WorkFilePath);
                    }
                    if (ImGui.BeginMenu("Save"))
                    {
                        if (ImGui.MenuItem("Csd Project...", "Ctrl + S"))
                        {
                            in_Renderer.SaveCurrentFile(null);
                        }
                        ImGui.BeginDisabled();
                        if (ImGui.MenuItem("Colors GNCP"))
                        {
                            in_Renderer.ExportProjectChunk(null, false);
                        }
                        ImGui.EndDisabled();
                        if (ImGui.MenuItem("Colors Ultimate XNCP"))
                        {
                            in_Renderer.ExportProjectChunk(null, true);

                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Save as..."))
                    {
                        if (ImGui.MenuItem("Csd Project...", "Ctrl + S"))
                        {
                            var dialog = NativeFileDialogSharp.Dialog.FileSave(Filters);
                            if (dialog.IsOk)
                            {
                                string path = dialog.Path;
                                if (!Path.HasExtension(path)) path += ".xncp";
                                in_Renderer.SaveCurrentFile(path);
                            }
                        }
                        ImGui.BeginDisabled();
                        if (ImGui.MenuItem("Colors GNCP"))
                        {
                            var dialog = NativeFileDialogSharp.Dialog.FileSave("gncp");
                            if (dialog.IsOk)
                            {
                                string path = dialog.Path;
                                if (!Path.HasExtension(path)) path += ".gncp";
                                in_Renderer.ExportProjectChunk(path, false);
                            }
                        }
                        ImGui.EndDisabled();
                        if (ImGui.MenuItem("Colors Ultimate XNCP"))
                        {
                            var dialog = NativeFileDialogSharp.Dialog.FileSave("xncp");
                            if (dialog.IsOk)
                            {
                                string path = dialog.Path;
                                if (!Path.HasExtension(path)) path += ".xncp";
                                in_Renderer.ExportProjectChunk(path, true);
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Associate extensions"))
                    {
                        ExecuteAsAdmin(@Path.Combine(Directory.GetParent(@Program.Path).FullName, "FileTypeRegisterService.exe"));
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.BeginMenu("Windows"))
                    {
                        if (ImGui.MenuItem("Sprite Crop Editor", CropEditor.Enabled))
                        {
                            CropEditor.Enabled = !CropEditor.Enabled;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("How to use Kunai"))
                    {
                        OpenUrl("https://wiki.hedgedocs.com/index.php/How_to_use_Kunai");
                    }
                    if (ImGui.MenuItem("Report a bug"))
                    {
                        OpenUrl("https://github.com/NextinMono/kunai/issues/new");
                    }

                    ImGui.EndMenu();
                }
            }

            if (UpdateChecker.UpdateAvailable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0.7f, 1, 1)));
                var size = ImGui.CalcTextSize("Update Available!").X;
                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - size - ImGui.GetStyle().ItemSpacing.X * 2);
                if (ImGui.Selectable("Update Available!"))
                {
                    OpenUrl("https://github.com/NextinMono/kunai/releases/latest");
                }
                ImGui.PopStyleColor();
            }            
            
            ImGui.EndMainMenuBar();
        }
    }
}