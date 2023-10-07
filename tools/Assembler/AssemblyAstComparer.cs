using System.Diagnostics;
using System.Reflection;

namespace Assembler;

[Obsolete]
public class AssemblyAstComparer : IEqualityComparer<IAssemblyAst> {
	public bool Equals(IAssemblyAst? left, IAssemblyAst? right) {
		if ((left == null) != (right == null)) {
			return false;
		}

		if (left == null) {
			return true;
		}

		Debug.Assert(right != null);
		
		if (left.GetType() != right.GetType()) {
			return false;
		}

		foreach (PropertyInfo property in left.GetType().GetProperties()) {
			var leftValue = property.GetValue(left);
			var rightValue = property.GetValue(right);

			if ((leftValue == null) != (rightValue == null)) {
				return false;
			}

			if (leftValue == null) {
				continue;
			}

			Debug.Assert(right != null);

			Type? enumerableType = property.PropertyType
				.GetInterfaces()
				.FirstOrDefault(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			
			if (enumerableType != null) {
				Type enumeratedType = enumerableType.GetGenericArguments()[0];

				bool result;

				var sequenceEquals = typeof(Enumerable).GetMethods().Where(method => method.Name == nameof(Enumerable.SequenceEqual)).ToList();
				var sequenceEqualNoComparer = sequenceEquals.First(method => method.GetParameters().Length == 2).MakeGenericMethod(typeof(IAssemblyAst));
				var sequenceEqualWithComparer = sequenceEquals.First(method => method.GetParameters().Length == 3).MakeGenericMethod(typeof(IAssemblyAst));
				
				if (enumeratedType.IsAssignableTo(typeof(IAssemblyAst))) {
					result = (bool) sequenceEqualWithComparer.Invoke(null, new object?[] { leftValue, rightValue, this })!;
				} else {
					result = (bool) sequenceEqualNoComparer.Invoke(null, new object?[] { leftValue, rightValue })!;
				}

				if (!result) {
					return false;
				}
			} else {
				if (!leftValue.Equals(rightValue)) {
					return false;
				}
			}
		}

		return true;
	}

	public int GetHashCode(IAssemblyAst obj) {
		return 0;
	}
}
