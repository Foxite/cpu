using Assembler.Assembly;
using CommandLine;

public class Program {
	public static readonly ProgramAssembler[] Assemblers = {
		ProgramAssemblers.Proc16a,
		ProgramAssemblers.Proc16b,
	};
	
	public static int Main(string[] args) {
		var commandParser = new Parser(settings => {
			settings.HelpWriter = Console.Error;
			settings.EnableDashDash = true;
			settings.CaseInsensitiveEnumValues = true;
		});
		
		return (int) commandParser.ParseArguments<CompileOptions, ListArchitectureOptions>(args)
			.MapResult(
				(CompileOptions opts) => new CompileVerbRunner().Run(opts),
				(ListArchitectureOptions opts) => new ListArchVerbRunner().Run(opts),
				errors => ExitCode.CommandInvalid
			);
	}
}
