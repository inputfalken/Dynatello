using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Handlers;
using Dynatello.Builders;
using Dynatello.Builders.Types;
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
          { IndexName = "INDEX" }, x => x.AmazonDynamoDB = amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_SuccessChunkedMock_ShouldReturnAttributes()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        const int ElementCount = 20;
        const int ChunkCount = 5;
        var chunks = Cat.Fixture
          .CreateMany<Cat>(ElementCount)
          .Chunk(ChunkCount)
          .Select((elements, index) =>
          {
              var lastId = elements[^1].Id.ToString();
              var key = index == (ElementCount / ChunkCount) - 1
                ? new()
                : new Dictionary<string, AttributeValue>() { { nameof(Cat.Id), new AttributeValue() { S = lastId } } };
              return new { LastId = lastId, Key = key, Elements = elements };
          })
          .ToArray();

        amazonDynamoDB
          .QueryAsync(Arg.Is<QueryRequest>(
            x => x.ExclusiveStartKey.Count == 0 || x.ExclusiveStartKey[nameof(Cat.Id)] != null
            ), Arg.Any<CancellationToken>())
          .Returns(
              new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = chunks.First().Elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = chunks.First().Key
              },
              chunks.Skip(1).Select(x => new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = x.Elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = x.Key
              }).ToArray()
          );

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, x => x.AmazonDynamoDB = amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(chunks.SelectMany(x => x.Elements).ToArray(), actual);
    }
}
