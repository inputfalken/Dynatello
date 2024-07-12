using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;
using NSubstitute;

namespace Dynatello.Tests.HandlerTests;

public class UpdateRequestHandlerTests
{
    [Theory]
    [InlineData("ALL_NEW")]
    [InlineData("ALL_OLD")]
    [InlineData("UPDATED_NEW")]
    [InlineData("UPDATED_OLD")]
    public async Task Send_SuccessMock_ShouldReturnAttributes(string returnValue)
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var update = Guid.NewGuid();

        var expected = Cat.Fixture.Create<Cat>();
        amazonDynamoDB
            .UpdateItemAsync(Arg.Any<UpdateItemRequest>())
            .Returns(
                new UpdateItemResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Attributes = Cat.UpdateHome.Marshall(expected)
                }
            );

        var actual = await Cat
            .UpdateHome.OnTable("TABLE")
            .ToUpdateRequestHandler(
                x =>
                    x.WithUpdateExpression((db, arg) => $"{db.HomeId} = {arg}")
                        .ToUpdateItemRequestBuilder((x, y) => x.PartitionKey(y.Id)) with
                    {
                        ReturnValues = new ReturnValue(returnValue)
                    },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send((expected.Id, expected.HomeId), default);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("None")]
    [InlineData(null)]
    public async Task Send_SuccessMock_ShouldNotReturnAttributes(string returnValue)
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var update = Guid.NewGuid();

        amazonDynamoDB
            .UpdateItemAsync(Arg.Any<UpdateItemRequest>())
            .Returns(new UpdateItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK, });

        var actual = await Cat
            .UpdateHome.OnTable("TABLE")
            .ToUpdateRequestHandler(
                x =>
                    x.WithUpdateExpression((db, arg) => $"{db.HomeId} = {arg}")
                        .ToUpdateItemRequestBuilder((x, y) => x.PartitionKey(y.Id)) with
                    {
                        ReturnValues = returnValue is not null ? new ReturnValue(returnValue) : null
                    },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send((Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.Equal(null, actual);
    }
}
