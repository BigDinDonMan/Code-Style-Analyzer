﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

//TODO: implement function tree
//TODO: implement C Function object
//TODO: filter out commented out code 


namespace CodeStyle {

    public enum LineProcessing : byte {
        All = 0,
        BracesOnly = 1,
        KeywordsOnly = 2
    }


    public class CodeAnalyzer {

        public readonly string[] operators              = null;
        public readonly string[] forbiddenNames         = null;
        public readonly string[] keywords               = null;
        public readonly string[] builtInFunctionNames   = null;
        public readonly char[] lookupCharList           = new char[] {
                '"', '\'', '.', ',', '/', '{', '}',
                '(', ')', '[', ']', '+', '-', '*',
                '&', '^', '%', '$', '#', '@', '!',
                '?', '<', '>', '=', '|', ';', ':'
        };
        private static CodeAnalyzer instance            = null;

        public static CodeAnalyzer GetInstance() {
            if (instance == null) instance = new CodeAnalyzer();
            return instance;
        }

        private CodeAnalyzer() {
            operators               = GetOperators();
            keywords                = GetKeywords();
            forbiddenNames          = GetForbiddenWords();
            builtInFunctionNames    = GetBuiltInFunctionNames();
        }

        private string[] GetKeywords() {
            string[] words = null;
            using (var reader = new StreamReader("../../ProgramFiles/keywords.json")) {
                words = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
            }
            Array.Sort(words, 0, words.Length);
            return words;
        }

        private string[] GetForbiddenWords() {
         /*   string[] _words = new string[] {
                "gowno",
                "kurwa",
                "cholera",
                "pierdolic",
                "pierdolenie",
                "benin",
                "xD",
                "chuj"
            };
            using (var fs = new FileStream("../../ProgramFiles/forbidden.json", FileMode.OpenOrCreate)) {
                using (var writer = new StreamWriter(fs)) {
                    writer.Write(JsonConvert.SerializeObject(_words));
                }
            }*/
            string[] words = null;

            using (var reader = new StreamReader("../../ProgramFiles/forbidden.json")) {
                words = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
            }
            Array.Sort(words, 0, words.Length);
            return words;
        }

        private string[] GetOperators() {
            string[] operators = null;
            using (var reader = new StreamReader("../../ProgramFiles/operators.json")) {
                operators = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
            }
            Array.Sort(operators, 0, operators.Length);
            return operators;
        }

        private string[] GetBuiltInFunctionNames() {
            List<string> names = new List<string>();
            using (var reader = new StreamReader("../../ProgramFiles/functions.json")) {
                names = JsonConvert.DeserializeObject<List<string>>(reader.ReadToEnd()).Where(s => !String.IsNullOrEmpty(s)).Select(s => s.Trim()).ToList();
            }
            names.Sort();
            return names.ToArray();
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
            string code = String.Join("\n", copy);
            List<string> functions = new List<string>();
            int leftBraceCount = 0, rightBraceCount = 0;

            StringBuilder func = new StringBuilder();

            for (int i = 0; i < code.Length; ++i) {

                func.Append(code[i]);
                if (code[i] == '{') {
                    if (code[i].ToString().IsBetweenTwo(code, '"', i)) continue;
                    if (code[i].ToString().IsCommentedOut(code, i)) continue;
                    leftBraceCount++;
                }
                if (code[i] == '}') {
                    if (code[i].ToString().IsBetweenTwo(code, '"', i)) continue;
                    if (code[i].ToString().IsCommentedOut(code, i)) continue;
                    rightBraceCount++;
                }

                if (leftBraceCount != 0 && rightBraceCount != 0) {
                    if (leftBraceCount == rightBraceCount) {
                        leftBraceCount = rightBraceCount = 0;
                        functions.Add(func.ToString());
                        func.Clear();
                    }
                }

            }

            return functions.Select(s => s.Trim()).ToList();
        }

