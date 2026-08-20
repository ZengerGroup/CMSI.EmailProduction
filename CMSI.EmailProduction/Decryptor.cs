using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CMSI.EmailProduction
{
    internal class Decryptor
    {
        public string OutputPath;
        public string BatchPath;
        public string BatchId;
        public bool Success;
        public Decryptor(string filePath)
        {
            BatchId = String.Format("{0}", DateTime.Now.ToString("MMddyy_HHmm"));
            BatchPath = GetBatchPath(filePath);
            OutputPath = Path.Combine(BatchPath, Path.GetFileNameWithoutExtension(filePath));
            Success = true;
            try
            {
                RunGPG(filePath);
                Success = true;
            }
            catch (Exception e)
            {
                Logger.Display(e.Message, false);
                Success = false;
            }
        }
        private void RunGPG(string filePath)
        {
            Logger.Display("Running GPG.", false);
            Process gpg = new Process();
            ProcessStartInfo gpgInfo = new ProcessStartInfo();
            gpgInfo.FileName = Configurator.GPGPath;
            gpgInfo.RedirectStandardInput = true;
            string arguments = String.Format("--ignore-mdc-error --passphrase-fd 0 -o {0} -d {1}", OutputPath, filePath);
            Logger.Display(arguments, false);
            gpgInfo.Arguments = arguments;
            gpgInfo.UseShellExecute = false;
            gpg.StartInfo = gpgInfo;
            gpg.Start();
            using (StreamWriter sw = gpg.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(Configurator.Secret);
                    Logger.Display("Writing to file.", false);
                }
            }
            gpg.WaitForExit();
        }
        private string GetBatchPath(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Logger.Display("File name: {0}", false, fileName);
            Logger.Display("Batch ID: {0}", false, BatchId);
            string dirPath = Path.Combine(Configurator.BatchDirectory, BatchId);
            Directory.CreateDirectory(dirPath);
            return dirPath;
        }
    }
}
