using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Handlers;
using Dynatello.Builders.Types;
using Dynatello.Builders;
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

        var expected = new UpdateItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
        };
        amazonDynamoDB
          .UpdateItemAsync(Arg.Any<UpdateItemRequest>())
          .Returns(expected);

        var actual = await Cat.UpdateHome
          .OnTable("TABLE")
          .ToUpdateRequestHandler(x => x
              .WithUpdateExpression((db, arg) => $"{db.HomeId} = {arg}")
              .ToUpdateItemRequestBuilder((x, y) => x.PartitionKey(y)) with
          { ReturnValues = new ReturnValue(returnValue) }, amazonDynamoDB
          )
          .Send(update, default);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("None")]
    [InlineData(null)]
    public async Task Send_SuccessMock_ShouldNotReturnAttributes(string returnValue)
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var update = Guid.NewGuid();

        var expected = new UpdateItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
        };
        amazonDynamoDB
          .UpdateItemAsync(Arg.Any<UpdateItemRequest>())
          .Returns(expected);

        var actual = await Cat.UpdateHome
          .OnTable("TABLE")
          .ToUpdateRequestHandler(x => x
              .WithUpdateExpression((db, arg) => $"{db.HomeId} = {arg}")
              .ToUpdateItemRequestBuilder((x, y) => x.PartitionKey(y)) with
          { ReturnValues = new ReturnValue(returnValue) }, amazonDynamoDB
          )
          .Send(update, default);

        Assert.Equal(expected, actual);
    }
}
