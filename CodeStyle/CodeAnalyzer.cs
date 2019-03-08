using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CodeStyle {
    public class CodeAnalyzer {

        private static CodeAnalyzer instance = null;

        public static CodeAnalyzer GetInstance() {
            if (instance == null) instance = new CodeAnalyzer();
            return instance;
        }

        private CodeAnalyzer() {}

        /*public Dictionary<string, List<string>> GetDefines(List<string> lines, string headerPath = null) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();

            string[] defines = lines.FindAll(line => line.Contains("#define")).ToArray();

            Dictionary<string, List<string>> defineTree = new Dictionary<string, List<string>>();

            foreach (var def in defines) {
                string[] temp = def.Split();
                if (!defineTree.ContainsKey(temp[2])) {
                    defineTree.Add(temp[2], new List<string>());
                }
                defineTree[temp[2]].Add(temp[1]);
            }

            return defineTree;
        }*/

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
        public object PreprocessCode(List<string> lines) {
            return null;
        }

        //TODO: think about how to implement the whole structure
        //this method should return whole function tree with If, for, while, do_while statements
        public Tree<IComparable> BuildFunctionTree(List<string> functionCode) {
            return null;
        }
    }
}
