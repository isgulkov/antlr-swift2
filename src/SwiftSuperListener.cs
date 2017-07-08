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
            OutLine("using System.Linq;");
			OutLine("");
			OutLine("class Program {");

            OutLine("static bool CompareWithTypes(object one, object another)");
			OutLine("{");
			OutLine("if(one is int && another is int) {");
			OutLine("return (int)one == (int)another;");
			OutLine("}");
			OutLine("else if(one is double && another is double) {");
			OutLine("return (double)one == (double)another;");
			OutLine("}");
			OutLine("else if(one is bool && another is bool) {");
			OutLine("return (bool)one == (bool)another;");
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

		string PrintExpression(SwiftParser.ExpressionContext context)
		{
			if(context.expression() == null) {
				return PrintExpression(context.tertiaryExpr());
			}
			else {
				return PrintExpression(context.tertiaryExpr()) + " = " + PrintExpression(context.expression());
			}
		}

		string PrintExpression(SwiftParser.TertiaryExprContext context)
		{
			string result = PrintExpression(context.disjunctiveExpr()[0]);

			if(context.disjunctiveExpr().Count() == 2) {
				result += " ? " + PrintExpression(context.disjunctiveExpr()[1]);
			}

			if(context.tertiaryExpr() != null) {
				result += " : " + PrintExpression(context.tertiaryExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.DisjunctiveExprContext context)
		{
			string result;

			if(context.disjunctiveExpr() == null) {
				result = PrintExpression(context.conjunctiveExpr());
			}
			else {
				result = PrintExpression(context.disjunctiveExpr()) + "||" + PrintExpression(context.conjunctiveExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.ConjunctiveExprContext context)
		{
			string result;

			if(context.conjunctiveExpr() == null) {
				result = PrintExpression(context.comparativeExpr());
			}
			else {
				result = PrintExpression(context.conjunctiveExpr()) + "&&" + PrintExpression(context.comparativeExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.ComparativeExprContext context)
		{
			string result;

			if(context.rangeExpr().Count() == 1) {
				result = PrintExpression(context.rangeExpr()[0]);
			}
			else if(context.children[1].GetText() == "===") {
				result = $"CompareWithTypes({PrintExpression(context.rangeExpr()[0])}," +
					$"{PrintExpression(context.rangeExpr()[1])})";
			}
			else if(context.children[1].GetText() == "!==") {
				result = $"!CompareWithTypes({PrintExpression(context.rangeExpr()[0])}," +
					$"{PrintExpression(context.rangeExpr()[1])})";
			}
			else {
				result = PrintExpression(context.rangeExpr()[0])
					+ context.children[1].GetText() + PrintExpression(context.rangeExpr()[1]);
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.RangeExprContext context)
		{
			string result;

			if(context.additiveExpr().Count() == 1) {
				result = PrintExpression(context.additiveExpr()[0]);
			}
			else {
				result = $"Enumerable.Range({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.AdditiveExprContext context)
		{
			string result;

			if(context.additiveExpr() == null) {
				result = PrintExpression(context.multiplicativeExpr());
			}
			else {
				result = PrintExpression(context.additiveExpr())
					+ context.children[1].GetText() + PrintExpression(context.multiplicativeExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.MultiplicativeExprContext context)
		{
			string result;

			if(context.multiplicativeExpr() == null) {
				result = PrintExpression(context.unaryExpr());
			}
			else {
				result = PrintExpression(context.multiplicativeExpr())
					+ context.children[1].GetText() + PrintExpression(context.unaryExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.UnaryExprContext context)
		{
			string result;

			if(context.ChildCount == 1) {
				result = PrintExpression(context.primaryExpr());
			}
			else {
				result = "!" + PrintExpression(context.primaryExpr());
			}

			return $"({result})";
		}

		string PrintExpression(SwiftParser.PrimaryExprContext context)
		{
			if(context.expression() != null) {
				return PrintExpression(context.expression());
			}
			else if(context.ID() != null) {
				return EscapeId(context.ID().GetText());
			}
			else {
				return context.GetText();
			}
		}

		public override void EnterPrintStmt(SwiftParser.PrintStmtContext context)
		{
			string toStringArgs = String.Join(
				",",
				context.expression().Select(PrintExpression).Select(s => $"({s}).ToString() + \" \"")
			);

			OutLine($"Console.WriteLine({toStringArgs});");
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

			OutLine($"foreach(int i in {PrintExpression(context.rangeExpr())}) {{");
		}

		public override void ExitLoopStmt(SwiftParser.LoopStmtContext context)
		{
			OutLine("}");
		}

		public override void EnterBreakStmt(SwiftParser.BreakStmtContext context)
		{
			OutLine("break;");
		}

		public override void EnterStatement(SwiftParser.StatementContext context)
		{
			if(context.expression() != null) {
				OutLine(PrintExpression(context.expression()) + ";");
			}
		}
	}
}
