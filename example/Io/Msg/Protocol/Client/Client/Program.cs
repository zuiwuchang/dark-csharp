using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
namespace Client
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
            Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //連接 服務器
                c.Connect("127.0.0.1", 1102);

                Console.WriteLine("connect {0} at", c.RemoteEndPoint);

                Client client = new Client(c);

                PostRecv(client);


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
                        client.PostMessage(msg);
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
                            Console.WriteLine("\nclose socket");
                            Console.WriteLine("$>");
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
                        Console.WriteLine("close socket");
                        Console.WriteLine("$>");
                    }
                }, null);
            }
            catch (SocketException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine("close socket");
                Console.WriteLine("$>");
            }
        }
    }
}
