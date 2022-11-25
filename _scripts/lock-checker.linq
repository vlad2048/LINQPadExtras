<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <Namespace>LINQPadExtras.DialogLogic</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>LINQPadExtras.Scripting_LockChecker</Namespace>
</Query>

void Main()
{
	Run();
	Run();
}

void Run()
{
	var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
	Directory.CreateDirectory(folder);
	var file = Path.Combine(folder, "lock.txt").Dump();
	using (var fs = File.OpenWrite(file))
	{
		var ok = LockChecker.CheckFolders(folder);
		ok.Dump();
	}
	Directory.Delete(folder, true);
	
	Enumerable.Range(0, 10).Dump();
}
