using BenchmarkDotNet.Running;
using EntityFrameworkCore.Performance;

public class Program
{
    public static int Main(string[] args)
    {
        BenchmarkRunner.Run<EntityFrameworkWhereBenchmark>();

        return 0;
    }

}
