using FluentAssertions;
using SSH.Commander.Tests.Attributes;
using SSH.Commander.Tools;

namespace SSH.Commander.Tests
{
    public class TestServerStatTools
    {
        [ManualFact]
        public async Task GetServerStat()
        {
            // Arrange
            var client = new SshCommanderClient("sber1");

            // Act
            var stat = await client.GetServerStat();

            // Assert
            stat.Disk.CurrentGB.Should().BeGreaterThan(0);
            stat.Disk.TotalGB.Should().BeGreaterThan(0);
            stat.Disk.UsedPercent.Should().BeGreaterThan(0);
            stat.Mem.CurrentMB.Should().BeGreaterThan(0);
            stat.Mem.TotalMB.Should().BeGreaterThan(0);
            stat.Mem.LoadPercent.Should().BeGreaterThan(0);
        }
    }
}
