using Amazon.DynamoDBv2;

namespace Dynatello.Handlers;

internal static class ReturnValueExtensions
{
    public static bool IsValueProvided(this ReturnValue? value) =>
        value switch
        {
            null => false,
            _ when value == ReturnValue.NONE => false,
            _ when value == ReturnValue.ALL_NEW => true,
            _ when value == ReturnValue.ALL_OLD => true,
            _ when value == ReturnValue.UPDATED_OLD => true,
            _ when value == ReturnValue.UPDATED_NEW => true,
            _ => throw new ArgumentOutOfRangeException(
                $"Could not determine value '{value.Value}' from type {typeof(ReturnValue)}"
            )
        };
}