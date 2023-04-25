using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FSharp.Compiler.CodeAnalysis;
using FSharp.Compiler.Syntax;
using FSharp.Compiler.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace Djilb
{
    public class Metric
    {
        static bool ContainsWholeWord(string line, string word)
        {
            return line.Split(new[] { ' ', '\t' }).Any(s => s == word);
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
                if (ContainsWholeWord(line, "if") || ContainsWholeWord(line, "elif"))
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

            Regex operatorsRegex =
                new Regex(@"[\s]*((let|yield|return|use|try|function|type|mutable|val|rec|and)|([\+\-\*/><=!&\|]+))\b");

            MatchCollection matches = operatorsRegex.Matches(text);
            int operatorsCount = matches.Count;

            float result = (float)LC / (operatorsCount + LC);
            return result;
        }

        /*public static int CalculateMaxTabDepth(string text) {
            int maxNesting = 0;
            int nestingLevel = 0;

            foreach (char c in text) {
                if (c == ' ') {
                    nestingLevel++;
                }
                else {
                    maxNesting = Math.Max(maxNesting, nestingLevel);
                    nestingLevel = 0;
                }
            }

            int matchCount = 0;
            string[] lines = text.Split('\n');
            
            foreach (string line in lines) {
                if (ContainsWholeWord(line, "match")) {
                    matchCount++;
                }
            }
            string pattern = @"match\s+.+\s+with\s+(?=\|)|\s*\|";
            Regex regex = new Regex(pattern);
            int matchBranches = regex.Matches(text).Count;
            matchBranches -= matchCount * 2;

            return maxNesting/4;
        }*/





        



    }

    public class TreeNode
    {
        public TreeNode left;
        public TreeNode right;

        public TreeNode()
        {
            left = null;
            right = null;
        }
    }
    public class Pr : TreeNode
    {

        public static int CalculateMaxConditionalDepth(string filePath)
        {
            ISourceText input = SourceText.ofString(File.ReadAllText(filePath));
            FSharpChecker checker = FSharpChecker.Create(
                FSharpOption<int>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                null,
                null,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<bool>.None,
                FSharpOption<DocumentSource>.None,
                FSharpOption<bool>.None
            );
            var projectTupleAsync = checker.GetProjectOptionsFromScript(filePath, input, FSharpOption<bool>.None,
                FSharpOption<DateTime>.None, FSharpOption<string[]>.None, FSharpOption<bool>.None,
                FSharpOption<bool>.None, FSharpOption<bool>.None, FSharpOption<string>.None, FSharpOption<long>.None,
                FSharpOption<string>.None);
            var projectTuple = FSharpAsync.RunSynchronously(projectTupleAsync, FSharpOption<int>.None,
                FSharpOption<CancellationToken>.None);
            var projectOptions = projectTuple.Item1;
            var parsingTuple = checker.GetParsingOptionsFromProjectOptions(projectOptions);
            var parsingOptions = parsingTuple.Item1;
            var parsedFileResultAsync = checker.ParseFile(filePath, input, parsingOptions, FSharpOption<bool>.None,
                FSharpOption<string>.None);
            var parsedFileResult = FSharpAsync.RunSynchronously(parsedFileResultAsync, FSharpOption<int>.None,
                FSharpOption<CancellationToken>.None);
            var parseTree = parsedFileResult.ParseTree;
            var treeRoot = ((ParsedInput.ImplFile)parseTree).Item;
            var content = treeRoot.contents.Head.decls;

            int depth = 0;
            foreach (var decl in content)
            {
                if (decl.IsLet)
                {
                    var bindings = ((SynModuleDecl.Let)decl).bindings;
                    foreach (var binding in bindings)
                    {
                        TreeNode root = new TreeNode();
                        CreateConditionalTree(binding.expr, root);
                        depth = Math.Max(MaxDepth(root) - 1, depth);
                    }
                }
            }

            return depth;
        }
        


        static List<TreeNode> SequentialExpr(SynExpr expr, List<TreeNode> forest, TreeNode currentNode)
        {
            
            var sequentialExpr = (SynExpr.Sequential)expr;
            if (sequentialExpr.expr1.IsMatch || sequentialExpr.expr1.IsIfThenElse || sequentialExpr.expr1.IsForEach || sequentialExpr.expr1.IsWhile)
            {
                var expr1 = sequentialExpr.expr1;
                TreeNode tempNode = new TreeNode();
                CreateConditionalTree(expr1, tempNode);
                forest.Add(tempNode);
            }
            else if (sequentialExpr.expr1.IsSequential)
            {
                forest = SequentialExpr(sequentialExpr.expr1, forest, currentNode);
            }
            else if (sequentialExpr.expr1.IsLetOrUse)
            {
                CreateConditionalTree(sequentialExpr.expr1, currentNode);
            }
            if (sequentialExpr.expr2.IsMatch || sequentialExpr.expr2.IsIfThenElse || sequentialExpr.expr2.IsForEach || sequentialExpr.expr2.IsWhile)
            {
                var expr2 = sequentialExpr.expr2;
                TreeNode tempNode = new TreeNode();
                CreateConditionalTree(expr2, tempNode);
                forest.Add(tempNode);
            }
            else if (sequentialExpr.expr2.IsSequential)
            {
                forest = SequentialExpr(sequentialExpr.expr2, forest, currentNode);
            }
            else if (sequentialExpr.expr2.IsLetOrUse)
            {
                CreateConditionalTree(sequentialExpr.expr2, currentNode);
            }
            
            return forest;
        } 


        static void CreateConditionalTree(SynExpr expr, TreeNode currentNode)
        {
            if (expr.IsLetOrUse)
            {
                var letOrUseExpr = (SynExpr.LetOrUse)expr;
                CreateConditionalTree(letOrUseExpr.body, currentNode);
            }
            else  if (expr.IsSequential)
            {
                List<TreeNode> forest = new List<TreeNode>();
                forest = SequentialExpr(expr, forest, currentNode);
                
                int j = 0;
                int maxDepth = 0;
                for (int i = 0; i < forest.Count; i++)
                {
                    int depth = MaxDepth(forest[i]);
                    if (depth > maxDepth)
                    {
                        maxDepth = depth;
                        j = i;
                    }
                }
                if (forest.Count != 0) 
                    currentNode.right = forest[j];
            }
            else if (expr.IsMatch)
            {
                var matchExpr = (SynExpr.Match)expr;
                CreateConditionalTreeMatch(matchExpr.clauses, matchExpr.clauses.Length, currentNode);
            } 
            else if (expr.IsIfThenElse)
            {
                var ifExpr = (SynExpr.IfThenElse)expr;
                if (ifExpr.elseExpr != null && (ifExpr.elseExpr.Value.IsIfThenElse || ifExpr.elseExpr.Value.IsForEach || ifExpr.elseExpr.Value.IsMatch || ifExpr.elseExpr.Value.IsWhile || ifExpr.elseExpr.Value.IsSequential || ifExpr.elseExpr.Value.IsLetOrUse))
                {
                    if (ifExpr.elseExpr.Value.IsIfThenElse || ifExpr.elseExpr.Value.IsMatch || ifExpr.elseExpr.Value.IsForEach || ifExpr.elseExpr.Value.IsWhile || ifExpr.elseExpr.Value.IsLetOrUse && !((SynExpr.LetOrUse)ifExpr.elseExpr.Value).body.IsSequential)
                    {
                        TreeNode leftNode = new TreeNode();
                        currentNode.left = leftNode;
                        CreateConditionalTree(ifExpr.elseExpr.Value, leftNode);
                    }
                    else
                    {
                        CreateConditionalTree(ifExpr.elseExpr.Value, currentNode);
                    }
                }
                if (ifExpr.thenExpr.IsIfThenElse || ifExpr.thenExpr.IsForEach || ifExpr.thenExpr.IsWhile || ifExpr.thenExpr.IsMatch || ifExpr.thenExpr.IsSequential)
                {
                    TreeNode rightNode = new TreeNode();
                    currentNode.right = rightNode;
                    CreateConditionalTree(ifExpr.thenExpr, rightNode);
                }

            } 
            else if (expr.IsForEach)
            {
                var forEachExpr = (SynExpr.ForEach)expr;
                if (forEachExpr.bodyExpr.IsForEach || forEachExpr.bodyExpr.IsIfThenElse || forEachExpr.bodyExpr.IsWhile || forEachExpr.bodyExpr.IsMatch || (forEachExpr.bodyExpr.IsLetOrUse && !((SynExpr.LetOrUse)forEachExpr.bodyExpr).body.IsSequential))
                {
                    TreeNode rightNode = new TreeNode();
                    currentNode.right = rightNode;
                    CreateConditionalTree(forEachExpr.bodyExpr, rightNode);
                }
                else if (forEachExpr.bodyExpr.IsSequential)
                {
                    CreateConditionalTree(forEachExpr.bodyExpr, currentNode);
                }
            }
            else if (expr.IsWhile)
            {
                var whileExpr = (SynExpr.While)expr;
                if (whileExpr.doExpr.IsWhile || whileExpr.doExpr.IsForEach || whileExpr.doExpr.IsMatch || whileExpr.doExpr.IsIfThenElse || whileExpr.doExpr.IsLetOrUse && !((SynExpr.LetOrUse)whileExpr.doExpr).body.IsSequential)
                {
                    TreeNode rightNode = new TreeNode();
                    currentNode.right = rightNode;
                    CreateConditionalTree(whileExpr.doExpr, rightNode);
                }
                else if (whileExpr.doExpr.IsSequential)
                {
                    CreateConditionalTree(whileExpr.doExpr, currentNode);
                }
            }
        }
        
        static void CreateConditionalTreeMatch(FSharpList<SynMatchClause> clauses, int length, TreeNode currentNode)
        {
            if (clauses.Length == 0) 
                return;SynMatchClause currentClause = clauses[0];
            clauses = clauses.Tail;
            if (currentClause.pat.IsWild && !(currentClause.resultExpr.IsIfThenElse || currentClause.resultExpr.IsForEach || currentClause.resultExpr.IsWhile || currentClause.resultExpr.IsMatch || currentClause.resultExpr.IsSequential || currentClause.resultExpr.IsLetOrUse))
                return;
            
            TreeNode leftNode = new TreeNode();
            if (clauses.Length == length - 1 || currentClause.pat.IsWild)
            {
                leftNode = currentNode;
            }
            else
            {
                currentNode.left = leftNode;
                currentNode = leftNode;
            }
            

            if (currentClause.resultExpr.IsMatch)
            {
                if (!currentClause.pat.IsWild)
                {
                    TreeNode rightNode = new TreeNode();
                    currentNode.right = rightNode;
                    CreateConditionalTreeMatch(((SynExpr.Match)currentClause.resultExpr).clauses, ((SynExpr.Match)currentClause.resultExpr).clauses.Length, rightNode);
                }
                else
                {
                    CreateConditionalTreeMatch(((SynExpr.Match)currentClause.resultExpr).clauses, ((SynExpr.Match)currentClause.resultExpr).clauses.Length, leftNode);
                }
            }
            else if (currentClause.resultExpr.IsIfThenElse || currentClause.resultExpr.IsForEach || currentClause.resultExpr.IsWhile || currentClause.resultExpr.IsSequential || currentClause.resultExpr.IsLetOrUse && !((SynExpr.LetOrUse)currentClause.resultExpr).body.IsSequential)
            {
                var expr = currentClause.resultExpr;
                if (!currentClause.pat.IsWild || !currentClause.resultExpr.IsSequential)
                {
                    TreeNode rightNode = new TreeNode();
                    currentNode.right = rightNode;
                    CreateConditionalTree(expr, rightNode);
                }
                else 
                {
                    CreateConditionalTree(expr, leftNode);
                }

            }

            if (clauses.Length > 0)
            {
                CreateConditionalTreeMatch(clauses, length, leftNode);
            }
        }
        
        static int MaxDepth(TreeNode node)
        {
            if (node == null)
            {
                return 0;
            }
    
            int leftDepth = MaxDepth(node.left);
            int rightDepth = MaxDepth(node.right);
    
            return Math.Max(leftDepth, rightDepth) + 1;
        }
    }
}
