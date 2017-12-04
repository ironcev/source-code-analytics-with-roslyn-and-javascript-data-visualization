<Query Kind="Program">
  <Reference Relative="lib\Microsoft.CodeAnalysis.CSharp.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data-new\lib\Microsoft.CodeAnalysis.CSharp.dll</Reference>
  <Reference Relative="lib\Microsoft.CodeAnalysis.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data-new\lib\Microsoft.CodeAnalysis.dll</Reference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
</Query>

// We are looking only for the "top level" fragmented conditions.
// Top level means we will not look into nested structures, but only on those IF statements that are directly in the method body.
// Two IF conditions are not fragmented if there are other statements between them.
// The algorithm has to work if the body of the IF statements are semantically equivalent, even if they are not "structurally identical".

void Main()
{
    var code = 
    @"
    public class A
    {
        int maybe_do_something(...)
        {
            var a = 0;
            
            if(something != -1)
                return 0;
            if(somethingelse != -1)
                    return   0;
            if(etc != -1) return 0 ;
            if(etc != -2) { return 0  ; }
            if(etc != -3) { return 1 ; }
            if(etc != -4) { return 1 ; }
            if(etc != -5) { return 1  ; }
    
            var x = 0;
            if(etc != 1) return 0;
            if(etc != 2) return 0;
            if(etc != 3) return  1;
    
            do_something();
        }
        
        int otherFun()
        {
            int w = 1;
        
            if(bailIfIEqualZero == 0)
                return;
            if(string.IsNullOrEmpty(shouldNeverBeEmpty))
                return;
                
            var x = 0;
    
            if(betterNotBeNull == null || betterNotBeNull.RunAwayIfTrue)
                return;
    
            return 1;
        }
        
        void Empty()
        {
        }
    }";
    
    
    var root = CSharpSyntaxTree.ParseText(code).GetRoot();
    
    foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
    {
        var subsequentIfStatements = new List<List<IfStatementSyntax>>();
        List<IfStatementSyntax> currentIfStatements = new List<IfStatementSyntax>(); // To simplify for null checks.
        subsequentIfStatements.Add(currentIfStatements);
     
        bool isPreviousStatementIfStatement = false;
        var enumerator = method.Body.Statements.GetEnumerator(); 
        while (enumerator.MoveNext())
        {
            bool isCurrentStatementIfStatement = enumerator.Current is IfStatementSyntax;
            if (!isCurrentStatementIfStatement)
            {
                isPreviousStatementIfStatement = false;
                continue;
            }
            
            if (isCurrentStatementIfStatement && !isPreviousStatementIfStatement)
            {            
                currentIfStatements = new List<IfStatementSyntax>();
                subsequentIfStatements.Add(currentIfStatements);
            }
            
            // The current statement is an if statement and we are adding it either to a new or
            // an existing group of if statements.
            currentIfStatements.Add((IfStatementSyntax)enumerator.Current);
            isPreviousStatementIfStatement = true;        
        }
        
        // Do not display those that do not have if statements.
        if (!subsequentIfStatements.Any(statements => statements.Any())) continue;
        
        
        
        new
        {
            MethodName = method.Identifier.ValueText,
            FragmentedIfStatements = subsequentIfStatements
                .Where(statements => statements.Any()) // Remove the dummy empty which is always there (add to avoid null checks).
                .Select(statements => statements.GroupBy(node => node, EqualityComparer.Default))
                .SelectMany(statements => statements)
                .Select(group => new
                {
                    IfStatement = group.Key.Statement.ToString(),
                    IfConditions = group.Select(ifStatement => ifStatement.Condition.ToString()),
                    AsSingleCondition = $"if ({string.Join(" || ", group.Select(ifStatement => $"({ifStatement.Condition.ToString()})"))}) {group.Key.Statement.ToString()}"
                })
                .Where(result => result.IfConditions.Count() > 1)
        }
        .Dump();
    }    
}

class EqualityComparer : IEqualityComparer<IfStatementSyntax>
{
    public static readonly EqualityComparer Default = new EqualityComparer();

    public bool Equals(IfStatementSyntax first, IfStatementSyntax second)
    {
        var firstBlock = first.Statement as BlockSyntax;
        var secondBlock = second.Statement as BlockSyntax;
    
        if (firstBlock != null && secondBlock != null)
            return firstBlock.IsEquivalentTo(secondBlock, false);
            
        if (firstBlock != null) // The second If has just a single statement.
        {
            if (firstBlock.Statements.Count != 1) return false;
            
            return firstBlock.Statements[0].IsEquivalentTo(second.Statement, false);
        }
        
        if (secondBlock != null) // The first If has just a single statement.
        {
            if (secondBlock.Statements.Count != 1) return false;
            
            return secondBlock.Statements[0].IsEquivalentTo(first.Statement, false);
        }
        
        // Both If-s are just single statements.
        return first.Statement.IsEquivalentTo(second.Statement, false);
    }
    
    public int GetHashCode(IfStatementSyntax node)
    {
        return 0; // We want Equals() to be always called.
    }
}