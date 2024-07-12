using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Handlers;
using NSubstitute;

namespace Dynatello.Tests.HandlerTests;
public class DeleteRequestHandlerTests
{


    [Fact]
    public async Task Send_SuccessMock_ShouldReturnItem()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
          .DeleteItemAsync(Arg.Any<DeleteItemRequest>())
          .Returns(new DeleteItemResponse
          {
              HttpStatusCode = System.Net.HttpStatusCode.OK,
              Attributes = Cat.GetById.Marshall(expected)
          });

        var actual = await Cat.GetById
          .OnTable("TABLE")
          .ToDeleteRequestHandler(x => x.ToDeleteRequestBuilder() with { ReturnValues = ReturnValue.ALL_OLD }, x => x.AmazonDynamoDB = amazonDynamoDB)
          .Send(expected.Id, default);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_MissingValue_ShouldReturnNull()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
          .DeleteItemAsync(Arg.Any<DeleteItemRequest>())
          .Returns(new DeleteItemResponse { });

        var actual = await Cat.GetById
          .OnTable("TABLE")
          .ToDeleteRequestHandler(x => x.ToDeleteRequestBuilder(), x => x.AmazonDynamoDB = amazonDynamoDB)
          .Send(expected.Id, default);

        Assert.Null(actual);
    }
}
