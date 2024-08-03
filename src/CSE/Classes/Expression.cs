using CSE.Syntax;

namespace CSE.Classes;
public class Expression : ICloneable, IEquatable<Expression>
{
    public virtual required string? MethodName { get; set; }

    public virtual required int[]? ChildValueIndexs { get; set; }

    public virtual object? Excute()
    {
        if(MethodName == null)
        {
            throw new ArgumentNullException(nameof(MethodName));
        } else if(CseSyntaxNode == null)
        {
            throw new ArgumentNullException(nameof(CseSyntaxNode));
        }

        var args = new object[] { CseSyntaxNode?.Text };
        if(ChildValueIndexs != null && CseSyntaxNode.Childs != null)
        {
            args = ChildValueIndexs.Select(t => CseSyntaxNode.Childs[t - 1]?.Expression?.Excute() ?? CseSyntaxNode.Childs[t - 1]?.Text).ToArray();
        }

        return CseCLR.CommonMethods[MethodName]?.Delegate?.DynamicInvoke(args);
    }

    #region 低功能性
    public virtual Expression Clone() => new() { MethodName = MethodName, ChildValueIndexs = ChildValueIndexs };

    public virtual bool Equals(Expression other) => other.MethodName == MethodName && other?.ChildValueIndexs?.SequenceEqual(this?.ChildValueIndexs) == true;

    object ICloneable.Clone() => Clone();

    public virtual required CseSyntaxNode? CseSyntaxNode { get; set; }
    #endregion
}
