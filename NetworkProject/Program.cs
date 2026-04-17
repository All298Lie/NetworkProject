using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static Dictionary<Socket, string> SocketDic;

    private static void Main(string[] args)
    {
        SocketDic = new Dictionary<Socket, string>();

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

        // 5. 연결된 소켓별로 쓰레드를 만들어 연결 후 이어서 연결 대기
        while (true)
        {
            Socket clientSock = listenSocket.Accept();

            Thread thread = new Thread(() => HandleClient(clientSock));

            thread.Start();
        }
    }

    private static void HandleClient(Socket clientSock)
    {
        using (clientSock) // 해당 블록을 나갈 경우 자동으로 clientSock.Close()처리
        {
            // 6. 데이터 통신
            bool isOpened = true;
            while (isOpened == true && clientSock.Connected == true)
            {
                try
                {
                    // 받기
                    // Receive(byte[]) : byte[](buffer)에 데이터를 받고, 리턴 값으로는 데이터의 크기를 줌.
                    // GetString(byte[], int, int) : int, int 부분은 byte 인덱스 몇번부터 몇번까지 가져올것인지를 정하는 것
                    byte[] buffer = new byte[1024]; // 데이터를 받을 버퍼 생성
                    int receiveCount = clientSock.Receive(buffer); // 클라이언트 소켓에서 전송받은 크기
                    string data = Encoding.UTF8.GetString(buffer, 0, receiveCount); // byte -> string으로 인코딩(0번부터 전송받은 크기만큼만 데이터로 변환)

                    JObject obj = JObject.Parse(data); // Json 파일 가져오기

                    int type = obj["Type"].Value<int>();

                    switch (type)
                    {
                        case (int)PacketType.C2S_Login: // 로그인 패킷일 경우, 로그인 확인 후 결과를 클라이언트에게 전송
                            S2C_LoginResult loginResult = HandleLogin(data, clientSock);

                            string loginJson = JsonConvert.SerializeObject(loginResult);

                            Console.WriteLine($"[To Server] : {loginJson}");
                            byte[] loginSendBuffer = Encoding.UTF8.GetBytes(loginJson);
                            int sendBytes = clientSock.Send(loginSendBuffer);

                            break;

                        case (int)PacketType.C2S_Chat: // 채팅 패킷일 경우, 채팅을 로그인한 모든 클라이언트에게 전송
                            S2C_Chat? chatResult = HandleChat(data);

                            string chatJson = JsonConvert.SerializeObject(chatResult);
                            byte[] chatSendBuffer = Encoding.UTF8.GetBytes(chatJson);

                            foreach (Socket client in SocketDic.Keys.ToList())
                            {
                                if (SocketDic.ContainsKey(client) == true)
                                {
                                    client.Send(chatSendBuffer);
                                }
                            }

                            break;
                    }
                }
                catch (SocketException ex)
                {
                    isOpened = false;
                }
            } // while 문 끝점
        } // using 문 끝점

        if (SocketDic.ContainsKey(clientSock) == true)
        {
            Console.WriteLine($"{SocketDic[clientSock]}와의 연결이 끊겼습니다.");

            lock (clientSock)
            {
                SocketDic.Remove(clientSock);
            }
        }
    }

    private static S2C_LoginResult HandleLogin(string data, Socket clientSock)
    {
        C2S_Login? packet = JsonConvert.DeserializeObject<C2S_Login>(data);

        if (packet != null)
        {
            Console.WriteLine($"[Login] ID : {packet.ID}, PW : {packet.PW}");

            lock (clientSock)
            {
                SocketDic.Add(clientSock, packet.ID);
            }

            return new S2C_LoginResult(true, "로그인 성공");
        }
        else
        {
            return new S2C_LoginResult(false, "로그인 실패");
        }
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
