using CSE.Classes;

using JinLei.Classes;
using JinLei.Extensions;

namespace CSE.Syntax;
public class CseSyntaxTreeNode : TreeNode<CseSyntaxTreeNode>
{
    public virtual string KindText { get; set; }

    public virtual bool IsKind(string kindText) => kindText == KindText || Parent.IsKind(kindText);

    public virtual List<string> MatchPatterns { get; set; } = [];

    public virtual int Priority { get; set; }

    public virtual CseSyntaxNode Match(Memory<char> text, Memory<CseSyntaxNode> nodes)
    {
        if(text.Length.Out(out var maxTextLength) >= 1)
        {
            var matchedItem = MatchPatterns.Where(t => t.Length <= maxTextLength).OrderByDescending(t => t.Length).FirstOrDefault(t => t.SequenceEqual(text.Slice(0, t.Length).ToArray()));
            if(matchedItem.IsNull() == false)
            {
                return new CseSyntaxNode()
                {
                    KindText = KindText,
                    Text = matchedItem,
                };
            }
        }

        if(nodes.Length.Out(out var maxNodesLength) >= 1)
        {
            var ignoredKindTexts = new[] { "琐碎节点" };

            var matchNodes = new LinkedList<CseSyntaxNode>();
            var ignoredNodes = new LinkedList<CseSyntaxNode>();
            for(var i = 0; i < nodes.Length; i++)
            {
                if(matchNodes.Count == MatchPatterns.Count)
                {
                    break;
                }

                (ignoredKindTexts.Contains(nodes.Span[i].KindText) ? ignoredNodes : matchNodes).AddLast(nodes.Span[i]);
            }

            if(MatchPatterns.SequenceEqual(matchNodes.Select(t => t.KindText)))
            {
                return new CseSyntaxNode()
                {
                    KindText = KindText,
                    Childs = new(matchNodes.Select(t => t.Clone())),
                    Expression = Expression?.Clone(),
                };
            }
        }

        return default;
    }

    public virtual Expression Expression { get; set; }
}