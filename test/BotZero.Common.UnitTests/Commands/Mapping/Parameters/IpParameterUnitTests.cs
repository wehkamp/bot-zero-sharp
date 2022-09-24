using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Parameters;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BotZero.Common.UnitTests.Commands.Mapping.Parameters;

/// <remarks>Tests copied from: https://www.regextester.com/104038</remarks>
public class IpParameterUnitTests
{
    [Theory]
    //[InlineData("0.0.0.0")]
    [InlineData("9.255.255.255")]
    [InlineData("11.0.0.0")]
    [InlineData("126.255.255.255")]
    [InlineData("129.0.0.0")]
    [InlineData("169.253.255.255")]
    [InlineData("169.255.0.0")]
    [InlineData("172.15.255.255")]
    [InlineData("172.32.0.0")]
    [InlineData("191.0.1.255")]
    [InlineData("192.88.98.255")]
    [InlineData("192.88.100.0")]
    [InlineData("192.167.255.255")]
    [InlineData("192.169.0.0")]
    [InlineData("198.17.255.255")]
    [InlineData("223.255.255.255")]
    //[InlineData("1200:0000:AB00:1234:0000:2552:7777:1313")]
    //[InlineData("21DA:D3:0:2F3B:2AA:FF:FE28:9C5A")]
    //[InlineData("FE80:0000:0000:0000:0202:B3FF:FE1E:8329")]
    public async Task Matches(string ip)
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new IpParameter("ip")
            });

        var context = new ActionContext
        {
            Message = "test now " + ip
        };

        await tool.Process(context);
        lastInvokedContext.Should().NotBeNull();
        lastInvokedContext?.Values["ip"].Should().Be(ip);
    }

    [Theory]
    [InlineData("1.1.1.265", "wrong ipv4")]
    [InlineData("1.1.1.265:80", "no port support")]
    //[InlineData("1200:0000:AB00:1234:O000:2552:7777:1313", "invalid characters present")]
    //[InlineData("[2001:db8:0:1]:80", "no support for port numbers")]
    //[InlineData("http://[2001:db8:0:1]:80", "no support for IP address in a URL")]
    public async Task NoMatches(string ip, string reason)
    {
        ActionContext? lastInvokedContext = null;

        Task Add(ActionContext cm)
        {
            lastInvokedContext = cm;
            return Task.CompletedTask;
        }

        var tool = new Command("test");

        tool.AddAction(
            "now",
            Add,
            Array.Empty<string>(),
            new IParameter[] {
                new IpParameter("ip")
            });

        var context = new ActionContext
        {
            Message = "test now " + ip
        };

        await tool.Process(context);
        lastInvokedContext.Should().BeNull(reason);
    }
}
