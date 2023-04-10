using LINQPad.Controls;
using LINQPad;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.PageLogic.Transformers;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Utils;
using PowMaybe;
using PowRxVar;

namespace LINQPadExtras.PageServing.ConnUIClientServer;

static class ConnUI
{
	public const string ConnClientId = "conn-client";
	public const string ConnServerId = "conn-server";
	private const string ConnClass = "conn-ui";

	private const string SharedCss = $$"""
		.{{ConnClass}} {
			display: flex;
			align-items: baseline;
			column-gap: 5px;
			background-color: #333;
			height: 25px;
		}
		.conn-ui-link {
			margin-left: auto;
		}
		""";

	public static ITransformer Client_BodyWrapper() =>
		// @formatter:off
		new BodyWrapperTransformer(body => $"""
			<div id="{ConnClientId}" class="{ConnClass}">
				<span>Client connection:</span>
				<span id="{FrontendScripts.RefreshStatusId}">not init</span>
			</div>
			{body}
			{FrontendScripts.JSNotifyLogic}
			""");
		// @formatter:off



	public static void Server_Dump(
		IRoVar<WSState> wsState,
		string? htmlEditFolder,
		int httpPort,
		PageMutator pageMutator,
		ReplierContentHolder contentHolder
	)
	{
		var pageUrl = $"http://{FrontendScripts.MachineName}:{httpPort}/";

		Util.HtmlHead.AddStyles(SharedCss);

		DumpContainer dcStatus, dcClose, dcError;
		TextBox folderText;

		var div = new Div(
			new Span("Server connection:"),
			dcStatus = new DumpContainer(),
			dcClose = new DumpContainer(),
			dcError = new DumpContainer(),
			new Label(pageUrl)
			{
				CssClass = "connection-link",
			},
			folderText = new TextBox(htmlEditFolder ?? string.Empty),
			new Button("Edit html", _ => PageSaver.Save(folderText.Text, pageMutator, contentHolder))
		)
		{
			CssClass = ConnClass,
			HtmlElement =
			{
				ID = ConnServerId
			}
		};

		wsState.Subscribe(state =>
		{
			dcStatus.UpdateContent($"{state.Status}");
			dcClose.UpdateContent(state.CloseNfo.IsSome(out var close) switch
			{
				true => $"code:{close!.Code} wasClean:{close.WasClean} reason:'{close.Reason}'",
				false => string.Empty
			});
			dcError.UpdateContent(state.ErrorNfo.IsSome(out var err) switch
			{
				true => $"err:{err!.Message} ex:{err.Ex.Message}",
				false => string.Empty
			});
		}).D(wsState);

		div.Dump();
	}
}