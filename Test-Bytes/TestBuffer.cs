﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_Bytes
{
    /// <summary>
    /// TestBuffer 的摘要说明
    /// </summary>
    [TestClass]
    public class TestBuffer
    {
        public TestBuffer()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性:
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethodBuffer()
        {
            //
            // TODO: 在此处添加测试逻辑
            //
            Dark.Bytes.Buffer buf = new Dark.Bytes.Buffer(8);
            string str = "0123456789";
            byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
            Assert.IsTrue(buf.Write(b) == b.Length);
            Assert.IsTrue(buf.Len() == b.Length);

            byte[] copy = new byte[str.Length];
            Assert.IsTrue(copy.Length == buf.CopyTo(copy));
            Assert.IsTrue(System.Text.Encoding.UTF8.GetString(copy) == str);

            byte[] bytes = new byte[str.Length];
            int offset = 0;
            int len = bytes.Length;
            while (true)
            {
                int need = 3;
                if (need > len)
                {
                    need = len;
                }

                int n = buf.Read(bytes, offset, need);
                if (n < 1)
                {
                    Assert.IsTrue(System.Text.Encoding.UTF8.GetString(bytes) == str);
                    break;
                }
                offset += n;
                len -= n;
            }

            Assert.IsTrue(buf.Len() == 0);

            //test copy
            {
                buf = new Dark.Bytes.Buffer(8);
                str = "0123456789abcdefghijklmnopqrstwxz";
                b = System.Text.Encoding.UTF8.GetBytes(str);
                Assert.IsTrue(buf.Write(b) == b.Length);

                b = new byte[b.Length];
                buf.CopyTo(b);
                Assert.IsTrue(System.Text.Encoding.UTF8.GetString(b) == str);

               
                int n= buf.CopyTo(3,b);
                Assert.IsTrue(System.Text.Encoding.UTF8.GetString(b,0,n) == str.Substring(3,str.Length-3));

            }
        }
    }
}
