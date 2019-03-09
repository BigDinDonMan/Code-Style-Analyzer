using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

//TODO: implement function tree
//TODO: implement C Function object
//TODO: filter out commented out code 

namespace CodeStyle {
    public class CodeAnalyzer {

        public readonly string[] operators = null;

        public readonly string[] keywords = null;

        private static CodeAnalyzer instance = null;

        public static CodeAnalyzer GetInstance() {
            if (instance == null) instance = new CodeAnalyzer();
            return instance;
        }

        private CodeAnalyzer() {
            operators = GetOperators();
            keywords = GetKeywords();
        }

        protected string[] GetKeywords() {
            string[] words = null;
            using (var reader = new StreamReader("../../ProgramFiles/keywords.json")) {
                words = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
            }
            return words;
        }
        
        protected string[] GetOperators() {
            string[] operators = null;
            using (var reader = new StreamReader("../../ProgramFiles/operators.json")) {
                operators = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
            }
            return operators;
        }

        public List<string> LoadCode(string path) {
            if (String.IsNullOrEmpty(path)) throw new NullReferenceException();
            if (!File.Exists(path)) throw new FileNotFoundException();

            List<string> result = new List<string>();

            using (var reader = new StreamReader(path)) {
                while (!reader.EndOfStream) {
                    result.Add(reader.ReadLine());
                }
            }

            return result;
        }

        public List<string> GetFunctions(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();

            List<string> copy = new List<string>(lines);
            copy = copy.Where(line => !line.Contains("#")).ToList();

            int index = copy.FindIndex(s => s == "{");
            if (index < 0) {
                int secondIndex = copy.FindIndex(s => s.Contains("{"));
                if (secondIndex > 0) {
                    List<string> s = new List<string>();
                    for (int i = secondIndex; i < copy.Count; ++i) {
                        s.Add(copy[i]);
                    }
                    copy = s;
                }
            }
            string code = String.Join("\n", copy.ToArray());
            List<string> functions = new List<string>();
            int leftBraceCount = 0, rightBraceCount = 0;

            string func = null;

            for (int i = 0; i < code.Length; ++i) {

                func += code[i];
                if (code[i] == '{') leftBraceCount++;
                if (code[i] == '}') rightBraceCount++;

                if (leftBraceCount != 0 && rightBraceCount != 0) {
                    if (leftBraceCount == rightBraceCount) {
                        leftBraceCount = rightBraceCount = 0;
                        functions.Add(func);
                        func = null;
                    }
                }

            }

            return functions.Select(s => s.Trim()).ToList();
        }

        public int LevenshteinDistance(string first, string second) {
            if (first == null || second == null) throw new NullReferenceException();

            if (first.Length == 0) return second.Length;
            if (second.Length == 0) return first.Length;

            int width = first.Length, height = second.Length;
            int[,] levenshteinMatrix = new int[height, width];

            for (int i = 0; i < width; ++i) {
                levenshteinMatrix[0, i] = i;
            }

            for (int i = 0; i < height; ++i) {
                levenshteinMatrix[i, 0] = i;
            }

            int cost = 0;

            for (int i = 1; i < height; ++i) {
                for (int j = 1; j < width; ++j, cost = 0) {
                    if (first[j] != second[i]) cost = 1;
                    int minimum = Math.Min(
                        Math.Min(levenshteinMatrix[i - 1, j] + 1, levenshteinMatrix[i, j - 1] + 1),
                        levenshteinMatrix[i - 1, j - 1] + cost
                    );
                    levenshteinMatrix[i, j] = minimum;
                }
            }

            return levenshteinMatrix[height - 1, width - 1];
        }

        public List<Tree<string>> BuildDefineTree(string path) {
            
            List<Tree<string>> defineTree = new List<Tree<string>>();

            List<string> defines = LoadCode(path).FindAll(elem => elem.Contains("#define")).ToList();
            if (Extensions.IsNullOrEmpty(defines)) return null;

            foreach (var define in defines) {
                string[] temp = define.Split();
                string name = temp[2], defineName = temp[1];
                
                if (Extensions.IsNullOrEmpty(defineTree)) {
                    var tree = new Tree<string>(new TreeNode<string>(name));
                    tree.root.AddChild(new TreeNode<string>(defineName));
                    defineTree.Add(tree);
                } else {
                    bool defineFound = false;
                    foreach (var t in defineTree) {
                        var tempNode = Tree<string>.Find(t.root, name);
                        if (tempNode != null) {
                            defineFound = true;
                            tempNode.AddChild(new TreeNode<string>(defineName));
                            break;
                        }
                    }
                    if (!defineFound) {
                        var tree = new Tree<string>(new TreeNode<string>(name));
                        tree.root.AddChild(new TreeNode<string>(defineName));
                        defineTree.Add(tree);
                    }
                }
            }

            return defineTree;
        }

        //TODO: change return type to the C preprocessed function
        /*PLACEHOLDER*/
        public object PreprocessCode(List<string> lines) {
            return null;
        }

        //TODO: think about how to implement the whole structure
        //this method should return whole function tree with If, for, while, do_while statements

        /*PLACEHOLDER*/
        public Tree<IComparable> BuildFunctionTree(List<string> functionCode) {
            return null;
        }

        //TODO: find all the code blocks/lines with comments in them
        /*PLACEHOLDER*/
        public List<string> GetCodeComments(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            return null;
        } 
    }
}
