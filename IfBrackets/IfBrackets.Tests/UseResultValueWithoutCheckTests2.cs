using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Testing.CSharp;
using Xunit;

namespace IfBrackets.Tests;

public class UseResultValueWithoutCheckTests2 : AbstractCSharpDiagnosticVerifier
    <UseResultValueWithoutCheck, DummyCodeFixProvider>
{
    public override DiagnosticDescriptor Descriptor => UseResultValueWithoutCheck.Rule;

    [Fact]
    public async Task AccesValueOnResultObject_WithoutcheckingIsSuccess_ShouldWarn2()
    {
        await VerifyDiagnosticAsync(
            AddContext("""
                              if(!result.IsSuccess) Console.WriteLine( [|result.Value|]);
                              if(result.IsSuccess == false) Console.WriteLine([|result.Value|]);
                              if(result.IsSuccess == false || new Random().Next() > 1) Console.WriteLine([|result.Value|]);
                              var x=  a > 0 ? [|result.Value|]: 0;
                       """), options: CSharpTestOptions());
    }
    
    [Fact]
    public async Task AccesValueOnResultObject_WithoutcheckingIsSuccessAndEarlyReturn_ShouldWarn()
    {
        await VerifyDiagnosticAsync(
            AddContext("""
                              Console.WriteLine([|result.Value|]);
                       """), options: CSharpTestOptions());
    }
    
    [Fact]
    public async Task AccesValueOnResultObject_WithcheckingIsSuccessAndEarlyReturn_ShouldPass()
    {
        await VerifyNoDiagnosticAsync(
            AddContext("""
                              if(!result.IsSuccess) return;
                              Console.WriteLine(result.Value);
                       """), options: CSharpTestOptions());
    }
    
    [Fact]
    public async Task AccesValueOnResultObject_WithCheckIsSuccess_ShouldPass()
    {
        await VerifyNoDiagnosticAsync(
            AddContext("""
                       if(result.IsSuccess) Console.WriteLine(result.Value);
                       if(result.IsSuccess == true) Console.WriteLine(result.Value);
                       if(result.IsSuccess && new Random().Next() > 1) Console.WriteLine(result.Value);
                       var x=  result.IsSuccess ? result.Value: 0;
                       """), options: CSharpTestOptions());
    }

    [Fact]
    public async Task AccesValueOnResultObject_WithCComplexIsSuccess_ShouldFail()
    {
        await VerifyDiagnosticAsync(
            AddContext("""
                       if(result.IsSuccess || new Random().Next() > 1) Console.WriteLine([|result.Value|]);
                       if(result.IsFailure || new Random().Next() > 1) Console.WriteLine([|result.Value|]);
                       if(result.IsFailure && new Random().Next() > 1) Console.WriteLine([|result.Value|]);
                       """), options: CSharpTestOptions());
    }
    
    [Fact]
    public async Task AccesValueOnResultObject_WithoutcheckingIsFailure_ShouldWarn()
    {
        
        await VerifyDiagnosticAsync(
            AddContext("""
                              if(result.IsFailure) Console.WriteLine([|result.Value|]);
                              var x=  result.IsFailure ? [|result.Value|]: 0;
                       """),
            options: CSharpTestOptions());
    }

    [Fact]
    public async Task AccesValueOnResultObject_WithcheckingIsFailure_ShouldPass()
    {
        await VerifyNoDiagnosticAsync(
            AddContext("""
                       if(!result.IsFailure) Console.WriteLine(result.Value);
                       var x =  !result.IsFailure ? result.Value: 0;
                       if (result.IsFailure) return;
                       var y = result.Value;
                       """),
            options: CSharpTestOptions());
    }


    private CSharpTestOptions CSharpTestOptions()
    {
        var cSharpFunctionalExtensions = MetadataReference.CreateFromFile(typeof(CSharpFunctionalExtensions.Result).Assembly.Location);
        var cSharpTestOptions = Options.WithMetadataReferences(Options.MetadataReferences.Add(cSharpFunctionalExtensions));
        return cSharpTestOptions;
    }

    private string AddContext(string testString) =>
        $$"""
          using System;
          using CSharpFunctionalExtensions;

          public class FunctionsWithResultObject
          {
              public void GetId(int a)
              {
                 var result = Result.Success(1);
                 {{testString}}
              }
          }
          """;
}