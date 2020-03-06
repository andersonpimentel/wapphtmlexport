using System;
using System.Diagnostics;
using System.IO;

namespace wapphtmlexport
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentException(nameof(args));
            }

            var parser = new Parser(args[0]);

            var html = parser.ExportHtml();

            var fileName = Path.GetRandomFileName() + ".html";

            File.WriteAllText(fileName, html);

            Process.Start(fileName);
        }
    }
}
