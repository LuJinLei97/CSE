using System.Collections.Immutable;

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

    public virtual int RelativePriority { get; set; }

    public virtual CseSyntaxNode Match(ReadOnlyMemory<char> text, ReadOnlyMemory<CseSyntaxNode> nodes)
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

            if(matchNodes.Count == MatchPatterns.Count && MatchPatterns.ForEachDo((t, i) => matchNodes[i].CseSyntaxTreeNode.IsKind(t)).All(t => t))
            {
                return new CseSyntaxNode()
                {
                    CseSyntaxTreeNode = this,
                    Childs = matchNodes.Concat(ignoredNodes).Select(t => t.Clone()).ToObservableCollection(),
                };
            }
        }

        return default;
    }

    public virtual Expression Expression { get => expression ?? Parent?.Expression; set => expression = value; }
    protected Expression expression;

    public virtual ParseProperties ParseProperties { get; set; }

    public virtual void Load(JToken cseSyntaxTreeNodeJToken)
    {
        KindText = (cseSyntaxTreeNodeJToken["title"].Value<string>().Trim().Out(out var k).StartsWith("\"") && k.EndsWith("\"")) ? k.Substring(1, k.Length - 2) : k;

        if(GetChild(cseSyntaxTreeNodeJToken).Out(out var childs).IsNull() == false)
        {
            Childs = childs.Select(t => new CseSyntaxTreeNode() { Parent = this }.Do(t1 => t1.Load(t))).ToObservableCollection();

            foreach(var boundary in cseSyntaxTreeNodeJToken["boundaries"].GetSelfOrEmpty())
            {
                var (S, E) = boundary["range"].Value<string>().Split(new[] { "(", ",", ")" }, StringSplitOptions.RemoveEmptyEntries).Do(t => (S: int.Parse(t[0]), E: int.Parse(t[1])));
                boundary["title"].Value<string>().TrimStart().Out(out var ps);
                Childs.ToImmutableList().GetRange(S, E - S + 1).ForEach(t => t.RelativePriority = int.Parse(ps));
            }

            var summaryRanges = cseSyntaxTreeNodeJToken["summaries"].GetSelfOrEmpty().ToDictionary(t => t["topicId"].Value<string>(), t => t["range"].Value<string>());
            foreach(var summary in summaryRanges.IsNullOrEmpty() == false ? cseSyntaxTreeNodeJToken["children"]["summary"] : Enumerable.Empty<JToken>())
            {
                var (S, E) = summaryRanges[summary["id"].Value<string>()].Split(new[] { "(", ",", ")" }, StringSplitOptions.RemoveEmptyEntries).Do(t => (S: int.Parse(t[0]), E: int.Parse(t[1])));
                foreach(var child in Childs.ToImmutableList().GetRange(S, E - S + 1))
                {
                    if(GetChild(GetChild(summary, "Expression")).FirstOrDefault().Out(out var expression).IsNull() == false)
                    {
                        child.Expression = new()
                        {
                            MethodName = expression["title"].Value<string>().Trim('"'),
                            ChildValueIndexs = GetChild(expression)?.Select(t => int.Parse(t["title"].Value<string>())).ToArray(),
                        };
                    }

                    if(GetChild(GetChild(summary, "解析属性"), "子语句解析").Out(out var subtextParsing).IsNull() == false)
                    {
                        child.ParseProperties = new()
                        {
                            是否子语句解析 = true,
                            匹配类型 = Enum.TryParse(GetChild(GetChild(subtextParsing, "匹配类型")).FirstOrDefault()["title"].Value<string>(), out 匹配类型 e).Return(e)
                        };
                    }
                }
            }
        } else
        {
            Parent?.MatchPatterns?.Add(KindText);
            this?.MatchPatterns?.Add(KindText);
        }

        JToken GetChild(JToken parent, string childName = default) => parent?["children"]?["attached"]?.Out(out var childs).Return(childName.IsNull() ? childs : childs?.FirstOrDefault(t => t["title"].Value<string>() == childName));
    }

    public virtual IEnumerable<CseSyntaxTreeNode> EnumerateNodes(Predicate<CseSyntaxTreeNode> predicate = default, SearchOption searchOption = SearchOption.TopTreeNodeOnly, int count = int.MaxValue / 2)
    {
        if(Childs.IsNull() || count <= 0)
        {
            yield break;
        }

        predicate ??= (t) => true;

        var sum = 0;
        foreach(var child in Childs.OrderByDescending(t => t.RelativePriority))
        {
            var isMatched = predicate(child);
            if(isMatched)
            {
                if(searchOption is SearchOption.TopTreeNodeOnly or SearchOption.AllTreeNode or SearchOption.TopTreeNode || searchOption == SearchOption.Parent && (child?.Childs?.Count).GetValueOrDefault(0) >= 1 || searchOption == SearchOption.Child && (child?.Childs?.Count).GetValueOrDefault(0) <= 0)
                {
                    yield return child;
                    if(++sum == count)
                    {
                        yield break;
                    }
                }
            }

            if(searchOption is SearchOption.AllTreeNode or SearchOption.Parent or SearchOption.Child || searchOption == SearchOption.TopTreeNode && isMatched == false)
            {
                foreach(var item in child.EnumerateNodes(predicate, searchOption, count - sum))
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
}

public enum SearchOption
{
    TopTreeNodeOnly,
    AllTreeNode,
    TopTreeNode,
    Parent,
    Child,
}