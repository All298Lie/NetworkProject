namespace ServerCore
{
    public class S2C_LoginResult : Packet
    {
        public bool IsSuccess;
        public string Message;

        public S2C_LoginResult(bool isSuccess, string message)
        {
            this.Type = (int)PacketType.S2C_LoginResult;

            this.IsSuccess = isSuccess;
            this.Message = message;
        }
    }
}
