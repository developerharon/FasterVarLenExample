using FASTER.client;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace FasterValLenApi.Models
{
    public class ProductStoreClient
    {
        private const string ip = "127.0.0.1";
        private const int port = 5000;
        private static Encoding _encode = Encoding.UTF8;
        private static ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        private readonly FasterKVClient<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> _client;
        private readonly ClientSession<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, (IMemoryOwner<byte>, int), byte, ProductMemoryFunctions, MemoryParameterSerializer<byte>> _session;

        public ProductStoreClient()
        {
            _client = new FasterKVClient<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(ip, port);
            _session = _client.NewSession(new ProductMemoryFunctions());
        }

        public void AddProduct(Product product)
        {
            int idLength = _encode.GetByteCount(product.Id.ToString());
            int productLength = _encode.GetByteCount(JsonSerializer.Serialize(product));

            byte[] idbytes = _pool.Rent(idLength);
            int bytesWritten = _encode.GetBytes(product.Id.ToString(), idbytes);
            var key = idbytes.AsMemory(0, bytesWritten);

            byte[] productBytes = _pool.Rent(productLength);
            bytesWritten = _encode.GetBytes(JsonSerializer.Serialize(product), productBytes);
            var value = productBytes.AsMemory(0, bytesWritten);

            _session.Upsert(key, value);
            // Flushes partially filled batches, does not wait for response
            _session.Flush();
        }


        public async Task<Product?> GetProductByIdAsync(int id)
        {
            int idLength = _encode.GetByteCount(id.ToString());

            byte[] idBytes = _pool.Rent(idLength);
            int bytesWritten = _encode.GetBytes(id.ToString(), idBytes);
            var key = idBytes.AsMemory(0, bytesWritten);

            Random r = new(100);
            var varLen = r.Next(1, 1000);
            var (status, output) = (await _session.ReadAsync(key));

            if (status.Found)
                return JsonSerializer.Deserialize<Product>(output.Item1.Memory.Span.Slice(0, output.Item2), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            else
                return null;
        }
    }
}