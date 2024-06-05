using System.Net.Sockets;
using System.Net;
using System.Text;

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("irc.freenode.net");
IPAddress ipAddress = ipHostInfo.AddressList[0];
IPEndPoint ipEndPoint = new(ipAddress, 6667);

using Socket client = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

async void send(string msg)
{
    msg += "\r\n";
    var messageBytes = Encoding.UTF8.GetBytes(msg);
    _ = await client.SendAsync(messageBytes, SocketFlags.None);
    Console.WriteLine($"Socket client sent message: \"{msg}\"");
}

Task askToSend = Task.Run(() =>
{
    while (true)
    {
        var msg = Console.ReadLine();
        send(msg);
    }
});

await client.ConnectAsync(ipEndPoint);
send("PASS none");
send("NICK fulgur");
send("USER blah blah blah blah");
while (true)
{
    var buffer = new byte[1_024];
    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);
    if (response.StartsWith("PING"))
    {
       send(response.Replace("PING", "PONG"));
    } else
    {
        Console.WriteLine(response);
    }
}

client.Shutdown(SocketShutdown.Both);