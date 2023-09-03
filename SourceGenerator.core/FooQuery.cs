namespace SourceGenerator;

public record FooQuery(int bar, string sheep):IRequest;

public record BarCommand(int Polisnummer, string Opmerking):IRequest;
public record AapCommand(string Naam):IRequest;

//This needs to be generated
// public class FooBarProxy
// {
//   private IMediator _mediator;
//
//   public FooBarProxy(IMediator mediator)
//   {
//     _mediator = mediator;
//   }
//
//   public void ExecuteFooQuery(FooQuery query)
//   {
//      _mediator.Execute(query);
//   }
//   public void ExecuteBarCommand(BarCommand command)
//   {
//       _mediator.Execute(command);
//   }
// }