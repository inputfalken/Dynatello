using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dynatello.Handlers;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using NSubstitute;
using AutoFixture;
using System.Linq.Expressions;

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
          .Returns(new QueryResponse
          {
              HttpStatusCode = System.Net.HttpStatusCode.OK,
              Items = expected.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList()
          });

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Send_SuccessChunkedMock_ShouldReturnAttributes()
    {
        var amazonDynamoDB = Substitute.For<IAmazonDynamoDB>();
        const int ElementCount = 20;
        const int ChunkCount = 5;
        var chunks = Cat.Fixture
          .CreateMany<Cat>(ElementCount)
          .Chunk(ChunkCount)
          .Select((elements, index) =>
          {
              var lastId = elements[^1].Id.ToString();
              var key = index == (ElementCount / ChunkCount) - 1
                ? new()
                : new Dictionary<string, AttributeValue>() { { nameof(Cat.Id), new AttributeValue() { S = lastId } } };
              return new { LastId = lastId, Key = key, Elements = elements };
          })
          .ToArray();

        amazonDynamoDB
          .QueryAsync(Arg.Is<QueryRequest>(
            x => x.ExclusiveStartKey.Count == 0 || x.ExclusiveStartKey[nameof(Cat.Id)] != null
            ), Arg.Any<CancellationToken>())
          .Returns(
              new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = chunks.First().Elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = chunks.First().Key
              },
              chunks.Skip(1).Select(x => new QueryResponse
              {
                  HttpStatusCode = System.Net.HttpStatusCode.OK,
                  Items = x.Elements.Select(x => Cat.QueryWithCuteness.Marshall(x)).ToList(),
                  LastEvaluatedKey = x.Key
              }).ToArray()
          );

        var actual = await Cat.QueryWithCuteness
          .OnTable("TABLE")
          .ToQueryRequestHandler(x => x
              .WithKeyConditionExpression(((x, y) => $"{x.Id} = {y.Id}"))
              .ToQueryRequestBuilder() with
          { IndexName = "INDEX" }, amazonDynamoDB)
          .Send((Guid.NewGuid(), 2), default);

        Assert.Equal(chunks.SelectMany(x => x.Elements).ToArray(), actual);
    }
}
public static class PredicateBuilder
{

    internal class SubstExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        public Dictionary<Expression, Expression> subst = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (subst.TryGetValue(node, out var newValue))
            {
                return newValue;
            }
            return node;
        }
    }
    public static Expression<Predicate<T>> And<T>(this Expression<Predicate<T>> a, Expression<Func<T, bool>> b)
    {

        ParameterExpression p = a.Parameters[0];

        SubstExpressionVisitor visitor = new SubstExpressionVisitor();
        visitor.subst[b.Parameters[0]] = p;

        Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body));
        return Expression.Lambda<Predicate<T>>(body, p);
    }

    public static Expression<Predicate<T>> Or<T>(this Expression<Predicate<T>> a, Expression<Func<T, bool>> b)
    {

        ParameterExpression p = a.Parameters[0];

        SubstExpressionVisitor visitor = new SubstExpressionVisitor();
        visitor.subst[b.Parameters[0]] = p;

        Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body));
        return Expression.Lambda<Predicate<T>>(body, p);
    }
}
