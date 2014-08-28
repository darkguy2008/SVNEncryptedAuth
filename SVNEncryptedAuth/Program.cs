using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using EasyHook;
using SVNEncryptedAuthHelper;
using System.Linq;
using System.Threading;

namespace SVNEncryptedAuth
{
    class Program
    {
        static String IPCChannelName = null;
        static void Main(string[] args)
        {
            Int32 TargetPID = 0;
            Process[] localByName = Process.GetProcessesByName("svnserve");
            if (localByName.Length > 0)
                TargetPID = localByName[0].Id;

            RemoteHooking.IpcCreateServer<FileInterface>(ref IPCChannelName, WellKnownObjectMode.SingleCall);
            RemoteHooking.Inject(
                TargetPID, 
                "SVNEncryptedAuthHelper.dll", 
                "SVNEncryptedAuthHelper.dll", 
                IPCChannelName
            );

            while (ProcessExists(TargetPID))
                Thread.Sleep(100);
        }

        private static bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }
    }

}
