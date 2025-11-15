using JsonViz.Actors;
using JsonViz.Utils;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace JsonViz
{
    internal class Program
    {
        public static string MasterEmptyFileName = "unlabeled-commits-3793.json";//"zero-shot-3793.json";
        public static string WorkingFolder = "C:\\Users\\stypl\\development\\crab-outputs";
        public static string ResultFolder = Path.Combine(WorkingFolder, "result");
        public static string SavesFolder = Path.Combine(WorkingFolder, "saves");
        public static string AnalysisFolder = Path.Combine(WorkingFolder, "analysis");
        static void Main(string[] args)
        {
            //  Formatter.Run();
           //   OpenAIAnalyzer.Run();
            Analyzer.Run();
            // TimeAnalyzer.Run();

        }
    }
}