        public int LevenshteinDistance(string first, string second) {
            if (first == null || second == null) throw new NullReferenceException();

            if (first.Length == 0) return second.Length;
            if (second.Length == 0) return first.Length;

            int m = first.Length + 1, n = second.Length + 1;
            int[,] levenshteinMatrix = new int[m, n];
            for (int i = 0; i < m; ++i) {
                levenshteinMatrix[i, 0] = i;   
            }
            for (int j = 1; j < n; ++j) {
                levenshteinMatrix[0, j] = j;
            }
            int cost = 0;
            for (int i = 1; i < m; ++i) {
                for (int j = 1; j < n; ++j) {
                    if (first[i - 1] != second[j - 1]) cost = 1;
                    else cost = 0;
                    levenshteinMatrix[i, j] = Math.Min(
                        Math.Min(levenshteinMatrix[i - 1, j] + 1, levenshteinMatrix[i, j - 1] + 1),
                        levenshteinMatrix[i - 1, j - 1] + cost
                    );
                }
            }
            #region
            /*int width = first.Length, height = second.Length;
            int[,] levenshteinMatrix = new int[height + 1, width + 1];

            for (int i = 0; i < width; ++i) {
                levenshteinMatrix[0, i] = i;
            }

            for (int i = 0; i < height; ++i) {
                levenshteinMatrix[i, 0] = i;
            }

            int cost = 0;*/

            /* for (int i = 1; i < height; ++i) {
                 for (int j = 1; j < width; ++j, cost = 0) {
                     if (first[j] != second[i]) cost = 1;
                     int minimum = Math.Min(
                         Math.Min(levenshteinMatrix[i - 1, j] + 1, levenshteinMatrix[i, j - 1] + 1),
                         levenshteinMatrix[i - 1, j - 1] + cost
                     );
                     levenshteinMatrix[i, j] = minimum;
                 }
             }*/

            // return levenshteinMatrix[height - 1, width - 1];
            #endregion
            return levenshteinMatrix[m - 1, n - 1];
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
        public List<string> PreprocessCode(List<string> lines, List<Tree<string>> defineTree, LineProcessing processingOption = LineProcessing.All) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            if (Extensions.IsNullOrEmpty(defineTree)) throw new NullReferenceException();
            var preprocessedCode = new List<string>(lines);
            for (int i = 0; i < preprocessedCode.Count; ++i) {
                foreach (var tree in defineTree) {
                    string temp = preprocessedCode[i];
                    ProcessCodeLine(ref temp, tree.root, tree.root, processingOption);
                    preprocessedCode[i] = temp;
                }
            }
            
            return preprocessedCode;
        }

        public void ProcessCodeLine(ref string line, TreeNode<string> node, TreeNode<string> root, LineProcessing processingOption = LineProcessing.All) {
            if (node == null || String.IsNullOrEmpty(node.Value)) throw new NullReferenceException();
            if (root == null || String.IsNullOrEmpty(root.Value)) throw new NullReferenceException();
            if (processingOption == LineProcessing.BracesOnly) {
                if (root.Value != "{" && root.Value != "}") return;
            }
            if (node.Value == root.Value) goto end;
            if (line.Contains("*/") || line.Contains("#define")) return;
            #region
            /*var matches = Regex.Matches(line, @"\W" + Regex.Escape(node.Value) + @"\W?\b?").Cast<Match>().ToArray();

            if (matches.Length <= 0) goto end;
            string copy = line;
            var builder = new StringBuilder(copy);
            var indexes = new List<Tuple<int, int>>();
            foreach (var match in matches) {
                foreach (Group g in match.Groups) {
                    indexes.Add(new Tuple<int, int>(g.Index, g.Index + node.Value.Length));
                }
            }
            indexes.ForEach(tup => {
                builder.Remove(tup.Item1 + 1, tup.Item2 - tup.Item1);
                builder.Insert(tup.Item1 + 1, root.Value);
            });
            line = builder.ToString();*/
            //end:
            //int index = 0;
            #endregion

            int index = 0;
            Regex r = new Regex(@"\W" + node.Value + @"\W?\b?");
            Regex r1 = new Regex(@"^?[a-zA-Z]" + node.Value + @"[a-zA-Z]");
            while ((index = line.IndexOf(node.Value, index)) != -1) {
                if (node.Value.IsBetweenTwo(line, '"', index)) {
                    index++;
                    continue;
                }
                Match m;
                try {
                    string substr = line.Substring(index - 1, node.Value.Length + 1);
                    m = r1.Match(substr);
                    if (m.Success) {
                        index += node.Value.Length;
                        continue;
                    }
                    m = r.Match(substr);
                    if (!m.Success) {
                        index += node.Value.Length;
                        continue;
                    }
                    m = null;
                } catch (ArgumentOutOfRangeException) {}
                line = line.Remove(index, node.Value.Length).Insert(index, root.Value);
            }

            end:
            foreach (var c in node.children) ProcessCodeLine(ref line, c, root, processingOption);
        }

