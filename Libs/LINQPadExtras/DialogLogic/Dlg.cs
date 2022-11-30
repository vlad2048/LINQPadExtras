using LINQPad.Controls;
using LINQPad;
using PowRxVar;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using LINQPadExtras.DialogLogic.Utils;
using LINQPadExtras.DialogLogic.Enums;

namespace LINQPadExtras.DialogLogic;


public interface IDlg
{
	IDCWrapper DC { get; }
	Div DCWrap { get; }
	Div ScrollDiv { get; }
	CancellationToken CancelToken { get; }
	void Close();
	void AddButton(DlgBtnLocation location, DlgBtnType type, Button btn);
}


class Dlg : IDlg, IDisposable
{
	public Disp D { get; } = new();
	public void Dispose() => D.Dispose();

	private readonly DCWrapper dcWrapper;
	private readonly Control root;
	private readonly Div footerLeft;
	private readonly Div footerRight;
	private readonly ISubject<Unit> whenClosed = new AsyncSubject<Unit>();

	public IObservable<Unit> WhenClosed => whenClosed.AsObservable();

	public IDCWrapper DC => dcWrapper;
	public Div DCWrap { get; }
	public Div ScrollDiv { get; }
	public CancellationToken CancelToken { get; }

	public Dlg(DumpContainer dc, Control root, Div dcWrap, Div scrollDiv, Div footerLeft, Div footerRight)
	{
		dcWrapper = new DCWrapper(dc).D(D);
		DCWrap = dcWrap;
		ScrollDiv = scrollDiv;
		this.root = root;
		this.footerLeft = footerLeft;
		this.footerRight = footerRight;

		var cancelSource = new CancellationTokenSource().D(D);
		CancelToken = cancelSource.Token;

		dcWrapper.WhenContentAdded
			.Delay(TimeSpan.FromMilliseconds(500))
			.Subscribe(_ =>
			{
				var scrollHeight = scrollDiv.HtmlElement.GetAttribute("scrollHeight");
				scrollDiv.HtmlElement.SetAttribute("scrollTop", scrollHeight);
			});

		WhenClosed.Subscribe(_ =>
		{
			cancelSource.Cancel();
		}).D(D);
	}

	public void AddButton(DlgBtnLocation location, DlgBtnType type, Button btn)
	{
		btn.IsMultithreaded = true;
		var parent = location switch
		{
			DlgBtnLocation.FooterLeft => footerLeft,
			DlgBtnLocation.FooterRight => footerRight,
			_ => throw new ArgumentException()
		};
		btn.CssClass = type switch
		{
			DlgBtnType.Normal => "modal-button",
			DlgBtnType.Main => "modal-button modal-button-main",
			_ => throw new ArgumentException()
		};
		parent.Children.Add(btn);
	}

	public void Close()
	{
		root.Styles["display"] = "none";
		whenClosed.OnNext(Unit.Default);
		whenClosed.OnCompleted();
	}
}
