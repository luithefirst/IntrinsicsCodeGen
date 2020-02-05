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


        public MyVector4(float x, float y, float z, float w)
        {
            //Unsafe.SkipInit(out var _vec); // .Net 5
            //Unsafe.SkipInit(out var _vec128); // .Net 5
            _vec = default(System.Numerics.Vector4);
            _vec128 = default(Vector128<float>);
            X = x;
            Y = y;
            Z = z;
            W = w;
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
    }

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
    }
}
