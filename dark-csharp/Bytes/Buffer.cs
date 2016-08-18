using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dark.Bytes
{
    public class BufferException : Exception
    {
        public BufferException(string msg):base(msg)
        { 
        }
    }

    //一個 可變的 byte 流 緩衝區
    public class Buffer
    {
        protected class Fragmentation
        {
            //分片數據
            protected byte[] data = null;
            //有效數據 偏移位置
            protected int offset = 0;
            //有效數據量
            protected int size = 0;
            public int Size { get { return size; } }

            public Fragmentation(int capacity)
            {
                data = new byte[capacity];
            }
            //返回 空閒 容量
            public int GetFree()
            {
                return data.Length - offset - size;
            }
            //寫入 數據 返回實際寫入 量
            public int Write(byte[] bytes, int start, int n)
            {
                int free = GetFree();
                int need = n;
                if (n > free)
                {
                    need = free;
                }
                Array.Copy(bytes, start, data, offset + size, need);
                size += need;

                return need;
            }
            //讀取 數據 返回實際讀取 量
            //被讀取的 數據 將被 移除 緩衝區
            public int Read(byte[] bytes, int start, int n)
            {
                int need = n;
                if (n > size)
                {
                    need = size;
                }
                Array.Copy(data, offset, bytes, start, need);
                size -= need;
                offset += need;

                return need;
            }
            public int CopyTo(byte[] bytes, int start, int n)
            {
                int need = n;
                if (n > size)
                {
                    need = size;
                }
                Array.Copy(data, offset, bytes, start, need);

                return need;
            }
        }

        protected int capacity;
        protected Object mutex = null;
        protected List<Fragmentation> fragmentations = new List<Fragmentation>();
        public Buffer(int capacity = 1024/*分塊長度*/, Object mutex = null/*不為null 使用 mutex 啟用 線程安全*/)
        {
            this.capacity = capacity;
            this.mutex = mutex;
        }

        //清空緩存 刪除所有數據
        public void Reset()
        {
            fragmentations.Clear();
        }

        //返回 流中 待讀字節數
        public int Len()
        {
            if (mutex == null)
            {
                return UnLockLen();
            }

            lock (mutex)
            {
                return UnLockLen();
            }
        }
        protected int UnLockLen()
        {
            int sum = 0;
            foreach (var item in fragmentations)
            {
                sum += item.Size;
            }
            return sum;
        }

        //將緩衝區 copy 到指定內存 返回實際 copy數據長 throw BufferException
        //被copy的數據 不會從 緩衝區中 刪除
        //如果 n > 緩衝區 數據 將 只copy 緩衝區 否則 copy n 字節數據
        public int CopyTo(byte[] bytes, int start, int n)
        {
            if (mutex == null)
            {
                return UnLockCopyTo(bytes, start,n);
            }
            lock (mutex)
            {
                return UnLockCopyTo(bytes, start, n);
            }
        }
        public int CopyTo(byte[] bytes)
        {
            if (mutex == null)
            {
                return UnLockCopyTo(bytes, 0, bytes.Length);
            }
            lock (mutex)
            {
                return UnLockCopyTo(bytes, 0, bytes.Length);
            }
        }
        protected int UnLockCopyTo(byte[] bytes, int start, int n)
        {
            if (bytes.Length < start + n)
            {
                throw new BufferException("Memory access violation on copy");
            }

            int sum = 0;
            foreach (var fragmentation in fragmentations)
            {
                int count = fragmentation.CopyTo(bytes, start, n);
                n -= count;
                start += count;
                sum += count;

                if (n < 1)
                {
                    break;
                }
            }
            return sum;
        }

        //寫入 數據到 流 返回 實際寫入 字節 throw BufferException
        public int Write(byte[] bytes)
        {
            if (mutex == null)
            {
                return UnLockWrite(bytes, 0, bytes.Length);
            }
            lock (mutex)
            {
                return UnLockWrite(bytes, 0, bytes.Length);
            }
        }
        public int Write(byte[] bytes,int start,int n)
        {
            if (mutex == null)
            {
                return UnLockWrite(bytes, start, n);
            }
            lock (mutex)
            {
                return UnLockWrite(bytes, start, n);
            }
        }
        protected int UnLockWrite(byte[] bytes, int start, int n)
        {
            if (bytes.Length < start + n)
            {
                throw new BufferException("Memory access violation on write");
            }

            int sum = 0;
            while (n > 0)
            {
                Fragmentation fragmentation = GetWriteFragmentation();
                int count = fragmentation.Write(bytes, start, n);
                n -= count;
                start += count;
                sum += count;
            }

            return sum;
        }
        //返回 可寫的 分片
        protected Fragmentation GetWriteFragmentation()
        {
            if (fragmentations.Count > 0)
            {
                Fragmentation last = fragmentations[fragmentations.Count - 1];
                if (last.GetFree() > 0)
                {
                    return last;
                }
            }

            Fragmentation fragmentation = new Fragmentation(capacity);
            fragmentations.Add(fragmentation);
            return fragmentation;
        }

        //讀取 數據 返回 實際讀取 字節 throw BufferException
        //被讀取的 字節 將被從 緩衝區中 刪除
        public int Read(byte[] bytes)
        {
            if (mutex == null)
            {
                return UnLockRead(bytes, 0, bytes.Length);
            }
            lock (mutex)
            {
                return UnLockRead(bytes, 0, bytes.Length);
            }
        }
        public int Read(byte[] bytes,int start,int n)
        {
            if (mutex == null)
            {
                return UnLockRead(bytes, start, n);
            }
            lock (mutex)
            {
                return UnLockRead(bytes, start, n);
            }
        }
        protected int UnLockRead(byte[] bytes, int start, int n)
        {
            if (bytes.Length < start + n)
            {
                throw new BufferException("Memory access violation on read");
            }

            int sum = 0;
            while (n > 0)
            {
                Fragmentation fragmentation = GetReadFragmentation();
                int count = fragmentation.Read(bytes, start, n);
                n -= count;
                start += count;
                sum += count;
            }
            RemoveNoReadFragmentation();
            return sum;
        }
        //返回 可讀的 分片
        protected Fragmentation GetReadFragmentation()
        {
            while(fragmentations.Count > 0)
            {
                Fragmentation fragmentation = fragmentations[0];
                if (fragmentation.Size < 1)
                {
                    fragmentations.Remove(fragmentation);
                    continue;
                }
                return fragmentation;
            }

            return null;
        }
        //刪除 無數據可讀的 分片
        protected void RemoveNoReadFragmentation()
        {
            while (fragmentations.Count > 0)
            {
                Fragmentation fragmentation = fragmentations[0];
                if (fragmentation.Size > 0)
                {
                    break;
                }
                fragmentations.Remove(fragmentation);
            }
        }
    }
}
