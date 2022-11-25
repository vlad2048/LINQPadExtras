using LINQPad;
using LINQPadExtras.Utils;

namespace LINQPadExtras;

static class ModuleInit
{
	private const string VarName = "restart-detector";

	public static void Check()
	{
		var val = CssVars.GetJS(VarName);
		if (val != string.Empty) return;
		CssVars.SetJS(VarName, "set");
		Init();
	}


	//[ModuleInitializer]
	//internal static void Init()
	private static void Init()
	{
		Util.HtmlHead.AddStyles(CssCmdPanel);
		Util.HtmlHead.AddStyles(CssDialog);
		Util.HtmlHead.AddStyles(CssDialogYesNo);
		Util.HtmlHead.AddStyles(CssTooltip);
		CssVars.Init();
	}

	private const string CssCmdPanel = """
		:root {
			--cmdpanel-output-border-color-light: #4e4e4e;
			--cmdpanel-output-border-color-dark: #272727;
		}

		.cmdpanel {
		}

		.cmdpanel-header {
			display: flex;
			column-gap: 5px;
			cursor: pointer;
		}
		.cmdpanel-header-active:hover {
			background-color: #465360;
		}
		
		.cmdpanel-header-statusbtn {
			color: #cbd769;
			width: 9px;
		}
		.cmdpanel-header-exe {
			color: #17ff68;
		}
		.cmdpanel-header-args {
			color: #00ffdd;
		}

		.cmdpanel-output {
			background-color: #484855;
			margin-left: 10px;
			padding-left: 5px;
			font-size: 12px;
			margin-top: 5px;
			border-left: 1px solid var(--cmdpanel-output-border-color-light);
			border-top: 1px solid var(--cmdpanel-output-border-color-light);
			border-right: 1px solid var(--cmdpanel-output-border-color-dark);
			border-bottom: 1px solid var(--cmdpanel-output-border-color-dark);
			background-color: #484855;
			margin-left: 10px;
			padding-left: 5px;
			font-size: 12px;
			margin-top: 5px;
			border-left: 1px solid var(--cmdpanel-output-border-color-light);
			border-top: 1px solid var(--cmdpanel-output-border-color-light);
			border-right: 1px solid var(--cmdpanel-output-border-color-dark);
			border-bottom: 1px solid var(--cmdpanel-output-border-color-dark);
		}

		.cmdpanel-output-stdout {
		}
		.cmdpanel-output-stderr {
			background-color: #61221d;
		}
		""";


	private const string CssDialog = """
		:root {
			--modal-backcolor: #22252A;
			--modal-backcolor-main: #353A40;
			--modal-type-normal-color: #d8d8d8;
			--modal-type-error-color: #df5252;
			--modal-gap: 10px;
			--modal-text-color: #d8d8d8;
			--modal-main-text-color: #ffffff;
		}

		.modal {
			/* Outer */
			position: absolute; inset: 0; left: 0; top: 0; margin: auto; padding: 0; border: 0;
			width: 100%; height: 100%;
			z-index: 2;
			/* Inner */
			display: flex;
			justify-content: center;
			/* Pretty */
			backdrop-filter: blur(2px);
			background-color: transparent;
		}


		.modal-inner {
			/* Outer */
			margin: auto 0; padding: 10px;
			/*width: 100%; height: 300px;
			max-width: 400px;*/
			max-width: calc(100% - 80px);
			max-height: calc(100% - 80px);
			flex: 0 1 auto;
			/* Inner */
			/*display: grid;
			grid-template-rows: auto 1fr auto;
			gap: var(--modal-gap);*/
			display: flex;
			flex-direction: column;
			row-gap: var(--modal-gap);
			/* Pretty */
			border-radius: 20px; border-top: 1px solid #333; border-left: 1px solid #333; border-right: 1px solid #222; border-bottom: 1px solid #222;
			background-color: var(--modal-backcolor);
			box-shadow: 2px 2px 2px 2px #1f2125, 7px 7px 14px 2px #131313C0;
			font-size: 18px;
			white-space: nowrap;
			overflow: hidden;
		}

		.modal-inner-max {
			width: calc(100% - 80px);
			height: calc(100% - 80px);
		}

		.modal-header {
			/* Outer */
			flex: 0 1 auto;
			/* Inner */
			display: flex;
			justify-content: space-between;
			align-items: center;
			/* Pretty */
			font-size: 24px;
			font-weight: bold;
			color: var(--modal-text-color);
		}

		.modal-header-closeicon {
			stroke: white;
			stroke-width: 2px;
			background-color: #555;
			border-radius: 5px;
			cursor: pointer;
		}
		.modal-header-closeicon:hover {
			background-color: #777;
		}

		.modal-main {
			/* Outer */
			flex: 1 1 auto;
			/* Inner */
			display: flex;
			overflow-y: hidden;

			/* Pretty */
			background-color: var(--modal-backcolor-main);
			font-size: 16px;
			color: var(--modal-main-text-color);
			padding: var(--modal-gap);
		}

		.modal-main-inner {
			width: 100%;
			/* Pretty */
			overflow: auto;
		}

		.modal-main-inner thead>tr:first-of-type {
			display: none;
		}


		.modal-footer {
			/* Outer */
			flex: 0 1 auto;
			/* Inner */
			display: flex;
			justify-content: space-between;
			align-items: center;
		}
		.modal-footer-part {
			/* Inner */
			display: flex;
			column-gap: var(--modal-gap);
		}

		.modal-button {
			background-color: #495057;
			border: 2px solid #495057;
			border-radius: 3px;
			color: var(--modal-text-color);
			font-weight: bold;
		}
		.modal-button:not([disabled]) {
			cursor: pointer;
		}
		.modal-button:hover:not(:disabled) {
			background-color: #777;
		}
		.modal-button-main {
			background-color: transparent;
			color: #91A7FF;
		}
		.modal-button:disabled {
			color: #383838;
		}
		""";

