using FASTER.server;

Console.WriteLine("FASTER variable-length KV server");

var path = Path.GetTempPath() + "faster/";

var serverOptions = new ServerOptions()
{
    Port = 5000,
    Address = "127.0.0.1",
    MemorySize = "16g",
    PageSize = "32m",
    SegmentSize = "1g",
    IndexSize = "8g",
    EnableStorageTier = false,
    LogDir = null,
    CheckpointDir = null,
    Recover = false,
    DisablePubSub = false,
    PubSubPageSize = "4k"
};

using var server = new VarLenServer(serverOptions);
server.Start();


Console.WriteLine("Server started.");

Thread.Sleep(Timeout.Infinite);