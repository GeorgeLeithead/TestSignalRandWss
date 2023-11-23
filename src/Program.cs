namespace TestSignalRandWss;

public class Program()
{
	public static async Task<int> Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.Error.WriteLine("Usage: WebSocketSample <URL>");
			Console.Error.WriteLine("");
			Console.Error.WriteLine("To connect to an ASP.NET Connection Handler, use 'ws://example.com/path/to/hub' or 'wss://example.com/path/to/hub' (for HTTPS)");
			return 1;
		}

		await RunWebSockets(args[0]);
		return 0;
	}

	private static async Task RunWebSockets(string url)
	{
		FeedClient webSocketClient = new FeedClient();
		webSocketClient.MessageReceived += (_, message) => Console.WriteLine($"Received message: {message}");

		webSocketClient.Connected += (_, message) => Console.WriteLine(message);

		webSocketClient.Disconnected += (_, message) => Console.WriteLine(message);

		webSocketClient.ErrorOccurred += (_, errorMessage) => Console.WriteLine($"Error: {errorMessage}");

		await webSocketClient.StartAsync(url);
	}
}
