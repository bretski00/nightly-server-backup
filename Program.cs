using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            var networkPath = args[0];
            var encryptedFileName = args[1];
            var folderName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

            // Load encrypted data from file
            byte[] encryptedData = File.ReadAllBytes(encryptedFileName);

            // Get additional entropy from environment variable
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string additionalEntropyStr = Environment.GetEnvironmentVariable("ADDITIONAL_ENTROPY");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (string.IsNullOrWhiteSpace(additionalEntropyStr))
            {
                throw new Exception("Environment variable ADDITIONAL_ENTROPY is not set or empty. Aborting.");
            }

            // Convert entropy to byte array
            byte[] additionalEntropy = ConvertStringToByteArray(additionalEntropyStr);

            // Decrypt the data
            string decryptedJson = DecryptData(encryptedData, additionalEntropy);

            // Deserialize JSON to get configuration items
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            dynamic config = JsonConvert.DeserializeObject(decryptedJson);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Create Directory for Backup
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CreateDirectory(config.Username, config.Password, networkPath, folderName);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not decrypted. An error occurred.");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred:");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
        }
    }

    static string DecryptData(byte[] encryptedData, byte[] entropy)
    {
        try
        {
            // Decrypt the data
#pragma warning disable CA1416 // Validate platform compatibility
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Validate platform compatibility
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException e)
        {
            throw new CryptographicException("Data decryption failed. An error occurred.", e);
        }
    }

    static byte[] ConvertStringToByteArray(string str)
    {
        string[] byteStrings = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        byte[] byteArray = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {
            if (!byte.TryParse(byteStrings[i], out byteArray[i]))
            {
                throw new Exception($"Error parsing byte at index {i}: {byteStrings[i]} is not a valid byte.");
            }
        }
        return byteArray;
    }

    static void CreateDirectory(string username, string password, string networkPath, string folderName)
    {
        // Create a network credential object
        NetworkCredential credentials = new NetworkCredential(username, password);

        // Create a credential cache and add the network credentials
        CredentialCache networkCredentialCache = new CredentialCache();
        networkCredentialCache.Add(new Uri(networkPath), "Basic", credentials);

        // Set the default network credentials to the credential cache
        WebRequest.DefaultWebProxy.Credentials = networkCredentialCache;

        // Create the directory using the network path
        Directory.CreateDirectory(Path.Combine(networkPath, folderName));
        Console.WriteLine("Folder created successfully.");
    }

    static void ExecuteBackup(string backupDestinationFolder, string serverBackupItems)
    {
        // Command to execute
        string command = "wbadmin";

        // Arguments for the command
        string arguments = $"start backup -backupTarget:'{backupDestinationFolder}' -include:{serverBackupItems} -vssCopy -quiet"; // Example: "/?" to get help information

        // Create process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process
        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            int exitCode = process.ExitCode;
            Console.WriteLine($"Process exited with code: {exitCode}");
        }
    }
}
