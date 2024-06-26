using Assembler.Assembly;
using Assembler.Assembly.V2;
using Assembler.Parsing;
using Assembler.Parsing.Antlr;
using Assembler.Ast;
using CommandLine;

public enum CompileOutputMode {
	Hex16,
	Raw,
}

[Verb("compile", HelpText = "Compile an assembly file.")]
public class CompileOptions {
	[Option('t', "trace", Required = false, HelpText = "Output trace logging messages.")]
	public bool TraceLogging { get; set; }
	
	[Option('l', "lines", Required = false, HelpText = "For every instruction in the main source file, output a line that maps the instruction's line number to its position in the machine code output.")]
	public bool LinePositions { get; set; }
	
	[Option('a', "arch", Required = true, HelpText = "Choose architecture")]
	public string Architecture { get; set; }
	
	[Option('m', "output-mode", Default = CompileOutputMode.Hex16, HelpText = "Choose output mode.")]
	public CompileOutputMode OutputMode { get; set; }
	
	[Option('p', "macro-path", Separator = ',', Default = null, HelpText = "Paths to search for macro definitions.")]
	public IEnumerable<string> MacroPath { get; set; }
	
	[Option('s', "symbol", Separator = ',', Default = null, HelpText = "Symbols defined globally in the program. key=value")]
	public IEnumerable<string> GlobalSymbols { get; set; }
	
	[Option('o', "output", Default = "-", HelpText = "Filename to output, or - to output to standard output.")]
	public string Output { get; set; }
	
	[Value(0, Default = "-", HelpText = "Input filename, or - to read from standard input.")]
	public string Input { get; set; }
}


public class CompileVerbRunner : VerbRunner<CompileOptions> {
	public ExitCode Run(CompileOptions opts) {
		if (!AssemblyContextFactory.ArchitectureIsSupported(opts.Architecture)) {
			Console.Error.WriteLine($"Architecture {opts.Architecture} is not recognized.");
			return ExitCode.CommandInvalid;
		}
		
		string sourceCode;
		string fullInputPath;
		
		try {
			if (opts.Input == "-") {
				fullInputPath = opts.Input;
				sourceCode = Console.In.ReadToEnd();
			} else {
				fullInputPath = Path.GetFullPath(opts.Input);
				using TextReader readSourceCode = File.OpenText(fullInputPath);
				sourceCode = readSourceCode.ReadToEnd();
			}
		} catch (IOException e) {
			Console.Error.WriteLine("Error reading input: " + e.Message);
			return ExitCode.CompileFileReadError;
		}

		var parser = new ProcAssemblyParser();
		AssemblerProgram program;
		try {
			program = new AssemblerProgram("main", fullInputPath, parser.Parse(opts.Input, sourceCode));
		} catch (ParserException) {
			//Console.Error.WriteLine(e.ToString());
			return ExitCode.CompileParseError;
		}

		var globalSymbols = new Dictionary<string, InstructionArgumentAst>(opts.GlobalSymbols.Select(item => {
			int split = item.IndexOf('=');
			string name = item[..split];
			string value = item[(split + 1)..];

			return new KeyValuePair<string, InstructionArgumentAst>(name, parser.ParseSymbolValue("Global symbol definitions", value));
		}));

		var macroProvider = new FileMacroProvider(parser, opts.MacroPath.ToArray());
		
		IReadOnlyList<ushort> machineCode;
		try {
			var assembler = new ProgramAssemblerv2();
			var contextFactory = AssemblyContextFactory.CreateFactory(macroProvider, opts.Architecture);
			var context = contextFactory.CreateContext(globalSymbols, assembler, 0, opts.TraceLogging, opts.LinePositions);
			machineCode = assembler.AssembleAst(context, program);
		} catch (InvalidProcAssemblyProgramException ex) {
			Console.Error.WriteLine(ex.Instructions.Count);
			if (ex.Instructions.Count == 0) {
				throw;
			}
			
			Console.Error.WriteLine("Unsupported statements:");
			foreach (InvalidInstruction invalidInstruction in ex.Instructions) {
				Console.Error.WriteLine($"{invalidInstruction.Instruction.File}:{invalidInstruction.Instruction.LineNumber}:{invalidInstruction.Instruction.Column} {invalidInstruction.Instruction}: {invalidInstruction.Message}");
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
