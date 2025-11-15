using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using PatchFile = System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>;

namespace JsonViz.Utils
{
    public static class Misc
    {

        public static void SplitProgram()
        {
            const string FILE_TO_SPLIT = "";
            PatchFile deserialized = JsonSerializer.Deserialize<PatchFile>(File.ReadAllText(FILE_TO_SPLIT)).ToList();
            foreach (var commit in deserialized)
            {
                foreach (var anotherCommit in deserialized)
                {
                    if (commit != anotherCommit && commit["SHA"] == anotherCommit["SHA"]) throw new Exception("Duplicates!");
                }
            }
            deserialized = Randomize(deserialized).ToList();
            PatchFile kyle = [];
            PatchFile pankaj = [];
            for (int i = 0; i < deserialized.Count; i++)
            {
                if (i < deserialized.Count / 2) kyle.Add(deserialized[i]);
                else pankaj.Add(deserialized[i]);
            }
            string kyleFile = Path.Combine(Program.WorkingFolder, FILE_TO_SPLIT.Split(".")[0] + $"-Kyle-{kyle.Count}-commits.json");
            string pankajFile = Path.Combine(Program.WorkingFolder, FILE_TO_SPLIT.Split(".")[0] + $"-Pankaj-{pankaj.Count}-commits.json");
            File.WriteAllText(kyleFile, JsonSerializer.Serialize(kyle, new JsonSerializerOptions() { WriteIndented = true }));

            File.WriteAllText(pankajFile, JsonSerializer.Serialize(pankaj, new JsonSerializerOptions() { WriteIndented = true }));
            Console.WriteLine($"Written files kyle[{kyle.Count}] and pankaj[{pankaj.Count}]");


            PatchFile realKyle = JsonSerializer.Deserialize<PatchFile>(File.ReadAllText(kyleFile));
            PatchFile realPankaj = JsonSerializer.Deserialize<PatchFile>(File.ReadAllText(pankajFile));
            Console.WriteLine($"The actual amount of commits in kyle is {realKyle.Count}");
            Console.WriteLine($"The actual amount of commits in pankaj is {realPankaj.Count}");
        }



        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = Random.Shared;
            return source.OrderBy((item) => rnd.Next());
        }
    }
}
