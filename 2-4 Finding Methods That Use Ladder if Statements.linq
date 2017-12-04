<Query Kind="Statements">
  <Reference Relative="lib\Microsoft.CodeAnalysis.CSharp.dll" />
  <Reference Relative="lib\Microsoft.CodeAnalysis.dll" />
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
</Query>

var code =
@"
void fun()
{
    //Call a function only once
    if(c1() == 1)
        f1();
    if(c1() == 2)
        f2();
    if(c1() == 3)
        f3();
    if(c1() == 4)
        f4();
    if(co() == 23)
        f22();
    if(co() == 24)
        f21();
}

void funny()
{
    read_that();
    if(c1() == 3)
        c13();
    if(c2() == 34)
        c45();
}";

var root = CSharpSyntaxTree.ParseText(code).GetRoot();

root
    .DescendantNodes()
    .OfType<MethodDeclarationSyntax>()
    .Select(method => new
        {
            MethodName = method.Identifier.ValueText,
            LadderFunctionCalls = method
                                    .Body
                                    .DescendantNodes()
                                    .OfType<IfStatementSyntax>()
                                    .Where(ifStatement => ifStatement.Condition.IsKind(SyntaxKind.EqualsExpression))
                                    .Select(ifStatement => ifStatement.Condition)
                                    .Cast<BinaryExpressionSyntax>()
                                    .Where(equalsExpression => equalsExpression.Left.IsKind(SyntaxKind.InvocationExpression) && equalsExpression.Right.IsKind(SyntaxKind.NumericLiteralExpression))
                                    .Where(equalsExpression => !((InvocationExpressionSyntax)equalsExpression.Left).ArgumentList.Arguments.Any())
                                    .Select(equalsExpression => new
                                        {
                                            CalledMethodName = ((InvocationExpressionSyntax)equalsExpression.Left).Expression.ToString(),
                                            IfStatement = equalsExpression.ToString()
                                        })
                                    .GroupBy(methodCall => methodCall.CalledMethodName)
                                    .Where(group => group.Count() > 1)
        })
    .Where(result => result.LadderFunctionCalls.Any())
    .Dump();