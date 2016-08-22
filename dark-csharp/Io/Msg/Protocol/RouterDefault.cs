using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dark.Io.Msg.Protocol
{
    public class RouterDefault<T>:IRouter<T>
    {
        protected System.Collections.Hashtable handlers;
        protected Object mutex = null;
        public RouterDefault(Object mutex = null)
        {
            handlers = new System.Collections.Hashtable();
            this.mutex = mutex;
        }
        public bool Transmit(System.Net.Sockets.Socket s, T info, Dark.Io.Msg.Message msg)
        {
            if (mutex == null)
            {
                return UnLockTransmit(s,info,msg);
            }

            lock (mutex)
            {
                return UnLockTransmit(s, info, msg);
            }
        }
        protected bool UnLockTransmit(System.Net.Sockets.Socket s, T info, Dark.Io.Msg.Message msg)
        {
            byte[] bytes = msg.GetData();
            if (bytes.Length < (int)Dark.Io.Msg.MessageConst.HEADER_SIZE + ProtocolConst.HASH_SIZE)
            {
                return false;
            }

            string hash = System.Text.Encoding.ASCII.GetString(bytes, (int)Dark.Io.Msg.MessageConst.HEADER_SIZE, ProtocolConst.HASH_SIZE);
            Object obj = handlers[hash];
            if (obj == null)
            {
                return false;
            }
            IHandler<T> handler = obj as IHandler<T>;
            if (handler == null)
            {
                return false;
            }
            return handler.Deal(s, info, msg);
        }
        public void Register(IHandler<T> h)
        {
            if (mutex == null)
            {
                handlers.Add(h.Hash(), h);
                return;
            }
            
            lock (mutex)
            {
                handlers.Add(h.Hash(), h);
            }
            
        }
        public void UnRegister(string hash)
        {
            if (mutex == null)
            {
                handlers.Remove(hash);
                return;
            }

            lock (mutex)
            {
                handlers.Remove(hash);
            }
            
        }
    }
}
