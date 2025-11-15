using System.Data;
using System.Text.Json;

namespace RosClonedDistroMiner
{
    class Program
    {
        public const bool VIEW_CODE_CHANGES = true;
        public const string REPOS_FOLDER = @"C:\Users\stypl\development\crabscratch";
        //  public const string OUTPUT_PATH_NONBACKPORTED = @"C:\Users\stypl\development\crabscratch\output-non-backported.json";
        public const int COLUMN_WIDTH = 85;
        public const int ANALYZE_COUNT = 300000000;

        private static List<Commit> Commits = new();

        private static string outputFileName => $"commits-{Commits.Count}-{DateTime.Now:MMM-dd-yy-hh-mm-ss}.json";
        private const string outputFolder = "C:\\Users\\stypl\\development\\crab-outputs";
        private static JsonSerializerOptions jsonOptions => new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        //   private static readonly List<MainlineBackportPair<CommitInfo>> NonBackportedPatchesDataSet = new();
        static void Main(string[] args)
        {
            Log(outputFileName);
            string[] repos = new DirectoryInfo(REPOS_FOLDER).GetDirectories().Select(dir => dir.Name).ToArray();

            string[] compareAgainstBranches = ["jazzy", "humble", "foxy", "galactic", "iron"];
            string[] excludeCommits = [/*
                @"C:\Users\stypl\development\roscrabs\round1\newdata100_77.json",
                @"C:\Users\stypl\development\roscrabs\round1\output-backported-21-commits_20.json",
                @"C:\Users\stypl\development\roscrabs\round2_100pts\newdata100.json",
                @"C:\Users\stypl\development\roscrabs\round2_100pts\output-backported-21-commits.json",
                @"C:\Users\stypl\development\roscrabs\round3_30pts\output-backported-30.json",
                @"C:\Users\stypl\development\roscrabs\deduped_diff_format.json"*/];
            string mainlineBranchName = "rolling";

            List<Exception> errors = [];

            int repoIdx = 0;
            foreach (string repoName in repos)
            {
                Log($"{repoIdx} / {repos.Length}", ConsoleColor.Green);
                Repository repo = new Repository(Path.Combine(REPOS_FOLDER, repoName), repoName);
                repo.ExecuteGitCommand("fetch --all");
                foreach (string branch in compareAgainstBranches)
                {
                    try
                    {
                        AnalyzeBranchPair(mainlineBranchName, branch, repo);
                    }
                    catch (Exception e)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] Error analyzing branch {branch} of {repo}.");
                        Console.ResetColor();
                        errors.Add(e);
                    }

                }
                repoIdx++;
            }

          //  File.WriteAllText(Path.Combine(outputFolder, "UNCLEANED" + outputFileName), JsonSerializer.Serialize(Commits, jsonOptions));

            Console.WriteLine($"Cleaning BackportedPatches list. There are {Commits.Count} commits to check.");

            // Commits = Commits.Distinct().ToList();
            List<Commit> distinctCommits = [];
            foreach (Commit toAdd in Commits)
            {
                //is this commit already in distinct commits?
                var foundCommitsFromDistinct = distinctCommits.Where(c => c.SHA.Equals(toAdd.SHA));
                if (foundCommitsFromDistinct.Count() == 0)
                {
                    distinctCommits.Add(toAdd);
                }
                else if (foundCommitsFromDistinct.Count() == 1)
                {
                    Commit foundDistinct = foundCommitsFromDistinct.First();
                    if (toAdd.IsBackported)
                    {
                        foundDistinct.MarkBackport(toAdd.BackportedToBranch, toAdd.BackportedToSHA);
                    }
                }
                else
                {
                    throw new Exception("Damnit");
                }

            }
            Commits = distinctCommits;

            Log($"Removed duplicates. BackportedPatches[{Commits.Count}]");

