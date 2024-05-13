using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IntrinsicsCodeGenTest
{
    struct MyVector4_Plain
    {
        public float X, Y, Z, W;
        public MyVector4_Plain(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    struct MyVector4_Numerics
    {
        Vector4 _vec;
        public MyVector4_Numerics(float x, float y, float z, float w)
        {
            _vec = new Vector4(x, y, z, w);
        }
    }

    //BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.630 (2004/?/20H1)
    //Intel Core i7-8700K CPU 3.70GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    //.NET Core SDK = 5.0.100

    //[Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
    //.NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT

    //Job=.NET Core 5.0  Runtime=.NET Core 5.0

    //|            Method |     Mean |     Error |    StdDev |
    //|------------------ |---------:|----------:|----------:|
    //|       Constructor | 5.132 ms | 0.0146 ms | 0.0136 ms | <- still significant overhead due to overlapping Numerics.Vector and Vector128
    //|           Creator | 5.132 ms | 0.0182 ms | 0.0170 ms |
    //|         Reference | 1.142 ms | 0.0065 ms | 0.0060 ms |
    //|           Vector4 | 1.845 ms | 0.0057 ms | 0.0048 ms |
    //| Vector4_Reference | 1.878 ms | 0.0247 ms | 0.0231 ms |

    [DisassemblyDiagnoser(printSource: true)]
    public class ConstructorBench
    {
        MyVector4[] arr = new MyVector4[1000000];
        MyVector4_Plain[] arr2 = new MyVector4_Plain[1000000];
        MyVector4_Numerics[] arr3 = new MyVector4_Numerics[1000000];
        Vector4[] arr4 = new Vector4[1000000];


        /// <summary>
        /// Constructor with overhead due to overlapping Numerics.Vector4 and Vector128
        /// </summary>
        [Benchmark]
        public void Constructor()
        {
            var local = arr;
            for (int i = 0; i < local.Length; i++)
                local[i] = new MyVector4(i, i, i, i);
        }

        [Benchmark]
        public void Creator()
        {
            var local = arr;
            for (int i = 0; i < local.Length; i++)
                local[i] = MyVector4.Create(i, i, i, i);
        }

        /// <summary>
        /// Constructor of plain Vector4 struct (Reference)
        /// </summary>
        [Benchmark]
        public void Reference()
        {
            var local = arr2;
            for (int i = 0; i < local.Length; i++)
                local[i] = new MyVector4_Plain(i, i, i, i);
        }

        /// <summary>
        /// Constructor of vector struct that wraps a Numerics.Vector4
        /// </summary>
        [Benchmark]
        public void Vector4()
        {
            var local = arr3;
            for (int i = 0; i < local.Length; i++)
                local[i] = new MyVector4_Numerics(i, i, i, i);
        }

        /// <summary>
        /// Direct construction of a Numerics.Vector4
        /// </summary>
        [Benchmark]
        public void Vector4_Reference()
        {
            var local = arr4;
            for (int i = 0; i < local.Length; i++)
                local[i] = new Vector4(i, i, i, i);
        }
    }
}
