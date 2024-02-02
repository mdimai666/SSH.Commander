using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Algorithms;
using Microsoft.DevTunnels.Ssh.Messages;
using Microsoft.DevTunnels.Ssh.Tcp;
using SshConfigParser;

namespace SSH.Commander
{
    public class SshCommanderClient
    {
        public class SshCommanderConfig
        {
            public bool UseUserSshConfigFile { get; set; } = true;
            public string SshConfigFilePath { get; set; } = "~/.ssh/config";
            public bool VerboseConsoleLog { get; set; } = false;

        }

        public SshCommanderConfig Config { get; set;/*init*/ } = new SshCommanderConfig();

        string SshConfigFilePathNormalized => ReplaceTildaAsUserDir(Config.SshConfigFilePath);

        string userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        SshUri sshUri;

        string user;
        int port;
        string hostName;
        string? password;
        string? identityFile;

        /// <inheritdoc cref="SshCommanderClient(string)"/>
        public SshCommanderClient(SshUri sshUri)
        {
            this.sshUri = sshUri;

            if (Config.UseUserSshConfigFile)
            {
                //https://github.com/JeremySkinner/Ssh-Config-Parser
                var config = SshConfig.ParseFile(SshConfigFilePathNormalized);
                var configHost = config.Compute(sshUri.Host);

                port = sshUri.Port ?? configHost.Port ?? 22;
                identityFile = configHost.IdentityFile;
                user = sshUri.User ?? configHost.User ?? Environment.UserName;
                hostName = configHost.HostName ?? sshUri.Host;
            }
            else
            {
                port = sshUri.Port ?? 22;
                user = sshUri.User ?? Environment.UserName;
                hostName = sshUri.Host;
            }
            password = sshUri.Password;
        }

        /// <summary>
        /// SshUri or Host from ~/.ssh/config
        /// <para>
        /// <example>host</example>
        /// <para>
        /// </para>
        /// <example>user:pass@host:22</example>
        /// </para>
        /// </summary>
        /// <param name="sshUri"></param>
        public SshCommanderClient(string sshUri) : this(new SshUri(sshUri)) { }

        async Task<(SshClient, SshClientSession, SshChannel)> Connect()
        {

            //----------Configure

            SshClientCredentials credentials;

            if (identityFile != null)
            {
                IKeyPair privateKey = KeyPair.ImportKeyFile(ReplaceTildaAsUserDir(identityFile));
                credentials = new SshClientCredentials(user, privateKey);
            }
            else
            {
                credentials = new SshClientCredentials(user, password);
            }

            ///----------Connect

            var client = new SshClient(SshSessionConfiguration.Default,
                               new TraceSource(nameof(SshClient)));
            SshClientSession session = await client.OpenSessionAsync(hostName, port);

            // Handle server public key authentication.
            session.Authenticating += (_, e) =>
            {
                e.AuthenticationTask = Task.Run<ClaimsPrincipal?>(() =>
                {
                    // TODO: Validate the server's public key.
                    // Return null if validation failed.
                    IKeyPair hostKey = e.PublicKey!;

                    var serverIdentity = new ClaimsIdentity();
                    return new ClaimsPrincipal(serverIdentity);
                });
            };


            if (!await session.AuthenticateAsync(credentials))
            {
                throw new Exception("Authentication failed.");
            }
            Log(">Authentication done!");

            // Open a channel, send a command, and read the command result.
            SshChannel channel = await session.OpenChannelAsync();
            channel.Request += Channel_Request;
            void Channel_Request(object? sender, Microsoft.DevTunnels.Ssh.Events.SshRequestEventArgs<ChannelRequestMessage> e)
            {
                Log(">Channel_Request: " + e.RequestType);
            }

            return (client, session, channel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">sh command</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="SshChannelException"></exception>
        public async Task<string> RequestAsync(string command, CancellationToken cancellationToken = default)
        {
            if (command.StartsWith("sudo")) throw new NotImplementedException("sudo not support");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (client, session, channel) = await Connect();

            //----------Execute command           
            bool commandAuthorized = await channel.RequestAsync(new CommandRequestMessage(command), cancellationToken);
            string result;

            try
            {
                if (commandAuthorized)
                {
                    Log($">commandAuthorized: '{command}'");
                    using (var channelStream = new SshStream(channel))
                    {
                        result = await new StreamReader(channelStream).ReadToEndAsync();
                        Log(">command complete!");
                    }
                }
                else
                {
                    result = "";
                    Log(">commandAuthorized FAIL!");
                }
            }
            finally
            {
                await channel.CloseAsync();
                stopwatch.Stop();
                Log($"Processing ssh CommandRequestMessage Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds");

                channel?.Dispose();
                session?.Dispose();
                client?.Dispose();
            }
            return result;
        }

        /// <inheritdoc cref="SshChannel.SendAsync(Microsoft.DevTunnels.Ssh.Buffer, CancellationToken)" />
        public async Task SendAsync(string command, CancellationToken cancellationToken = default)
        {
            if (command.StartsWith("sudo")) throw new NotImplementedException("sudo not support");

            var (client, session, channel) = await Connect();
            try
            {
                //----------Execute command           
                await channel.SendAsync(new CommandRequestMessage(command).ToBuffer(), cancellationToken);
            }
            finally
            {
                await channel.CloseAsync();
                channel?.Dispose();
                session?.Dispose();
                client?.Dispose();
            }
        }

        string ReplaceTildaAsUserDir(string path)
        {
            return Config.SshConfigFilePath.StartsWith('~') ? Path.Join(userPath, path.Substring(2).ToString()) : Config.SshConfigFilePath;
        }

        public void Log(string message)
        {
            if (Config.VerboseConsoleLog)
            {
                Console.WriteLine(message);
            }
        }
    }

}
