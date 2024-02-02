using FluentAssertions;

namespace SSH.Commander.Tests;

public class TestSshUri
{
    [Fact]
    public void ParseSshString()
    {
        // Arrange
        var user = "dima";
        var pass = "P@s$sW0rD123!";
        var host = "hostname.ru";
        var port = 22;

        var ssh1 = "dima:P@s$sW0rD123!@hostname.ru:22";
        var ssh2 = "dima@hostname.ru";
        var ssh3 = "hostname.ru:22";


        // Act
        var s1 = new SshUri(ssh1);
        var s2 = new SshUri(ssh2);
        var s3 = new SshUri(ssh3);

        // Assert
        s1.User.Should().Be(user);
        s1.Password.Should().Be(pass);
        s1.Host.Should().Be(host);
        s1.Port.Should().Be(port);
        s1.UserInfo.Should().NotBeNull();

        s2.User.Should().Be(user);
        s2.Password.Should().BeNull();
        s2.Port.Should().BeNull();

        s3.UserInfo.Should().BeNull();
        s3.Port.Should().NotBeNull();
    }
}