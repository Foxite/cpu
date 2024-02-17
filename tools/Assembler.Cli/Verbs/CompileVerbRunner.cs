using Assembler.Assembly;
using Assembler.Parsing;
using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;
using CommandLine;

public enum CompileOutputMode {
	Hex16,
	Raw,
}

public enum ParserSelection {
	Csly,
	Antlr,
}

[Verb("compile", HelpText = "Compile an assembly file.")]
public class CompileOptions {
	[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
	public bool Verbose { get; set; }
	
	[Option('a', "arch", Required = true, HelpText = "Choose architecture")]
	public string Architecture { get; set; }
	
	[Option('m', "output-mode", Default = CompileOutputMode.Hex16, HelpText = "Choose output mode.")]
	public CompileOutputMode OutputMode { get; set; }
	
	[Option('p', "parser", Default = ParserSelection.Csly, HelpText = "Choose parser.")]
	public ParserSelection Parser { get; set; }
	
	[Option('o', "output", Default = "-", HelpText = "Filename to output, or - to output to standard output.")]
	public string Output { get; set; }
	
	[Value(0, Default = "-", HelpText = "Input filename, or - to read from standard input.")]
	public string Input { get; set; }
}


public class CompileVerbRunner : VerbRunner<CompileOptions> {
	public ExitCode Run(CompileOptions opts) {
		ProgramAssembler? assembler = Program.Assemblers.FirstOrDefault(assembler => assembler.ArchitectureName.ToLower() == opts.Architecture);

		if (assembler == null) {
			Console.Error.WriteLine($"Architecture {opts.Architecture} is not recognized.");
			return ExitCode.CommandInvalid;
		}
		
		string sourceCode;
		
		try {
			if (opts.Input == "-") {
				sourceCode = Console.In.ReadToEnd();
			} else {
				using TextReader readSourceCode = File.OpenText(opts.Input);
				sourceCode = readSourceCode.ReadToEnd();
			}
		} catch (IOException e) {
			Console.Error.WriteLine("Error reading input: " + e.Message);
			return ExitCode.CompileFileReadError;
		}


		var parser = new ProcAssemblyParser();

		ProgramAst program;
		try {
			program = parser.Parse(sourceCode);
		} catch (ParserException e) {
			//Console.Error.WriteLine(e.ToString());
			return ExitCode.CompileParseError;
		}

		IEnumerable<ushort> machineCode;

		try {
			machineCode = assembler.Assemble(program);
		} catch (InvalidProcAssemblyProgramException ex) {
			Console.Error.WriteLine("Unsupported statements:");
			foreach (InvalidInstruction invalidInstruction in ex.Instructions) {
				Console.WriteLine($"Statement {invalidInstruction.Index}: {invalidInstruction.Instruction}: {invalidInstruction.Message}");
			}
			
			return ExitCode.ProgramNotSupported;
		}

		try {
			using Stream output = opts.Output == "-" ? Console.OpenStandardOutput() : new FileStream(opts.Output, FileMode.Create);

			if (opts.OutputMode == CompileOutputMode.Raw) {
				foreach (ushort word in machineCode) {
					byte msbyte = (byte) (word >> 8);
					byte lsbyte = (byte) (word & 0xFF);

					output.WriteByte(msbyte);
					output.WriteByte(lsbyte);
				}
			} else if (opts.OutputMode == CompileOutputMode.Hex16) {
				using var outputWriter = new StreamWriter(output);
				
				foreach (ushort word in machineCode) {
					outputWriter.WriteLine($"0x{word:X4}");
				}
			}
		} catch (IOException e) {
			Console.Error.WriteLine("Error writing output: " + e.Message);
			return ExitCode.CompileFileWriteError;
		}

		return ExitCode.Success;
	}
}
