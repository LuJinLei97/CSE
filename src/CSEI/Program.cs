using CSE;
using CSE.Syntax;

using JinLei.Extensions;
using JinLei.Utilities;

namespace CSEI;

internal class Program
{
    private static void Main(string[] args) => RunInteractive();

    private static void Test()
    {
        var cse = new CustomSchemeEngine();
        cse.LoadCseSyntaxTree(new FileInfo("../../../../../CSE语法树.xmind"));

        var root = new CseSyntaxNode();

        root = CseCompilerServices.ParseText(File.ReadAllText("../../../Program.cs"), cse);
        foreach(var cseSyntaxNode in root.Childs)
        {
            //Console.WriteLine(cseSyntaxNode.ToString());
            //Console.Write(cseSyntaxNode?.Expression?.Excute());
        }

        root = CseCompilerServices.ParseText("1+2+3", cse);
        root = CseCompilerServices.ParseText("10-1-2", cse);
        root = CseCompilerServices.ParseText("2*4*8", cse);
        root = CseCompilerServices.ParseText("8/4/2", cse);
        root = CseCompilerServices.ParseText("-1", cse);
        foreach(var cseSyntaxNode in root.Childs.Where(t => t.CseSyntaxTreeNode.IsKind("琐碎节点") == false))
        {
            Console.WriteLine(cseSyntaxNode.ToString());
            Console.Write(cseSyntaxNode?.Expression?.Excute());
        }
    }

    private static void RunInteractive()
    {
        var cse = new CustomSchemeEngine();
        cse.LoadCseSyntaxTree(new FileInfo("../../../../../CSE语法树.xmind"));

        var ns = cse.CseSyntaxTree.EnumerateNodesFromRoot("数字", 2);
        var nf = ns.FirstOrDefault();
        var nl = ns.LastOrDefault();

        while(ConsoleUtility.TipAndReadLine("cse text:").Out(out var line).Trim().Equals("esc", StringComparison.CurrentCultureIgnoreCase) == false)
        {
            foreach(var node in CseCompilerServices.ParseText(line, cse).Childs)
            {
                Console.WriteLine(node.ToString());
                Console.WriteLine(node?.Expression?.Excute());
            }
        }
    }
}
