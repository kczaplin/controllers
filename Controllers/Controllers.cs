using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

    }
    class Controllers
    {
        String name;
        IPAddress ControllersAddress;
        int ControllersPort;
        Socket CCRCSocket;
        IPAddress NCCAddress;
        int NCCPort;
        private ConcurrentDictionary<string, IPAddress> nameToIp;
        private ConcurrentDictionary<IPAddress, string> ipToName;

        private ConcurrentDictionary<string, Socket> NodeToSocket;
        private ConcurrentDictionary<Socket, string> SocketToNode;
        private ConcurrentDictionary<string, IPEndPoint> nameToEP;
        private ConcurrentDictionary<string, IPAddress> nameToIP;

        private int MAXSLOTS = 20;
        private List<LRMRow> LRM;
        private Graf topology;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public Controllers(string configFile)
        {
            nameToIp = new ConcurrentDictionary<string, IPAddress>();
            ipToName = new ConcurrentDictionary<IPAddress, string>();
            nameToEP = new ConcurrentDictionary<string, IPEndPoint>();
            NodeToSocket = new ConcurrentDictionary<string, Socket>();
            SocketToNode = new ConcurrentDictionary<Socket, string>();
            if (name.Equals("CCRC1"))
            {
                topology = new Graf("graf1.txt");
            }
            else if (name.Equals("CCRC2"))
            {
                topology = new Graf("graf2.txt");
            }
            else if (name.Equals("CCRC3"))
            {
                topology = new Graf("graf3.txt");
            }
            foreach (Krawedz krawedz in topology.krawedzie)
            {
                LRM.Add(new LRMRow(krawedz.PodajPoczatek(), krawedz.PodajKoniec(), MAXSLOTS));
            }
            List<string> lines;
            lines = File.ReadAllLines(configFile).ToList();
            ReadConfig(configFile);
        }

        public void ReadConfig(string configFile)
        {
            List<string> lines;
            lines = File.ReadAllLines(configFile).ToList();
            SetValues(lines);
            readIP(lines, nameToIp, ipToName);
            int port;
            IPAddress ip;
            foreach (var line in lines.FindAll(line => line.StartsWith("CCRC")))
            {
                string[] entries;
                entries = line.Split(' ');
                ip = IPAddress.Parse(entries[2]);
                port = Convert.ToInt32(entries[3]);
                IPEndPoint ep = new IPEndPoint(ip, port);
                nameToEP.TryAdd(entries[1], ep);
                nameToIP.TryAdd(entries[1],ip);
            }
        }
        /*
        private void StartNCC()
        {
            IPEndPoint NCCRemoteEP = new IPEndPoint(NCCAddress, NCCPort);

            NCCSocket = new Socket(NCCAddress.AddressFamily,
                       SocketType.Stream, ProtocolType.Tcp);
            NCCSocket.Connect(NCCRemoteEP);
            Console.WriteLine("Connecting with: {0} {1}", ControllersAddress, ControllersPort);
            NCCSocket.Send(Encoding.ASCII.GetBytes($"{name} : hello"));
            Task.Run(() => ListenToNCC());

        }
 
            private void ListenToNCC()

        {
            Console.WriteLine("Jestem w listen");
            Byte[] receiveByte = new Byte[1024];
            string sourceNodeName = string.Empty;
            while (true)
            {
                try
                {
                    CCRCSocket.Receive(receiveByte);
                    String sourcehost = "";
                    String desthost = "";
                    if (receiveByte.Length != 0)
                    {
                        Console.WriteLine("Dostalem wiadomosc");
                        CallRequest CallReq = CallRequest.convertToRequest(receiveByte);
                        ipToName.TryGetValue(CallReq.destination, out desthost);
                        ipToName.TryGetValue(CallReq.source, out sourcehost);

                        Console.WriteLine("[{0}:{1}:{2}.{3}] Got message from NCC to connect {4} with {5} with capacity {6}",
                            DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond,
                            sourcehost, desthost, CallReq.capacity);

                        //To wpisane na sztywno dla testu!!!!!!!!
                        CallResponse Call_resp = new CallResponse(CallReq.source, CallReq.destination, CallReq.capacity, 0, 3);
                        Console.WriteLine("[{0}:{1}:{2}.{3}] Sending message to NCC: source: {4} destination: {5}",
                            DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond,
                            sourcehost, desthost);

                        CCRCSocket.Send(Call_resp.convertToByte());

                    }
                    Thread.Sleep(10);
                }
                catch (SocketException e)
                {
                        Console.WriteLine(e.ToString());
                }
                Array.Clear(receiveByte, 0, receiveByte.Length);
            }
        }
        */


        public void StartCCRC()
        {


            // Create a TCP/IP socket.  
            CCRCSocket = new Socket(NCCAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);


            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                ControllersAddress = IPAddress.Parse("127.0.0.1");

                CCRCSocket.Bind(new IPEndPoint(ControllersAddress, ControllersPort));

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();
                    CCRCSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        CCRCSocket);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);


            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Read data from the client socket.
            int bytesRead;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            state.sb.Clear();
            state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));
            if (state.sb.ToString().Contains("hello"))
            {
                Send(handler, Encoding.ASCII.GetBytes("Connection with CCRC established"));
                //Hello H3 e.g.
                var split = state.sb.ToString().Split(' ');
                string nodeName = split[0];

                Console.WriteLine(" {0}:{1}:{2}.{3} Received message {4}",
                     DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, state.sb);
                while (true)
                {
                    if (NodeToSocket.TryAdd(nodeName, handler))
                    {
                        Console.WriteLine($"Adding{nodeName}");
                        break;
                    }
                    Thread.Sleep(100);
                }
                while (true)
                {
                    if (SocketToNode.TryAdd(handler, nodeName))
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }


            }

            if (state.sb.ToString().Contains("Connect")) //jesli jest connect to ccrc1 ma wyslac do ccrc2 wejscie i wyjscie z podsieci - connect dostaje od ncc
            {
                List<int> result = new List<int>();
                int iterator = 0, odleglosc = 0;
                SortedSet<int> fsu = new SortedSet<int>();
                List<int> nodeIndex = new List<int>();
                result = topology.dijkstra(1, 7);
                foreach (int x in result)
                {
                    if (x == -777)
                        iterator++;
                    if (iterator == 0 && x != -777)
                        odleglosc = x;
                    else if (iterator == 1 && x != -777)
                        fsu.Add(x);
                    else if (iterator == 2 && x != -777)
                        nodeIndex.Add(x);

                }

                Console.WriteLine("*******************");
                Console.WriteLine("ODLEGLOSC DO KONCA: " + odleglosc);
                Console.WriteLine("NUMERY SZCZELIN DOSTEPNYCH: ");
                foreach (int number in fsu)
                {
                    Console.Write(number + " ");
                }
                Console.WriteLine();
                Console.WriteLine("Numery indeksow wezlow: ");
                foreach (int number in nodeIndex)
                {
                    Console.Write(number);
                }
                Socket CCRC2Socket;
                IPEndPoint CCRC2EP;
                IPAddress CCRC2IP;
                ConnectionRequest ConReq = new ConnectionRequest("CCRC2", WEJSCIE , WYJSCIE);
                nameToEP.TryGetValue("CCRC2", out CCRC2EP);
                nameToIP.TryGetValue("CCRC2", out CCRC2IP);
                CCRC2Socket = new Socket(CCRC2IP.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                CCRC2Socket.Connect(CCRC2EP);
                CCRC2Socket.Send(ConReq.convertReqToByte()); 

            }
            if (state.sb.ToString().Contains("CCRC2")) //obslugujemy wiadomosc w CCRC2
            {
                Socket CCRC1Socket;
                IPEndPoint CCRC1EP;
                IPAddress CCRC1IP;

                nameToEP.TryGetValue("CCRC1", out CCRC1EP);
                nameToIP.TryGetValue("CCRC1", out CCRC1IP);
                CCRC1Socket = new Socket(CCRC1IP.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                CCRC1Socket.Connect(CCRC1EP);
                CCRC1Socket.Send(ConReq.convertReqToByte());

            }

            state.sb.Clear();

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

        }

        private static void Send(Socket handler, byte[] byteData)
        {
            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                //handler.Shutdown(SocketShutdown.Send);
                // handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        public void SetValues(List<string> lines)
        {
            this.name = GetValueFromConfig("NAME", lines);
            Console.Title = name;
            Console.SetWindowSize(60, 13);
            this.ControllersAddress = IPAddress.Parse(GetValueFromConfig("CONTROL_ADDRESS", lines));
            this.ControllersPort = Convert.ToInt32(GetValueFromConfig("CONTROL_PORT", lines));
            this.NCCAddress = IPAddress.Parse(GetValueFromConfig("NCC_ADDRESS", lines));
            this.NCCPort = Convert.ToInt32(GetValueFromConfig("NCC_PORT", lines));


        }
        public string GetValueFromConfig(string name, List<string> lines)
        {
            string[] entries;
            entries = lines.Find(line => line.StartsWith(name)).Split(' ');
            return entries[1];
        }

        private void readIP(List<string> lines, ConcurrentDictionary<string, IPAddress> nameToIp, ConcurrentDictionary<IPAddress, string> ipToName)
        {
            foreach (var line in lines.FindAll(line => line.StartsWith("ROW")))
            {
                string[] entries;
                entries = line.Split(' ');
                nameToIp.TryAdd(entries[1], IPAddress.Parse(entries[2]));
                ipToName.TryAdd(IPAddress.Parse(entries[2]), entries[1]);
            }
        }
    }
}