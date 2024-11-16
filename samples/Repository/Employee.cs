using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

namespace Repository;

[DynamoDBMarshaller(AccessName = "GetByEmail", ArgumentType = typeof(string))]
[DynamoDBMarshaller(
    AccessName = "GetEmployee",
    ArgumentType = typeof((string department, string email))
)]
[DynamoDBMarshaller(
    AccessName = "Query",
    ArgumentType = typeof((string Department, string EmailPrefix, DateTime MustBeLessThan))
)]
[DynamoDBMarshaller(AccessName = "Create")]
[DynamoDBMarshaller(
    AccessName = "UpdateLastname",
    ArgumentType = typeof((string Department, string Email, string NewLastname))
)]
public partial record Employee(
    [property: DynamoDBHashKey] string Department,
    [property: DynamoDBRangeKey, DynamoDBGlobalSecondaryIndexHashKey("EmailLookup")] string Email,
    string LastName,
    string[] Skills,
    Metadata Metadata
);

public record Metadata(DateTime Timestamp);