using Godot;

static class Util {
	/// <summary>
	/// Returns the translation of the given translateID
	/// </summary>
	/// <param name="translateID"></param>
	/// <returns></returns>
	public static string Tr(string translateID) => TranslationServer.Translate(translateID);
}