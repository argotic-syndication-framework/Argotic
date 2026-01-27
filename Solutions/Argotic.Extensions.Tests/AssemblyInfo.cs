

// Configure test parallelization (required for MSTest 4.0 to avoid MSTEST0001 warning)
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
