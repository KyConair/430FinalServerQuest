using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ServerQuest
{
    class UDPC
    {
        public enum MessageType
        {
            Joined,
            Left,
            Message
        }

        UdpClient udpClient;
        int id = -1;
        int packetDelay = 50;
        byte[] recBuffer = new byte[512];

        public void startup(int num, int packDelay)
        {
            //Though unused, preserve the ability to handle a client exiting
            Console.CancelKeyPress += new ConsoleCancelEventHandler(shutdown);

            id = num;
            packetDelay = packDelay;
            try
            {
                //Try setting up our UDP Client
                IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11012);
                udpClient = new UdpClient();
                udpClient.Connect(udpEndPoint);

                sendUDPData(id, MessageType.Joined, "", false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to server, too many clients entered");
                System.Environment.Exit(-1);
            }

        }

        public void beginSending(int id)
        {
            sendUDPData(id, MessageType.Message, "pew", true);
        }

        public void shutdown(object sender, ConsoleCancelEventArgs args)
        {
            if (udpClient != null && id != -1)
            {
                sendUDPData(id, MessageType.Left, "", false);
            }
        }

        public async void sendUDPData(int id, MessageType type, string message, bool sendRecursively)
        {
            byte[] packetBuffer = new byte[4];

            byte[] usernameBuffer = BitConverter.GetBytes(id);

            byte[] messageBuffer = System.Text.Encoding.ASCII.GetBytes(message);

            packetBuffer[0] = (byte)type;
            packetBuffer[1] = (byte)(usernameBuffer.Length % 256);
            packetBuffer[2] = (byte)(usernameBuffer.Length / 256);
            packetBuffer[3] = (byte)(messageBuffer.Length);

            byte[] udpMessage = packetBuffer.Concat(usernameBuffer).Concat(messageBuffer).ToArray();

            udpClient.Client.Send(udpMessage);

            if (sendRecursively)
            {
                await Task.Delay(packetDelay);
                sendUDPData(id, MessageType.Message, "hi", true);
            }
        }
    }
}
