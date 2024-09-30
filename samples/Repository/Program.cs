using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;
using Dynatello.Pipelines;

internal class Program
{
    const string TableName = "Employees";
    private static readonly IAmazonDynamoDB DynamoDB = new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig() { ServiceURL = "http://localhost:8000" }
    );

    public static async Task<bool> EnsureTableIsCreated(CancellationToken cancellationToken)
    {
        try
        {
            await DynamoDB.DescribeTableAsync(TableName, cancellationToken);

            return true;
        }
        catch (ResourceNotFoundException)
        {
            await DynamoDB.CreateTableAsync(
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

    public static async Task QuerySalesDepartment(
        EmployeeRepository repository,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("Querying sales department");
        var salesDepartment = await repository.QueryByDepartment(
            "Sales",
            "Email1",
            DateTime.UtcNow,
            cancellationToken
        );

        foreach (var x in salesDepartment)
        {
            Console.WriteLine(x);
        }
    }

    public static async Task CreateDepartments(
        EmployeeRepository repository,
        CancellationToken cancellationToken
    )
    {
        if (await EnsureTableIsCreated(cancellationToken) is false)
        {
            await repository.GenerateEmployeesInDeparment("IT", 100, cancellationToken);
            await repository.GenerateEmployeesInDeparment("Sales", 100, cancellationToken);
        }
    }

    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var repository = new EmployeeRepository(TableName, DynamoDB);

        await CreateDepartments(repository, cts.Token);
        await QuerySalesDepartment(repository, cts.Token);

        Console.WriteLine("Creating employee");
        await repository.CreateEmployee(
            new Employee(
                "IT",
                "someEmail@test.com",
                "Andersson",
                new[] { "Software Development" },
                new Metadata(DateTime.UtcNow)
            ),
            cts.Token
        );

        Console.WriteLine("Getting employee");
        var employee = await repository.GetPersonById("IT", "someEmail@test.com", cts.Token);
        Console.WriteLine(employee);

        Console.WriteLine("Updating employee");
        var updatedEmployee = await repository.UpdateLastName(
            employee!.Department,
            employee.Email,
            "Sparrow",
            cts.Token
        );
        Console.WriteLine(updatedEmployee);

        Console.WriteLine("Deleting employee");
        await repository.DeleteEmployee(
            updatedEmployee!.Department,
            updatedEmployee.Email,
            cts.Token
        );
    }
}

public class RequestLogAnalyzer : IRequestPipeLine
{
    public async Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    )
    {
        var stopwatch = Stopwatch.StartNew();
        if (requestContext.Request is QueryRequest qr)
        {
            Console.WriteLine($"Performing {nameof(QueryRequest)} with the following params:");
            foreach (var x in qr.ExpressionAttributeNames)
            {
                Console.WriteLine($"REF: '{x.Key}', NAME: '{x.Value}'");
            }

            foreach (var x in qr.ExpressionAttributeValues)
            {
                KeyValuePair<string, object> f = string.IsNullOrWhiteSpace(x.Value.S)
                    ? new KeyValuePair<string, object>(x.Key, x.Value)
                    : new KeyValuePair<string, object>(x.Key, x.Value.S);
                Console.WriteLine($"VALUE: {(f)}");
            }

            Console.WriteLine($"KeyConditionExpression: '{qr.KeyConditionExpression}'");
            Console.WriteLine($"FilterExpression: '{qr.FilterExpression}'");
        }

        if (requestContext.Request is UpdateItemRequest uir)
        {
            Console.WriteLine($"Performing {nameof(QueryRequest)} with the following params:");
            foreach (var x in uir.ExpressionAttributeNames)
            {
                Console.WriteLine($"REF: '{x.Key}', NAME: '{x.Value}'");
            }

            foreach (var x in uir.ExpressionAttributeValues)
            {
                KeyValuePair<string, object> f = string.IsNullOrWhiteSpace(x.Value.S)
                    ? new KeyValuePair<string, object>(x.Key, x.Value)
                    : new KeyValuePair<string, object>(x.Key, x.Value.S);
                Console.WriteLine($"VALUE: {(f)}");
            }
            Console.WriteLine($"UpdateExpression: '{uir.UpdateExpression}'");
            Console.WriteLine($"ConditionExpression: '{uir.ConditionExpression}'");
        }

        var response = await continuation(requestContext);

        Console.WriteLine(
            $"{requestContext.Request.GetType().Name} finished after '{stopwatch.Elapsed}'."
        );

        return response;
    }
}

