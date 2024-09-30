# Dynatello

## What does the library do?

Offers a unified API based on the source generated low-level API provided from [DynamoDB.SourceGenerator](https://github.com/inputfalken/DynamoDB.SourceGenerator). 

## Features

* Builder patterns to create request builders.
* Request handlers that perform the DynamoDB request and handles the response.
  *  Middlware Support through `IRequestPipeline`. 

## Installation

Add the following NuGet package as a dependency to your project.

[![DynamoDBGenerator][1]][2]

[1]: https://img.shields.io/nuget/v/Dynatello.svg?label=Dynatello
[2]: https://www.nuget.org/packages/Dynatello

## Example
The code is also available in the [samples](samples/) directory.

### Extend DTO's
Extend a DTO to contain the mashaller functionality.

```csharp
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(x => x.ToGetRequestBuilder());

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
```


### Request middleware

Every `RequestHandler<Targ, TResponse>` supports having multiple middlewares.

```csharp
using System.Diagnostics;
using Amazon.Runtime;
using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;
using Dynatello.Pipelines;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(
        x => x.ToGetRequestBuilder(),
        x =>
        {
            x.RequestsPipelines.Add(new RequestDurationConsoleLogger());
        }
    );

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

public class RequestDurationConsoleLogger : IRequestPipeLine
{
    public async Task<AmazonWebServiceResponse> Invoke(
        RequestContext requestContext,
        Func<RequestContext, Task<AmazonWebServiceResponse>> continuation
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await continuation(requestContext);
        Console.WriteLine($"Duration: {stopwatch.Elapsed}");
        
        return result;
    }
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
```


### Repository
Isolate DynamoDB code to a repository class.

```csharp
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
```
