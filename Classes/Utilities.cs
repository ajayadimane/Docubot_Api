using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using DocuBot_Api.Models;
using ClosedXML.Excel;

namespace DocuBot_Api.Classes
{
    public class Utilities
    {

        public readonly IConfiguration _configuration;

        public Utilities(IConfiguration configuration)
        {
           _configuration = configuration;
        }

     

        //public IHttpActionResult GetFile(string fileName)
        //{
        //    List<string> reqfiles = Directory.GetFiles("").ToList();
        //    List<string> fndFile = reqfiles.Where(m => m.Contains(fileName)).ToList();

        //    HttpResponseMessage responseMessage = new HttpResponseMessage();
        //    var result = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
        //    {
        //        Content = new System.Net.Http.ByteArrayContent(System.IO.File.ReadAllBytes(filepath))
        //    };


        //    result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        //    return responseMessage;
        //}



        public static void CreateFile(List<PageProcess> lines, string file, string[] splitfiles)
        {
            string dpath = Path.GetDirectoryName(file);
            string flname = Path.GetFileNameWithoutExtension(file);
            List<int> docIDs = lines.Select(m => m.Docid).Distinct().ToList();
            int fileConcat = 1;
            foreach (int docid in docIDs)
            {

                string newflname = flname + "_" + fileConcat + ".txt";
                string fullpath = Path.Combine(dpath, newflname);
                List<string> lins = lines.Where(m => m.Docid == docid).Select(m => m.Linedump).ToList();

                using (StreamWriter sw = File.CreateText(fullpath))
                {
                    foreach (string ln in lins)
                    {
                        sw.WriteLine(ln);
                    }

                }
                fileConcat++;
            }

            int maxPg = lines.Select(m => m.Docndpg).Max();
            List<int> pgends = lines.OrderBy(m => m.Docndpg).Select(m => m.Docndpg).Distinct().ToList();
            List<string> mtchdFiles = new List<string>();
            foreach (string fm in splitfiles)
            {
                string name = Path.GetFileNameWithoutExtension(fm);
                if (name.Substring(0, name.LastIndexOf(".p")) == flname)
                {
                    mtchdFiles.Add(fm);
                }
            }
            int n = 1; int k = 1;
            foreach (int pgend in pgends)
            {

                for (int i = 1; i <= maxPg; i++)
                {
                    string pgno = i.ToString();
                    if (i > pgend)
                        break;
                    if (maxPg > 9 && i < 10)
                        pgno = string.Concat("0", i.ToString());
                    string found = mtchdFiles.Find(m => (Path.GetFileNameWithoutExtension(m).Substring(Path.GetFileNameWithoutExtension(m).LastIndexOf(".p") + 1).Replace("page", "")) == pgno);
                    if (found != null)
                    {
                        string newname = Path.GetFileNameWithoutExtension(found) + "_" + n;
                        string dirname = Path.GetDirectoryName(found);
                        newname = Path.Combine(dirname, newname + ".txt");
                        System.IO.File.Move(found, newname);
                        mtchdFiles.Remove(found);
                    }
                    else
                    {
                        pgno = pgno.Replace("0", "");
                        found = mtchdFiles.Find(m => (Path.GetFileNameWithoutExtension(m).Substring(Path.GetFileNameWithoutExtension(m).LastIndexOf(".p") + 1).Replace("page", "")) == pgno);
                        if (found != null)
                        {
                            string newname = Path.GetFileNameWithoutExtension(found) + "_" + n;
                            string dirname = Path.GetDirectoryName(found);
                            newname = Path.Combine(dirname, newname + ".txt");
                            System.IO.File.Move(found, newname);
                            mtchdFiles.Remove(found);
                        }
                    }

                }
                n++;
            }


        }

        public void ConvertPDFtoCSV(string appenduser, string userClassifyPath)
        {
            #region convert PDF TO CSV FILES
            //creation and Execution of batch file
            try
            {
                var configSettingsSection = _configuration.GetSection("ConfigSettings");
                string userInputfilesPath = configSettingsSection.GetValue<string>("inputfilesPath");
                userInputfilesPath = Path.Combine(Environment.CurrentDirectory, userInputfilesPath, appenduser);
                string csvconverterpath = configSettingsSection.GetValue<string>("CSVConverter");
                string inputpath = userInputfilesPath + "\\*.pdf";
                string outputpath = userClassifyPath;

                var myBatFilePath = Path.Combine(userInputfilesPath, "pdftocsv.bat");
                StreamWriter w = new StreamWriter(myBatFilePath);
                w.WriteLine("echo inbatch");
                w.WriteLine(@"""" + csvconverterpath + @"""" + " " + @"""" + inputpath + @"""" + " " + @"""" + outputpath + @""" - c csv");
                w.Close();


                //execute bat file
                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;

                p1.StartInfo.RedirectStandardInput = true;
                p1.StartInfo.WorkingDirectory = userInputfilesPath;
                p1.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, userInputfilesPath, "pdftocsv.bat");
                p1.Start();
                p1.WaitForExit();
            }
            catch (Exception ec) { }
            //creation and Execution of batch file
            #endregion
        }

