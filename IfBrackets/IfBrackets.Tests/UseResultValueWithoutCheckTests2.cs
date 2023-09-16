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
    public async Task AccesValueOnResultObjectWithoutcheckingIsSuccessOrFailureShouldWarn2()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyDiagnosticAsync(
            """
            using System;
            using CSharpFunctionalExtensions;

            public class FunctionsWithResultObject
            {
                public void GetId(int a)
                {
                   var result = Result.Success(1);
                   if(!result.IsSuccess) Console.WriteLine( [|result.Value|]);
                   if(result.IsSuccess == false) Console.WriteLine([|result.Value|]);
                   var x=  a > 0 ? [|result.Value|]: 0;
                }
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }
    
    [Fact]
    public async Task AccesValueOnResultObjectWithcheckingIsSuccessShouldNotFail()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyDiagnosticAsync(
            """
            using System;
            using CSharpFunctionalExtensions;

            public class FunctionsWithResultObject
            {
                public void GetId(int a)
                {
                   var result = Result.Success(1);
                   if(result.IsSuccess) Console.WriteLine(result.Value);
                   if(result.IsSuccess == true) Console.WriteLine(result.Value);
                   var x=  result.IsSuccess ? [|result.Value|]: 0;
                }
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }
}