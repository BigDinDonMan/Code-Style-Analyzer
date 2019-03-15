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
            try {

                var codeLines = CodeAnalyzer.GetInstance().LoadCode(@"xxxxxxxxx.c");
                //var comments = CodeAnalyzer.GetInstance().GetCodeComments(codeLines);
                //// comments.Print();
                var funcs = CodeAnalyzer.GetInstance().GetFunctions(codeLines);
                var defineTree = CodeAnalyzer.GetInstance().BuildDefineTree(@"xxxxxxxxx.c");
                var processedCode = CodeAnalyzer.GetInstance().PreprocessCode(codeLines, defineTree);
                processedCode.Print();
            } catch (Exception) { return; }
           // double ratio = CodeAnalyzer.GetInstance().GetCommentKeywordRatio(comments);
            // Console.WriteLine(ratio.ToString());
            // Console.WriteLine("ala ma kotakotakota".GetOcurrenceCount("kota"));
            //string s = "// japierdole jaka faza XD\n";
            //funcs.Print();
            //comments.Print();
           // comments.Print();
            Console.ReadKey();
        }
    }
}
