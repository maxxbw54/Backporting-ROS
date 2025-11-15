using JsonViz.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static JsonViz.Utils.AnalysisCalculators;

namespace JsonViz.Actors
{
    public static class Analyzer
    {
        private static List<string> csv = ["Experiment,Model,Training Size,Evaluation Size,kappa,microPrecision,microRecall,microF1,macroPrecision,macroRecall,macroF1,BackTP,BackFP,BackFN,NotBackTP,NotBackFP,NotBackFN"];
        public static void Run()
        {
           // PrintAnalysis(Editor.OpenResult("zero-shot-3793-v2.json"), "GPT4.", "Zero Shot", -1);
            PrintAnalysis(Editor.OpenResult("turbo-zero-shot.json"), "GPT3.5", "Turbo Zero Shot", -1);
            /*
                        Commit[] random = Editor.OpenResult("zero-shot-3793-v2.json");
                        RandomLabel(random);
                        PrintAnalysis(random, "N/A", "Randomly Assigned", -1);

                        Commit[] allFalse = Editor.OpenResult("zero-shot-3793-v2.json");
                        foreach (var commit in allFalse) commit.Analysis.PredictsBackported = false;
                        PrintAnalysis(allFalse, "N/A", "All False", -1);

                        Commit[] allTrue = Editor.OpenResult("zero-shot-3793-v2.json");
                        foreach (var commit in allFalse) commit.Analysis.PredictsBackported = true;
                        PrintAnalysis(allFalse, "N/A", "All True", -1);

                        // PrintAnalysis(Editor.OpenResult("bert-labeled-small-v2.json"), "SMALL BERT");

                        //  PrintAnalysis(Editor.OpenResult("bert-labeled-large-1-v2.json"), "LARGE BERT 3");

                        // PrintAnalysis(Editor.OpenResult("roberta-labeled-1-formatted-v2.json"), "ROBERTA 4e");

                        //  PrintAnalysis(Editor.OpenResult("roberta-labeled-1-raw-v2.json"), "ROBERTA 4r");

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-3-v2.json"), "GPT3.5 Turbo", "Commit message and code changes were included. Only the modified lines of code were included. No context lines were included. 200 Points (balanced 100 & 100)", 200);

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-large.json"), "GPT3.5 Turbo", "Commit message and code changes were included. Only the modified lines of code were included. No context lines were included. 1188 Points (balanced 594 & 594) No Context", 1188);

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-no-code.json"), "GPT3.5 Turbo", "Training on only the commit message.", 1188);

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-no-msg.json"), "GPT3.5 Turbo", "Training on only the code changes. No surrounding lines were included for context.", 1188);

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-no-code-with-context.json"), "GPT3.5 Turbo", "Training on only code changes with 3 context lines above and below each modified line.", 1220);

                        PrintAnalysis(Editor.OpenResult("backport-worthy-classifier-0-temp-change-blocks.json"), "GPT3.5 Turbo", "Grouped changed lines of code together in seperate messages. Temperature = 0", 1998);
                        PrintAnalysis(Editor.OpenResult("size-correct-change-block.json"), "GPT3.5 Turbo", "Size is 8:1:1 ratio. Change blocks. Temperature = 0", 1498);

                        File.WriteAllText(Path.Join(Program.AnalysisFolder,"analysis.csv"), string.Join("\n", csv));

                        Commit[] zero = Editor.OpenResult("zero-shot-final.json");
                        PrintAnalysis(zero, "3.5 turbo", "zero shot", -1);

                        Commit[] finetune = Editor.OpenResult("finetune-final.json");
                        PrintAnalysis(finetune, "3.5 turbo","finetune", -1);

                        foreach (Commit c in zero)
                        {
                            if (!finetune.Any(f => f.SHA == c.SHA)) throw new Exception();
                        }
                        Commit[] zeroCorrect = zero.Where(c => c.IsBackported == c.Analysis.PredictsBackported).ToArray();
                        Commit[] ftCorrect = finetune.Where(c => c.IsBackported == c.Analysis.PredictsBackported).ToArray();
                        Commit[] zeroIncorrect = zero.Where(c => c.IsBackported != c.Analysis.PredictsBackported).ToArray();
                        Commit[] ftIncorrect = finetune.Where(c => c.IsBackported != c.Analysis.PredictsBackported).ToArray();

                        Commit[] bothCorrect = zeroCorrect.Intersect(ftCorrect).ToArray();
                        Commit[] bothWrong = zeroIncorrect.Intersect(ftIncorrect).ToArray();
                        Commit[] zeroCorrectOnly = zeroCorrect.Intersect(ftIncorrect).ToArray();
                        Commit[] ftCorrectOnly = zeroIncorrect.Intersect(ftCorrect).ToArray();

                        Dictionary<string, Commit[]> divisions = new()
                        {
                            ["bothCorrect"] = bothCorrect,
                            ["bothWrong"] = bothWrong,
                            ["zeroCorrectOnly"] = zeroCorrectOnly,
                            ["ftCorrectOnly"] = ftCorrectOnly,
                        };
                        Console.WriteLine("-------------Amount Correct-----------");
                        foreach (var division in divisions)
                            Console.WriteLine($"{division.Key}, {division.Value.Count(),0:0.00}");


                        Console.WriteLine("-------------Average Change Blocks-----------");
                        foreach (var division in divisions) 
                            Console.WriteLine($"{division.Key}, {division.Value.Average(c => Formatter.GetCleanChangeBlocks(c.Changes).Count),0:0.00}");


                        Console.WriteLine("-------------Average Char Length-----------");
                        foreach (var division in divisions)
                            Console.WriteLine($"{division.Key}, {division.Value.Average(c => Formatter.GetCleanChangeBlocks(c.Changes).Sum(b => b.Length)),0:0.00}");


                        Console.WriteLine("-------------Average Lines Length-----------");
                        foreach (var division in divisions)
                            Console.WriteLine($"{division.Key}, {division.Value.Average(c => Formatter.GetCleanChangeBlocks(c.Changes).Sum(b => b.Split("\n").Length)),0:0.00}");

                        //Console.WriteLine("-------------Line Lengths-----------");
                        // foreach (var division in divisions)
                        //     Console.WriteLine($"{division.Key}, {string.Join(",",division.Value.Select(c => c.Changes.Split("\n").Length).Order())}");

                        Console.WriteLine("-------------Average Cyclomatic Complexity-----------");
                        foreach (var division in divisions)
                            Console.WriteLine($"{division.Key}, {division.Value.Average(c => CyclomaticComplexity(Formatter.GetCleanChangeBlocks(c.Changes))), 0:0.00}");

                        Console.WriteLine("-------------Cyclomatic Complexities-----------");
                        foreach (var division in divisions)
                            Console.WriteLine($"{division.Key}, {string.Join(",",division.Value.Select(c => CyclomaticComplexity(Formatter.GetCleanChangeBlocks(c.Changes))).Where(c => c > 1).Order())}");


                        // Console.WriteLine("-------------Line Length Buckets-----------");
                        //double[][] lengthRanges = [[0,300],[300,400],[400,450],[450,500], [500, int.MaxValue]];
                        //Console.WriteLine($"range, {string.Join(",", divisions.Select(d => d.Key))}");
                        //foreach (var range in lengthRanges)
                        //{
                        //    Console.WriteLine($"[{range[0]}...{range[1]}],{
                        //        string.Join(",", divisions.Select(d => d.Value.Where(c => {
                        //            double bv = bucketValue(c);
                        //            return range[0] <= bv && bv < range[1];
                        //        }).Count()))}");

                        //    double bucketValue(Commit c) => Formatter.GetCleanChangeBlocks(c.Changes).Sum(b => b.Split("\n").Length);

                        //}

                        Console.WriteLine("-------------Cyclomatic Complexity Buckets-----------");
                        double[][] lengthRanges = [[1, 2], [2, 3], [3, 5],[5, 10], [10, int.MaxValue]];
                        Console.WriteLine($"range, {string.Join(",", divisions.Select(d => d.Key))}");
                        foreach (var range in lengthRanges)
                        {
                            Console.WriteLine($"[{range[0]}...{range[1]}],{string.Join(",", divisions.Select(d => d.Value.Where(c =>
                            {
                                double bv = bucketValue(c);
                                return range[0] <= bv && bv < range[1];
                            }).Count()))}");

                            double bucketValue(Commit c) => CyclomaticComplexity(Formatter.GetCleanChangeBlocks(c.Changes));

                        }

                        */
        }


