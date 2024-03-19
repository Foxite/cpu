using System.Reflection;
using Assembler.Ast;

namespace Assembler.Assembly;

public abstract class InstructionMapInstructionConverter : IInstructionConverter {
	public string Architecture { get; }
	protected Dictionary<string, Instruction> Instructions { get; } = new();
	
	protected InstructionMapInstructionConverter(string architecture) {
		Architecture = architecture;
	}

	public InstructionSupport ValidateInstruction(InstructionAst instructionAst) {
		if (!Instructions.TryGetValue(instructionAst.Mnemonic, out Instruction? instruction)) {
			return InstructionSupport.NotRecognized;
		}
		
		return instruction.Validate(instructionAst.Arguments);
	}
	
	public ushort ConvertInstruction(InstructionAst instructionAst) {
		Instructions.TryGetValue(instructionAst.Mnemonic, out Instruction? instruction);
		
		return instruction!.Convert(instructionAst.Arguments);
	}
	
	protected bool AddInstruction(string term, Instruction instruction) {
		return Instructions.TryAdd(term, instruction);
	}

	protected abstract record Instruction {
		public InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> arguments) {
			List<Type> argumentTypes = arguments.Select(arg => arg.GetType()).ToList();

			MethodInfo? converter = GetType()
				.GetMethods(BindingFlags.Instance)
				.SingleOrDefault(method =>
					method.GetCustomAttribute<ConverterAttribute>() != null &&
					method.GetParameters().Select(param => param.ParameterType).SequenceEqual(argumentTypes)
				);
			
			MethodInfo? validator = GetType()
				.GetMethods(BindingFlags.Instance)
				.SingleOrDefault(method =>
					method.GetParameters().Select(param => param.ParameterType).SequenceEqual(argumentTypes) &&
					method.GetCustomAttribute<ValidatorAttribute>() != null
				);
			
			if (converter == null) {
				return InstructionSupport.ParameterType;
			} else if (validator != null) {
				return (InstructionSupport) validator.Invoke(this, arguments.Cast<object?>().ToArray())!;
			} else {
				return InstructionSupport.Supported;
			}
		}

		public ushort Convert(IReadOnlyList<InstructionArgumentAst> arguments) {
			List<Type> argumentTypes = arguments.Select(arg => arg.GetType()).ToList();

			MethodInfo overload = GetType()
				.GetMethods(BindingFlags.Instance)
				.SingleOrDefault(method =>
					method.GetCustomAttribute<ConverterAttribute>() != null &&
					method.GetParameters().Select(param => param.ParameterType).SequenceEqual(argumentTypes)
				) ?? throw new Exception($"No overload is available. This should have been caught during validation. {GetType().Name} {string.Join(", ", arguments.Select(arg => arg.GetType().Name))}");

			return (ushort) overload.Invoke(this, arguments.Cast<object?>().ToArray())!;
		}

		[AttributeUsage(AttributeTargets.Method)] protected sealed class ValidatorAttribute : Attribute { }
		[AttributeUsage(AttributeTargets.Method)] protected sealed class ConverterAttribute : Attribute { }
	}
}
