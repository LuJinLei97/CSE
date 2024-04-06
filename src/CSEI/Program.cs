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

        var testTexts = new[]
        {
            "1+2",
            "1+2-3",
            "1+2-3*4",
            "1+2*3-4/5",
        };
        foreach(var testText in testTexts)
        {
            var root = CseCompilerServices.ParseText(testText, cse);
            foreach(var node in root.Childs)
            {
                if(node.Expression.IsNull() == false)
                {
                    Console.WriteLine($"{node.Text}:{node?.Expression?.Excute()}");
                }
            }
        }

        while(ConsoleUtility.TipAndReadLine("cse text:").Out(out var line).Trim().Equals("esc", StringComparison.CurrentCultureIgnoreCase) == false)
        {
            var root = CseCompilerServices.ParseText(line, cse);
            foreach(var node in root.Childs)
            {
                if(node.Expression.IsNull() == false)
                {
                    Console.WriteLine(node?.Expression?.Excute());
                }
            }
        }
    }
}
