using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StickyKeyboard
{
    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Solution
    {
        static void Main(string[] args)
        {

            int N = int.Parse(Console.ReadLine());
            var lines = new List<string>();
            var wordLines = new List<string[]>();
            for (int i = 0; i < N; i++)
            {
                var line = Console.ReadLine();
                lines.Add(line);
                wordLines.Add(line.Split(" "));
            }

            //create vocabulary
            var vocabularyHistogram = GetHistogram(lines);
            foreach (var referenceW in vocabularyHistogram)
            {
                //if either half is on a high count word, take the high count word if +1 or -1 length to it
                foreach (var evaluatedW in vocabularyHistogram)
                {
                    if ((evaluatedW.Key == "did" && referenceW.Key == "dd") || (evaluatedW.Key == "dd" && referenceW.Key == "did"))
                    {
                        var o = 0;
                    }

                    var anal = IsPartner(referenceW.Key, evaluatedW.Key);
                    if (anal != Result.noPartner)
                    {
                        if (evaluatedW.Value == referenceW.Value) continue;

                        var larger = referenceW.Key.Length > evaluatedW.Key.Length ? referenceW : evaluatedW;
                        var smaller = referenceW.Key.Length > evaluatedW.Key.Length ? evaluatedW : referenceW;
                        var replacement = "";
                        var badWord = "";
                        //if missed letter, then smaller word is the one that should be replaced
                        if (larger.Value > smaller.Value && anal == Result.missedLetter)
                        {
                            replacement = larger.Key;
                            badWord = smaller.Key;
                        }else if (smaller.Value > larger.Value && anal == Result.repeatedLetter)
                        {
                            replacement = smaller.Key;
                            badWord = larger.Key;
                        }
                        else
                        {
                            //Shouldn't replace anything
                            continue;
                        }

                        //if did not print, then smaller word is the one that should be replaced

                        //take the word with higher count
                        var i = 0;
                        foreach (var words in wordLines)
                        {
                            for (var x = 0 ; x < words.Length ; x++)
                            {
                                var tempWord = words[x];
                                var word = tempWord;
                                //last char a punctuation
                                var hasPunctuationEnd = !IsCharInRage(word[word.Length - 1]);
                                var hasPunctuationStart = !IsCharInRage(word[0]);
                                if (hasPunctuationEnd)
                                {
                                    word = word.Remove(word.Length - 1, 1);
                                }
                                if (hasPunctuationStart)
                                {
                                    word = word.Remove(0,1);
                                }

                                if (word == badWord)
                                {
                                    var newW = hasPunctuationEnd ? replacement + tempWord[tempWord.Length - 1] : replacement;
                                    newW = hasPunctuationStart ? tempWord[0] + newW : newW;
                                    words[x] = newW;
                                }
                            }
                            i++;
                        }
                    }
                }
            }

            foreach (var fixedLine in wordLines)
            {
                var finalLine = "";
                foreach (var word in fixedLine)
                {
                    finalLine += word + " ";
                }
                //Console.WriteLine($"<{finalLine.TrimEnd()}>");
                Console.WriteLine($"{finalLine.TrimEnd()}");
            }
        }

        public static Dictionary<string, int> GetHistogram(List<string> lines)
        {
            var vocabularyHistogram = new Dictionary<string, int>();
            for (var l = 0; l < lines.Count(); l++)
            {
                var line = lines[l];
                for (var c = 0; c < line.Count(); c++)
                {
                    if (!IsCharInRage(line[c]))
                    {
                        //Console.WriteLine($"Replacing: <{line[c]}>");
                        line = line.Replace(line[c], ' ');

                    }
                }

                var words = line.Split(" ");
                foreach (var w in words)
                {
                    if (string.IsNullOrEmpty(w)) continue;
                    if (vocabularyHistogram.ContainsKey(w))
                    {
                        vocabularyHistogram[w]++;
                    }
                    else
                    {
                        vocabularyHistogram.Add(w, 1);
                    }
                }
            }

            //Console.WriteLine($"Histogram");
            foreach (var w in vocabularyHistogram)
            {
                //Console.WriteLine($"{w.Key}:{w.Value} \n");
            }

            return vocabularyHistogram;
        }

        public enum Result
        {
            noPartner,
            repeatedLetter,
            missedLetter
        }
        public static Result IsPartner(string refW, string evalW)
        {
            if (Math.Abs(refW.Length - evalW.Length) != 1) return Result.noPartner;


            //check if extra letter
            var pattern = @"(.)\1";
            var rg = new Regex(pattern);
            if (rg.IsMatch(evalW) && evalW.Length > refW.Length)
            {
                var parts = rg.Split(evalW);
                if (parts.Length > 0)
                {
                    foreach (var part in parts)
                    {
                        if (!refW.Contains(part)) return Result.noPartner;
                    }

                    return Result.repeatedLetter;
                }
            }
            else
            {
                //possible missing letter, brute force it
                for (var i = 0; i < refW.Length; i++)
                {
                    var newW = refW.Remove(i, 1);
                    if (newW == evalW) return Result.missedLetter;
                }
            }

            return Result.noPartner;
        }

        public static bool IsCharInRage(char c)
        {
            var lowerCaseStart = (int)'a';
            var lowerCaseEnd = (int)'z';
            var upperCaseStart = (int)'A';
            var upperCaseEnd = (int)'Z';
            var ignore = (int)' ';
            return (int)c >= lowerCaseStart && (int)c <= lowerCaseEnd ||
                   (int)c >= upperCaseStart && (int)c <= upperCaseEnd ||
                   (int)c == ignore;
        }
    }
}