        public List<string> ClearComments(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();

            var linesCopy = new List<string>(lines);
            /*clearing line comments*/
            for (int i = 0; i < lines.Count; ++i) {
                int _index = linesCopy[i].IndexOf("//");
                if (_index == -1 || "//".IsBetweenTwo(lines[i], '"', _index)) continue;
                linesCopy[i] = linesCopy[i].Remove(_index);
            }
            var codeLines = linesCopy.Where(line => !String.IsNullOrWhiteSpace(line)).ToList();

            /*clearing block comments*/
            var code = String.Join("\n", codeLines);
            int index = 0;
            while ((index = code.IndexOf("/*", index)) != -1) {
                if ("/*".IsBetweenTwo(code, '"', index)) {
                    index++;
                    continue;
                }
                int temp;
                while (true) {
                    temp = code.IndexOf("*/", index);
                    if (!"*/".IsBetweenTwo(code, '"', temp)) break;
                    else temp++;
                }
                try {
                    code = code.Remove(index, temp + 2 - index);
                } catch (ArgumentOutOfRangeException) { continue; }
            }
            return code.Split('\n').Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
        }

        //TODO: think about how to implement the whole structure
        //this method should return whole function tree with If, for, while, do_while statements
        /*PLACEHOLDER*/
        public Tree<IComparable> BuildFunctionTree(List<string> functionCode) {
            return null;
        }

