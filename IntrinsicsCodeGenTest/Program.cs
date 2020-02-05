using BenchmarkDotNet.Running;
using System;
using System.Runtime.InteropServices;

namespace IntrinsicsCodeGenTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<ConstructorBench>();
            BenchmarkRunner.Run<CodeGenTests>();
        }
    }
}
