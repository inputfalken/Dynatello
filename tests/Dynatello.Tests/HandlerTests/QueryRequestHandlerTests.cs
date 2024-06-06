using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Handlers;
using Dynatello.Builders;
using NSubstitute;
using AutoFixture;

namespace Dynatello.Tests.HandlerTests;
public class QueryRequestHandlerTests
{
    [Fact]
    public async Task Send_SuccessMock_ShouldReturnAttributes()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();

        var expected = Cat.Fixture.CreateMany<Cat>().ToArray();
        amazonDynamoDB
          .QueryAsync(Arg.Any<QueryRequest>())
          .Returns(new QueryResponse
          {
              HttpStatusCode = System.Net.HttpStatusCode.OK,
              Items = expected.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList()
          });

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(expected, actual);
    }
}
