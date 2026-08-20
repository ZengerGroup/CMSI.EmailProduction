using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace CMSI.EmailProduction
{
    internal class Transporter
    {
        TcpListener TcpServer;
        TcpClient Client;
        IPAddress IP;
        NetworkStream NetStream;

        int PortNumber;
        byte[] StreamBuffer;
        bool ReadingFileStream;
        string IncomingFileName;
        string BatchDirectory;
        string BatchId;
        int BytesRead;
        int DataLength;
        string OutputPath;
        //Constructor
        public Transporter(string ipAddress, string port)
        {
            if (!Int32.TryParse(port, out PortNumber)) Logger.ErrorExit(["Configuration error, port number is invalid."], 101);
            if (!IPAddress.TryParse(ipAddress, out IP)) Logger.ErrorExit(["Configuration error, ip address is invalid."], 102);
            TcpServer = new TcpListener(IP, PortNumber);
            StreamBuffer = new byte[1024];
            ReadingFileStream = false;
            Logger.WriteLog("TCP listener has been configured successfully.", false);
        }
        //Start listener
        public void StartListener()
        {
            while (true)
            {
                TcpServer.Start();
                Logger.Display("Awaiting Connection", false);
                AcceptConnection();
                if (ProcessFile())
                {
                    ReturnProcessedFile();
                    CleanUp();
                }
                else 
                {
                    Logger.Display("Failed to decrypt data file - {0}.", false, OutputPath);
                }
                ReadingFileStream = false;
            }
        }
        private void AcceptConnection()
        {
            try
            {
                Client = TcpServer.AcceptTcpClient();
                NetStream = Client.GetStream();
                Logger.Display("Connection successful.", false);
                ReadStream();
            }
            catch (Exception e)
            {
                Logger.Display("Connection interrupted, socket exception: {0}.", false, e.Message);
            }
        }
        //Receive file
        private void ReadStream()
        {
            Logger.Display("Reading data stream.", false);
            OutputPath = GenerateOutputPath();
            int i;
            DataLength = -1;
            try
            {
                while ((i = NetStream.Read(StreamBuffer, 0, StreamBuffer.Length)) != 0)
                {
                    //if (!ReadingFileStream) ReadFileName(Encoding.UTF8.GetString(StreamBuffer).Trim());
                    //else 
                    ReadFileStream(StreamBuffer);
                    if (DataLength > 0 && BytesRead >= DataLength) break;
                }
            }
            catch
            {
                Logger.Display("Connection interrupted.", false);
            }
        }
        private string GenerateOutputPath()
        {
            string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return Path.Combine(Configurator.TempPath, String.Format("{0}_cms_email.txt.pgp", new string(Random.Shared.GetItems(allowedChars.ToCharArray(), 6))));
        }
        private async void ReadFileStream(byte[] buffer)
        {
            Logger.Display("Reading File data...", false);
            if (DataLength < 0)
            {
                if (ReadFirstBuffer(buffer).Result) SendResponse("OK!");
                else SendResponse("BAD");
            } else
            {
                if (ReadSubsequentBuffer(buffer).Result) SendResponse("OK!");
                else SendResponse("BAD");
            }
                
        }
        private async Task<bool> ReadFirstBuffer(byte[] buffer)
        {
            Logger.Display("First buffer...", false);
            try
            {
                DataLength = BitConverter.ToInt32(buffer[0..4]);
                AppendAllBytes(OutputPath, buffer[4..buffer.Length]);
                BytesRead = 1024;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> ReadSubsequentBuffer(byte[] buffer)
        {
            try
            {
                AppendAllBytes(OutputPath, buffer);
                BytesRead += buffer.Length;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void SendResponse(string statusString)
        {
            try
            {
                Console.WriteLine("Sending response: {0}", statusString);
                byte[] statusBytes = Encoding.UTF8.GetBytes(statusString);
                NetStream.Write(statusBytes, 0, statusBytes.Length);
                NetStream.Flush();
            }
            catch
            {
                Console.WriteLine("Unable to send response along connection.");
            }
            
        }
        static bool AppendAllBytes(string path, byte[] bytes)
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Append))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        private bool ProcessFile()
        {
            Logger.Display("Processing file.", false);
            try
            {
                Logger.Display("Create Decryptor object.", false);
                Decryptor FileDecryptor = new Decryptor(OutputPath);
                BatchDirectory = FileDecryptor.BatchPath;
                BatchId = FileDecryptor.BatchId;
                if (FileDecryptor.Success)
                {
                    BCCProcessor Processor = new BCCProcessor(FileDecryptor.BatchPath, FileDecryptor.BatchId, FileDecryptor.OutputPath);
                    return true;
                }
                else return false;

            }
            catch (Exception e)
            {
                Logger.Display(e.Message, false);
                return false;
            }
        }
        //Return file
        private void ReturnProcessedFile()
        {
            Logger.Display("Preparing zip file.", false);
            string tempPath = Path.Combine(Configurator.TempPath, BatchId);
            string zipPath = String.Format("{0}_email.zip",tempPath);
            if (!GetReportFile()) Logger.WriteLog("Failed to move report file.", false);
            try
            {
                ZipFile.CreateFromDirectory(BatchDirectory, zipPath);
            }
            catch (Exception e)
            {
                Logger.Display("Failed to create zip file.", false);
                Logger.WriteLog(e.Message, false);
            }
            if (SendZip(zipPath).Result) Logger.Display("Processing complete!", false);
            else Logger.Display("Error sending file data!", false);
        }
        private async Task<bool> SendZip(string zipPath)
        {
            Logger.Display("Sending zip file.", false);
            byte[] fileData = File.ReadAllBytes(zipPath);
            byte[] dataLength = BitConverter.GetBytes(fileData.Length + 4);
            byte [] dataPackage = new byte[4 + fileData.Length];
            dataLength.CopyTo(dataPackage, 0);
            fileData.CopyTo(dataPackage, 4);
            int dataSent = 0;
            while (dataSent < dataPackage.Length)
            {
                SendBufferToStream(dataSent, dataPackage);
                if (!GetStatus()) 
                {
                    Logger.WriteLog("Error processing {0}.", false, zipPath);
                    return false;
                } 
                dataSent += 1024;
            }
            return true;
        }
        public async void SendBufferToStream(int dataSent, byte[] dataPackage)
        {
            int bufferSize = ((dataPackage.Length - dataSent) > 1024) ? 1024 : dataPackage.Length - dataSent;
            byte[] buffer = new byte[bufferSize];
            dataPackage[dataSent..(dataSent + bufferSize)].CopyTo(buffer, 0);
            NetStream.Write(buffer, 0, buffer.Length);
        }
        private bool GetStatus()
        {
            try
            {
                byte[] status = new byte[3];
                NetStream.Read(status, 0, 3);
                NetStream.Flush();
                if (Encoding.UTF8.GetString(status).Contains("OK!")) return true;
                else return false;
            }
            catch { return false; }
            
        }
        private bool GetReportFile()
        {
            try
            {
                string[] reportFiles = Directory.GetFiles(Configurator.ReportPath);
                for (int i = 0; i < reportFiles.Length; i++) if (reportFiles[i].Contains("email"))
                    {
                        File.Move(reportFiles[i], Path.Combine(BatchDirectory, Path.GetFileName(reportFiles[i]).Replace("email_", BatchId)));
                        break;
                    }
                return true;
            }
            catch { return false; }
        }
        //Clean up temp files.
        private void CleanUp()
        {
            string[] batchFolders = Directory.GetDirectories(Configurator.BatchDirectory);
            for (int i = 0; i < batchFolders.Length; i++) Directory.Delete(batchFolders[i], true);
            string[] tempFiles = Directory.GetFiles(Configurator.TempPath);
            for (int i = 0; i < tempFiles.Length; i++) File.Delete(tempFiles[i]);
        }
        //Error catch and restart.
        private void Reset()
        {
            //nothing yet yo!
        }
    }
}
