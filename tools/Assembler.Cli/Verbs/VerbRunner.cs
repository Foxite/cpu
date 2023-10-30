public interface VerbRunner<in T> {
	public ExitCode Run(T opts);
}
