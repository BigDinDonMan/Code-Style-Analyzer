using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeStyle {
    public static class Extensions {

        /*list extensions*/
        public static bool IsNullOrEmpty<T>(List<T> l) => l == null || l.Count == 0;
        public static void Print<T>(this List<T> l) {
            if (IsNullOrEmpty(l)) return;
            int counter = 1;
            foreach (var x in l) {
                Console.WriteLine("Element nr {0}", counter);
                Console.WriteLine(x.ToString());
                counter++;
            }
        }

        /*dict extensions*/
        public static bool IsNullOrEmpty<T, U>(Dictionary<T, U> dict) => dict == null || dict.Count == 0;


        /*string extensions*/
        public static bool IsBetweenTwo(this string s, string target, char c, int startIndex) {
            if (String.IsNullOrEmpty(s)) throw new ArgumentNullException();
            if (startIndex < 0) throw new FormatException();
            if (startIndex > target.Length) throw new FormatException();

            bool left = false, right = false;

            for (int i = startIndex; i >= 0; --i) {
                if (target[i] == '\n' && c != '\n') break;
                if (target[i] == c) {
                    left = true;
                    break;
                }
            }

            for (int i = startIndex + s.Length; i < target.Length; ++i) {
                if (target[i] == '\n' && c != '\n') break;
                if (target[i] == c) {
                    right = true;
                    break;
                }
            }

            return left && right;
        }
        public static bool IsBetweenTwo(this string s, string target, Tuple<string, string> pair, int startIndex) {
            if (String.IsNullOrEmpty(s)) throw new ArgumentNullException();
            if (startIndex < 0) throw new FormatException();
            if (startIndex > target.Length) throw new FormatException();

            bool left, right;
            left = right = false;
            string temp = null;
            for (int i = startIndex; i >= 0; --i) {
                try {
                    temp = target.Substring(i - pair.Item1.Length, pair.Item1.Length);
                } catch (ArgumentOutOfRangeException) { continue; }

                if (pair.Item1 == "//") {
                    if (target[i] == '\n') {
                        left = false;
                        break;
                    }
                }

                if (temp == pair.Item1) {
                    left = true;
                    break;
                }
                
            }

            for (int i = startIndex + 1; i < target.Length; ++i) {
                try {
                    temp = target.Substring(i, pair.Item2.Length);
                } catch (ArgumentOutOfRangeException) { continue; }
                if (temp == pair.Item2) {
                    right = true;
                    break;
                }
            }

            

            return left && right;
        }
        public static int GetOcurrenceCount(this string s, string toFind) {
            return System.Text.RegularExpressions.Regex.Matches(s, toFind).Count;
        }
        public static bool IsCommentedOut(this string s, string target, int startIndex) {
            if (String.IsNullOrEmpty(target)) throw new NullReferenceException();
            if (startIndex < 0) throw new ArgumentException("startindex is less than zero");
            return s.IsBetweenTwo(target, new Tuple<string, string>("//", "\n"), startIndex) || s.IsBetweenTwo(target, new Tuple<string, string>("/*", "*/"), startIndex);
        }
        public static bool IsNumeric(this string s) {
            if (string.IsNullOrEmpty(s)) return false;

            try {
                Int32.Parse(s);
            } catch (Exception) {
                return false;
            }

            return !false;
        }
    }
}
