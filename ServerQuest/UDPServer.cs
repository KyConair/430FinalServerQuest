using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ServerQuest
{
    class UDPServer
    {
        public enum MessageType
        {
            Joined,
            Left,
            Message
        }

        Socket udpSocket;
        List<EndPoint> udpClients = new List<EndPoint>();
        List<int> userIDs = new List<int>();
        byte[] recBuffer = new byte[512];
        int numPacketsReceived = 0;
        

        public void startup()
        {
            Task.Factory.StartNew(() =>
            {
                // Listen for any address on port 11012
                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11012);
                udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(localEndPoint);

                udpSocket.BeginReceiveFrom(recBuffer, 0, 512, SocketFlags.None, ref localEndPoint, new AsyncCallback(MessageReceivedCallback), this);


            });
            Console.Write("Server is started.\n");
        }


        void MessageReceivedCallback(IAsyncResult result)
        {
            EndPoint remoteEndPoint = new IPEndPoint(0, 0);

            try
            {
                int readBytes = udpSocket.EndReceiveFrom(result, ref remoteEndPoint);
                // How to detect if already exists/help track
                if (udpClients.Contains(remoteEndPoint) == false)
                {
                    udpClients.Add(remoteEndPoint);
                }

            }
            catch (Exception e)
            {

            }

            int id = -1;
            string message = "";

            udpSocket.BeginReceiveFrom(recBuffer, 0, 512, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(MessageReceivedCallback), this);

            short usernameLength = (short)(recBuffer[1] + (recBuffer[2] * 256));

            id = BitConverter.ToInt32(recBuffer, 4);

            if (recBuffer[0] == (byte)MessageType.Joined)
            {
                //Console.WriteLine(username + " has joined the server!");
                //sendUDPData(id, MessageType.Joined, message);
                userIDs.Add(id);
                numPacketsReceived++;
            }
            else if (recBuffer[0] == (byte)MessageType.Left)
            {
                //Console.WriteLine(username + " has left the server!");
                //sendUDPData(id, MessageType.Left, message);
                userIDs.Remove(id);
                numPacketsReceived++;
            }
            else if (recBuffer[0] == (byte)MessageType.Message)
            {
                message = Encoding.ASCII.GetString(recBuffer, 4 + usernameLength, recBuffer[3]);
                //Console.WriteLine(username + ": " + message);
                //sendUDPData(username, MessageType.Message, message);
                numPacketsReceived++;
            }

            Array.Clear(recBuffer, 0, recBuffer.Length);
        }

        public int reportLastRun()
        {
            Console.Write("\nNumber of packets caught by the server: " + numPacketsReceived);
            return numPacketsReceived;
        }
    }
}
