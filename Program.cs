using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EDFab_Telemetry_Server
{
    class Program
    {
        static void Main()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket sensor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.ReceiveTimeout = 3000;
            client.SendTimeout = 3000;
            sensor.ReceiveTimeout = 3000;
            sensor.SendTimeout = 3000;
            client.Bind(new IPEndPoint(IPAddress.Any, 3000));
            sensor.Bind(new IPEndPoint(IPAddress.Any, 3001));
            client.Listen(0);
            sensor.Listen(0);
            Console.WriteLine("[+] Server Listening on Port 3000 & 3001");
            while (true)
            {
                if (client.Poll(1000, SelectMode.SelectRead))
                {
                    Task.Run(() => ClientHandler(new SecureSockets(client.Accept())));
                }
                if (sensor.Poll(1000, SelectMode.SelectRead))
                {
                    Task.Run(() => SensorHandler(sensor.Accept()));
                }
            }
        }
        static void ClientHandler(SecureSockets s)
        {
            try
            {
                PacketData packet = s.Receive();
                switch (packet.Request)
                {
                    case "Auth":
                        s.Send(Logins.Parse(packet));
                        break;

                    case "User":
                        s.Send(User.Parse(packet));
                        break;

                    case "Admin":
                        s.Send(Admin.Parse(packet));
                        break;
                }
                s.Close();
            }
            catch (Exception e) { Console.WriteLine(e.Message); s.Close(); }
        }
        static void SensorHandler(Socket s)
        {
            try
            {
                byte[] bytes = new byte[1024];
                int count = s.Receive(bytes);
                Sensors.Parse(Encoding.UTF8.GetString(bytes, 0, count), ((IPEndPoint)s.RemoteEndPoint).Address.ToString());
                s.Close();
            }
            catch (Exception) { s.Close(); }
        }
    }
}
