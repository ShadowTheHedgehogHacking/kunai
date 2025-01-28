using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Kunai
{
    class Program
    {
        public static string[] arguments;
        public static string path = System.Environment.ProcessPath;
        static void Main(string[] args)
        {
            MainWindow wnd = new MainWindow();
            arguments = args;
            wnd.Run();
        }

        

        public static void AssociateFileTypes()
        {

        }
    }
}
