using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetworkProject
{
    public class User
    {
        private static readonly Regex regex = new Regex("$[a-zA-Z0-9]{3, 12}^", RegexOptions.Compiled);

        public Socket client { get; private set; }
        public NetworkServer server { get; private set; }

        public string nickname { get; private set; } = string.Empty;

        public User(NetworkServer server, Socket client)
        {
            this.server = server;
            this.client = client;
        }

        public async Task HandleReceiveAsync()
        {
            try
            {
                while (true)
                {
                    // 받기
                    string data = await PacketHelper.ReceivePacket(this.client);

                    Packet? packet = JsonConvert.DeserializeObject<Packet>(data); // Json 파일 가져오기
                    if (packet == null) continue;

                    int type = packet.Type;

                    switch (type)
                    {
                        case (int)PacketType.C2S_LoginReq: // 로그인 패킷일 경우, 로그인 확인 후 결과를 클라이언트에게 전송
                            C2S_LoginReq loginReq = JsonConvert.DeserializeObject<C2S_LoginReq>(data)!;
                            _ = HandleLogin(loginReq);
                            break;

                        case (int)PacketType.C2S_ChatReq: // 채팅 패킷일 경우, 채팅을 로그인한 모든 클라이언트에게 전송
                            C2S_ChatReq chatReq = JsonConvert.DeserializeObject<C2S_ChatReq>(data)!;
                            _ = HandleChat(chatReq);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        private void Disconnect() // 연결 종료 로직
        {
            server.RemoveUser(this.nickname);

            try
            {
                this.client.Close();
            }
            catch (Exception ex)
            {
                // 이미 로그아웃 처리 된 경우 패스
            }
        }

        private async Task HandleLogin(C2S_LoginReq req)
        {
            S2C_LoginRes res = new S2C_LoginRes();

            // 1. 닉네임 입력이 되어있는지 확인
            if (string.IsNullOrEmpty(req.Nickname) == true)
            {
                res.IsSuccess = false;
                res.Message = "닉네임을 입력해야 합니다.";

                await PacketHelper.SendPacket(client, res);

                return;
            }

            // 2. 닉네임 확인
            string nickname = req.Nickname;
            if (regex.IsMatch(nickname) == false)
            {
                res.IsSuccess = false;
                res.Message = "영문자와 숫자만을 사용하여 3~12자 길이로 입력해야 합니다.";

                await PacketHelper.SendPacket(client, res);

                return;
            }

            // 3. 존재하는 닉네임인지 확인
            if (server.TryAddUser(nickname, this) == false)
            {
                res.IsSuccess = false;
                res.Message = "이미 존재하는 닉네임 입니다.";

            }
            else
            {
                this.nickname = nickname;

                res.IsSuccess = true;
                res.Message = "닉네임 등록이 완료되었습니다.";
            }

            await PacketHelper.SendPacket(client, res);
        }

        private async Task HandleChat(C2S_ChatReq req)
        {
            S2C_ChatRes res = new S2C_ChatRes();

            // 1. 로그인을 한 상태인지 확인
            if (string.IsNullOrEmpty(this.nickname) == true)
            {
                res.IsSuccess = false;
                res.Message = "로그인 후 이용할 수 있는 서비스입니다.";

                await PacketHelper.SendPacket(this.client, res);
                return;
            }

            // 2. 유저한테 메세지 전송
            S2C_ChatNoti noti = new S2C_ChatNoti();
            noti.Sender = this.nickname;
            noti.Message = res.Message;

            List<User> users = server.GetAllUsersSnapshot();
            List<Task> sendTask = new List<Task>();

            foreach (User user in users)
            {
                sendTask.Add(PacketHelper.SendPacket(user.client, noti));
            }

            try
            {
                await Task.WhenAll(sendTask);
            }
            catch (Exception ex)
            {
                // 메세지가 전송되지 않았지만 무시하고 다른 유저에게 메세지 전송
            }
        }
    }
}
