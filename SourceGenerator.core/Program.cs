namespace SourceGenerator;

public static partial class Program
{
    public static void Main(string[] args)
    {
        Almar.HelloFrom("Generated Code");
        if (args[0] == "A")
        {
            throw new Exception("ddd");
        }

        var proxy = new FooBarProxy(new MediatrFake());
        proxy.ExecuteAapCommand(new AapCommand("s"));
    }
}

public class MediatrFake: IMediator
{
    public void Execute(IRequest request)
    {
        Console.WriteLine(request.ToString());
    }
}