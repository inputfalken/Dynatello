using System.Diagnostics;
using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Dynatello.Pipelines;

namespace Repository;

public class RequestLogAnalyzer : IRequestPipeLine
{
    private static void WriteLine(string? input)
    {
        if (input is null)
            return;

        Console.WriteLine(input);
    }

    private static string? StringifyExpression(
        string? expression,
        [CallerArgumentExpression("expression")] string? message = null
    )
    {
        if (expression is null)
        {
            return null;
        }
        return $"{message} = {expression}";
    }

    private IEnumerable<string> StringifyAttributes(
        IReadOnlyDictionary<string, string>? items,
        [CallerArgumentExpression("items")] string? expression = null
    )
    {
        if (items is null)
            yield break;

        foreach (var item in items)
            yield return $"{expression}[{item.Key}] = {item.Value}";
    }

    private IEnumerable<string> StringifyAttributes(
        IReadOnlyDictionary<string, AttributeValue>? items,
        [CallerArgumentExpression("items")] string? expression = null
    )
    {
        if (items is null)
            yield break;

        foreach (var item in items)
            yield return $"{expression}[{item.Key}] = {FromAttributeValue(item.Value)}";

        static string FromAttributeValue(AttributeValue av)
        {
            return av switch
            {
                { } a when string.IsNullOrWhiteSpace(a.S) is false => a.S,
                { } a when string.IsNullOrWhiteSpace(a.N) is false => a.N,
                _ => av.ToString() ?? "N/A",
            };
        }
    }

    private void OnQueryRequest(QueryRequest queryRequest)
    {
        foreach (var x in StringifyAttributes(queryRequest.ExpressionAttributeNames))
            WriteLine(x);

        foreach (var x in StringifyAttributes(queryRequest.ExpressionAttributeValues))
            WriteLine(x);

        WriteLine(StringifyExpression(queryRequest.KeyConditionExpression));
        WriteLine(StringifyExpression(queryRequest.FilterExpression));
    }

    private void OnUpdateItemRequest(UpdateItemRequest updateItemRequest)
    {
        foreach (var x in StringifyAttributes(updateItemRequest.ExpressionAttributeNames))
            WriteLine(x);

        foreach (var x in StringifyAttributes(updateItemRequest.ExpressionAttributeValues))
            WriteLine(x);

        WriteLine(StringifyExpression(updateItemRequest.ConditionExpression));
        WriteLine(StringifyExpression(updateItemRequest.UpdateExpression));
    }

    private void OnPutItemRequest(PutItemRequest putItemRequest)
    {
        foreach (var x in StringifyAttributes(putItemRequest.ExpressionAttributeNames))
            WriteLine(x);

        foreach (var x in StringifyAttributes(putItemRequest.ExpressionAttributeValues))
            WriteLine(x);

        foreach (var x in StringifyAttributes(putItemRequest.Item))
            WriteLine(x);

        WriteLine(StringifyExpression(putItemRequest.ConditionExpression));
    }

    private void OnGetItemRequest(GetItemRequest getItemRequest)
    {
        foreach (var x in StringifyAttributes(getItemRequest.Key))
            WriteLine(x);
    }

    public async Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    )
    {
        WriteLine($"===== {requestContext.Request.GetType().Name} started =====");

        (
            (Action)(
                requestContext.Request switch
                {
                    QueryRequest x => () => OnQueryRequest(x),
                    UpdateItemRequest x => () => OnUpdateItemRequest(x),
                    PutItemRequest x => () => OnPutItemRequest(x),
                    GetItemRequest x => () => OnGetItemRequest(x),
                    DeleteItemRequest x => () => OnDeleteItemRequest(x),
                    _ => () => { },
                }
            )
        )();

        var stopwatch = Stopwatch.StartNew();
        var response = await continuation(requestContext);
        WriteLine(
            $"===== {requestContext.Request.GetType().Name} finished after '{stopwatch.Elapsed}' ====="
        );

        return response;
    }

    private void OnDeleteItemRequest(DeleteItemRequest deleteItemRequest)
    {
        foreach (var x in StringifyAttributes(deleteItemRequest.Key))
            WriteLine(x);

        foreach (var x in StringifyAttributes(deleteItemRequest.ExpressionAttributeValues))
            WriteLine(x);

        foreach (var x in StringifyAttributes(deleteItemRequest.ExpressionAttributeNames))
            WriteLine(x);

        StringifyExpression(deleteItemRequest.ConditionExpression);
    }
}