        /// <summary>
        /// Move the file to corresponding folder and structure out the data to insert into required tables
        /// </summary>
        /// <param name="folder"></param> folder path
        /// <param name="fileName"></param> file name
        /// <param name="file"></param> file path
        /// <param name="dId"></param> dictionary to hold unique folder name as key and list of files in it (max 2) for training data
        /// <param name="docustrt"></param>
        /// <param name="docuend"></param>
        /// <param name="newDoctypeID"></param> new doctypeID
        /// <param name="mtchdWrdsList"></param> list of matched words which contains initial lines from file
        /// <param name="mtchdWrds"></param> matched common words (if only 1 file, then words from file)
        /// <param name="fileNameList"></param>
        public void MoveFileAndGetStructureData(string folder, string fileName, string file, Dictionary<int, List<string>> dId, List<string> docustrt, List<string> docuend,
                                    int newDoctypeID, List<string> mtchdWrdsList, string mtchdWrds, List<string> fileNameList)
        {
            string newpath = Path.Combine(folder, fileName);
            int fld = Convert.ToInt32(Path.GetFileName(folder));
            try { File.Move(file, newpath); } catch { }

            if (!dId.ContainsKey(fld))
            {
                GetDocStrtDocEnd(newpath, docustrt, docuend);
                List<string> common2files = new List<string>();
                common2files.Add(newpath);
                dId.Add(newDoctypeID, common2files);
                mtchdWrdsList.Add(mtchdWrds); fileNameList.Add(fileName);
            }
            else
            {
                int index = dId.Keys.ToList().IndexOf(fld);
                mtchdWrdsList.RemoveAt(index); //modify the list with actual commmon words
                mtchdWrdsList.Insert(index, mtchdWrds);
                List<string> ind = dId.GetValueOrDefault(fld);
                if (ind.Count() < 21)
                {
                    ind.Add(newpath);
                    dId[fld] = ind;
                }
            }
        }
        public void SortFoldersByCreationTime(DirectoryInfo[] dis)
        {
            Array.Sort(dis, delegate (DirectoryInfo fi1, DirectoryInfo fi2) { return fi1.CreationTime.CompareTo(fi2.CreationTime); });
        }
        public static void GetDocStrtDocEnd(string file, List<string> docustrt, List<string> docuend)
        {
            try
            {
                string[] docdetails = File.ReadAllLines(file).Where(ln => ln.Trim() != string.Empty && !ln.ToLower().Contains("page")).ToArray();
                string[] docstrt = docdetails.FirstOrDefault().Split("  ", StringSplitOptions.RemoveEmptyEntries);
                string docstrtval = docstrt.FirstOrDefault().Trim();
                string[] docend = docdetails.LastOrDefault().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string docendval = docend.LastOrDefault().Trim();
                docustrt.Add(docstrtval); docuend.Add(docendval);
            }
            catch
            {
                docustrt.Add(" "); docuend.Add(" ");
            }

        }
        public static DataTable DetailsObjToDataTable(List<DataValues> objects)
        {
            DataTable dt = new(); int maxRowCnt = objects.Select(m => m.Values).Select(m => m.Count).Max();
            try
            {
                foreach (var colname in objects)
                    dt.Columns.Add(colname.ReturnKey);

                //foreach (var obj in objects)
                {
                    //DataColumn dc = new(obj.ReturnKey);
                    //dt.Columns.Add(dc);
                    for (int i = 0; i < maxRowCnt; i++)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (var coll in dt.Columns)
                        {
                            try { dr[coll.ToString()] = objects.Where(m => m.ReturnKey == coll.ToString()).FirstOrDefault().Values[i]; } catch { }
                        }
                        dt.Rows.Add(dr);
                    }
                    //foreach (var val in obj.Values)
                    //{
                    //    DataRow dr = dt.NewRow();             
                    //    dr[obj.ReturnKey] = val;
                    //    dt.Rows.Add(dr);
                    //}
                    //dt.Columns.Add(dc);
                }
                dt.AcceptChanges();
            }
            catch (Exception ec)
            {

            }

