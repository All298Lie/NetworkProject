using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public enum PacketType
    {
        None = 0,

        C2S_LoginReq = 1,
        S2C_LoginRes = 2,

        C2S_ChatReq = 3,
        S2C_ChatRes = 4,
        S2C_ChatNoti = 5
    }

    public abstract class Packet
    {
        public int Type { get; protected set; }
    }

    public class C2S_LoginReq : Packet
    {
        public string Nickname { get; set; } = string.Empty;

        public C2S_LoginReq()
        {
            this.Type = (int)PacketType.C2S_LoginReq;
        }
    }

    public class S2C_LoginRes : Packet
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;

        public S2C_LoginRes()
        {
            this.Type = (int)PacketType.S2C_LoginRes;
        }
    }

    public class C2S_ChatReq : Packet
    {
        public string Message { get; set; } = string.Empty;

        public C2S_ChatReq()
        {
            this.Type = (int)PacketType.C2S_ChatReq;
        }
    }

    public class S2C_ChatRes : Packet
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;

        public S2C_ChatRes()
        {
            this.Type = (int)PacketType.S2C_ChatRes;
        }
    }

    public class S2C_ChatNoti : Packet
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public S2C_ChatNoti()
        {
            this.Type = (int)PacketType.S2C_ChatNoti;
        }
    }
}
