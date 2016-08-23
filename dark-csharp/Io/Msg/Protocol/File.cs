using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dark.Io.Msg.Protocol
{
    public class FileConst
    {
        private FileConst() { }
        //md5 cerberus protocol file transport by dark king
        public static string HASH = "d9bebd0a8b0f0f9b7c2035d92ed79cbf";

        public static UInt32 CMD_FILE_CREATE = 1;
        public static UInt32 CMD_FILE_BINARY = 2;
        public static UInt32 CMD_FILE_OK = 3;

        //send CMD_FILE_CREATE (set file name)

        //send CMD_FILE_BINARY (set file binary)

        //send CMD_FILE_OK (set file binary)
    }
    public class FileWriter : IWriter
    {
        public string Hash()
        {
            return FileConst.HASH;
        }
        protected Dark.Io.Msg.Writer writer;
        protected Object mutex;
        public FileWriter(int capacity = 1024/*分塊長度*/, Object mutex = null/*不為null 使用 mutex 啟用 線程安全*/)
        {
            writer = new Dark.Io.Msg.Writer(capacity);
            this.mutex = mutex;
            writer.WriteString(Hash());
        }

        public int WriteCreate(string name)
        {
            if (mutex == null)
            {
                return UnlockWriteCreate(name);
            }

            lock (mutex)
            {
                return UnlockWriteCreate(name);
            }
        }
        protected int UnlockWriteCreate(string name)
        {
            byte[] bytes = BitConverter.GetBytes(FileConst.CMD_FILE_CREATE);
            int sum = writer.Write(bytes, 0, bytes.Length);
            sum += writer.WriteString(name);
            return sum;
        }

        public int WriteBinaryHeader()
        {
            if (mutex == null)
            {
                return UnLockWriteBinaryHeader();
            }

            lock (mutex)
            {
                return UnLockWriteBinaryHeader();
            }
        }
        protected int UnLockWriteBinaryHeader()
        {
            byte[] bytes = BitConverter.GetBytes(FileConst.CMD_FILE_BINARY);
            return writer.Write(bytes, 0, bytes.Length); 
        }
        public int WriteBinary(byte[] bytes,int start,int n)
        {
            if (mutex == null)
            {
                return writer.Write(bytes, start, n);
            }

            lock (mutex)
            {
                return writer.Write(bytes, start, n);
            }
        }
        public int WriteOk()
        {
            if (mutex == null)
            {
                return UnLockWriteOk();
            }

            lock (mutex)
            {
                return UnLockWriteOk();
            }
        }
        protected int UnLockWriteOk()
        {
            byte[] bytes = BitConverter.GetBytes(FileConst.CMD_FILE_OK);
            return writer.Write(bytes, 0, bytes.Length); 
        }

        public Message CreateMsg(uint id)
        {
            if (mutex == null)
            {
                Dark.Io.Msg.Message msg = writer.CreateMsg(id);

                writer.WriteString(Hash());
                return msg;
            }

            lock (mutex)
            {
                Dark.Io.Msg.Message msg = writer.CreateMsg(id);

                writer.WriteString(Hash());
                return msg;
            }
        }
    }

    public class FileHandler<T> : IHandler<T>
    {

        public string Hash()
        {
            return FileConst.HASH;
        }
        protected System.IO.FileStream fw = null;
        public bool Deal(System.Net.Sockets.Socket s, T info, Message msg)
        {
            byte[] bytes = msg.GetData();
            UInt32 cmd = BitConverter.ToUInt32(bytes, ProtocolConst.BODY_OFFSET);
            if (cmd == FileConst.CMD_FILE_CREATE)
            {
                //獲取文件名
                int offset = ProtocolConst.BODY_OFFSET + 4;
                string name = System.Text.Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);


                Console.WriteLine("CMD_FILE_CREATE {0}", name);
                //關閉未正常關閉文件
                if (fw != null)
                {
                    fw.Close();
                }

                //創建新文件
                fw = System.IO.File.Create(name);
            }
            else if (cmd == FileConst.CMD_FILE_BINARY)
            {
                Console.WriteLine("CMD_FILE_BINARY");
                //寫入文件 數據
                if (fw != null)
                {
                    int offset = ProtocolConst.BODY_OFFSET + 4;
                    fw.Write(bytes, offset, bytes.Length - offset);
                }
            }
            else if (cmd == FileConst.CMD_FILE_OK)
            {
                Console.WriteLine("CMD_FILE_OK");
                //文件傳輸 成功 關閉之
                if (fw != null)
                {
                    fw.Close();
                    fw = null;
                }
            }
            return true;
        }
    }
}
