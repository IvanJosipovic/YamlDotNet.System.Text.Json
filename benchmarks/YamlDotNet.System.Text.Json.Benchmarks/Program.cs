using BenchmarkDotNet.Running;
using YamlBenchmarks;

BenchmarkSwitcher.FromAssembly(typeof(SmallConfigBenchmarks).Assembly).Run(args);
