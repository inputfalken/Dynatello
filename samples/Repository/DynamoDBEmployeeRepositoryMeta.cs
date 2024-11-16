using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;

namespace Repository;

public partial class DynamoDBEmployeeRepository
{
    private const string TableName = "Employees";

    public async Task GenerateEmployeesInDeparment(
        string department,
        int count,
        CancellationToken cancellationToken
    )
    {
        var fixture = new Fixture();
        for (var i = 0; i < count; i++)
        {
            var employee = fixture.Create<Employee>() with
            {
                Department = department,
                Metadata = new Metadata(DateTime.UtcNow),
            };

            await _createEmployee.Send(employee, cancellationToken);
        }
    }

    public async Task<bool> EnsureTableIsCreated(CancellationToken cancellationToken)
    {
        try
        {
            await _database.DescribeTableAsync(TableName, cancellationToken);

            return true;
        }
        catch (ResourceNotFoundException)
        {
            await _database.CreateTableAsync(
                new CreateTableRequest()
                {
                    TableName = TableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition()
                        {
                            AttributeName = nameof(Employee.Department),
                            AttributeType = "S",
                        },
                        new AttributeDefinition()
                        {
                            AttributeName = nameof(Employee.Email),
                            AttributeType = "S",
                        },
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement()
                        {
                            AttributeName = nameof(Employee.Department),
                            KeyType = KeyType.HASH,
                        },
                        new KeySchemaElement()
                        {
                            AttributeName = nameof(Employee.Email),
                            KeyType = KeyType.RANGE,
                        },
                    },
                    ProvisionedThroughput = new ProvisionedThroughput()
                    {
                        ReadCapacityUnits = 5,
                        WriteCapacityUnits = 5,
                    },
                },
                cancellationToken
            );
            return false;
        }
    }
}