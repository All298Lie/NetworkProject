namespace ServerCore
{
    public enum PacketType
    {
        None = 0,
        C2S_Login = 1,
        S2C_LoginResult = 2,
        C2S_Chat = 3
    }

    public abstract class Packet
    {
        public int Type
        {
            get { return Type; }
            protected set { Type = value; }
        }
    }
}


