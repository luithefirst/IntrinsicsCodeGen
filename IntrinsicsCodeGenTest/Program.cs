using BenchmarkDotNet.Running;
using System;

namespace IntrinsicsCodeGenTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<CodeGenTests>();
        }
    }
}
