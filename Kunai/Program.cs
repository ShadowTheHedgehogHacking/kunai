using System.IO;
using System.Threading.Tasks;

namespace Kunai
{
    internal class Program
    {
        public static string[] Arguments;
        public static string Path = Directory.GetParent(System.Environment.ProcessPath).FullName;

        private static void Main(string[] in_Args)
        {
            MainWindow mainWindow = new MainWindow();
            Arguments = in_Args;

            Task.Run(UpdateChecker.CheckUpdate);

            mainWindow.Run();
        }
    }
}
