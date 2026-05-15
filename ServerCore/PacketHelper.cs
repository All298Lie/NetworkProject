using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace ServerCore
{
    public static class PacketHelper
    {
        public static async Task SendPacket(Socket socket, Packet packet)
        {
            string json = JsonConvert.SerializeObject(packet);
            // 1. Json을 byte[]로 변환
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // 2. 길이를 byte[]로 변환
            int len = jsonBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(len);

            // 3. header와 payload 붙이기
            byte[] data = new byte[4 + len];
            Buffer.BlockCopy(lengthBytes, 0, data, 0, 4);
            Buffer.BlockCopy(jsonBytes, 0, data, 4, len);

            // 4. 전송
            await socket.SendAsync(data);
        }

        public static async Task<byte[]> ReceiveExactly(Socket socket, int length)
        {
            byte[] buffer = new byte[length];
            int totalReceived = 0;

            while (totalReceived < length)
            {
                int received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, totalReceived, length - totalReceived), SocketFlags.None);

                if (received == 0) throw new SocketException();

                totalReceived = totalReceived + received;
            }

            return buffer;
        }

        public static async Task<string> ReceivePacket(Socket socket)
        {
            // 1. header 받기
            byte[] lengthBytes = await ReceiveExactly(socket, 4);
            int len = BitConverter.ToInt32(lengthBytes);

            // 2. 비정상 길이 체크
            if (len <= 0 || len > 10_000_000) throw new Exception();

            // 2. payload 받기
            byte[] jsonBytes = await ReceiveExactly(socket, len);
            string json = Encoding.UTF8.GetString(jsonBytes);

            return json;
        }
    }
}
