using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace CodeStyle {
    //TODO: think about the scale of code style analisation: e.g. max 10 points, max 100 points, etc.
    //TODO: implement this
    //TODO: make an enum with style problem value, e.g. slight problem - 25%, medium 50%, hard 75%, extreme 100% 

    public enum FlagSeverity : byte {
        None = 0,
        Slight = 25,
        Medium = 50,
        Concerning = 75,
        Unacceptable = 100
    }

    public class StyleStatistic {//flags are: commented out code, using defines, using "forbidden" names, using good variable names (not i, j, x, m, n etc.), indents, using comments in the intended way

        public const int MAX_SCORE = 150;

        public int score;

        public Dictionary<string, bool> styleFlags;
        public Dictionary<string, Dictionary<FlagSeverity, int>> flagScoreValues; //load this one from JSON

        public StyleStatistic() {
            score = MAX_SCORE;
            styleFlags = new Dictionary<string, bool>();
            flagScoreValues = new Dictionary<string, Dictionary<FlagSeverity, int>>();
            DumpScore();
            LoadFlagsValues();
        }

        public void CalculateScore() {

        }

        public void DumpScore() {
            flagScoreValues.Add("HasCommentedOutCode", new Dictionary<FlagSeverity, int>() {
                { FlagSeverity.None, 0 },
                { FlagSeverity.Slight, 0 },
                { FlagSeverity.Medium, 0 },
                { FlagSeverity.Concerning, 0},
                { FlagSeverity.Unacceptable, 0 }
            });
            flagScoreValues.Add("UsingForbiddenNames", new Dictionary<FlagSeverity, int>() {
                { FlagSeverity.None, 0 },
                { FlagSeverity.Slight, 0 },
                { FlagSeverity.Medium, 0 },
                { FlagSeverity.Concerning, 0 },
                { FlagSeverity.Unacceptable, 0 }
            });
            flagScoreValues.Add("UsingDefines", new Dictionary<FlagSeverity, int>() {
                { FlagSeverity.None, 0 },
                { FlagSeverity.Slight, 0 },
                { FlagSeverity.Medium, 0 },
                { FlagSeverity.Concerning, 0 },
                { FlagSeverity.Unacceptable, 0 }
            });
            flagScoreValues.Add("UsingComments", new Dictionary<FlagSeverity, int>() {
                { FlagSeverity.None, 0 },
                { FlagSeverity.Slight, 0 },
                { FlagSeverity.Medium, 0 },
                { FlagSeverity.Concerning, 0 },
                { FlagSeverity.Unacceptable, 0 }
            });
            flagScoreValues.Add("UsingReadableVariableNames", new Dictionary<FlagSeverity, int>() {
                { FlagSeverity.None, 0 },
                { FlagSeverity.Slight, 0 },
                { FlagSeverity.Medium, 0 },
                { FlagSeverity.Concerning, 0 },
                { FlagSeverity.Unacceptable, 0 }
            });

            using (var fs = new FileStream("../../ProgramFiles/statistics.json", FileMode.OpenOrCreate)) {
                using (var writer = new StreamWriter(fs)) {
                    writer.Write(JsonConvert.SerializeObject(flagScoreValues, Formatting.Indented));
                }
            }
        }

        private void LoadFlagsValues() {
            using (var reader = new StreamReader("../../ProgramFiles/statistics.json")) {
                flagScoreValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<FlagSeverity, int>>>(reader.ReadToEnd());
            }
        }
    }
}
