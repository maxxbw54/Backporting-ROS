using JsonViz.Utils;

namespace JsonViz.Actors
{
    class OpenAIAnalyzer
    {
        public static void Run()
        {
            string[] experiment =
                ["UncontextualGPT4.1largerTokenLimit-09-17-25-01-47-24", "ft:gpt-4.1-2025-04-14:north-carolina-state-university:no-context-blocks-backport-worthy-classifier:CGs7XzLd"]
            ;


            Commit[] points = Editor.Open($@"C:\Users\stypl\development\crab-outputs\saves\{experiment[0]}\test-SAVE.json");

            // Commit[] isolated = points.ToList().ToArray().Randomize().Take((int)(points.Length * 0.2)).ToArray();
            OpenAIAnalyzeFineTune(points, experiment[0], experiment[1]);
            Analyzer.PrintCounts(points, "backport-worthy-classifier");
            Analyzer.PrintAnalysis(points, "", "backport-worthy-classifier", -1);



            //OpenAIAnalyzeFew(points, examples);
        }
        public static void OpenAIAnalyzeFineTune(Commit[] commits, string saveFolder, string model)
        {
            OpenAiClient openAPI = new();

            int i = 0;
            foreach (var commit in commits)
            {
                try
                {
                    string response = "";
                    bool validResponse = false;
                    for (int retries = 0; !validResponse && retries < 5; retries++)
                    {
                        try
                        {
                            // string code = retries == 0 ? commit.Changes : Formatter.CompressCode(commit.Changes);

                            // string prompt = $"Message:{commit.RawMessage}\\nCode:{JsonSerializer.Serialize(code).Trim('"')}\\n";
                            // Console.WriteLine(prompt);
                            OpenAIQueryObject promptObj = Formatter.ToQuery(commit, false, 0f, model);
                            Console.WriteLine("\nQuery created");
                            Console.WriteLine("Characters: " + promptObj.messages.Select(m => m.content.Length).Sum());
                            Console.WriteLine("IsBackported: " + commit.IsBackported);

                            response = openAPI.QueryChatGPT(promptObj)
                                            .ToLower()
                                            .Trim();

                            if (new string[] { "yes", "no" }.Any(s => response.Contains(s))) validResponse = true;
                            else
                            {
                                Console.WriteLine($"[Retry {retries}] Invalid response from ChatGPT! [{response}] Reprompting");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[{retries}] Error in query... Reprompting in a second.");
                            Console.WriteLine(e.Message);
                            Thread.Sleep(1000);
                        }

                    }

                    commit.Analysis ??= new()
                    {
                        ChatGPTResponse = response,
                        PredictsBackported = response.Contains("yes")
                    };
                    Editor.SaveJSON(commits, "finetune", "finetune-" + saveFolder);


                    Console.WriteLine($"[{commit.SHA}][{commit.RawMessage,10}] -> [{response}] {i} / {commits.Length}");

                    i++;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Major error in commit, skipping...");
                    Console.WriteLine(e.Message);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
