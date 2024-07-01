using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Handlers;
using Dynatello.Pipelines;
using NSubstitute;

namespace Dynatello.Tests.HandlerTests;
public class GetRequestHandlerTests
{
    [Fact]
    public async Task Send_SuccessMock_ShouldReturnItem2()
    {
        var expected = Cat.Fixture.Create<Cat>();
        var response = new GetItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            IsItemSet = true,
            Item = Cat.GetById.Marshall(expected)
        };
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();

        var pipelines = Substitute.For<IRequestPipeLine>();
        pipelines
          .Invoke(Arg.Any<RequestContext>(), x => Task.FromResult<AmazonWebServiceResponse>(response))
          .Returns(response);

        amazonDynamoDB
          .GetItemAsync(Arg.Any<GetItemRequest>())
          .Returns(response);

        var actual = await Cat.GetById
          .OnTable("TABLE")
          .ToGetRequestHandler(x => x.ToGetRequestBuilder(), amazonDynamoDB, new[] { pipelines })
          .Send(expected.Id, default);

        Assert.Equal(expected, actual);
    }

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
          .ToGetRequestHandler(x => x.ToGetRequestBuilder(), amazonDynamoDB)
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
          .ToGetRequestHandler(x => x.ToGetRequestBuilder(), amazonDynamoDB)
          .Send(expected.Id, default);

        Assert.Equal((Cat)null!, actual);
    }
}
