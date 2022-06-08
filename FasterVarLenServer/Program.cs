using FASTER.server;

Console.WriteLine("FASTER variable-length KV server");

var serverOptions = new ServerOptions()
{
    Port = 5000,
    Address = "127.0.0.1",
};

using var server = new VarLenServer(serverOptions);
server.Start();

Console.WriteLine("Server started.");

Thread.Sleep(Timeout.Infinite);