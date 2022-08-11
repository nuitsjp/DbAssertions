using System.Net;

namespace DbAssertions;

public class HostNameProvider : IHostNameProvider
{
    public string GetHostName() => Dns.GetHostName();
}