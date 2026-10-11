using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MoeSoft.Services;

public static class ProxyHandlerFactory
{
    public static SocketsHttpHandler CreateHandler(bool useProxy, string? proxyAddress)
    {
        var handler = new SocketsHttpHandler();
        if (!useProxy)
        {
            return handler;
        }

        if (!Uri.TryCreate(proxyAddress?.Trim(), UriKind.Absolute, out var proxyUri) || string.IsNullOrWhiteSpace(proxyUri.Host))
        {
            throw new ArgumentException("代理地址无效。", nameof(proxyAddress));
        }

        if (proxyUri.Scheme.Equals("socks5", StringComparison.OrdinalIgnoreCase) ||
            proxyUri.Scheme.Equals("socks5h", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(proxyUri.UserInfo))
            {
                throw new NotSupportedException("当前不支持需要用户名或密码认证的 SOCKS5 代理。");
            }

            handler.ConnectCallback = (context, cancellationToken) =>
                ConnectThroughSocks5Async(proxyUri, context.DnsEndPoint, cancellationToken);
        }
        else if (proxyUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                 proxyUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            handler.Proxy = new WebProxy(proxyUri);
            handler.UseProxy = true;
        }
        else
        {
            throw new NotSupportedException("代理协议仅支持 HTTP、HTTPS 和 SOCKS5。");
        }

        return handler;
    }

    private static async ValueTask<Stream> ConnectThroughSocks5Async(
        Uri proxyUri,
        DnsEndPoint destination,
        CancellationToken cancellationToken)
    {
        int proxyPort = proxyUri.IsDefaultPort ? 1080 : proxyUri.Port;
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync(new DnsEndPoint(proxyUri.Host, proxyPort), cancellationToken);
            var stream = new NetworkStream(socket, ownsSocket: true);

            try
            {
                await stream.WriteAsync(new byte[] { 5, 1, 0 }, cancellationToken);
                byte[] greeting = await ReadExactlyAsync(stream, 2, cancellationToken);
                if (greeting[0] != 5 || greeting[1] != 0)
                {
                    throw new HttpRequestException("SOCKS5 代理不支持无认证连接。");
                }

                byte[] address;
                byte addressType;
                if (proxyUri.Scheme.Equals("socks5h", StringComparison.OrdinalIgnoreCase))
                {
                    string host = new IdnMapping().GetAscii(destination.Host);
                    address = Encoding.ASCII.GetBytes(host);
                    if (address.Length > byte.MaxValue)
                    {
                        throw new HttpRequestException("目标域名过长，无法通过 SOCKS5 代理连接。");
                    }

                    addressType = 3;
                }
                else if (IPAddress.TryParse(destination.Host, out var ipAddress))
                {
                    address = ipAddress.GetAddressBytes();
                    addressType = ipAddress.AddressFamily == AddressFamily.InterNetwork ? (byte)1 : (byte)4;
                }
                else
                {
                    IPAddress[] addresses = await Dns.GetHostAddressesAsync(destination.Host, cancellationToken);
                    IPAddress? resolvedAddress = addresses.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetworkV6)
                        ?? addresses.FirstOrDefault();
                    if (resolvedAddress is null)
                    {
                        throw new HttpRequestException("无法解析目标主机名。");
                    }

                    address = resolvedAddress.GetAddressBytes();
                    addressType = resolvedAddress.AddressFamily == AddressFamily.InterNetwork ? (byte)1 : (byte)4;
                }

                using var request = new MemoryStream();
                request.Write(new byte[] { 5, 1, 0, addressType });
                if (addressType == 3)
                {
                    request.WriteByte((byte)address.Length);
                }

                request.Write(address);
                request.WriteByte((byte)(destination.Port >> 8));
                request.WriteByte((byte)destination.Port);
                await stream.WriteAsync(request.ToArray(), cancellationToken);

                byte[] response = await ReadExactlyAsync(stream, 4, cancellationToken);
                if (response[0] != 5 || response[1] != 0)
                {
                    throw new HttpRequestException($"SOCKS5 代理连接目标失败，状态码：{response[1]}。");
                }

                int boundAddressLength = response[3] switch
                {
                    1 => 4,
                    3 => (await ReadExactlyAsync(stream, 1, cancellationToken))[0],
                    4 => 16,
                    _ => throw new HttpRequestException("SOCKS5 代理返回了无效响应。")
                };
                await ReadExactlyAsync(stream, boundAddressLength + 2, cancellationToken);
                return stream;
            }
            catch
            {
                await stream.DisposeAsync();
                throw;
            }
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static async Task<byte[]> ReadExactlyAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < buffer.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (bytesRead == 0)
            {
                throw new IOException("SOCKS5 代理意外关闭了连接。");
            }

            offset += bytesRead;
        }

        return buffer;
    }
}
