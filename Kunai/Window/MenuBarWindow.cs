using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Kunai.Window
{
    public class MenuBarWindow : Singleton<MenuBarWindow>, IWindow
    {
        public static float MenuBarHeight = 32;
        private static readonly string FiltersOpen = "xncp,yncp,gncp,sncp";
        private static readonly string Filters = "xncp;yncp;gncp;sncp";

        
        //https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string in_Url)
        {
            try
            {
                Process.Start(in_Url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    in_Url = in_Url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(in_Url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", in_Url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", in_Url);
                }
                else
                {
                    throw;
                }
            }
        }
        public void OnReset(IProgramProject in_Renderer)
        {
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
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
                            renderer.LoadFile(dialog.Path);
                        }
                    }
                    if (ImGui.MenuItem("Reload File"))
                    {
                        renderer.WorkProjectCsd = null;
                        renderer.LoadFile(renderer.Config.WorkFilePath);
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Save"))
                    {
                        if (ImGui.MenuItem("Csd Project...", "Ctrl + S"))
                        {
                            renderer.SaveCurrentFile(null);
                        }
                        ImGui.BeginDisabled();
                        if (ImGui.MenuItem("Colors GNCP"))
                        {
                            renderer.ExportProjectChunk(null, false);
                        }
                        ImGui.EndDisabled();
                        if (ImGui.MenuItem("Colors Ultimate XNCP"))
                        {
                            renderer.ExportProjectChunk(null, true);

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
                                renderer.SaveCurrentFile(path);
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
                                renderer.ExportProjectChunk(path, false);
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
                                renderer.ExportProjectChunk(path, true);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                    {
                        Environment.Exit(0);
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.BeginMenu("Reference Image"))
                    {
                        if(ImGui.MenuItem("Load"))
                        {
                            var dialog = NativeFileDialogSharp.Dialog.FileOpen("png,jpg,jpeg,dds");
                            if (dialog.IsOk)
                            {
                                renderer.LoadReferenceImage(dialog.Path);
                                renderer.referenceImageData.opacity = 1;
                            }
                        }
                        if (ImGui.MenuItem("Remove"))
                        {
                            renderer.referenceImageData.enabled = false;
                        }
                        ImGui.Separator();
                        if (ImGui.BeginMenu("Opacity"))
                        {
                            float transparency = renderer.referenceImageData.opacity;
                            ImGui.SliderFloat("##referenceopacity", ref transparency, 0, 1);
                            renderer.referenceImageData.opacity = transparency;
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Preferences", SettingsWindow.Enabled))
                    {
                        SettingsWindow.Enabled = !SettingsWindow.Enabled;
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Tools"))
                {
                    if (ImGui.MenuItem("Scan folder for textures"))
                    {
                        var e = NativeFileDialogSharp.Dialog.FolderPicker("");
                        if (e.IsOk)
                        {
                            var files = Directory.EnumerateFiles(e.Path, "*.*", SearchOption.AllDirectories).ToList();
                            var textureNames = new HashSet<string>(SpriteHelper.Textures.Select(t => t.Name));

                            foreach (string file in files)
                            {
                                if (textureNames.Any(file.Contains))
                                {
                                    string newPath = Path.Combine(Directory.GetParent(renderer.Config.WorkFilePath).FullName, Path.GetFileName(file));
                                    if (!File.Exists(newPath))
                                        File.Copy(file, newPath);
                                }
                            }
                        }
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
                    if (ImGui.MenuItem("Show Cast quads", renderer.Config.ShowQuads))
                        renderer.Config.ShowQuads = !renderer.Config.ShowQuads;
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