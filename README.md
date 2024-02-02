<p align="center">
    <img src="ssh-commander-icon.png" width="128px" alt="SSH.Commander logo" />
</p>

# SSH.Commander

#### [![Nuget](https://img.shields.io/nuget/v/SSH.Commander)](https://www.nuget.org/packages/SSH.Commander/) 

dotnet library for convenient use of ssh

the library can use the config file ~/.ssh/config

## Install

    dotnet add package SSH.Commander

## Reference
> The project uses another git (without nuget):
>
> [https://github.com/JeremySkinner/Ssh-Config-Parser.git](https://github.com/JeremySkinner/Ssh-Config-Parser.git)

## Usage

```c#
SshUri sshUri = new("myserver"); //hostname from ~/.ssh/config
// or
//SshUri sshUri = new("user:pass@sshhost:22");
var sshClient = new SshCommanderClient(sshUri.Host)
{
    Config = new() { VerboseConsoleLog = true, UseUserSshConfigFile = true }
};
string command = " date && echo \"OK\"";
string output = await sshClient.RequestAsync(command);

// output: 
// > Thu 01 Feb 2024 02:09:40 PM +09
// > OK
```

