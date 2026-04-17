namespace ServerCore
{
    public class S2C_Chat : Packet
    {
        public string Sender;
        public string Message;

        public S2C_Chat(string sender, string message)
        {
            this.Type = (int)PacketType.S2C_Chat;

            this.Sender = sender;
            this.Message = message;
        }
    }
}
