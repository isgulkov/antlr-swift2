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

		}

		static string PrintExpression(SwiftParser.ComparativeExprContext context)
		{

		}

		static string PrintExpression(SwiftParser.RangeExprContext context)
		{

		}

		static string PrintExpression(SwiftParser.PrimaryExprContext context)
		{

		}

		static string PrintExpression(SwiftParser.MultiplicativeExprContext context)
		{

		}

		static string PrintExpression(SwiftParser.AdditiveExprContext context)
		{
			return context.GetText();
		}

		static string PrintExpression(SwiftParser.UnaryExprContext context)
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
