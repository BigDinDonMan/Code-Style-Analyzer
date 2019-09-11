using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using StringUtils;
using System.Text.RegularExpressions;
using CollectionUtils;

//TODO: create functions that will get defines from all the files (header, main file etc.) and will load all the functions from the files
//TODO: create a function called ParseCodeBlock that parses the code until the brace counts are equal (like the for loops in ParseFunctions)

/*order of operations:
 - load code lines
 - get all comments
 - get all defines
 - clear typedefs, structs and defines
 - preprocess the files
 - do the rest in any order (?)
     
     */

// check if someone is using a define in while, do while and for loops or a plain number
// use levenshtein to check if variable names arent swearwords, stupid names etc.


namespace CodeStyle {

    public enum LineProcessing : byte {
        All = 0,
        BracesOnly = 1,
        KeywordsOnly = 2
    }

    public enum OperatorContext : byte {
        None, Unary, Binary, IncrementOrDecrement
    }

    public static class CodeAnalysis {
        public static readonly string[] operators = null;
        public static readonly string[] forbiddenNames = null;
        public static readonly string[] keywords = null;
        public static readonly string[] builtInFunctionNames = null;
        public static readonly char[] lookupCharList = new char[] {
                '"', '\'', '.', ',', '/', '{', '}',
                '(', ')', '[', ']', '+', '-', '*',
                '&', '^', '%', '$', '#', '@', '!',
                '?', '<', '>', '=', '|', ';', ':'
        };
        public static readonly string[] operatorsWithSpaces = new string[] {
            "+", "-", "*", "/", "%", "==", "!=", "<", ">", "<=", ">=",
            "&&", "||", "&", "|", "^", "<<", ">>", ">>=", "<<=", "=", 
            "+=", "-=", "*=", "/=", "%=", "^=", "&=", "|=", "?", ":"
        };
        public static readonly string[] equalityOperators = new string[] {
            "=", "==", "!=", "<", "<=", ">", ">=",
            "%=", ">>=", "<<=", "+=", "-=",
            "/=", "*=", "|=", "&=", "^="
        };
        public static readonly string[] baseTypes = new string[] {
            "char", "short", "int", "float", "double", "long"
        };
        public static readonly int checkBackValue = 10;
        public const int MAX_LINE_LENGTH = 150;

        static CodeAnalysis() {
            operators = GetOperators();
            forbiddenNames = GetForbiddenWords();
            keywords = GetKeywords();
            builtInFunctionNames = GetBuiltInFunctionNames();
        }

        #region File data loading functions

        private static string[] GetKeywords() {
            string[] words = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(@"../../ProgramFiles/keywords.json"));
            Array.Sort(words, 0, words.Length);
            return words;
        }

