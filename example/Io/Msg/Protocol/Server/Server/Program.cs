using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
namespace Server
{
    class Program
    {
        static Object mutex = new Object();
        static UInt32 mid = 0;
        static UInt32 GetId()
        {
            lock (mutex)
            {
                return ++mid;
            }
        }
        static void Main(string[] args)
        {
            //創建一個 socket
            Socket l = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                int port = 1102;

                //綁定 socket
                l.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));

                //監聽 socket
                l.Listen(50);

                Console.WriteLine("Listen:{0}", port);

                //投遞新的 accept
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    PostAccept(l);
                }

                while (true)
                {
                    Console.Write("$>");
                    string cmd = Console.ReadLine();
                    if (cmd == "exit" || cmd == "quit")
                    {
                        break;
                    }
                    else if (cmd.StartsWith("echo", true, null))
                    {
                        string text = cmd.Substring(4).Trim();

                        Dark.Io.Msg.Protocol.EchoWriter writer = new Dark.Io.Msg.Protocol.EchoWriter();
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
                        writer.Write(bytes, 0, bytes.Length);
                        var msg = writer.CreateMsg(GetId());
                        SingletonClients.GetInstance().PostMessage(msg);
                    }
                    else if (cmd.StartsWith("upload", true, null))
                    {
                        string text = cmd.Substring(6).Trim();
                        string[] names = text.Split(new char[] { '-' });
                        if (names.Length < 3)
                        {
                            Console.WriteLine("params format error(upload -locak file -remote name)");
                            continue;
                        }

                        string path = names[1];
                        string name = names[2];
                        if (path == "" || name == "")
                        {
                            Console.WriteLine("params format error(upload -locak file -remote name)");
                            continue;
                        }

                        Dark.Io.Msg.Protocol.FileWriter writer = new Dark.Io.Msg.Protocol.FileWriter();
                        writer.WriteCreate(name);
                        Dark.Io.Msg.Message msg = writer.CreateMsg(GetId());
                        SingletonClients.GetInstance().PostMessage(msg);

                        using (System.IO.FileStream fr = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            byte[] bytes = new byte[1024];
                            while (fr.Position < fr.Length)
                            {
                                int n = fr.Read(bytes, 0, bytes.Length);
                                writer.WriteBinaryHeader();
                                writer.WriteBinary(bytes, 0, n);
                                msg = writer.CreateMsg(GetId());
                                SingletonClients.GetInstance().PostMessage(msg);
                            }
                        }

                        writer.WriteOk();
                        msg = writer.CreateMsg(GetId());
                        SingletonClients.GetInstance().PostMessage(msg);
                    }
                    else
                    {
                        Console.WriteLine("not found command");
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void PostAccept(Socket l)
        {
            //異步 accept
            l.BeginAccept(asyncResult =>
            {
                //投遞新的 accept
                PostAccept(l);

                //返回 客戶端
                Socket c = l.EndAccept(asyncResult);
                Console.WriteLine("\n{0} in", c.RemoteEndPoint);
                Console.Write("$>");

                Client client = new Client(c);
                SingletonClients.GetInstance().Add(client);
                {
                    Dark.Io.Msg.Protocol.EchoWriter writer = new Dark.Io.Msg.Protocol.EchoWriter();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes("welcome to cerberus");
                    writer.Write(bytes, 0, bytes.Length);
                    Dark.Io.Msg.Message msg = writer.CreateMsg(GetId());
                    client.PostMessage(msg);

                    bytes = System.Text.Encoding.UTF8.GetBytes("this is cerberus's server");
                    writer.Write(bytes, 0, bytes.Length);
                    msg = writer.CreateMsg(GetId());
                    client.PostMessage(msg);

                }

                //投遞新的 recv
                PostRecv(client);
            }, null);
        }
        static void OnClientExit(Client client)
        {
            SingletonClients.GetInstance().Remove(client);
            Console.WriteLine("\n{0} out", client.Socket.RemoteEndPoint);
            Console.Write("$>");
        }
        static void PostRecv(Client client)
        {

            byte[] bytes = new byte[1024];
            try
            {
                client.Socket.BeginReceive(bytes, 0, bytes.Length, SocketFlags.None,
                asyncResult =>
                {
                    //返回數據
                    try
                    {
                        int n = client.Socket.EndReceive(asyncResult);
                        if (n < 1)
                        {
                            OnClientExit(client);
                            return;
                        }

                        Console.WriteLine();
                        client.OnRecv(bytes, 0, n);
                        Console.Write("$>");

                        //投遞新的 recv
                        PostRecv(client);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine();
                        Console.WriteLine(e.Message);
                        OnClientExit(client);
                    }
                }, null);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                OnClientExit(client);
            }
        }
    }
}
