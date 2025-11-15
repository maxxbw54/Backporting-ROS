using JsonViz.Utils;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonViz.Actors
{
    class Formatter
    {
        public const int MAX_TOKENS = 65536;
        public const double TOKENS_PER_CHAR = 0.7;
        public static void Run()
        {
            Commit[] commitFile = Editor.OpenEmpty();

            int totalCommits = commitFile.Length;
            commitFile = commitFile.Randomize().Where(c => !IsCommitJSONLTooLarge(c) && GetCleanChangeBlocks(c.Changes).Count > 0).ToArray();
            Console.WriteLine($"Removed {totalCommits - commitFile.Length} commits because they were too big.");
            Console.WriteLine($"totalCommits[{commitFile.Length}]");


            List<Commit> training = [.. commitFile];
            List<Commit> test = [];
            List<Commit> validation = [];

            //Vizualizer.PrintChanges(training[85].Changes);

            const int TRAINING_SIZE = 1500;
            const int TEST_VALID_SIZE = (int)(TRAINING_SIZE * 0.1);

            //random training data
            totalCommits = training.Count;
            while (test.Count < TEST_VALID_SIZE)
            {
                Commit c = training[0];
                test.Add(c);
                training.Remove(c);
            }

            training = Balance(training, TRAINING_SIZE + TEST_VALID_SIZE);

            //chatgpt 50/50 validation
            while (validation.Count < TEST_VALID_SIZE)
            {
                Commit BP = training.Find(c => c.IsBackported);
                Commit NBP = training.Find(c => !c.IsBackported);
                validation.Add(BP);
                validation.Add(NBP);
                training.Remove(BP);
                training.Remove(NBP);
            }

            //not included print out
            List<Commit> notIncluded = commitFile.Except(training).Except(validation).Except(test).ToList();
            Analyzer.PrintCounts(notIncluded.ToArray(), "not included :(");

            Analyzer.PrintCounts(training.ToArray(), "TRAINING");
            Analyzer.PrintCounts(validation.ToArray(), "VALIDATION");
            Analyzer.PrintCounts(test.ToArray(), "TEST");

            string foldername = "UncontextualGPT4.1largerTokenLimit";
            Editor.SaveString(GetJSONL(ToTrainingDataPoints(training.ToArray())), $"training-{training.Count}.jsonl", foldername);
            Editor.SaveString(GetJSONL(ToTrainingDataPoints(validation.ToArray())), $"validation-{validation.Count}.jsonl", foldername);
            Editor.SaveString(GetJSONL(ToTrainingDataPoints(test.ToArray())), $"test-{test.Count}.jsonl", foldername);
            Editor.SaveJSON(test, "test.json", foldername);
            Editor.SaveJSON(training, $"training-{training.Count}.json", foldername);
            Editor.SaveJSON(validation, $"validation-{validation.Count}.json", foldername);

        }

        public static bool EqualsMOE(float n1, float n2, float MOE)
        {
            return Math.Abs(n1 - n2) < MOE;
        }

        public static List<Commit> Balance(List<Commit> dataset, int desiredsize)
        {
            int backported = dataset.Where(c => c.IsBackported).Count();
            int notbackported = dataset.Where(c => !c.IsBackported).Count();
            int min = Math.Min(Math.Min(backported, notbackported), desiredsize / 2);
            return SelectSmallQuantity(dataset.ToArray(), min, min).ToList();
        }
        public static bool IsCommitJSONLTooLarge(Commit commit)
        {
            return GetJSONL(ToTrainingDataPoints([commit])).Length * TOKENS_PER_CHAR > (MAX_TOKENS);
        }

        public static Commit[] SelectSmallQuantity(Commit[] commits, int backported, int notBackported)
        {
            List<Commit> smaller = [];
            smaller.AddRange(commits.Randomize().Where(c => c.IsBackported).Take(backported));
            smaller.AddRange(commits.Randomize().Where(c => !c.IsBackported).Take(notBackported));
            return smaller.ToArray();
        }
        public static Commit[] CompressAll(Commit[] commits)
        {
            foreach (var commit in commits) CompressChanges(commit);
            return commits;
        }
        private static void CompressChanges(Commit commit)
        {
            commit.Changes = CompressCode(commit.Changes);
        }

        public static string CompressCode(string code)
        {
            List<string> compressedChanges = code.Split("\n").ToList();


            compressedChanges = compressedChanges.Where(line => new char[] { '+', '-' }.Any(ch => line.StartsWith(ch))).ToList();

            /*  Vizualizer.PrintChanges(commit.Changes);
            Console.WriteLine("-----------------------------------------------");
            Vizualizer.PrintChanges(string.Join('\n', compressedChanges)); 
            Vizualizer.ClearScreen(); */

            return string.Join('\n', compressedChanges);
        }

        private static string GetTextLabelJson(Commit[] commits)
        {
            return JsonSerializer.Serialize(commits.Select(c => new BertDataPoint()
            {
                text = $"Message:{c.RawMessage}\nCode Changes:{c.Changes}",
                label = c.IsBackported ? 1 : 0
            }).ToArray(), new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }

        public static OpenAIQueryObject[] ToTrainingDataPoints(Commit[] commits) => [.. commits.Select(c => ToQuery(c))];
        public static OpenAIQueryObject ToQuery(Commit commit, bool includeAnswer = true, float? temperature = null, string? model = null)
        {
            
            OpenAIQueryObject q = new()
            {
                temperature = temperature,
                model = model,
                messages = []
            };
            q.messages.Add(new()
            {
                role = "system",
                content = "You are a commit classifier that decides if a code commit is important enough to be backported. " +
                            "‘+’ marks added lines, ‘–’ marks removed lines. Only the changed lines of code are given. " +
                            "Reply only with 'Yes' or 'No'."
            });
            foreach (string block in GetCleanChangeBlocks(commit.Changes))
            {
                q.messages.Add(new()
                {
                    role = "user",
                    content = $"{block}"
                });
            }
            if (includeAnswer)
            {
                q.messages.Add(new()
                {
                    role = "assistant",
                    content = commit.IsBackported ? "Yes" : "No"
                });
            }
            return q;
        }

        public static List<string> GetCleanChangeBlocks(string code)
        {
            return GetChangeBlocks(code, '+', '-').Where(block => !block.StartsWith("+++") && !block.StartsWith("---")).ToList();
        }
        private static List<string> GetChangeBlocks(string code, int contextLines, params char[] lineStarter)
        {
            //create change blocks
            List<string> changeBlocks = [];
            string[] lines = Regex.Split(code, "\r?\n");
            List<string> blockLines = null;
            foreach (string line in lines)
            {
                if (lineStarter.Any(c => line.StartsWith(c)))
                {
                    blockLines ??= [];
                    blockLines.Add(line);
                }
                else if (blockLines != null)
                {
                    changeBlocks.Add(string.Join('\n', blockLines));
                    blockLines = null;
                }
            }
            if (blockLines != null) changeBlocks.Add(string.Join('\n', blockLines));

            return changeBlocks;
        }


        private static string GetJSONL(OpenAIQueryObject[] datapoints)
        {
            StringBuilder str = new();
            foreach (var datapoint in datapoints)
            {
                str.AppendLine(JsonSerializer.Serialize(datapoint, new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }));
            }
            return str.ToString();
        }
    }


    public class BertDataPoint
    {
        public string text { get; set; }
        public int label { get; set; }
    }
}
