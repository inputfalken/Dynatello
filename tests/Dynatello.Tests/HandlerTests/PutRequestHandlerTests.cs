using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Handlers;
using NSubstitute;

namespace Dynatello.Tests.HandlerTests;

public class PutRequestHandlerTests
{
    [Theory]
    [InlineData("ALL_NEW")]
    [InlineData("ALL_OLD")]
    [InlineData("UPDATED_NEW")]
    [InlineData("UPDATED_OLD")]
    public async Task Send_SuccessMock_ShouldReturnAttributes(string returnValue)
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
            .PutItemAsync(Arg.Any<PutItemRequest>())
            .Returns(
                new PutItemResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Attributes = Cat.Put.Marshall(expected)
                }
            );

        var actual = await Cat
            .Put.OnTable("TABLE")
            .ToPutRequestHandler(
                x => x.ToPutRequestBuilder() with { ReturnValues = new ReturnValue(returnValue) },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send(expected, default);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("None")]
    [InlineData(null)]
    public async Task Send_SuccessMock_ShouldNotReturnAttributes(string returnValue)
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
            .PutItemAsync(Arg.Any<PutItemRequest>())
            .Returns(new PutItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK, });

        var actual = await Cat
            .Put.OnTable("TABLE")
            .ToPutRequestHandler(
                x =>
                    x.ToPutRequestBuilder() with
                    {
                        ReturnValues = returnValue is not null ? new ReturnValue(returnValue) : null
                    },
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send(expected, default);

        Assert.Equal(actual, null);
    }
}
