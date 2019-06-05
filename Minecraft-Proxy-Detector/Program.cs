using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Configuration;
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
                    var json = client.DownloadString(ConfigurationManager.AppSettings["API_URL"] + s);
                    dynamic data = JObject.Parse(json);
                    // Returns whether using vpn or not
                    // TODO: Find a way of allowing the configuration of data.[API_FIELD]
                    return data.vpn_or_proxy;  // vpn? yes / no
                } catch (Exception) {
                    Console.WriteLine("Error: API is not working or is not responding!");
                    return "no";
                }
            }
        }
        static void Main(string[] args)
        {
            string log_location = ConfigurationManager.AppSettings["LogLocation"];
            string output_location = ConfigurationManager.AppSettings["OutputLocation"];
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
                            
                            if (proxy_checker(s) == ConfigurationManager.AppSettings["API_VALUE"])
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(s + " is likely a Proxy or VPN!");
                                Console.ResetColor();

                                // Outputs possible VPN IP addresses to vpn.txt so that they can be reviewed.
                                File.AppendAllText(output_location, (s + Environment.NewLine));

                            } else if (ConfigurationManager.AppSettings["Verbose"] == "true")
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(s + " is likely not a Proxy or VPN!");
                                Console.ResetColor();
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
