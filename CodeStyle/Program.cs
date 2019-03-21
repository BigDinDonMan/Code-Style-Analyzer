using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CodeStyle.Tree<string>;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CodeStyle {
    class Program {
        static void Main(string[] args) {
            string codePath = @"xxxxxxxxx.c";
            var code = CodeAnalyzer.GetInstance().LoadCode(codePath);
            var defines = CodeAnalyzer.GetInstance().BuildDefineTree(codePath);
            var codeWithBraces = CodeAnalyzer.GetInstance().PreprocessCode(code, defines, LineProcessing.BracesOnly);
            //codeWithBraces?.Print();
            var funcs = CodeAnalyzer.GetInstance().GetFunctions(codeWithBraces);
           // funcs?.Print();
            List<string> preprocessedFuncs = new List<string>();
            foreach (var func in funcs) {
                preprocessedFuncs.Add(String.Join("\n", CodeAnalyzer.GetInstance().PreprocessCode(func.Split('\n').ToList(), defines)));
            }
            preprocessedFuncs?.Print();
            var s1 = CodeAnalyzer.GetInstance().GetAndClearComments(preprocessedFuncs[0].Split('\n').ToList());
            s1.Item2.Print();
            Console.WriteLine("{0}", CodeAnalyzer.GetInstance().CountForbiddenWords(s1.Item2));
            s1.Item1.Clear();
            s1.Item2.Clear();
            
            GC.Collect();
            Console.ReadKey();
        }
    }
}
