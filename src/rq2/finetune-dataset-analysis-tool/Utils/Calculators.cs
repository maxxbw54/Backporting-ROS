using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonViz.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AnalysisCalculators
    {
        public class F1Result
        {
            public double Precision;
            public double Recall;
            public double F1;
        }
        public static F1Result ComputeF1OnClass(int TP, int FP, int FN)
        {
            double precision = TP / ((double)TP + FP);
            double recall = TP / ((double)TP + FN);
            double f1 = 2 * (precision * recall) / (precision + recall);
            return new F1Result
            {
                Precision = precision,
                Recall = recall,
                F1 = f1
            };
        }

        public static F1Result ComputeMicroF1(int TP1, int FP1, int FN1, int TP2, int FP2, int FN2)
        {
            double precision = ((double)TP1 + TP2) / (TP1 + TP2 + FP1 + FP2);
            double recall = (double)(TP1 + TP2) / (TP1 + TP2 + FN1 + FN2);
            double f1 = 2 * (precision * recall) / (precision + recall);
            return new F1Result
            {
                Precision = precision,
                Recall = recall,
                F1 = f1
            };
        }

        public static F1Result ComputeMacroF1(F1Result C1, F1Result C2)
        {
            double precision = (C1.Precision + C2.Precision) / 2.0;
            double recall = (C1.Recall + C2.Recall) / 2.0;
            double f1 = (C1.F1 + C2.F1) / 2.0;
            return new F1Result
            {
                Precision = precision,
                Recall = recall,
                F1 = f1
            };
        }
        public static double ComputeCohenKappa(List<string> rater1, List<string> rater2)
        {
            if (rater1.Count != rater2.Count)
                throw new ArgumentException("Both raters must have the same number of items.");

            var categories = rater1.Concat(rater2).Distinct().ToList();
            var confusionMatrix = new Dictionary<(string, string), int>();

            // Initialize matrix
            foreach (var c1 in categories)
            {
                foreach (var c2 in categories)
                {
                    confusionMatrix[(c1, c2)] = 0;
                }
            }

            // Fill matrix
            for (int i = 0; i < rater1.Count; i++)
            {
                confusionMatrix[(rater1[i], rater2[i])]++;
            }

            int total = rater1.Count;

            // Calculate observed agreement p_o
            double po = categories.Sum(c => confusionMatrix[(c, c)]) / (double)total;

            // Calculate expected agreement p_e
            var rater1Counts = new Dictionary<string, int>();
            var rater2Counts = new Dictionary<string, int>();

            foreach (var c in categories)
            {
                rater1Counts[c] = rater1.Count(x => x == c);
                rater2Counts[c] = rater2.Count(x => x == c);
            }

            double pe = categories.Sum(c =>
                rater1Counts[c] / (double)total * (rater2Counts[c] / (double)total)
            );

            // Kappa formula
            if (pe == 1) return 1.0; // Avoid divide by zero

            double kappa = (po - pe) / (1 - pe);
            return kappa;
        }
    }

}
