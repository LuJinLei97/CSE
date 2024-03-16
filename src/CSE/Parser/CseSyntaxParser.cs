using CSE.Classes;
using CSE.Syntax;

using Microsoft.CodeAnalysis.CSharp;

namespace CSE.Parser;
public class CseSyntaxParser
{
    #region static property
    public static CseSyntaxParser DefaultSyntaxParser => new()
    {
        MatchChildKindTextsList = new() { [SyntaxKind.UnknownAccessorDeclaration.ToString()] = [new() { SyntaxKind.UnknownAccessorDeclaration.ToString(), SyntaxKind.UnknownAccessorDeclaration.ToString() }] }
    };
    #endregion

    public virtual Dictionary<string, List<string>> MatchTexts { get; set; }

    public virtual Dictionary<string, List<List<string>>> MatchChildKindTextsList { get; set; }

    public virtual string KindText { get => kindText ?? (MatchTexts?.Keys.FirstOrDefault() ?? MatchChildKindTextsList?.Keys.FirstOrDefault()) ?? DefaultSyntaxParser.KindText; set => kindText = value; }
    protected string kindText;

    public virtual CseSyntaxNode Match(string text, List<CseSyntaxNode> nodes)
    {
        if(nodes != null && MatchChildKindTextsList != null)
        {
            var matchedItem = MatchChildKindTextsList.Values.SelectMany(t => t).Where(t => t.Count <= nodes.Count).OrderByDescending(t => t.Count).FirstOrDefault(t => Enumerable.Range(0, t.Count).All(i => new[] { nodes[i].KindText, nodes[i].MatchedKindText }.Contains(t[i])));
            if(matchedItem != null)
            {
                var parent = new CseSyntaxNode() { Childs = new(nodes.GetRange(0, matchedItem.Count).Select(t => t.Clone())), KindText = KindText, MatchedKindText = MatchChildKindTextsList.FirstOrDefault(t => t.Value.Contains(matchedItem)).Key, Expression = Expression?.Clone() };

                return parent;
            }
        }

        if(text != null && MatchTexts != null)
        {
            var matchedItem = MatchTexts.Values.SelectMany(t => t).Where(t => t.Length <= text.Length).OrderByDescending(t => t.Length).FirstOrDefault(t => t.Equals(text.Substring(0, t.Length)));
            if(matchedItem != null)
            {
                var node = new CseSyntaxNode() { Text = text.Substring(0, matchedItem.Length), KindText = KindText, MatchedKindText = MatchTexts.FirstOrDefault(t => t.Value.Contains(matchedItem)).Key, Expression = Expression?.Clone() };

                return node;
            }
        }

        return default;
    }

    public virtual Expression Expression { get; set; }

    public virtual CseSyntaxParser Clone()
    {
        CseSyntaxParser result = new()
        {
            KindText = KindText,
            MatchTexts = MatchTexts?.ToDictionary(t => t.Key, t => t.Value?.ToList()),
            MatchChildKindTextsList = MatchChildKindTextsList?.ToDictionary(t => t.Key, t => t.Value?.Select(t => t.ToList()).ToList()),
            Expression = Expression?.Clone(),
        };

        return result;
    }
}