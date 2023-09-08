using System;
using CSharpFunctionalExtensions;

namespace IfBrackets.Sample;

public class FunctionsWithResultObject
{
    public Result<int, string> GetId()
    {
        var idFromDbResult = GetIdFromDb();
        //This is dangerous because we didn't check if the result was succesfull
        idFromDbResult.IsSuccess
        Console.WriteLine(idFromDbResult.Value);
        return idFromDbResult;
    }

    private Result<int, string> GetIdFromDb()=>"This is an error";
}