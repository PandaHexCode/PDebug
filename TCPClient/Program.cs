using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDebugTCPClient{
    class Program{
        private static String ip = "127.0.0.1";
        private static Int32 port = 13000;
        private static NetworkStream stream = null;
        private static TcpClient client = null;

        private static Thread consoleListeningThread;

        public static void Main(string[] args){
            Start();      
        }

        public static void Start(){
            try{
                Connect();
            }catch (ArgumentNullException e){ Console.WriteLine("ArgumentNullException: {0}", e);
            }catch (SocketException e){ Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\nPress enter to restart...");
            Console.Read();
            Console.Clear();
            Start();
        }

        public static void Connect(){
            Console.WriteLine("PDebugDrawGUI TCP Client by PandaHexCode");
            Console.WriteLine("Try to connect to " + ip + ":" + port);
            client = new TcpClient(ip, port);

            stream = client.GetStream();

            Console.WriteLine("Connected to " + GetCommand("getName") + ", Game: " + GetCommand("getGameName") + ", UnityVersion: " + GetCommand("getUnityVersion"));

            consoleListeningThread = new Thread(ConsoleListening);
            consoleListeningThread.IsBackground = true;
            consoleListeningThread.Start();
            while (true){
                string message = Console.ReadLine();
                SendCommand(message);
            }
        }

        private static void ConsoleListening(){
            while (true){
                Byte[] bytes = new Byte[1024];
                using (NetworkStream stream = client.GetStream()){
                    int lenght;

                    while ((lenght = stream.Read(bytes, 0, bytes.Length)) != 0){
                        var data = new byte[lenght];
                        Array.Copy(bytes, 0, data, 0, lenght);
                        string message = Encoding.ASCII.GetString(data);
                        if (message.StartsWith("CONSOLE: "))
                            Console.WriteLine(message);
                    }
                }
            }
        }

        public static string GetCommand(string message){
            SendMessage(message);
            if (message.StartsWith("get"))
                 return ReceiveData();

            return string.Empty;
        }
         
        public static void SendCommand(string message){
            SendMessage(message);
            if (message.StartsWith("get"))
                Console.WriteLine(ReceiveData());
            else if (message.Equals("listSceneObjects") | message.Equals("compareScene")){
                bool list = false;
                while (!list){
                    string rec = ReceiveData();
                    if (rec.Contains("endSceneList123")){ 
                        list = true;
                        Console.Write("\n");
                        return;
                    }
                    Console.Write(rec);
                }
            }else if (message.Equals("clear")){ 
                Console.Clear();
                Console.WriteLine("PDebugDrawGUI TCP Client by PandaHexCode");
                Console.WriteLine("Connected to " + GetCommand("getName") + ", Game: " + GetCommand("getGameName") + ", UnityVersion: " + GetCommand("getUnityVersion"));
            }
        }

        public static void SendMessage(string message){
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            stream.Write(data, 0, data.Length);
        }

        public static string ReceiveData(){
            Byte[] data = new Byte[2056];

            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            return responseData;
        }

        public static void Disconect(){
            stream.Close();
            client.Close();
        }
    }
}
