using Microsoft.Win32;

namespace TileStyle.Common;

public class ServiceInstaller
{
    private const string RUN_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Install the application in the <see cref="RUN_REGISTRY_PATH"/> registery.
    /// </summary>
    /// <param name="appName">The name of the application to install. This will be used as the key.</param>
    /// <param name="appPath">The full path to the application.</param>
    /// <param name="arguments">Arguments for the application</param>
    public void Install(string appName, string appPath, string? arguments = null)
    {
        string command = string.IsNullOrEmpty(arguments)
            ? $"\"{appPath}\""
            : $"\"{appPath}\" {arguments}";

        using RegistryKey runKey = Registry.CurrentUser.OpenSubKey(RUN_REGISTRY_PATH, true)
                                   ?? Registry.CurrentUser.CreateSubKey(RUN_REGISTRY_PATH);
        runKey.SetValue(appName, command, RegistryValueKind.String);
    }

    /// <summary>
    /// Remove the application from startup.
    /// </summary>
    /// <param name="appName">The key/name of the application.</param>
    /// <exception cref="InvalidOperationException">If the user isnt allowed to remove the key, an invalid operation exception is thrown.</exception>
    public void Uninstall(string appName)
    {
        using RegistryKey? runRegistryKey = Registry.CurrentUser.OpenSubKey(RUN_REGISTRY_PATH, true);
        if (runRegistryKey is null)
        {
            return;
        }

        try
        {
            runRegistryKey.DeleteValue(appName, throwOnMissingValue: false);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Permission denied when trying to remove the registry value.", ex);
        }
    }
	
    /// <summary>
    /// Checks if the key <see cref="appName"/> is present in the registry.
    /// </summary>
    /// <param name="appName">The key/name of the application.</param>
    /// <returns>Returns true if present.</returns>
    public bool IsInstalled(string appName) =>
        Registry.CurrentUser.OpenSubKey(RUN_REGISTRY_PATH)?.GetValue(appName) is not null;
}