using DocuBot_Api.Context;
using DocuBot_Api.Models.Doqbot_Models;
using DocuBot_Api.Models.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoqBotController : ControllerBase
    {

        private readonly string _connectionString;
        private readonly string _procedureName = "usp_autoTrain";
        private readonly DocubotDbContext _context;
        private readonly KvalRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DoqBotController> _logger;


        public DoqBotController(IConfiguration configuration, DocubotDbContext docubotDbContext, KvalRepository kvalRepository, ILogger<DoqBotController> logger)
        {
            _connectionString = configuration.GetConnectionString("myconn");
            _context = docubotDbContext;
            _repository = kvalRepository;
            _configuration = configuration;
            _logger = logger;

        }


        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadFiles([FromForm] FileUploadRequest request)
        //{
        //    if (request.Files == null || request.Files.Count == 0)
        //        return BadRequest("No files were provided.");

        //    // Check and create the Input directory if it doesn't exist
        //    string inputFolderPath = "C:\\Input";
        //    if (!Directory.Exists(inputFolderPath))
        //    {
        //        Directory.CreateDirectory(inputFolderPath);
        //    }

        //    // Save each file in the Input directory
        //    foreach (var file in request.Files)
        //    {
        //        string filePath = Path.Combine(inputFolderPath, file.FileName);
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }
        //    }

        //    // Paths to the batch files
        //    string batFilePath1 = @"c:/input/p.bat";
        //    string batFilePath2 = @"c:/input/tot.bat";

        //    // Call the first batch file
        //    var result1 = await ExecuteBatchFileAsync(batFilePath1);
        //    if (!result1.Success)
        //    {
        //        return StatusCode(500, new { Message = "Error executing first batch file.", Error = result1.Error });
        //    }

        //    // Call the second batch file
        //    var result2 = await ExecuteBatchFileAsync(batFilePath2);
        //    if (!result2.Success)
        //    {
        //        return StatusCode(500, new { Message = "Error executing second batch file.", Error = result2.Error });
        //    }

        //    return Ok(new { Message = "Files uploaded and batch files executed successfully." });
        //}

        //private async Task<(bool Success, string Error)> ExecuteBatchFileAsync(string batFilePath)
        //{
        //    ProcessStartInfo processInfo = new ProcessStartInfo(batFilePath)
        //    {
        //        UseShellExecute = false,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        CreateNoWindow = true,
        //        WorkingDirectory = workingDirectory,
        //    };

        //    try
        //    {
        //        using (Process process = Process.Start(processInfo))
        //        {
        //            await process.WaitForExitAsync();
        //            string error = await process.StandardError.ReadToEndAsync();

        //            if (process.ExitCode != 0)
        //            {
        //                return (false, error);
        //            }
        //        }

        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, ex.Message);
        //    }
        //}


        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteStoredProcedure()
        {

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(_procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Map the strongly-typed properties to the stored procedure parameters
                        command.Parameters.AddWithValue("@path", "c:\\inbox\\");
                        command.Parameters.AddWithValue("@usr", "ajayy");

                        await connection.OpenAsync();

                        // Execute the command and read the result
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var resultList = new List<object>();

                            while (await reader.ReadAsync())
                            {
                                var result = new
                                {
                                    ljid = reader.GetInt32(reader.GetOrdinal("ljid")),
                                    usrname = reader.GetString(reader.GetOrdinal("usrname")),
                                    processed = reader.GetInt32(reader.GetOrdinal("processed")),
                                    untrained = reader.GetInt32(reader.GetOrdinal("untrained")),
                                    error = reader.GetInt32(reader.GetOrdinal("error")),
                                    docname = reader.GetString(reader.GetOrdinal("docname")),
                                    status = reader.GetInt32(reader.GetOrdinal("status")),
                                    lfid = reader.GetInt32(reader.GetOrdinal("lfid"))
                                };
                                resultList.Add(result);
                            }

                            return Ok(resultList);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Message = "Error executing stored procedure.", Error = ex.Message });
            }
        }

        [HttpGet("lfid/{lfid}")]
        public async Task<IActionResult> GetByLfid(int lfid)
        {
            var results = await _repository.GetKvalDetailsByLfidAsync(lfid);

            if (results == null || results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);
        }

        [HttpGet("tabledata/{lfid}")]
        public async Task<IActionResult> GetTableDataByLfid(int lfid)
        {
            // Call the updated repository method
            var result = await _repository.GetCleanedTableDataByLfidAsync(lfid);

            // Check if the result is null or empty and return appropriate response
            if (result == null || result.Count == 0)
            {
                return NotFound();
            }

            // Return the result as JSON
            return Ok(result);
        }

        [HttpGet("details/{ljid}")]
        public async Task<IActionResult> GetDocumentDetailsByLJID(int ljid)
        {
            try
            {
                var result = await _repository.GetDocumentDetailsByLJIDAsync(ljid);
                return Ok(result); // Returns the JSON response
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 Internal Server Error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] FileUploadRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
                return BadRequest("No files were provided.");

            // Check and create the Input directory if it doesn't exist
            string inputFolderPath = "C:\\Input";
            if (!Directory.Exists(inputFolderPath))
            {
                Directory.CreateDirectory(inputFolderPath);
            }

            // Save each file in the Input directory
            foreach (var file in request.Files)
            {
                string filePath = Path.Combine(inputFolderPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            // Call the first batch file
            //var result1 = await ExecuteBatchFileAsync(@"E:\Input\p.bat");
            //if (!result1.Success)
            //{
            //    return StatusCode(500, new { Message = "Error executing p batch file.", Error = result1.Error });
            //}

            // Call the second batch file
            var result2 = await ExecuteBatchFileAsync(@"C:\Input\tot.bat");
            if (!result2.Success)
            {
                return StatusCode(500, new { Message = "Error executing tot batch file.", Error = result2.Error });
            }

            // Call the stored procedure and get the results
            var procedureResult = await ExecuteStoredProcedures();
            if (procedureResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            // Cast the result to OkObjectResult to access the result value
            var okResult = procedureResult as OkObjectResult;
            if (okResult != null)
            {
                // Access the result value directly and return it in your response
                return Ok(new
                {
                    Message = "Files uploaded and batch files executed successfully.",
                    StoredProcedureResult = okResult.Value
                });
            }

            return StatusCode(500, "Unexpected error occurred.");
        }

        private async Task<IActionResult> ExecuteStoredProcedures()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(_procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Map the strongly-typed properties to the stored procedure parameters
                        command.Parameters.AddWithValue("@path", "c:\\inbox\\");
                        command.Parameters.AddWithValue("@usr", "test");

                        await connection.OpenAsync();

                        // Execute the command and read the result
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var resultList = new List<object>();

                            while (await reader.ReadAsync())
                            {
                                var result = new
                                {
                                    ljid = reader.GetInt32(reader.GetOrdinal("ljid")),
                                    usrname = reader.GetString(reader.GetOrdinal("usrname")),
                                    processed = reader.GetInt32(reader.GetOrdinal("processed")),
                                    untrained = reader.GetInt32(reader.GetOrdinal("untrained")),
                                    error = reader.GetInt32(reader.GetOrdinal("error")),
                                    docname = reader.GetString(reader.GetOrdinal("docname")),
                                    status = reader.GetInt32(reader.GetOrdinal("status")),
                                    lfid = reader.GetInt32(reader.GetOrdinal("lfid"))
                                };
                                resultList.Add(result);
                            }

                            return Ok(resultList);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Message = "Error executing stored procedure.", Error = ex.Message });
            }
        }


        private async Task<(bool Success, string Error, string Output)> ExecuteBatchFileAsync(string batFilePath)
        {
            var workingDirectory = Path.GetDirectoryName(batFilePath);

            //if (!File.Exists(batFilePath))
            //{
            //    return (false, $"Batch file not found at path: {batFilePath}", null);
            //}

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = batFilePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            try
            {
                using (Process process = Process.Start(processInfo))
                {
                    bool exited = await Task.Run(() => process.WaitForExit(120000)); // 30 seconds
                    if (!exited)
                    {
                        return (false, "Batch file execution timed out.", null);
                    }

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode != 0)
                    {
                        return (false, string.IsNullOrWhiteSpace(error) ? "Unknown error occurred." : error, output);
                    }

                    return (true, null, output);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }


        [HttpGet("run")]
        public IActionResult RunBatchFile()
        {
            string batchFilePath = _configuration["BatchFilePath"];
            _logger.LogInformation($"Attempting to run batch file at: {batchFilePath}");

            if (string.IsNullOrEmpty(batchFilePath) || !System.IO.File.Exists(batchFilePath))
            {
                _logger.LogError($"Batch file not found at path: {batchFilePath}");
                return NotFound($"Batch file not found at path: {batchFilePath}");
            }

            try
            {
                _logger.LogInformation("File exists, attempting to create process");
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batchFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(batchFilePath)
                };

                process.StartInfo = startInfo;
                _logger.LogInformation("Starting process");
                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                _logger.LogInformation($"Process completed. Exit code: {process.ExitCode}");
                _logger.LogInformation($"Output: {output}");

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError($"Error output: {error}");
                }

                if (process.ExitCode == 0)
                {
                    return Ok(output);
                }
                else
                {
                    return BadRequest($"Batch file execution failed. Exit code: {process.ExitCode}. Error: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the batch file");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutKval(int id, [FromBody] Kval kval)
        {
            if (id != kval.Id)
            {
                return BadRequest();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"UPDATE [dbo].[kval]
                              SET 
                                  [k1] = @K1,
                                  [kval1] = @Kval1              
                              WHERE [id] = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = kval.Id });
                    command.Parameters.Add(new SqlParameter("@Lfid", SqlDbType.Int) { Value = kval.Lfid });
                    command.Parameters.Add(new SqlParameter("@Linedump", SqlDbType.NVarChar) { Value = kval.Linedump });
                    command.Parameters.Add(new SqlParameter("@K1", SqlDbType.NVarChar) { Value = kval.K1 });
                    command.Parameters.Add(new SqlParameter("@Kval1", SqlDbType.NVarChar) { Value = kval.Kval1 });
                    command.Parameters.Add(new SqlParameter("@K2", SqlDbType.NVarChar) { Value = kval.K2 });
                    command.Parameters.Add(new SqlParameter("@Kval2", SqlDbType.NVarChar) { Value = kval.Kval2 });
                    command.Parameters.Add(new SqlParameter("@K3", SqlDbType.NVarChar) { Value = kval.K3 });
                    command.Parameters.Add(new SqlParameter("@Kval3", SqlDbType.NVarChar) { Value = kval.Kval3 });
                    command.Parameters.Add(new SqlParameter("@K4", SqlDbType.NVarChar) { Value = kval.K4 });
                    command.Parameters.Add(new SqlParameter("@Kval4", SqlDbType.NVarChar) { Value = kval.Kval4 });
                    command.Parameters.Add(new SqlParameter("@Locn", SqlDbType.NVarChar) { Value = kval.Locn });

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
            }

            return NoContent();
        }

        [HttpGet("preview")]
        public IActionResult PreviewPdf(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound("PDF file not found.");
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, "application/pdf");
        }
    } 
}
    





