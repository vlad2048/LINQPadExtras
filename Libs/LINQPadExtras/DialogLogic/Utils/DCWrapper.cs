using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using PowRxVar;

namespace LINQPadExtras.DialogLogic.Utils;

public interface IDCWrapper
{
    T UpdateContent<T>(T obj);
    T AppendContent<T>(T obj, bool inline = false);
}

class DCWrapper : IDCWrapper, IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly DumpContainer dc;
    private readonly ISubject<Unit> whenContentAdded;

    public IObservable<Unit> WhenContentAdded => whenContentAdded.AsObservable();

    public T UpdateContent<T>(T obj)
    {
        var res = dc.UpdateContent(obj);
        whenContentAdded.OnNext(Unit.Default);
        return res;
    }

    public T AppendContent<T>(T obj, bool inline = false)
    {
        var res = dc.AppendContent(obj, inline);
        whenContentAdded.OnNext(Unit.Default);
        return res;
    }

    public DCWrapper(DumpContainer dc)
    {
        this.dc = dc;
        whenContentAdded = new Subject<Unit>().D(d);
    }
}