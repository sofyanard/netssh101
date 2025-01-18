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
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

string srcHost = configuration["Source:Host"] ?? string.Empty;
string srcUsername = configuration["Source:Username"] ?? string.Empty;
string srcPassword = configuration["Source:Password"] ?? string.Empty;
string srcCommand = configuration["Source:Command"] ?? string.Empty;

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

            string dateTimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logger.LogInformation(dateTimeNow);
            File.AppendAllText(outputFilePath, dateTimeNow + Environment.NewLine);

            logger.LogInformation("Executing command: {0}", srcCommand);

            var cmd = sshClient.CreateCommand(srcCommand);
            var result = cmd.Execute();

            logger.LogInformation("Command executed successfully!");
            logger.LogInformation(result);
            File.AppendAllText(outputFilePath, result + Environment.NewLine);
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