        public static int CyclomaticComplexity(List<string> changeBlocks)
        {
            for (int i = 0; i < changeBlocks.Count; i++)
            {
                if (changeBlocks[i].Contains("//")) changeBlocks[i] = changeBlocks[i][..changeBlocks[i].IndexOf("//")];
            }
            //Console.WriteLine("----------------------------------------------------");
            //foreach (var code in changeBlocks)
            //{
            //    Console.WriteLine("vvvvvvvvvvvvvvvvvvvvvvvv");
            //    Vizualizer.PrintChanges(code);
            //    Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^");
            //}
            //Console.WriteLine("----------------------------------------------------");


            int decisionDiamonds = changeBlocks.Sum(block => Regex.Matches(block, @" if( |\()").Count);
            return decisionDiamonds + 1;
        }
        public static void RandomLabel(Commit[] commits)
        {
            foreach(Commit commit in commits)
            {
                commit.Analysis = new()
                {
                    PredictsBackported = Random.Shared.NextDouble() < 0.5
                };
            }
        }
        public static void PrintCounts(Commit[] dataset, string name)
        {
            foreach (var commit in dataset)
            {
                foreach (var anotherCommit in dataset)
                {
                    if (!ReferenceEquals(commit, anotherCommit) && commit.SHA == anotherCommit.SHA) throw new Exception("Duplicates!");
                }
            }

            Console.WriteLine($"---------------------------------------------");
            Console.WriteLine(name);
            Console.WriteLine($"There are {dataset.Count()} commits.");
            float backported = dataset.Where(c => c.IsBackported).Count();
            float notbackported = dataset.Where(c => !c.IsBackported).Count();
            Console.WriteLine($"[Actual]There are {backported} backported commits.");
            Console.WriteLine($"[Actual]There are {notbackported} not backported commits."); 
            Console.WriteLine($"[Actual]{backported / (backported + notbackported) * 100:0.000}% of the commits are backported.");
            Console.WriteLine($"[Actual]{notbackported / (backported + notbackported) * 100:0.000}% of the commits arent backported.");

            if (dataset.All(c => c.Analysis != null))
            {
                float Pbackported = dataset.Where(c => c.Analysis.PredictsBackported).Count();
                float Pnotbackported = dataset.Where(c => !c.Analysis.PredictsBackported).Count();
                Console.WriteLine($"[Predicted]There are {Pbackported} backported commits.");
                Console.WriteLine($"[Predicted]There are {Pnotbackported} not backported commits.");
                Console.WriteLine($"[Predicted]{Pbackported / (backported + notbackported) * 100:0.000}% of the commits are backported.");
                Console.WriteLine($"[Predicted]{dataset.Where(c => c.Analysis.PredictsBackported && c.IsBackported).Count() / Pbackported * 100:0.000}% of those are right.");
                Console.WriteLine($"[Predicted]{Pnotbackported / (backported + notbackported) * 100:0.000}% of the commits arent backported.");
                Console.WriteLine($"[Predicted]{dataset.Where(c => !c.Analysis.PredictsBackported && !c.IsBackported).Count() / Pnotbackported * 100:0.000}% of those are right.");
            }
        }

