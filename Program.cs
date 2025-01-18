using Renci.SshNet;

string host = "10.20.59.235";
string username = "admins";
string password = "atrbpn123";

try
{
    using (var sshClient = new SshClient(host, username, password))
    {
        Console.WriteLine("Connecting to SSH server...");
        sshClient.Connect();

        if (sshClient.IsConnected)
        {
            Console.WriteLine("Connected successfully!");

            string command = "ls -l";
            var cmd = sshClient.CreateCommand(command);
            var result = cmd.Execute();

            Console.WriteLine("Command executed successfully:");
            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine("Failed to connect to SSH server.");
        }

        sshClient.Disconnect();
        Console.WriteLine("Disconnected");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}

Console.WriteLine("Hello, World!");
