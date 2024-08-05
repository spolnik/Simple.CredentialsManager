Simple.CredentialsManager
=========================

C# Api for accessing Windows Credential Manager (reading, writing and removing of credentials)
Linux is also supported (see Linux Support below).
Tested up to .NET Core 8.0


# Linux Support
The tests were run against Ubuntu 24.04 (Desktop).

```sudo apt install dbus-x11 gnome-keyring libsecret-tools```

The Secret Service DBus-API is being used to retreive data from the Linux Credential Manager.

https://manpages.ubuntu.com/manpages/focal/man1/secret-tool.1.html
https://unix.stackexchange.com/questions/473528/how-do-you-enable-the-secret-tool-command-backed-by-gnome-keyring-libsecret-an
