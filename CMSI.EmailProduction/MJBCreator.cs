using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSI.EmailProduction
{
    internal class MJBCreator
    {
        private string mjbFilename;
        private string listname;
        private StreamWriter mailmanScript;
        private int processCount;

        public MJBCreator(string mjbFilename, string listname)
        {
            this.mjbFilename = mjbFilename;
            this.listname = listname;
            this.mailmanScript = new StreamWriter(mjbFilename, false);
            this.processCount = 1;
        }
        public void createList(string listTemplate)
        {
            mailmanScript.WriteLine("[NEWLISTTEMPLATE-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("FILENAME=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("DESCRIPTION=" + '"' + listname + '"');
            mailmanScript.WriteLine("SETTINGS=" + '"' + listTemplate + '"');
            mailmanScript.WriteLine("USEINDEXES = Y");
            mailmanScript.WriteLine("OVERWRITE = Y");
            processCount++;
        }

        public void import(string importFilename, string settings)
        {
            mailmanScript.WriteLine("[IMPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS =" + '"' + settings + '"');
            mailmanScript.WriteLine("FILENAME= " + '"' + importFilename + '"');
            mailmanScript.WriteLine("PARALLEL = N");
            mailmanScript.WriteLine("WAIT=DEFAULT");
            mailmanScript.WriteLine("STARTTIME=DEFAULT");
            mailmanScript.WriteLine("SUPPRESSERRORS = N");
            processCount++;
        }
        public void presort(string presortSettings, string select, string type)
        {
            mailmanScript.WriteLine("[PRESORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS=" + '"' + presortSettings + '"');
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("ADDRESSGROUP=MAIN");
            mailmanScript.WriteLine("SELECTIVITY= \"" + select + '"');
            mailmanScript.WriteLine("PARALLEL=N");
            mailmanScript.WriteLine("WAIT=DEFAULT");
            mailmanScript.WriteLine("STARTTIME=DEFAULT");
            mailmanScript.WriteLine("SUPPRESSERRORS=N");
            processCount++;
        }
        public void presortselectexp(string presortSettings, string select, string type)
        {
            mailmanScript.WriteLine("[PRESORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS=" + '"' + presortSettings + '"');
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("ADDRESSGROUP=MAIN");
            mailmanScript.WriteLine("SELECTIVITYEXPRESSION= " + select);
            mailmanScript.WriteLine("PARALLEL=N");
            mailmanScript.WriteLine("WAIT=DEFAULT");
            mailmanScript.WriteLine("STARTTIME=DEFAULT");
            mailmanScript.WriteLine("SUPPRESSERRORS=N");
            processCount++;
        }
        public void postageStatement(int copies, string postage, string type)
        {
            mailmanScript.WriteLine("[POSTAGESTATEMENT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("COPIES=" + copies);
            mailmanScript.WriteLine("PRINTER=" + '"' + Configurator.PrinterDuplex + '"');
            mailmanScript.WriteLine("COMMENTS=" + '"' + listname + type + "" + '"');

            switch (postage)
            {
                case "METER":
                    mailmanScript.WriteLine("AFFIXED=Y");
                    mailmanScript.WriteLine("METERED=Y");
                    mailmanScript.WriteLine("AFFIXEDAT=LOWEST");
                    break;
                case "PERMIT":
                    break;
            }
            mailmanScript.WriteLine("STREAMLIST=AUTO;MACH;SINGLE_PC;AUTO/NONAUTO");
            mailmanScript.WriteLine("COLLATE=Y");
            mailmanScript.WriteLine("HIGHQUALITY=Y");
            mailmanScript.WriteLine("USEAGENTPHONE=Y");
            mailmanScript.WriteLine("MOVEUPDATENCOA=Y");
            mailmanScript.WriteLine("MAILINGDATE=NONE");
            processCount++;
        }

        public void postageStatementPdf(int copies, string postage, string type)
        {
            mailmanScript.WriteLine("[POSTAGESTATEMENT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("COPIES=" + copies);
            mailmanScript.WriteLine(String.Format("Printer=\"{0}\"", Configurator.PdfPrinter));
            mailmanScript.WriteLine("COMMENTS=" + '"' + listname + type + "" + '"');

            switch (postage)
            {
                case "METER":
                    mailmanScript.WriteLine("AFFIXED=Y");
                    mailmanScript.WriteLine("METERED=Y");
                    mailmanScript.WriteLine("AFFIXEDAT=LOWEST");
                    mailmanScript.WriteLine("PERMITADDITIONAL=Y");
                    break;
                case "PERMIT":
                    mailmanScript.WriteLine("PERMITADDITIONAL=Y");
                    break;
            }
            mailmanScript.WriteLine("STREAMLIST=AUTO;MACH;SINGLE_PC;AUTO/NONAUTO");
            mailmanScript.WriteLine("COLLATE=Y");
            mailmanScript.WriteLine("HIGHQUALITY=Y");
            mailmanScript.WriteLine("USEAGENTPHONE=Y");
            mailmanScript.WriteLine("MOVEUPDATENCOA=Y");
            mailmanScript.WriteLine("MAILINGDATE=NONE");
            processCount++;
        }

        public void qualificationReport(string type)
        {
            mailmanScript.WriteLine("[QUALIFICATIONREPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("STREAMLIST=AUTO;MACH;SINGLE_PC;AUTO/NONAUTO");
            mailmanScript.WriteLine("ABSOLUTECONTAINERNUMBERS=Y");
            mailmanScript.WriteLine(String.Format("Printer=\"{0}\"", Configurator.PrinterDuplex));
            processCount++;

        }
        
        public void outputFile(string outputFileName, string labelSettings, string type)
        {
            mailmanScript.WriteLine("[PRESORTEDLABELS-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("PRESORTNAME=" + '"' + listname + " " + type + '"');
            mailmanScript.WriteLine("SETTINGS=" + labelSettings);
            mailmanScript.WriteLine("STREAMLIST=AUTO;MACH;SINGLE_PC;AUTO/NONAUTO");
            mailmanScript.WriteLine("ABSOLUTECONTAINERNUMBERS=Y");
            mailmanScript.WriteLine("OVERWRITE=Y");
            mailmanScript.WriteLine("FILENAME=" + '"' + outputFileName + '"');
            processCount++;

        }
        public void distributionReportPrint(string settings)
        {
            mailmanScript.WriteLine("[DISTRIBUTIONREPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine(String.Format("Printer=\"{0}\"", Configurator.PrinterSimplex));
            mailmanScript.WriteLine("SETTINGS=" + settings);
            processCount++;
        }
        public void userDefinedreport(string settings, string select)
        {
            mailmanScript.WriteLine("[USERDEFINEDREPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine(String.Format("Printer=\"{0}\"", Configurator.PrinterSimplex));
            mailmanScript.WriteLine("SETTINGS=" + settings);
            mailmanScript.WriteLine("SELECTIVITYEXPRESSION= " + select);
            processCount++;
        }
        public void terminate()
        {
            mailmanScript.WriteLine("[TERMINATE-" + processCount + "]");
            mailmanScript.Close();
        }
        public void modify(string settings)
        {
            mailmanScript.WriteLine("[MODIFY-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS=" + settings);
            processCount++;
        }
        public void distributionReportExport(string settings)
        {
            mailmanScript.WriteLine("[DISTRIBUTIONREPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS=" + settings);
            mailmanScript.WriteLine("OVERWRITE=Y");
            processCount++;
        }
        public void hideRecords(string select)
        {
            mailmanScript.WriteLine("[HIDE-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SELECTIVITYEXPRESSION = " + select);
            processCount++;
        }
        public void hideSelect(string select)
        {
            mailmanScript.WriteLine("[HIDE-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SELECTIVITY = " + select);
            mailmanScript.WriteLine("SUPPRESSERRORS = Y");
            processCount++;
        }
        public void Export(string settings, string filePath)
        {
            mailmanScript.WriteLine("[EXPORT-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            mailmanScript.WriteLine("SETTINGS=" + settings);
            mailmanScript.WriteLine(String.Format("FILENAME=\"{0}\"", filePath));
            mailmanScript.WriteLine("OVERWRITE=Y");
            processCount++;
        }
        public void encode()
        {
            mailmanScript.WriteLine("[ENCODE-" + processCount + "]");
            mailmanScript.WriteLine(String.Format("List=\"{0}.dbf\"", Path.Combine(Configurator.BCCListDirectory, listname)));
            processCount++;
        }

        public void done()
        {
            mailmanScript.Close();
        }
    }
}
