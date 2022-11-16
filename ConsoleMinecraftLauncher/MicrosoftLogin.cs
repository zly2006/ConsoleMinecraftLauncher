using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;

namespace ConsoleMinecraftLauncher;
/// <summary>
/// Why: in Windows, there will be a 
/// </summary>
public class MicrosoftLogin
{
    public string listenAddress = "";
    TcpListener listener;
    public string code;
    public string clientID;

    public string BuildRequest()
    {
        return
            $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=6731de76-14a6-49ae-97bc-6eba6914391e&response_type=code&redirect_uri={UrlEncoder.Default.Encode(listenAddress)}&response_mode=query&scope=openid%20offline_access%20XboxLive.signin&state=12345";
    }
    
    public void StartListener(int port = 50012)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listenAddress = $"http://localhost:{port}";
        listener.Start();
        Task task = new Task(() =>
        {
            while (true)
            {
                var clientSocket = listener.AcceptSocket();
                var httpReq = new byte[1024];
                clientSocket.Receive(httpReq);
                var req = Encoding.UTF8.GetString(httpReq);
                Console.WriteLine(req);
                listener.Stop();
            }
        });
    }
}