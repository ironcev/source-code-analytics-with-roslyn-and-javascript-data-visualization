<Query Kind="Statements">
  <Reference Relative="lib\Microsoft.CodeAnalysis.CSharp.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data\lib\Microsoft.CodeAnalysis.CSharp.dll</Reference>
  <Reference Relative="lib\Microsoft.CodeAnalysis.dll">C:\p\source-code-analytics-with-roslyn-and-javascript-data\lib\Microsoft.CodeAnalysis.dll</Reference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
</Query>

var code =
@"
float fun(int g)
{
    int size = 10000;
    g+=23456;//bad code. magic number 23456 is used.
    g+=size;
    return g/10;
}

decimal updateRate(decimal rate)
{
    return rate / 0.2345M;
}

decimal updateRateM(decimal rateM)
{
    decimal basis = 0.2345M;
    return rateM/basis;
}";

var mathematicalExpressions = new []
{
    SyntaxKind.AddAssignmentExpression,
    SyntaxKind.SubtractAssignmentExpression,
    SyntaxKind.MultiplyAssignmentExpression,
    SyntaxKind.DivideAssignmentExpression,
    SyntaxKind.AddExpression,
    SyntaxKind.SubtractExpression,
    SyntaxKind.MultiplyExpression,
    SyntaxKind.DivideExpression
};

var root = CSharpSyntaxTree.ParseText(code).GetRoot();

root
    .DescendantNodes()
    .OfType<MethodDeclarationSyntax>()
    .Select(method => new 
        {
            MethodName = method.Identifier.ValueText,
            MagicLines = method
                .Body
                .DescendantNodes()
                .Where(node => mathematicalExpressions.Contains(node.Kind()))
                .Where(node => node.DescendantNodes().Any(descendantNode => descendantNode.IsKind(SyntaxKind.NumericLiteralExpression)))
                .Select(node => node.ToString())
        })
    .Where(result => result.MagicLines.Any())
    .Dump();