namespace CSE.Classes;
public class ParseProperties
{
    public virtual bool? 是否子语句解析 { get; set; }

    public virtual 匹配类型 匹配类型 { get; set; }
}

public enum 匹配类型
{
    最短成对匹配,
    最长成对匹配,
    顺序匹配
}