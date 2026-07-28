namespace InfotecsTestTask.Application.Utils;

public static class Rounder
{
    private const int DefaultDecimalDigits = 3;

    public static double Round(double value)
    {
        return Math.Round(value, DefaultDecimalDigits, MidpointRounding.AwayFromZero);
    }
}