        public List<string> GetCodeComments(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();

            var lineComments = lines.Where(s => s.Contains("//")).ToList();
            var comments = new List<string>();
            
            foreach (var line in lineComments) {
                var matches = Regex.Matches(line, "//").Cast<Match>().ToArray();
                var tempList = new List<string>();
                foreach (var m in matches) {
                    for (int i = 0; i < m.Groups.Count; ++i) {
                        if (!m.Groups[i].Value.IsBetweenTwo(line, '"', m.Groups[i].Index)) {
                            tempList.Add(line.Substring(m.Groups[i].Index));
                        }
                    } 
                }
                comments.AddRange(tempList);
            }
            
            lineComments = comments.Select(comment => comment.Replace("//", "").Trim()).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

            var code = String.Join("\n", lines.ToArray());
            //TODO: filter out the excess block beginnings and ignore them if theyre in a line comment

            var blockBeginnings = Regex.Matches(code, "/\\*").Cast<Match>().ToList();//Where(m => !m.Groups[0].Value.IsBetweenTwo(code, '"', m.Groups[0].Index)).Select(m => m.Groups[0].Index).ToList();
            var blockEnds = Regex.Matches(code, "\\*/").Cast<Match>().ToList();//Where(m => !m.Groups[0].Value.IsBetweenTwo(code, '"', m.Groups[0].Index)).Select(m => m.Groups[0].Index).ToList();

            var blockBegginingsIndexes = new List<int>();
            var blockEndsIndexes = new List<int>();

            foreach (Match m in blockBeginnings) {
                foreach (Group g in m.Groups) {
                    if (g.Value.IsBetweenTwo(code, '"', g.Index)) continue;
                    blockBegginingsIndexes.Add(g.Index);
                }
            }

            foreach (Match m in blockEnds) {
                foreach (Group g in m.Groups) {
                    if (g.Value.IsBetweenTwo(code, '"', g.Index)) continue;
                    blockEndsIndexes.Add(g.Index);
                }
            }

            Dictionary<int, List<int>> occurences = new Dictionary<int, List<int>>();
            
            for (int i = 0; i < blockEndsIndexes.Count; ++i) {
                occurences.Add(blockEndsIndexes[i], new List<int>());
                for (int j = 0; j < blockBegginingsIndexes.Count; ++j) {
                    if (i == 0) {
                         if (blockBegginingsIndexes[j] < blockEndsIndexes[i]) {
                             occurences[blockEndsIndexes[i]].Add(blockBegginingsIndexes[j]);
                         }
                    } else {
                        if (blockBegginingsIndexes[j] < blockEndsIndexes[i] && !occurences[blockEndsIndexes[i - 1]].Contains(blockBegginingsIndexes[j])) {
                            occurences[blockEndsIndexes[i]].Add(blockBegginingsIndexes[j]);
                        }
                    }
                }
            }

            List<string> blockPairs = null;

            try {
                blockPairs = occurences.Select(entry => new Tuple<int, int>(entry.Key, entry.Value[entry.Value.Count - 1])).Select(tup => code.Substring(tup.Item2, tup.Item1 - tup.Item2)).ToList();
            } catch (ArgumentOutOfRangeException) {
                return null;
            }

            var blocks = new List<string>();

            blockPairs.ForEach(str => blocks.AddRange(str.Split('\n')));

            return lineComments.Concat(blocks).Select(s => s.Replace("/*", "").Replace("*/", "").Trim()).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
        }

        public Tuple<List<string>, List<string>> GetAndClearComments(List<string> lines) {
            return new Tuple<List<string>, List<string>>(GetCodeComments(lines), ClearComments(lines));
        }
        
        public double GetCommentKeywordRatio(List<string> comments) {//TODO: change the replacement of the characters to not include the operators
    
            var words = new List<string>();

            foreach (var line in comments) {
                string temp = line;
                foreach (var c in lookupCharList) {
                    temp = temp.Replace(c.ToString(), " ");
                }
                words.AddRange(temp.Split());
            }
            words = words.Where(word => word.Length > 1).ToList();

            double keywordCount = words.Where(s => builtInFunctionNames.Contains(s)).Count() + words.Where(s => keywords.Contains(s)).Count();
            double normalCount = words.Where(s => !builtInFunctionNames.Contains(s) && !keywords.Contains(s)).Count();

            return keywordCount / normalCount;
        }

        public TreeNode<string> GetIfStatements(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            return null;
        }

        public TreeNode<string> GetForLoops(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            return null;
        }

        public TreeNode<string> GetWhileLoops(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            return null;
        }
        
        public TreeNode<string> GetDoWhileLoops(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();
            return null;
        }

        public int CountForbiddenWords(List<string> lines) {
            if (Extensions.IsNullOrEmpty(lines)) throw new NullReferenceException();

            var code = String.Join("\n", lines);

            foreach (var c in lookupCharList) {
                code = code.Replace(c.ToString(), " ");
            }

            HashSet<string> uniques = new HashSet<string>();
            foreach (var word in code.Split()) {
                if (String.IsNullOrWhiteSpace(word) || word.IsNumeric()) continue;
                foreach (var forbiddenWord in forbiddenNames) {
                    int difference = LevenshteinDistance(word, forbiddenWord);
                    if (difference <= (word.Length > forbiddenWord.Length ? forbiddenWord.Length : word.Length) / 2) {
                        if (keywords.Contains(word)) continue;
                        uniques.Add(word);
                    }
                }
            }
            return uniques.Count;
        }
    }
}
