using JsonViz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonViz.Actors
{
    public static class Editor
    {
        public static string ProgramTimeString = DateTime.Now.ToString("MM-dd-yy-hh-mm-ss");
        private static string GetSavesFolderPath(string name = "saves")
        {
            return Path.Combine(Program.SavesFolder, $"{name}-{ProgramTimeString}");
        }
        private static void MakeSaveFolder(string ourSaveFolder)
        {
            if (!Directory.Exists(ourSaveFolder))
            {
                Console.WriteLine($"Creating saves folder at {ourSaveFolder}");
                Directory.CreateDirectory(ourSaveFolder);
            }
        }
        public static void SaveJSON(IEnumerable<Commit> commits, string name, string folderAddendum = "")
        {
            string saveFolder = GetSavesFolderPath(folderAddendum);
            string saveFile = name.Split(".")[0] + $"-SAVE.json";
            MakeSaveFolder(saveFolder);

            File.WriteAllText(Path.Combine(saveFolder, saveFile), JsonSerializer.Serialize(commits, new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }));
            Console.WriteLine($"Saved {saveFile} to {saveFolder}!");
        }

        public static void SaveString(string stringData, string nameWithExtension, string folderAddendum = "")
        {
            string saveFolder = GetSavesFolderPath(folderAddendum);
            MakeSaveFolder(saveFolder);

            string filename = nameWithExtension.Split(".")[0];
            string extension = nameWithExtension.Split(".")[1];

            File.WriteAllText(Path.Combine(saveFolder, $"{filename}-SAVE.{extension}"), stringData);
            Console.WriteLine("Saved!");
        }
        public static Commit[] Open(string directory, string filename = "")
        {
            return JsonSerializer.Deserialize<Commit[]>(File.ReadAllText(Path.Combine(directory, filename)));
        }
        public static Commit[] OpenEmpty()
        {
            return Open(Program.WorkingFolder, Program.MasterEmptyFileName);
        }
        public static Commit[] OpenResult(string filename)
        {
            return Open(Program.ResultFolder, filename);
        }
       
        public static readonly Dictionary<char, string> TYPES = new Dictionary<char, string>()
        {
            { 'b', "Bug Fix"},
            { 'f', "Functional Improvement"},
            { 'n', "NonFunctional Enhancement"},
            { 'd', "Documentation"},
            { 't', "Test"}
        }; 
        public static DateTime ProgramStartTime = DateTime.Now;
        /*public static void EditorProgram(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                var fileInfo = new FileInfo(args[0]);
                Program.FileName = fileInfo.Name;
                Program.WorkingFolder = fileInfo.Directory.FullName;
            }


            string patchesFile = Path.Combine(Program.WorkingFolder, Program.FileName);

            if (!File.Exists(patchesFile))
            {
                Console.WriteLine($"Couldn't find anything at the location {patchesFile}. Please restart the program and drag the data points file onto the .exe");
                Console.ReadLine();
                return;
            }
            string contents = File.ReadAllText(patchesFile);

            Console.WriteLine($"Creating saves folder at {SavesFolder}");
            Directory.CreateDirectory(SavesFolder);

            Console.WriteLine("Loading commits...");
            Commit[] commits = JsonSerializer.Deserialize<Commit[]>(contents);
            int numLabeledAtStart = CountLabeled();
            Console.WriteLine($"Success.\n\nThere are {commits.Length} commits.\n\nThere are {new DirectoryInfo(SavesFolder).GetFiles().Length} saves.");
            Console.WriteLine($"\nThe last unlabeled commit is at index {numLabeledAtStart}.");
            Thread.Sleep(5250);
            Vizualizer.ClearScreen();

            for (int i = 0; i < commits.Length; i++)
            {
                DateTime start = DateTime.Now;
                Commit commit = commits[i];

                Console.WriteLine($"SHA: " + commit.SHA);
                Console.WriteLine($"Repository: " + commit.Repository);
                Console.WriteLine($"RawMessage: " + commit.RawMessage);
                Console.WriteLine($"BackportedTo: " + commit.BackportedTo);

                Console.WriteLine("------------Changes-----------------");

                commit.Changes.PrintChanges();

                Console.WriteLine("------------END OUTPUT-----------------");
                if (!string.IsNullOrEmpty(commit.Type)) Console.WriteLine($"Previous Type: " + commit.Type);

                bool successfulParse = false;
                while (!successfulParse)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(string.Join(", ", TYPES.ToArray().Select(type => $"{type.Value} ({type.Key})")));
                    Console.WriteLine("Type goto # to jump to a commit at a specific index. (replace # with a number) ");
                    Console.ResetColor();
                    Console.Write($"[{i}] Type: ");

                    string input = Console.ReadLine();
                    string trimmedInput = input.Trim().ToLower();

                    try
                    {
                        if (trimmedInput.StartsWith("goto"))
                        {
                            i = int.Parse(trimmedInput.Split(" ")[1]) - 1;
                            successfulParse = true;
                        }
                        else if (trimmedInput.Length == 1 && TYPES.Keys.Any(c => c == trimmedInput[0]))
                        {
                            commit.Type = TYPES[trimmedInput[0]];

                            Console.ResetColor();

                            Console.WriteLine($"Updated to {commit.Type}!");

                            Save(commits, i);
                            successfulParse = true;
                        }
                        else
                        {
                            Console.WriteLine("Command not recognized.");
                            successfulParse = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error parsing input or writing to save file.\nError: {e.Message}");
                        successfulParse = false;
                    }
                    Thread.Sleep(500);
                }

                Vizualizer.ClearScreen();
                TimeSpan remaining = EstimateRemainingTime();
                if (remaining != TimeSpan.Zero)
                {
                    Console.WriteLine($"Time remaining estimate: {remaining.Hours,2} hours, {remaining.Minutes,2} minutes, {remaining.Seconds,2} seconds");
                }
            }

        int CountLabeled()
            {
                int count = 0;
                for (int i = 0; i < commits.Length; i++)
                {
                    if (string.IsNullOrEmpty(commits[i].Type))
                    {
                        count = i;
                        break;
                    }
                }
                return count;
            }

            TimeSpan EstimateRemainingTime()
            {
                float numLabeled = CountLabeled() - numLabeledAtStart;
                if (numLabeled < 1) return TimeSpan.Zero;
                TimeSpan avgTimePerLabel = (DateTime.Now - ProgramStartTime) / numLabeled;
                float numRemaining = commits.Length - CountLabeled();
                return avgTimePerLabel * numRemaining;
            }
        }*/
    }
}
