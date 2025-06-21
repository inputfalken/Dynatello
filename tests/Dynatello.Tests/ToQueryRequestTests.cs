using System.Globalization;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using FluentAssertions;

namespace Dynatello.Tests;

public class ToQueryRequestTests
{
    [Fact]
    public void Build_Request()
    {
        var builder = Cat
            .QueryWithCuteness.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .ToQueryRequestBuilder();
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10)
            .Should()
            .AllSatisfy(tuple =>
            {
                builder
                    .Build(tuple)
                    .Should()
                    .BeEquivalentTo(
                        new QueryRequest
                        {
                            TableName = "TABLE",
                            ExpressionAttributeNames = new Dictionary<string, string>
                            {
                                { "#Id", nameof(Cat.Id) },
                            },

                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                {
                                    ":p1",
                                    new AttributeValue { S = tuple.Id.ToString() }
                                },
                            },
                            KeyConditionExpression = "#Id = :p1",
                            KeyConditions = null,
                            ConditionalOperator = null,
                            AttributesToGet = null,
                            ReturnConsumedCapacity = null,
                            FilterExpression = null,
                            Limit = 0,
                            ConsistentRead = null,
                            Select = null,
                            ProjectionExpression = null,
                            IndexName = null,
                            QueryFilter = null,
                            ExclusiveStartKey = null,
                            IsLimitSet = false,
                            ScanIndexForward = null
                        }
                    );
            });
    }

    [Fact]
    public void Build_Request_FilterExpression()
    {
        var builder = Cat
            .QueryWithCuteness.OnTable("TABLE")
            .ToRequestBuilderFactory()
            .WithKeyConditionExpression((x, y) => $"{x.Id} = {y.Id}")
            .WithFilterExpression((x, y) => $"{x.Cuteness} > {y.MinimumCuteness}")
            .ToQueryRequestBuilder();
        Cat.Fixture.CreateMany<(Guid Id, double MinimumCuteness)>(10)
            .Should()
            .AllSatisfy(tuple =>
            {
                builder
                    .Build(tuple)
                    .Should()
                    .BeEquivalentTo(
                        new QueryRequest
                        {
                            TableName = "TABLE",
                            ExpressionAttributeNames = new Dictionary<string, string>
                            {
                                { "#Id", nameof(Cat.Id) },
                                { "#Cuteness", nameof(Cat.Cuteness) },
                            },

                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                {
                                    ":p1",
                                    new AttributeValue { S = tuple.Id.ToString() }
                                },
                                {
                                    ":p2",
                                    new AttributeValue
                                    {
                                        N = tuple.MinimumCuteness.ToString(
                                            CultureInfo.InvariantCulture
                                        ),
                                    }
                                },
                            },
                            KeyConditionExpression = "#Id = :p1",
                            KeyConditions = null,
                            ConditionalOperator = null,
                            AttributesToGet = null,
                            ReturnConsumedCapacity = null,
                            FilterExpression = "#Cuteness > :p2",
                            Limit = 0,
                            ConsistentRead = null,
                            Select = null,
                            ProjectionExpression = null,
                            IndexName = null,
                            QueryFilter = null,
                            ExclusiveStartKey = null,
                            IsLimitSet = false,
                            ScanIndexForward = null
                        }
                    );
            });
    }
}
