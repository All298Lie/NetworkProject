using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkProject
{
    public class NetworkServer
    {
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly object _lock = new object();

        private Socket listenSock;
        public NetworkServer(IPAddress ipAddr, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);
            this.listenSock = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            this.listenSock.Bind(endPoint); // 소켓에 EndPoint(도착 지점) 부여
            this.listenSock.Listen(1000); // 클라이언트 연결 요청 대기 (1000 : 대기열의 최대길이)
        }

        public async Task Start()
        {
            try
            {
                Console.WriteLine("Listening...");

                while (true)
                {
                    Socket clientSock = await this.listenSock.AcceptAsync();

                    User user = new User(this, clientSock);

                    _ = user.HandleReceiveAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예기치 못한 오류로 서버가 종료되었습니다. : {ex.Message}");
            }
        }

        public bool TryAddUser(string nickname, User user)
        {
            lock (_lock)
            {
                if (_users.ContainsKey(nickname) == true) return false;

                _users.Add(nickname, user);

                return true;
            }
        }

        public void RemoveUser(string nickname)
        {
            if (string.IsNullOrEmpty(nickname) == true) return;

            lock (_lock)
            {
                _users.Remove(nickname);
            }
        }

        public List<User> GetAllUsersSnapshot()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }
    }
}
