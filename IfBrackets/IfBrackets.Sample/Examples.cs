// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System.Data;

namespace IfBrackets.Sample;

// If you don't see warnings, build the Analyzers Project.

public class Examples
{
    public class MyCompanyClass // Try to apply quick fix using the IDE.
    {
    }

    public void ToStars()
    {
        var spaceship = new Spaceship();
        if (spaceship.ToString() == "x")
        {
            throw new SyntaxErrorException();
        }

        if (2 != 2)
            throw new SyntaxErrorException();
        if (2 != 2)
            throw new SyntaxErrorException();
        if (2 != 2)
            throw new SyntaxErrorException();


        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        spaceship.SetSpeed(42);

        if (1 != 2)
        {
            throw new SyntaxErrorException();
        }
    }
}