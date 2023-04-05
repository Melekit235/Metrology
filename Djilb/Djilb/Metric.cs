using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Djilb
{
    public class Metric
    {
        static bool ContainsWholeWord(string line, string word)
        {
            return line.Split(new[] {' ', '\t'}).Any(s => s == word);
        }

        public static int LogicalComplexity(string text)
        {
            int conditionalCount = 0;
            int loopCount = 0;
            int matchCount = 0;
            string pattern = @"match\s+.+\s+with\s+(?=\|)|\s*\|";
            string[] lines = text.Split('\n');

            foreach (string line in lines)
            {
                if (ContainsWholeWord(line, "if") ||  ContainsWholeWord(line, "elif"))
                {
                    conditionalCount++;
                }
                else if (ContainsWholeWord(line, "while") || ContainsWholeWord(line, "for"))
                {
                    loopCount++;
                }
                else if (ContainsWholeWord(line, "match"))
                {
                    matchCount++;
                }
            }
        
            Regex regex = new Regex(pattern);
            int matchBranches = regex.Matches(text).Count;
            matchBranches -= matchCount * 2;
            int absoluteComplexity = conditionalCount + loopCount + matchBranches;
            return absoluteComplexity;
        }
        
        public static float RelativeComplexity(string text, int LC)
        {
            string[] lines = text.Split('\n');

            Regex operatorsRegex = new Regex(@"[\s]*((let|yield|return|use|try|function|type|mutable|val|rec|and)|([\+\-\*/><=!&\|]+))\b");
        
            MatchCollection matches = operatorsRegex.Matches(text);
            int operatorsCount = matches.Count;
        
            float result = (float)LC / (operatorsCount + LC); 
            return result;
        }
        
        public static int CalculateMaxTabDepth(string text)
        {
            int maxNesting = 0;
            int nestingLevel = 0;

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    nestingLevel++;
                }
                else
                {
                    maxNesting = Math.Max(maxNesting, nestingLevel);
                    nestingLevel = 0;
                }
            }

            int matchCount = 0;
            string[] lines = text.Split('\n');
            
            foreach (string line in lines)
            {
                if (ContainsWholeWord(line, "match"))
                {
                    matchCount++;
                }
            }
            string pattern = @"match\s+.+\s+with\s+(?=\|)|\s*\|";
            Regex regex = new Regex(pattern);
            int matchBranches = regex.Matches(text).Count;
            matchBranches -= matchCount * 2;

            return maxNesting/4;
        }
    }
}