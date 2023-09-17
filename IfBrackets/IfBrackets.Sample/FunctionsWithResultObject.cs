using System;
using CSharpFunctionalExtensions;

namespace IfBrackets.Sample;

public class FunctionsWithResultObject
{
    public Result<int, string> GetId()
    {
        var idFromDbResult = GetIdFromDb();
        var a = new Random().Next();
        //This is dangerous because we didn't check if the result was succesfull
        //if (idFromDbResult.IsSuccess) return "gaat niet goed";
        //if (idFromDbResult.IsSuccess)  return "GetIdFromDb()";
        if (!idFromDbResult.IsSuccess)  return "GetIdFromDb()" + idFromDbResult.Value;
        
        Console.WriteLine(idFromDbResult.Value);
        return "";
    }   
    
    public Result<int, string> GetId(int axxx)
    {
        var idFromDbResult = GetIdFromDb();
        var a = new Random().Next();
        var x = Result.Success(1);
        if (true) Console.WriteLine(x.Value);
        if(1 > a && idFromDbResult.IsSuccess )
            Console.WriteLine(idFromDbResult.Value);
        if(idFromDbResult.IsFailure) return 0;
        Console.WriteLine(x.Value);
       //if (idFromDbResult.IsSuccess) return idFromDbResult.IsSuccess ? idFromDbResult.Value : 0;
       //if (!idFromDbResult.IsSuccess) Console.WriteLine(idFromDbResult.Value);
        

        return idFromDbResult.IsSuccess ? idFromDbResult.Value : 0;
    }
    
    public void GetIdB(int a)
    {
        var result = Result.Success(1);
        if(result.IsSuccess) Console.WriteLine(result.Value);
        if(result.IsSuccess ) Console.WriteLine(result.Value);
        var x=  result.IsSuccess ? result.Value: 0;
    }
    
    public void GetIdFailure(int a)
    {
        var result = Result.Success(1);
        if(result.IsFailure) Console.WriteLine(result.Value);
        if(!result.IsFailure) Console.WriteLine(result.Value);
        var x=  result.IsFailure ? result.Value: 0;

        if (result.IsFailure) return;

        var y = result.Value;
    }

    private Result<int, string> GetIdFromDb()=>"This is an error";
}