public class EmployeeRepository
{
    private readonly IRequestHandler<(string department, string email), Employee?> _getEmployee;
    private readonly IRequestHandler<(string department, string email), Employee?> _deleteEmployee;
    private readonly IRequestHandler<string, IReadOnlyList<Employee>> _queryByEmail;
    private readonly IRequestHandler<Employee, Employee?> _createEmployee;
    private readonly IRequestHandler<
        (string Department, string EmailPrefix, DateTime MustBeLessThan),
        IReadOnlyList<Employee>
    > _queryByDepartment;
    private readonly IRequestHandler<
        (string Department, string Email, string NewLastname),
        Employee?
    > _updateLastname;

    public EmployeeRepository(string table, IAmazonDynamoDB dynamoDB)
    {
        _deleteEmployee = Employee
            .GetEmployee.OnTable(table)
            .ToDeleteRequestHandler(
                x => x.ToDeleteRequestBuilder(y => y.department, y => y.email),
                x => x.AmazonDynamoDB = dynamoDB
            );

        _getEmployee = Employee
            .GetEmployee.OnTable(table)
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(y => y.department, y => y.email),
                x => x.AmazonDynamoDB = dynamoDB
            );

        _queryByEmail = Employee
            .GetByEmail.OnTable(table)
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression((x, y) => $"{x.Email} = {y} ")
                        .ToQueryRequestBuilder() with
                    {
                        IndexName = "EmailLookup",
                    },
                x => x.AmazonDynamoDB = dynamoDB
            );

        _createEmployee = Employee
            .Create.OnTable(table)
            .ToPutRequestHandler(x => x.ToPutRequestBuilder(), x => x.AmazonDynamoDB = dynamoDB);

        _queryByDepartment = Employee
            .Query.OnTable(table)
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression(
                            (x, y) =>
                                $"{x.Department} = {y.Department} and begins_with({x.Email}, {y.EmailPrefix})"
                        )
                        .WithFilterExpression(
                            (x, y) => $"{x.Metadata.Timestamp} < {y.MustBeLessThan}"
                        )
                        .ToQueryRequestBuilder(),
                x =>
                {
                    x.RequestsPipelines.Add(new RequestLogAnalyzer());
                    x.AmazonDynamoDB = dynamoDB;
                }
            );

        _updateLastname = Employee
            .UpdateLastname.OnTable(table)
            .ToUpdateRequestHandler(
                x =>
                    x.WithUpdateExpression((x, y) => $"SET {x.LastName} = {y.NewLastname}")
                        .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Department, y.Email)) with
                    {
                        ReturnValues = ReturnValue.ALL_NEW,
                    },
                x => x.AmazonDynamoDB = dynamoDB
            );
    }

    private static Fixture Fixture = new();

    public Task<Employee?> GetPersonById(
        string department,
        string email,
        CancellationToken cancellationToken
    ) => _getEmployee.Send((department, email), cancellationToken);

    public Task<IReadOnlyList<Employee>> SearchByEmail(
        string email,
        CancellationToken cancellationToken
    ) => _queryByEmail.Send(email, cancellationToken);

    public Task<Employee?> CreateEmployee(Employee employee, CancellationToken cancellationToken) =>
        _createEmployee.Send(employee, cancellationToken);

    public Task<IReadOnlyList<Employee>> QueryByDepartment(
        string department,
        string emailStartsWith,
        DateTime updatedBefore,
        CancellationToken cancellationToken
    ) => _queryByDepartment.Send((department, emailStartsWith, updatedBefore), cancellationToken);

    public Task<Employee?> UpdateLastName(
        string department,
        string email,
        string lastname,
        CancellationToken cancellationToken
    ) => _updateLastname.Send((department, email, lastname), cancellationToken);

    public Task DeleteEmployee(
        string department,
        string email,
        CancellationToken cancellationToken
    ) => _deleteEmployee.Send((department, email), cancellationToken);

    public async Task GenerateEmployeesInDeparment(
        string department,
        int count,
        CancellationToken cancellationToken
    )
    {
        for (var i = 0; i < count; i++)
        {
            var employee = Fixture.Create<Employee>() with
            {
                Department = department,
                Metadata = new Metadata(DateTime.UtcNow),
            };

            await _createEmployee.Send(employee, cancellationToken);
        }
    }
}

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
