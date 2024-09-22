using System.Diagnostics;
using Amazon.Runtime;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;
using Dynatello.Pipelines;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(
        x => x.ToGetRequestBuilder(),
        x =>
        {
            x.RequestsPipelines.Add(new RequestDurationConsoleLogger());
        }
    );

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

public class RequestDurationConsoleLogger : IRequestPipeLine
{
    public async Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await continuation(requestContext);
        Console.WriteLine($"Duration: {stopwatch.Elapsed}");
        
        return result;
    }
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
