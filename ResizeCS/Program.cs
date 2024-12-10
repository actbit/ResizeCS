using SkiaSharp;
using ConsoleAppFramework;
namespace ResizeCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string filePath = args[0];
            var app = ConsoleApp.Create();
            app.Add<Resize>();
            app.Run(args);
            
        }
    }
}
