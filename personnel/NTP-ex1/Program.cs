using System;
using System.Net;
using System.Net.Sockets;
using NodaTime;
using NodaTime.TimeZones;
using static NodaTime.NetworkClock;
using System.Threading.Tasks;

namespace NTP_ex1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Configuration du NetworkClock
            var networkClock = NetworkClock.Instance;
            networkClock.NtpServer = "pool.ntp.org";
            networkClock.CacheTimeout = Duration.FromMinutes(15);

            try
            {
                // Obtenir l'heure précise via NTP
                Instant ntpTime = networkClock.GetCurrentInstant();
                Instant systemTime = SystemClock.Instance.GetCurrentInstant();

                Console.WriteLine($"Heure NTP (UTC): {ntpTime}");
                Console.WriteLine($"Heure système (UTC): {systemTime}");

                // Calcul du drift initial
                Duration drift = systemTime - ntpTime;
                Console.WriteLine($"Drift détecté: {drift.TotalNanoseconds / 1_000_000.0:F3} ms");

                ClockDriftAnalyzer dri = new ClockDriftAnalyzer();
                dri.MeasureDriftAsync();
                dri.DisplayDriftStatistics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur NetworkClock: {ex.Message}");
                // Fallback sur l'horloge système
            }

            /*Exercice 1
             * 
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

        // Exercise F: World Clock Display
        public static void DisplayWorldClocks(DateTime utcTime)
        {
            var timeZones = new[]
            {
                ("UTC", TimeZoneInfo.Utc),
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Tokyo", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                ("Sydney", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
            };

            foreach (var (name, tz) in timeZones)
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
                Console.WriteLine($"{name}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
        }*/
        }

        public class ClockDriftAnalyzer
        {
            private readonly List<DriftMeasurement> _measurements = new();
            private readonly NetworkClock _networkClock;
            private readonly IClock _systemClock;

            public ClockDriftAnalyzer()
            {
                _networkClock = NetworkClock.Instance;
                _systemClock = SystemClock.Instance;
            }

            public class DriftMeasurement
            {
                public Instant Timestamp { get; set; }
                public Duration SystemOffset { get; set; }
                public Duration NetworkLatency { get; set; }
                public double DriftRatePpm { get; set; } // Parts per million
            }

            public async Task<DriftMeasurement> MeasureDriftAsync()
            {
                var startTime = _systemClock.GetCurrentInstant();

                try
                {
                    // Mesurer le temps réseau avec plusieurs tentatives
                    var measurements = new List<(Instant ntpTime, Duration latency)>();

                    for (int i = 0; i < 5; i++)
                    {
                        var before = _systemClock.GetCurrentInstant();
                        var ntpTime = _networkClock.GetCurrentInstant();
                        var after = _systemClock.GetCurrentInstant();

                        var latency = after - before;
                        measurements.Add((ntpTime, latency));

                    }

                    // Sélectionner la mesure avec la latence minimale
                    var bestMeasurement = measurements.OrderBy(m => m.latency).First();
                    var systemTime = _systemClock.GetCurrentInstant();

                    var drift = new DriftMeasurement
                    {
                        Timestamp = systemTime,
                        SystemOffset = systemTime - bestMeasurement.ntpTime,
                        NetworkLatency = bestMeasurement.latency,
                        DriftRatePpm = CalculateDriftRate()
                    };

                    _measurements.Add(drift);
                    return drift;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur mesure drift: {ex.Message}");
                    return null;
                }
            }

            private double CalculateDriftRate()
            {
                if (_measurements.Count < 2) return 0.0;

                var first = _measurements.First();
                var last = _measurements.Last();

                var timeDiff = last.Timestamp - first.Timestamp;
                var offsetDiff = last.SystemOffset.TotalNanoseconds - first.SystemOffset.TotalNanoseconds;

                if (timeDiff.TotalNanoseconds == 0) return 0.0;

                // Calcul en parts par million (ppm)
                return (double)offsetDiff / timeDiff.TotalNanoseconds * 1_000_000;
            }

            public void DisplayDriftStatistics()
            {
                if (_measurements.Count == 0)
                {
                    Console.WriteLine("Aucune mesure disponible");
                    return;
                }

                var avgOffset = _measurements.Average(m => m.SystemOffset.TotalNanoseconds) / 1_000_000.0;
                var maxOffset = _measurements.Max(m => Math.Abs(m.SystemOffset.TotalNanoseconds)) / 1_000_000.0;
                var avgLatency = _measurements.Average(m => m.NetworkLatency.TotalNanoseconds) / 1_000_000.0;
                var currentDriftRate = _measurements.Last().DriftRatePpm;

                Console.WriteLine("=== STATISTIQUES DE DRIFT ===");
                Console.WriteLine($"Nombre de mesures: {_measurements.Count}");
                Console.WriteLine($"Offset moyen: {avgOffset:F3} ms");
                Console.WriteLine($"Offset maximal: {maxOffset:F3} ms");
                Console.WriteLine($"Latence réseau moyenne: {avgLatency:F3} ms");
                Console.WriteLine($"Taux de drift actuel: {currentDriftRate:F2} ppm");

                // Classification de la qualité
                if (Math.Abs(avgOffset) < 1.0)
                    Console.WriteLine("Qualité: EXCELLENTE (< 1ms)");
                else if (Math.Abs(avgOffset) < 10.0)
                    Console.WriteLine("Qualité: BONNE (< 10ms)");
                else if (Math.Abs(avgOffset) < 100.0)
                    Console.WriteLine("Qualité: ACCEPTABLE (< 100ms)");
                else
                    Console.WriteLine("Qualité: PROBLÉMATIQUE (> 100ms)");
            }
        }
        public class PredictiveDriftCorrector
        {
            private readonly ClockDriftAnalyzer _analyzer;
            private Duration _predictedDrift;

            public PredictiveDriftCorrector(ClockDriftAnalyzer analyzer)
            {
                _analyzer = analyzer;
            }

            public Instant GetCorrectedTime()
            {
                var systemTime = SystemClock.Instance.GetCurrentInstant();
                return systemTime - _predictedDrift;
            }

            public async Task CalibrateAsync(int measurementCount = 10, Duration interval = default)
            {
                if (interval == default) interval = Duration.FromSeconds(30);

                Console.WriteLine($"Calibration en cours ({measurementCount} mesures)...");

                for (int i = 0; i < measurementCount; i++)
                {
                    var measurement = await _analyzer.MeasureDriftAsync();
                    if (measurement != null)
                    {
                        _predictedDrift = measurement.SystemOffset;
                        Console.WriteLine($"Mesure {i + 1}/{measurementCount}: " +
                                        $"Offset = {measurement.SystemOffset.TotalNanoseconds / 1_000_000.0:F3} ms, " +
                                        $"Latence = {measurement.NetworkLatency.TotalNanoseconds / 1_000_000.0:F3} ms");
                    }

                    if (i < measurementCount - 1)
                        await Task.Delay((int)interval.TotalMilliseconds);
                }

                Console.WriteLine("Calibration terminée.");
                _analyzer.DisplayDriftStatistics();
            }
        }
    }
}
