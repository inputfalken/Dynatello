using Amazon.DynamoDBv2;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Builders.Types;
using Dynatello.Handlers;

public partial class DynamoDBEmployeeRepository : IEmployeeRepository
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

    private readonly IAmazonDynamoDB _database;

    public DynamoDBEmployeeRepository(IAmazonDynamoDB dynamoDB)
    {
        _database = dynamoDB;
        var middleware = new RequestLogAnalyzer();
        _deleteEmployee = Employee
            .GetEmployee.OnTable(TableName)
            .ToDeleteRequestHandler(
                x => x.ToDeleteRequestBuilder(y => y.department, y => y.email),
                x =>
                {
                    x.AmazonDynamoDB = dynamoDB;
                    x.RequestsPipelines.Add(middleware);
                }
            );

        _getEmployee = Employee
            .GetEmployee.OnTable(TableName)
            .ToGetRequestHandler(
                x => x.ToGetRequestBuilder(y => y.department, y => y.email),
                x =>
                {
                    x.AmazonDynamoDB = dynamoDB;
                    x.RequestsPipelines.Add(middleware);
                }
            );

        _queryByEmail = Employee
            .GetByEmail.OnTable(TableName)
            .ToQueryRequestHandler(
                x =>
                    x.WithKeyConditionExpression((x, y) => $"{x.Email} = {y} ")
                        .ToQueryRequestBuilder() with
                    {
                        IndexName = "EmailLookup",
                    },
                x =>
                {
                    x.AmazonDynamoDB = dynamoDB;
                    x.RequestsPipelines.Add(middleware);
                }
            );

        _createEmployee = Employee
            .Create.OnTable(TableName)
            .ToPutRequestHandler(
                x =>
                    x.WithConditionExpression(
                            (x, y) => $"{x.Department} <> {y.Department} AND {x.Email} <> {y.Email}"
                        )
                        .ToPutRequestBuilder(),
                x =>
                {
                    x.AmazonDynamoDB = dynamoDB;
                    x.RequestsPipelines.Add(middleware);
                }
            );

        _queryByDepartment = Employee
            .Query.OnTable(TableName)
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
            .UpdateLastname.OnTable(TableName)
            .ToUpdateRequestHandler(
                x =>
                    x.WithUpdateExpression((x, y) => $"SET {x.LastName} = {y.NewLastname}")
                        .ToUpdateItemRequestBuilder((x, y) => x.Keys(y.Department, y.Email)) with
                    {
                        ReturnValues = ReturnValue.ALL_NEW,
                    },
                x =>
                {
                    x.AmazonDynamoDB = dynamoDB;
                    x.RequestsPipelines.Add(middleware);
                }
            );
    }

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
}
