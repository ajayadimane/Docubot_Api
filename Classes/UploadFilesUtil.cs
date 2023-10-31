using System.Diagnostics;

namespace DocuBot_Api.Classes
{
    public class UploadFilesUtil
    {
        public readonly IConfiguration _configuration;
        public UploadFilesUtil(IConfiguration _configuration)
        {
            this._configuration = _configuration;
        }
        public List<string> CreateFolders()
        {
            var configSettingsSection = _configuration.GetSection("ConfigSettings");

            List<string> paths = new();
            #region create inputfile path - user specific
            string userInputfilesPath = configSettingsSection.GetValue<string>("inputfilesPath");
            userInputfilesPath = Path.Combine(Environment.CurrentDirectory, userInputfilesPath);
            if (!Directory.Exists(userInputfilesPath))
                Directory.CreateDirectory(userInputfilesPath);
            else
            {
                string[] fiillees = Directory.GetFiles(userInputfilesPath);
                foreach (string filee in fiillees)
                {
                    File.Delete(filee);
                }
            }
            paths.Add(userInputfilesPath); //first 
            #endregion

            //#region create classify path - user specific
            //string userClassifyPath = configSettingsSection.GetValue<string>("classifyPath");
            //userClassifyPath = Path.Combine(Environment.CurrentDirectory, userClassifyPath, appendUser);
            //if (!Directory.Exists(userClassifyPath))
            //    Directory.CreateDirectory(userClassifyPath);
            //paths.Add(userClassifyPath);
            //#endregion

            //#region create classified path - user specific
            //string userClassifiedPath = configSettingsSection.GetValue<string>("classifiedPath");
            //userClassifiedPath = Path.Combine(Environment.CurrentDirectory, userClassifiedPath, appendUser);
            //if (!Directory.Exists(userClassifiedPath))
            //    Directory.CreateDirectory(userClassifiedPath);
            //paths.Add(userClassifiedPath);
            //#endregion

            #region create textfile path - user specific
            string usertxtfilesPath = configSettingsSection.GetValue<string>("TxtfilePath");
            usertxtfilesPath = Path.Combine(Environment.CurrentDirectory, usertxtfilesPath);
            if (!Directory.Exists(usertxtfilesPath))
                Directory.CreateDirectory(usertxtfilesPath);
            else
            {
                string[] fiillees = Directory.GetFiles(usertxtfilesPath);
                foreach (string filee in fiillees)
                {
                    System.IO.File.Delete(filee);
                }
            }
            paths.Add(usertxtfilesPath); //second
            #endregion

            #region create DispTxtfilePath path - user specific
            string userdisplySplttxtfilesPath = configSettingsSection.GetValue<string>("DispTxtfilePath");
            userdisplySplttxtfilesPath = Path.Combine(Environment.CurrentDirectory, userdisplySplttxtfilesPath);
            if (!Directory.Exists(userdisplySplttxtfilesPath))
                Directory.CreateDirectory(userdisplySplttxtfilesPath);
            else
            {
                string[] fiillees = Directory.GetFiles(userdisplySplttxtfilesPath);
                foreach (string filee in fiillees)
                {
                    File.Delete(filee);
                }
            }
            paths.Add(userdisplySplttxtfilesPath); //third
            #endregion

            return paths;
        }
        public void ConvertPDFtoTXT(string userInputfilesPath, string userTxtFilePath)
        {
            #region convert PDF TO TXT FILES
            //creation and Execution of batch file
            try
            {
                var configSettingsSection = _configuration.GetSection("ConfigSettings");

                string txtconverterpath = configSettingsSection.GetValue<string>("TXTConverter");
                string txtconverterCmd = configSettingsSection.GetValue<string>("TXTConverterCmd");
                string inputpath = userInputfilesPath + "\\*.pdf";
                string outputpath = userTxtFilePath;

                var myBatFilePath = Path.Combine(userInputfilesPath, "pdftotxt.bat");
                StreamWriter w = new StreamWriter(myBatFilePath);
                w.WriteLine("echo inbatch");
                w.WriteLine(@"""" + txtconverterpath + @"""" + " " + @"""" + inputpath + @"""" + " " + @"""" + outputpath + @""" " + txtconverterCmd);
                //w.WriteLine(@"""" + txtconverterpath + @"""" + " " + @"""" + inputpath + @"""" + " " + @"""" + outputpath + @""" - c txt");
                w.Close();
                /*pdf to text pilot
                w.WriteLine("echo inbatch");
                w.WriteLine("dir /b *.pdf > list.txt");
                w.WriteLine(@"""" + txtconverterpath + @"""" + "/convert list.txt  /saveto " + @"""" + userTxtFilePath + @"""");
                w.Close();
                /**/

                //execute bat file
                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;

                p1.StartInfo.RedirectStandardInput = true;
                p1.StartInfo.WorkingDirectory = userInputfilesPath;
                p1.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, userInputfilesPath, "pdftotxt.bat");
                p1.Start();
                p1.WaitForExit();
            }
            catch (Exception ec) { }
            //creation and Execution of batch file
            #endregion
        }
        public void ConvertPDFtoSplitText(string userInputfilesPath, string userClassifyPath)
        {
            #region convert PDF TO SPLIT FILES
            //creation and Execution of batch file
            try
            {
                var configSettingsSection = _configuration.GetSection("ConfigSettings");
                string txtconverterpath = configSettingsSection.GetValue<string>("SplitTxtConverter");

                string inputpath = userInputfilesPath + "\\*.pdf";
                string outputpath = userClassifyPath;

                var myBatFilePath = Path.Combine(userInputfilesPath, "pdftosplittxt.bat");
                StreamWriter w = new StreamWriter(myBatFilePath);
                w.WriteLine("echo inbatch");
                w.WriteLine(@"""" + txtconverterpath + @"""" + " " + @"""" + inputpath + @"""" + " " + @"""" + outputpath + @""" - c txt -s");
                w.Close();


                //execute bat file
                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;

                p1.StartInfo.RedirectStandardInput = true;
                p1.StartInfo.WorkingDirectory = userInputfilesPath;
                p1.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, userInputfilesPath, "pdftosplittxt.bat");
                p1.Start();
                p1.WaitForExit();
            }
            catch (Exception ec) { }
            //creation and Execution of batch file
            #endregion
        }
    }
}

