namespace SourceGenerator.core;

[GenerateProxy] public record FooQuery(int bar, string sheep):IRequest;

public record BarCommand(int Polisnummer, string Opmerking):IRequest;
[GenerateProxy] public record AapCommand(string Naam):IRequest;

public static class Schaap
{
    [GenerateProxy] 
    public record SchaapCommand(string Naam):IRequest;

}
