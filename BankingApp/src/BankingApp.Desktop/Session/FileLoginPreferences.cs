namespace BankingApp.Desktop.Session;

using System;
using System.IO;
using System.Text.Json;

/// <summary>Persists login preferences to a JSON file in the local app data directory.</summary>
public sealed class FileLoginPreferences : ILoginPreferences
{
    private const string PreferencesFileName = "login-preferences.json";

    private readonly string _filePath;
    private string? _savedEmail;
    private bool _rememberMe;

    /// <summary>Initializes a new instance of the <see cref="FileLoginPreferences"/> class and loads any saved preferences.</summary>
    public FileLoginPreferences()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankingApp");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, PreferencesFileName);
        Load();
    }

    /// <inheritdoc />
    public string? SavedEmail
    {
        get { return _savedEmail; }
    }

    /// <inheritdoc />
    public bool RememberMe
    {
        get { return _rememberMe; }
    }

    /// <inheritdoc />
    public void Save(string email, bool rememberMe)
    {
        _rememberMe = rememberMe;
        _savedEmail = rememberMe ? email : null;

        PreferencesData data = new()
        {
            Email = _savedEmail,
            RememberMe = _rememberMe
        };

        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(_filePath, json);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _savedEmail = null;
        _rememberMe = false;

        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            PreferencesData? data = JsonSerializer.Deserialize<PreferencesData>(json);
            if (data is not null)
            {
                _savedEmail = data.Email;
                _rememberMe = data.RememberMe;
            }
        }
        catch (JsonException)
        {
            Clear();
        }
    }

    private sealed class PreferencesData
    {
        public string? Email { get; set; }

        public bool RememberMe { get; set; }
    }
}
