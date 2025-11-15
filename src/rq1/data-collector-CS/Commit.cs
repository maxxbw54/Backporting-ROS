using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RosClonedDistroMiner
{
    public class Commit : IEquatable<Commit>
    {
        [JsonIgnore]
        public Repository Repo { get; set; }
        public string Repository { get; set; }
        public string RawMessage { get; set; }

        [JsonIgnore]
        public string Branch { get; set; }
        public string SHA { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Changes { get; set; }
        public bool IsBackported { get; set; }

        [JsonIgnore]
        public string? BackportedToBranch { get; set; } = null;
        [JsonIgnore]
        public string? BackportedToSHA { get; set; } = null;
        public DateTime? BackportedToTimestamp { get; set; }
        //public string Type { get; set; } = "";

        public Commit(string formatted, string branch, Repository repo)
        {
            var parts = formatted.Split(' ', 2);
            SHA = parts[0].Trim();
            RawMessage = parts.Length > 1 ? parts[1].Trim() : "";
            Branch = branch;
            Repo = repo;
            Repository = repo.Name;
            Changes = "NOT LOADED";
            IsBackported = false;
            BackportedToBranch = null;


            //Change parsing
            /* ModifiedFiles = new();
             ModifiedFilesSegmented = new();
             if (!extractFileContents) return;

             RawMessage = repo.ExecuteGitCommand($"log --format=%B -n 1 {SHA}");

             string[] modifiedFiles = repo.ExecuteGitCommand($"diff --name-only {SHA}^ {SHA}")
             .Split("\n", StringSplitOptions.RemoveEmptyEntries)
             .Where(path => !new string[] { ".github" }.Any(str => path.Contains(str)))
             .Select(path => Path.Combine(repo.Path, path))
             .ToArray();

             var modified = new Modified<List<File>>()
             {
                 Before = new List<File>(),
                 After = new List<File>()
             };

             //get the contents of all modified files on the commit BEFORE this one
             repo.ExecuteGitCommand($"checkout {SHA}^1");
             foreach (string path in modifiedFiles) modified.Before.Add(new File(path, extractFileContents));

             //switch to this SHA, then get contents
             repo.ExecuteGitCommand($"checkout {SHA}");
             foreach (string path in modifiedFiles) modified.After.Add(new File(path, extractFileContents));

             //convert Modified<List<FileSegment>> to List<Modified<FileSegment>>
             foreach (string path in modifiedFiles)
             {
                 ModifiedFiles.Add(new Modified<File>()
                 {
                     Before = modified.Before.Find(file => file.Path.Equals(path)) ?? throw new Exception("darn 1"),
                     After = modified.After.Find(file => file.Path.Equals(path)) ?? throw new Exception("darn 2")
                 });
             }
           /*  foreach (var wholeFile in ModifiedFiles)
             {
                 ModifiedFilesSegmented.AddRange(FindDifferences(wholeFile.Before.Contents, wholeFile.After.Contents, 2)
                     .Select(diff => new Modified<FileSegment>()
                     {
                         Before = new FileSegment(wholeFile.Before.Path, diff.Before, wholeFile.Before.Exists),
                         After = new FileSegment(wholeFile.After.Path, diff.After, wholeFile.After.Exists)
                     }));
             }*/
        }

        public bool IsChangeLoaded()
        {
            return Changes != "NOT LOADED";
        }
        public void MarkBackport(string toRepo, string toSHA)
        {
            IsBackported = true;
            BackportedToBranch = toRepo;

            BackportedToSHA = toSHA;
        }
        public void LoadChanges()
        {
            Changes = Repo.ExecuteGitCommand("show --pretty=\"\" --no-prefix " + SHA);
        }

        public void LoadTimestamp()
        {
            Timestamp = load(SHA);
            BackportedToTimestamp = BackportedToSHA == null ? null : load(BackportedToSHA);

            DateTime load(string sha)
            {
                string timestamp = Repo.ExecuteGitCommand($"show --no-patch --format=%ci {sha}");
                timestamp = string.Join(" ", timestamp.Split(" ").Take(2));
                return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

        }

        public bool IsProbablyNotCode()
        {
            return RawMessage.Contains("Changelog") || Regex.IsMatch(RawMessage, @"\d+\.\d+\.\d+");
        }
        public string CleanedMessage()
        {
            return Regex.Replace(RawMessage.Trim(), @"\([^()]*#\d+[^()]*\)", "");
        }
        public override string ToString()
        {
            return $"[{SHA/*Hash.Substring(0, 5)*/}] {RawMessage}";
        }
        public bool Equals(Commit? other)
        {
            return other != null && other.SHA == SHA;
        }
    }
}
