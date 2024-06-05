
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Handlers;
using NSubstitute;

namespace Dynatello.Tests;
public class GetTaskHandlerTests
{
    [Fact]
    public async Task Send_SuccessMock_ShouldReturnItem()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
          .GetItemAsync(Arg.Any<GetItemRequest>())
          .Returns(new GetItemResponse
          {
              HttpStatusCode = System.Net.HttpStatusCode.OK,
              IsItemSet = true,
              Item = Cat.GetById.Marshall(expected)
          });

        var actual = await Cat.GetById
          .OnTable("TABLE")
          .WithGetRequestFactory(x => x.ToGetRequestBuilder(), amazonDynamoDB)
          .Send(expected.Id, default);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_MissingValue_ShouldReturnNull()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
          .GetItemAsync(Arg.Any<GetItemRequest>())
          .Returns(new GetItemResponse
          {
              IsItemSet = false,
          });

        var actual = await Cat.GetById
          .OnTable("TABLE")
          .WithGetRequestFactory(x => x.ToGetRequestBuilder(), amazonDynamoDB)
          .Send(expected.Id, default);

        Assert.Equal((Cat)null!, actual);
    }
}
