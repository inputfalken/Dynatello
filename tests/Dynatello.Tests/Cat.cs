using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using DynamoDBGenerator.Attributes;

namespace Dynatello.Tests;

[DynamoDBMarshaller(AccessName = "QueryWithCuteness", ArgumentType = typeof((Guid Id, double MinimumCuteness)))]
[DynamoDBMarshaller(AccessName = "GetByCompositeKey", ArgumentType = typeof((Guid Id, Guid HomeId)))]
[DynamoDBMarshaller(AccessName = "GetById", ArgumentType = typeof(Guid))]
[DynamoDBMarshaller(AccessName = "Put")]
[DynamoDBMarshaller(AccessName = "UpdateHome", ArgumentType = typeof(Guid))]
[DynamoDBMarshaller(AccessName = "GetByInvalidPartition", ArgumentType = typeof(string))]
[DynamoDBMarshaller(AccessName = "GetByCompositeInvalidPartition", ArgumentType = typeof((string Id, Guid HomeId)))]
[DynamoDBMarshaller(AccessName = "GetByCompositeInvalidRange", ArgumentType = typeof((Guid Id, string HomeId)))]
[DynamoDBMarshaller(AccessName = "GetByCompositeInvalidPartitionAndRange", ArgumentType = typeof((double Id, string HomeId)))]
public partial record Cat(
    [property: DynamoDBHashKey] Guid Id,
    [property: DynamoDBRangeKey] Guid HomeId,
    string Name,
    double Cuteness)
{
    public static readonly Fixture Fixture = new();
}

