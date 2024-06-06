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

    [Fact]
    public async Task Send_SuccessChunkedMock_ShouldReturnAttributes()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var chunks = Cat.Fixture.CreateMany<Cat>(20).Chunk(5).Select((x, y) => (x, y)).ToArray();

        foreach (var (elements, index) in chunks)
        {
            Dictionary<string, AttributeValue> LastEvaluatedKey = index == 0
              ? new()
              : LastEvaluatedKey = new() { { nameof(Cat.Id), new AttributeValue() { S = elements[^1].Id.ToString() } } };
            
            amazonDynamoDB
              .QueryAsync(Arg.Is<QueryRequest>(x => x.ExclusiveStartKey == LastEvaluatedKey))
              .Returns(new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = LastEvaluatedKey
              });
        }

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(chunks.SelectMany(x => x.x), actual);
    }
}
