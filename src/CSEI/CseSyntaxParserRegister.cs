using CSE;
using CSE.Parser;

using Microsoft.CodeAnalysis.CSharp;

namespace CSEI;
internal class CseSyntaxParserRegister
{
    public static CustomSchemeEngine CustomSchemeEngine { get => customSchemeEngine ??= CustomSchemeEngine.Instance; set => customSchemeEngine = value; }
    private static CustomSchemeEngine customSchemeEngine;

    public static void RegisterCseSyntaxParsers()
    {
        CseCompilerServices.RegisterCseSyntaxParser(CseSyntaxParser.DefaultSyntaxParser, CustomSchemeEngine);

        #region 数学计算表达式相关
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.WhitespaceTrivia.ToString()] = [" "] }, MatchChildKindTextsList = new() { [SyntaxKind.WhitespaceTrivia.ToString()] = [new() { SyntaxKind.WhitespaceTrivia.ToString(), SyntaxKind.WhitespaceTrivia.ToString() }] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.PlusToken.ToString()] = ["+"] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.MinusToken.ToString()] = ["-"] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.AsteriskToken.ToString()] = ["*"] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.SlashToken.ToString()] = ["/"] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.DotToken.ToString()] = ["."] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.OpenParenToken.ToString()] = ["("] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { [SyntaxKind.CloseParenToken.ToString()] = [")"] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchTexts = new() { ["IntNumericLiteralToken"] = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"] }, MatchChildKindTextsList = new() { ["IntNumericLiteralToken"] = [new() { "IntNumericLiteralToken", "IntNumericLiteralToken" }], ["DoubleNumericLiteralToken"] = [new() { "IntNumericLiteralToken", SyntaxKind.DotToken.ToString(), "IntNumericLiteralToken" }], ["NegativeNumericLiteralToken"] = [new() { SyntaxKind.MinusToken.ToString(), "NumericLiteralToken" }] }, KindText = SyntaxKind.NumericLiteralToken.ToString(), Expression = new() { MethodName = "number.ToNumber" } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchChildKindTextsList = new() { [SyntaxKind.AddExpression.ToString()] = [new() { SyntaxKind.NumericLiteralToken.ToString(), SyntaxKind.PlusToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }, new() { SyntaxKind.AddExpression.ToString(), SyntaxKind.PlusToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }] }, Expression = new() { MethodName = "number.Add", ChildValueIndexs = [0, 2] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchChildKindTextsList = new() { [SyntaxKind.SubtractExpression.ToString()] = [new() { SyntaxKind.NumericLiteralToken.ToString(), SyntaxKind.MinusToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }, new() { SyntaxKind.SubtractExpression.ToString(), SyntaxKind.MinusToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }] }, Expression = new() { MethodName = "number.Subtract", ChildValueIndexs = [0, 2] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchChildKindTextsList = new() { [SyntaxKind.MultiplyExpression.ToString()] = [new() { SyntaxKind.NumericLiteralToken.ToString(), SyntaxKind.AsteriskToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }, new() { SyntaxKind.MultiplyExpression.ToString(), SyntaxKind.AsteriskToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }] }, Expression = new() { MethodName = "number.Multiply", ChildValueIndexs = [0, 2] } }, CustomSchemeEngine);
        CseCompilerServices.RegisterCseSyntaxParser(new() { MatchChildKindTextsList = new() { [SyntaxKind.DivideExpression.ToString()] = [new() { SyntaxKind.NumericLiteralToken.ToString(), SyntaxKind.SlashToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }, new() { SyntaxKind.DivideExpression.ToString(), SyntaxKind.SlashToken.ToString(), SyntaxKind.NumericLiteralToken.ToString() }] }, Expression = new() { MethodName = "number.Divide", ChildValueIndexs = [0, 2] } }, CustomSchemeEngine);
        #endregion
    }
}
