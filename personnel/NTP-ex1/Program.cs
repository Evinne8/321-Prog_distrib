using System;
using System.Net;
using System.Net.Sockets;

namespace NTP_ex1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
             * string ntpServer = "0.ch.pool.ntp.org";
            
            UdpClient udpClient = new UdpClient();
            byte[] timeMessage = new byte[48];
            timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
            IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
            using (UdpClient client = new UdpClient())
            {
                client.Connect(ntpReference);
                client.Send(timeMessage, timeMessage.Length);
                timeMessage = client.Receive(ref ntpReference);

                ulong intPart = (ulong)timeMessage[40] << 24 | (ulong)timeMessage[41] << 16 | (ulong)timeMessage[42] << 8 | (ulong)timeMessage[43];
                ulong fractPart = (ulong)timeMessage[44] << 24 | (ulong)timeMessage[45] << 16 | (ulong)timeMessage[46] << 8 | (ulong)timeMessage[47];

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
                DateTime ntpTime = networkDateTime;
                Console.WriteLine("Différent formats : ");
                Console.WriteLine($"Heure actuelle : {ntpTime.ToLongDateString()}");
                Console.WriteLine($"Heure actuelle : {ntpTime}");
                Console.WriteLine($"Heure actuelle : {ntpTime.ToShortDateString()}");
                Console.WriteLine($"Heure actuelle : {ntpTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");


                DateTime date = DateTime.Now;
                TimeSpan timeDiff = date - networkDateTime;
                Console.WriteLine($"Différence de temps : {timeDiff.TotalSeconds:F2} secondes");

                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(networkDateTime, TimeZoneInfo.Local);
                Console.WriteLine($"Heure locale : {localTime}");

                // 3. Convert to specific time zones
                TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                DateTime swissTime = TimeZoneInfo.ConvertTimeFromUtc(networkDateTime, swissTimeZone);
                Console.WriteLine($"Heure suisse : {swissTime}");

                TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
                DateTime backToUtc = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, utcTimeZone);
                Console.WriteLine($"Retour vers UTC : {backToUtc}");
                DisplayWorldClocks(networkDateTime);
                client.Close();
            }

            // NTP Server Pool and Reliability
            string[] ntpServers = {
                "0.pool.ntp.org",
                "1.pool.ntp.org",
                "time.google.com",
                "time.cloudflare.com"
            };

            // TODO : Try multiple servers for reliability
            ntpServers.ToList()
                .ForEach(ns =>
                {
                    IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ns)[0], 123);
                    using (UdpClient udpClient = new UdpClient())
                    {
                        byte[] timeMessage = new byte[48];
                        timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
                        udpClient.Connect(ntpReference);
                        Console.Write($"serveur : {ns}\t");
                        udpClient.Send(timeMessage,timeMessage.Length);
                        timeMessage = udpClient.Receive(ref ntpReference);
                        ulong intPart = (ulong)timeMessage[40] << 24 | (ulong)timeMessage[41] << 16 | (ulong)timeMessage[42] << 8 | (ulong)timeMessage[43];
                        ulong fractPart = (ulong)timeMessage[44] << 24 | (ulong)timeMessage[45] << 16 | (ulong)timeMessage[46] << 8 | (ulong)timeMessage[47];

                        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                        var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
                        DateTime ntpTime = networkDateTime;
                        Console.WriteLine($"time : {ntpTime}");
                        udpClient.Close();
                    }

                });
        }

        }
    }
}
