using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    static void Main(string[] args)
    {
        // 1. IP주소 가져오기
        string host = Dns.GetHostName(); // 로컬 호스트 이름을 가져옴
        IPHostEntry ipHost = Dns.GetHostEntry(host); // 호스트 이름 또는 IP 주소를 통해 IPHostEntry를 리턴받음
        IPAddress ipAddr = ipHost.AddressList[0]; // ipHost의 속성에서 IPAddress를 가져옴

        // 2. EndPoint 지정
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // 네트워크 끝 점을 IP주소와 포트(7777)로 생성

        // 3. 통신용 소켓 생성
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // 소켓을 AddressFamily, SocketType, ProtocolType을 지정하여 생성

        // 4. 서버에 연결 시도
        socket.Connect(endPoint);
        Console.WriteLine($"Conneted to {socket.RemoteEndPoint.ToString()}");

        // 5. 데이터 통신
        while(true)
        {
            // 보내기
            string strLine = Console.ReadLine();
            byte[] buffer = Encoding.UTF8.GetBytes(strLine);
            int sendBytes = socket.Send(buffer);

            // 받기
            byte[] recvBuff = new byte[1024];
            int recvBytes = socket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

            Console.WriteLine($"[Server] : {recvData}");
        }
    }
}
