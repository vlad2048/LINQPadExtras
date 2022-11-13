using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LINQPadExtras.Utils;

static class LockFinder
{
	public static List<Process> WhoIsLockingFile(string file) => WhoIsLockingFiles(new[] { file });

	public static List<Process> WhoIsLockingFolder(string folder) => WhoIsLockingFiles(Files.FindRecursively(folder, "*.*"));


	/// <summary>
	/// Find out what process(es) have a lock on the specified file.
	/// </summary>
	/// <param name="files">Path of the files.</param>
	/// <returns>Processes locking the file</returns>
	/// <remarks>See also:
	/// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
	/// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
	/// 
	/// </remarks>
	private static List<Process> WhoIsLockingFiles(string[] files)
	{
		var key = Guid.NewGuid().ToString();
		var processes = new List<Process>();

		var res = RmStartSession(out var handle, 0, key);
		if (res != 0)
			throw new Exception("Could not begin restart session.  Unable to determine file locker.");

		try
		{
			var pnProcInfo = 0u;
			var lpdwRebootReasons = RmRebootReasonNone;
			var resources = files; // Just checking on one resource.

			res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);
			if (res != 0)
				throw new Exception("Could not register resource.");

			//Note: there's a race condition here -- the first call to RmGetList() returns
			//      the total number of process. However, when we call RmGetList() again to get
			//      the actual processes this number may have increased.
			res = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
			switch (res)
			{
				case ERROR_MORE_DATA:
				{
					// Create an array to store the process results
					RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
					pnProcInfo = pnProcInfoNeeded;

					// Get the list
					res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
					if (res == 0)
					{
						processes = new List<Process>((int)pnProcInfo);

						// Enumerate all of the results and add them to the 
						// list to be returned
						for (int i = 0; i < pnProcInfo; i++)
						{
							try
							{
								processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
							}
							// catch the error -- in case the process is no longer running
							catch (ArgumentException) { }
						}
					}
					else throw new Exception("Could not list processes locking resource.");

					break;
				}

				case 0:
					break;

				default:
					throw new Exception($"Could not list processes locking resource. Failed to get size of result. res={res}");
			}
		}
		finally
		{
			RmEndSession(handle);
		}

		return processes;
	}






	private const int ERROR_MORE_DATA = 234;
	
	[StructLayout(LayoutKind.Sequential)]
	private struct RM_UNIQUE_PROCESS
	{
		public readonly int dwProcessId;
		private readonly System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
	}

	private const uint RmRebootReasonNone = 0;
	private const int CCH_RM_MAX_APP_NAME = 255;
	private const int CCH_RM_MAX_SVC_NAME = 63;

	private enum RM_APP_TYPE
	{
		// ReSharper disable UnusedMember.Local
		RmUnknownApp = 0,
		RmMainWindow = 1,
		RmOtherWindow = 2,
		RmService = 3,
		RmExplorer = 4,
		RmConsole = 5,
		RmCritical = 1000
		// ReSharper restore UnusedMember.Local
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct RM_PROCESS_INFO
	{
		public readonly RM_UNIQUE_PROCESS Process;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
		private readonly string strAppName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
		private readonly string strServiceShortName;

		private readonly RM_APP_TYPE ApplicationType;
		private readonly uint AppStatus;
		private readonly uint TSSessionId;
		[MarshalAs(UnmanagedType.Bool)]
		private readonly bool bRestartable;
	}

	[DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
	private static extern int RmRegisterResources(
		uint pSessionHandle,
		uint nFiles,
		string[] rgsFilenames,
		uint nApplications,
		[In] RM_UNIQUE_PROCESS[]? rgApplications,
		uint nServices,
		string[]? rgsServiceNames
	);

	[DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
	private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

	[DllImport("rstrtmgr.dll")]
	private static extern int RmEndSession(uint pSessionHandle);

	[DllImport("rstrtmgr.dll")]
	private static extern int RmGetList(
		uint dwSessionHandle,
		out uint pnProcInfoNeeded,
		ref uint pnProcInfo,
		[In, Out] RM_PROCESS_INFO[]? rgAffectedApps,
		ref uint lpdwRebootReasons
	);
}