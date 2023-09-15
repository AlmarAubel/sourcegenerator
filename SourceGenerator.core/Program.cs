namespace SourceGenerator;

using System;
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
        proxy.ExecuteAapCommand("s");
        proxy.ExecuteSchaap_SchaapCommand("s");
        proxy.ExecutePieterSchaap();
    }
}

public partial  class FooBarProxy
{
    public void ExecutePieterSchaap()
    {
        var request = new Schaap.SchaapCommand("Pieter");
        _mediator.Execute(request);
    }
}
public class MediatrFake: IMediator
{
    public void Execute(IRequest request)
    {
        Console.WriteLine(request.ToString());
    }
}