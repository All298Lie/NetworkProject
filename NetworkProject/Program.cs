using NetworkProject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    async static void Main(string[] args)
    {
        // 1. IP주소 가져오기
        string host = Dns.GetHostName(); // 로컬 호스트 이름을 가져옴
        IPHostEntry ipHost = Dns.GetHostEntry(host); // 호스트 이름을 통해 네트워크 정보를 가져옴
        IPAddress ipAddr = ipHost.AddressList[0]; // 네트워크가 가진 IP 주소 목록 중 첫번째 선택

        NetworkServer server = new NetworkServer(ipAddr, 7777);
        await server.Start();
    }

    private static S2C_Chat? HandleChat(string data)
    {
        C2S_Chat? packet = JsonConvert.DeserializeObject<C2S_Chat>(data);

        if (packet != null)
        {
            Console.WriteLine($"[Chat] {packet.Name} : {packet.Message}");

            return new S2C_Chat(packet.Name, packet.Message);
        }

        return null;
    }
}
