using FluentAssertions;
using SSH.Commander.Tests.Attributes;

namespace SSH.Commander.Tests;

public class TestSshConnect
{
    [ManualFact]
    public async Task EchoText()
    {
        // Arrange
        var client = new SshCommanderClient("sber1");
        var expectString = "123";

        // Act
        var response = await client.RequestAsync("echo 123");

        // Assert
        response.Trim().Should().Be(expectString);
    }

}
