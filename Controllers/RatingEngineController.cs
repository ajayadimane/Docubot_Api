using DocuBot_Api.Context;

using DocuBot_Api.Models.RatingEngine_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using static DocuBot_Api.Models.RatingEngine_Models.Requestforrating;

namespace DocuBot_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingEngineController : ControllerBase
    {

        public RatingEngineDB db;
        private readonly IConfiguration _configuration;
        //private readonly AppSettings _appSettings;

        public RatingEngineController(IConfiguration configuration)
        {

            db = new RatingEngineDB();
            _configuration = configuration;
        }


        [HttpPost]
        [Route("GetLoanDetails")]
        public IActionResult GetDetails(LoanApplicationNo loanapplno)
        {
            try
            {
                LoanDetails lndet = db.GetLoanDetails(loanapplno.ApplNo);

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
                           // lndet.Loantypeid,
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
                        message = "Loan Details retrieved Successfully",
                    });
                }
                else
                {
                    return new JsonResult(new { code = "0", message = "Loan Details could not be found", status = "Failure" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Log the exception or handle it as needed
                return new JsonResult(new { code = "0", message = "An error occurred while retrieving loan details", status = "Failure" });
            }
        }


        [HttpPost]
        [Route("InsertLoanDetails")]
        public IActionResult InsertLoanDetails(LoanDetailsMod loanDetails)
        {
            try
            {

               // DocubotDbContext _context = new();
                db.InsertIntoLoanDetails(loanDetails);
                return new JsonResult(new { code = "1", message = "Loan Details saved Successfully", status = "Success" });


            }
            catch (Exception ec)
            {
                return new JsonResult(new { code = "0", message = "Loan Details could not be saved", status = "Failure" });
            }
        }


        [HttpPost]
        [Route("UpdateLoanDetails")]
        public IActionResult UpdateLoanDetails(LoanDetailsMod loanDetails)
        {
            try
            {
                DocubotDbContext _context = new();
                db.UpdateLoanDetails(loanDetails);
                return new JsonResult(new { code = "1", message = "Loan Details saved Successfully", status = "Success" });


            }
            catch (Exception ec)
            {
                return new JsonResult(new { code = "0", message = "Loan Details could not be saved", status = "Failure" });
            }
        }


        //[HttpPost]
        //[Route("RatingEngineData")]
        //public IActionResult RatingEngineData(LoanDocuments loanDocuments)
        //{
        //    try
        //    {

        //        DocubotDbContext _context = new();
        //        db.InsertIntoLoanDoc(loanDocuments);
        //        List<LoanSchedule> loanSchedule = db.GetLoanSchedule(loanDocuments.LoanApplNo);
        //        LoanDetail loanDetail = db.GetLoanDetails(loanDocuments.LoanApplNo);
        //        return new JsonResult(new { code = "1", loanschedule = loanSchedule, loandetails = loanDetail, message = "Loan schedule and details retrieved Successfully", status = "Success" });


        //    }
        //    catch (Exception ec)
        //    {
        //        return new JsonResult(new { code = "0", message = "Loan schedule and details could not be retrieved", status = "Failure" });
        //    }
        //}


        [HttpPost("CalculateRating")]
        public async Task<IActionResult> CalculateRating(Requestforrating requestforrating)
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
                        //command.Parameters.AddWithValue("@lfid", requestforrating.DocumentId);
                        command.Parameters.AddWithValue("@app", requestforrating.ApplNo);

                        await command.ExecuteNonQueryAsync();
                    }

                //    string updateQuery = @"
                //UPDATE [DEMODOCUBOT].[dbo].[LoanDetails]
                //SET
                //    [rating] = 3318
                //WHERE
                //    [applno] = @applno";


                //    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                //    {
                //        updateCommand.Parameters.AddWithValue("@applno", requestforrating.ApplNo);
                //        await updateCommand.ExecuteNonQueryAsync();
                //    }

                    using (SqlCommand secondCommand = new SqlCommand("USP_GetLoanParam", connection))
                    {
                        secondCommand.CommandType = CommandType.StoredProcedure;
                        secondCommand.Parameters.AddWithValue("@loancode", requestforrating.ApplNo);

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


                        LoanDetails lndet = db.GetLoanDetails(requestforrating.ApplNo);
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

    }
}


    

