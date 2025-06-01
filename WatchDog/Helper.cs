using System;

namespace WatchDog;

public static class Helper
{
    public static void ValidateString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Input cannot be empty");
        }
    }

    public static void ValidateId(int value)
    {
        if (value < 1)
        {
            throw new ArgumentException("ID must be greater than 0", nameof(value));
        }
    }
}