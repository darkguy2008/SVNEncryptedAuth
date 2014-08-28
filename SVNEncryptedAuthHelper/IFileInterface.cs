using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVNEncryptedAuthHelper
{
    public class FileInterface : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("Encrypted SVN Authentication installed on PID {0}.\r\n", InClientPID);
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + InInfo.ToString());
        }
    }
}
