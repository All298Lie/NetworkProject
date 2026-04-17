namespace ServerCore
{
    public class C2S_Login : Packet
    {
        public string ID;
        public string PW;

        public C2S_Login(string ID, string PW)
        {
            this.Type = (int)PacketType.C2S_Login;

            this.ID = ID;
            this.PW = PW;
        }
    }
}
