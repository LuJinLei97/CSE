using System.IO.Compression;

using CSE.Classes;
using CSE.Syntax;

using JinLei.Extensions;

using Newtonsoft.Json.Linq;

namespace CSE;
public class CustomSchemeEngine
{
    /*  todo:
        完善 CseCLR.Method 对应功能 预计改进成WF(工作流)活动执行
        完善 生成Exe或dll功能,预计通过AssemblyBuilder.DefineDynamicAssembly(...)生成对应文件
        完善 CSE加载CseSyntaxParser机制,做到在哪个层级目录用哪个层级的Parsers
        完善 CSE to C# 转换,用于c#的源生成器以及代码生成
        完善 IDE支持
    */

    /*  todo:
        模仿Rosyln解析foreach语法糖(Syntactic sugar),转换特定范围内的语法树
        解析语法过程也执行代码,确定一些变量状态,类型:
        var d = new Dictionary<string,object>(){ ["1"] = 1 }后,能通过智能提示d["1"]且读取到数字1
        var Method(object p) 静态推断返回值类型
        Method(MethodCallExpression p) => Method(Method1(...))
    */

    public static CustomSchemeEngine Instance { get; } = new CustomSchemeEngine();

    public virtual CseSyntaxTreeNode CseSyntaxTree { get; set; } = new();

    public virtual CseSyntaxTreeNode LoadCseSyntaxTree(FileInfo cseSyntaxParsersFile)
    {
        if(cseSyntaxParsersFile?.Exists == true)
        {
            if(cseSyntaxParsersFile.Extension.Equals(".xmind", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    // 规定语法树下有语法节点,语法节点下有文本节点和语句节点 (后续为配置名称)
                    // 文本节点的终端节点是最后一级以'"'包括的文本
                    // 语句节点的终端节点是最后一级含子节点的节点

                    var jsonString = new StreamReader(ZipFile.OpenRead(cseSyntaxParsersFile.FullName).GetEntry("content.json").Open()).ReadToEnd();
                    var syntaxTree = JToken.Parse(jsonString)[0]["rootTopic"];

                    CseSyntaxTree = new CseSyntaxTreeNode().Do(t => t.Load(syntaxTree));
                } catch(Exception e)
                {
                    //throw e;
                }
            }
        }

        return CseSyntaxTree;
    }

    public virtual object RunText(string text) => CseCompilerServices.ParseText(text, this)?.Expression?.Excute();
}

public static class CseCompilerServices
{
    public static CseSyntaxNode ParseText(string text, CustomSchemeEngine customSchemeEngine = default)
    {
        var root = new CseSyntaxNode() { Childs = [] };

        if(text.IsNull())
        {
            goto Result;
        }

        customSchemeEngine ??= CustomSchemeEngine.Instance;
        var unknownNode = customSchemeEngine.CseSyntaxTree.EnumerateNodes(t => t.IsKind("未明确节点"), Syntax.SearchOption.TopTreeNode, 1).FirstOrDefault();

        // 解析
        // 1.文本节点匹配
        // 2.语句节点按优先级从高到低匹配,再按节点从左到右匹配,直到无匹配项

        ParseToken();
        MergeNodes();

    Result:
        return root;

        void ParseToken()
        {
            var cseSyntaxParsers = customSchemeEngine.CseSyntaxTree.EnumerateNodes(t => t.IsKind("文本节点"), Syntax.SearchOption.Child);

            var position = 0;
            while(position < text.Length)
            {
                var cseSyntaxNode = new CseSyntaxNode
                {
                    Text = text[position].ToString(),
                    Position = position,
                    CseSyntaxTreeNode = unknownNode,
                };

                foreach(var textParser in cseSyntaxParsers)
                {
                    var parseResult = textParser.Match(text.AsMemory(position), default);
                    if(parseResult.IsNull() == false)
                    {
                        cseSyntaxNode = parseResult;
                        cseSyntaxNode.Position = position;
                        break;
                    }
                }

                position = cseSyntaxNode.EndPosition;
                root.Childs.Add(cseSyntaxNode);
            }
        }

        void MergeNodes()
        {
            var cseSyntaxParsers = customSchemeEngine.CseSyntaxTree.EnumerateNodes(t => t.IsKind("语句节点"), Syntax.SearchOption.Child).GroupBy(t => t.Parent).Select(t => t.Key);

            var isMatched = false;
            do
            {
                isMatched = false;
                for(var i = 0; i < root.Childs.Count; i++)
                {
                    foreach(var cseSyntaxParser in cseSyntaxParsers)
                    {
                        var parseResult = cseSyntaxParser.Match(default, root.Childs.ToArray().AsMemory(i));
                        if(parseResult != null)
                        {
                            root.Childs[i] = parseResult;
                            root.Childs.RemoveRange(i + 1, parseResult.Childs.Count - 1);
                            i--;
                            break;
                        }
                    }
                }
            } while(isMatched);
        }
    }
}

public static class CseCLR
{
    public static Dictionary<string, Method> CommonMethods { get => commonMethods ??= []; set => commonMethods = value; }
    private static Dictionary<string, Method> commonMethods;

    public static void RegisterCommonMethod(Method method) => CommonMethods[method.Name] = method;

    static CseCLR()
    {
        #region 数学计算相关
        RegisterCommonMethod(new() { Name = "number.ToNumber", Delegate = (object v1) => decimal.Parse(v1.ToString()) });
        RegisterCommonMethod(new() { Name = "number.Add", Delegate = (decimal v1, decimal v2) => decimal.Add(v1, v2) });
        RegisterCommonMethod(new() { Name = "number.Subtract", Delegate = (decimal v1, decimal v2) => decimal.Subtract(v1, v2) });
        RegisterCommonMethod(new() { Name = "number.Multiply", Delegate = (decimal v1, decimal v2) => decimal.Multiply(v1, v2) });
        RegisterCommonMethod(new() { Name = "number.Divide", Delegate = (decimal v1, decimal v2) => decimal.Divide(v1, v2) });
        RegisterCommonMethod(new() { Name = "object.Get", Delegate = (object v1) => v1 });
        #endregion
    }
}