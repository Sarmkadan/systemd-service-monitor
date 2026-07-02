using BenchmarkDotNet.Running;
using SystemdServiceMonitor.Benchmarks;

var summary = BenchmarkRunner.Run<ServiceRepositoryBenchmarks>();
