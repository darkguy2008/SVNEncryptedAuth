
SVN Encrypted Authentication wrapper
Window's C# IAT-hooking patch (through EasyHook) for encrypting passwords in the auth file when serving a SVN repository through svnserve.

This is a kind of a hack, written with the help of the EasyHook binaries in order to provide a bit of security when using the svnserve binary. It will attach to a running svnserve.exe binary and patch its CreateFileA/W function in order to redirect the file being accessed to a different one, while keeping the original one with encrypted passwords instead of plaintext ones.

Passwords must be changed in:

    SVNEncryptedAuthHelper\SVNEncryptedAuthHelper.cs
    SVNEncryptedAuthHelper\Crypto.cs
    SVNEncryptedPasswordGen\Program.cs (optional)

I bet there is room for improvements, but the app works fine and dandy as-is. I've also included a small console app to generate passwords with masking in case you need to tell someone else to input a password by themselves.

I you find this useful :)

- DARKGuy
