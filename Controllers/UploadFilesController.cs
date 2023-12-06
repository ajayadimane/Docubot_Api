using DocuBot_Api.Models;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Services;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;
using System.Data;

using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using ClosedXML.Excel;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFilesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static readonly string OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "outputfiles");


        public UploadFilesController(IConfiguration configuration)
        {
            _configuration = configuration;



        }



        [HttpPost("ProcessDocument")]
        public async Task<IActionResult> ProcessDocument(string refDoc1)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");
                int returnValue = -1; // Initialize to a default value.

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_IdentifyInDoc", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@refDoc1", refDoc1);

                        // Define the return value parameter
                        var returnParam = new SqlParameter
                        {
                            ParameterName = "@return_value",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnParam);

                        command.ExecuteNonQuery();

                        // Get the return value from the stored procedure
                        returnValue = (int)returnParam.Value;
                    }
                }

                return Ok(new { Message = "Found in Main find", ReturnValue = returnValue, doc = refDoc1 });
                // Return the return value as JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("keyvalue")]
        public async Task<IActionResult> NewDocumentProcessing(int doctype, string indoc)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_SinglHTMPageKVal", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@doctype", doctype);
                        command.Parameters.AddWithValue("@indoc", indoc);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var resultData = new Dictionary<string, string>(); // Create a list to hold the extracted data
                            string currentKey = null;

                            while (await reader.ReadAsync())
                            {
                                var key = reader["k1"].ToString();
                                var value = reader["kval1"].ToString();

                                if (!string.IsNullOrEmpty(key))
                                {
                                    // If a key is present, store it in the dictionary.
                                    resultData[key] = value;
                                    currentKey = key;
                                }
                                else if (!string.IsNullOrEmpty(currentKey))
                                {
                                    // If no key is present, append the value to the previous key's value.
                                    resultData[currentKey] += " " + value;
                                }
                            }
                            ConvertDateFormate(resultData);

                            return Ok(resultData); // Return the extracted data as JSON

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void ConvertDateFormate(Dictionary<string, string> resultData)
        {
            // Specify the input date format
            string[] dateFormats = { "dd MMM yyyy" };

            foreach (var kvp in resultData.ToList())
            {
                if (DateTime.TryParseExact(kvp.Value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    // Convert date to "DD/MM/YYYY" format and update the dictionary
                    resultData[kvp.Key] = date.ToString("dd/MM/yyyy");
                }
            }
        }

        //[HttpPost("ExtractColumns")]
        //public async Task<IActionResult> ExtractColumns(int doctype, string indoc)
        //{
        //    try
        //    {
        //        string connectionString = _configuration.GetConnectionString("myconn");

        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            await connection.OpenAsync();

        //            using (SqlCommand command = new SqlCommand("usp_SinglHTMPageTBL", connection))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@doctype", doctype);
        //                command.Parameters.AddWithValue("@indoc", indoc);

        //                using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //                {
        //                    // Extract the first row as keys
        //                    if (await reader.ReadAsync())
        //                    {
        //                        var keys = new List<string>();
        //                        for (int i = 1; i <= 9; i++)
        //                        {
        //                            keys.Add(reader[$"col{i}"].ToString().Trim());
        //                        }

        //                        var resultData = new List<Dictionary<string, string>>();

        //                        while (await reader.ReadAsync())
        //                        {
        //                            var dataRow = new Dictionary<string, string>();
        //                            for (int i = 1; i <= 7; i++)
        //                            {
        //                                dataRow[keys[i - 1]] = reader[$"col{i}"].ToString().Trim();
        //                            }
        //                            resultData.Add(dataRow);
        //                        }

        //                        return Ok(resultData); // Return the extracted data as JSON with keys from the first row
        //                    }
        //                }
        //            }
        //        }

        //        // If the first row is not found, return an appropriate response.
        //        return NotFound("No data found.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        ///mnt/Backup/CVision/docubot-api/DocuBot/static/results/html/Bank Statement(1).html

        [HttpPost("ExtractColumns")]
        public async Task<IActionResult> ExtractColumns(int doctype, string indoc)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_SinglHTMPageTBL", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@doctype", doctype);
                        command.Parameters.AddWithValue("@indoc", indoc);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Extract the first row as keys
                            if (await reader.ReadAsync())
                            {
                                var keys = new List<string>();
                                for (int i = 1; i <= 9; i++)
                                {
                                    keys.Add(reader[$"col{i}"].ToString().Trim());
                                }

                                var resultData = new List<Dictionary<string, string>>();

                                while (await reader.ReadAsync())
                                {
                                    var dataRow = new Dictionary<string, string>();
                                    for (int i = 1; i <= 7; i++)
                                    {
                                        dataRow[keys[i - 1]] = reader[$"col{i}"].ToString().Trim();
                                    }

                                    // Convert dates to "DD-MM-YYYY" format
                                    ConvertDatesToDDMMYYYY(dataRow);

                                    resultData.Add(dataRow);
                                }

                                return Ok(resultData); // Return the extracted data as JSON with keys from the first row
                            }
                        }
                    }
                }

                // If the first row is not found, return an appropriate response.
                return NotFound("No data found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to convert dates to "DD-MM-YYYY" format
        // Helper method to convert dates to "DD/MM/YYYY" format
        private void ConvertDatesToDDMMYYYY(Dictionary<string, string> dataRow)
        {
            // Specify the input date format
            string[] dateFormats = { "dd MMM yyyy" };

            foreach (var kvp in dataRow.ToList())
            {
                if (DateTime.TryParseExact(kvp.Value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    // Convert date to "DD/MM/YYYY" format and update the dictionary
                    dataRow[kvp.Key] = date.ToString("dd/MM/yyyy");
                }
            }
        }







        //[HttpPost]
        //public IActionResult ConvertToHtml(IFormFile pdfFile)
        //{
        //    try
        //    {
        //        if (pdfFile == null || pdfFile.Length == 0)
        //        {
        //            return BadRequest("No file uploaded.");
        //        }

        //        var pdfFileName = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
        //        var pdfFilePath = Path.Combine(OutputDirectory, pdfFileName);
        //        var htmlFileName = Path.ChangeExtension(pdfFileName, ".html");
        //        var htmlFilePath = Path.Combine(OutputDirectory, htmlFileName);

        //        using (var stream = new FileStream(pdfFilePath, FileMode.Create))
        //        {
        //            pdfFile.CopyTo(stream);
        //        }

        //        // Perform PDF to HTML conversion using PdfSharpCore
        //        using (PdfDocument pdfDocument = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import))
        //        {
        //            pdfDocument.Save(htmlFilePath);
        //        }

        //        return Ok($"PDF converted to HTML and saved: {htmlFilePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Conversion error: {ex.Message}");
        //    }
        //}



        [HttpPost("UploadPdf")]
        public async Task<ActionResult> UploadPdf(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string url = "https://demo.botaiml.com/cnvrt/con  vert/pdf";

                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri(url)
                    };
                    client.Timeout = TimeSpan.FromMinutes(20);



                    using var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {

                        // Deserialize the JSON response from the Python API.
                        var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                        result.html_path = Path.Combine("/mnt/Backup/CVision/docubot-api/DocuBot/", result.html_path);

                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Bad request to Python API");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
            {
                return BadRequest("Invalid PDF file.");
            }
        }

        [HttpPost]
        public IActionResult ConvertPdfToHtml(IFormFile file)
        {
            try
            {
                // Create folders if not exists
                string inputFilesPath = Path.Combine(Environment.CurrentDirectory, "inputfiles");
                string htmlFilesPath = Path.Combine(Environment.CurrentDirectory, "HtmlFiles");

                Directory.CreateDirectory(inputFilesPath);
                Directory.CreateDirectory(htmlFilesPath);

                // Save the file to the inputfiles folder
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string filePath = Path.Combine(inputFilesPath, $"{fileName}.pdf");

                // Clear or replace existing data in inputfiles folder
                ClearOrReplaceFolder(inputFilesPath);

                using (FileStream fileStream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                // Unzip the file if it's a zip file
                if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string extractionPath = Path.Combine(inputFilesPath, fileName);
                    ZipFile.ExtractToDirectory(filePath, extractionPath);
                    filePath = Path.Combine(extractionPath, $"{fileName}.pdf");
                }

                // Clear or replace existing data in HtmlFiles folder
                ClearOrReplaceFolder(htmlFilesPath);

                // Convert PDF to HTML using the ConvertPDFtoHTML method
                ConvertPDFtoHTML(inputFilesPath, htmlFilesPath, filePath);

                // Get the HTML file path
                string htmlFilePath = Path.Combine(htmlFilesPath, $"{Path.GetFileNameWithoutExtension(filePath)}.txt");

                return Ok(new { Message = "Conversion successful.", HtmlFilePath = htmlFilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        private void ConvertPDFtoHTML(string userInputfilesPath, string userHtmlFilePath, string pdfFilePath)
        {
            try
            {
                var configSettingsSection = _configuration.GetSection("ConfigSettings");

                string pdfToHtmlConverterPath = configSettingsSection.GetValue<string>("PDFToHtmlConverter");
                string pdfToHtmlConverterCmd = configSettingsSection.GetValue<string>("PDFToHtmlConverterCmd");
                string inputPath = pdfFilePath;
                string outputPath = Path.Combine(userHtmlFilePath, $"{Path.GetFileNameWithoutExtension(pdfFilePath)}.txt");

                var batchFilePath = Path.Combine(userInputfilesPath, "pdftohtml.bat");
                StreamWriter w = new StreamWriter(batchFilePath);
                w.WriteLine("echo inbatch");
                w.WriteLine(@"""" + pdfToHtmlConverterPath + @"""" + " " + @"""" + inputPath + @"""" + " " + @"""" + outputPath + @""" " + pdfToHtmlConverterCmd);
                w.Close();

                // Execute bat file
                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.RedirectStandardInput = true;
                p1.StartInfo.WorkingDirectory = userInputfilesPath;
                p1.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, batchFilePath);
                p1.Start();
                p1.WaitForExit();
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        private void ClearOrReplaceFolder(string folderPath)
        {
            // Clear or replace existing data in the folder
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            Directory.CreateDirectory(folderPath);
        }


        [HttpPost("UpdateAndRetrieveLoanDetails")]
        public async Task<IActionResult> UpdateAndRetrieveLoanDetails(int appId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_rating", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@appId", appId);

                        // Execute the stored procedure (assumed to perform the update)
                        await command.ExecuteNonQueryAsync();
                    }

                    // Retrieve the updated loan details based on appId
                    using (SqlCommand retrieveCommand = new SqlCommand("SELECT * FROM LoanDetails WHERE AppId = @appId", connection))
                    {
                        retrieveCommand.Parameters.AddWithValue("@appId", appId);

                        using (SqlDataReader reader = await retrieveCommand.ExecuteReaderAsync())
                        {
                            var resultData = new List<Dictionary<string, string>>();

                            while (await reader.ReadAsync())
                            {
                                var dataRow = new Dictionary<string, string>();

                                // Assuming the columns in LoanDetails table are named col1, col2, ..., colN
                                for (int i = 1; i <= reader.FieldCount; i++)
                                {
                                    dataRow[reader.GetName(i - 1)] = reader[i - 1].ToString().Trim();
                                }

                                resultData.Add(dataRow);
                            }

                            return Ok(resultData); // Return the updated loan details as JSON
                        }
                    }
                }

                // If no data is found, return an appropriate response.
                return NotFound("Loan details not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}












