using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

string srcHost = configuration["Source:Host"] ?? string.Empty;
string srcUsername = configuration["Source:Username"] ?? string.Empty;
string srcPassword = configuration["Source:Password"] ?? string.Empty;
string srcCommand = configuration["Source:Command"] ?? string.Empty;

logger.LogInformation("{0} - {1} - {2}", srcHost, srcUsername, srcPassword);

string dstHost = configuration["Destination:Host"] ?? string.Empty;
string dstUsername = configuration["Destination:Username"] ?? string.Empty;
string dstPassword = configuration["Destination:Password"] ?? string.Empty;
string dstCommand = configuration["Destination:Command"] ?? string.Empty;

string outputFilePath = configuration["OutputFilePath"] ?? string.Empty;
Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);

try
{
    using (var sshClient = new SshClient(srcHost, srcUsername, srcPassword))
    {
        logger.LogInformation("Connecting to SSH server...");
        sshClient.Connect();

        if (sshClient.IsConnected)
        {
            logger.LogInformation("Connected successfully!");

            // Create a CancellationTokenSource to allow graceful shutdown
            using var cts = new CancellationTokenSource();

            // Start the repeating task
            var task = RunPeriodicTaskAsync(TimeSpan.FromMinutes(1), cts.Token, sshClient, srcCommand, logger, outputFilePath);

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Signal cancellation and wait for the task to stop
            cts.Cancel();
            await task;
        }
        else
        {
            logger.LogError("Failed to connect to SSH server!");
        }

        sshClient.Disconnect();
        logger.LogInformation("Disconnected from SSH server!");
    }
}
catch (Exception e)
{
    logger.LogError(e, e.Message);
}

Console.WriteLine("Hello, World!");



static string RunSshCommand(SshClient sshClient, string command, ILogger logger, string outputFilePath)
{
    try
    {
        string dateTimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        logger.LogInformation(dateTimeNow);
        File.AppendAllText(outputFilePath, dateTimeNow + Environment.NewLine);
        logger.LogInformation("Success writing DateTime to file");

        logger.LogInformation("Executing command: {0}", command);

        var cmd = sshClient.CreateCommand(command);
        var result = cmd.Execute();

        logger.LogInformation("Result: {0}", result);
        File.AppendAllText(outputFilePath, result + Environment.NewLine);
        logger.LogInformation("Success writing Result to file");

        return result;
    }
    catch (Exception e)
    {
        logger.LogError(e, e.Message);
        throw;
    }
}

static async Task RunPeriodicTaskAsync(TimeSpan interval, CancellationToken cancellationToken, SshClient sshClient, string command, ILogger logger, string outputFilePath)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            // Perform your task
            Console.WriteLine($"Task executed at {DateTime.Now}");
            string result = RunSshCommand(sshClient, command, logger, outputFilePath);

            // Wait for the specified interval
            await Task.Delay(interval, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Task was cancelled, break the loop
            break;
        }
        catch (Exception ex)
        {
            // Log unexpected errors and continue
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