        public static void PrintAnalysis(Commit[] commits, string model, string experiment, int trainingSize)
        {
            PrintCounts(commits, model + "@[" + trainingSize + "]: " + experiment);
            double total = commits.Length;
            int correct = commits.Where(c => c.IsBackported == c.Analysis.PredictsBackported).Count();

            int BackportedTP = commits.Where(c => c.IsBackported && c.Analysis.PredictsBackported).Count();
            int BackportedFP = commits.Where(c => !c.IsBackported && c.Analysis.PredictsBackported).Count();
            int BackportedFN = commits.Where(c => c.IsBackported && !c.Analysis.PredictsBackported).Count();


            int NotTP = commits.Where(c => !c.IsBackported && !c.Analysis.PredictsBackported).Count();
            int NotFP = commits.Where(c => c.IsBackported && !c.Analysis.PredictsBackported).Count();
            int NotFN = commits.Where(c => !c.IsBackported && c.Analysis.PredictsBackported).Count();

            Console.WriteLine("----------------------Counts-----------------------");
            Console.WriteLine($"{correct} commits are correctly labeled.");
            Console.WriteLine($"{(float)correct / commits.Length * 100.0}% of the commits are correctly labeled.");

            Console.WriteLine("---------------------Analysis----------------------");
            Console.WriteLine($"Backported TP: {BackportedTP / total}");
            Console.WriteLine($"Backported FP: {BackportedFP / total}");
            Console.WriteLine($"Backported FN: {BackportedFN / total}");
            Console.WriteLine($"NOT Backported TP: {NotTP / total}");
            Console.WriteLine($"NOT Backported FP: {NotFP / total}");
            Console.WriteLine($"NOT Backported FN: {NotFN / total}");

            Console.WriteLine($"--------------------Metrics------------------------");
            var backportedF1Metrics = AnalysisCalculators.ComputeF1OnClass(BackportedTP, BackportedFP, BackportedFN);
            var NOTbackportedF1Metrics = AnalysisCalculators.ComputeF1OnClass(NotTP, NotFP, NotFN);
            var microavg = ComputeMicroF1(BackportedTP, BackportedFP, BackportedFN, NotTP, NotFP, NotFN);
            var macroavg = ComputeMacroF1(backportedF1Metrics, NOTbackportedF1Metrics);

            const int sep = -20;
            Console.WriteLine($"{"",sep}{"Precision",sep}{"Recall",sep}{"F1",sep}");
            Console.WriteLine($"{"Backported",sep}{backportedF1Metrics.Precision,sep}{backportedF1Metrics.Recall,sep}{backportedF1Metrics.F1,sep}");
            Console.WriteLine($"{"Not Backported",sep}{NOTbackportedF1Metrics.Precision,sep}{NOTbackportedF1Metrics.Recall,sep}{NOTbackportedF1Metrics.F1,sep}");
            Console.WriteLine($"{"Micro-average",sep}{microavg.Precision,sep}{microavg.Recall,sep}{microavg.F1,sep}");
            Console.WriteLine($"{"Macro-average",sep}{macroavg.Precision,sep}{macroavg.Recall,sep}{macroavg.F1,sep}");

            double kappa = AnalysisCalculators.ComputeCohenKappa(
                commits.Where(c => c.Analysis?.PredictsBackported != null).Select(c => c.IsBackported ? "A" : "B").ToList(),
                commits.Where(c => c.Analysis?.PredictsBackported != null).Select(c => c.Analysis.PredictsBackported ? "A" : "B").ToList());

            Console.WriteLine($"Cohen Kappa Coefficient: {kappa}");

            //["Experiment,Model,Training Size,Evaluation Size,kappa,microPrecision,microRecall,microF1,macroPrecision,macroRecall,macroF1,BackTP,BackFP,BackFN,NotBackTP,NotBackFP,NotBackFN"];
            csv.Add($"{experiment},{model},{(trainingSize == -1 ? "N/A" : trainingSize)},{total},{kappa},{microavg.Precision},{microavg.Recall},{microavg.F1},{macroavg.Precision},{macroavg.Recall},{macroavg.F1},{BackportedTP},{BackportedFP},{BackportedFN},{NotTP},{NotFP},{NotFN}");
            Console.WriteLine();
        }

