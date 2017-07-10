using System;
using System.IO;
using System.Linq;

namespace SwiftTranslator
{
	public class SwiftMainListener : SwiftBaseListener
	{
		readonly string OutputFilename;
		StreamWriter OutputWriter;

		string ClassDeclrsOutput = "";

		bool InClass = false;

		public SwiftMainListener(string outputFilename)
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
			OutLine("return one.GetType() == another.GetType() && one == another;");
			OutLine("}");

            OutLine("static bool LessThanEvenForStrings(object one, object another)");
			OutLine("{");
			OutLine("if(one is string && another is string) {");
			OutLine("return String.Compare((string)one, (string)another) < 0;");
			OutLine("}");
			OutLine("else {");
			OutLine("throw new Exception($\"< cannot be applied to objects of types {one.GetType()} and {another.GetType()}\");");
			OutLine("}");
			OutLine("}");
			OutLine("static bool GreaterThanEvenForStrings(object one, object another)");
			OutLine("{");
			OutLine("if(one is string && another is string) {");
			OutLine("return String.Compare((string)one, (string)another) > 0;");
			OutLine("}");
			OutLine("else {");
			OutLine("throw new Exception($\"> cannot be applied to objects of types {one.GetType()} and {another.GetType()}\");");
			OutLine("}");
			OutLine("}");

			OutLine("static string Print(object o)");
			OutLine("{");
			OutLine("if(o is bool) {");
			OutLine("if((bool)o) {");
			OutLine("return \"true\";");
			OutLine("}");
			OutLine("else {");
			OutLine("return \"false\";");
			OutLine("}");
			OutLine("}");
			OutLine("else {");
			OutLine("return o.ToString();");
			OutLine("}");
			OutLine("}");

			OutLine("public static void Main() {");
		}

		public override void ExitFile(SwiftParser.FileContext context)
		{
			OutLine("}");
			Out(ClassDeclrsOutput);
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

				if(context.tertiaryExpr() != null) {
					result += " : " + PrintExpression(context.tertiaryExpr());
				}

				return $"({result})";
			}