        private static string[] GetForbiddenWords() {
            string[] words = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(@"../../ProgramFiles/forbidden.json"));
            Array.Sort(words, 0, words.Length);
            return words;
        }

        private static string[] GetOperators() {
            string[] operators = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(@"../../ProgramFiles/operators.json"));
            Array.Sort(operators, 0, operators.Length);
            return operators;
        }

        private static string[] GetBuiltInFunctionNames() {
            string[] names = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(@"../../ProgramFiles/functions.json")).
                Where(s => !String.IsNullOrWhiteSpace(s)).
                Select(s => s.Trim()).
                ToArray();
            Array.Sort(names, 0, names.Length);
            return names;
        }

        #endregion

        #region Filter functions

        

        public static List<string> FilterOutPreprocessor(List<string> lines) {

            bool IsPreprocessorLineExcludingDefines(string s) {
                return s.Contains("#include") || s.Contains("#ifndef") || s.Contains("#ifdef");
            }

            var filtered = new List<string>();

            for (int i = 0; i < lines.Count; ++i) {
                if (IsPreprocessorLineExcludingDefines(lines[i])) continue; //if line contains include then we simply skip it
                if (lines[i].Contains("#define")) { //if a line contains define
                    try {
                        if (lines[i + 1].Contains("#define")) continue;//we check if the next line is also a define, if it is then its fine
                        else { //if it isnt then it might be a multiline macro
                            string temp = lines[i].Replace("\n", "").Replace("\r", "");//we replace the newline sign (\n\r on windows, \n on linux)
                            if (temp.EndsWith(@"\")) {//we check if it ends with \ (it indicates the next line of the macro)
                                int iterator = i;
                                do {
                                    temp = lines[iterator++].Replace("\n", "").Replace("\r", "");
                                } while (temp.EndsWith(@"\"));//we iterate through the lines until the multiline macro ends
                                i = iterator;
                            }
                        }
                    } catch (ArgumentOutOfRangeException) {
                        continue;
                    }
                } else {
                    filtered.Add(lines[i]);
                }
            }

            return filtered;
        }

        public static List<string> FullFilter(List<string> lines) {
            return FilterOutTypedefsAndStructs(FilterOutFunctionDeclarations(FilterOutPreprocessor(lines)));
        }

        public static List<string> FilterOutFunctionDeclarations(List<string> lines) {
            Regex headerPattern = new Regex(@"^((\w+\*?){2,3}\s*(.*\)\s*(\r?\n)*\s*;))");
            return lines.Where(line => !Regex.IsMatch(line, headerPattern.ToString()) && !builtInFunctionNames.Contains(line)).ToList();
        }

        public static List<string> FilterOutTypedefsAndStructs(List<string> lines) {
            Regex typedefPattern, structPattern, unionPattern;
            string code = String.Join("\n", lines);
            typedefPattern = new Regex(@"typedef (.|\r?\n)*?;");
            structPattern = new Regex(@"struct .*\s*(\r?\n)*\{(.|\r?\n)*?\};?");
            unionPattern = new Regex(@"union .*\s*(\r?\n)*\{(.|\r?\n)*?\};?");
            
            string withoutStructs = structPattern.Replace(code, "");
            string withoutUnions = unionPattern.Replace(withoutStructs, "");
            string finalResult = typedefPattern.Replace(withoutUnions, "");

            return finalResult.SplitLines();
        }

        #endregion

        #region Consistency of code (braces and spaces etc.)


        //sprawdzić co jest po dwóch stronach (-5 jest dobre, ale - 500 jest źle
        public static int[] CheckOperatorsSpaces(List<string> lines) {//returns an array with indexes of operators without spaces

            bool IsOperatorException(string s) {
                string[] ops = new string[] { "+", "-", "&", "*" };
                return ops.Contains(s);
            }

            OperatorContext GetOperatorContext(string s, int index, string op, out bool hasSpaceLeft, out bool hasSpaceRight) {
                int i = index - 1, j = index + 1;
                hasSpaceLeft = hasSpaceRight = false;
                while (Char.IsWhiteSpace(s[i]) || s[i].Equals('(') || s[i].Equals(')')) {
                    if (Char.IsWhiteSpace(s[i])) {
                        hasSpaceLeft = true;
                    }
                    i--;
                }
                char left, right;
                left = s[i];
                while (Char.IsWhiteSpace(s[j]) || s[j].Equals('(') || s[j].Equals(')')) {
                    if (Char.IsWhiteSpace(s[j])) {
                        hasSpaceRight = true;
                    }
                    j++;
                }
                right = s[j];
                bool leftResult, rightResult, contains;
                switch (op) {
                    case "+":
                    case "-":
                        contains = equalityOperators.Contains(left.ToString());
                        rightResult = (Char.IsLetterOrDigit(right) || right.Equals('_'));
                        if ((left != '-' && Char.IsPunctuation(left)) && rightResult) {
                            return OperatorContext.Unary;
                        } else if (contains && rightResult) {
                            return OperatorContext.Unary;
                        } else if (!contains && rightResult) {
                            return OperatorContext.Binary;
                        } else if (op.Equals(left.ToString()) || op.Equals(right.ToString())) {
                            return OperatorContext.IncrementOrDecrement;
                        }
                        break;

                    case "*":

                        break;

                    case "&":
                        leftResult = Char.IsPunctuation(left) || operators.Contains(left.ToString());
                        rightResult = Char.IsLetter(right);
                        return leftResult && rightResult ? OperatorContext.Unary : OperatorContext.Binary;
                }
                return OperatorContext.None;
            }

            var occurenceList = new List<int>();
            string code = String.Join("\n", lines);
            for (int i = 0; i < code.Length; ++i) {
                try {
                    string lenOne, lenTwo, lenThree;
                    lenOne = code[i].ToString();
                    lenTwo = code.Substring(i, 2);
                    lenThree = code.Substring(i, 3);

                    if (operatorsWithSpaces.Contains(lenThree) && !lenThree.IsBetweenTwo(code, '"', i)) {
                        if (code[i - 1] != ' ' || code[i + lenThree.Length] != ' ') {
                            occurenceList.Add(i);
                        }
                        i += lenThree.Length;
                    } else if (operatorsWithSpaces.Contains(lenTwo) && !lenTwo.IsBetweenTwo(code, '"', i)) {
                        if (code[i - 1] != ' ' || code[i + lenTwo.Length] != ' ') {
                            occurenceList.Add(i);
                        }
                        i += lenTwo.Length;
                    } else if (operatorsWithSpaces.Contains(lenOne) && !lenOne.IsBetweenTwo(code, '"', i)) {
                        if (IsOperatorException(lenOne)) {
                            OperatorContext context = GetOperatorContext(code, i, lenOne, out bool leftSpace, out bool rightSpace);
                            if (context.Equals(OperatorContext.IncrementOrDecrement)) {
                                i++;
                                continue;
                            }
                            if (context.Equals(OperatorContext.Binary)) {
                                if (!leftSpace || !rightSpace) {
                                    occurenceList.Add(i);
                                    i++;
                                }
                            }
                        } else {
                            if (code[i - 1] != ' ' && code[i + lenOne.Length] != ' ') {
                                occurenceList.Add(i);
                                i++;
                            }
                        }
                    }

                    #region
                    //if (lenTwo.Equals("++") || lenTwo.Equals("--")) {
                    //    i += lenTwo.Length;
                    //    continue;
                    //}

                    //if (operatorsWithSpaces.Contains(lenThree) && !lenThree.IsBetweenTwo(code, '"', i)) {
                    //    if (code[i - 1] != ' ' && code[i + lenThree.Length] != ' ') {
                    //        occurenceList.Add(i);
                    //    }
                    //    i += lenThree.Length;
                    //} else if (operatorsWithSpaces.Contains(lenTwo) && !lenTwo.IsBetweenTwo(code, '"', i)) {
                    //    if (code[i - 1] != ' ' && code[i + lenTwo.Length] != ' ') {
                    //        occurenceList.Add(i);
                    //    }
                    //    i += lenTwo.Length;
                    //} else if (operatorsWithSpaces.Contains(lenOne) && !lenOne.IsBetweenTwo(code, '"', i)) {
                    //    if (code[i - 1] != ' ' && code[i + lenOne.Length] != ' ') {
                    //        occurenceList.Add(i);
                    //    }
                    //    i += lenOne.Length;
                    //}
                    #endregion
                } catch (ArgumentOutOfRangeException) {}
            }

            return occurenceList.ToArray();
        }

        public static bool AreBraceNewlinesConsistent(List<string> lines) {

            string code = String.Join("\n", lines);
            int newlineBraces = 0, sameLineBraces = 0;
            int[] braceIndexes = code.FindAll("{");

            foreach (var i in braceIndexes) {
                //int counter = 0;
                //bool found = false;
                //for (int j = i; counter < checkBackValue; ++counter, --j) {
                //    try {
                //        if (code[j] == '\n' && !'\n'.ToString().IsBetweenTwo(code, '"', j)) {
                //            found = true;
                //            break;
                //        }
                //    } catch (ArgumentOutOfRangeException) { }
                //}
                //if (found) {
                //    newlineBraces++;
                //} else {
                //    sameLineBraces++;
                //}
                //counter = 0;
                //found = false;
                bool found = false;
                for (int j = i - 1; j > 0; --j) {
                    try {
                        if (!Char.IsWhiteSpace(code[j])) break;
                        if (code[j] == '\n' && !code[j].ToString().IsBetweenTwo(code, '"', j)) {
                            found = true;
                            break;
                        }
                    } catch (ArgumentOutOfRangeException) {}
                }
                if (found) {
                    newlineBraces++;
                } else {
                    sameLineBraces++;
                }
                found = false;
            }
            return (newlineBraces == 0 && sameLineBraces != 0) || (newlineBraces != 0 && sameLineBraces == 0);
        }

        public static int CheckLinesLength(List<string> lines) {
            return lines.Count(line => line.Length > MAX_LINE_LENGTH);
        }

        public static int CheckLoopsStatements(List<string> lines) {

            string code = String.Join("\n", lines);

            int magicNumberCount = 0;

            string[] whileStatements = Regex.Matches(code, @"while\s*\(.*\)").
                Cast<Match>().
                Select(m => m.Value).
                Select(s => {
                    Match m = Regex.Match(s, @"\(.*\)");
                    return m.Success ? m.Value : "";
                }).
                Where(s => !String.IsNullOrWhiteSpace(s)).
                ToArray();

            string[] forStatements = Regex.Matches(code, @"for\s*\(.*\)").
                Cast<Match>().
                Select(m => m.Value).
                Select(s => {
                    Match m = Regex.Match(s, @"\(.*\)");
                    return m.Success ? m.Value : "";
                }).
                Where(s => !String.IsNullOrWhiteSpace(s)).
                ToArray();

            foreach (var forLoop in forStatements) {
                Match infLoopMatch = Regex.Match(forLoop, @"\(\s*;\s*;\s*\)");
                if (infLoopMatch.Success) continue;
                string[] parts = forLoop.Split(';');
                string conditionPart = parts[1];
                MatchCollection integerMatches = Regex.Matches(conditionPart, @"\-?\d+");
                if (integerMatches.Count != 0) {
                    IEnumerable<Match> validMatches = integerMatches.
                        Cast<Match>().
                        Where(m => !(m.Value.Equals("0") || m.Value.Equals("-1")));
                    if (validMatches.Count() != 0) {
                        magicNumberCount++;
                    }
                }
            }

            foreach (var whileLoop in whileStatements) {
                Match infLoopMatch = Regex.Match(whileLoop, @"\(\-?\d+\)");
                if (infLoopMatch.Success) continue;
            }

            return magicNumberCount;
        }

        #endregion

        #region Comment operations

        public static List<string> GetCodeComments(List<string> lines) {
            var comments = new List<string>();
            var code = String.Join("\n", lines);
            string codeCopy = code;
            List<string> singleLineComments = lines.Select(line => {
                if (!line.Contains("//")) return "";
                int index = 0;
                while ((index = line.IndexOf("//", index == 0 ? index : index + 1)) != -1) {
                    if ("//".IsBetweenTwo(line, '"', index) || "//".IsBetweenTwo(line, new Tuple<string, string>("/*", "*/"), index)) continue;
                    return line.Substring(index + 2);
                }
                return "";
            }).Where(comment => !String.IsNullOrWhiteSpace(comment)).ToList();
            var multilineComments = new List<string>();
            int i = code.IndexOf("/*");
            while (i != -1) {
                if ("/*".IsBetweenTwo(code, '"', i)) {
                    i = code.IndexOf(code, i + 1);
                    continue;
                }
                int i1 = code.IndexOf("*/", i);
                while ("*/".IsBetweenTwo(code, '"', i1)) {
                    i1 = code.IndexOf("*/", i1 + 1);
                }
                string s = code.Substring(i + 2, i1 - i + 1);
                if (s.All(c => c.Equals('/') || c.Equals('*') || Char.IsWhiteSpace(c)) || String.IsNullOrWhiteSpace(s)) {
                    i = code.IndexOf("/*", i + 1);
                    continue;
                }
                multilineComments.Add(s.Trim());
                i = code.IndexOf("/*", i + 1);
            }
            return singleLineComments.Concat(multilineComments).ToList();
        }

        public static List<string> ClearCodeComments(List<string> lines) {
            var copies = new List<string>(lines);
            for (int i = 0; i < copies.Count; ++i) {
                if (copies[i].Contains("//")) {
                    int index = copies[i].IndexOf("//");
                    int lastindex = 0;
                    while (index != -1) {
                        if (!("//".IsBetweenTwo(copies[i], '"', index) || "//".IsBetweenTwo(copies[i], new Tuple<string, string>("/*", "*/"), index))) {
                            break;
                        }
                        index = copies[i].IndexOf("//", index + 1);
                        if (index >= 0) {
                            lastindex = index;
                        }
                    }
                    copies[i] = copies[i].Remove(lastindex);
                }
            }
            string code = String.Join("\n", copies);

            int commentIndex = code.IndexOf("/*");
            while (commentIndex != -1) {
                if ("/*".IsBetweenTwo(code, '"', commentIndex)) {
                    goto end;
                }
                int i = code.IndexOf("*/", commentIndex);
                while (i != -1 && "*/".IsBetweenTwo(code, '"', i)) {
                    i = code.IndexOf("*/", i + 1);
                }
                if (code[i - 1] == '/') goto end;
                string withRemovedPart = code.Remove(commentIndex, i - commentIndex + 2);
                code = withRemovedPart;
                end:
                commentIndex = code.IndexOf("/*", commentIndex + 1);
            }
            
            return code.SplitLines();
        }

        public static double GetCommentedOutCodeRatio(List<string> comments) {
            StringBuilder sb = new StringBuilder(String.Join(" ", comments));
            foreach (var c in lookupCharList) {
                sb.Replace(c, ' ');
            }
            string[] words = sb.ToString().
                Split(new string[] { " ", "\n" }, StringSplitOptions.None).
                Select(s => s.Trim()).
                Where(s => !String.IsNullOrWhiteSpace(s)).
                ToArray();
            double keywordCount, normalWordCount;
            keywordCount = words.Where(w => builtInFunctionNames.Contains(w) || keywords.Contains(w)).Count();
            normalWordCount = words.Where(w => !(builtInFunctionNames.Contains(w) || keywords.Contains(w))).Count();
            return Math.Round(keywordCount / normalWordCount, 2);
        }

        public static int LevenshteinDistance(string first, string second) {

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
            return levenshteinMatrix[m - 1, n - 1];
        }

        #endregion

        #region Line indentation measurements

        public static List<Tuple<string, int>> MeasureLineIndents(List<string> lines) {

            var result = new List<Tuple<string, int>>();

            int IndentCount(string line) {
                if (String.IsNullOrWhiteSpace(line)) return -1;
                line = line.Replace("\t", "    ");
                int count = 0;
                foreach (var c in line) {
                    if (!Char.IsWhiteSpace(c)) break;
                    count++;
                }
                return count;
            }

            foreach (var line in lines) {
                int count = IndentCount(line);
                if (count == -1) continue;
                result.Add(new Tuple<string, int>(line, count));
            }

            return result;
        }

        public static Dictionary<int, int> GetIndentDictionary(List<Tuple<string, int>> indents) {
            HashSet<int> uniques = indents.Select(item => item.Item2).Where(i => i > 0).ToHashSet();
            Dictionary<int, int> resultDict = new Dictionary<int, int>();
            foreach (var number in uniques) {
                resultDict.Add(number, 0);
            }
            foreach (var element in indents) {
                try {
                    resultDict[element.Item2]++;
                } catch (Exception) {
                    continue;
                }
            }
            return resultDict;
        }

        public static int GetIrregularIndentsCount(Dictionary<int, int> indents) {
            
            int startValue = indents.Keys.Min();
            int counter = 0;
            int[] sortedKeys = new List<int>(indents.Keys).OrderBy(i => i).ToArray();
            int prev = startValue;
            bool start = true;
            foreach (var indent in sortedKeys) {
                if (start) {
                    start = false;
                    continue;
                }
                if (prev + startValue != indent) {
                    counter++;
                } else {
                    prev = indent;
                }
            }

            return counter;
        }

        #endregion

        #region Loops and statements parsing

        public static List<string> PreprocessCode(List<string> lines, List<Tree<string>> defineTree, LineProcessing processingOption = LineProcessing.All) {
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

        public static void ProcessCodeLine(ref string line, TreeNode<string> node, TreeNode<string> root, LineProcessing processingOption = LineProcessing.All) {
            if (node == null || String.IsNullOrEmpty(node.Value)) throw new NullReferenceException();
            if (root == null || String.IsNullOrEmpty(root.Value)) throw new NullReferenceException();
            if (processingOption == LineProcessing.BracesOnly) {
                if (root.Value != "{" && root.Value != "}") return;
            }
            if (node.Value == root.Value) goto end;
            if (line.Contains("*/") || line.Contains("#define")) return;

            //int index = 0;
            //Regex r = new Regex(@"\W" + node.Value + @"\W?\b?");
            //Regex r1 = new Regex(@"^?[a-zA-Z]" + node.Value + @"[a-zA-Z]");
            //while ((index = line.IndexOf(node.Value, index)) != -1) {
            //    if (node.Value.IsBetweenTwo(line, '"', index)) {
            //        index++;
            //        continue;
            //    }
            //    Match m;
            //    try {
            //        string substr = line.Substring(index - 1, node.Value.Length + 1);
            //        m = r1.Match(substr);
            //        if (m.Success) {
            //            index += node.Value.Length;
            //            continue;
            //        }
            //        m = r.Match(substr);
            //        if (!m.Success) {
            //            index += node.Value.Length;
            //            continue;
            //        }
            //        m = null;
            //    } catch (ArgumentOutOfRangeException) { }
            //    line = line.Remove(index, node.Value.Length).Insert(index, root.Value);
            //}

            string afterReplace = Regex.Replace(line, @"\b" + node.Value + @"\b", root.Value);
            line = afterReplace;


            end:
            foreach (var c in node.children) {
                ProcessCodeLine(ref line, c, root, processingOption);
            }
        }

        public static string[] ProfanityOccurences(List<string> lines) {

            StringBuilder sb = new StringBuilder(String.Join("\n", lines));

            foreach (var c in lookupCharList) {
                sb.Replace(c, ' ');
            }

            var words = sb.ToString().
                Split(new string[] { "\n", " " }, StringSplitOptions.None).
                Where(w => !String.IsNullOrWhiteSpace(w) && !w.IsNumeric()).
                Select(w => w.Trim()).
                Where(w => w.Length > 1).
                Distinct().
                Where(w => !keywords.Contains(w) && !builtInFunctionNames.Contains(w));

            var occurences = new List<string>();

            foreach (var profanity in forbiddenNames) {
                foreach (var word in words) {
                    int distance = LevenshteinDistance(profanity, word);
                    if (distance <= (profanity.Length / 2)) {
                        occurences.Add(word);
                    }
                }
            }
            
            return occurences.ToArray();
        }
 
        #endregion

        #region Define handling

        public static List<string> GetNonMacroDefines(List<string> lines) {

            var defines = new List<string>();
            for (int i = 0; i < lines.Count; ++i) {
                if (lines[i].Contains("#include")) continue; 
                if (lines[i].Contains("#define") && lines[i].Split(' ').Length == 3) { 
                    try {
                        if (lines[i + 1].Contains("#define")) {
                            defines.Add(lines[i]);
                        }
                        else continue;
                    } catch (ArgumentOutOfRangeException) {
                        if (i == lines.Count - 1) {
                            defines.Add(lines[i]);
                        } 
                    }
                } 
            }

            return defines;
        }

        public static List<Tree<string>> BuildNonMacroDefineTree(List<string> defines) {

            var definesList = new List<Tree<string>>();

            defines.ForEach(def => {
                string alias, original;
                string[] temp = def.Split(' ');
                if (temp.Length != 3) return;
                alias = temp[1];
                original = temp[2];
                if (definesList.IsEmpty()) {
                    Tree<string> newTree = new Tree<string>(new TreeNode<string>(original));
                    newTree.root.AddChild(new TreeNode<string>(alias));
                    definesList.Add(newTree);
                } else {
                    bool defineFound = false;
                    foreach (var t in definesList) {
                        var tempNode = Tree<string>.Find(t.root, original);
                        if (tempNode != null) {
                            defineFound = true;
                            tempNode.AddChild(new TreeNode<string>(alias));
                            break;
                        }
                    }
                    if (!defineFound) {
                        var tree = new Tree<string>(new TreeNode<string>(original));
                        tree.root.AddChild(new TreeNode<string>(alias));
                        definesList.Add(tree);
                    }
                }
            });

            return definesList;
        }

        //method should return a define tree that has a keyword or operator as a define
        //maybe add a filter for stupid names as well
        public static List<Tree<string>> GetInvalidDefines(List<Tree<string>> defines) {
            return defines.Where(tree => {
                return operators.Contains(tree.root.Value) || 
                    keywords.Contains(tree.root.Value) ||
                    lookupCharList.Select(c => c.ToString()).Contains(tree.root.Value);
            }).ToList();
        }

        #endregion

        #region Parsing

        /// <summary>
        /// parses functions from input source file
        /// </summary>
        /// <returns>List of strings where each string represents a function</returns>
        public static List<string> ParseFunctions(List<string> lines) {

            var copy = FullFilter(lines);
            var functions = new List<string>();
            var func = new StringBuilder();

            string code = String.Join("\n", copy);
            int leftBraceCount = 0, rightBraceCount = 0;

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


            return functions.Select(_func => _func.Trim()).ToList();
        }

        /// <summary>
        /// loads source code in lines from a file
        /// </summary>
        /// <param name="path">path to source code file</param>
        /// <returns>list containing code lines</returns>
        public static List<string> LoadCodeLines(string path) {
            return File.ReadAllLines(path).ToList();
        }

        #endregion

        public static StyleStatistic AnalyzeCodeStyle(params string[] paths) {
            if (paths.Length <= 0) return null;
            var headers = paths.Where(path => {
                string ext = Path.GetExtension(path).ToLower();
                return ext == ".h" || ext == ".hpp";
            });
            var normalFiles = paths.Where(path => {
                string ext = Path.GetExtension(path).ToLower();
                return ext == ".c" || ext == ".cpp";
            });
            var fileContents = paths.Select(h => LoadCodeLines(h)).Select(p => GetNonMacroDefines(p));
            var definesList = fileContents.Aggregate((acc, l) => acc.Concat(l).ToList());
            var defineTree = BuildNonMacroDefineTree(definesList);
            defineTree.ForEach(t => {
                Console.WriteLine("nowe dżewo");
                Tree<string>.Print(t.root);
            });
            return null;
        }

    }
}
