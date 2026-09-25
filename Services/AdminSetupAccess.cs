using System.Net;

namespace ShopMobileWorld.Services;

public static class AdminSetupAccess
{
    public static bool IsAllowed(HttpContext context, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return false;
        var host = context.Request.Host.Host;
        if (host != "localhost" && host != "127.0.0.1" && host != "[::1]" && host != "::1") return false;
        var remote = context.Connection.RemoteIpAddress;
        if (remote is null) return false;
        if (remote.IsIPv4MappedToIPv6) remote = remote.MapToIPv4();
        return IPAddress.IsLoopback(remote);
    }
}
