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
        if (idFromDbResult.IsFailure) return "gaat niet goed";
        //if (!idFromDbResult.IsSuccess) GetIdFromDb();
        if(1 > a && idFromDbResult.IsSuccess)Console.WriteLine(idFromDbResult.Value);
        Console.WriteLine(idFromDbResult.Value);
        return idFromDbResult;
    }

    private Result<int, string> GetIdFromDb()=>"This is an error";
}