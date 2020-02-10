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

    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
    public class ConstructorBench
    {
        MyVector4[] arr = new MyVector4[1000000];
        MyVector4_Plain[] arr2 = new MyVector4_Plain[1000000];
        MyVector4_Numerics[] arr3 = new MyVector4_Numerics[1000000];
        Vector4[] arr4 = new Vector4[1000000];

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

        [Benchmark]
        public void Reference()
        {
            var local = arr2;
            for (int i = 0; i < local.Length; i++)
                local[i] = new MyVector4_Plain(i, i, i, i);
        }

        [Benchmark]
        public void Vector4()
        {
            var local = arr3;
            for (int i = 0; i < local.Length; i++)
                local[i] = new MyVector4_Numerics(i, i, i, i);
        }

        [Benchmark]
        public void Vector4_Reference()
        {
            var local = arr4;
            for (int i = 0; i < local.Length; i++)
                local[i] = new Vector4(i, i, i, i);
        }
    }
}
