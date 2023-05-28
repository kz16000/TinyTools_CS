using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImgFileRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = System.Environment.GetCommandLineArgs();

            Assembly asm = Assembly.GetExecutingAssembly();
            Version ver = asm.GetName().Version;
            Console.WriteLine("ImgRenamer (Version " + ver + ")");

            if ( files.Length > 1 )
            {
                for (int i = 1; i < files.Length; ++i)
                {
                    ImgFileProcessor p = new ImgFileProcessor(files[i]);
                    p.Process();
                }
            }
            else
            {
                Console.WriteLine("No file dropped.\n");
            }

            Console.WriteLine("==== Press Return. ====");
            Console.ReadLine();
        }
    }
}
