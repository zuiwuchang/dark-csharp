﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        protected Socket s;
        protected Dark.Io.Msg.Reader reader;
        protected Dark.Io.Msg.Protocol.RouterDefault<Client> router;

        public Socket Socket { get { return s; } }
        
        public Client(Socket s)
        {
            this.s = s;
            reader = new Dark.Io.Msg.Reader();
            router = new Dark.Io.Msg.Protocol.RouterDefault<Client>();

            router.Register(new Dark.Io.Msg.Protocol.EchoHandler<Client>());
            router.Register(new Dark.Io.Msg.Protocol.FileHandler<Client>());

            msgs = new List<Dark.Io.Msg.Message>();
        }

        protected List<Dark.Io.Msg.Message> msgs;
        public void PostMessage(Dark.Io.Msg.Message msg)
        {
            lock (this)
            {
                msgs.Add(msg);
            }
            PostMessage();
        }
        protected void PostMessage()
        {
            lock (this)
            {
                if (msgs.Count() < 1)
                {
                    return;
                }
                Dark.Io.Msg.Message msg = msgs[0];
                msgs.RemoveAt(0);

                byte[] bytes = msg.GetData();
                try
                {
                    s.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, asyncResult =>
                    {
                        try
                        {
                            s.EndSend(asyncResult);
                            PostMessage();
                        }
                        catch (SocketException)
                        {
                            // Console.WriteLine(e.Message);
                        }

                    }, null);
                }
                catch (SocketException)
                {
                    // Console.WriteLine(e.Message);
                }

            }
        }

        public void OnRecv(byte[] bytes, int start, int n)
        {
            reader.Write(bytes, 0, n);
            while (true)
            {
                var msg = reader.GetMsg();
                if (msg == null)
                {
                    break;
                }
                router.Transmit(Socket, this, msg);
            }
        }
    }
}
