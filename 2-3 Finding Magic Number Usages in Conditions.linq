<Query Kind="Statements">
  <Reference Relative="lib\Microsoft.CodeAnalysis.CSharp.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data-new\lib\Microsoft.CodeAnalysis.CSharp.dll</Reference>
  <Reference Relative="lib\Microsoft.CodeAnalysis.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data-new\lib\Microsoft.CodeAnalysis.dll</Reference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
</Query>

var code =
@"
class PasswordManager
{
    int x = 8;
    
    bool IsGood(string password)
    {
        if(password.Length < 5)
            return false;
        return password.Length >= 7;
    }
    
    int fun()
    {
        return g[x];
    }
    
    bool zun()
    {
        if(z > 3.4)
            return false;
        else
            return true;
    }

    bool zun2()
    {
        if(z > x)
            return false;
        else
            return true;
    }
}";

var conditionalExpressions = new []
{
    SyntaxKind.LessThanExpression,
    SyntaxKind.LessThanOrEqualExpression,
    SyntaxKind.EqualsExpression,
    SyntaxKind.GreaterThanExpression,
    SyntaxKind.GreaterThanOrEqualExpression
};

var root = CSharpSyntaxTree.ParseText(code).GetRoot();

root
    .DescendantNodes()
    .Where(node => conditionalExpressions.Contains(node.Kind()))
    .Where(node => node.DescendantNodes().Any(descendantNode => descendantNode.IsKind(SyntaxKind.NumericLiteralExpression)))
    .Select(node => new
        {
            Class = node.Ancestors().OfType<ClassDeclarationSyntax>().First().Identifier.ValueText,
            Method = node.Ancestors().OfType<MethodDeclarationSyntax>().First().Identifier.ValueText,
            MagicLines = node.ToString()
        })
    .Dump();