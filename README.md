# malias - Mail Alias Manager

Generate unique email aliases for online services,
detect data breaches and regenerate an address for
one service in case of spam.

The tool adds an random code into the email address,
so that the address cannot be guessed.

```txt
<prefix>.<name>.<randomcode>@example.com
a.amazon.a4uth@example.com
```

## Options

`./malias --help`:

```txt
maliasmgr 1.0.0
Copyright (C) 2020 maliasmgr

  -c amazon, --create=amazon             What do you want to do?

  -d a.amazon.fj292@example.com
  --delete=a.amazon.fj292@example.com    Alias to delete

  -l, --list                             List existing aliases

  -i, --info                             Show current configuration

  -f, --force                            Force the current operation

  --delete-existing                      Delete existing alias with the same name

  --silent                               No output

  --help                                 Display this help screen.

  --version                              Display version information.
```

## Create an alias

```sh
./malias --create amazon
```

Output:

```txt
a.amazon.a4uth@example.com
```

## Configuration

```json
{
  "MailDomain": "example.com",
  "TargetAddress": "christian@example.com",
  "Prefix": "prefix",
  "UniqeIdLength": 5,
  "Provider": "ProviderName",
  "ProviderConfig": [
    { "Key": "KeyName", "Value": "KeyValue" }
  ]
}
```

| Config Property | Description |
|---|---|
| MailComain | Domain for creating mail aliases |
| TargetAddress | Email address we will create aliases for |
| Prefix | Prefix which is used on all aliases |
| UniqueIdLength | Length of the random code to make the email address unguessable |
| Provider | The used provider (see below) |
| ProviderConfig | Provider-specific settings (credencials for example) |
| ProviderConfig[i].Key | Name of one property required by a provider plugin |
| ProviderConfig[i].Value | Value of one property required by a provider plugin |

### Provider Config

Each provider can have different configuration parameters.
So these settings are designed to be completely dynamic.
Which parameters are required depends on the provider which is used.
See below.

## Supported Providers

### All-Inkl.com

Configuration Parameters for `~/malias.json`:

```json
{
  "MailDomain": "example.com",
  "TargetAddress": "christian@example.com",
  "Prefix": "a",
  "UniqeIdLength": 5,
  "Provider": "AllInkl",
  "ProviderConfig": [
    { "Key": "Username", "Value": "my kas username" },
    { "Key": "PasswordHash", "Value": "my kas password, sha1 hashed" }
  ]
}
```

### Your Provider

If you have C# / .NET Core skills, just create your own implementation of the `Data.IProvider`
interface and add the provider in the `Program.cs`.

## Build

You need the .NET Core 3.1 SDK to build this application.

```sh
git clone git@github.com:perryflynn/malias.git
cd malias
mkdir -p publish
dotnet publish --self-contained -o publish/ -r linux-x64 -c Release .
./publish/malias --list
```
