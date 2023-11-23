using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace TestSignalRandWss;

public class FeedClient
{
	private HubConnection? connection;

	public event EventHandler<string>? Connected;

	public event EventHandler<string>? Disconnected;

	public event EventHandler<string>? ErrorOccurred;

	public event EventHandler<string>? MessageReceived;

	public async Task ReconnectAsync()
	{
		while (connection is not null)
		{
			try
			{
				await Task.Delay(5000); // Delay before attempting to reconnect
				await connection.StartAsync();
				OnConnected("Reconnected to WebSocket feed.");
				break;
			}
			catch (Exception ex)
			{
				OnErrorOccurred($"Reconnection error: {ex.Message}");
			}
		}
	}

	public async Task StartAsync(string webSocketUrl)
	{
		var uri = new Uri(webSocketUrl);
		connection = new HubConnectionBuilder()
			.WithUrl(uri, Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets)
			.WithAutomaticReconnect()
			.AddJsonProtocol()
			.Build();

		if (connection is null)
		{
			OnErrorOccurred("Connection is null");
			return;
		}

		connection.Closed += async (error) =>
		{
			OnDisconnected(error?.Message ?? string.Empty);
			await ReconnectAsync();
		};

		_ = connection.On<string>("ReceiveMessage", OnMessageReceived);

		try
		{
			await connection.StartAsync(CancellationToken.None);
			OnConnected("Connected to WebSocket feed.");
		}
		catch (Exception ex)
		{
			OnErrorOccurred($"Error connecting to WebSocket feed: {ex.Message}");
		}
	}

	public void Stop()
	{
		if (connection is not null)
		{
			_ = connection.StopAsync();
		}

		OnDisconnected("Disconnected from WebSocket feed.");
	}

	private void OnConnected(string message) => Connected?.Invoke(this, message);

	private void OnDisconnected(string message) => Disconnected?.Invoke(this, message);

	private void OnErrorOccurred(string errorMessage) =>  ErrorOccurred?.Invoke(this, errorMessage);

	private void OnMessageReceived(string message) => MessageReceived?.Invoke(this, message);
}
