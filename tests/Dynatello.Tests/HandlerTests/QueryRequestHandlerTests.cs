using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;
using NSubstitute;

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
            .Returns(
                new QueryResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Items = expected.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                }
            );

        var actual = await Cat
            .QueryWithCuteness.OnTable("TABLE")
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}")
                            .ToQueryRequestBuilder() with
                        {
                            IndexName = "INDEX",
                        },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send((Guid.NewGuid(), 2), CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_SuccessChunkedMock_ShouldReturnAttributes()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        const int elementCount = 20;
        const int chunkCount = 5;
        var chunks = Cat
            .Fixture.CreateMany<Cat>(elementCount)
            .Chunk(chunkCount)
            .Select((elements, index) =>
                {
                    var lastId = elements[^1].Id.ToString();
                    var key =
                        index == elementCount / chunkCount - 1
                            ? new Dictionary<string, AttributeValue>()
                            : new Dictionary<string, AttributeValue>
                            {
                                {
                                    nameof(Cat.Id),
                                    new AttributeValue { S = lastId }
                                },
                            };
                    return new
                    {
                        LastId = lastId,
                        Key = key,
                        Elements = elements,
                    };
                }
            )
            .ToArray();

        amazonDynamoDB
            .QueryAsync(
                Arg.Is<QueryRequest>(x =>
                    x.ExclusiveStartKey == null 
                    || x.ExclusiveStartKey.Count == 0 
                    || x.ExclusiveStartKey[nameof(Cat.Id)] != null
                ),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new QueryResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Items = chunks
                        .First()
                        .Elements.Select(x => Cat.QueryWithCuteness.Marshall(x))
                        .ToList(),
                    LastEvaluatedKey = chunks.First().Key
                },
                chunks
                    .Skip(1)
                    .Select(x => new QueryResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK,
                        Items = x.Elements.Select(static x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                        LastEvaluatedKey = x.Key
                    })
                    .ToArray()
            );

        var actual = await Cat
            .QueryWithCuteness.OnTable("TABLE")
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression(static (x, y) => $"{x.Id} = {y.Id}")
                            .ToQueryRequestBuilder() with
                        {
                            IndexName = "INDEX"
                        },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send((Guid.NewGuid(), 2), CancellationToken.None);

        Assert.Equal(chunks.SelectMany(x => x.Elements).ToArray(), actual);
    }
}