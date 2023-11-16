using System;
using System.Threading;

public class RepeatingCancellationTokenSource
{
    private CancellationTokenSource _cancellationTokenSource;
    private bool _destroyed;

    public CancellationToken Token => _cancellationTokenSource.Token;

    public RepeatingCancellationTokenSource()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    ~RepeatingCancellationTokenSource()
    {
        if(!_destroyed)
        {
            Destroy();
        }
    }

    public void Reset()
    {
        ThrowIfDestroyed();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Destroy()
    {
        ThrowIfDestroyed();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _destroyed = true;
    }

    private void ThrowIfDestroyed()
    {
        if (_destroyed)
        {
            throw new InvalidOperationException("Cancelltion source has already destroy");
        }
    }
}