            return dt;
        }


        public UserLoginDetails GetUserDetailsFrmTkn(JwtSecurityToken jwtToken)
        {
            UserLoginDetails loginDetails = new();
            loginDetails.UserId = Convert.ToInt32(jwtToken.Claims.First(x => x.Type == "UserId").Value);
            loginDetails.UserName = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
            loginDetails.UserRole = jwtToken.Claims.First(x => x.Type == "role").Value;
            return loginDetails;
        }
        public static List<string> RemoveUnwantedLines(List<string> ignoreLinesTb, List<string> newlines)
        {
            try
            {
                //remove all unwanted lines from file
                for (int k = 0; k < ignoreLinesTb.Count(); k++)
                {
                    for (int i = 0; i < newlines.Count(); i++)
                    {
                        try
                        {
                            if (newlines[i].ToLower().Replace("'", "").Replace(" ", string.Empty).Contains(ignoreLinesTb[k].ToLower().Replace("'", "").Replace(" ", "")))
                            {
                                newlines[i] = newlines[i].Replace("'", "").Replace(ignoreLinesTb[k], "");
                                break;
                            }

                        }
                        catch (Exception ec) { Logs.WriteLog("username", ec.Message.ToString()); }
                    }
                }
                newlines = newlines.Where(ln => ln.Trim() != string.Empty).Select(ln => ln).ToList();
            }
            catch (Exception ec)
            {
                Logs.WriteLog("username", ec.Message);
            }
            return newlines;
        }

