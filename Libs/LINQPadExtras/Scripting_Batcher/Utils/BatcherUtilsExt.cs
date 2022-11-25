namespace LINQPadExtras.Scripting_Batcher.Utils;

static class BatcherUtilsExt
{
	public static string GetArtifactLink(this string artifact) => (Directory.Exists(artifact), File.Exists(artifact)) switch
	{
		(true, false) => artifact,
		(false, true) => Path.GetDirectoryName(artifact) ?? artifact,
		_ => artifact
	};
}