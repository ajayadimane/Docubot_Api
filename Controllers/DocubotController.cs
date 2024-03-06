using DocuBot_Api.Context;
using DocuBot_Api.Models;
using DocuBot_Api.Models.RatingEngine_Models;
using DocuBot_Api.Models_Pq;
using DocuBot_Api.Models_Pq.RequestViewModels;
using DocuBot_Api.Models_Pq.ResponseModels;
using DocuBot_Api.Rating_Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Text;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocubotController : ControllerBase
    {
        public RatingEngineDB db;
        private readonly RatingContext _context;
        private readonly DocubotDbContext _docubotDbContext;
        private readonly IConfiguration _configuration;
        //private readonly IHttpClientFactory _httpClientFactory;

        public DocubotController(RatingContext context, DocubotDbContext docubotDbContext, IConfiguration configuration)
        {
            _context = context;
            _docubotDbContext = docubotDbContext;
            db = new RatingEngineDB();
            _configuration = configuration;
            //_httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpGet("GetLoadedFiles")]
        public async Task<ActionResult> GetLoadedFiles()
        {
            var res = await _context.Loadedfiles.ToListAsync();
          
            return Ok(res);
        }

        [Authorize]
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
                        //var ResFilepath = Path.Combine("/rating/UploadFiles/", file.FileName);
                        var ResFilepath = Path.Combine("/rating/UploadFiles/", file.FileName);



                        var InsertLoadedFilesModel = new Rating_Models.Loadedfile
                        { 
                            Docname = ResFilepath,
                            Applno = Applno
                        };

                        await _context.Loadedfiles.AddAsync(InsertLoadedFilesModel);
                        await _context.SaveChangesAsync();
                       
                        results.Add(new UploadFilesResModel
                        {
                            
                            Applno = Applno,
                            Lfid = InsertLoadedFilesModel.Id
                            
                        });

                    }
                  
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


        [Authorize]
        [HttpPost("ExtractKeyVal")]
        public async Task<ActionResult> ExtractKeyval(int docid)
        {
            string baseUrl = "https://demo.botaiml.com";
            string endpoint = "/extract/extract_details/";

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(endpoint))
            {
                return BadRequest("Invalid base URL or endpoint.");
            }

            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };

                // Append docid to the endpoint URL
                string requestUrl = $"{baseUrl}{endpoint}?docid={docid}";

                var response = await client.PostAsync(requestUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    var bankName = _context.ExtractionKeyValues
                    .Where(e => e.Docid == docid)
                    .Select(e => e.Bankname)
                    .FirstOrDefault();

                    if (string.IsNullOrEmpty(bankName))
                    {
                        return NotFound($"Bank not found for docId: {docid}");
                    }

                    var bankConfiguration = _configuration.GetSection($"BankConfigurations:{bankName.ToUpper()}")
                        .Get<BankConfiguration>();

                    if (bankConfiguration == null)
                    {
                        return NotFound($"Configuration not found for bank: {bankName}");
                    }

                    var bankDetails = _context.ExtractionKeyValues
                        .Where(e => e.Docid == docid && e.Bankname == bankName)
                        .Select(e => GenerateResponse(e, bankConfiguration.Fields))
                        .FirstOrDefault();


                    if (bankDetails == null)
                    {
                        return NotFound();
                    }

                    return Ok(bankDetails);
                
                }
                else if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    // Handle redirect by extracting the new location from headers
                    var redirectUrl = response.Headers.Location;
                    // Make a new request to the redirected URL
                    response = await client.PostAsync(redirectUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return Ok(responseContent);
                    }
                }
                else
                {
                    // Handle other cases where the response status code is not success or redirect
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError($"Exception occurred: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            return BadRequest("Unexpected error occurred");
        }


        private static object GenerateResponse(ExtractionKeyValue entity, IEnumerable<string> fields)
        {
            var response = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;

            // Check if Address1 and Address2 are in the fields list
            var mergeAddressFields = fields.Contains("Address1") && fields.Contains("Address2");

            // Check if Bankaddress1 and Bankaddress2 are in the fields list
            var mergeBankAddressFields = fields.Contains("Bankaddress1") && fields.Contains("Bankaddress2");

            foreach (var field in fields)
            {
                if (field == "Address")
                {
                    var address = entity.GetType().GetProperty("Address")?.GetValue(entity) as string;

                    // Conditionally merge Address1 and Address2 into Address
                    if (mergeAddressFields)
                    {
                        var address1 = entity.GetType().GetProperty("Address1")?.GetValue(entity) as string;
                        var address2 = entity.GetType().GetProperty("Address2")?.GetValue(entity) as string;
                        var address3 = entity.GetType().GetProperty("Address3")?.GetValue(entity) as string;
                        var address4 = entity.GetType().GetProperty("Address4")?.GetValue(entity) as string;
                        address = $"{address} {address1} {address2} {address3} {address4}".Trim();
                    }

                    response.Add("Address", address);
                }
                else if (field == "Bankaddress")
                {
                    var bankAddress = entity.GetType().GetProperty("Bankaddress")?.GetValue(entity) as string;

                    // Conditionally merge Bankaddress1 and Bankaddress2 into Bankaddress
                    if (mergeBankAddressFields)
                    {
                        var bankAddress1 = entity.GetType().GetProperty("Bankaddress1")?.GetValue(entity) as string;
                        var bankAddress2 = entity.GetType().GetProperty("Bankaddress2")?.GetValue(entity) as string;
                        bankAddress = $"{bankAddress} {bankAddress1} {bankAddress2}".Trim();
                    }

                    response.Add("Bankaddress", bankAddress);
                }
                else if (field != "Bankaddress1" && field != "Bankaddress2" && field != "Address1" && field != "Address2")
                {
                    response.Add(field, entity.GetType().GetProperty(field)?.GetValue(entity));
                }
            }

            return response;
        }






        [HttpPost("ExtarctDetialsby ID")]
        public async Task<ActionResult> GetKeyval(int docid)
        {
            var Extarctedvalues = await _context.ExtractionKeyValues.Where(e => e.Docid == docid).ToListAsync();

            return Ok(Extarctedvalues);
        }



        [Authorize]
        [HttpPost("ExtractTransactions")]
        public async Task<ActionResult> ExtractTableData(int docid)
        {
            string baseUrl = "https://demo.botaiml.com";
            string endpoint = "/extract/extract_transactions/";

            if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(endpoint))
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri(baseUrl)
                    };

                    // Append docid to the endpoint URL
                    string requestUrl = $"{baseUrl}{endpoint}?docid={docid}";

                    var response = await client.PostAsync(requestUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var bankName = _context.ExtractTransactionData
                        .Where(e => e.Docid == docid)
                        .Select(e => e.Bankname)
                        .FirstOrDefault();

                        if (string.IsNullOrEmpty(bankName))
                        {
                            return NotFound($"Bank not found for docId: {docid}");
                        }

                        var bankConfiguration = _configuration.GetSection($"TransBankConfig:{bankName.ToUpper()}")
                            .Get<TransBankConfig>();

                        if (bankConfiguration == null)
                        {
                            return NotFound($"Configuration not found for bank: {bankName}");
                        }

                        var bankDetails = await _context.ExtractTransactionData
                        .Where(e => e.Docid == docid && e.Bankname == bankName)
                        .Select(e => GenerateResponsefromconfig(e, bankConfiguration.Fields))
                        .ToListAsync();



                        if (bankDetails == null)
                        {
                            return NotFound();
                        }

                        return Ok(bankDetails);
                    }
                    else if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        // Handle redirect by extracting the new location from headers
                        var redirectUrl = response.Headers.Location;
                        // Make a new request to the redirected URL
                        response = await client.PostAsync(redirectUrl, null);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return Ok(responseContent);
                        }
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, responseContent);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    //_logger.LogError($"Exception occurred: {ex.Message}");
                    return StatusCode(500, "Internal Server Error");
                }
            }

            return BadRequest();
        }

        private static object GenerateResponsefromconfig(Rating_Models.ExtractTransactionDatum entity, IEnumerable<string> fields)
        {
            var response = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;

            foreach (var field in fields)
            {
                response.Add(field, entity.GetType().GetProperty(field)?.GetValue(entity));
            }

            return response;
        }


        [Authorize]
        [HttpPost("InsertLoanDetails")]
        public async Task<ActionResult> InsertLoanDetails(LoanDetailsMod req)
        {
            try
            {
                if (req != null)
                {

                    if (await _context.Loandetails.AnyAsync(u => u.Applno == req.Applno))
                    {
                        return Conflict("LoanDetails with Same Applno already exists in Database.");
                    }
                    // DocubotDbContext _context = new();
                    db.InsertIntoLoanDetails(req);
                        
                    var request = new Rating_Models.Loandetail
                    {
                        Applno = req.Applno,
                        Loanamt = req.Loanamt,
                        Loantype = req.Loantype,
                        Emi = req.Emi,
                        Assetval = req.Assetval,
                        Tenor = req.Tenor,
                        Approvaldate = req.Approvaldate,
                        Disbdate = req.Disbdate,
                        Owncontrib = req.Owncontrib,
                        Income = req.Income,
                        Permth = req.Permth,
                        Taxpaid = req.Taxpaid,
                        Othemi = req.Othemi,
                        Lvr = req.Lvr,
                        Cibil = req.Cibil,
                        Custtype = req.Custtype,
                        Ccbal = req.Ccbal,
                        Emistartdate = req.Emistartdate
                    };

                    

                    await _context.Loandetails.AddAsync(request);
                    await _context.SaveChangesAsync();

                    


                }
                return Ok("Inserted Success");
            }


            catch (Exception ex)
            {
                return BadRequest();
            }
        }



        [Authorize]
        [HttpPost("Rating")]
        public async Task<ActionResult> Rating(string applno)
        {
            string baseUrl = "https://demo.botaiml.com";
            string endpoint = "/extract/rating/";

            if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(endpoint))
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri(baseUrl)
                    };

                    // Append docid to the endpoint URL
                    string requestUrl = $"{baseUrl}{endpoint}?applno={applno}";

                    var response = await client.PostAsync(requestUrl, null);

                    if (response.IsSuccessStatusCode)

                    {
                          var Upadtesql = GetFields(applno);

                        try
                        {
                            string connectionString = _configuration.GetConnectionString("myconn");

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                await connection.OpenAsync();

                              

                            
                                using (SqlCommand secondCommand = new SqlCommand("USP_GetLoanParam", connection))
                                {
                                    secondCommand.CommandType = CommandType.StoredProcedure;
                                    secondCommand.Parameters.AddWithValue("@loancode", applno);

                                    List<LoanSchedule> loanSchedule = new List<LoanSchedule>();

                                    // Execute the second stored procedure
                                    using (SqlDataReader secondReader = await secondCommand.ExecuteReaderAsync())
                                    {
                                        while (await secondReader.ReadAsync())
                                        {
                                            LoanSchedule scheduleItem = new LoanSchedule
                                            {
                                                Id = secondReader.GetInt32(secondReader.GetOrdinal("id")),
                                                Period = secondReader.GetInt32(secondReader.GetOrdinal("period")),
                                                PayDate = secondReader.GetDateTime(secondReader.GetOrdinal("paydate")),
                                                Payment = secondReader.GetDecimal(secondReader.GetOrdinal("payment")),
                                                Current_Balance = secondReader.GetDecimal(secondReader.GetOrdinal("current_balance")),
                                                Interest = secondReader.GetDecimal(secondReader.GetOrdinal("interest")),
                                                Principal = secondReader.GetDecimal(secondReader.GetOrdinal("principal"))
                                            };

                                            loanSchedule.Add(scheduleItem);
                                        }
                                    }


                                    LoanDetails lndet = db.GetLoanDetails(applno);
                                    if (lndet != null)
                                    {
                                        var ratingCalcObject = JsonConvert.DeserializeObject<RatingCalculationContainer>(lndet.RatingCalc);
                                        return new JsonResult(new
                                        {
                                            code = "1",
                                            loandetails = new
                                            {
                                                lndet.Id,
                                                lndet.Applno,
                                                //lndet.Loantypeid,
                                                lndet.Loanamt,
                                                lndet.Emi,
                                                lndet.Assetval,
                                                lndet.Tenor,
                                                //lndet.Appid,
                                                lndet.Approvaldate,
                                                lndet.Disbdate,
                                                //lndet.Status,
                                                lndet.Owncontrib,
                                                //lndet.Intrate,
                                                //lndet.Loanacno,
                                                lndet.Loantype,
                                                lndet.Income,
                                                lndet.Permth,
                                                lndet.Taxpaid,
                                                lndet.Rir,
                                                lndet.Othemi,
                                                lndet.Lvr,
                                                lndet.Cibil,
                                                lndet.Bounced,
                                                lndet.Delayed,
                                                lndet.Custtype,
                                                lndet.Ccbal,
                                                lndet.Emistartdate,
                                                lndet.Rating,
                                                lndet.Dependents,
                                                lndet.Expenses
                                            },

                                            scheduleHeading = "Loan Schedule",

                                            loanSchedule,

                                            message = "Loan schedule and details retrieved Successfully",
                                            status = "Success"
                                        });

                                    }

                                    return Ok("Rating calculation executed successfully.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        // Handle redirect by extracting the new location from headers
                        var redirectUrl = response.Headers.Location;
                        // Make a new request to the redirected URL
                        response = await client.PostAsync(redirectUrl, null);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return Ok(responseContent);
                        }
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, responseContent);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    //_logger.LogError($"Exception occurred: {ex.Message}");
                    return StatusCode(500, "Internal Server Error");
                }
            }

            return BadRequest();
        }


        [HttpGet("GetRIE")]
        public async Task<ActionResult<object>> GetFields(string applno)
        {
            var loanDetails = await _context.Loandetails
                .Where(e => e.Applno == applno)
                .Select(e => new { e.Rating, e.Income, e.Expenses })
                 .FirstOrDefaultAsync();

            if (loanDetails == null)
            {
                return NotFound();
            }

            // Update SQL Server table based on PostgreSQL data
            var sqlServerEntity = await _docubotDbContext.LoanDetailsDemo
                .FirstOrDefaultAsync(e => e.Applno == applno);

            if (sqlServerEntity != null)
            {
                // Update fields in the SQL Server entity
                sqlServerEntity.Rating = loanDetails.Rating;
                sqlServerEntity.Income = (int?)loanDetails.Income;
                sqlServerEntity.Expenses = (int?)loanDetails.Expenses;

                // Save changes to the SQL Server database
                _docubotDbContext.Update(sqlServerEntity);
                _docubotDbContext.SaveChanges();
            }
            else
            {
                // Handle case where record is not found in SQL Server table
                return NotFound("Record not found in SQL Server table.");
            }

            return Ok("Record updated successfully.");
        }

    }
}
