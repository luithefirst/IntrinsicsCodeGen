using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IntrinsicsCodeGenTest
{
    [StructLayout(LayoutKind.Explicit)]
    //[SkipLocalsInit]
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
            //Unsafe.SkipInit(out var _vec); // .Net 5
            //Unsafe.SkipInit(out var _vec128); // .Net 5
            //_vec = default(System.Numerics.Vector4);
            //_vec128 = default(Vector128<float>);
            this = default;
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
    }

    //BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
    //Intel Core i7-8700K CPU 3.70GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    //.NET Core SDK = 5.0.100

    // [Host]     : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
    //  Job-ZTUXSF : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT

    //Runtime=.NET Core 3.1

    //|                 Method |     Mean |     Error |    StdDev |
    //|----------------------- |---------:|----------:|----------:|
    //|   Vec4Length_Reference | 1.630 ms | 0.0177 ms | 0.0157 ms |
    //|      Vec4Length_Sse_V1 | 1.222 ms | 0.0198 ms | 0.0185 ms |
    //|      Vec4Length_Sse_V2 | 1.313 ms | 0.0140 ms | 0.0124 ms |
    //|      Vec4Length_Sse_V3 | 1.427 ms | 0.0197 ms | 0.0184 ms |
    //|   Vec4Length_Sse_Array | 1.094 ms | 0.0117 ms | 0.0109 ms |
    //| Vec4Length_NumericsVec | 1.116 ms | 0.0060 ms | 0.0053 ms |
    //|   Vec4Length_Vector128 | 1.443 ms | 0.0059 ms | 0.0055 ms |
    //|  Vec4Length_Sse_V4Safe | 1.341 ms | 0.0079 ms | 0.0074 ms |

    // NOTE: there seems to be a minor regression in Vec4Length_Reference when comparing to my original results here: https://github.com/dotnet/runtime/issues/31692

    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [DisassemblyDiagnoser(printAsm: true, printSource: true)]
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
    }
}
