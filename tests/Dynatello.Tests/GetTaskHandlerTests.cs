
using Amazon.DynamoDBv2;
using Dynatello.Builders;
namespace Dynatello.Tests;
public class GetTaskHandlerTests
{

    [Fact]
    public void Test()
    {
        // TODO MOCK
        var f = Cat.GetById
          .OnTable("TABLE")
          .WithGetRequestFactory(x => x.ToGetRequestBuilder(), new AmazonDynamoDBClient())
    }
}
