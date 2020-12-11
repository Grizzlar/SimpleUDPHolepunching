using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SHC
{
    class Client{
        public int timeout;
        private UdpClient udpClient;
        private IPEndPoint serverEP;
        public Client(int port){
            this.udpClient =  new UdpClient();
            this.serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        }
        public void DataHandler(){
            byte[] data;
            byte[] ka = new byte[] { 0x61, 0x6e };
            this.Send(ka);
            while(true){
                data = this.Receive();
                if(data.Length == 2){
                    this.Send(ka);
                }
            }
        }
        public byte[] Receive(){
            return this.udpClient.Receive(ref this.serverEP);
        }
        public void Send(byte[] data){
            this.udpClient.Send(data, data.Length, this.serverEP);
        }
        public void Start(int timeout=3000)
        {
            Console.WriteLine("Client Initialized!");
            this.timeout = timeout;

            Console.Write("Starting handler thread... ");
            Thread handler = new Thread(this.DataHandler);
            handler.IsBackground = true;
            handler.Start();
            Console.WriteLine("Done!");
        }
        public static void Main(string[] args){
            Client SHClient = new Client(1102);
            SHClient.Start();
            while(true){}
        }
    }
}