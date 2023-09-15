using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier =
    IfBrackets.Tests.AnalyzerVerifier<IfBrackets.UseResultValueWithoutCheck>;

namespace IfBrackets.Tests;  

public class UseResultValueWithoutCheckTests
{
    [Fact]
    public async Task AccesValueOnResultObjectWithoutcheckingIsSuccessOrFailureShouldWarn()
    {
        var testCode = """
                       using System;
                       using CSharpFunctionalExtensions;

                       namespace IfBrackets.Sample;

                       public class FunctionsWithResultObject
                       {
                           public Result<int, string> GetId()
                           {
                               var idFromDbResult = GetIdFromDb();
                               
                               Console.WriteLine(idFromDbResult.Value); //This is dangerous because we didn't check if the result was succesfull
                               return idFromDbResult;
                           }
                       
                           private Result<int, string> GetIdFromDb() => "This is an error";
                       }
                       """;

        var expected = Verifier.Diagnostic(UseResultValueWithoutCheck.DiagnosticId)
            .WithLocation(8, 13)
            .WithMessage("Accessing Value without checking IsSuccess or IsError can result in an error.");

        await Verifier.VerifyAnalyzerAsync(testCode, expected);
    }
    
    [Fact]
    public async Task AccesValueOnResultObjectWithcheckingShouldPass()
    {
        var testCode = """
                       using System;
                       using CSharpFunctionalExtensions;

                       namespace IfBrackets.Sample;

                       public class FunctionsWithResultObject
                       {
                           public Result<int, string> GetId()
                           {
                               var idFromDbResult = GetIdFromDb();
                               var x= 10;
                               
                               if (idFromDbResult.IsSuccess)
                              {
                                  Console.WriteLine(idFromDbResult.Value);
                              }
                              
                               if (!idFromDbResult.IsSuccess) return "gaat niet goed";
                               Console.WriteLine(idFromDbResult.Value); //This is dangerous because we didn't check if the result was succesfull
                               return idFromDbResult;
                           }
                       
                           private Result<int, string> GetIdFromDb() => "This is an error";
                       }
                       """;

        var expected = Verifier.Diagnostic(UseResultValueWithoutCheck.DiagnosticId)
            .WithLocation(8, 13)
            .WithMessage("Accessing Value without checking IsSuccess or IsError can result in an error.");

        await Verifier.VerifyAnalyzerAsync(testCode, expected);
    }
}