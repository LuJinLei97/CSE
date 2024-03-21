
using CSE.Extensions;

using JinLei.Classes;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSE.Syntax;

/// <summary>
/// 优先以子节点匹配
/// </summary>
public class CseSyntaxNode : TreeNode<CseSyntaxNode>
{
    #region static property
    public static CseSyntaxNode DefaultCseSyntaxNode => new() { KindText = SyntaxKind.UnknownAccessorDeclaration.ToString() };
    #endregion

    public virtual string Text { get => Childs?.Aggregate(string.Empty, (r, t) => $"{r}{t?.Text}") ?? text; set => text = value; }
    protected string text;

    [JsonIgnore]
    public virtual string KindText { get => kindText ?? DefaultCseSyntaxNode.KindText; set => kindText = value; }
    protected string kindText;

    public virtual string MatchedKindText { get => matchedKindText ?? KindText; set => matchedKindText = value; }
    protected string matchedKindText;

    [JsonIgnore]
    public virtual Classes.Expression Expression
    {
        get
        {
            if(expression != null)
            {
                expression.CseSyntaxNode = this;
            }

            return expression;
        }
        set
        {
            expression = value;
            if(expression != null)
            {
                expression.CseSyntaxNode = this;
            }
        }
    }
    protected Classes.Expression expression;

    [JsonIgnore]
    public virtual Func<AnalysisResult> AnalyzeFunc { get; set; }

    #region 低功能性
    public virtual CseSyntaxNode Clone()
    {
        var result = new CseSyntaxNode()
        {
            KindText = KindText,
            MatchedKindText = MatchedKindText,

            Text = Text,
            Position = Position,

            Childs = Childs?.Select(t => t.Clone()).ToObservableCollection(),
            //Parent = Parent,

            Expression = Expression?.Clone(),
            AnalyzeFunc = AnalyzeFunc,
        };

        return result;
    }

    public virtual int Position
    {
        get => Childs?.FirstOrDefault()?.Position ?? position;
        set => position = value;
    }
    protected int position;

    public virtual int EndPosition => Childs?.LastOrDefault()?.EndPosition ?? Position + Text?.Length ?? 0;

    public override string ToString() => JToken.FromObject(this).ToString();
    #endregion
}

public static class CseSyntaxNodeExtension
{
    public static bool IsTrivia(this CseSyntaxNode cseSyntaxNode) => cseSyntaxNode.KindText.EndsWith("Trivia");

    public static bool IsExpression(this CseSyntaxNode cseSyntaxNode) => cseSyntaxNode.KindText.EndsWith("Expression");
}