using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class C2S_Login : Packet
    {
        public string ID;
        public string PW;

        public C2S_Login()
        {
            Type = (int)PacketType.C2S_Login;

            ID = "";
            PW = "";
        }
    }
}