         /*   foreach (string path in excludeCommits)
            {
                Log($"Checking against {path}");
                try
                {
                    Dictionary<string, object>[] diffJson = JsonSerializer.Deserialize<Dictionary<string, object>[]>(File.ReadAllText(path));
                    Log($"There are {diffJson.Length} commits in this file.");
                    Commits.RemoveAll(commit =>
                    {
                        //  Console.WriteLine($"diff {diffJson.Length} {diffJson[0]["SHA"]} {diffJson.Any(jsonCommit => (jsonCommit["SHA"] as string) == commit.SHA)}");
                        return diffJson.Any(jsonCommit => jsonCommit["SHA"].ToString() == commit.SHA);
                    });
                    Log($"Finished. BackportedPatches[{Commits.Count}]");
                }
                catch (Exception e)
                {
                    Log($"Error opening and excluding commits from {path}\nError: {e.Message}", ConsoleColor.Red);
                }
            }*/
          //  Console.WriteLine($"Done cleaning. BackportedPatches [{Commits.Count}]");


            Log("Loading Changes...");

            Commits.LoadAll(i =>
            {
                //if (i != 0) Extensions.ClearCurrentConsoleLine();
                float percentage = (float)i / Commits.Count;
                Console.WriteLine($"{percentage * 100:0.0}%");
                // Extensions.PrintProgressBar(percentage, 90);
                Commits[i].LoadTimestamp();
            }, 8);

            Console.WriteLine("Writing data file...");

            
            File.WriteAllText(Path.Combine(outputFolder, outputFileName), JsonSerializer.Serialize(Commits, jsonOptions));
            //    File.WriteAllText(OUTPUT_PATH_NONBACKPORTED, JsonSerializer.Serialize(NonBackportedPatchesDataSet, jsonOptions));

            //Visualize data
            // foreach (MainlineBackportCommitPair dataPoint in BackportedPatchesDataSet) Console.WriteLine(dataPoint.ToString());

            Console.WriteLine($"Total data points: {Commits.Count}");
        }
        static void AnalyzeBranchPair(string devBranchName, string releaseBranchName, Repository repo)
        {
            if (Commits.Count >= ANALYZE_COUNT)
            {
                Log($"There are {Commits.Count} commits, skipping {devBranchName} -> {releaseBranchName}");
                return;
            }
            Console.WriteLine($"Analyzing {repo.Name}");

            repo.ExecuteGitCommand($"checkout {devBranchName}");
            repo.ExecuteGitCommand($"checkout {releaseBranchName}");

            string firstNewCommitInForked = repo.ExecuteGitCommand($"merge-base {releaseBranchName} {devBranchName}").Trim();

            //Get a list of the mainline's commits up to when this forked from mainline
            repo.ExecuteGitCommand($"checkout {devBranchName}");
            List<Commit> devCommits = repo.ExecuteGitCommand($"log --pretty=\"format:%H %s\" {firstNewCommitInForked}..HEAD")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(str => new Commit(str, devBranchName, repo))
                .Where(c => !c.IsProbablyNotCode())
                .ToList();
            Console.WriteLine($"{devBranchName} has {devCommits.Count()} commits.");

            //Get a list of the forked distro's commits up to when this forked from mainline
            repo.ExecuteGitCommand($"checkout {releaseBranchName}");

            List<Commit> releaseBranchCommits = repo.ExecuteGitCommand($"log --pretty=\"format:%H %s\" {firstNewCommitInForked}..HEAD")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(str => new Commit(str, releaseBranchName, repo))
            .Where(c => !c.IsProbablyNotCode())
            .ToList();


            //List of backport data points
            Console.WriteLine($"{releaseBranchName} has {releaseBranchCommits.Count()} commits.");
            Console.ResetColor();


            foreach (var commitInReleaseBranch in releaseBranchCommits)
            {
               // if (Commits.Count >= ANALYZE_COUNT) continue;
                Commit? matchDev = devCommits.FuzzFindSimilar(commitInReleaseBranch, commit => commit.CleanedMessage(), 90, 8);
                if (matchDev == null) continue;

                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------------");

                Console.WriteLine($"{releaseBranchName,-20}{commitInReleaseBranch}");
                Console.WriteLine($"{devBranchName,-20}{matchDev}");

                matchDev.MarkBackport(releaseBranchName, commitInReleaseBranch.SHA);

            }

            Commits.AddRange(devCommits);
            Console.WriteLine($"Added {devCommits.Count} commits.");

            //  Log("----------------------------", ConsoleColor.Magenta);
            //  Log($"{unmatchedCommitsRelease.Count} Commits in {releaseBranchName} that couldn't be paired up to a commit in {devBranchName}", ConsoleColor.Magenta);

        }
        public static void Log(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

    }
}
