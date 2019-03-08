using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeStyle {
    public static class Extensions {

        public static bool IsNullOrEmpty<T>(List<T> l) => l == null || l.Count == 0;

        public static void Print<T>(this List<T> l) {
            int counter = 1;
            foreach (var x in l) {
                Console.WriteLine("Element nr {0}", counter);
                Console.WriteLine(x.ToString());
                counter++;
            }
        }

        public static bool IsNullOrEmpty<T, U>(Dictionary<T, U> dict) => dict == null || dict.Count == 0;
    }
}
