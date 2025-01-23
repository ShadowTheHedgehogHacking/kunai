using ImGuiNET;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public static class MenuBarWindow
    {
        public static float menuBarHeight = 32;

        public static void Render(ShurikenRenderHelper in_Renderer)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBarWindow.menuBarHeight = ImGui.GetWindowSize().Y;
                uint test = ImGui.GetWindowDockID();
                ImGui.Text(test.ToString());
                if (ImGui.Button("Trigger load prompt"))
                {
                    var testdial = NativeFileDialogSharp.Dialog.FileOpen();
                    if (testdial.IsOk)
                    {
                        in_Renderer.LoadFile(@testdial.Path);
                        //else
                        //{
                        //    //GNCP/SNCP requires TXDs
                        //    if (extension == ".gncp" || extension == ".sncp")
                        //    {
                        //        GSncpImportWindow windowImport = new GSncpImportWindow();
                        //        windowImport.ShowDialog();
                        //    }
                        //    else
                        //        MessageBox.Show("The loaded UI file has an invalid texture list, textures will not load.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //}
                    }
                }
            }
            ImGui.EndMainMenuBar();
        }
    }
}
