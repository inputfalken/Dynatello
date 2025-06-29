using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Exceptions;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToDeleteItemRequestTests
{
    [Fact]
    public void Build_CondtionalRequest_CompositeKeys_InvalidPartition()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidPartition.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .WithConditionExpression((x, y) => $"{x.Id} = {y.Id} AND {x.HomeId} = {y.HomeId}")
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build(("", Guid.Empty));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_CondtionalRequest_CompositeKeys_InvalidRange()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidRange.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .WithConditionExpression((x, y) => $"{x.Id} = {y.Id} AND {x.HomeId} = {y.HomeId}")
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build((Guid.Empty, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_CondtionalRequest_CompositeKeys_InvalidPartitionAndRange()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidPartitionAndRange.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .WithConditionExpression((x, y) => $"{x.Id} = {y.Id} AND {x.HomeId} = {y.HomeId}")
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build((2.3, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_CondtionalRequest_WithInvalidPartitionKey()
    {
        var act = () =>
            Cat
                .GetByInvalidPartition.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .WithConditionExpression((x, y) => $"{x.Id} = {y}")
                .ToDeleteRequestBuilder(x => x)
                .Build("TEST");

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_CondtionalRequest_PartitionKeyOnly()
    {
        var getCatByPartitionKey = Cat
            .GetById.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .WithConditionExpression((x, y) => $"{x.Id} = {y}")
            .ToDeleteRequestBuilder(x => x);

        Cat.Fixture.CreateMany<Guid>()
            .Should()
            .AllSatisfy(x =>
                getCatByPartitionKey
                    .Build(x)
                    .Should()
                    .BeEquivalentTo(
                        new DeleteItemRequest
                        {
                            Key = new Dictionary<string, AttributeValue>
                            {
                                {
                                    nameof(Cat.Id),
                                    new AttributeValue { S = x.ToString() }
                                },
                            },
                            TableName = "TABLE",
                            ConditionExpression = "#Id = :p1",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                            {
                                {
                                    ":p1",
                                    new AttributeValue() { S = x.ToString() }
                                },
                            },
                            ExpressionAttributeNames = new Dictionary<string, string>()
                            {
                                { "#Id", "Id" },
                            },
                        }
                    )
            );
    }

    [Fact]
    public void Build_CondtionalRequest_CompositeKeys()
    {
        var getCatByCompositeKeys = Cat
            .GetByCompositeKey.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .WithConditionExpression((x, y) => $"{x.Id} = {y.Id} AND {x.HomeId} = {y.HomeId}")
            .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId);

        Cat.Fixture.CreateMany<(Guid PartitionKey, Guid RangeKey)>()
            .Should()
            .AllSatisfy(x =>
                getCatByCompositeKeys
                    .Build(x)
                    .Should()
                    .BeEquivalentTo(
                        new DeleteItemRequest
                        {
                            Key = new Dictionary<string, AttributeValue>
                            {
                                {
                                    nameof(Cat.Id),
                                    new AttributeValue { S = x.PartitionKey.ToString() }
                                },
                                {
                                    nameof(Cat.HomeId),
                                    new AttributeValue { S = x.RangeKey.ToString() }
                                },
                            },
                            TableName = "TABLE",
                            ReturnConsumedCapacity = null,
                            ConditionExpression = "#Id = :p1 AND #HomeId = :p2",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                            {
                                {
                                    ":p1",
                                    new AttributeValue() { S = x.PartitionKey.ToString() }
                                },
                                {
                                    ":p2",
                                    new AttributeValue() { S = x.RangeKey.ToString() }
                                },
                            },
                            ExpressionAttributeNames = new Dictionary<string, string>()
                            {
                                { "#Id", "Id" },
                                { "#HomeId", "HomeId" },
                            },
                        }
                    )
            );
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartition()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidPartition.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build(("", Guid.Empty));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidRange()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidRange.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build((Guid.Empty, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_CompositeKeys_InvalidPartitionAndRange()
    {
        var act = () =>
            Cat
                .GetByCompositeInvalidPartitionAndRange.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId)
                .Build((2.3, ""));

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_WithInvalidPartitionKey()
    {
        var act = () =>
            Cat
                .GetByInvalidPartition.OnTable("TABLE")
                .ToRequestBuilderFactory()
                .ToDeleteRequestBuilder(x => x)
                .Build("TEST");

        act.Should()
            .Throw<DynamoDBMarshallingException>()
            .WithMessage("Value '*' from argument '*' is not convertable*");
    }

    [Fact]
    public void Build_Request_PartitionKeyOnly()
    {
        var getCatByPartitionKey = Cat
            .GetById.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .ToDeleteRequestBuilder(x => x);

        Cat.Fixture.CreateMany<Guid>()
            .Should()
            .AllSatisfy(x =>
                getCatByPartitionKey
                    .Build(x)
                    .Should()
                    .BeEquivalentTo(
                        new DeleteItemRequest
                        {
                            Key = new Dictionary<string, AttributeValue>
                            {
                                {
                                    nameof(Cat.Id),
                                    new AttributeValue { S = x.ToString() }
                                },
                            },
                            TableName = "TABLE",
                            ExpressionAttributeNames = null,
                            ReturnConsumedCapacity = null,
                        }
                    )
            );
    }

    [Fact]
    public void Build_Request_CompositeKeys()
    {
        var getCatByCompositeKeys = Cat
            .GetByCompositeKey.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .ToDeleteRequestBuilder(x => x.Id, x => x.HomeId);

        Cat.Fixture.CreateMany<(Guid PartitionKey, Guid RangeKey)>()
            .Should()
            .AllSatisfy(x =>
                getCatByCompositeKeys
                    .Build(x)
                    .Should()
                    .BeEquivalentTo(
                        new DeleteItemRequest
                        {
                            Key = new Dictionary<string, AttributeValue>
                            {
                                {
                                    nameof(Cat.Id),
                                    new AttributeValue { S = x.PartitionKey.ToString() }
                                },
                                {
                                    nameof(Cat.HomeId),
                                    new AttributeValue { S = x.RangeKey.ToString() }
                                },
                            },
                            TableName = "TABLE",
                            ExpressionAttributeNames = null,
                            ReturnConsumedCapacity = null,
                        }
                    )
            );
    }
}
