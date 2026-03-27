using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    static void Main(string[] args)
    {
        // 1. IP주소 가져오기
        string host = Dns.GetHostName(); // 로컬 호스트 이름을 가져옴
        IPHostEntry ipHost = Dns.GetHostEntry(host); // 호스트 이름을 통해 네트워크 정보를 가져옴
        IPAddress ipAddr = ipHost.AddressList[0]; // 네트워크가 가진 IP 주소 목록 중 첫번째 선택

        // 2. EndPoint 지정 / EndPoint = 도착지 = 주소
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // IP주소와 포트(7777)로 EndPoint 생성

        // 3. 리스너 소켓 생성
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // 소켓을 AddressFamily, SocketType, ProtocolType을 지정하여 생성
        
        // 4. 소켓 설정 및 대기
        listenSocket.Bind(endPoint); // 소켓에 EndPoint(도착 지점) 부여
        listenSocket.Listen(1000); // 클라이언트 연결 요청 대기 (1000 : 대기열의 최대길이)

        Console.WriteLine("Listening...");

        // 5. 클라이언트 연결 수락
        Socket clientSock = listenSocket.Accept();

        // 6. 데이터 통신
        while (true)
        {

            // 받기
            // Receive(byte[]) : byte[](buffer)에 데이터를 받고, 리턴 값으로는 데이터의 크기를 줌.
            // GetString(byte[], int, int) : int, int 부분은 byte 인덱스 몇번부터 몇번까지 가져올것인지를 정하는 것
            byte[] buffer = new byte[1024]; // 데이터를 받을 버퍼 생성
            int receiveCount = clientSock.Receive(buffer); // 클라이언트 소켓에서 전송받은 크기
            string data = Encoding.UTF8.GetString(buffer, 0, receiveCount); // byte -> string으로 인코딩(0번부터 전송받은 크기만큼만 데이터로 변환)

            Console.WriteLine($"[Client] : {data}"); // 출력

            // 보내기
            byte[] sendBuffer; // 데이터를 담을 버퍼 생성

            if (data.Equals("!ping")) // !ping 이란 데이터를 받았을 경우,
            {
                sendBuffer = Encoding.UTF8.GetBytes("pong!");
            }
            else // 그 외
            {
                sendBuffer = Encoding.UTF8.GetBytes(data);
            }
            clientSock.Send(sendBuffer);
        }
    }
}
