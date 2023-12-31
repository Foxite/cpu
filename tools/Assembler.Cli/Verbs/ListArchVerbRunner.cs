using Assembler.Assembly;
using CommandLine;

[Verb("arch", HelpText = "List available architectures.")]
public class ListArchitectureOptions { }

public class ListArchVerbRunner : VerbRunner<ListArchitectureOptions> {
	public ExitCode Run(ListArchitectureOptions opts) {
		foreach (ProgramAssembler assembler in Program.Assemblers) {
			Console.WriteLine(assembler.ArchitectureName);
		}
		
		return ExitCode.Success;
	}
}
