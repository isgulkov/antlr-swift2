using System;
using System.IO;
using System.Linq;

namespace SwiftTranslator
{
	public class SwiftSuperListener : SwiftBaseListener
	{
		readonly string OutputFilename;
		StreamWriter OutputWriter;

		public SwiftSuperListener(string outputFilename)
		{
			OutputFilename = outputFilename;

			OutputWriter = new StreamWriter(File.Open(OutputFilename, FileMode.Create));
			OutputWriter.AutoFlush = true;
		}

		void Out(string s)
		{
			OutputWriter.Write(s);
		}

		void OutLine(string s)
		{
			OutputWriter.WriteLine(s);
		}

		void PrintErrorAndExit(int error_code, string message)
		{
			Console.WriteLine($"Error {error_code}: {message}");

			OutputWriter.Close();
			File.Delete(OutputFilename);

			Environment.Exit(1);
		}

		public override void EnterFile(SwiftParser.FileContext context)
		{
            OutLine("using System;");
			OutLine("");
			OutLine("class Program {");

            OutLine("static bool CompareWithTypes(object one, object another)");
			OutLine("{");
			OutLine("if(one is int && another is int) {");
			OutLine("return (int)one == (int)another");
			OutLine("}");
			OutLine("else if(one is double && another is double) {");
			OutLine("return (double)one == (double)another");
			OutLine("}");
			OutLine("else if(one is bool && another is bool) {");
			OutLine("return (bool)one == (bool)another");
			OutLine("}");
			OutLine("else {");
			OutLine("return false;");
			OutLine("}");
			OutLine("}");


			OutLine("public static void Main() {");
		}

		public override void ExitFile(SwiftParser.FileContext context)
		{
			OutLine("}");
			OutLine("}");
		}

		/*
		 * Escape ids that are C# keywords
		 */
		static string EscapeId(string id)
		{
			return "_" + id;
		}

		// TODO: consider putting parentheses around all printed expressions
		static string PrintExpression(SwiftParser.ExpressionContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.tertiaryExpr());
			}
			else {
				return PrintExpression(context.tertiaryExpr()) + " = " + PrintExpression(context.expression());
			}
		}

		static string PrintExpression(SwiftParser.TertiaryExprContext context)
		{
			string result = PrintExpression(context.disjunctiveExpr()[0]);

			if(context.disjunctiveExpr().Count() == 2) {
				result += " ? " + PrintExpression(context.disjunctiveExpr()[1]);
			}

			if(context.tertiaryExpr() != null) {
				result += " : " + PrintExpression(context.tertiaryExpr());
			}

			return result;
		}

		static string PrintExpression(SwiftParser.DisjunctiveExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.conjunctiveExpr());
			}
			else {
				return PrintExpression(context.disjunctiveExpr()) + "||" + PrintExpression(context.conjunctiveExpr());
			}
		}

		static string PrintExpression(SwiftParser.ConjunctiveExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.comparativeExpr());
			}
			else {
				return PrintExpression(context.conjunctiveExpr()) + "&&" + PrintExpression(context.comparativeExpr());
			}
		}

		static string PrintExpression(SwiftParser.ComparativeExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.rangeExpr()[0]);
			}
			else if(context.children[1].GetText() == "===") {
				return $"CompareWithTypes({PrintExpression(context.rangeExpr()[0])}," +
					$"{PrintExpression(context.rangeExpr()[1])})";
			}
			else if(context.children[1].GetText() == "!==") {
				return $"!CompareWithTypes({PrintExpression(context.rangeExpr()[0])}," +
					$"{PrintExpression(context.rangeExpr()[1])})";
			}
			else {
				return PrintExpression(context.rangeExpr()[0])
					+ context.children[1].GetText() + PrintExpression(context.rangeExpr()[1]);
			}
		}

		static string PrintExpression(SwiftParser.RangeExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.additiveExpr()[0]);
			}
			else {
				return $"Enumerable.Range({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
		}

		static string PrintExpression(SwiftParser.AdditiveExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.multiplicativeExpr());
			}
			else {
				return PrintExpression(context.additiveExpr())
					+ context.children[1].GetText() + PrintExpression(context.multiplicativeExpr());
			}
		}

		static string PrintExpression(SwiftParser.MultiplicativeExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.unaryExpr());
			}
			else {
				return PrintExpression(context.multiplicativeExpr())
					+ context.children[1].GetText() + PrintExpression(context.unaryExpr());
			}
		}

		static string PrintExpression(SwiftParser.UnaryExprContext context)
		{
			if(context.RuleIndex == 0) {
				return PrintExpression(context.primaryExpr());
			}
			else  {
				return "!" + PrintExpression(context.primaryExpr());
			}
		}

		static string PrintExpression(SwiftParser.PrimaryExprContext context)
		{

		}

		public override void EnterDeclarationStmt(SwiftParser.DeclarationStmtContext context)
		{
			string csTypename;

			switch(context.TYPENAME().GetText()) {
				case "Int":
					csTypename = "int";
					break;
				case "Float":
					csTypename = "double";
					break;
				default: // Never happens
					csTypename = "";
					break;
			}

			/*
			 * Underscore added to escape ids that are C# keywords
			 */
			Out($"{csTypename} {EscapeId(context.ID().GetText())}");

			if(context.expression() != null) {
				Out($" = {PrintExpression(context.expression())}");
			}

			OutLine(";");
		}

		public override void EnterIfCondition(SwiftParser.IfConditionContext context)
		{
			Out($"if({PrintExpression(context.expression())} ");
		}

		public override void ExitIfCondition(SwiftParser.IfConditionContext context)
		{
			OutLine(") {");
		}

		public override void ExitIfBlock(SwiftParser.IfBlockContext context)
		{
			OutLine("}");
		}

		public override void EnterElseBlock(SwiftParser.ElseBlockContext context)
		{
			OutLine("else {");
		}

		public override void ExitElseBlock(SwiftParser.ElseBlockContext context)
		{
			OutLine("}");
		}

		public override void EnterLoopStmt(SwiftParser.LoopStmtContext context)
		{
			if(context.rangeExpr().additiveExpr().Count() != 2) {
				PrintErrorAndExit(1001, "Range not specified in a for-loop statement");
			}

			OutLine($"for(int {EscapeId(context.ID().GetText())}" +
					$"= {PrintExpression(context.rangeExpr().additiveExpr()[0])};" +
					$"i <= {PrintExpression(context.rangeExpr().additiveExpr()[1])}; i++) {{");
		}

		public override void ExitLoopStmt(SwiftParser.LoopStmtContext context)
		{
			OutLine("}");
		}

		public override void EnterBreakStmt(SwiftParser.BreakStmtContext context)
		{
			OutLine("break;");
		}
	}
}
