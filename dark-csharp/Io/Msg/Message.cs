using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace Dark.Io.Msg
{
    public class Header
    {
        //消息 標記
        public UInt32 Flag;
        //消息 唯一標識符
        public UInt32 Id;
        //消息長度
        public Int32 Size;

        public byte[] GetBytes()
        {
            byte[] data = new byte[Message.HEADER_SIZE];
            
            BitConverter.GetBytes(Flag).CopyTo(data, 0);
            BitConverter.GetBytes(Id).CopyTo(data, 4);
            BitConverter.GetBytes(Size).CopyTo(data, 8);

            return data;
        }

        public override string ToString()
        {
            return String.Format("Flag = {0}    Id = {1}    Size = {2}",Flag,Id,Size);
        }
    }
    public sealed class Message
    {
        //消息頭標記
        public static UInt32 HEADER_FLAG = 0x0000044E;
        //消息頭長度
        public static UInt32 HEADER_SIZE = 12;

        public static Int32 HEADER_SIZE_OFFSET = 8;


        public Message(byte[] data)
        {
            this.data = data;
        }

        //二進制數據
        private byte[] data = null;

       
        //返回 數據
        public byte[] GetData()
        {
            return data;
        }

        //返回 消息頭/null
        public Header GetHeader()
        {
            if (data == null || data.Length < HEADER_SIZE)
            {
                return null;
            }
            
            Header header = new Header();
            header.Flag = BitConverter.ToUInt32(data, 0);
            if (header.Flag != HEADER_FLAG)
            {
                return null;
            }
            header.Id = BitConverter.ToUInt32(data, 4);
            header.Size = BitConverter.ToInt32(data, 8);

            return header;
        }

        
    }
}
