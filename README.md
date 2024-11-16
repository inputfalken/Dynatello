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
All examples can be found through the links.

### Extend DTO's
[Extend a DTO to contain the mashaller functionality:](samples/ExtendDto)

```csharp
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
[Every `IRequestHandler<Targ, TResponse>` supports middlewares:](samples/RequestPipeline)



```csharp
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
[Through a repository pattern:](samples/Repository)

```csharp
public partial class DynamoDBEmployeeRepository : IEmployeeRepository
{
    private readonly IRequestHandler<Employee, Employee?> _createEmployee;
    private readonly IAmazonDynamoDB _database;
    private readonly IRequestHandler<(string department, string email), Employee?> _deleteEmployee;
    private readonly IRequestHandler<(string department, string email), Employee?> _getEmployee;
    private readonly IRequestHandler<(string Department, string EmailPrefix, DateTime MustBeLessThan), IReadOnlyList<Employee>> _queryByDepartment;
    private readonly IRequestHandler<string, IReadOnlyList<Employee>> _queryByEmail;
    private readonly IRequestHandler<(string Department, string Email, string NewLastname), Employee?> _updateLastname;

    public DynamoDBEmployeeRepository(IAmazonDynamoDB dynamoDB)
    {
        _database = dynamoDB;
        var middleware = new RequestLogAnalyzer();
        _deleteEmployee = Employee
            .GetEmployee.OnTable(TableName)
            .ToDeleteRequestHandler(
                builder => builder.ToDeleteRequestBuilder(y => y.department, y => y.email),
                options =>
                {
                    options.AmazonDynamoDB = dynamoDB;
                    options.RequestsPipelines.Add(middleware);
                }
            );

        _getEmployee = Employee
            .GetEmployee.OnTable(TableName)
            .ToGetRequestHandler(
                builder => builder.ToGetRequestBuilder(x => x.department, x => x.email),
                options =>
                {
                    options.AmazonDynamoDB = dynamoDB;
                    options.RequestsPipelines.Add(middleware);
                }
            );

        _queryByEmail = Employee
            .GetByEmail.OnTable(TableName)
            .ToQueryRequestHandler(
                builder =>
                    builder.WithKeyConditionExpression((x, y) => $"{x.Email} = {y} ").ToQueryRequestBuilder() with
                    {
                        IndexName = "EmailLookup"
                    },
                options =>
                {
                    options.AmazonDynamoDB = dynamoDB;
                    options.RequestsPipelines.Add(middleware);
                }
            );

        _createEmployee = Employee
            .Create.OnTable(TableName)
            .ToPutRequestHandler(
                builder =>
                    builder.WithConditionExpression(
                            (x, y) => $"{x.Department} <> {y.Department} AND {x.Email} <> {y.Email}"
                        )
                        .ToPutRequestBuilder(),
                options =>
                {
                    options.AmazonDynamoDB = dynamoDB;
                    options.RequestsPipelines.Add(middleware);
                }
            );

        _queryByDepartment = Employee
            .Query.OnTable(TableName)
            .ToQueryRequestHandler(
                builder =>
                    builder.WithKeyConditionExpression(
                            (x, y) =>
                                $"{x.Department} = {y.Department} and begins_with({x.Email}, {y.EmailPrefix})"
                        )
                        .WithFilterExpression(
                            (x, y) => $"{x.Metadata.Timestamp} < {y.MustBeLessThan}"
                        )
                        .ToQueryRequestBuilder(),
                options =>
                {
                    options.RequestsPipelines.Add(new RequestLogAnalyzer());
                    options.AmazonDynamoDB = dynamoDB;
                }
            );

        _updateLastname = Employee
            .UpdateLastname.OnTable(TableName)
            .ToUpdateRequestHandler(
                builder =>
                    builder.WithUpdateExpression((x, y) => $"SET {x.LastName} = {y.NewLastname}")
                            .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Department, y.Email)) with
                        {
                            ReturnValues = ReturnValue.ALL_NEW
                        },
                options =>
                {
                    options.AmazonDynamoDB = dynamoDB;
                    options.RequestsPipelines.Add(middleware);
                }
            );
    }

    public Task<Employee?> GetPersonById(string department, string email, CancellationToken cancellationToken) =>
        _getEmployee.Send((department, email), cancellationToken);

    public Task<IReadOnlyList<Employee>> SearchByEmail(string email, CancellationToken cancellationToken) =>
        _queryByEmail.Send(email, cancellationToken);

    public Task<Employee?> CreateEmployee(Employee employee, CancellationToken cancellationToken) =>
        _createEmployee.Send(employee, cancellationToken);

    public Task<IReadOnlyList<Employee>> QueryByDepartment(
        string department,
        string emailStartsWith,
        DateTime updatedBefore,
        CancellationToken cancellationToken) =>
        _queryByDepartment.Send((department, emailStartsWith, updatedBefore), cancellationToken);

    public Task<Employee?> UpdateLastName(
        string department,
        string email,
        string lastname,
        CancellationToken cancellationToken) =>
        _updateLastname.Send((department, email, lastname), cancellationToken);

    public Task DeleteEmployee(
        string department,
        string email,
        CancellationToken cancellationToken) =>
        _deleteEmployee.Send((department, email), cancellationToken);
}
```
