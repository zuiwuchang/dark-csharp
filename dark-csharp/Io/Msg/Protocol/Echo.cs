using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dark.Io.Msg.Protocol
{
    public class EchoConst
    {
        private EchoConst() { }
        //md5 cerberus protocol echo by dark king
        public static string HASH = "f652f3005d69a24e66dd9075a522668f";
    }
    public class EchoWriter : IWriter
    {
        public string Hash()
        {
            return EchoConst.HASH;
        }
        protected Dark.Io.Msg.Writer writer;
        protected Object mutex;
        public EchoWriter(int capacity = 1024/*分塊長度*/, Object mutex = null/*不為null 使用 mutex 啟用 線程安全*/)
        {
            writer = new Dark.Io.Msg.Writer(capacity);
            this.mutex = mutex;
            writer.WriteString(Hash());
        }

        public int Write(byte[] bytes, int start, int n)
        {
            if (mutex == null)
            {
                return writer.Write(bytes, start, n);
            }

            lock (mutex)
            {
                return  writer.Write(bytes, start, n);
            }
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
    public class EchoHandler<T> : IHandler<T>
    {
        public string Hash()
        {
            return EchoConst.HASH;
        }
        protected Object mutex = null;
        public EchoHandler(Object mutex =null)
        {
            this.mutex = mutex;
        }

        public bool Deal(System.Net.Sockets.Socket s, T info, Message msg)
        {
            if (mutex == null)
            {
                return UnLockDeal(s, info, msg);
 
            }
            lock (mutex)
            {
                return UnLockDeal(s, info, msg);
            }
        }
        protected bool UnLockDeal(System.Net.Sockets.Socket s, T info, Message msg)
        {
            byte[] bytes = msg.GetData();
            string str = System.Text.Encoding.UTF8.GetString(msg.GetData(), ProtocolConst.BODY_OFFSET, bytes.Length - ProtocolConst.BODY_OFFSET);

            Console.WriteLine("recv {0} {1} :   {2}", s.RemoteEndPoint, info, str);

            return true;
        }
    }
}
