# malias - Mail Alias Manager

Generate unique email aliases for online services,
detect data breaches and regenerate an address for 
one service in case of spam.

The tool adds an random code into the email address,
so that the address cannot be guessed.

```
<prefix>.<name>.<randomcode>@example.com
a.amazon.a4uth@example.com
```

## Status

Current status: Early Alpha

- [x] Provider System
- [x] Configuration File
- [x] Create alias
- [x] List alias
- [ ] Force recreation
- [ ] Delete old variant of an alias
- [ ] Silent (no output) mode

Known issues:

- `requests/all-inkl` is not included in build, so requests 
  fail if the work directory is not the git root directory

## Create an alias

```sh
./publish/maliasmgr --create amazon
```

Output:

```
a.amazon.a4uth@example.com
```

## Supported Providers

### All-Inkl.com

Configuration Parameters for `~/mailias.json`:

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
./publish/maliasmgr --list
```