	private const string CssDialogYesNo = """
		.modal-yesno-div {
			display: flex;
			column-gap: 20px;
		}

		.modal-yesno-button {
			border-radius: 3px;
			font-weight: bold;
			background-color: transparent;
			margin: 10px;
			padding: 5px 20px;
			color: var(--modal-text-color);
		}
		.modal-yesno-button:not(:disabled) {
			cursor: pointer;
		}
		.modal-yesno-button:disabled {
			border: 2px solid #51565a;
			color: #808080;
		}

		.modal-yesno-button-yes {
			background-color: #20e63a10;
		}
		.modal-yesno-button-yes:not(:disabled) {
			border: 2px solid #20e63a;
		}
		.modal-yesno-button-yes:hover:not(:disabled) {
			background-color: #20e63a30;
		}

		.modal-yesno-button-no {
			background-color: #e6202010;
		}
		.modal-yesno-button-no:not(:disabled) {
			border: 2px solid #e62020;
		}
		.modal-yesno-button-no:hover:not(:disabled) {
			background-color: #e6202030;
		}
		""";


	private const string CssTooltip = """
		.tooltip {
			position: relative;
			display: block;
			border-bottom: 1px dotted black;
		}

		.tooltip .tooltiptext {
			visibility: hidden;
			width: 120px;
			background-color: #555;
			color: #fff;
			text-align: left;
			border-radius: 6px;
			padding: 5px;
			position: absolute;
			z-index: 1;
			bottom: 125%;
			left: 50%;
			margin-left: -60px;
			opacity: 0;
			transition: opacity 0.3s;
		}

		.tooltip .tooltiptext::after {
			content: "";
			position: absolute;
			top: 100%;
			left: 50%;
			margin-left: -5px;
			border-width: 5px;
			border-style: solid;
			border-color: #555 transparent transparent transparent;
		}

		.tooltip:hover .tooltiptext {
			visibility: visible;
			opacity: 1;
		}
		.tooltip {
			position: relative;
			display: block;
			border-bottom: 1px dotted black;
		}

		.tooltip .tooltiptext {
			visibility: hidden;
			width: 120px;
			background-color: #555;
			color: #fff;
			text-align: left;
			border-radius: 6px;
			padding: 5px;
			position: absolute;
			z-index: 1;
			bottom: 125%;
			left: 50%;
			margin-left: -60px;
			opacity: 0;
			transition: opacity 0.3s;
		}

		.tooltip .tooltiptext::after {
			content: "";
			position: fixed;
			top: 100%;
			left: 50%;
			margin-left: -5px;
			border-width: 5px;
			border-style: solid;
			border-color: #555 transparent transparent transparent;
		}

		.tooltip:hover .tooltiptext {
			visibility: visible;
			opacity: 1;
		}
		""";
}