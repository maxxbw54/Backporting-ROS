using FuzzySharp;
using System.Text;

namespace RosClonedDistroMiner
{
    public static class Extensions
    {

        public static string HorizontalLine(int width)
        {
            StringBuilder str = new();
            for (int i = 0; i < width; i++) str.Append("-");
            str.Append("\n");
            return str.ToString();
        }
        public static string SideBySide(string str1, string str2, int columnWidth)
        {
            var lines1 = str1.Split('\n');
            var lines2 = str2.Split('\n');

            var wrappedLines1 = lines1.SelectMany(line => CharWrap(line, columnWidth).Split('\n')).ToArray();
            var wrappedLines2 = lines2.SelectMany(line => CharWrap(line, columnWidth).Split('\n')).ToArray();

            int maxLines = Math.Max(wrappedLines1.Length, wrappedLines2.Length);

            StringBuilder output = new();

            for (int i = 0; i < maxLines; i++)
            {
                string line1 = i < wrappedLines1.Length ? wrappedLines1[i].PadRight(columnWidth) : new string(' ', columnWidth);
                string line2 = i < wrappedLines2.Length ? wrappedLines2[i].PadRight(columnWidth) : new string(' ', columnWidth);

                output.Append(line1);
                output.Append(" | ");
                output.Append(line2);
                output.Append('\n');
            }
            return output.ToString();
        }

        public static void PrintSideBySide(string str1, string str2, int columnWidth)
        {
            Console.Write(SideBySide(str1, str2, columnWidth));
        }
        static string CharWrap(string text, int maxWidth)
        {
            var result = "";
            for (int i = 0; i < text.Length; i += maxWidth)
            {
                result += text.Substring(i, Math.Min(maxWidth, text.Length - i)) + "\n";
            }
            return result.TrimEnd();
        }

        public static void ClearCurrentConsoleLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void PrintProgressBar(float percentage, int length)
        {
            Console.Write("|");
            for (int i = 0; i < length; i++)
            {
                if ((float)i / length < percentage) Console.Write("■");
                else Console.Write(" ");
            }
            Console.WriteLine("|");
        }
        public static T FuzzFindSimilar<T>(this List<T> list, T item, Func<T, string> exposer, int minFuzzRatio = 95, int threads = 1)
        {
            int count = list.Count;
            List<T>[] buckets = new List<T>[threads];
            for (int i = 0; i < buckets.Length; i++) buckets[i] ??= [];

            for (int i = 0; i < count; i++)
            {
                int bucket = (int)((float)i / count * threads);
                buckets[bucket].Add(list[i]);
            }

            List<Thread> workers = [];
            bool isFound = false;
            T foundItem = default;
            foreach (List<T> bucket in buckets)
            {
                
                Thread worker = new Thread(() =>
                {
                    foreach(T bucketItem in bucket)
                    {
                        if (isFound) break;
                        if (Fuzz.Ratio(exposer(bucketItem), exposer(item)) > minFuzzRatio)
                        {
                            foundItem = bucketItem;
                            isFound = true;
                        }
                    }
                });
                worker.Start();
                workers.Add(worker);
            }
            foreach (Thread worker in workers) worker.Join();

            return foundItem;
        }

        public static void LoadAll(this List<Commit> commits, Action<int> onLoaded, int threads = 1)
        {
            int loaded = 0;
            int count = commits.Count;
            List<Commit>[] commitBuckets = new List<Commit>[threads];
            for (int i = 0; i < commitBuckets.Length; i++) commitBuckets[i] ??= [];

            for (int i = 0; i < count; i++)
            {
                int bucket = (int)((float)i / count * threads);
                commitBuckets[bucket].Add(commits[i]);
            }

            List<Thread> workers = [];
            foreach (List<Commit> bucket in commitBuckets)
            {
                Thread worker = new Thread(() =>
                {
                    foreach (Commit commit in bucket)
                    {
                        commit.LoadChanges();
                        onLoaded(loaded++);
                    }
                });
                worker.Start();
                workers.Add(worker);
            }
            foreach (Thread worker in workers) worker.Join();
            foreach (Commit commit in commits) if (!commit.IsChangeLoaded()) throw new Exception("Change not loaded for some reaons.");
        }
    }
}
