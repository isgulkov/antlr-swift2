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
	}
}
