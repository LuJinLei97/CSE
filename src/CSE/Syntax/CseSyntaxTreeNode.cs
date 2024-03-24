using CSE.Classes;
using CSE.Extensions;

using JinLei.Classes;
using JinLei.Extensions;

using Newtonsoft.Json.Linq;

namespace CSE.Syntax;
public class CseSyntaxTreeNode : TreeNode<CseSyntaxTreeNode>
{
    public virtual string KindText { get; set; }

    public virtual bool IsKind(string kindText) => kindText == KindText || Parent?.IsKind(kindText) == true;

    public virtual List<string> MatchPatterns { get; set; } = [];

    public virtual int? Priority { get => priority ?? (Parent?.Priority + RelativePriority); set => priority = value; }
    protected int? priority;

    public virtual int RelativePriority { get; set; }

    public virtual CseSyntaxNode Match(Memory<char> text, Memory<CseSyntaxNode> nodes)
    {
        if(text.Length.Out(out var maxTextLength) >= 1)
        {
            var matchedItem = MatchPatterns.Where(t => t.Length <= maxTextLength).OrderByDescending(t => t.Length).FirstOrDefault(t => t.SequenceEqual(text.Slice(0, t.Length).ToArray()));
            if(matchedItem.IsNull() == false)
            {
                return new CseSyntaxNode()
                {
                    CseSyntaxTreeNode = this,
                };
            }
        }

        if(nodes.Length.Out(out var maxNodesLength) >= 1)
        {
            var ignoredKindTexts = new[] { "琐碎节点" };

            var matchNodes = new List<CseSyntaxNode>();
            var ignoredNodes = new List<CseSyntaxNode>();
            for(var i = 0; i < nodes.Length; i++)
            {
                if(matchNodes.Count == MatchPatterns.Count)
                {
                    break;
                }

                (ignoredKindTexts.Any(t => nodes.Span[i].CseSyntaxTreeNode.IsKind(t)) ? ignoredNodes : matchNodes).Add(nodes.Span[i]);
            }

            if(matchNodes.Count == MatchPatterns.Count && MatchPatterns.ForEach((t, i) => matchNodes[i].CseSyntaxTreeNode.IsKind(t)).All(t => t))
            {
                return new CseSyntaxNode()
                {
                    CseSyntaxTreeNode = this,
                    Childs = matchNodes.Select(t => t.Clone()).ToObservableCollection(),
                };
            }
        }

        return default;
    }

    public virtual IEnumerable<CseSyntaxTreeNode> EnumerateNodesFromRoot(string kindText = default, int count = int.MaxValue)
    {
        if(Childs.IsNull())
        {
            yield break;
        }

        var sum = 0;
        foreach(var child in Childs)
        {
            if(string.IsNullOrWhiteSpace(kindText) || child.IsKind(kindText))
            {
                yield return child;
                if(++sum == count)
                {
                    yield break;
                }
            } else
            {
                foreach(var item in child.EnumerateNodesFromRoot(kindText, count))
                {
                    yield return item;
                    if(++sum == count)
                    {
                        yield break;
                    }
                }
            }
        }
    }

    public virtual Expression Expression { get => expression ?? Parent?.Expression; set => expression = value; }
    protected Expression expression;

    public virtual void Load(JToken cseSyntaxTreeNodeJToken)
    {
        KindText = (cseSyntaxTreeNodeJToken["title"].Value<string>().Trim().Out(out var k).StartsWith("\"") && k.EndsWith("\"")) ? k.Substring(1, k.Length - 2) : k;

        if(GetChild(cseSyntaxTreeNodeJToken).Out(out var childs).IsNull() == false)
        {
            Childs = childs.Select(t => new CseSyntaxTreeNode() { Parent = this }.Do(t1 => t1.Load(t))).ToObservableCollection();

            foreach(var boundary in cseSyntaxTreeNodeJToken["boundaries"].GetSelfOrEmpty())
            {
                var (S, E) = boundary["range"].Value<string>().Split(new[] { "(", ",", ")" }, StringSplitOptions.RemoveEmptyEntries).Do(t => (S: int.Parse(t[0]), E: int.Parse(t[1])));
                if(boundary["title"].Value<string>().TrimStart().Out(out var ps).StartsWith("+") || ps.StartsWith("-"))
                {
                    Childs.GetRange(S, E - S + 1).ForEach(t => t.RelativePriority = int.Parse(ps));
                } else
                {
                    Childs.GetRange(S, E - S + 1).ForEach(t => t.Priority = int.Parse(ps));
                }
            }

            var summaryRanges = cseSyntaxTreeNodeJToken["summaries"].GetSelfOrEmpty().ToDictionary(t => t["topicId"].Value<string>(), t => t["range"].Value<string>());
            foreach(var summary in summaryRanges.IsNullOrEmpty() == false ? cseSyntaxTreeNodeJToken["children"]["summary"] : Enumerable.Empty<JToken>())
            {
                var (S, E) = summaryRanges[summary["id"].Value<string>()].Split(new[] { "(", ",", ")" }, StringSplitOptions.RemoveEmptyEntries).Do(t => (S: int.Parse(t[0]), E: int.Parse(t[1])));
                foreach(var child in Childs.GetRange(S, E - S + 1))
                {
                    child.Expression = new()
                    {
                        MethodName = summary["title"].Value<string>().Trim('"'),
                        ChildValueIndexs = GetChild(summary)?.Select(t => int.Parse(t["title"].Value<string>())).ToArray(),
                    };
                }
            }
        } else
        {
            Parent?.MatchPatterns?.Add(KindText);
        }

        JToken GetChild(JToken parent, string childName = default) => parent["children"]?["attached"]?.Out(out var childs).Return(childName.IsNull() ? childs : childs?.FirstOrDefault(t => t["title"].Value<string>() == childName));
    }
}

public enum SearchOption
{
    TopTreeNodeOnly,
    AllTreeNode
}