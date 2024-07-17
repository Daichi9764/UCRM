using System.IO;

namespace KNXBoostDesktop;

public class LoadingTimeEntry
{
    public string ProjectName { get; set; }
    public int AddressCount { get; set; }
    public int DeviceCount { get; set; }
    public bool IsDeleted { get; set; }
    public int DeletedAddresses { get; set; }
    public bool IsTranslated { get; set; }
    public TimeSpan TotalLoadingTime { get; set; }

}