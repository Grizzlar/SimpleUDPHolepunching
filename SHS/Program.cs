using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace SHS
{
    class Server
    {
        private Socket server;
        public int port { get; private set; }
        public int timeout;
        public static byte[] kaMessage = new byte[] {0x61, 0x6e};
        public static int SIO_UDP_CONNRESET = -1744830452;
        private ConcurrentQueue<long> kaQueue = new ConcurrentQueue<long>();
        public static long endPointToLong(IPEndPoint ep){
            return BitConverter.ToUInt32(ep.Address.GetAddressBytes()) | ((uint)ep.Port * 0x100000000);
        }
        public static IPEndPoint longToEndPoint(long ep){
            uint ipPart = (uint)(ep & 0x00000000ffffffff);
            int port = (int)((ep & 0x0000ffff00000000) / 0x100000000);
            return new IPEndPoint(new IPAddress(BitConverter.GetBytes(ipPart)), port);
        }
        public void Listen(int port = 1102){
            this.port = port;
            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.server.IOControl(
                (IOControlCode)Server.SIO_UDP_CONNRESET, 
                new byte[] { 0, 0, 0, 0 }, 
                null
            );
            this.server.Bind(new IPEndPoint(IPAddress.Any, this.port));
        }
        private void KeepAliveThread(){
            long host;
            do {
                while(kaQueue.TryDequeue(out host)){
                    IPEndPoint to = longToEndPoint(host);
                    this.server.SendTo(Server.kaMessage, 0, 2, SocketFlags.None, to);
                }
                Thread.Sleep(timeout);
            }while(true);
        }
        private void Receiver(){
            byte[] data = new byte[2];
            while(true){
                EndPoint from = new IPEndPoint(IPAddress.Any, 0);
                if(this.server.ReceiveFrom(data, 2, SocketFlags.None, ref from) == 2){
                    this.kaQueue.Enqueue(endPointToLong((IPEndPoint)from));
                    Console.WriteLine(((IPEndPoint)from).ToString());
                }
            }
        }
        public void Start(int timeout=3000)
        {
            this.timeout = timeout;

            Console.Write("Starting the server... ");
            Listen(1102);
            Console.WriteLine("Done!\n\tRunning as "+IPAddress.Any.ToString()+":"+this.port);

            Console.Write("Starting the keep alive threads... ");
            Thread keepAliveThread = new Thread(this.KeepAliveThread);
            keepAliveThread.IsBackground = true;
            keepAliveThread.Start();
            Console.WriteLine("Done!");

            Console.Write("Starting the receiver thread... ");
            Thread receiver = new Thread(this.Receiver);
            receiver.IsBackground = true;
            receiver.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Initialization completed!");
        }
        public static void Main(string[] args){
            Server SHServer = new Server();
            SHServer.Start();
            while(true){}
        }
    }
}