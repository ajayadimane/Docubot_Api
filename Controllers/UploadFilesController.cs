using DocuBot_Api.Dbcontext;
using DocuBot_Api.Models;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Services;
using System.Data;
using System.IO.Compression;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFilesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
      

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

                return Ok( new { Message = "Found in Main find", ReturnValue = returnValue, doc = refDoc1 });
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

                    using (SqlCommand command = new SqlCommand("usp_keyvaluedetails", connection))
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

                    using (SqlCommand command = new SqlCommand("usp_descriptiondetails", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@doctype", doctype);
                        command.Parameters.AddWithValue("@indoc", indoc);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var resultData = new List<Dictionary<string, string>>(); // Create a list to hold the extracted data

                            while (await reader.ReadAsync())
                            {
                                var dataRow = new Dictionary<string, string>();
                                for (int i = 1; i <= 5; i++) // Extract col1 to col5
                                {
                                    dataRow[$"col{i}"] = reader[$"col{i}"].ToString();
                                }
                                resultData.Add(dataRow);
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


    }

}

