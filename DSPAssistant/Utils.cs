using DSharpPlus.ButtonCommands;
using Serilog;

namespace DSPAssistant;

public static class Utils
{
	private static string _connectionString;
	private static ButtonCommandsExtension _bce;

	public static void SetConnectionString(string host, string username, string password, string database)
	{
		_connectionString = $"Server={host};Port=5432;Database={database};User Id={username};Password={password};";
		Log.Information("Using connection string {ConnStr}",
			_connectionString.Replace("Password=" + password, "Password=<PASSWORD REMOVED>"));
	}

	public static string GetConnectionString() => _connectionString;

	public static void SetButtonCommands(ButtonCommandsExtension e) => _bce = e;
	public static string GetButtonId(string commandName, params object[] arguments) => _bce.BuildButtonId(commandName, arguments);

	// probably a bad idea idk im not being paid enough for this
	public static DatabaseContext CreateDatabaseContext() => new();
}