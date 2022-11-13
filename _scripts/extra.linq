<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0\LINQPadExtras.dll</Reference>
  <NuGetReference>CliWrap</NuGetReference>
  <Namespace>CliWrap</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LINQPadExtras.Styling</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>LINQPadExtras.CmdRunning</Namespace>
  <Namespace>LINQPadExtras</Namespace>
</Query>

const string file = @"C:\tmp\dd\lock.txt";
const string folder = @"C:\tmp\dd";

void Main()
{
	//var exeFile = @"C:\Dev_Nuget\Libs\LINQPadExtras\Tools\Cmder\bin\Debug\net7.0\Cmder.exe";
	//Con.Run(exeFile, "100", "0");
	
	Con.DeleteFile(file);
	//Con.DeleteFolder(folder);
}
