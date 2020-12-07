using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpWithRust
{
    public static class Rust
    {
        const string DLL = "csharp_with_rust";

        [DllImport(DLL)]
        public static extern long int_add(sbyte i8, short i16, int i32, long i64);
        [DllImport(DLL)]
        public static extern ulong uint_add(byte u8, ushort u16, uint u32, ulong u64);
        [DllImport(DLL)]
        public static extern double float_add(float f32, double f64);

        [DllImport(DLL)]
        public static extern int string_count_ascii([MarshalAs(UnmanagedType.LPStr)] string strPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int string_count_ascii_with_null([MarshalAs(UnmanagedType.LPStr)] string strPtr);
        [DllImport(DLL)]
        public static extern int string_count_utf8([MarshalAs(UnmanagedType.LPUTF8Str)] string strPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int string_count_utf8_with_null([MarshalAs(UnmanagedType.LPUTF8Str)] string strPtr);
        [DllImport(DLL)]
        public static extern int string_count_utf16([MarshalAs(UnmanagedType.LPWStr)] string strPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int string_count_utf16_with_null([MarshalAs(UnmanagedType.LPWStr)] string strPtr);

        [DllImport(DLL)]
        public static extern int get_string_ascii([In][Out] byte[] bufPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int get_string_utf8([In][Out] byte[] bufPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int get_string_utf16([In][Out] byte[] bufPtr, int lenBytes);

        public enum EnumType : int
        {
            None,
            First,
            Second,
        }

        [DllImport(DLL)]
        public static extern void get_values_via_out(out int i32, out EnumType e1, out EnumType e2);
        [DllImport(DLL)]
        public static extern int get_bytes_with_index_as_value_ptr([In][Out] byte[] bufPtr, int lenBytes);
        [DllImport(DLL)]
        public static extern int get_bytes_with_index_as_value_buf_copy([In][Out] byte[] bufPtr, int lenBytes);

        [DllImport(DLL)]
        public static extern void call_callback(Action callback);
        public delegate void CallbackWithUTF8String([MarshalAs(UnmanagedType.LPUTF8Str)] string str);
        [DllImport(DLL)]
        public static extern void call_callback_with_utf8_string(CallbackWithUTF8String callback);

        [DllImport(DLL)]
        public static extern IntPtr create_rust_obj();
        [DllImport(DLL)]
        public static extern void add_rust_obj_value(IntPtr rustObjPtr, int addition);
        [DllImport(DLL)]
        public static extern int get_rust_obj_value(IntPtr rustObjPtr);
        [DllImport(DLL)]
        public static extern int dispose_rust_obj(IntPtr rustObjPtr);

        [StructLayout(LayoutKind.Sequential)]
        public struct Strukt
        {
            public int Num;
        }

        [DllImport(DLL)]
        public static extern Strukt double_struct_value(Strukt strukt);
    }

    [TestClass]
    public class RustTest
    {
        [TestMethod]
        public void NumberFunctionsShouldWork()
        {
            Assert.AreEqual(1 + 2 + 3 - 10, Rust.int_add(1, 2, 3, -10));
            Assert.AreEqual((ulong)(1 + 2 + 3 + 10), Rust.uint_add(1, 2, 3, 10));
            Assert.AreEqual(1.1 - 10.1, Rust.float_add(1.1f, -10.1), 0.001);
        }

        [TestMethod]
        public void StringFunctionsShouldWork()
        {
            var asciistr = "Hello, Rust!";
            Assert.AreEqual(asciistr.Length, Rust.string_count_ascii(asciistr, Encoding.ASCII.GetByteCount(asciistr)));
            Assert.AreEqual(asciistr.Length, Rust.string_count_ascii_with_null(asciistr));

            var unistr = "Hello, Rust! 日本語";
            Assert.AreEqual(unistr.Length, Rust.string_count_utf8(unistr, Encoding.UTF8.GetByteCount(unistr)));
            Assert.AreEqual(unistr.Length, Rust.string_count_utf8_with_null(unistr));
            Assert.AreEqual(unistr.Length, Rust.string_count_utf16(unistr, Encoding.Unicode.GetByteCount(unistr)));
            Assert.AreEqual(unistr.Length, Rust.string_count_utf16_with_null(unistr));
        }

        [TestMethod]
        public void ReturnStringFunctionsShouldWork()
        {
            var buffer = new byte[100];

            var asciistr = "Hello, C#!";
            var len = Rust.get_string_ascii(buffer, buffer.Length);
            Assert.AreEqual(asciistr, Encoding.ASCII.GetString(buffer, 0, len));

            var unistr = "Hello, C#! 日本語";
            len = Rust.get_string_utf8(buffer, buffer.Length);
            Assert.AreEqual(unistr, Encoding.UTF8.GetString(buffer, 0, len));
            len = Rust.get_string_utf16(buffer, buffer.Length);
            Assert.AreEqual(unistr, Encoding.Unicode.GetString(buffer, 0, len));
        }

        [TestMethod]
        public void CanGetValuesViaOut()
        {
            Rust.get_values_via_out(out var i32, out var e1, out var e2);
            Assert.AreEqual(123, i32);
            Assert.AreEqual(Rust.EnumType.First, e1);
            Assert.AreEqual(Rust.EnumType.Second, e2);
        }

        [TestMethod]
        public void CanGetBytesViaArgument()
        {
            {
                var byteArray = new byte[100];
                var len = Rust.get_bytes_with_index_as_value_ptr(byteArray, byteArray.Length);
                Assert.AreEqual(10, len);
                CollectionAssert.AreEqual(Enumerable.Range(0, len).Select(v => (byte)v).ToArray(), byteArray.Take(len).ToArray());
            }

            {
                var byteArray = new byte[100];
                var len = Rust.get_bytes_with_index_as_value_buf_copy(byteArray, byteArray.Length);
                Assert.AreEqual(10, len);
                CollectionAssert.AreEqual(Enumerable.Range(0, len).Select(v => (byte)v).ToArray(), byteArray.Take(len).ToArray());
            }
        }

        [TestMethod]
        public void CanCallCallback()
        {
            var called = false;
            Rust.call_callback(() => called = true);
            Assert.IsTrue(called);

            string str = null;
            Rust.call_callback_with_utf8_string(s => str = s);
            Assert.AreEqual("this is callback. これはコールバックです。", str);
        }

        [TestMethod]
        public void CanManipulateRustObjectByPointer()
        {
            var ptr = Rust.create_rust_obj();
            Assert.AreEqual(100, Rust.get_rust_obj_value(ptr));

            Rust.add_rust_obj_value(ptr, 100);
            Assert.AreEqual(200, Rust.get_rust_obj_value(ptr));

            Rust.dispose_rust_obj(ptr);
        }

        [TestMethod]
        public void CanManipulateStructPassedAsValue()
        {
            var strukt = new Rust.Strukt() { Num = 100 };
            var addedStrukt = Rust.double_struct_value(strukt);
            Assert.AreEqual(200, addedStrukt.Num);
        }
    }
}
