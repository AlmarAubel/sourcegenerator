using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using  Roslynator.Testing.CSharp;
using Xunit;

namespace IfBrackets.Tests;  

public class UseResultValueWithoutCheckTests2:AbstractCSharpDiagnosticVerifier
    <UseResultValueWithoutCheck,DummyCodeFixProvider>
{
    public override DiagnosticDescriptor Descriptor => UseResultValueWithoutCheck.Rule;
    
    [Fact]
    public async Task AccesValueOnResultObject_WithoutcheckingIsSuccess_ShouldWarn2()
    {
        var cSharpFunctionalExtensions= 
            MetadataReference
                .CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        
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
    public async Task AccesValueOnResultObject_WithCheckIsSuccess_ShouldPass()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyNoDiagnosticAsync(
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
                   if(result.IsFailure != true) Console.WriteLine(result.Value);
                   var x=  result.IsSuccess ? result.Value: 0;
                }
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }  
    
    [Fact]
    public async Task AccesValueOnResultObject_WithoutcheckingIsFailure_ShouldWarn()
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
                   if(result.IsFailure) Console.WriteLine([|result.Value|]);
                   
                   var x=  result.IsFailure ? [|result.Value|]: 0;
                }
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }
    
    [Fact]
    public async Task AccesValueOnResultObject_WithcheckingIsFailure_ShouldPass()
    {
        var cSharpFunctionalExtensions= MetadataReference.CreateFromFile(typeof (CSharpFunctionalExtensions.Result).Assembly.Location);
        await VerifyNoDiagnosticAsync(
            """
            using System;
            using CSharpFunctionalExtensions;

            public class FunctionsWithResultObject
            {
                public void GetId(int a)
                {
                   var result = Result.Success(1);
                   if(!result.IsFailure) Console.WriteLine(result.Value);
                 
                   var x =  !result.IsFailure ? result.Value: 0;
                   
                   if (result.IsFailure) return;
                   var y = result.Value;
                   
                   
                }
            }
            """, options: Options.WithMetadataReferences( Options.MetadataReferences.Add(cSharpFunctionalExtensions)));
    }
}