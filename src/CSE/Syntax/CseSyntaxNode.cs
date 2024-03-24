using CSE.Extensions;

using JinLei.Classes;
using JinLei.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSE.Syntax;

/// <summary>
/// 优先以子节点匹配
/// </summary>
public class CseSyntaxNode : TreeNode<CseSyntaxNode>
{
    public virtual string Text { get => Childs?.Aggregate(string.Empty, (r, t) => $"{r}{t?.Text}") ?? text ?? CseSyntaxTreeNode.MatchPatterns.FirstOrDefault(); set => text = value; }
    protected string text;

    [JsonIgnore]
    public virtual CseSyntaxTreeNode CseSyntaxTreeNode { get; set; }

    [JsonIgnore]
    public virtual Classes.Expression Expression => expression == CseSyntaxTreeNode?.Expression ? expression : CseSyntaxTreeNode?.Expression?.Clone()?.Do(t => { t.CseSyntaxNode = this; expression = t; });
    protected Classes.Expression expression;

    #region 低功能性
    public virtual CseSyntaxNode Clone()
    {
        var result = new CseSyntaxNode()
        {
            CseSyntaxTreeNode = CseSyntaxTreeNode,

            Childs = Childs?.Select(t => t.Clone()).ToObservableCollection(),

            Position = Position,
        };

        return result;
    }

    public virtual int Position
    {
        get => Childs?.Min(t => t.Position) ?? position;
        set => position = value;
    }
    protected int position;

    public virtual int EndPosition => Childs?.Max(t => t.EndPosition) ?? Position + (Text?.Length ?? 0);

    public override string ToString() => JToken.FromObject(this).ToString();
    #endregion
}

public static class CseSyntaxNodeExtension
{
}