using HekonrayBase;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kunai
{
    internal class Program
    {
        public static string[] Arguments;
        public static string Path = Directory.GetParent(System.Environment.ProcessPath).FullName;
        public static string PathToExec = System.Environment.ProcessPath;

        private static void Main(string[] in_Args)
        {
            Application app = new Application();
            app.Start(in_Args);
            MainWindow mainWindow = new MainWindow(new Version(3,3), new Vector2(1600, 900));
            Arguments = in_Args;

            Task.Run(UpdateChecker.CheckUpdate);

            mainWindow.Run();
        }
    }
}
