using DocuBot_Api.Dbcontext;
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
                                for (int i = 1; i <= 7; i++)
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
                string url = "https://demo.botaiml.com/cnvrt/convert/pdf";

                        try
                        {
                            using var client = new HttpClient
                            {
                                BaseAddress = new Uri(url)
                            };

                            using var content = new MultipartFormDataContent();
                            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

                            var response = await client.PostAsync(url, content);
                            var responseContent = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                // Deserialize the JSON response from the Python API.
                                var result = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
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
    }


}
    




