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
    private class TestPipeLine : IRequestPipeLine
    {
        public DateTime? TimeStamp;

        public async Task<AmazonWebServiceResponse> Invoke(
            RequestContext requestContext,
            Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
        )
        {
            await Task.Delay(50);
            TimeStamp = DateTime.Now;
            return await continuation(requestContext);
        }
    }

    [Fact]
    public async Task Send_SuccessMockWithMockedRequestPipeline_ShouldReturnItemAndExecutePipeLine()
    {
        var expected = Cat.Fixture.Create<Cat>();
        var response = new GetItemResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK,
            IsItemSet = true,
            Item = Cat.GetById.Marshall(expected),
        };
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();

        amazonDynamoDB
            .GetItemAsync(Arg.Any<GetItemRequest>())
            .Returns(async x =>
            {
                await Task.Delay(200);
                return response;
            });

        var pipelines = new TestPipeLine[]
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        Assert.All(pipelines, x => Assert.Null(x.TimeStamp));
        var actual = await Cat
            .GetById.OnTable("TABLE")
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(),
                x =>
                {
                    x.RequestsPipelines.Add(pipelines[0]);
                    x.RequestsPipelines.Add(pipelines[1]);
                    x.RequestsPipelines.Add(pipelines[2]);
                    x.RequestsPipelines.Add(pipelines[3]);
                    x.RequestsPipelines.Add(pipelines[4]);
                    x.AmazonDynamoDB = amazonDynamoDB;
                }
            )
            .Send(expected.Id, CancellationToken.None);
        Assert.Equal(expected, actual);
        Assert.All(pipelines, x => Assert.NotNull(x.TimeStamp));
        var pipeLineTimestamps = pipelines.Select(x => x.TimeStamp!.Value).ToArray();

        Assert.DoesNotContain(
            false,
            pipeLineTimestamps.Zip(
                pipeLineTimestamps.Skip(1),
                (x, y) => x < y &&
                          y - x < TimeSpan.FromMilliseconds(100) // ugly hack to verify that request comes last.
            )
        );
    }

    [Fact]
    public async Task Send_SuccessMock_ShouldReturnItem()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
            .GetItemAsync(Arg.Any<GetItemRequest>())
            .Returns(
                new GetItemResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    IsItemSet = true,
                    Item = Cat.GetById.Marshall(expected),
                }
            );

        var actual = await Cat
            .GetById.OnTable("TABLE")
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send(expected.Id, CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_MissingValue_ShouldReturnNull()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        var expected = Cat.Fixture.Create<Cat>();

        amazonDynamoDB
            .GetItemAsync(Arg.Any<GetItemRequest>())
            .Returns(new GetItemResponse { IsItemSet = false, Item = null });

        var actual = await Cat
            .GetById.OnTable("TABLE")
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(),
                x => x.AmazonDynamoDB = amazonDynamoDB
            )
            .Send(expected.Id, CancellationToken.None);

        Assert.Equal(null!, actual);
    }
}