namespace KNXBoostDesktop;

public class ProgressBarProgressReporter : IProgress<int>
{
    public event EventHandler<int> ProgressChanged;

    public void Report(int value)
    {
        ProgressChanged?.Invoke(this, value);
    }
}
