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

            //string sourcepath = @"test.c";
            //Stopwatch sw = Stopwatch.StartNew();
            //List<string> loadedCode = CodeAnalysis.LoadCodeLines(sourcepath);
            //var nonMacroDefines = CodeAnalysis.GetNonMacroDefines(loadedCode);

            //foreach (var def in nonMacroDefines) {
            //    Console.WriteLine(def);
            //}

            //List<Tree<string>> defineTree = CodeAnalysis.BuildNonMacroDefineTree(nonMacroDefines);
            //List<Tree<string>> invalidDefines = CodeAnalysis.GetInvalidDefines(defineTree);

            //loadedCode = CodeAnalysis.ClearCodeComments(loadedCode);
            //loadedCode = CodeAnalysis.FullFilter(loadedCode);
            //foreach (var line in loadedCode) Console.WriteLine(line);

            //int[] operatorsWithoutSpacesOccurences = CodeAnalysis.CheckOperatorsSpaces(loadedCode);

            //Console.WriteLine(operatorsWithoutSpacesOccurences.Length);
            //sw.Stop();

            //bool consistent = CodeAnalysis.AreBraceNewlinesConsistent(loadedCode);

            ////  CodeAnalysis.AnalyzeCodeStyle("test.c");
            //Console.WriteLine(consistent.ToString());
            //CodeAnalysis.CheckLoopsStatements(loadedCode);

            //Console.WriteLine(sw.Elapsed.ToString());

            //Stopwatch sw1 = Stopwatch.StartNew();

            //int x = CodeAnalysis.ProfanityOccurences(loadedCode);

            //sw1.Stop();

            //Console.WriteLine(x);

            //Console.WriteLine(sw1.Elapsed.ToString());

            //Console.ReadKey();

            Stopwatch sw = Stopwatch.StartNew();

            string path = @"4.31.c";
            var codeLines = CodeAnalysis.LoadCodeLines(path);

            var comments = CodeAnalysis.GetCodeComments(codeLines);
            double commentedOutCodeRatio = CodeAnalysis.GetCommentedOutCodeRatio(comments);
            var codeWithoutComments = CodeAnalysis.ClearCodeComments(codeLines);

            if (commentedOutCodeRatio > 0.1) {
                Console.WriteLine("program has detected that you have some commented out code.");
            } else {
                Console.WriteLine("Commented out code: clear");
            }

            var nonMacroDefines = CodeAnalysis.GetNonMacroDefines(codeWithoutComments);
            var defineTree = CodeAnalysis.BuildNonMacroDefineTree(nonMacroDefines);
            var invalidDefines = CodeAnalysis.GetInvalidDefines(defineTree);
            if (invalidDefines.Count > 0) {
                Console.WriteLine("program has detected that some of your defines don't make sense (define for a keyword or existing function name). please change it ASAP");
            }

            if (Extensions.IsNullOrEmpty(defineTree) == false) {
                var preprocessedCode = CodeAnalysis.PreprocessCode(codeWithoutComments, defineTree, LineProcessing.All);
                foreach (var line in preprocessedCode) {
                    Console.WriteLine(line);
                }
            }
            var filteredCode = CodeAnalysis.FullFilter(codeWithoutComments);
            filteredCode = CodeAnalysis.FilterOutPreprocessor(filteredCode);

            var measuredIndents = CodeAnalysis.MeasureLineIndents(filteredCode);
            var indentDict = CodeAnalysis.GetIndentDictionary(measuredIndents);

            int incosistentIndentations = CodeAnalysis.GetIrregularIndentsCount(indentDict);
            if (incosistentIndentations != 0) {
                Console.WriteLine("program has detected that indentations are irregular.");
            }

            bool consistentBraceStyle = CodeAnalysis.AreBraceNewlinesConsistent(filteredCode);
            if (!consistentBraceStyle) {
                Console.WriteLine("program has detected that braces are not in a single style (newline or same line)");
            }

            //var profanityOccurences = CodeAnalysis.ProfanityOccurences(filteredCode);
            //if (profanityOccurences.Length != 0) {
            //    Console.WriteLine("program has detected vulgar language. please change it ASAP");
            //    foreach (var s in profanityOccurences) {
            //        Console.WriteLine(s);
            //    }
            //}

            var operatorsCheck = CodeAnalysis.CheckOperatorsSpaces(filteredCode);
            if (operatorsCheck.Length > 0) {
                string code = String.Join("\n", filteredCode);
                foreach (var i in operatorsCheck) {
                    try {
                        Console.WriteLine(code.Substring(i - 1, 3));
                    } catch (Exception) { }
                }
                Console.WriteLine("program has detected that you are not using spaces between operators. please change it.");
            }

            var tooLongLines = CodeAnalysis.CheckLinesLength(filteredCode);
            if (tooLongLines > 0) {
                Console.WriteLine("program has detected that some of your code lines are too long. consider shortening them.");
            }

            sw.Stop();
            Console.WriteLine("Done.");
            Console.WriteLine($"Time taken for analysis: {sw.Elapsed}");

            Console.ReadKey();
        }
    }
}
