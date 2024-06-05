using Amazon.DynamoDBv2;

namespace Dynatello.Handlers;

internal static class ReturnValueExtensions
{
    public static bool IsValueProvided(this ReturnValue value)
    {
        if (value == ReturnValue.NONE)
            return false;

        if (value == ReturnValue.ALL_NEW)
            return true;

        if (value == ReturnValue.ALL_OLD)
            return true;

        if (value == ReturnValue.UPDATED_OLD)
            return true;

        if (value == ReturnValue.UPDATED_NEW)
            return true;

        throw new ArgumentOutOfRangeException($"Could not determine value '{value.Value}' from type {typeof(ReturnValue)}");
    }
}

