using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore;
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
        Console.WriteLine($"Conneted to {socket.RemoteEndPoint?.ToString() ?? "존재하지 않음"}");

        // 5. 로그인 정보 전송
        string playerID = Guid.NewGuid().ToString();
        string playerPW = "1234";

        C2S_Login loginPacket = new C2S_Login(playerID, playerPW);

        string _json = JsonConvert.SerializeObject(loginPacket);
        byte[] _buffer = Encoding.UTF8.GetBytes(_json);
        int _sendBytes = socket.Send(_buffer);

        // 6. 서버에게 쓰레드를 통해 계속 통신을 받음
        Thread receiveThread = new Thread(() => ReceivePacket(socket));

        receiveThread.Start();

        // 5. 채팅 통신을 서버에게 전송
        while (true)
        {
            string? strLine = Console.ReadLine();

            if (strLine == null) continue;

            C2S_Chat packet = new C2S_Chat(playerID, strLine);

            string json = JsonConvert.SerializeObject(packet);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            int sendBytes = socket.Send(buffer);
        }
    }

    private static void ReceivePacket(Socket serverSock)
    {
        using (serverSock)
        {
            bool isOpen = true;
            while (serverSock.Connected == true && isOpen == true)
            {
                try
                {
                    // 받기
                    // Receive(byte[]) : byte[](buffer)에 데이터를 받고, 리턴 값으로는 데이터의 크기를 줌.
                    // GetString(byte[], int, int) : int, int 부분은 byte 인덱스 몇번부터 몇번까지 가져올것인지를 정하는 것
                    byte[] buffer = new byte[1024]; // 데이터를 받을 버퍼 생성
                    int receiveCount = serverSock.Receive(buffer); // 클라이언트 소켓에서 전송받은 크기
                    string data = Encoding.UTF8.GetString(buffer, 0, receiveCount); // byte -> string으로 인코딩(0번부터 전송받은 크기만큼만 데이터로 변환)

                    JObject obj = JObject.Parse(data); // Json 파일 가져오기

                    int type = obj["Type"].Value<int>();

                    switch (type)
                    {
                        case (int)PacketType.S2C_LoginResult:
                            HandleLoginResult(data);
                            break;

                        case (int)PacketType.S2C_Chat:
                            HandleChat(data);
                            break;
                    }
                }
                catch (SocketException ex)
                {
                    isOpen = false;
                }
            } // while 문 끝점
        } // using 문 끝점

        Console.WriteLine("서버와의 연결이 끊겼습니다.");
    }

    private static void HandleLoginResult(string data)
    {
        S2C_LoginResult? packet = JsonConvert.DeserializeObject<S2C_LoginResult>(data);

        if (packet != null)
        {
            Console.WriteLine($"서버와의 연결을 {(packet.IsSuccess ? "성공" : "실패")}하였습니다.");
            Console.WriteLine($"내용 : {packet.Message}");
        }
    }

    private static void HandleChat(string data)
    {
        S2C_Chat? packet = JsonConvert.DeserializeObject<S2C_Chat>(data);

        if (packet != null)
        {
            Console.WriteLine($"{packet.Sender} : {packet.Message}");
        }
    }
}