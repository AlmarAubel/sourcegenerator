using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using  Roslynator.Testing.CSharp;

using Xunit;

using Roslynator.Testing.CSharp;

namespace IfBrackets.Tests;  

public class UseResultValueWithoutCheckTests2:AbstractCSharpDiagnosticVerifier<IfBrackets.UseResultValueWithoutCheck,DummyCodeFixProvider>
{
    public override DiagnosticDescriptor Descriptor { get; } = IfBrackets.UseResultValueWithoutCheck.Rule;
    
    [Fact]
    public async Task AccesValueOnResultObjectWithoutcheckingIsSuccessOrFailureShouldWarn()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyDiagnosticAsync(
            """
            using System;
            using CSharpFunctionalExtensions;

            namespace IfBrackets.Sample;

            public class FunctionsWithResultObject
            {
                public Result<int, string> GetId()
                {
                    var idFromDbResult = GetIdFromDb();
                    if (!idFromDbResult.IsSuccess)
                        Console.WriteLine(idFromDbResult.Value); //This is dangerous because we didn't check if the result was succesfull
                    return idFromDbResult;
                }
            
                private Result<int, string> GetIdFromDb() => "This is an error";
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }  
    [Fact]
    public async Task AccesValueOnResultObjectWithoutcheckingIsSuccessOrFailureShouldWarn2()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyDiagnosticAsync(
            """
            using System;
            using CSharpFunctionalExtensions;

            namespace IfBrackets.Sample;

            public class FunctionsWithResultObject
            {
                public int GetId()
                {
                    var idFromDbResult = GetIdFromDb();
                  
                   var b = 0;
                   if(!idFromDbResult.IsSuccess) 
                    b= [|idFromDbResult.Value|];
                   
                   return b;
                }
            
                private Result<int, string> GetIdFromDb() => "This is an error";
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }
    
}