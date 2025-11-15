using JsonViz.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonViz.Actors
{
    class TimeAnalyzer
    {
        public static void Run()
        {
            Commit[] empty = Editor.Open(@"C:\Users\stypl\development\crab-outputs\commits-3793-Aug-10-25-02-14-24.json").Where(c => c.IsBackported).ToArray();



            double[] minutes = empty.Select(c => (c.BackportedToTimestamp - c.Timestamp).Value.TotalMinutes).Where(c => c > 0).ToArray();

            Console.WriteLine($"Min:{minutes.Min()}");
            Console.WriteLine($"Max:{TimeSpan.FromMinutes(minutes.Max()).TotalDays}");
            Console.WriteLine($"Avg:{TimeSpan.FromMinutes( minutes.Average()).TotalDays}");
        }
    }
}
