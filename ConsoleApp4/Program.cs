using System.Net.Sockets;
using System.Net;
using System.Text;

string serverIP = "irc.freenode.net";
string allowIP = "*.freenode.net";

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(serverIP);
IPAddress ipAddress = ipHostInfo.AddressList[0];
IPEndPoint ipEndPoint = new(ipAddress, 6667);

using Socket client = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

async void send(string msg)
{
    Console.WriteLine($"Socket client sent message: \"{msg}\"");
    msg += "\r\n";
    var messageBytes = Encoding.UTF8.GetBytes(msg);
    _ = await client.SendAsync(messageBytes, SocketFlags.None);
}

Task askToSend = Task.Run(() =>
{
    while (true)
    {
        var msg = Console.ReadLine();
        if (msg.StartsWith("/join"))
        {
            send($"JOIN {msg.Split(" ")[1]}");
        } else
        {
            send(msg);
        }
    }
});

await client.ConnectAsync(ipEndPoint);
send("PASS Test1231!");
send("NICK fulgur");
send("USER fulgur blah blah fulgur");
while (true)
{
    var buffer = new byte[1_024];
    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);
    if (response.StartsWith("PING"))
    {
        send(response.Replace("PING", "PONG"));
    } else if (response.StartsWith(":"))
    {
        var args1 = response.Split(" ")[0];
        response = response.Replace(args1, "");
        Console.WriteLine(response);
    }
    else
    {
        Console.WriteLine(response);
    }
}

client.Shutdown(SocketShutdown.Both);