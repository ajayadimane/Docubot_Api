using DocuBot_Api.Context;
using DocuBot_Api.Models;
using DocuBot_Api.Models.RatingEngine_Models;
using DocuBot_Api.Models_Pq;
using DocuBot_Api.Models_Pq.RequestViewModels;
using DocuBot_Api.Models_Pq.ResponseModels;
using DocuBot_Api.Rating_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("GetLoadedFiles")]
        public async Task<ActionResult> GetLoadedFiles()
        {
            var res = await _context.Loadedfiles.ToListAsync();
          
            return Ok(res);
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
                            Id = InsertLoadedFilesModel.Id,
                            Docname = InsertLoadedFilesModel.Docname,
                            Applno = Applno
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

        [HttpPost("ExtractKeyVal")]
        public async Task<ActionResult> ExtractKeyval(int docid)
        {
            string baseUrl = "https://demo.botaiml.com";
            string endpoint = "/extract/extract_details/";

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

                        var transactionDetail = await _context.ExtractionKeyValues.Where(td => td.Docid == docid).FirstOrDefaultAsync();

                        if (transactionDetail == null)
                        {
                            return NotFound();
                        }

                        var Extractkeyval = new KeyvalRes
                        {
                            Bankname = transactionDetail.Bankname,
                            Accountholder = transactionDetail.Accountholder,
                            Address = transactionDetail.Address,
                            Date = transactionDetail.Date,
                            Accountno = transactionDetail.Accountno,
                            Accountdescription = transactionDetail.Accountdescription,
                            Branch = transactionDetail.Branch,
                            Drawingpower = transactionDetail.Drawingpower,
                            Interestrate = transactionDetail.Interestrate,
                            Ifsc = transactionDetail.Ifsc,
                            Micrcode = transactionDetail.Micrcode,
                            //Cif = transactionDetail.Cif,
                            Nominationregistered = transactionDetail.Nominationregistered,
                            Balanceason = transactionDetail.Balanceason,
                            Balanceamount = transactionDetail.Balanceamount,
                            Statementperiod = transactionDetail.Statementperiod,
                        };

                        return Ok(Extractkeyval);
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

        [HttpPost("ExtarctDetialsby ID")]
        public async Task<ActionResult> GetKeyval(int docid)
        {
            var Extarctedvalues = await _context.ExtractionKeyValues.Where(e => e.Docid == docid).ToListAsync();

            return Ok(Extarctedvalues);
        }




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
                        var transactionDetails = await _context.ExtractTransactionData
                        .Where(td => td.Docid == docid)
                        .ToListAsync();

                        if (transactionDetails == null || !transactionDetails.Any())
                        {
                            return NotFound();
                        }

                        var responseList = transactionDetails.Select(td => new ExtractTransResModel
                        {
                            TxnDate = td.TxnDate,
                            ValueDate = td.ValueDate,
                            Description = td.Description,
                            ChequeNumber = td.ChequeNumber,
                            Amount = td.Amount,
                            Debit = td.Debit,
                            Credit = td.Credit,
                            Balance = td.Balance
                        }).ToList();

                        return Ok(responseList);
                    }
                    else if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        // Handle redirect by extracting the new location from headers
                        var redirectUrl = response.Headers.Location;
                        // Make a new request to the redirected URL
                        response = await client.PostAsync(redirectUrl, null);

                        if (response.IsSuccessStatusCode)
                        {
                            var Extarctedvalues = await _context.ExtractTransactionData.Where(e => e.Docid == docid).ToListAsync();

                            return Ok(Extarctedvalues);
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

        [HttpPost]
        public async Task<ActionResult> InsertLoanDetails(LoanDetailsMod req)
        {
            try
            {
                if (req != null)
                {
                    
                    

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
                        var ExtarctRIE = await _context.Loandetails
                        .Where(e => e.Applno == applno)
                        .Select(e => new { e.Rating, e.Income, e.Expenses })
                        .FirstOrDefaultAsync();

                        if (ExtarctRIE == null)
                        {
                            return NotFound();
                        }

                        // Update SQL Server table based on PostgreSQL data
                        var sqlServerEntity = await _docubotDbContext.LoanDetailsDemo
                            .FirstOrDefaultAsync(e => e.Applno == applno);

                        if (sqlServerEntity != null)
                        {
                            // Update fields in the SQL Server entity
                            sqlServerEntity.Rating = ExtarctRIE.Rating;
                            sqlServerEntity.Income = (int?)ExtarctRIE.Income;
                            sqlServerEntity.Expenses = (int?)ExtarctRIE.Expenses;

                            // Save changes to the SQL Server database
                            _docubotDbContext.Update(sqlServerEntity);
                            _docubotDbContext.SaveChanges();
                        }




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
                                                lndet.Expenses,


                                                // Properly structure the "Rating Calculation" object
                                                ratingCalcObject

                                                // Include other properties as needed
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
