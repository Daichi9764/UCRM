namespace KNXBoostDesktop;

public interface IProgressReporter<T>
{
    event EventHandler<T> ProgressChanged;
}