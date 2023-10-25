using Assembler;
using Assembler.Assembly;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;

using CommandLine;

public enum CompileOutputMode {
	Hex16,
	Raw
}

[Verb("compile", HelpText = "Compile an assembly file.")]
public class CompileOptions {
	[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
	public bool Verbose { get; set; }
	
	[Option('a', "arch", Required = true, HelpText = "Choose architecture")]
	public string Architecture { get; set; }
	
	[Option('m', "output-mode", Default = CompileOutputMode.Hex16, HelpText = "Choose output mode.")]
	public CompileOutputMode OutputMode { get; set; }
	
	[Option('o', "output", Default = "-", HelpText = "Filename to output, or - to output to standard output.")]
	public string Output { get; set; }
	
	[Value(0, Default = "-", HelpText = "Input filename, or - to read from standard input.")]
	public string Input { get; set; }
}


public class CompileVerbRunner : VerbRunner<CompileOptions> {
	public ExitCodes Run(CompileOptions opts) {
		ProgramAssembler? assembler = Program.Assemblers.FirstOrDefault(assembler => assembler.ArchitectureName.ToLower() == opts.Architecture);

		if (assembler == null) {
			Console.Error.WriteLine($"Architecture {opts.Architecture} is not recognized.");
			return ExitCodes.CommandInvalid;
		}
		
		var parserDefinition = new AssemblyParser();
		var parserBuilder = new ParserBuilder<AssemblyToken, IAssemblyAst>();

		BuildResult<Parser<AssemblyToken, IAssemblyAst>> buildResult = parserBuilder.BuildParser(parserDefinition, ParserType.EBNF_LL_RECURSIVE_DESCENT, "Program");

		if (buildResult.IsError) {
			Console.Error.WriteLine("Unable to build parser!");
			return ExitCodes.InternalError;
		}
		
		Parser<AssemblyToken, IAssemblyAst> parser = buildResult.Result;
		ParseResult<AssemblyToken, IAssemblyAst> parseResult;
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
			return ExitCodes.CompileFileReadError;
		}
		
		parseResult = parser.Parse(sourceCode);

		if (parseResult.IsError) {
			Console.Error.WriteLine("Syntax error in source file:");
			foreach (var error in parseResult.Errors) {
				Console.Error.WriteLine($"{error.Line}:{error.Column} [{error.ErrorType}] {error.ErrorMessage}");
			}
			return ExitCodes.CompileParseError;
		}

		var program = (ProgramAst) parseResult.Result;

		IEnumerable<ushort> machineCode;

		try {
			machineCode = assembler.Assemble(program);
		} catch (UnsupportedStatementException ex) {
			Console.Error.WriteLine("Unsupported statements:");
			foreach ((IStatement statement, int index) in ex.Statements) {
				Console.WriteLine($"Statement {index}: {statement}");
			}
			
			return ExitCodes.ProgramNotSupported;
		}

		try {
			using Stream output = opts.Output == "-" ? Console.OpenStandardOutput() : File.OpenWrite(opts.Output);

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
			return ExitCodes.CompileFileWriteError;
		}

		return ExitCodes.Success;
	}
}
