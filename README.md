# SVN Encrypted Authentication wrapper

* * *

_C# IAT-hooking patch for Windows OS (through EasyHook) for encrypting passwords in the auth file when serving a SVN repository through svnserve._

This is a kind of a hack, written with the help of the EasyHook binaries in order to provide a bit of security when using the svnserve binary. It will attach to a running svnserve.exe binary and patch its CreateFileA/W function in order to redirect the file being accessed to a different, temporary file, while keeping the original one with encrypted passwords instead of plaintext ones.

So far it's been tested only in Windows 7 x6\. with official subversion binaries as of date (1st September 2014).

* * *

## Usage
Simply start svnserve.exe first and then run SVNEncryptedAuth.exe, it will look for svnserve.exe's process ID and attach to it, then it will wait until svnserve.exe is closed/killed/etc and terminates execution.

---

## Important notes

You **_*should*_** change the passwords shown (p$ssw0rd and p$ssw0rdv3ct0r1\. located in these files:

    SVNEncryptedAuthHelper\SVNEncryptedAuthHelper.cs
    SVNEncryptedAuthHelper\Crypto.cs
    SVNEncryptedPasswordGen\Program.cs (optional)

I bet there is room for improvements, but the app works fine and dandy as-is. I've also included a small console app to generate passwords with masking in case you need to tell someone else to input a password by themselves while avoiding giving it to you directly (some workplaces are very restrictive about passwords, even if you trust each other).

I hope you find this useful :) - DARKGuy
