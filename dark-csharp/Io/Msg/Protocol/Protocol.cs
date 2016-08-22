using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace Dark.Io.Msg.Protocol
{
    public class ProtocolConst
    {
        public static int HASH_SIZE = 32;
        public static int BODY_OFFSET = (int)Dark.Io.Msg.MessageConst.HEADER_SIZE + HASH_SIZE;
    };

    //生產 協議 數據
    public interface IWriter
    {
        //返回 協議 唯一標識 32字節 的 字符串
        string Hash();
        //寫入 數據流
        int Write(byte[] bytes, int start, int n);
        //用 已輸入數據流 生產一個協議消息
        Message CreateMsg(UInt32 id);
    }
    //協議 處理器
    public interface IHandler<T>
    {
        //返回 協議 唯一標識 32字節 的 字符串
        string Hash();
        bool Deal(System.Net.Sockets.Socket s/*觸發socket*/,T info/*與socket綁定的 信息數據*/,Dark.Io.Msg.Message msg/*收到的 socket 消息*/);
    }

    //協議 路由器
    public interface IRouter<T>
    {
        //分發 消息 到路由
        //如沒沒有 匹配路由 返回 false
        bool Transmit(System.Net.Sockets.Socket s, T info, Dark.Io.Msg.Message msg);

        //註冊 路由
        void Register(IHandler<T> h);

        //註銷 路由
        void UnRegister(string hash);
    }

}