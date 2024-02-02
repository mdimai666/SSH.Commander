using System;
using System.Linq;
using System.Net;
using System.Web;

namespace SSH.Commander
{
    public class SshUri
    {
        Uri _uri;
        public string? UserInfo { get; private set; }
        public string Host => _uri.Host;
        public int? Port { get; private set; }
        public string? User { get; private set; }
        public string? Password { get; private set; }

        public SshUri(string sshString)
        {
            sshString = EncodeLeft(sshString);
            sshString = "ssh://" + sshString;
            _uri = new Uri(sshString);

            var userInfoPass = !string.IsNullOrEmpty(_uri.UserInfo);

            UserInfo = userInfoPass ? HttpUtility.UrlDecode(_uri.UserInfo) : null;
            if (userInfoPass)
            {
                User = UserInfo?.Split(':', 2)[0];
            }
            if (userInfoPass && (UserInfo?.Contains(':') ?? false))
            {
                Password = UserInfo?.Split(':', 2)[1];
            }
            if (_uri.Port == -1) Port = null;
            else Port = _uri.Port;

        }

        public static string EncodeLeft(string sshString)
        {
            if (sshString.Contains("@"))
            {
                var ss = sshString.Split('@');
                var left = string.Join('@', ss.SkipLast(1));
                var right = ss.Last();
                sshString = $"{WebUtility.UrlEncode(left)}@{right}";
            }
            return sshString;
        }
    }
}