public interface IEmployeeRepository
{
    public Task<Employee?> GetPersonById(
        string department,
        string email,
        CancellationToken cancellationToken
    );

    public Task<IReadOnlyList<Employee>> SearchByEmail(
        string email,
        CancellationToken cancellationToken
    );

    public Task<Employee?> CreateEmployee(Employee employee, CancellationToken cancellationToken);

    public Task<IReadOnlyList<Employee>> QueryByDepartment(
        string department,
        string emailStartsWith,
        DateTime updatedBefore,
        CancellationToken cancellationToken
    );

    public Task<Employee?> UpdateLastName(
        string department,
        string email,
        string lastname,
        CancellationToken cancellationToken
    );

    public Task DeleteEmployee(
        string department,
        string email,
        CancellationToken cancellationToken
    );
}
