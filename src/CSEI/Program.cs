using CSE;

using JinLei.Extensions;
using JinLei.Utilities;

namespace CSEI;

internal class Program
{
    private static void Main(string[] args) => RunInteractive();

    private static void RunInteractive()
    {
        var cse = new CustomSchemeEngine();
        cse.LoadCseSyntaxTree(new FileInfo("../../../../../CSE语法树.xmind"));

        var root = CseCompilerServices.ParseText("1+1", cse);
        root = CseCompilerServices.ParseText("(1+2)*3-4/5+6-12", cse);
        Console.WriteLine(root.Childs[0].Expression?.Excute());

        while(ConsoleUtility.TipAndReadLine("cse text:").Out(out var line).Trim().Equals("esc", StringComparison.CurrentCultureIgnoreCase) == false)
        {
            var root1 = CseCompilerServices.ParseText(line, cse);
            foreach(var node in root1.Childs)
            {
                Console.WriteLine(node.ToString());
                Console.WriteLine(node?.Expression?.Excute());
            }
        }
    }
}
