using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dark.Io.Msg
{
    //定義接口
    public interface IToBytes
    {
         byte[] GetBytes();
    }

    public class Writer
    {
        protected Object mutex = null;
        protected Dark.Bytes.Buffer stream;
        public Writer(int capacity = 1024/*分塊長度*/, Object mutex = null/*不為null 使用 mutex 啟用 線程安全*/)
        {
            stream = new Dark.Bytes.Buffer(capacity);
            this.mutex = mutex;
        }
        //写入 bytes
        public int Write(byte[] bytes, int start, int n)
        {
            if (mutex == null)
            {
                return stream.Write(bytes, start, n);
            }
            lock (mutex)
            {
               return stream.Write(bytes, start, n);
            }
        }
        public int Write(byte[] bytes)
        {
            if (mutex == null)
            {
                return stream.Write(bytes);
            }
            lock (mutex)
            {
                return stream.Write(bytes);
            }
        }
        //写入 IToBytes 接口
        public int Write(IToBytes obj)
        {
            if (obj == null)
            {
                return 0;
            }

            if (mutex == null)
            {
                return stream.Write(obj.GetBytes());
            }
            lock (mutex)
            {
                return stream.Write(obj.GetBytes());
            }
        }
        //寫入 uttf8 字符串
        public int WriteString(string str)
        {
            if (str.Length == 0)
            {
                return 0;
            }

            if (mutex == null)
            {
                return stream.Write(System.Text.Encoding.UTF8.GetBytes(str));
            }

            lock (mutex)
            {
                return stream.Write(System.Text.Encoding.UTF8.GetBytes(str));
            }
        }
        
        //创建一个 message
        public Message CreateMsg(UInt32 id)
        {
            if (mutex == null)
            {
                return UnLockCreateMsg(id);
            }

            lock (mutex)
            {
                return UnLockCreateMsg(id);
            }
        }
        protected Message UnLockCreateMsg(UInt32 id)
        {
            int sum = stream.Len();
            byte[] data = new byte[sum + Message.HEADER_SIZE];

            Header header = new Header();
            header.Flag = Message.HEADER_FLAG;
            header.Id = id;
            header.Size = sum;
            header.GetBytes().CopyTo(data, 0);

            if (sum > 0)
            {
                int offset = (int)Message.HEADER_SIZE;
                stream.Read(data, offset, sum);
            }

            return new Message(data);
        }

    }
}
