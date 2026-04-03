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

        // 4. 시드를 통해 1에서 100 사이의 랜덤 값 지정
        DateTime time = DateTime.Now;
        Random rand = new Random(Convert.ToInt32(time.ToString("MMddHHmmss")));
        int value = rand.Next(0, 100);

        int t = 0;
        bool currect = false;

        // 5. 소켓 설정 및 대기
        listenSocket.Bind(endPoint); // 소켓에 EndPoint(도착 지점) 부여
        listenSocket.Listen(1000); // 클라이언트 연결 요청 대기 (1000 : 대기열의 최대길이)

        Console.WriteLine($"저장된 숫자 : {value}");
        Console.WriteLine("Listening...");

        // 6. 클라이언트 연결 수락
        Socket clientSock = listenSocket.Accept();

        // 7. 데이터 통신
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
            string text = ""; // 전송할 데이터를 담는 텍스트 생성

            if (data.Equals("!ping")) // !ping 이란 데이터를 받았을 경우,
            {
                text = "pong!";
            }
            else if (currect == false) // 정답을 맞추지 않은 경우만
            {
                try
                {
                    int tryValue = Convert.ToInt32(data);

                    if (tryValue >= 0 && tryValue <= 100) // 숫자 맞추기를 할 경우
                    {
                        t = t + 1; // 시도 횟수 증가

                        if (tryValue > value)
                        {
                            text = $"{tryValue}보다 작은 숫자입니다. 시도 횟수 : {t}";
                        }
                        else if (tryValue < value)
                        {
                            text = $"{tryValue}보다 큰 숫자입니다. 시도 횟수 : {t}";
                        }
                        else
                        {
                            text = $"{tryValue}는 정답입니다! 시도 횟수 : {t}";

                            if (t <= 10) text += "\n 10번 안에 정답을 맞추셨습니다!";
                        }
                    }
                    else // 숫자 맞추기가 아닐 경우 클라이언트가 전송한 데이터 재전송
                    {
                        text = data;
                    }
                }
                catch (FormatException ex) // 숫자로 형변환이 제대로 이뤄지지 않은 경우 -> 숫자가 아닐 경우
                {
                    text = data;
                }
                catch (Exception ex) // 그 외 예외처리
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else
            {
                text = data;
            }

            sendBuffer = Encoding.UTF8.GetBytes(text);

            clientSock.Send(sendBuffer);
        }
    }
}
