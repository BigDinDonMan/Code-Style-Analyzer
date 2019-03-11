using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeStyle.Tree<string>;
using System.IO;
using Newtonsoft.Json;

namespace CodeStyle {
    class Program {
        static void Main(string[] args) {
            var codeLines = CodeAnalyzer.GetInstance().LoadCode(@"xxxxxxxxx.c");
            var comments = CodeAnalyzer.GetInstance().GetCodeComments(codeLines);
            comments.Print();
            Console.ReadKey();
        }
    }
}
