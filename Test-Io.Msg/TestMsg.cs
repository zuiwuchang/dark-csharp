using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dark.Io.Msg;
namespace Test_Io.Msg
{
    /// <summary>
    /// TestMsg 的摘要说明
    /// </summary>
    [TestClass]
    public class TestMsg
    {
        public TestMsg()
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
        public void TestMethodMessage()
        {
            //write
            Writer writer = new Writer(10);
            string str1 = "welcome to cerberus's server";
            string str2 = "this is cerberus's server";
            Assert.IsTrue(writer.WriteString(str1) == str1.Length);
            Assert.IsTrue(writer.WriteString(str2) == str2.Length);

            Message msg = writer.CreateMsg(1);
            Assert.IsNotNull(msg);

            Header header = msg.GetHeader();
            byte[] b = msg.GetData();
            string str = System.Text.Encoding.UTF8.GetString(b, (int)MessageConst.HEADER_SIZE, (int)header.Size);
            Assert.IsTrue(str == str1 + str2);

            Assert.IsTrue(header.Id == 1 &&
                header.Flag == MessageConst.HEADER_FLAG &&
                header.Size == str1.Length + str2.Length);
           
            //read
            Reader reader = new Reader(10);
            Assert.IsTrue(reader.Write(b) == b.Length);
            




            //write 2
            Assert.IsTrue(writer.WriteString(str1) == str1.Length);
            msg = writer.CreateMsg(2);
            header = msg.GetHeader();
            b = msg.GetData();
            str = System.Text.Encoding.UTF8.GetString(b, (int)MessageConst.HEADER_SIZE, (int)header.Size);
            Assert.IsTrue(str == str1);

            Assert.IsTrue(header.Id == 2 &&
                header.Flag == MessageConst.HEADER_FLAG &&
                header.Size == str1.Length);

            //read 2
            Assert.IsTrue(reader.Write(b) == b.Length);

            
            
            Message m_0 = reader.GetMsg();
            Assert.IsNotNull(m_0);

            header = m_0.GetHeader();
            b = m_0.GetData();
            str = System.Text.Encoding.UTF8.GetString(b, (int)MessageConst.HEADER_SIZE, (int)header.Size);
            Assert.IsTrue(str == str1 + str2);

            Assert.IsTrue(header.Id == 1 &&
                header.Flag == MessageConst.HEADER_FLAG &&
                header.Size == str1.Length + str2.Length);



            Message m_1 = reader.GetMsg();
            Assert.IsNotNull(m_1);


            header = m_1.GetHeader();
            b = m_1.GetData();
            str = System.Text.Encoding.UTF8.GetString(b, (int)MessageConst.HEADER_SIZE, (int)header.Size);
            Assert.IsTrue(str == str1);

            Assert.IsTrue(header.Id == 2 &&
                header.Flag == MessageConst.HEADER_FLAG &&
                header.Size == str1.Length);



            Message m_2 = reader.GetMsg();
            Assert.IsNull(m_2);
        }
    }
}
