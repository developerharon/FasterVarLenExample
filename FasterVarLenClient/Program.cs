using FASTER.client;
using System.Buffers;
using System.Text;
using System.Text.Json;


TestClass.Run();

internal static class TestClass
{

    private const string ip = "127.0.0.1";
    private const int port = 5000;
    private static Encoding encode = Encoding.UTF8;
    private static ArrayPool<byte> pool = ArrayPool<byte>.Shared;


    public static void Run()
    {

        using var client1 = new FasterKVClient<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(ip, port);
        using var session = client1.NewSession(new MemoryFunctions()); // uses protocol WireFormat.DefaultVarLenKV by default

        SyncMemorySamples(session);
    }
    static void SyncMemorySamples(ClientSession<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, (IMemoryOwner<byte>, int), byte, MemoryFunctions, MemoryParameterSerializer<byte>> session)
    {
        var student = new Student
        {
            Id = 3,
            Name = "Emmanuel Changole",
            Course = "Computer Science"
        };

        int idLength = encode.GetByteCount(student.Id.ToString());
        int studentLength = encode.GetByteCount(JsonSerializer.Serialize(student));

        byte[] idBytes = pool.Rent(idLength);
        int bytesWritten = encode.GetBytes(student.Id.ToString(), idBytes);
        var key = idBytes.AsMemory(0, bytesWritten);


        byte[] studentBytes = pool.Rent(studentLength);
        bytesWritten = encode.GetBytes(JsonSerializer.Serialize(student), studentBytes);
        var value = studentBytes.AsMemory(0, bytesWritten);

        session.Upsert(key, value);

        // Flushes partially filled batches, does not wait for responses
        session.Flush();

        session.Read(key);
        session.CompletePending(true);
    }
}

public class MemoryFunctions : MemoryFunctionsBase<byte>
{
    public override void ReadCompletionCallback(ref ReadOnlyMemory<byte> key, ref ReadOnlyMemory<byte> input, ref (IMemoryOwner<byte>, int) output, byte ctx, Status status)
    {
        Memory<int> expected = new Memory<int>(new int[key.Span.Length]);

        var student = JsonSerializer.Deserialize<Student>(output.Item1.Memory.Span.Slice(0, output.Item2), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Console.WriteLine($"Output read: {student}");
        //for (int i = 0; i < output.Item2; i++)
        //{
        //    Console.WriteLine($"Output read: {output.Item1.Memory.Span[i]}");
        //}
    }
}

public class MemoryFunctionsByte : MemoryFunctionsBase<byte>
{
    public override void ReadCompletionCallback(ref ReadOnlyMemory<byte> key, ref ReadOnlyMemory<byte> input, ref (IMemoryOwner<byte>, int) output, byte ctx, Status status)
    {
        Console.WriteLine($"{nameof(this.ReadCompletionCallback)} 2 called");
        try
        {
            if (ctx == 0)
            {
                if (!status.Found || !key.Span.SequenceEqual(output.Item1.Memory.Span.Slice(0, output.Item2)))
                    throw new Exception("Incorrect read result");
            }
            else
            {
                throw new Exception("Unexpected user context");
            }
        }
        finally
        {
            output.Item1.Dispose();
        }
    }
}


public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Course { get; set; }

    public override string ToString()
    {
        return $"\nId: {Id}\nName: {Name}\nCourse: {Course}";
    }
}