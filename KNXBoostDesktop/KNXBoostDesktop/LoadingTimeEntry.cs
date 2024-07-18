namespace KNXBoostDesktop;

public class LoadingTimeEntry
{
    public string ProjectName { get; init; } = null!;
    public int AddressCount { get; init; }
    public int DeviceCount { get; init; }
    public bool IsDeleted { get; init; }
    public int DeletedAddresses { get; init; }
    public bool IsTranslated { get; init; }
    public TimeSpan TotalLoadingTime { get; init; }

}