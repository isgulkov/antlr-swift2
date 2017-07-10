using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SwiftTranslator
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if(args.Length != 2) {
				Console.WriteLine("usage: SwiftTranslator.exe [input file] [output file]");
				Environment.Exit(1);
			}

			string input = File.ReadAllText(args[0]);

			var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
			var lexer = new SwiftLexer(new AntlrInputStream(ms));

			var tokens = new CommonTokenStream(lexer);

			var parser = new SwiftParser(tokens);

			var tree = parser.file();

			var pastwk = new ParseTreeWalker();

			/*
			 * Первым проходом по дереву проверим, не находятся ли два оператора на одной строке, не разделенные точкой
			 * с запятой
			 * 
			 * В рамках грамматики ANTLR это проверить проблематично, потому что для этого нужно обрабатывать в ней
			 * символы переноса строки, а не пропускать их. Так как символы переноса строки в языке Swift разрешены
			 * почти везде, это приведет к необходимости пихать всюду `NEWLINE*`, что приведет грамматику в нечитаемый
			 * и неподдерживаемый вид, а также может привести к многочисленным трудноуловимым ошибкам вида "пропущено
			 * `NEWLINE*`". В данном случае возможная альтернатива двухпроходной обработке синтаксического дерева
			 * — предоварительная потока лексем на основе отдельной грамматики.
			 */
			SwiftStatementPlacementCheckListener collisionChecker = new SwiftStatementPlacementCheckListener();

			pastwk.Walk(collisionChecker, tree);

			if(!collisionChecker.IsValid) {
				foreach(Tuple<int, int> collisionLoc in collisionChecker.CollisionLocations) {
					Console.WriteLine($"Error 1889: At line {collisionLoc.Item1}, column {collisionLoc.Item2}: " +
					                  "two consecutive statements should be separated by a newline or a semicolon");
				}

				Environment.Exit(1);
			}

			pastwk.Walk(new SwiftMainListener(args[1]), tree);
		}
	}
}
