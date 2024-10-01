using Amazon.DynamoDBv2;

internal class Program
{
    public static async Task CreateDepartments(
        DynamoDBEmployeeRepository repository,
        CancellationToken cancellationToken
    )
    {
        if (await repository.EnsureTableIsCreated(cancellationToken) is false)
        {
            await repository.GenerateEmployeesInDeparment("IT", 100, cancellationToken);
            await repository.GenerateEmployeesInDeparment("Sales", 100, cancellationToken);
        }
    }

    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var dynamoDb = new AmazonDynamoDBClient(
            new AmazonDynamoDBConfig() { ServiceURL = "http://localhost:8000" }
        );
        var repository = new DynamoDBEmployeeRepository(dynamoDb);
        await CreateDepartments(repository, cts.Token);

        await UseRepository(repository, cts.Token);
    }

    private static async Task UseRepository(
        IEmployeeRepository repository,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("Querying employees");
        var salesDepartment = await repository.QueryByDepartment(
            "Sales",
            "Email1",
            DateTime.UtcNow,
            cancellationToken
        );

        Console.WriteLine("Creating employee");
        await repository.CreateEmployee(
            new Employee(
                "IT",
                "someEmail@test.com",
                "Andersson",
                new[] { "Software Development" },
                new Metadata(DateTime.UtcNow)
            ),
            cancellationToken
        );

        Console.WriteLine("Getting employee");
        var employee = await repository.GetPersonById(
            "IT",
            "someEmail@test.com",
            cancellationToken
        );

        var updatedEmployee = await repository.UpdateLastName(
            employee!.Department,
            employee.Email,
            "Sparrow",
            cancellationToken
        );

        Console.WriteLine("Deleting employee");
        await repository.DeleteEmployee(
            updatedEmployee!.Department,
            updatedEmployee.Email,
            cancellationToken
        );

        Console.WriteLine($"Original: {employee}");
        Console.WriteLine($"Updated: {updatedEmployee}");
        foreach (var salesEmployee in salesDepartment)
            Console.WriteLine($"Queried: {salesEmployee}");
    }
}
