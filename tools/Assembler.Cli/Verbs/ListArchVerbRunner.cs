using Assembler.Assembly.V2;
using CommandLine;

[Verb("arch", HelpText = "List available architectures.")]
public class ListArchitectureOptions { }

public class ListArchVerbRunner : VerbRunner<ListArchitectureOptions> {
	public ExitCode Run(ListArchitectureOptions opts) {
		foreach (string architecture in AssemblyContextFactory.SupportedArchitectures) {
			Console.WriteLine(architecture);
		}
		
		return ExitCode.Success;
	}
}
