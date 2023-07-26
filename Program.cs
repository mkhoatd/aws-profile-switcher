// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using Sharprompt;

var userHomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var awsDir = Path.Combine(userHomeDir, ".aws");
var configFilePath = Path.Combine(awsDir, "config");
// read whole config file
var config = File.ReadAllText(configFilePath);
// detect profiles
var profiles = Regex.Matches(config, @"\[(?<profile>[^\]]+)\]")
    .Select(m => m.Groups["profile"].Value)
    .Select(p => p.Trim().Replace("profile ", ""))
    .ToList();
// read .zshrc file
var zshrcFilePath = Path.Combine(userHomeDir, ".zshrc");
var apsConfigFilePath = Path.Combine(userHomeDir, ".aps_config");
// check if apsFile exists
if (!File.Exists(apsConfigFilePath))
{
    File.WriteAllText(apsConfigFilePath, "export AWS_PROFILE=default");
}
var apsConfigContent = File.ReadAllText(apsConfigFilePath).Split("\n");
if (!apsConfigContent.Length.Equals(1) || !apsConfigContent[0].Contains("AWS_PROFILE"))
{
    File.WriteAllText(apsConfigFilePath, "export AWS_PROFILE=default");
}
apsConfigContent = File.ReadAllText(apsConfigFilePath).Split("\n");
var zshrc = File.ReadAllText(zshrcFilePath).Split("\n");
var apsFileLine = zshrc.FirstOrDefault(l => l.Contains("""
                                                       alias aps="aps && source ~/.aps_config"
                                                       """));
if (apsFileLine is null)
{
    var newZshrc = zshrc.Append("""
                                alias aps="aps && source ~/.aps_config"
                                """).ToArray();
    var bakZshrcFilePath = Path.Combine(userHomeDir, ".zshrc.bak");
    File.Copy(zshrcFilePath, bakZshrcFilePath, true);
    File.WriteAllLines(zshrcFilePath, newZshrc);
}
var profileLine = apsConfigContent[0]!;
var oldProfileName = profileLine.Split("=").LastOrDefault()?.Trim();
var newProfile = Prompt.Select("Select new profile", profiles);
if (oldProfileName != null)
{
    var bakApsConfigFileePath = Path.Combine(userHomeDir, ".aps_config.bak");
    File.Copy(apsConfigFilePath, bakApsConfigFileePath, true);
    var newApsConfigContent = profileLine.Replace(oldProfileName, newProfile);
    File.WriteAllText(apsConfigFilePath, newApsConfigContent);
}
else 
{
    var newApsConfigContent = zshrc.Append($"export AWS_PROFILE={newProfile}");
    File.WriteAllLines(apsConfigFilePath, newApsConfigContent);
}
