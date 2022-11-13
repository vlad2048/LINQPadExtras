namespace Cmder;

static class Program
{
	private static int PauseMs;

	public static int Main(string[] args)
	{
		PauseMs = int.Parse(args[0]);
		var isError = args[1] == "1";

		Out("Hello");
		Out("there");

		Err("Aie");
		Err("First error");

		Out("let's get started:");
		OutIn("Same line ... ");
		OutIn("continues ... ");

		Err("OuieOuieOuie");
		ErrIn("Inline");
		Err("Second error");

		OutIn("again ... ");
		Out("and now finishes");
		Out("");

		return isError switch
		{
			true => 123,
			false => 0
		};
	}

	private static void OutIn(string str)
	{
		Console.Write(str);
		Pause();
	}

	private static void Out(string str)
	{
		Console.WriteLine(str);
		Pause();
	}

	private static void ErrIn(string str)
	{
		Console.Error.Write(str);
		Pause();
	}

	private static void Err(string str)
	{
		Console.Error.WriteLine(str);
		Pause();
	}

	private static void Pause() => Sleep(PauseMs);
	private static void Sleep(int ms) => Thread.Sleep(ms);
}