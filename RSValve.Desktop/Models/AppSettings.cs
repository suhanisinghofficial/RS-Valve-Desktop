namespace RSValve.Desktop.Models;

public sealed class AppSettings
{
    public string MainUrl { get; set; } = "https://rsvalve.tractioncontrolsbc.com";
    public string Lobby { get; set; } = "SBC";
    public string Token { get; set; } = "";
}
