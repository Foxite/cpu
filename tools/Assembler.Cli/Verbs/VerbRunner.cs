public interface VerbRunner<in T> {
	public ExitCodes Run(T opts);
}
