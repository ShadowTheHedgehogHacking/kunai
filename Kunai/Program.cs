using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Kunai
{
    internal class Program
    {
        public static string[] Arguments;
        public static string Path = System.Environment.ProcessPath;

        private static void Main(string[] in_Args)
        {
            MainWindow wnd = new MainWindow();
            Arguments = in_Args;
            wnd.Run();
        }

        

        public static void AssociateFileTypes()
        {

        }
    }
}
