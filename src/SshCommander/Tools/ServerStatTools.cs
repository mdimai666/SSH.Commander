using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SSH.Commander.Models;

namespace SSH.Commander.Tools
{
    public static class ServerStatTools
    {
        public static async Task<ServerStat> GetServerStat(this SshCommanderClient client)
        {
            //SshUri sshUri = new SshUri(connectString);
            //                    string command2 =
            //                @"free -m | awk 'NR==2{printf ""Memory Usage: %s/%sMB (%.2f%%)\n"", $3,$2,$3*100/$2 }'
            //df -h | awk '$NF==""/""{printf ""Disk Usage: %d/%dGB (%s)\n"", $3,$2,$5}'
            //top -bn1 | grep load | awk '{printf ""CPU Load: %.2f\n"", $(NF-2)}' ";

            //https://gist.github.com/walm/e084e5184bc14da9ddbe
            string[] commands =
                new string[]
                {
                            @"echo -n '{'",
                            //# memory as "mem": { "current": 800, "total": 1024, "load", 82 } where amount is in MB and load in %
                            @"free -m | awk 'NR==2{printf ""\""mem\"": { \""current\"":%d, \""total\"":%d, \""load\"": %.2f }"", $3,$2,$3*100/$2 }'",
                            @"echo -n ','",
                            //# diska as "disk": { "current": 6, "total": 40, "used": 19 } where amount is in GB and used in %
                            @"df -h | awk '$NF==""/""{printf ""\""disk\"": { \""current\"":%d, \""total\"":%d, \""used\"": %d }"", $3,$2,$5}'",
                            @"echo -n ','",
                            //# cpu as "cpu": { "load": 40 } where load is in %
                            @"top -bn1 | grep load | awk '{printf ""\""cpu\"": { \""load\"": %.2f }"", $(NF-2)}'",
                            @"echo -n '}'",
                };

            /*
            example:    
            {
               "cpu" : {
                  "load" : 0
               },
               "disk" : {
                  "used" : 19,
                  "current" : 6,
                  "total" : 40
               },
               "mem" : {
                  "current" : 1814,
                  "load" : 90.61,
                  "total" : 2002
               }
            }
             */
            string command = string.Join(" &&\\\n", commands);

            string output = await client.RequestAsync(command);

            //Console.WriteLine($"======== {sshUri.Host} ========");
            //Console.WriteLine(output);
            //return output;

            var stats = JsonSerializer.Deserialize<ServerStat>(output.Trim())!;
            return stats;

            //https://askubuntu.com/questions/726333/how-to-save-htop-output-to-file - htop to html
        }

        public static async Task<IReadOnlyList<ServerStat>> GetServersStatsAsync(params string[] sshStrings)
        {
            //string[] sshStrings = ["sber-node01", "sber1", "web:Admin123!@web100"];
            //string[] sshStrings = new string[] { "sber-node01" };
            //string[] sshStrings = ["dexp_z250"];

            var tasks = sshStrings.Select(connectString => (connectString, new SshCommanderClient(connectString).GetServerStat())).ToList();
            await Task.WhenAll(tasks.Select(task => task.Item2));
            var stats = tasks.Select(task => task.Item2.Result).ToList();

            return stats;

            //var stats = configContents.Select(st => JsonSerializer.Deserialize<ServerStat>(st)).ToList();
        }
    }
}
