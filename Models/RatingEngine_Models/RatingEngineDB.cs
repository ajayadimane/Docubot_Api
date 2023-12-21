using DocuBot_Api.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DocuBot_Api.Models.RatingEngine_Models
{
    public class RatingEngineDB
    {
        public LoanDetails GetLoanDetails(string app)
        {
            DocubotDbContext _context = new();
            LoanDetails lndetils = _context.LoanDetailsDemo.Where(ln => ln.Applno == app).FirstOrDefault();

            // Check if lndetils is not null and RatingCalc is not null or empty
            if (lndetils != null && !string.IsNullOrEmpty(lndetils.RatingCalc))
            {
                // Deserialize the JSON string to an object
                object ratingCalcObject = JsonConvert.DeserializeObject<object>(lndetils.RatingCalc);

                // Convert the object to a JSON-formatted string
                lndetils.RatingCalc = JsonConvert.SerializeObject(ratingCalcObject);
            }

            return lndetils;
        }


        public void InsertIntoLoanDetails(LoanDetailsMod loanDetails)
        {
            DocubotDbContext _context = new();
            try
            {
                LoanDetails lon = new()
                {
                    Applno = loanDetails.Applno,
                    Loantypeid = loanDetails.Loantypeid,
                    Loanamt = loanDetails.Loanamt,
                    Emi = loanDetails.Emi,
                    Assetval = loanDetails.Assetval,
                    Tenor = loanDetails.Tenor,
                    Appid = loanDetails.Appid,
                    Approvaldate = loanDetails.Approvaldate,
                    Disbdate = loanDetails.Disbdate,
                    Status = loanDetails.Status,
                    Owncontrib = loanDetails.Owncontrib,
                    Intrate = loanDetails.Intrate,
                    Loanacno = loanDetails.Loanacno,
                    Loantype = loanDetails.Loantype,
                    Income = loanDetails.Income,
                    Permth = loanDetails.Permth,
                    Taxpaid = loanDetails.Taxpaid,
                    //Rir = loanDetails.Rir,
                    Othemi = loanDetails.Othemi,
                    Lvr = loanDetails.Lvr,
                    Cibil = loanDetails.Cibil,
                    //Bounced = loanDetails.Bounced,
                    //Delayed = loanDetails.Delayed,
                    Custtype = loanDetails.Custtype,
                    Ccbal = loanDetails.Ccbal,
                    Emistartdate = loanDetails.Emistartdate,
                    // Rating = loanDetails.Rating,
                    //Dependents = loanDetails.Dependents,
                    //Expenses = loanDetails.Expenses,
                    //RatingCalc = loanDetails.RatingCalc,
                    //Emistartdate = loanDetails.Emistartdate,
                    //Disposableinc = loanDetails.Disposableinc,
                    //Fixedexp = loanDetails.Fixedexp,
                    //Discretionexp = loanDetails.Discretionexp,
                    //Prodtypeid = loanDetails.Prodtypeid,
                    //Profession = loanDetails.Profession,
                    //Incomerangeid = loanDetails.Incomerangeid,
                    //Education = loanDetails.Education,
                    //Residencetypeid = loanDetails.Residencetypeid,
                    //Age = loanDetails.Age,
                    //Dependents = loanDetails.Dependents,
                    //Yrsinresidence = loanDetails.Yrsinresidence,
                    //Yrsofearning = loanDetails.Yrsofearning,
                    //Landholding = loanDetails.Landholding,
                    //Giloanratio = loanDetails.Giloanratio,
                    //Nondiscexpratio = loanDetails.Nondiscexpratio,
                    //Loandefaults = loanDetails.Loandefaults,
                    //Loanpurpose = loanDetails.Loanpurpose

                };
                _context.LoanDetailsDemo.Add(lon);
                _context.SaveChanges();
            }
            catch (Exception ex)
            { }

        }

        public void UpdateLoanDetails(LoanDetailsMod loanDetails)
        {
            DocubotDbContext _context = new();
            try
            {
                LoanDetails lon = _context.LoanDetailsDemo.Where(ln => ln.Loanacno == loanDetails.Loanacno).FirstOrDefault();
                if (lon != null)
                {
                    lon.Applno = loanDetails.Applno;
                    lon.Loantypeid = loanDetails.Loantypeid;
                    lon.Loanamt = loanDetails.Loanamt;
                    lon.Emi = loanDetails.Emi;
                    lon.Assetval = loanDetails.Assetval;
                    lon.Tenor = loanDetails.Tenor;
                    lon.Appid = loanDetails.Appid;
                    lon.Approvaldate = loanDetails.Approvaldate;
                    lon.Disbdate = loanDetails.Disbdate;
                    lon.Status = loanDetails.Status;
                    lon.Owncontrib = loanDetails.Owncontrib;
                    lon.Intrate = loanDetails.Intrate;
                    lon.Loanacno = loanDetails.Loanacno;
                    lon.Loantype = loanDetails.Loantype;
                    lon.Income = loanDetails.Income;
                    lon.Permth = loanDetails.Permth;
                    lon.Taxpaid = loanDetails.Taxpaid;
                    //lon.Rir = loanDetails.Rir;
                    lon.Othemi = loanDetails.Othemi;
                    lon.Lvr = loanDetails.Lvr;
                    lon.Cibil = loanDetails.Cibil;
                    //lon.Bounced = loanDetails.Bounced;
                    //lon.Delayed = loanDetails.Delayed;
                    lon.Custtype = loanDetails.Custtype;
                    lon.Ccbal = loanDetails.Ccbal;
                    lon.Emistartdate = loanDetails.Emistartdate;
                    //lon.Rating = loanDetails.Rating;
                    //lon.Dependents = loanDetails.Dependents;
                    //lon.Expenses = loanDetails.Expenses;
                    //lon.RatingCalc = loanDetails.RatingCalc;
                    //lon.Emistartdate = loanDetails.Emistartdate;
                    //lon.Disposableinc = loanDetails.Disposableinc;
                    //lon.Fixedexp = loanDetails.Fixedexp;
                    //lon.Discretionexp = loanDetails.Discretionexp;
                    //lon.Prodtypeid = loanDetails.Prodtypeid;
                    //lon.Profession = loanDetails.Profession;
                    //lon.Incomerangeid = loanDetails.Incomerangeid;
                    //lon.Education = loanDetails.Education;
                    //lon.Residencetypeid = loanDetails.Residencetypeid;
                    //lon.Age = loanDetails.Age;
                    //lon.Dependents = loanDetails.Dependents;
                    //lon.Yrsinresidence = loanDetails.Yrsinresidence;
                    //lon.Yrsofearning = loanDetails.Yrsofearning;
                    //lon.Landholding = loanDetails.Landholding;
                    //lon.Giloanratio = loanDetails.Giloanratio;
                    //lon.Nondiscexpratio = loanDetails.Nondiscexpratio;
                    //lon.Loandefaults = loanDetails.Loandefaults;
                    //lon.Loanpurpose = loanDetails.Loanpurpose;
                };
                _context.Update(lon);
                _context.SaveChanges();
            }
            catch (Exception ex)
            { }

        }

        public void InsertIntoLoanDoc(LoanDocuments loanDocDetails)
        {
            DocubotDbContext _context = new();
            try
            {
                for (int i = 0; i < loanDocDetails.DocId.Count; i++)
                {
                    LoanDoc londc = new()
                    {
                        DocId = loanDocDetails.DocId[i],
                        LoanApplNo = loanDocDetails.LoanApplNo
                    };
                    _context.LoanDocs.AddRange(londc);
                }

                //List<LoanDocuments> londt = loanDocDetails.loanDocs;

                //foreach (var lon in londt)
                //{
                //    LoanDoc londc = new()
                //    {
                //        DocId=lon.DocId,
                //        LoanReferenceId=lon.LoanReferenceId
                //    };
                //    _context.LoanDocs.AddRange(londc);
                //}                
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public List<LoanSchedule> GetLoanSchedule(string loanapplno)
        {
            DocubotDbContext _context = new();
            var details = _context.loanschedule.FromSqlInterpolated($"Exec USP_GetLoanParam @loancode = {loanapplno}").ToList();
            return details;
            //_context.downloadREData.ExecuteSqlInterpolated($"Exec USP_GetLoanParam @loancode = {loanrefid}");
        }
    }
}
