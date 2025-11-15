using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonViz.Utils
{
    public class Commit : IEquatable<Commit>
    {
        public string Repository { get; set; }
        public string SHA { get; set; }
        public string RawMessage { get; set; }
        public string Changes { get; set; }
        public string BackportedTo { get; set; }
        public bool IsBackported { get; set; }
        public DateTime? Timestamp { get; set; }
        public DateTime? BackportedToTimestamp { get; set; }
        public AnalysisData Analysis { get; set; }

        public bool Equals(Commit? other)
        {
            if (other == null) return false;
            return SHA == other.SHA;
        }

        public override int GetHashCode()
        {
            return SHA.GetHashCode();
        }

        public class AnalysisData
        {
            public string ChatGPTResponse { get; set; }
            public bool PredictsBackported { get; set; }
            public bool Compressed { get; set; }
            
        }
    }
}
