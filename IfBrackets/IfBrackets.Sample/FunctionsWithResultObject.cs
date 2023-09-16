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
        
        if(1 > a && idFromDbResult.IsSuccess )
            Console.WriteLine(idFromDbResult.Value);

       //if (idFromDbResult.IsSuccess) return idFromDbResult.IsSuccess ? idFromDbResult.Value : 0;
       if (!idFromDbResult.IsSuccess) Console.WriteLine(idFromDbResult.Value);
        

        return idFromDbResult.IsSuccess ? idFromDbResult.Value : 0;
    }

    private Result<int, string> GetIdFromDb()=>"This is an error";
}