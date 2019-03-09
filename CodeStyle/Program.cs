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
            //List<string> code = CodeAnalyzer.GetInstance().LoadCode(@"28.6.c"/*@"xxxxxxxxx.c"*/);

            /*List<string> funcs = CodeAnalyzer.GetInstance().GetFunctions(code);
            funcs.Print();
            CodeAnalyzer.GetInstance().GetDefines(code);
            Console.WriteLine("coś {0}, {1}, {2}", 5, "beniz", 6.2);*/
            /*TreeNode<string> root = new TreeNode<string>("xD");
            Tree<string> t = new Tree<string>(root);
            t.root.AddChild(new TreeNode<string>("tsooo"));
            t.root.AddChild(new TreeNode<string>("xDD"));
            Tree<string>.Print(t.root);*/
            /* string s1, s2;
             s1 = "kit";
             s2 = "kot";
             int distance = CodeAnalyzer.GetInstance().LevenshteinDistance(s1, s2);
             Console.WriteLine("distance: {0}", distance.ToString());
             Console.ReadKey();

             Tree<string> strings = new Tree<string>(new TreeNode<string>("coś here"));
             strings.root.AddChild(new TreeNode<string>("xD"));
             strings.root.AddChild(new TreeNode<string>("x1"));
             strings.root.AddChild(new TreeNode<string>("x2"));
             strings.root.GetChild(0).AddChild(new TreeNode<string>("s"));
             Print(strings.root);
             Console.WriteLine("{0}", Find(strings.root, "xD").Value.ToString());
             //Console.WriteLine(Tree<string>.Ge)
             Console.ReadKey();*/
            /*  string path = @"definetest.c";
              List<string> lines = CodeAnalyzer.GetInstance().LoadCode(path);
              var defines = CodeAnalyzer.GetInstance().GetDefines(lines);
              foreach (var entry in defines) {
                  Console.Write("key: {0}, values: ", entry.Key);
                  foreach (var value in entry.Value) {
                      Console.Write("{0} ", value);
                  }
              }
              Console.ReadKey();*/
            /*string path = "definetest.c";
            var tree = CodeAnalyzer.GetInstance().BuildDefineTree(path);
            foreach (var t in tree) {
                Print(t.root);
            }
            GC.Collect();
            Console.ReadKey();*/
            //string[] operators = new string[] {"+", "-", "++", "--", "*", "/", "%", "==", "!=", "<", ">", "<=", ">=", "&&", "||", "!", "~", "&", "|", "^", "<<", ">>", "=", "+=", "-=", "*=", "/=", "%=", "<<=", ">>=", "^=", "&=", "|=", "sizeof"};
            //string[] keywords = new string[] {
            //    "auto",
            //    "break",
            //    "case",
            //    "char",
            //    "const",
            //    "continue",
            //    "default",
            //    "do",
            //    "double",
            //    "else",
            //    "enum",
            //    "extern",
            //    "float",
            //    "for",
            //    "goto",
            //    "if",
            //    "inline",
            //    "int",
            //    "long",
            //    "register",
            //    "restrict",
            //    "return",
            //    "short",
            //    "signed",
            //    "sizeof",
            //    "static",
            //    "struct",
            //    "switch",
            //    "typedef",
            //    "union",
            //    "unsigned",
            //    "void",
            //    "volatile",
            //    "while"
            // };
            //using (var fs = new FileStream("../../ProgramFiles/operators.json", FileMode.OpenOrCreate)) {
            //    using (var writer = new StreamWriter(fs)) {
            //        writer.Write(JsonConvert.SerializeObject(operators));
            //    }
            //}

            //using (var fs = new FileStream("../../ProgramFiles/keywords.json", FileMode.OpenOrCreate)) {
            //    using (var writer = new StreamWriter(fs)) {
            //        writer.Write(JsonConvert.SerializeObject(keywords));
            //    }
            //}
        }
    }
}
