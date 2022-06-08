using FASTER.client;

Console.WriteLine("FASTER variable-length KV client");

using var client = new FasterKVClient<Student, Student>("127.0.0.1", 5000);

var session = client.NewSession(new StudentFunctions(), FASTER.common.WireFormat.DefaultVarLenKV);


internal class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Course { get; set; }

    public Student(string name, string course)
    {
        Name = name;
        Course = course;
    }
}

internal class StudentFunctions : CallbackFunctionsBase<Student, Student, Student, Student, byte>
{
    public override void ReadCompletionCallback(ref Student key, ref Student input, ref Student output, byte ctx, Status status)
    {
        base.ReadCompletionCallback(ref key, ref input, ref output, ctx, status);
    }
}