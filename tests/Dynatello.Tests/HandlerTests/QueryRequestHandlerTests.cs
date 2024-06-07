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

        await Task.Delay(10000);
        foreach (var (elements, index) in chunks)
        {
            var lastId = elements[^1].Id.ToString();
            var key = new Dictionary<string, AttributeValue>() { { nameof(Cat.Id), new AttributeValue() { S = lastId } } };
            amazonDynamoDB
              .QueryAsync(index is 0
                  ? Arg.Is<QueryRequest>(x => x.ExclusiveStartKey.Count == 0)
                  : Arg.Is<QueryRequest>(x => x.ExclusiveStartKey[nameof(Cat.Id)].S == lastId),
                Arg.Any<CancellationToken>()
              )
              .Returns(new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = index == chunks.Length - 1 ? new() : key
              });

        }

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        var expected = chunks.SelectMany(x => x.x).ToArray();
        Assert.Equal(expected.Length, actual.Count);
        Assert.Equal(expected, actual);
    }
}
