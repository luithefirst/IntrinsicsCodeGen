﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IntrinsicsCodeGenTest
{
    [StructLayout(LayoutKind.Explicit)]
    [SkipLocalsInit] // ??
    struct MyVector4
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Z;
        [FieldOffset(12)]
        public float W;

        [FieldOffset(0)]
        private System.Numerics.Vector4 _vec;
        [FieldOffset(0)]
        private Vector128<float> _vec128;


        public MyVector4(float x, float y, float z, float w) : this()
        {
            //unsafe
            //{
            //    fixed (MyVector4* pThis = &this)
            //    {
            //        // This skips the C# definite assignment rule that all fields of the struct
            //        // must be assigned before the constructor exits.
            //    }
            //}
            Unsafe.SkipInit(out _vec); // .Net 5
            Unsafe.SkipInit(out _vec128); // .Net 5
            //_vec = default(System.Numerics.Vector4);
            //_vec128 = default(Vector128<float>);
            //this = default;
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public readonly float VX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _vec128.GetElement(0); }
        }

        public readonly float VY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _vec128.GetElement(1); }
        }

        public readonly float VZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _vec128.GetElement(2); }
        }

        public readonly float VW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _vec128.GetElement(3); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector4 Create(float x, float y, float z, float w)
        {
            var vec = new MyVector4();
            vec.X = x;
            vec.Y = y;
            vec.Z = z;
            vec.W = w;
            return vec;
        }

        public readonly float Length_Ref
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(X * X + Y * Y + Z * Z + W * W); }
        }

        public readonly float Length_Vector128Prop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(VX * VX + VY * VY + VZ * VZ + VW * VW); }
        }

        public readonly float Length_Vector4Prop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(_vec.X * _vec.X + _vec.Y * _vec.Y + _vec.Z * _vec.Z + _vec.W * _vec.W); }
        }

        public readonly float Length_Sse_V1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    fixed (MyVector4* pthis = &this)
                    {
                        var mmx = Sse.LoadVector128((float*)pthis);
                        mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                        var l2 = mmx.GetElement(0);
                        return MathF.Sqrt(l2);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float Length_Sse_V2_Helper(MyVector4 vec)
        {
            var ptr = (float*)&vec;
            var mmx = Sse.LoadVector128(ptr);
            mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
            var l2 = mmx.GetElement(0);
            return MathF.Sqrt(l2);
        }

        public readonly float Length_Sse_V2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Length_Sse_V2_Helper(this); }
        }

        public readonly float Length_Sse_V3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                unsafe
                {
                    var vec = this;
                    var ptr = (float*)&vec;
                    var mmx = Sse.LoadVector128(ptr);
                    mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                    var l2 = mmx.GetElement(0);
                    return MathF.Sqrt(l2);
                }
            }
        }

        public readonly float Length_NumericsVec
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _vec.Length(); }
        }

        public readonly float Length_Vector128
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var mmx = Sse41.DotProduct(_vec128, _vec128, 0xF1);
                var l2 = mmx.GetElement(0);
                return MathF.Sqrt(l2);
            }
        }

        public readonly float Length_Sse_V4Safe
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var mmx = Vector128.Create(X, Y, Z, W);
                mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                var l2 = mmx.GetElement(0);
                return MathF.Sqrt(l2);
            }
        }

        public readonly float Length_Sse_V5
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    fixed (void* pthis = &this)
                    {
                        var mmx = *((Vector128<float>*)pthis);
                        mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                        var l2 = mmx.GetElement(0);
                        return MathF.Sqrt(l2);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float Length_Sse_V6_Helper(MyVector4 vec)
        {
            var mmx = *((Vector128<float>*)&vec);
            mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
            var l2 = mmx.GetElement(0);
            return MathF.Sqrt(l2);
        }

        public readonly float Length_Sse_V6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Length_Sse_V6_Helper(this); }
        }

        public readonly float Length_Sse_V7
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                unsafe
                {
                    var vec = this;
                    var mmx = *((Vector128<float>*)&vec);
                    mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                    var l2 = mmx.GetElement(0);
                    return MathF.Sqrt(l2);
                }
            }
        }
    }

    //BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
    //Intel Core i7-8700K CPU 3.70GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    //.NET Core SDK = 5.0.100

    // [Host]     : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
    //  Job-POFHKZ : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT

    //Runtime=.NET Core 3.1

    //|                   Method |     Mean |     Error |    StdDev |
    //|------------------------- |---------:|----------:|----------:|
    //|     Vec4Length_Reference | 1.553 ms | 0.0137 ms | 0.0122 ms |
    //|   Vec4Length_Vector4Prop | 1.615 ms | 0.0073 ms | 0.0069 ms |
    //| Vec4Length_Vector128Prop | 2.839 ms | 0.0496 ms | 0.0464 ms |
    //|        Vec4Length_Sse_V1 | 1.210 ms | 0.0161 ms | 0.0150 ms |
    //|        Vec4Length_Sse_V2 | 1.338 ms | 0.0253 ms | 0.0236 ms |
    //|        Vec4Length_Sse_V3 | 1.409 ms | 0.0087 ms | 0.0077 ms |
    //|     Vec4Length_Sse_Array | 1.087 ms | 0.0073 ms | 0.0061 ms |
    //|   Vec4Length_NumericsVec | 1.113 ms | 0.0040 ms | 0.0038 ms | <- best performance, but introduces overahead due to overlapping field with redundant intialaization (can be avoided in .net 5)
    //|     Vec4Length_Vector128 | 1.439 ms | 0.0043 ms | 0.0040 ms |
    //|    Vec4Length_Sse_V4Safe | 1.332 ms | 0.0040 ms | 0.0036 ms |
    //|        Vec4Length_Sse_V5 | 1.200 ms | 0.0092 ms | 0.0086 ms |
    //|        Vec4Length_Sse_V6 | 1.285 ms | 0.0056 ms | 0.0052 ms |

    // NOTE: there seems to be a minor regression in Vec4Length_Reference when comparing to my original results here: https://github.com/dotnet/runtime/issues/31692


    //BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.630 (2004/?/20H1)
    //Intel Core i7-8700K CPU 3.70GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    //.NET Core SDK = 5.0.100

    // [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
    // .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT

    //Job=.NET Core 5.0  Runtime=.NET Core 5.0

    //|                   Method |     Mean |     Error |    StdDev | Code Size |
    //|------------------------- |---------:|----------:|----------:|----------:|
    //|     Vec4Length_Reference | 1.620 ms | 0.0039 ms | 0.0035 ms |     105 B |
    //|   Vec4Length_Vector4Prop | 1.555 ms | 0.0081 ms | 0.0072 ms |     105 B |
    //| Vec4Length_Vector128Prop | 2.809 ms | 0.0064 ms | 0.0060 ms |     189 B |
    //|        Vec4Length_Sse_V1 | 1.170 ms | 0.0061 ms | 0.0051 ms |      80 B |
    //|        Vec4Length_Sse_V2 | 1.337 ms | 0.0107 ms | 0.0094 ms |      82 B |
    //|        Vec4Length_Sse_V3 | 1.299 ms | 0.0043 ms | 0.0036 ms |      94 B |
    //|     Vec4Length_Sse_Array | 1.106 ms | 0.0215 ms | 0.0191 ms |     122 B |
    //|   Vec4Length_NumericsVec | 1.454 ms | 0.0077 ms | 0.0072 ms |      58 B |
    //|     Vec4Length_Vector128 | 1.458 ms | 0.0147 ms | 0.0130 ms |      58 B |
    //|    Vec4Length_Sse_V4Safe | 1.353 ms | 0.0150 ms | 0.0133 ms |      94 B |
    //|        Vec4Length_Sse_V5 | 1.182 ms | 0.0155 ms | 0.0145 ms |      80 B |
    //|        Vec4Length_Sse_V6 | 1.296 ms | 0.0070 ms | 0.0055 ms |      78 B |


    //|                           Method |     Mean |     Error |    StdDev |
    //|--------------------------------- |---------:|----------:|----------:|
    //|     Vec4Length_Reference (3.1.0) | 1.451 ms | 0.0046 ms | 0.0041 ms |
    //|     Vec4Length_Reference (3.1.9) | 1.553 ms | 0.0137 ms | 0.0122 ms |
    //|     Vec4Length_Reference (5.0.0) | 1.620 ms | 0.0039 ms | 0.0035 ms |
    //|        Vec4Length_Sse_V1 (3.1.0) | 1.191 ms | 0.0052 ms | 0.0049 ms |
    //|        Vec4Length_Sse_V1 (3.1.9) | 1.210 ms | 0.0161 ms | 0.0150 ms |
    //|        Vec4Length_Sse_V1 (5.0.0) | 1.170 ms | 0.0061 ms | 0.0051 ms |
    //|   Vec4Length_NumericsVec (3.1.9) | 1.113 ms | 0.0040 ms | 0.0038 ms |
    //|   Vec4Length_NumericsVec (5.0.0) | 1.454 ms | 0.0077 ms | 0.0072 ms |



    // UPDATE:
    //
    //BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4291/22H2/2022Update)
    //Intel Core i7-8700K CPU 3.70GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    //.NET SDK 8.0.204
    //  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
    //  DefaultJob : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2


    //| Method                   | Mean     | Error     | StdDev    | Code Size |
    //|------------------------- |---------:|----------:|----------:|----------:|
    //| Vec4Length_Reference     | 1.506 ms | 0.0252 ms | 0.0224 ms |      98 B |
    //| Vec4Length_Vector4Prop   | 1.508 ms | 0.0222 ms | 0.0197 ms |      98 B |
    //| Vec4Length_Vector128Prop | 1.677 ms | 0.0176 ms | 0.0156 ms |      97 B |
    //| Vec4Length_Sse_V1        | 1.305 ms | 0.0063 ms | 0.0059 ms |      89 B |
    //| Vec4Length_Sse_V2        | 1.229 ms | 0.0189 ms | 0.0167 ms |      75 B |
    //| Vec4Length_Sse_V3        | 1.227 ms | 0.0222 ms | 0.0296 ms |      87 B |
    //| Vec4Length_Sse_Array     | 1.139 ms | 0.0213 ms | 0.0188 ms |     122 B |
    //| Vec4Length_NumericsVec   | 1.137 ms | 0.0174 ms | 0.0162 ms |      55 B |
    //| Vec4Length_Vector128     | 1.162 ms | 0.0081 ms | 0.0076 ms |      58 B |
    //| Vec4Length_Sse_V4Safe    | 1.116 ms | 0.0073 ms | 0.0065 ms |      55 B |
    //| Vec4Length_Sse_V5        | 1.303 ms | 0.0086 ms | 0.0072 ms |      89 B |
    //| Vec4Length_Sse_V6        | 1.117 ms | 0.0049 ms | 0.0046 ms |      55 B |
    //| Vec4Length_Sse_V7        | 1.125 ms | 0.0131 ms | 0.0129 ms |      55 B |

    [DisassemblyDiagnoser(printSource: true)]
    public class CodeGenTests
    {
        MyVector4[] arr = new MyVector4[1000000];
        MyVector4_Plain[] arr2 = new MyVector4_Plain[1000000];
        float test;

        public CodeGenTests()
        {
            var rnd1 = new Random(1);
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new MyVector4((float)rnd1.NextDouble(), (float)rnd1.NextDouble(), (float)rnd1.NextDouble(), (float)rnd1.NextDouble());
            test = Vec4Length_Reference();
        }

        [Benchmark]
        public float Vec4Length_Reference()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Ref;
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Vector4Prop()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Vector4Prop;
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Vector128Prop()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Vector128Prop;
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V1()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V1;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V2()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V2;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V3()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V3;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_Array()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            unsafe
            {
                fixed (MyVector4* ptrArr = local)
                {
                    for (int i = 0; i < cnt; i++)
                    {
                        var mmx = Sse.LoadVector128((float*)(ptrArr + i));
                        mmx = Sse41.DotProduct(mmx, mmx, 0xF1);
                        var l2 = mmx.GetElement(0);
                        sum += MathF.Sqrt(l2);
                    }
                }
            }
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_NumericsVec()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_NumericsVec;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Vector128()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Vector128;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V4Safe()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V4Safe;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V5()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V5;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V6()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V6;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }

        [Benchmark]
        public float Vec4Length_Sse_V7()
        {
            var local = arr;
            var cnt = local.Length;
            var sum = 0.0f;
            for (int i = 0; i < cnt; i++)
                sum += local[i].Length_Sse_V7;
            //if (Math.Abs(sum - test) > 1e-5) throw new Exception("FAIL");
            return sum;
        }
    }
}
