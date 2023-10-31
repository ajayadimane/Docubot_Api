namespace DocuBot_Api.Classes
{
    public class Logs
    {
        public static void WriteLog(string username, string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = new DirectoryInfo(Environment.CurrentDirectory).FullName;
            logFilePath = Path.Combine(logFilePath, "Logs\\");
            logFilePath = logFilePath + "Log-" + System.Environment.MachineName + username + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(strLog);
            log.Close();
        }

    }
}

