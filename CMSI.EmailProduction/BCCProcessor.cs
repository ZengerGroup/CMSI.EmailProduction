using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CMSI.EmailProduction
{
    internal class BCCProcessor
    {
        string BatchPath;
        string BatchID;
        string MJBPath;
        string BccMJBPath;
        public BCCProcessor(string batchPath, string batchID, string filePath) 
        {
            BatchPath = batchPath;
            BatchID = batchID;
            MJBPath = Path.Combine(batchPath, "cmsiAUTO.mjb");
            BccMJBPath = Path.Combine(Configurator.BCCJobDirectory, "cmsiAUTO.mjb");
            MJBCreator Creator = new MJBCreator(MJBPath, String.Format("CMSI {0}_email", BatchID));

            //Options:
            Creator.createList("CMSI AUTO");
            Creator.import(filePath, "CMSI IMPORT");
            Creator.modify("ORIGINAL FIX");
            Creator.modify("FIXC37");
            Creator.modify("Remove Credits Modify");
            Creator.distributionReportExport("Email Batch Final Counts");
            Creator.Export("Export_for_Email_Ltrs", Path.Combine(BatchPath, String.Format("{0}_email.txt", BatchID)));
            Creator.terminate();
            Creator.done();

            File.Copy(MJBPath, BccMJBPath, true);
            ProcessStartInfo bccInfo = new ProcessStartInfo();
            bccInfo.FileName = Configurator.BCCPath;
            bccInfo.Arguments = "-j cmsiAUTO";
            Process BCC = Process.Start(bccInfo);
            BCC.WaitForExit();
        }
    }
}
