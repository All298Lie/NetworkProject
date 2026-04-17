using System;
namespace ServerCore
{
    public class C2S_Chat : Packet
    {
        public string Name;
        public string Message;

        public C2S_Chat(string name, string message)
        {
            this.Type = (int)PacketType.C2S_Chat;

            this.Name = name;
            this.Message = message;
        }
    }
}
