using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dark.Io.Msg
{
    public class Reader
    {
        protected Object mutex = null;
        protected Dark.Bytes.Buffer stream;
        public Reader(int capacity = 1024/*分塊長度*/, Object mutex = null/*不為null 使用 mutex 啟用 線程安全*/)
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
        //從 緩衝區中 解析一個 message
        //沒有一個完整的 message 返回 null
        public Message GetMsg()
        {
            if (mutex == null)
            {
                return UnLockGetMsg();
            }

            lock (mutex)
            {
                return UnLockGetMsg();
            }
        }
        protected Message UnLockGetMsg()
        {
            if (stream.Len() < Message.HEADER_SIZE)
            {
                return null;
            }
            byte[] bytes = new byte[Message.HEADER_SIZE];
            stream.CopyTo(bytes);
            int size = BitConverter.ToInt32(bytes, Message.HEADER_SIZE_OFFSET);
            if (stream.Len() < size + Message.HEADER_SIZE)
            {
                return null;
            }
            bytes = new byte[size + Message.HEADER_SIZE];
            stream.Read(bytes);
            return new Message(bytes);
        }
    }
}
