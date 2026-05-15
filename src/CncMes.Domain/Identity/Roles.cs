namespace CncMes.Domain.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Operator = "Operator";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Manager, Operator };
}