			return result;
		}

		string PrintExpression(SwiftParser.DisjunctiveExprContext context)
		{
			if(context.disjunctiveExpr() == null) {
				return PrintExpression(context.conjunctiveExpr());
			}
			else {
				return $"({PrintExpression(context.disjunctiveExpr())} || {PrintExpression(context.conjunctiveExpr())})";
			}
		}

		string PrintExpression(SwiftParser.ConjunctiveExprContext context)
		{
			if(context.conjunctiveExpr() == null) {
				return PrintExpression(context.comparativeExpr());
			}
			else {
				return $"{PrintExpression(context.conjunctiveExpr())} && {PrintExpression(context.comparativeExpr())}";
			}
		}

		string PrintExpression(SwiftParser.ComparativeExprContext context)
		{
			if(context.additiveExpr().Count() == 1) {
				return PrintExpression(context.additiveExpr()[0]);
			}
			else if(context.children[1].GetText() == "===") {
				return $"CompareWithTypes({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else if(context.children[1].GetText() == "!==") {
				return $"!CompareWithTypes({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else if(context.children[1].GetText() == "<") {
				return $"LessThanEvenForStrings({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else if(context.children[1].GetText() == "<=") {
				return $"!GreaterThanEvenForStrings({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else if(context.children[1].GetText() == ">") {
				return $"GreaterThanEvenForStrings({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else if(context.children[1].GetText() == ">=") {
				return $"!LessThanEvenForStrings({PrintExpression(context.additiveExpr()[0])}," +
					$"{PrintExpression(context.additiveExpr()[1])})";
			}
			else {
				return $"{PrintExpression(context.additiveExpr()[0])}"
					+ $" {context.children[1].GetText()} {PrintExpression(context.additiveExpr()[1])}";
			}
		}

		string PrintExpression(SwiftParser.AdditiveExprContext context)
		{
			if(context.additiveExpr() == null) {
				return PrintExpression(context.multiplicativeExpr());
			}
			else {
				return $"({PrintExpression(context.additiveExpr())}"
					+ $" {context.children[1].GetText()} {PrintExpression(context.multiplicativeExpr())})";
			}
		}

		string PrintExpression(SwiftParser.MultiplicativeExprContext context)
		{
			if(context.multiplicativeExpr() == null) {
				return PrintExpression(context.unaryExpr());
			}
			else {
				return $"({PrintExpression(context.multiplicativeExpr())}"
					+ $" {context.children[1].GetText()} {PrintExpression(context.unaryExpr())})";
			}
		}

		string PrintExpression(SwiftParser.UnaryExprContext context)
		{
			if(context.ChildCount == 1) {
				return PrintExpression(context.primaryExpr());
			}
			else {
				return $"(!{PrintExpression(context.primaryExpr())})";
			}
		}

		string PrintExpression(SwiftParser.PrimaryExprContext context)
		{
			if(context.expression() != null) {
				return $"({PrintExpression(context.expression())})";
			}
			else if(context.ID() != null && context.primaryExpr() != null) {
				return $"({PrintExpression(context.primaryExpr())}.{EscapeId(context.ID().GetText())})";
			}
			else if(context.ID() != null && context.ChildCount == 3) {
				return $"(new {EscapeId(context.ID().GetText())}())";
			}
			else if(context.ID() != null) {
				return EscapeId(context.ID().GetText());
			}
			else { // Literals
				return context.GetText();
			}
		}

		public override void EnterPrintStmt(SwiftParser.PrintStmtContext context)
		{
			string toStringArgs = String.Join(
				" + ",
				context.expression().Select(PrintExpression).Select(s => $"Print({s})")
			);

			OutLine($"Console.WriteLine({toStringArgs});");
		}

		public override void EnterVariableDeclStmt(SwiftParser.VariableDeclStmtContext context)
		{
			string result = "";

			if(InClass) {
				result += "public ";
			}

			string csTypename;

			if(context.TYPENAME() != null) {
				switch(context.TYPENAME().GetText()) {
					case "Bool":
						csTypename = "bool";
						break;
					case "String":
						csTypename = "string";
						break;
					default: // Never happens
						csTypename = "";
						break;
				}
			}
			else {
				csTypename = EscapeId(context.ID()[0].GetText());
			}

			if(context.TYPENAME() != null) {
				result += $"{csTypename} {EscapeId(context.ID()[0].GetText())}";
			}
			else {
				result += $"{EscapeId(context.ID()[1].GetText())} {EscapeId(context.ID()[0].GetText())}";
			}

			if(context.expression() != null) {
				result += $" = {PrintExpression(context.expression())}";
			}

			result += ";\n";

			if(InClass) {
				ClassDeclrsOutput += result;
			}
			else {
				Out(result);
			}
		}

		public override void EnterLoopStmt(SwiftParser.LoopStmtContext context)
		{
			OutLine("do {");
		}

		public override void ExitLoopStmt(SwiftParser.LoopStmtContext context)
		{
			OutLine($"}} while ({PrintExpression(context.expression())});");
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

		public override void EnterClassDeclStmt(SwiftParser.ClassDeclStmtContext context)
		{
			ClassDeclrsOutput += $"class {EscapeId(context.ID()[0].GetText())}";

			if(context.ID().Count() == 2) {
				ClassDeclrsOutput += $" : {EscapeId(context.ID()[1].GetText())}\n";
			}
			else {
				ClassDeclrsOutput += "\n";
			}

			ClassDeclrsOutput += "{\n";

			InClass = true;
		}

		public override void ExitClassDeclStmt(SwiftParser.ClassDeclStmtContext context)
		{
			ClassDeclrsOutput += "}\n";

			InClass = false;
		}
	}
}