        public static void PrintAnalysisIncorrectly(Commit[] commits, string name)
        {
            throw new Exception();
            List<Commit> labeled = commits.Where(c => c.Analysis != null).ToList();
            List<Commit> correctlyLabeled = labeled.Where(c => c.IsBackported == c.Analysis.PredictsBackported).ToList();

            List<Commit> backported = commits.Where(c => c.IsBackported).ToList();
            List<Commit> labeledBackported = labeled.Where(c =>  c.Analysis?.PredictsBackported ?? false).ToList();
            int TP = labeledBackported.Where(c => c.IsBackported).Count();
            int FP = labeledBackported.Where(c => !c.IsBackported).Count();


            List<Commit> notBackported = commits.Where(c => !c.IsBackported).ToList();
            List<Commit> labeledNotBackported = labeled.Where(c => c.Analysis != null && !c.Analysis.PredictsBackported).ToList();
            int TN = labeledNotBackported.Where(c => !c.IsBackported).Count();
            int FN = labeledNotBackported.Where(c => c.IsBackported).Count();


            PrintCounts(commits, "BAD BAD BAD STOP");
            Console.WriteLine("----------------------Counts-----------------------");
            Console.WriteLine($"{labeled.Count} commits are labeled.");
            Console.WriteLine($"{correctlyLabeled.Count} commits are correctly labeled.");
            Console.WriteLine($"{(float)correctlyLabeled.Count / labeled.Count * 100.0}% of the commits are correctly labeled.");

            //false positive: llm thinks the commit should be backported when it shouldn't be

            Console.WriteLine("----------------------Positive-----------------------");
            Console.WriteLine($"False positive count: {FP}");
            Console.WriteLine($"True positive count: {TP}");

            Console.WriteLine("----------------------Negative-----------------------");
            Console.WriteLine($"False negative count: {FN}");
            Console.WriteLine($"True negative count: {TN}");
            Console.WriteLine($"Total: {FP + TP + FN + TN}");

            Console.WriteLine("----------------------Agreement-----------------------");
            double kappa = AnalysisCalculators.ComputeCohenKappa(
                commits.Where(c => c.Analysis?.PredictsBackported != null).Select(c => c.IsBackported ? "A" : "B").ToList(),
                commits.Where(c => c.Analysis?.PredictsBackported != null).Select(c => c.Analysis.PredictsBackported ? "A" : "B").ToList());

            AnalysisCalculators.F1Result f1 = AnalysisCalculators.ComputeF1OnClass(TP, FP, FN);

            Console.WriteLine($"Cohen Kappa Coefficient: {kappa}");
            Console.WriteLine($"Precision: {f1.Precision}");
            Console.WriteLine($"Recall: {f1.Recall}");
            Console.WriteLine($"F1: {f1.F1}");

            Console.WriteLine($"");

            csv.Add($"{name},{TP},{FP},{TN},{FN},{kappa},{f1.F1}");
        }

        
    }
}
