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
using System.Xml;
using Microsoft.AspNetCore.Authorization;

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
                string connectionString = _configuration.GetConnectionString("myconn");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("usp_SinglHTMPageKVal", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@LFID", DocumentID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var resultData = new Dictionary<string, string>(); // Create a list to hold the extracted data
                            string currentKey = null;

                            while (await reader.ReadAsync())
                            {
                                var key = reader["k1"].ToString().Trim();
                                var value = reader["kval1"].ToString().Trim();

                                // Remove unwanted characters from the value
                                value = value.Replace("<br>", "").Replace("\t", "").Replace("\n", "").Replace("</body>", "").Replace(" </body_<_-_-/html>", "");

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
                            // Create a new dictionary for the formatted result
                            var formattedResult = new Dictionary<string, string>();

                            // Add other key-value pairs from the original resultData to the formattedResult
                            foreach (var kvp in resultData)
                            {
                                // Check if the key is "Interest Rate"
                                if (kvp.Key == "Interest Rate")
                                {
                                    // Include the "Interest Rate" value as is
                                    formattedResult[kvp.Key] = kvp.Value;
                                }
                                else if (kvp.Key == "Account Balance as on")
                                {
                                    // Extract the value for "Account Balance as on"
                                    var accountBalanceOnValue = kvp.Value;

                                    // Extract the date and balance information
                                    var parts = accountBalanceOnValue.Split(':');
                                    if (parts.Length == 2)
                                    {
                                        var datePart = parts[0].Trim();
                                        var balancePart = parts[1].Trim();

                                        // Parse and format the date
                                        if (DateTime.TryParse(datePart, out DateTime balanceDate))
                                        {
                                            // Format the date in the desired format (e.g., "01-01-2023")
                                            formattedResult["Account Balance as on"] = balanceDate.ToString("dd-MM-yyyy") + " : " + balancePart;
                                        }
                                        else
                                        {
                                            // If it's not a date, include the value as is
                                            formattedResult["Account Balance as on"] = kvp.Value;
                                        }
                                    }
                                    else
                                    {
                                        // If the format is unexpected, include the value as is
                                        formattedResult["Account Balance as on"] = kvp.Value;
                                    }
                                }
                                else
                                {
                                    // Format the date values for other keys
                                    if (DateTime.TryParse(kvp.Value, out DateTime dateValue))
                                    {
                                        // Format the date in the desired format (e.g., "01-08-2023")
                                        formattedResult[kvp.Key] = dateValue.ToString("dd-MM-yyyy");
                                    }
                                    else
                                    {
                                        // If it's not a date, include the value as is
                                        formattedResult[kvp.Key] = kvp.Value;
                                    }
                                }
                            }

                            // Return the formatted result as JSON
                            return Ok(formattedResult);




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
        // Helper method to convert dates to "DD-MM-YYYY" format
        // Helper method to convert dates to "DD-MM-YYYY" format
        private void ConvertDatesToDDMMYYYY(Dictionary<string, string> dataRow)
        {
            // Specify the input date format
            string[] dateFormats = { "d MMM yyyy" };

            foreach (var kvp in dataRow.ToList())
            {
                if (DateTime.TryParseExact(kvp.Value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    // Convert date to "DD-MM-YYYY" format and update the dictionary
                    dataRow[kvp.Key] = date.ToString("dd-MM-yyyy");
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
                            // result.html_path = Path.Combine("/mnt/Backup/CVision/docubot-api/DocuBot/", result.html_path);
                            result.html_path = Path.Combine("/mnt/Backup/CVision/docubot-api/DocuBot/", result.html_path);

                            try
                            {
                                string connectionString = _configuration.GetConnectionString("myconn");

                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    var indoc = "E:\\docubot\\SBI Demo.html";
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

                                                results.Add(new
                                                {
                                                    Message = "Conversion successful.",
                                                    ApplNo = Applno,
                                                    DocumentId = docid,
                                                    // HtmlFilePath = result.html_path
                                                });
                                            }
                                        }
                                    }
                                }

                                // If the stored procedure did not return a result, return a 404 Not Found

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

        [HttpPost("Extarctxml")]
        public IActionResult ProcessXml(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var xmlContent = reader.ReadToEnd();
                        XmlProcessor processor = new XmlProcessor();
                        XmlProcessorResult result = processor.ProcessXml(xmlContent);

                        return Ok(result);
                    }
                }
                else
                {
                    return BadRequest("No file uploaded.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, log, or throw as needed
                Console.WriteLine($"Error processing XML: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }


        }



        [HttpGet("Generate xml")]
        public IActionResult GenerateXml()
        {
            // Fetch data from your SQL database using Entity Framework or other methods
            List<TransactionModel> transactions = FetchDataFromDatabase();

            // Create XML document
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement root = xmlDoc.CreateElement("Account");

            // Add attributes to the Account element
            root.SetAttribute("linkedAccRef", "8736eaae-3657-4abf-a724-471945866ce9");
            root.SetAttribute("maskedAccNumber", "XXXXXXXX8000");
            root.SetAttribute("xsi:schemaLocation", "http://api.rebit.org.in/FISchema/deposit ../FISchema/deposit.xsd");
            root.SetAttribute("type", "deposit");
            root.SetAttribute("version", "1.1");

            // Add Profile and Summary elements as child elements of Account
            XmlElement profileElement = xmlDoc.CreateElement("Profile");
            XmlElement holdersElement = xmlDoc.CreateElement("Holders");

            // Add attributes to the Holders element
            holdersElement.SetAttribute("type", "SINGLE");

            // Add Holder element as child element of Holders
            XmlElement holderElement = xmlDoc.CreateElement("Holder");
            holderElement.SetAttribute("address", "8/1190, 5th Cross, 3rd Main, 7th Block, Jayanagar, Bangalore - 560011");
            holderElement.SetAttribute("ckycCompliance", "true");
            holderElement.SetAttribute("dob", "1947-08-15");
            holderElement.SetAttribute("email", "akshayku@gmail.com");
            holderElement.SetAttribute("landline", "");
            holderElement.SetAttribute("mobile", "7999517080");
            holderElement.SetAttribute("name", "Akshay Kumar");
            holderElement.SetAttribute("nominee", "REGISTERED");
            holderElement.SetAttribute("pan", "AAAAA0000A");

            holdersElement.AppendChild(holderElement);
            profileElement.AppendChild(holdersElement);

            XmlElement summaryElement = xmlDoc.CreateElement("Summary");

            // Add attributes to the Summary element
            summaryElement.SetAttribute("currentBalance", "187536.52");
            summaryElement.SetAttribute("currency", "INR");
            summaryElement.SetAttribute("branch", "Jayanagar 4th Block");
            summaryElement.SetAttribute("balanceDateTime", "2023-04-03T14:11:49+00:00");
            summaryElement.SetAttribute("currentODLimit", "0");
            summaryElement.SetAttribute("drawingLimit", "0");
            summaryElement.SetAttribute("exchgeRate", " ");
            summaryElement.SetAttribute("facility", "OD");
            summaryElement.SetAttribute("ifscCode", "ICIC0001124");
            summaryElement.SetAttribute("micrCode", "500240246");
            summaryElement.SetAttribute("openingDate", "2004-08-06");
            summaryElement.SetAttribute("status", "ACTIVE");
            summaryElement.SetAttribute("type", "SAVINGS");

            XmlElement pendingElement = xmlDoc.CreateElement("Pending");
            pendingElement.SetAttribute("transactionType", "DEBIT");
            pendingElement.SetAttribute("amount", "0");

            summaryElement.AppendChild(pendingElement);

            root.AppendChild(profileElement);
            root.AppendChild(summaryElement);

            // Create root element
            XmlElement Transactions = xmlDoc.CreateElement("Transactions");

            // Set start and end date attributes
            Transactions.SetAttribute("startDate", "2022-10-02");
            Transactions.SetAttribute("endDate", "2023-03-31");

            root.AppendChild(Transactions);

            // Add each transaction as a child element
            foreach (var transaction in transactions)
            {
                XmlElement transactionElement = xmlDoc.CreateElement("Transaction");

                // Add attributes with null checks and default values
                AddAttribute(xmlDoc, transactionElement, "amount", transaction.Amount, "null");
                AddAttribute(xmlDoc, transactionElement, "currentBalance", transaction.CurrentBalance, "null");
                AddAttribute(xmlDoc, transactionElement, "mode", transaction.Mode, "null");
                AddAttribute(xmlDoc, transactionElement, "narration", transaction.Narration, "null");
                AddAttribute(xmlDoc, transactionElement, "reference", transaction.Reference, "null");
                AddAttribute(xmlDoc, transactionElement, "transactionTimestamp", transaction.TransactionTimestamp.ToString("yyyy-MM-ddTHH:mm:sszzz"), "null");
                AddAttribute(xmlDoc, transactionElement, "txnId", transaction.TxnId, "null");
                AddAttribute(xmlDoc, transactionElement, "type", GetTransactionType(transaction.Amount, transaction.CurrentBalance), "null");
                AddAttribute(xmlDoc, transactionElement, "valueDate", transaction.ValueDate.ToString("yyyy-MM-dd"), "null");


             






                // Assuming you have other properties like mode, type, txnId, you can add them similarly.

                Transactions.AppendChild(transactionElement);
            }

            xmlDoc.AppendChild(root);

            // Save the XML document to a file in the XmlFiles folder
            string xmlFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "XmlFiles");

            if (!Directory.Exists(xmlFolderPath))
            {
                Directory.CreateDirectory(xmlFolderPath);
            }

            string xmlFilePath = Path.Combine(xmlFolderPath, "transactions.xml");
            xmlDoc.Save(xmlFilePath);

            return Ok($"XML file generated successfully. Path: {xmlFilePath}");
        }

        private void AddAttribute(XmlDocument xmlDoc, XmlElement element, string attributeName, string attributeValue, string defaultValue = null)
        {
            element.SetAttribute(attributeName, attributeValue ?? defaultValue);
        }

        private string GetTransactionType(string col5, string col6)
        {
            if (!string.IsNullOrEmpty(col5) && double.TryParse(col5, out double col5Value))
            {
                if (col5Value < 0)
                {
                    return "DEBIT";
                }
                else if (col5Value > 0)
                {
                    return "CREDIT";
                }
            }
            else if (!string.IsNullOrEmpty(col6) && double.TryParse(col6, out double col6Value))
            {
                if (col6Value < 0)
                {
                    return "CREDIT";
                }
                else if (col6Value > 0)
                {
                    return "DEBIT";
                }
            }

            return "UNKNOWN";
        }







        [HttpGet("transactions-xml")]
        public IActionResult GetXmlFile()
        {
            string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "XmlFiles", "transactions.xml");

            if (!System.IO.File.Exists(xmlFilePath))
            {
                return NotFound("XML file not found. Path: " + xmlFilePath);
            }

            var fileBytes = System.IO.File.ReadAllBytes(xmlFilePath);
            return File(fileBytes, "application/xml", "transactions.xml");
        }

        private List<TransactionModel> FetchDataFromDatabase()
        {
            // Fetch only specific columns (col1 to col7) from your SQL database
            string connectionString = _configuration.GetConnectionString("myconn");
            List<TransactionModel> transactions = new List<TransactionModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT [col1], [col2], [col3], [col4], [col5], [col6], [col7] FROM [DEMODOCUBOT].[dbo].[tabledata]";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map columns to your model
                            TransactionModel transaction = new TransactionModel
                            {
                                // Map columns to your model properties
                                TransactionTimestamp = GetDateTime(reader, "col1"),
                                ValueDate = GetDateTime(reader, "col2"),
                                Narration = reader.GetString(reader.GetOrdinal("col3")),
                                Reference = reader.GetString(reader.GetOrdinal("col4")),
                                Amount = GetAmount(reader, "col5", "col6"),
                                CurrentBalance = reader.GetString(reader.GetOrdinal("col7"))
                                // Add other properties if needed
                            };

                            transactions.Add(transaction);
                        }
                    }
                }
            }

            return transactions;
        }


        private DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            int columnIndex = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(columnIndex))
            {
                return DateTime.MinValue; // Or any other default value you prefer for null dates
            }

            string stringValue = reader.GetString(columnIndex);

            if (DateTime.TryParse(stringValue, out DateTime result))
            {
                return result;
            }

            // If parsing fails, you might want to log or handle this case appropriately
            // For now, returning DateTime.MinValue, but you can choose a different default
            return DateTime.MinValue;
        }



        private string GetAmount(SqlDataReader reader, string col5Name, string col6Name)
        {
            int col5Index = reader.GetOrdinal(col5Name);
            int col6Index = reader.GetOrdinal(col6Name);

            // If col5 is null or negative, consider col6 as the amount with a negative sign
            if (reader.IsDBNull(col5Index) || !double.TryParse(reader[col5Index].ToString(), out double col5Value) || col5Value < 0)
            {
                if (double.TryParse(reader[col6Index].ToString(), out double col6Value))
                {
                    return (col6Value).ToString("F2"); // Return with a negative sign
                }
                else
                {
                    return "0.00"; // Default value if parsing fails
                }
            }

            // Otherwise, consider col5 as the amount
            return col5Value.ToString("F2");
        }






    }
}













