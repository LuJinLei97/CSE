using System.Text.Json;
using System.Text.Json.Serialization;

using CSE.Classes;
using CSE.Parser;
using CSE.Syntax;

using JinLei.Extensions;

namespace CSE;
public class CustomSchemeEngine
{
    /*  todo:
        CseSyntaxNodeExtension.IsKind(string childKind, string parentKind) 用于分类节点,如所有节点都应是 CseSyntaxNode
        CseSyntaxParser.IgnoreKindTexts 用于忽略某些琐碎节点
        Expression.ObjectTypeName 定义好默认的几个类型,如number是c#中decimal,用于字符串转换到类型
        Expression.ObjectValue 为了性能考虑,使用确定类型执行Expression
        完善 CseCLR.Method 对应功能
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

    public const string FileExtension = ".cse";

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReferenceHandler = ReferenceHandler.IgnoreCycles };

    public static CustomSchemeEngine Instance { get; } = new CustomSchemeEngine();

    public Dictionary<string, CseSyntaxParser> CseSyntaxParsers { get => cseSyntaxParsers ??= []; set => cseSyntaxParsers = value; }
    private Dictionary<string, CseSyntaxParser> cseSyntaxParsers;

    public Dictionary<string, CseSyntaxParser> LoadCseSyntaxParsers(FileInfo cseSyntaxParsersFile)
    {
        if(cseSyntaxParsersFile?.Exists != true)
        {
            return CseSyntaxParsers;
        }

        foreach(var cseSyntaxParser in JsonSerializer.Deserialize<Dictionary<string, CseSyntaxParser>>(File.ReadAllText(cseSyntaxParsersFile.FullName)).Values)
        {
            CseCompilerServices.RegisterCseSyntaxParser(cseSyntaxParser, this);
        }

        return CseSyntaxParsers;
    }

    public object RunCseFile(FileInfo cseFile = default, DirectoryInfo workDir = default)
    {
        if((workDir ??= cseFile?.Directory) == null)
        {
            return default;
        }

        if(cseFile?.Exists != true)
        {
            return default;
        }

        var secondFileExtension = Path.GetExtension(Path.GetFileNameWithoutExtension(cseFile.FullName)).TrimStart('.');

        var cseSyntaxParsersFiles = new[]
        {
            new FileInfo(Path.Combine(workDir.FullName, "Cse.cse.cse")),
            new FileInfo(Path.Combine(workDir.FullName, $"{secondFileExtension}.{secondFileExtension}.cse")),
            new FileInfo(Path.Combine(cseFile.Directory.FullName, "Cse.cse.cse")),
            new FileInfo(Path.Combine(cseFile.Directory.FullName, $"{secondFileExtension}.{secondFileExtension}.cse")),
        };

        foreach(var cseSyntaxParsersFile in cseSyntaxParsersFiles.GroupBy(t => t.FullName.ToLower().Trim()).Select(t => t.FirstOrDefault()))
        {
            LoadCseSyntaxParsers(cseSyntaxParsersFile);
        }

        return RunText(File.ReadAllText(cseFile.FullName));
    }

    public object RunText(string text) => CseCompilerServices.ParseText(text, this)?.Expression?.Excute();

}

public static class CseCompilerServices
{
    public static Dictionary<string, CseSyntaxParser> GetCseSyntaxParsers(CustomSchemeEngine customSchemeEngine = default) => (customSchemeEngine ?? CustomSchemeEngine.Instance).CseSyntaxParsers;

    public static void RegisterCseSyntaxParser(CseSyntaxParser cseSyntaxParser, CustomSchemeEngine customSchemeEngine = default) => GetCseSyntaxParsers(customSchemeEngine)[cseSyntaxParser.KindText] = cseSyntaxParser;

    public static CseSyntaxNode ParseText(string text, CustomSchemeEngine customSchemeEngine = default)
    {
        if(text == null)
        {
            return CseSyntaxNode.DefaultCseSyntaxNode;
        }

        customSchemeEngine ??= CustomSchemeEngine.Instance;

        var root = new CseSyntaxNode() { Childs = [] };

        ParseToken();
        MergeNodes();

        return root;

        void ParseToken()
        {
            var cseSyntaxParsers = GetCseSyntaxParsers(customSchemeEngine).Values.Where(t => t.MatchTexts != null).SelectMany(t => t.MatchTexts.SelectMany(t1 => t1.Value, (t1, t2) => (Text: t2, MatchKindText: t1.Key, Parser: t))).OrderByDescending(t => t.Text.Length);

            var position = 0;
            while(position < text.Length)
            {
                var cseSyntaxNode = CseSyntaxNode.DefaultCseSyntaxNode;
                cseSyntaxNode.Text = text[position].ToString();
                cseSyntaxNode.Position = position;

                foreach(var (Text, MatchKindText, Parser) in cseSyntaxParsers)
                {
                    var parser = Parser.Clone();
                    parser.MatchTexts = new() { [MatchKindText] = [Text] };
                    var parseResult = parser.Match(text.Substring(position, Math.Min(Text.Length, text.Length - position)), default);
                    if(parseResult != null)
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
            var cseSyntaxParsers = GetCseSyntaxParsers(customSchemeEngine).Values.Where(t => t.MatchChildKindTextsList != null).SelectMany(t => t.MatchChildKindTextsList.SelectMany(t1 => t1.Value, (t1, t2) => (ChildNodeKindTexts: t2, MatchKindText: t1.Key, Parser: t))).OrderByDescending(t => t.ChildNodeKindTexts.Count);
            var cseSyntaxParsers1 = cseSyntaxParsers.Where(t => t.ChildNodeKindTexts.All(t1 => t.Parser.MatchChildKindTextsList.ContainsKey(t1) || t1 == t.Parser.KindText));

            MergeNodes(cseSyntaxParsers1);
            MergeNodes(cseSyntaxParsers.Except(cseSyntaxParsers1));

            void MergeNodes(IEnumerable<(List<string> ChildNodeKindTexts, string MatchKindText, CseSyntaxParser Parser)> cseSyntaxParsers)
            {
                for(var i = 0; i < root.Childs.Count - 1; i++)
                {
                    foreach(var (ChildNodeKindTexts, MatchKindText, Parser) in cseSyntaxParsers)
                    {
                        var parser = Parser.Clone();
                        parser.MatchChildKindTextsList = new() { [MatchKindText] = [ChildNodeKindTexts] };
                        var parseResult = parser.Match(default, root.Childs.GetRange(i, Math.Min(ChildNodeKindTexts.Count, root.Childs.Count - i)));
                        if(parseResult != null)
                        {
                            root.Childs[i] = parseResult;
                            root.Childs.RemoveRange(i + 1, parseResult.Childs.Count - 1);
                            i--;
                            break;
                        }
                    }
                }
            }
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
        #endregion
    }
}