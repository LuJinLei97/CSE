using CSE.Syntax;

namespace CSE.Classes;
public class Expression
{
    public virtual string MethodName { get; set; }

    public virtual int[] ChildValueIndexs { get; set; }

    public virtual object Excute()
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
            args = ChildValueIndexs.Select(t => CseSyntaxNode.Childs[t]?.Expression?.Excute() ?? CseSyntaxNode.Childs[t]?.Text).ToArray();
        }

        return CseCLR.CommonMethods[MethodName]?.Delegate?.DynamicInvoke(args);
    }

    #region 低功能性
    public virtual Expression Clone() => new() { MethodName = MethodName, ChildValueIndexs = ChildValueIndexs };

    public virtual CseSyntaxNode CseSyntaxNode { get; set; }
    #endregion
}
