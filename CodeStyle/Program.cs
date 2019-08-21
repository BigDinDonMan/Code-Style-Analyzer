using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CodeStyle.Tree<string>;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using StringUtils;
using Utils;

namespace CodeStyle {
    class Program {
        static void Main(string[] args) {
            string sourcepath = @"4.31.c";
            List<string> loadedCode = CodeAnalysis.LoadCodeLines(sourcepath);

            List<Tree<string>> defineTree = CodeAnalysis.BuildNonMacroDefineTree(CodeAnalysis.GetNonMacroDefines(loadedCode));
            List<Tree<string>> invalidDefines = CodeAnalysis.GetInvalidDefines(defineTree);

            loadedCode = CodeAnalysis.ClearCodeComments(loadedCode);
            loadedCode = CodeAnalysis.FullFilter(loadedCode);

            int[] operatorsWithoutSpacesOccurences = CodeAnalysis.CheckOperatorsSpaces(loadedCode);

            Console.WriteLine(operatorsWithoutSpacesOccurences.Length);

            string code = String.Join("\n", loadedCode);
            foreach (var index in operatorsWithoutSpacesOccurences) {
                try {
                    Console.WriteLine(code.Substring(index - 1, 3));
                } catch (Exception e) {

                }
            }

            Console.ReadKey();
        }

        static void WhileCheck() {
            string whileLoop = File.ReadAllText("test.c");
            int index = whileLoop.IndexOf("while");
            String s = null;
            while (whileLoop[index] != '}') {
                s += whileLoop[index++];
            }
            foreach (var c in s) {
                Console.WriteLine("numeric: {0}, character: {1}", (int)c, c);
            }
            Console.WriteLine("tab: {0}", (int)'\t');
        }
    }
}
