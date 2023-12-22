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
using DocumentFormat.OpenXml.Office2010.Excel;
using DocuBot_Api.Models.RatingEngine_Models;
using DocumentFormat.OpenXml.Office2010.Word;
using Azure.Core;

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
        public async Task<IActionResult> NewDocumentProcessing(ExtractionRequest request)
        {
            try
            {
                int DocumentID = request.DocumentId;

                IActionResult getDocNameResult = await GetDocNameById(DocumentID);

                if (getDocNameResult is OkObjectResult okResult && okResult.Value is string docname)
                {
                    // Now, use the retrieved docname as indoc in the stored procedure
                    int doctype = 2; // Set doctype as 2


                    string connectionString = _configuration.GetConnectionString("myconn");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (SqlCommand command = new SqlCommand("usp_SinglHTMPageKVal", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@doctype", doctype);
                            command.Parameters.AddWithValue("@indoc", docname);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                var resultData = new Dictionary<string, string>(); // Create a list to hold the extracted data
                                string currentKey = null;

                                while (await reader.ReadAsync())
                                {
                                    var key = reader["k1"].ToString().Trim();
                                    var value = reader["kval1"].ToString().Trim();

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

                                // Create a new dictionary for the formatted result
                                var formattedResult = new Dictionary<string, string>();

                                // Add other key-value pairs from the original resultData to the formattedResult
                                foreach (var kvp in resultData)
                                {
                                    // Skip the "Account Balance as on" key, as it has already been processed
                                    if (kvp.Key != "Account Balance as on")
                                    {
                                        formattedResult[kvp.Key] = kvp.Value;
                                    }
                                }

                                // Check if "Account Balance as on" key is present in the resultData
                                if (resultData.ContainsKey("Account Balance as on"))
                                {
                                    // Extract the value for "Account Balance as on "
                                    var accountBalanceOnKey = "Account Balance as on";
                                    var accountBalanceOnValue = resultData[accountBalanceOnKey];

                                    // Split the value into two parts based on the ':' separator
                                    var parts = accountBalanceOnValue.Split(':');
                                    if (parts.Length == 2)
                                    {
                                        // Trim and format the parts
                                        var datePart = parts[0].Trim();
                                        var balancePart = parts[1].Trim();

                                        // Add the formatted parts to the new dictionary
                                        formattedResult["Account Balance Date"] = datePart;
                                        formattedResult["Account Balance"] = balancePart;
                                    }
                                }
                                ConvertDatesToDDMMYYYY(formattedResult);
                                // Return the formatted result as JSON
                                return Ok(formattedResult);
                            }
                        }
                    }
                }


                else
                {
                    return NotFound("No data found.");
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
        public async Task<IActionResult> ExtractColumns(ExtractionRequest request)
        {
            int DocumentID = request.DocumentId;
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_SinglHTMPageTBL", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@lfid", DocumentID);


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
                return Ok("data not found");
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


        [HttpGet("GetDocNameById/{DocumentId}")]
        public async Task<IActionResult> GetDocNameById(int DocumentId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("SELECT [docname] FROM [dbo].[LoadedFiles] WHERE [id] = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", DocumentId);

                        object result = await command.ExecuteScalarAsync();

                        if (result != null)
                        {
                            string docname = result.ToString();
                            return Ok(docname);
                        }
                        else
                        {
                            return NotFound($"No document found for the provided id: {DocumentId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private string GenerateFilePath(string docname, int doctypeid, string purpose)
        {
            // Customize this function to generate the desired file path based on input parameters
            // For example, you can concatenate docname, doctypeid, and purpose in the path
            string fileName = $"{docname}_type{doctypeid}_purpose_{purpose}.html";

            // Assuming you want to save in a folder named "GeneratedFiles" within the current working directory
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedFiles", fileName);

            return filePath;
        }


        [HttpPost("UploadDocument")]
        public async Task<ActionResult> UploadPdf(IFormFileCollection files, string Applno)
        {
            if (files != null && files.Count > 0)
            {
                try
                {
                    // Validate the loanaccno parameter as needed

                    // Get the working directory of the application
                    string workingDirectory = Directory.GetCurrentDirectory();

                    // Create a folder for the loanaccno in the UploadedFiles directory
                    string loanaccnoFolder = Path.Combine(workingDirectory, "UploadedFiles", Applno);
                    Directory.CreateDirectory(loanaccnoFolder);

                    // List to store results for each file
                    var results = new List<object>();

                    // Iterate through each uploaded file
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        // Define input file name with loanaccno and index
                        string inputFileName = $"{Applno}_file{i + 1}{Path.GetExtension(file.FileName)}";

                        // Save the input file in the loanaccno folder
                        string inputFilepath = Path.Combine(loanaccnoFolder, inputFileName);
                        using (var inputStream = new FileStream(inputFilepath, FileMode.Create))
                        {
                            await file.CopyToAsync(inputStream);
                        }

                        // Define the API endpoint URL
                        string apiUrl = "https://demo.botaiml.com/cnvrt/convert/pdf";

                        using var client = new HttpClient
                        {
                            BaseAddress = new Uri(apiUrl)
                        };
                        client.Timeout = TimeSpan.FromMinutes(20);

                        using var content = new MultipartFormDataContent();
                        content.Add(new StreamContent(file.OpenReadStream()), "file", inputFileName);
                        content.Add(new StringContent(Applno), "loanaccno");

                        var response = await client.PostAsync(apiUrl, content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            // Deserialize the JSON response from the Python API
                            var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                            result.html_path = Path.Combine("/mnt/Backup/CVision/docubot-api/DocuBot/", result.html_path);

                            try
                            {
                                string connectionString = _configuration.GetConnectionString("myconn");

                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    await connection.OpenAsync();

                                    using (SqlCommand command = new SqlCommand("usp_uploadDoc", connection))
                                    {
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.Parameters.AddWithValue("@loanid", Applno);
                                        command.Parameters.AddWithValue("@indoc", result.html_path);

                                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                                        {
                                            if (await reader.ReadAsync())
                                            {
                                                // Retrieve the docid from the result
                                                int docid = reader.GetInt32(reader.GetOrdinal("lfid"));

                                                // Other code to extract additional information if needed

                                                return Ok(new
                                                {
                                                    Message = "Conversion successful.",
                                                    ApplNo = Applno,
                                                    DocumentId = docid,
                                                    HtmlFilePath = result.html_path
                                                });
                                            }
                                        }
                                    }
                                }

                                // If the stored procedure did not return a result, return a 404 Not Found
                                return NotFound("No data found for the provided parameters.");
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, ex.Message);
                            }

                            // Add the result to the list
                            //results.Add(new
                            //{
                            //    Message = "Conversion successful.",
                            //    LoanaccNo = loanaccno,
                            //    Lfid = storedProcedureResult.Lfid,
                            //    HtmlFilePath = result.html_path
                            //});
                        }
                        else
                        {
                            // Add an error result to the list
                            results.Add(new
                            {
                                Message = "Error in conversion.",
                                ApplNo = Applno
                                // Add other error details as needed
                            });
                        }
                    }

                    // Return the results as JSON
                    return Ok(results);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
            {
                return BadRequest("No files uploaded.");
            }
        }


      




        //[HttpPost("UploadPdf")]
        //public async Task<ActionResult> UploadPdf(IFormFile file)
        //{
        //    if (file != null && file.Length > 0)
        //    {
        //        string url = "https://demo.botaiml.com/cnvrt/convert/pdf";

        //        try
        //        {
        //            using var client = new HttpClient
        //            {
        //                BaseAddress = new Uri(url)
        //            };
        //            client.Timeout = TimeSpan.FromMinutes(20);



        //            using var content = new MultipartFormDataContent();
        //            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

        //            var response = await client.PostAsync(url, content);
        //            var responseContent = await response.Content.ReadAsStringAsync();


        //            if (response.IsSuccessStatusCode)
        //            {

        //                // Deserialize the JSON response from the Python API.
        //                var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        //                result.html_path = Path.Combine("/mnt/Backup/CVision/docubot-api/DocuBot/", result.html_path);

        //                return Ok(result);
        //            }
        //            else
        //            {
        //                return BadRequest("Bad request to Python API");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest("Invalid PDF file.");
        //    }
        //}

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


    }
}












