using System;
using System.Linq;

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

            foreach (var author in parser.Authors)
            {
                Console.WriteLine("- " + author);
            }
        }
    }
}
