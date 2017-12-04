<Query Kind="Statements">
  <Reference Relative="lib\Microsoft.CodeAnalysis.CSharp.dll" />
  <Reference Relative="lib\Microsoft.CodeAnalysis.dll" />
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
</Query>

var code =
@"
int fun(int b)
{
    int x = 323;
    int z = dic[x] + x + dic[323];
    return z + b;
}

float funny(float c)
{
    int d = 234;
    Dictionary<float,string> dic = getDic();
    float z = dic[d];
    return z;
}

Dictionary<float,string> getDic()
{
    return new Dictionary<float,string>();
}";

var root = CSharpSyntaxTree.ParseText(code).GetRoot();

root
    .DescendantNodes()
    .OfType<MethodDeclarationSyntax>()
    .Select(method => new
        {
            MethodName = method.Identifier.ValueText,
            MagicIndices = method
                .Body
                .DescendantNodes()
                .OfType<BracketedArgumentListSyntax>()
                .Where(argumentList => argumentList
                                        .Arguments
                                        .SelectMany(argument => argument.DescendantNodes())
                                        .Any(node => node.IsKind(SyntaxKind.NumericLiteralExpression)))
                .Select(argumentList => argumentList.ToString())
        })
    .Where(result => result.MagicIndices.Any())
    .Dump();