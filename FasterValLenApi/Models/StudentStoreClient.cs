using FASTER.client;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace FasterValLenApi.Models
{
    public class StudentStoreClient
    {
        private const string ip = "127.0.0.1";
        private const int port = 5002;
        private static Encoding _encode = Encoding.UTF8;
        private static ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        private readonly FasterKVClient<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> _client;
        private readonly ClientSession<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, (IMemoryOwner<byte>, int), byte, ProductMemoryFunctions, MemoryParameterSerializer<byte>> _session;

        public StudentStoreClient()
        {
            _client = new FasterKVClient<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(ip, port);
            _session = _client.NewSession(new ProductMemoryFunctions());
        }

       public void AddStudent(Student student)
        {
            int idLength = _encode.GetByteCount(student.Id.ToString());
            int studentLength = _encode.GetByteCount(JsonSerializer.Serialize(student));

            byte[] idbytes = _pool.Rent(idLength);
            int bytesWritten = _encode.GetBytes(student.Id.ToString(), idbytes);
            var key = idbytes.AsMemory(0, bytesWritten);

            byte[] studentBytes = _pool.Rent(studentLength);
            bytesWritten = _encode.GetBytes(JsonSerializer.Serialize(student), studentBytes);
            var value = studentBytes.AsMemory(0, bytesWritten);

            _session.Upsert(key, value);
            // Flushes partially filled batches, does not wait for response
            _session.Flush();
        }


        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            int idLength = _encode.GetByteCount(id.ToString());

            byte[] idBytes = _pool.Rent(idLength);
            int bytesWritten = _encode.GetBytes(id.ToString(), idBytes);
            var key = idBytes.AsMemory(0, bytesWritten);

            var (status, output) = (await _session.ReadAsync(key));

            if (status.Found)
                return JsonSerializer.Deserialize<Student>(output.Item1.Memory.Span.Slice(0, output.Item2), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            else
                return null;
        }
    }
}
