using System.Diagnostics;
using Assembler.Assembly;
using CommandLine;

public class Program {
	public static int Main(string[] args) {
		Debugger.Launch();
		
		var commandParser = new Parser(settings => {
			settings.HelpWriter = Console.Error;
			settings.EnableDashDash = true;
			settings.CaseInsensitiveEnumValues = true;
		});

		
		try {
			return (int) commandParser.ParseArguments<CompileOptions, ListArchitectureOptions>(args)
				.MapResult(
					(CompileOptions opts) => new CompileVerbRunner().Run(opts),
					(ListArchitectureOptions opts) => new ListArchVerbRunner().Run(opts),
					errors => ExitCode.CommandInvalid
				);
		} catch (Exception e) {
			Console.Error.WriteLine(e.ToString());
			Console.Error.WriteLine(e.StackTrace);
			return (int) ExitCode.InternalError;
		}
	}
}
