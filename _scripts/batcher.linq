<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <Namespace>LINQPadExtras.Scripting_Batcher</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.ComponentModel</Namespace>
</Query>

void Main()
{
	var exe = Path.Combine(NativeMethods.GetLINQPadExtraRootFolder(), "Tools", "Cmder", "bin", "Debug", "net7.0", "Cmder.exe");
	
	Batcher.Run(
		"Running command",
		cmd =>
		{
			for (var i = 0; i < 20; i++)
			{
				cmd.Log($"Item_{i}");
				//Thread.Sleep(100);
			}
			
			//cmd.Run(exe, "500", "0");
		}
	);
}


public static class NativeMethods
{
	public static string GetLINQPadExtraRootFolder()
	{
		var folder = GetFinalPathName(Path.GetDirectoryName(Util.CurrentQueryPath)!);
		if (folder.StartsWith(@"\\?\"))
			folder = folder[4..];
		return Path.GetFullPath(Path.Combine(folder, ".."));
	}
	
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    private const uint FILE_READ_EA = 0x0008;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);

    private static string GetFinalPathName(string path)
    {
        var h = CreateFile(path, 
            FILE_READ_EA, 
            FileShare.ReadWrite | FileShare.Delete, 
            IntPtr.Zero, 
            FileMode.Open, 
            FILE_FLAG_BACKUP_SEMANTICS,
            IntPtr.Zero);
        if (h == INVALID_HANDLE_VALUE)
            throw new Win32Exception();

        try
        {
            var sb = new StringBuilder(1024);
            var res = GetFinalPathNameByHandle(h, sb, 1024, 0);
            if (res == 0)
                throw new Win32Exception();

            return sb.ToString();
        }
        finally
        {
            CloseHandle(h);
        }
    }
}