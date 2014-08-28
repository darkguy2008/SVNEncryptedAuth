using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace SVNEncryptedAuthHelper
{
    public class Main : EasyHook.IEntryPoint
    {
        #region Hooking stuff
        FileInterface Interface;
        LocalHook CreateFileHook;
        Stack<String> Queue = new Stack<String>();

        public Main(RemoteHooking.IContext InContext, String InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<FileInterface>(InChannelName);
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            try
            {
                CreateFileHook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                    new DCreateFile(CreateFile_Hooked),
                    this
                );
                CreateFileHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);
                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());
            RemoteHooking.WakeUpProcess();

            while (true)
                Thread.Sleep(500);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate IntPtr DCreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        // just use a P-Invoke implementation to get native API access from C# (this step is not necessary for C++.NET)
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);
        #endregion

        // Main CreateFile hook where we'll be doing the password file swap and encryption/decryption on-the-fly
        static IntPtr CreateFile_Hooked(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile)
        {
            String newFilename = InFileName;
            if (File.Exists(InFileName))
            {
                FileInfo fi = new FileInfo(InFileName);

                // If the file we're going to access is called passwd (by default, should be able to be configured
                // somewhere in a config file, maybe?).
                if (fi.Name.ToLowerInvariant().Trim() == "passwd")
                {                    
                    // Remove temporary files older than 5 seconds from Windows's temp folder
                    foreach (String f in Directory.GetFiles(Path.GetTempPath(), "*.tmp.svnserve"))
                        if((DateTime.Now - new FileInfo(f).CreationTime).Seconds > 5)
                            File.Delete(f);

                    // Read the passwd file and start writing a temporary one with the same data, except...
                    String tmpFile = Path.GetTempFileName() + ".svnserve";
                    using (StreamReader sr = new StreamReader(fi.FullName))
                    {
                        using (StreamWriter sw = new StreamWriter(tmpFile))
                        {
                            while(!sr.EndOfStream)
                            {
                                String s = sr.ReadLine().Trim();

                                // If the line we're reading has the "user = password" format...
                                if (s.Contains("="))
                                {
                                    // In the temporary file, if the line begins with "key:" (meaning it's encrypted) decrypt
                                    // it to plain text. Else, write it as is (in the case of adding a new user in the passwd file).
                                    String username = s.Substring(0, s.IndexOf('=')).Trim();
                                    String password = s.Substring(s.IndexOf('=') + 1).Trim();
                                    if (password.Contains("key:"))
                                        sw.WriteLine(username + " = " + StringCipher.Decrypt(password.Substring(4), "p4$$w0rd"));
                                    else
                                        sw.WriteLine(username + " = " + password);
                                }
                                else
                                    sw.WriteLine(s);
                            }
                        }
                    }

                    // Now, before returning a handle to the temp file we wrote with the passwords in plain text,
                    // update the current passwd file with the encrypted keys. This is to avoid plain text passwords
                    // to stay in the passwd file in case we added a new user there. Same algorithm as above, except
                    // file names are swapped and instead of decrypting we encript the password if it's not.
                    using (StreamReader sr = new StreamReader(tmpFile))
                    {
                        using (StreamWriter sw = new StreamWriter(fi.FullName))
                        {
                            while (!sr.EndOfStream)
                            {
                                String s = sr.ReadLine().Trim();
                                if (s.Contains("="))
                                {
                                    String username = s.Substring(0, s.IndexOf('=')).Trim();
                                    String password = s.Substring(s.IndexOf('=') + 1).Trim();
                                    if (!password.Contains("key:"))
                                        sw.WriteLine(username + " = key:" + StringCipher.Encrypt(password, "p4$$w0rd"));
                                }
                                else
                                    sw.WriteLine(s);
                            }
                        }
                    }

                    // Give svnserve the filename of the temporary file we wrote with the plaintext 
                    // password, instead of the original filename it's trying to access.
                    newFilename = tmpFile;
                }
            }

            return CreateFile(
                newFilename,
                InDesiredAccess,
                InShareMode,
                InSecurityAttributes,
                InCreationDisposition,
                InFlagsAndAttributes,
                InTemplateFile);
        }
    }
}
