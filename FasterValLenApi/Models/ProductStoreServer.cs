using FASTER.server;
using System.Diagnostics;

namespace FasterValLenApi.Models
{
    public class ProductStoreServer : BackgroundService
    {
        private readonly ServerOptions _serverOptions;
        private readonly VarLenServer _server;

        public ProductStoreServer()
        {
            _serverOptions = new ServerOptions()
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

            _server = new VarLenServer(_serverOptions);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _server.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                Debug.WriteLine("ProductStore is running");
                await Task.Delay(1000);
            }
        }

        public override void Dispose()
        {
            _server.Dispose();
            base.Dispose();
        }
    }
}
