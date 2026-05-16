namespace CncMes.Domain.Production;

public static class JobStatus
{
    public const string Pending = "Pending";
    public const string Assigned = "Assigned";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Pending, Assigned, InProgress, Done, Cancelled
    };

    public static string ToTurkish(string status) => status switch
    {
        Pending    => "Beklemede",
        Assigned   => "Atandı",
        InProgress => "Devam Ediyor",
        Done       => "Tamamlandı",
        Cancelled  => "İptal",
        _          => status,
    };
}
