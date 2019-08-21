using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStyle {
    public class Code {

        private List<string> lines;
        private List<string> comments;
        private object defines;//temporary field


        public Code() {}

        public List<string> Lines { get => lines; set => lines = value; }
        public List<string> Comments { get => comments; set => comments = value; }
    }
}
