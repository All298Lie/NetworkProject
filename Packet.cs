using System;

namespace Packet
{
    public enum PacketType
    {
        None = 0,
        C2S_Login = 1,
        S2C_LoginResult = 2,
        C2S_Chat = 3
    }
}

internal abstract class Packet
{
	protected PacketType
	{
		public get, protected set
	}
}