        public static DataTable MasterObjToDataTable(List<MasterTable> objects)
        {
            DataTable dt = new DataTable();

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(MasterTable));
            foreach (PropertyDescriptor p in props)
                dt.Columns.Add(p.Name, p.PropertyType);
            foreach (Object obj in objects)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyInfo info in typeof(MasterTable).GetProperties())
                {
                    dr[info.Name] = info.GetValue(obj, null);
                }
                dt.Rows.Add(dr);
            }
            dt.AcceptChanges();
            return dt;
        }

        public static DataTable TempMasterObjToDataTable(List<AutoTrainFileTbl> objects)
        {
            DataTable dt = new();

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(AutoTrainFileTbl));
            try
            {
                foreach (PropertyDescriptor p in props)
                    dt.Columns.Add(p.Name, p.PropertyType);
                foreach (Object obj in objects)
                {
                    DataRow dr = dt.NewRow();
                    foreach (PropertyInfo info in typeof(AutoTrainFileTbl).GetProperties())
                    {
                        dr[info.Name] = info.GetValue(obj, null);
                    }
                    dt.Rows.Add(dr);
                }
                dt.AcceptChanges();
            }
            catch (Exception ec)
            {

            }

            return dt;
        }



        public static Dictionary<string, byte[]> Convertfiletobytes(string[] filenames)
        {
            // List<byte[]> bytess = new List<byte[]>();

            //foreach(string file in filenames)
            //{
            //    using (FileStream fs = new FileStream(file,FileMode.Open, FileAccess.Read))
            //    {
            //        // Create a byte array of file stream length
            //        byte[] bytes = System.IO.File.ReadAllBytes(file);
            //        //Read block of bytes from stream into the byte array
            //        fs.Read(bytes, 0, System.Convert.ToInt32(fs.Length));
            //        //Close the File Stream
            //        fs.Close();
            //        bytess.Add(bytes); //return the byte data
            //    }
            //}

            Dictionary<string, byte[]> filebytes = new Dictionary<string, byte[]>();

            foreach (string file in filenames)
            {
                using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
                {
                    // Create a byte array of file stream length
                    byte[] bytes = System.IO.File.ReadAllBytes(file);
                    //Read block of bytes from stream into the byte array
                    fs.Read(bytes, 0, System.Convert.ToInt32(fs.Length));
                    //Close the File Stream
                    fs.Close();
                    filebytes.Add(Path.GetFileName(file), bytes); //return the byte data
                }
            }
            return filebytes;
        }

        //public bool ValidateDates(ReportDateRange reportDate)
        //{
        //    string[] dateTypes = { "MM/dd/yyyy", "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy","dd-MMM-yyyy","MMM-dd-yyyy" };
        //    //bool chFrmValid = DateTime.TryParseExact(reportDate.FromDate, dateTypes, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime d1);
        //    bool chToValid = DateTime.TryParseExact(reportDate.ToDate, dateTypes, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d2);
        //    //return chFrmValid && chToValid;
        //    return true;
        //}



        public static List<DataTable> ExtractTblData(List<string> lines, List<MDTablesMod> tblObj)
        {
            int doctypeid = tblObj[0].DocTypeID;
            List<DataTable> tables = new(); List<int> headRwCnt = new();
            List<string> tblEnd = tblObj.Where(m => !string.IsNullOrWhiteSpace(m.TableEnd)).Select(m => m.TableEnd).ToList();

            List<int> distTblcnt = tblObj.Select(m => m.TableCount).Distinct().ToList();
            List<string> distTblHdr = new();
            try
            {
                for (int m = 0; m < distTblcnt.Count; m++)
                {
                    distTblHdr = new List<string>(); headRwCnt = new List<int>();
                    for (int j = 0; j < tblObj.Count; j++)
                    {
                        if (tblObj[j].TableCount == distTblcnt[m])
                        {
                            distTblHdr.Add(tblObj[j].Header);
                            headRwCnt.Add(tblObj[j].Hdrextlinecnt);
                        }
                        else
                            continue;
                    }
                    DataTable dt = new();
                    foreach (string head in distTblHdr.Skip(1))
                    {
                        string hd = head.Trim();
                        try
                        {
                            dt.Columns.Add(hd);
                        }
                        catch //columns with same header value, to avoid conflit error
                        {
                            hd = " " + hd;
                            dt.Columns.Add(hd);
                        }

                    }
                    List<string> tblLns = new(); int columnsCnt = distTblHdr.Count;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string ln = lines[i]; int cntcmp = 2;
                        if (distTblHdr.Count() > 3) cntcmp = 3;
                        if (distTblHdr.Count() == 2) cntcmp = 1;
                        List<string> fndlns = distTblHdr.FindAll(x => ln.ToLower().Contains(x.ToLower())).Where(m => m.Trim() != string.Empty).Distinct().ToList();
                        int cmp = 5; if (distTblHdr.Distinct().Count() == 2) cmp = 1;
                        if (fndlns.Count <= 1 && distTblHdr.Count > cmp)
                        {
                            List<string> newhrds = new();
                            foreach (string head in distTblHdr)
                            {
                                newhrds.AddRange(head.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries));
                            }
                            fndlns = newhrds.FindAll(x => ln.ToLower().Contains(x.ToLower())).Where(m => m.Trim() != string.Empty).Distinct().ToList();
                        }
                        if (fndlns.Count > 0 && fndlns.Count >= cntcmp)
                        {
                            tblLns.Add(ln);
                            lines = lines.Skip(i).ToList();
                            break;
                        }
                    }
                    if (tblLns.Count == 0 && headRwCnt[0] > 0)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            string ln = lines[i]; int cntcmp = 0;
                            if (distTblHdr.Count() > 3) cntcmp = 1;

                            List<string> fndlns = distTblHdr.FindAll(x => ln.ToLower().Contains(x.ToLower())).Where(m => m.Trim() != string.Empty).Distinct().ToList();
                            if (fndlns.Count > cntcmp)
                            {
                                tblLns.Add(ln);
                                lines = lines.Skip(i).ToList();
                                break;
                            }
                        }
                    }
                    if (tblLns.Count > 0)
                    {
                        int n = headRwCnt[0];
                        lines = lines.Skip(n + 1).ToList();
                        foreach (string ln in lines)
                        {
                            string[] tblends = tblEnd[m].Split('|');
                            if (tblends.Count() == 1)
                            {
                                if (Regex.Replace(ln, @"\W", "").ToLower().Contains(Regex.Replace(tblEnd[m], @"\W", "").ToLower())) //ln.ToLower().Replace(" ", "").Replace("-", "").Contains(tblEnd[m].Replace(" ", "").Replace("-", "").Trim().ToLower()))
                                    break;
                                else
                                    tblLns.Add(ln);
                            }
                            else
                            {
                                if (Regex.Replace(ln, @"\W", "").ToLower().Contains(Regex.Replace(tblends[0], @"\W", "").ToLower())
                                    || Regex.Replace(ln, @"\W", "").ToLower().Contains(Regex.Replace(tblends[1], @"\W", "").ToLower())) //ln.ToLower().Replace(" ", "").Replace("-", "").Contains(tblEnd[m].Replace(" ", "").Replace("-", "").Trim().ToLower()))
                                    break;
                                else
                                    tblLns.Add(ln);
                            }

                        }
                        tblLns = tblLns.Where(ln => ln.Trim() != string.Empty).ToList();
                        int tempCol = 0; int tempRow = 0; List<int> skplines = new List<int>();
                        dt.Rows.Add();
                        for (int k = 1; k < tblLns.Count(); k++)
                        {
                            if (skplines.Contains(k)) continue;
                            string lin = tblLns[k];
                            string prevLine = string.Empty;
                            if (k > 1)
                                prevLine = tblLns[k - 1];
                            string nxtLine = string.Empty;
                            if (k + 1 < tblLns.Count())
                                nxtLine = tblLns[k + 1];

                            string prevLineData = string.Empty; string nxtLineData = string.Empty;
                            for (int i = 1; i < distTblHdr.Count; i++)
                            {
                                if (lin.Trim() == string.Empty) break;
                                string col = string.Empty;
                                try
                                {
                                    col = lin.Substring(tblObj[i].DataStart, tblObj[i].DataEnd - tblObj[i].DataStart);
                                    //if (prevLine != string.Empty)
                                    //    prevLineData = prevLine.Substring(tblObj[i].DATAST, tblObj[i].DATAND - tblObj[i].DATAST);
                                    //if (prevLineData == string.Empty)

                                }
                                catch
                                {
                                    if (tblObj[i].DataStart > lin.Length)
                                        col = string.Empty;
                                    else
                                        col = lin[tblObj[i].DataStart..];
                                }
                                if (doctypeid == 86 && i == 5 && m == 6)
                                {
                                    if (nxtLine != string.Empty)
                                    {
                                        try { nxtLineData = nxtLine[tblObj[i].DataStart..tblObj[i].DataEnd]; }
                                        catch
                                        {
                                            if (tblObj[i].DataStart > nxtLine.Length)
                                                nxtLineData = string.Empty;
                                            else
                                                nxtLineData = nxtLine[tblObj[i].DataStart..].Trim();
                                        }
                                    }
                                    if (prevLine != string.Empty)
                                    {
                                        try { prevLineData = prevLine[tblObj[i].DataStart..tblObj[i].DataEnd]; }
                                        catch
                                        {
                                            if (tblObj[i].DataStart > prevLine.Length)
                                                prevLineData = string.Empty;
                                            else
                                                prevLineData = prevLine[tblObj[i].DataStart..].Trim();
                                        }
                                    }
                                }
                                dt.Rows[tempRow][tempCol] += col.Trim() + " ";
                                tempCol++;
                            }
                            try
                            {
                                if (doctypeid == 86 && m == 6)
                                {
                                    if ((prevLineData != string.Empty && nxtLineData != string.Empty) || (prevLineData != string.Empty && nxtLineData == string.Empty))
                                    { tempRow++; dt.Rows.Add(); }
                                }

                                else if (k + 1 < tblLns.Count && (tblLns[k + 1][tblObj[1].DataStart..tblObj[1].DataEnd].Trim() != string.Empty || tblLns[k + 1][tblObj[2].DataStart..tblObj[2].DataEnd].Trim() != string.Empty)) //temporary purpose
                                {
                                    tempRow++; dt.Rows.Add();
                                }
                            }
                            catch { tempRow++; dt.Rows.Add(); }


                            tempCol = 0;
                        }
                        tblObj = tblObj.Skip(columnsCnt).ToList(); //removes extracted table's meta data
                        //lines = lines.Except(tblLns).ToList();
                        DataTable crrctTable = dt.Copy();

                        //Commented out for new type (temporary comment)
                        //List<DataRow> del = new List<DataRow>();
                        //int cmp = 3; int r = 0;
                        //if (dt.Columns.Count >= 12) cmp = 5;
                        //else if (dt.Columns.Count >= 7) cmp = 4;
                        //if (dt.Rows.Count > 1)
                        //{
                        //    foreach (DataRow dtt in dt.AsEnumerable())
                        //    {
                        //        if (dtt.ItemArray.All(x => string.IsNullOrWhiteSpace(x.ToString()))) { continue; }
                        //        int skp = 0;
                        //        foreach (string dct in dtt.ItemArray)
                        //        {
                        //            //if (dct.Trim() == string.Empty)
                        //            //{
                        //            //    skp++;
                        //            //}
                        //            if (dtt.ItemArray[0].ToString() == string.Empty)
                        //            {
                        //                skp++;
                        //            }

                        //        }
                        //        if (skp >= cmp)
                        //            del.Add(dtt);
                        //    }
                        //}
                        //for (int p = 0; p < dt.Rows.Count; p++)
                        //{
                        //    if (!del.Contains(dt.Rows[p]))
                        //    {
                        //        crrctTable.ImportRow(dt.Rows[p]);
                        //        r++;
                        //    }
                        //    else
                        //    {
                        //        try
                        //        {

                        //            for (int q = 0; q < dt.Columns.Count; q++)// to combine multiline data
                        //            {
                        //                if (dt.Rows[p][q].ToString() != string.Empty)
                        //                {
                        //                    if (del.Count > 0)
                        //                    {
                        //                        foreach (string val in del.FirstOrDefault().ItemArray)
                        //                        {
                        //                            if (val != string.Empty)
                        //                            {
                        //                                if (r == 0)
                        //                                {
                        //                                    crrctTable.ImportRow(dt.Rows[p]);
                        //                                    r++;
                        //                                }
                        //                                else
                        //                                {
                        //                                    crrctTable.Rows[r - 1][q] += " " + val.Trim();
                        //                                    q++;
                        //                                }

                        //                            }

                        //                        }
                        //                        del.RemoveAt(0);
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        catch { crrctTable = dt.Copy(); }
                        //    }
                        //}
                        tables.Add(crrctTable);
                    }
                }
            }
            catch (Exception ec) { Logs.WriteLog("username", ec.Message); }
            return tables;
        }



        public string DataTableToJsonObj(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Merge(dt);
            StringBuilder JsonString = new StringBuilder();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    JsonString.Append("{");
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        if (j < ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\",");
                        }
                        else if (j == ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == ds.Tables[0].Rows.Count - 1)
                    {
                        JsonString.Append("}");
                    }
                    else
                    {
                        JsonString.Append("},");
                    }
                }
                JsonString.Append("]");
                return JsonString.ToString();
            }
            else
            {
                return null;
            }
        }

        public string GetZipFolderPath(string appendUser)
        {
            var configSettingsSection = _configuration.GetSection("ConfigSettings");
            string zipFldrPath = configSettingsSection.GetValue<string>("ZipFolderPath");
            string dwnloadFilepath = configSettingsSection.GetValue<string>("DownloadFilePath");
            zipFldrPath = Path.Combine(Environment.CurrentDirectory, dwnloadFilepath, appendUser, zipFldrPath);
            //zipFldrPath = Path.Combine(Environment.CurrentDirectory, zipFldrPath);
            if (File.Exists(zipFldrPath))
            {
                File.Delete(zipFldrPath);
            }
            return zipFldrPath;
        }
        public string CreateDownloadPath(string appendUser)
        {
            var configSettingsSection = _configuration.GetSection("ConfigSettings");
            string dwnloadFilepath = configSettingsSection.GetValue<string>("DownloadFilePath");
            dwnloadFilepath = Path.Combine(Environment.CurrentDirectory, dwnloadFilepath, appendUser, "ExtractedFiles");
            if (!Directory.Exists(dwnloadFilepath))
                Directory.CreateDirectory(dwnloadFilepath);
            else     //clear the folder
            {
                string[] fiillees = Directory.GetFiles(dwnloadFilepath);
                foreach (string filee in fiillees)
                {
                    File.Delete(filee);
                }
            }
            return dwnloadFilepath;
        }

        public static void ExportToRequiredFormat(DataTable dataKeyVal, string path, string fileName)
        {
            try
            {
                DataTable tb = dataKeyVal.Copy();
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(fileName);
                worksheet.Cell(1, 1).InsertTable(tb);
                workbook.SaveAs(Path.Combine(path, fileName + ".xlsx"));
            }
            catch (Exception ec)
            {
                Logs.WriteLog("username", ec.Message);
            }
        }
        public static void ExportToCSVFormat(DataTable dataKeyVal, string path, string fileName)
        {
            try
            {
                StringBuilder fileContent = new StringBuilder();

                foreach (var col in dataKeyVal.Columns)
                {
                    fileContent.Append(col.ToString() + ",");
                }

                fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

                foreach (DataRow dr in dataKeyVal.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        fileContent.Append("\"" + column.ToString() + "\",");
                    }

                    fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);
                }

                System.IO.File.WriteAllText(Path.Combine(path, fileName + ".csv"), fileContent.ToString());
            }
            catch (Exception ec)
            {
                Logs.WriteLog("username", ec.Message);
            }
        }
    }
}
