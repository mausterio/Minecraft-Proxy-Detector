using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Minecraft_Proxy_Detector
{
    class Program
    {
        static string proxy_checker(string s)
        {
            using (var client = new WebClient())
            {
                try
                {
                    // TODO: Add multiple API's so that if one goes down or rate limit hit, program is still functional.
                    var json = client.DownloadString("https://ip.teoh.io/api/vpn/" + s);
                    dynamic data = JObject.Parse(json);
                    // Returns whether using vpn or not
                    return (data.vpn_or_proxy);  // vpn? yes / no
                } catch (Exception) {
                    Console.WriteLine("API is not working and/or responding!");
                    return "no";
                }
            }
        }
        static void Main(string[] args)
        {
            string log_location = "C:/Users/Administrator/Desktop/All+the+Mods+3+Remix+-+v1.3.0-Serverfiles-FULL/logs/latest.log";
            string output_location = "C:/Users/Administrator/Desktop/vpn.txt";
            var wh = new AutoResetEvent(false);
            var fsw = new FileSystemWatcher(".");
            fsw.Filter = log_location;
            fsw.EnableRaisingEvents = true;
            fsw.Changed += (s, e) => wh.Set();

            var fs = new FileStream(log_location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                var s = "";
                while (true)
                {
                    s = sr.ReadLine();
                    if (s != null)
                    {
                        // Match format: [/0.0.0.0
                        Match match = Regex.Match(s, @"(?:\[)(?:\/)(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])");
                        if (match.Success)
                        {
                            // Outputs new connections with position coordinates, and IP address with Port
                            s = match.Value;

                            // Removes the initial [/
                            s = s.Replace("[/", "");
                            
                            if (proxy_checker(s) == "yes")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(s + " is likely a Proxy or VPN!");
                                Console.ResetColor();

                                // Outputs possible VPN IP addresses to vpn.txt so that they can be reviewed.
                                File.AppendAllText(output_location, (s + Environment.NewLine));

                            }
                        }

                    } else {
                        wh.WaitOne(1000);
                    }
                }
            }
            wh.Close();
        }

    }
}
