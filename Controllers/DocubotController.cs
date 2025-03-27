using DocuBot_Api.Context;
using DocuBot_Api.Models;
using DocuBot_Api.Models.RatingEngine_Models;
using DocuBot_Api.Models_Pq;
using DocuBot_Api.Models_Pq.RequestViewModels;
using DocuBot_Api.Models_Pq.ResponseModels;
using DocuBot_Api.Rating_Models;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Loadedfile = DocuBot_Api.Rating_Models.Loadedfile;

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
        private readonly RatingContext _ratingContext;
        //private readonly IHttpClientFactory _httpClientFactory;

        public DocubotController(RatingContext context, DocubotDbContext docubotDbContext, IConfiguration configuration, RatingContext ratingContext)
        {
            _context = context;
            _docubotDbContext = docubotDbContext;
            db = new RatingEngineDB();
            _configuration = configuration;
            _ratingContext = ratingContext;
            //_httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpGet("GetLoadedFiles")]
        public async Task<ActionResult> GetLoadedFiles()
        {
            var res = await _context.Loadedfiles.ToListAsync();
          
            return Ok(res);
        }




        //[Authorize]
        //[HttpPost("UploadDocument")]
        //public async Task<ActionResult> UploadPdf(IFormFileCollection files, string Applno)
        //{
        //    if (files != null && files.Count > 0)
        //    {
        //        try
        //        {
        //            // Validate the loanaccno parameter as needed

        //            // Get the working directory of the application
        //            string workingDirectory = Directory.GetCurrentDirectory();

        //            // Create a folder for the loanaccno in the UploadedFiles directory
        //            string loanaccnoFolder = Path.Combine(workingDirectory, "UploadedFiles", Applno);
        //            Directory.CreateDirectory(loanaccnoFolder);

        //            // List to store results for each file
        //            var results = new List<object>();

        //            // Iterate through each uploaded file
        //            for (int i = 0; i < files.Count; i++)
        //            {
        //                var file = files[i];

        //                // Define input file name with loanaccno and index
        //                string inputFileName = $"{Applno}_file{i + 1}{Path.GetExtension(file.FileName)}";

        //                // Save the input file in the loanaccno folder
        //                string inputFilepath = Path.Combine(loanaccnoFolder, inputFileName);
        //                using (var inputStream = new FileStream(inputFilepath, FileMode.Create))
        //                {
        //                    await file.CopyToAsync(inputStream);
        //                }
        //                //var ResFilepath = Path.Combine("/rating/UploadFiles/", file.FileName);
        //                var ResFilepath = Path.Combine("/rating/UploadFiles/", file.FileName);



        //                var InsertLoadedFilesModel = new Rating_Models.Loadedfile
        //                { 
        //                    Docname = ResFilepath,
        //                    Applno = Applno
        //                };

        //                await _context.Loadedfiles.AddAsync(InsertLoadedFilesModel);
        //                await _context.SaveChangesAsync();
                       
        //                results.Add(new UploadFilesResModel
        //                {
                            
        //                    Applno = Applno,
        //                    Docid = InsertLoadedFilesModel.Id
                            
        //                });

        //            }
                  
        //            return Ok(results);
                   
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest("No files uploaded.");
        //    }
        //}

        [Authorize]
        [HttpPost("UploadDocument")]
        public async Task<ActionResult> UploadDocument(IFormFileCollection files, string applno)
        {
            string baseUrl = "https://demo.botaiml.com";
            string endpoint = "/extract/uploaddocument/";

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

                // Create a new instance of MultipartFormDataContent
                using var formData = new MultipartFormDataContent();

                // Add Applno as a string content
                formData.Add(new StringContent(applno), "applno");

                //var results = new List<object>();
                var results = new List<UploadFilesResModel>();
                // Add each file in the collection to the form data
                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    var fileName = "./XmlFiles/" + file.FileName;

                    if (fileExtension == ".xml")
                    {

                        var existingFile = _ratingContext.Loadedfiles.FirstOrDefault(f => f.Docname == fileName && f.Applno == applno);

                        if (existingFile != null)
                        {
                            // File with the same name and applno already exists, return its details
                            //results.Add(new UploadFilesResModel
                            //{
                            //    Appno = applno,
                            //    Docid = existingFile.Id,   
                            //    Filename = existingFile.Docname
                            //});

                            //continue; // Skip saving and processing this file

                            return new JsonResult(new
                            {
                                code = "0",
                                message = $"Xml file already present with the docid = {existingFile.Id}",
                                status = "Failure"
                            });
                        }
                    
                        // Create a directory for XML files if it doesn't exist
                        string xmlFolderPath = Path.Combine("XmlFiles");
                        if (!Directory.Exists(xmlFolderPath))
                        {
                            Directory.CreateDirectory(xmlFolderPath);
                        }

                        // Save the XML file to the folder
                        string xmlFilePath = Path.Combine(xmlFolderPath, file.FileName);
                        using (var stream = new FileStream(xmlFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var relativeFilePath = $"{fileName.Replace(@"\", "/")}";

                        var loadedFile = new Loadedfile
                        {
                            Docname = relativeFilePath, // Store the file path with the correct format
                            Applno = applno
                        };

                        _ratingContext.Loadedfiles.Add(loadedFile);
                        await _ratingContext.SaveChangesAsync();

                        results.Add(new UploadFilesResModel
                        {
                            Appno = applno,
                            Docid = loadedFile.Id,
                            Filename = relativeFilePath
                        });
                    }
                    else if (fileExtension == ".pdf")
                    {

                        // Add the file content to the form data
                        formData.Add(new StreamContent(file.OpenReadStream())
                        {
                            Headers =
                        {
                          ContentLength = file.Length,
                           ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType)
                        }
                        },
                  "files", file.FileName);
                    }
                }

                if (formData.Any(content => content.Headers.ContentType.MediaType == "application/pdf"))
                {

                    // Append docid to the endpoint URL
                    string requestUrl = $"{baseUrl}{endpoint}?applno={applno}";

                    // Post the form data to the API
                    var response = await client.PostAsync(requestUrl, formData);

                    if (response.IsSuccessStatusCode)
                    {
                       
                     var responseContent = await response.Content.ReadAsStringAsync();
                     var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                     var insertedIds = responseObject["inserted_ids"];
                    if (insertedIds == null || !insertedIds.HasValues)
                    {
                       // return Ok(new { message = responseObject["message"].ToString() });

                        return new JsonResult(new
                        {
                            code = "1",
                            message = responseObject["message"].ToString(),
                            //insertedFiles = results,
                            status = "Success"
                        });
                    }

                    //var results = new List<UploadFilesResModel>();

                    foreach (var item in insertedIds)
                    {
                        results.Add(new UploadFilesResModel
                        {
                            Appno = applno,
                            Docid = item.id,
                            Filename = item.file_path
                        });
                    }

                      //return Ok(new { message = responseObject["message"].ToString(), insertedFiles = results });
                    return new JsonResult(new
                    {
                        code = "1",
                        message = responseObject["message"].ToString(),
                        insertedFiles = results,
                        status = "Success"
                    });
                }
                    else if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        // Handle redirect by extracting the new location from headers
                        var redirectUrl = response.Headers.Location;
                        // Make a new request to the redirected URL
                        response = await client.PostAsync(redirectUrl, formData);

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
                        //return StatusCode((int)response.StatusCode, responseContent);
                    return new JsonResult(new
                    {
                        code = "0",
                        message = responseContent,
                        status = "Failure"
                    });

                    }
                             
                }
                else
                {
                    return new JsonResult(new
                    {
                        code = "1",
                        message = "Xml file has been inserted Succesfully",
                        insertedFiles = results,
                        status = "Success"
                    });
                }
            }

            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError($"Exception occurred: {ex.Message}");
                return new JsonResult(new { code = "0", message = ex.Message + " Files could not be uploaded. Please Try Again", status = "Failure" });
            }

            return BadRequest("Unexpected error occurred");
        }



        [Authorize]
        [HttpPost("ExtractKeyVal")]
        public async Task<ActionResult> ExtractKeyval(int docid)
        {
            // Retrieve the file path based on the docid
            var filePath = _ratingContext.Loadedfiles
                            .Where(f => f.Id == docid)
                            .Select(f => f.Docname)
                            .FirstOrDefault();

            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound($"File not found for docId: {docid}");
            }

            // Check the file extension
            var fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".xml")
            {
                // Call ProcessXml method
                return await ExtractData(filePath , docid);
            }
            else if (fileExtension == ".pdf")
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
                        var responseContent = await response.Content.ReadAsStringAsync();

                        //// Parse the response content to check for pdf_path
                        //var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                        //if (responseObject != null && responseObject.ContainsKey("pdf_path"))
                        //{
                        //    string pdfPath = responseObject["pdf_path"].ToString();
                        //    return await ExtractData(pdfPath);
                        //}

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
                            //return NotFound();
                            return new JsonResult(new { code = "0", message = "Process failed, Key-Value data not found", status = "Failure" });
                        }

                        ConvertDatesToStandardFormat(bankDetails);

                        //return Ok(bankDetails);

                        return new JsonResult(new
                        {
                            code = "1",
                            extracted_Values = bankDetails,
                            message = "Documents processing completed",
                            status = "Success"
                        });


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
                            //return Ok(responseContent);

                        }
                    }
                    else
                    {
                        // Handle other cases where the response status code is not success or redirect
                        var responseContent = await response.Content.ReadAsStringAsync();
                        //return StatusCode((int)response.StatusCode, responseContent);

                        return new JsonResult(new
                        {
                            code = "0",
                            message = responseContent,
                            status = "Failure"
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    //_logger.LogError($"Exception occurred: {ex.Message}");
                    //return StatusCode(500, "Internal Server Error");
                    return new JsonResult(new { code = "0", message = ex.Message, status = "Failure" });
                }
                //return BadRequest("Unexpected error occurred");
                return new JsonResult(new { code = "0", message = "Unexpected error occurred", status = "Failure" });
            }
            {
                return BadRequest("Unsupported file type. Only .xml and .pdf are supported.");
            }
        }


        private async Task<ActionResult> ExtractData(string pdfPath, int docid)
        {
            try
            {
                if (string.IsNullOrEmpty(pdfPath) || !System.IO.File.Exists(pdfPath))
                {
                    return NotFound("File not found.");
                }

                var existingData = await _ratingContext.ExtractionKeyValues
          .Where(e => e.Docid == docid)
          .FirstOrDefaultAsync();

                if (existingData != null)
                {
                    // Return the existing data
                    var response = new
                    {
                        code = "1",
                        extracted_Values = new
                        {
                            Accountholder = existingData.Accountholder,
                            Address = existingData.Address,
                            Accountno = existingData.Accountno,
                            Branch = existingData.Branch,
                            Ifsc = existingData.Ifsc,
                            Micrcode = existingData.Micrcode,
                            Opendate = FormatDate (existingData.Opendate),
                            Dob = FormatDate(existingData.DOB),
                            Mobileno = existingData.Mobileno,
                            Email = existingData.Email,
                            Balanceason = existingData.Balanceason,
                            Accountstatus = existingData.Accountstatus,
                            Accounttype = existingData.Accounttype,
                            Pan = existingData.Pan,
                            Statementperiodfrom = FormatDate(existingData.Statementperiodfrom),
                            Statementperiodto = FormatDate(existingData.Statementperiodto)
                        },
                        message = "Data retrieved from the database",
                        status = "Success"
                    };

                    return new JsonResult(response);
                }


                using var stream = new FileStream(pdfPath, FileMode.Open);

                XDocument doc;
                try
                {
                    doc = XDocument.Load(stream);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error loading XML content: {ex.Message}");
                }

                XNamespace ns = "http://api.rebit.org.in/FISchema/deposit";

                var account = doc.Descendants(ns + "Account").FirstOrDefault();
                var holder = doc.Descendants(ns + "Holder").FirstOrDefault();
                var summary = doc.Descendants(ns + "Summary").FirstOrDefault();
                var transactions = doc.Descendants(ns + "Transactions").FirstOrDefault();

                if (account == null || holder == null || summary == null || transactions == null)
                {
                    return BadRequest("Invalid XML format or missing elements.");
                }

                // Function to format date as dd/MM/yyyy
                string FormatDate(string dateStr)
                {
                    if (DateTime.TryParseExact(dateStr, new[] { "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" },
                        null, DateTimeStyles.None, out var date))
                    {
                        return date.ToString("dd-MM-yyyy");
                    }
                    return ""; // Return empty if the date is invalid
                }



                var accountInfo = new ExtractionKeyValue
                {
                    Docid = docid,
                    Accountholder = holder.Attribute("name")?.Value,
                    Address = holder.Attribute("address")?.Value,
                    Accountno = account.Attribute("maskedAccNumber")?.Value,
                    Date = DateTime.TryParseExact(summary.Attribute("balanceDateTime")?.Value, new[] { "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" },
                           null, DateTimeStyles.None, out var balanceDateTime) ? balanceDateTime : (DateTime?)null,
                    Email = holder.Attribute("email")?.Value,
                    Mobileno = holder.Attribute("mobile")?.Value,
                    Pan = holder.Attribute("pan")?.Value,
                    Balanceason = summary.Attribute("currentBalance")?.Value,
                    Branch = summary.Attribute("branch")?.Value,
                    DOB = FormatDate(holder.Attribute("dob")?.Value),
                    Ifsc = summary.Attribute("ifscCode")?.Value,
                    Micrcode = summary.Attribute("micrCode")?.Value,
                    Odlimit = summary.Attribute("drawingLimit")?.Value,
                    Opendate = FormatDate(summary.Attribute("openingDate")?.Value),
                    Accountstatus = summary.Attribute("status")?.Value,
                    Accounttype = summary.Attribute("type")?.Value,
                    Statementperiodfrom = FormatDate(transactions?.Attribute("startDate")?.Value),
                    Statementperiodto = FormatDate(transactions?.Attribute("endDate")?.Value)
                };

                _ratingContext.ExtractionKeyValues.Add(accountInfo);
                await _ratingContext.SaveChangesAsync();


                var newresponse = new
                {
                    code = "1",
                    extracted_Values = new
                    {
                        Accountholder = accountInfo.Accountholder,
                        Address = accountInfo.Address,
                        Accountno = accountInfo.Accountno,
                        Branch = accountInfo.Branch,
                        Ifsc = accountInfo.Ifsc,
                        Micrcode = accountInfo.Micrcode,
                        Opendate = accountInfo.Opendate,
                        Dob = accountInfo.DOB,
                        Mobileno = accountInfo.Mobileno,
                        Email = accountInfo.Email,
                        Balanceason = accountInfo.Balanceason,
                        Accountstatus = accountInfo.Accountstatus,
                        Accounttype = accountInfo.Accounttype,
                        Pan = accountInfo.Pan,
                        Statementperiodfrom = accountInfo.Statementperiodfrom,
                        Statementperiodto = accountInfo.Statementperiodto
                    },
                    message = "Documents processing completed",
                    status = "Success"
                };

                return new JsonResult(newresponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        private static object GenerateResponse(ExtractionKeyValue entity, IEnumerable<string> fields)
        {
            var response = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;

            // Check if Address1 and Address2 are in the fields list
            var mergeAddressFields = fields.Contains("Address1");

            // Check if Bankaddress1 and Bankaddress2 are in the fields list
            var mergeBankAddressFields = fields.Contains("Bankaddress1");

            // Check if Statementperiod is in the fields list
            var statementPeriodFields = fields.Contains("Statementperiod");

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
                    var bankaddress = entity.GetType().GetProperty("Bankaddress")?.GetValue(entity) as string;

                    // Conditionally merge Bankaddress1 and Bankaddress2 into Bankaddress
                    if (mergeBankAddressFields)
                    {
                        var bankaddress1 = entity.GetType().GetProperty("Bankaddress1")?.GetValue(entity) as string;
                        var bankaddress2 = entity.GetType().GetProperty("Bankaddress2")?.GetValue(entity) as string;
                        var bankaddress3 = entity.GetType().GetProperty("Bankaddress3")?.GetValue(entity) as string;

                        bankaddress = $"{bankaddress} {bankaddress1} {bankaddress2} {bankaddress3}".Trim();
                    }

                    response.Add("Bankaddress", bankaddress);
                }
                else if (field == "Statementperiod")
                {
                    var statementPeriod = entity.GetType().GetProperty("Statementperiod")?.GetValue(entity) as string;

                    // Handle different date formats and split into Statementperiodfrom and Statementperiodto
                    if (!string.IsNullOrEmpty(statementPeriod))
                    {
                        DateTime statementPeriodFrom, statementPeriodTo;

                        if (Regex.IsMatch(statementPeriod, @"(\d{2})-(\d{2})-(\d{4}) to (\d{2})-(\d{2})-(\d{4})")) // Match format "dd-mm-yyyy to dd-mm-yyyy"
                        {
                            var dates = statementPeriod.Split("to");

                            statementPeriodFrom = DateTime.ParseExact(dates[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            statementPeriodTo = DateTime.ParseExact(dates[1].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                            response.Add("Statementperiodfrom", statementPeriodFrom.ToString("d MMM yyyy"));
                            response.Add("Statementperiodto", statementPeriodTo.ToString("d MMM yyyy"));
                        }
                        else if (Regex.IsMatch(statementPeriod, @"Account\s+Statement\s+from\s+(\d{1,2})\s*(\w+)\s*(\d{4})\s*to\s*(\d{1,2})\s*(\w+)\s*(\d{4})")) // Match format "Account Statement from dd MMM yyyy to dd MMM yyyy"
                        {
                            var matches = Regex.Match(statementPeriod, @"Account\s+Statement\s+from\s+(\d{1,2})\s*(\w+)\s*(\d{4})\s*to\s*(\d{1,2})\s*(\w+)\s*(\d{4})");

                            var startDay = matches.Groups[1].Value;
                            var startMonth = matches.Groups[2].Value;
                            var startYear = matches.Groups[3].Value;

                            var endDay = matches.Groups[4].Value;
                            var endMonth = matches.Groups[5].Value;
                            var endYear = matches.Groups[6].Value;

                            var startDateString = $"{startDay} {startMonth} {startYear}";
                            var endDateString = $"{endDay} {endMonth} {endYear}";

                            if (DateTime.TryParseExact(startDateString, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodFrom) &&
                                DateTime.TryParseExact(endDateString, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodTo))
                            {
                                response.Add("Statementperiodfrom", statementPeriodFrom.ToString("d MMM yyyy"));
                                response.Add("Statementperiodto", statementPeriodTo.ToString("d MMM yyyy"));
                            }
                            else
                            {
                                // If parsing fails, add the original value
                                response.Add("Statementperiod", statementPeriod);
                            }
                        }
                        else if (Regex.IsMatch(statementPeriod, @"([a-zA-Z]+)\s*(\d{2})[,/]\s*(\d{4})\s*to\s*([a-zA-Z]+)\s*(\d{2})[,/]\s*(\d{4})")) // Match format "MonthDD,YYYY to MonthDD,YYYY" or "Month DD, YYYY to Month DD, YYYY"
                        {
                            var matches = Regex.Match(statementPeriod, @"([a-zA-Z]+)\s*(\d{2})[,/]\s*(\d{4})\s*to\s*([a-zA-Z]+)\s*(\d{2})[,/]\s*(\d{4})");

                            var startMonth = matches.Groups[1].Value;
                            var startDay = matches.Groups[2].Value;
                            var startYear = matches.Groups[3].Value;

                            var endMonth = matches.Groups[4].Value;
                            var endDay = matches.Groups[5].Value;
                            var endYear = matches.Groups[6].Value;

                            var startDateString = $"{startMonth} {startDay}, {startYear}";
                            var endDateString = $"{endMonth} {endDay}, {endYear}";

                            if (DateTime.TryParseExact(startDateString, "MMMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodFrom) &&
                                DateTime.TryParseExact(endDateString, "MMMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodTo))
                            {
                                response.Add("Statementperiodfrom", statementPeriodFrom.ToString("d MMM yyyy"));
                                response.Add("Statementperiodto", statementPeriodTo.ToString("d MMM yyyy"));
                            }
                            else
                            {
                                // If parsing fails, add the original value
                                response.Add("Statementperiod", statementPeriod);
                            }
                        }
                        else if (Regex.IsMatch(statementPeriod, @"(\d{1,2})[/-](\d{2})[/-](\d{4}) to (\d{1,2})[/-](\d{2})[/-](\d{4})")) // Match format "dd/mm/yyyy to dd/mm/yyyy"
                        {
                            var matches = Regex.Match(statementPeriod, @"(\d{1,2})[/-](\d{2})[/-](\d{4}) to (\d{1,2})[/-](\d{2})[/-](\d{4})");

                            var startDay = matches.Groups[1].Value;
                            var startMonth = matches.Groups[2].Value;
                            var startYear = matches.Groups[3].Value;

                            var endDay = matches.Groups[4].Value;
                            var endMonth = matches.Groups[5].Value;
                            var endYear = matches.Groups[6].Value;

                            var startDateString = $"{startDay}-{startMonth}-{startYear}";
                            var endDateString = $"{endDay}-{endMonth}-{endYear}";

                            if (DateTime.TryParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodFrom) &&
                                DateTime.TryParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementPeriodTo))
                            {
                                response.Add("Statementperiodfrom", statementPeriodFrom.ToString("d MMM yyyy"));
                                response.Add("Statementperiodto", statementPeriodTo.ToString("d MMM yyyy"));
                            }
                            else
                            {
                                // If parsing fails, add the original value
                                response.Add("Statementperiod", statementPeriod);
                            }
                        }
                        else
                        {
                            // If parsing fails, add the original value
                            response.Add("Statementperiod", statementPeriod);
                        }
                    }
                }




                else if (field != "Bankaddress1" && field != "Bankaddress2" && field != "Bankaddress3" && field != "Address1" && field != "Address2" && field != "Address3" && field != "Address4")
                {
                    response.Add(field, entity.GetType().GetProperty(field)?.GetValue(entity));
                }
            }

            return response;
        }


        private void ConvertDatesToStandardFormat(object bankDetails)
        {
            if (bankDetails is IDictionary<string, object> details)
            {
                foreach (var key in details.Keys.ToList())
                {
                    if (details[key] is DateTime date)
                    {
                        details[key] = date.ToString("dd-MM-yyyy");
                    }
                    else if (details[key] is string dateStr)
                    {
                        DateTime parsedDate;
                        if (DateTime.TryParse(dateStr, out parsedDate))
                        {
                            details[key] = parsedDate.ToString("dd-MM-yyyy");
                        }
                    }
                }
            }
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
            // Retrieve the file path based on the docid
            var filePath = _ratingContext.Loadedfiles
                            .Where(f => f.Id == docid)
                            .Select(f => f.Docname)
                            .FirstOrDefault();

            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound($"File not found for docId: {docid}");
            }

            // Check the file extension
            var fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".xml")
            {
                // Call ProcessXml method
                return await ProcessXml(docid);
            }
            else if (fileExtension == ".pdf")
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
                            var responseContent = await response.Content.ReadAsStringAsync();

                            //// Parse the response content to check for pdf_path
                            //var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                            //if (responseObject != null && responseObject.ContainsKey("pdf_path"))
                            //{
                            //    string pdfPath = responseObject["pdf_path"].ToString();
                            //    return await ProcessXml(docid);
                            //}

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



                            if (bankDetails == null || !bankDetails.Any())
                            {
                                //return NotFound();
                                return new JsonResult(new { code = "0", message = "Process failed, No  data found", status = "Failure" });
                            }

                            ConvertDatesToStandardFormat(bankDetails);

                            //return Ok(bankDetails);

                            return new JsonResult(new
                            {
                                code = "1",
                                extracted_Values = bankDetails,
                                message = "Documents processing completed",
                                status = "Success"
                            });
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
                            //return StatusCode((int)response.StatusCode, responseContent);
                            return new JsonResult(new
                            {
                                code = "0",
                                message = responseContent,
                                status = "Failure"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        //_logger.LogError($"Exception occurred: {ex.Message}");
                        return new JsonResult(new { code = "0", message = ex.Message, status = "Failure" });
                    }
                }

                return new JsonResult(new { code = "0", message = "Unexpected error occurred, please pass the correct docid", status = "Failure" });
            }
            {
                return new JsonResult(new { code = "0", message = "Unsupported file type. Only .xml and .pdf are supported.", status = "Failure" });
            }

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

        private void ConvertDatesToStandardFormat(List<object> bankDetails)
        {
            foreach (var obj in bankDetails)
            {
                if (obj is IDictionary<string, object> transaction)
                {
                    if (transaction.ContainsKey("TxnDate") && transaction["TxnDate"] is DateTime txnDate)
                    {
                        // Convert TxnDate to the desired format
                        transaction["TxnDate"] = txnDate.ToString("dd-MM-yyyy");
                    }
                    if (transaction.ContainsKey("ValueDate") && transaction["ValueDate"] is string valueDate)
                    {
                        transaction["ValueDate"] = ConvertToStandardDateFormat(valueDate);
                    }
                }
            }
        }

        // Function to convert date string to standard format
        private string ConvertToStandardDateFormat(string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(date, new string[] { "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy", "d MMM yyyy", "dd/MM/yy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("dd-MM-yyyy");
            }
            return date; // Return original string if unable to parse
        }


        private async Task<ActionResult> ProcessXml(int docid)
        {
            try
            {
                // Retrieve the file path based on the docid
                var filePath = _ratingContext.Loadedfiles
                                .Where(f => f.Id == docid)
                                .Select(f => f.Docname)
                                .FirstOrDefault();

                if (string.IsNullOrEmpty(filePath))
                {
                    return NotFound("Document not found.");
                }

                // Ensure the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                var existingData = await _ratingContext.ExtractTransactionData
                .Where(e => e.Docid == docid)
               .Select(e => new
              {
                   TxnDate = e.TxnDate.HasValue ? e.TxnDate.Value.ToString("dd-MM-yyyy") : null,
                   ValueDate = !string.IsNullOrEmpty(e.ValueDate) ? DateTime.Parse(e.ValueDate).ToString("dd-MM-yyyy") : null,
                   Description = e.Description,
                   Mode = e.Mode,
                   Credit = FormatDecimal(e.Credit),
                   Debit = FormatDecimal(e.Debit),
                   Balance = FormatDecimal(e.Balance),
                   TxnId = e.TransactionId
               })
           .ToListAsync();

                if (existingData.Any())
                {
                    // Return the existing data
                    var response = new
                    {
                        code = "1",
                        extracted_Values = existingData,
                        message = "Data retrieved from the database",
                        status = "Success"
                    };

                    return new JsonResult(response);
                }


                // Read and process the file
                var xmlContent = System.IO.File.ReadAllText(filePath);
                XmlProcessor processor = new XmlProcessor();
                XmlProcessorResult result = processor.ProcessXml(xmlContent);

                foreach (var detail in result.TransactionDetails)
                {
                    var entity = new Rating_Models.ExtractTransactionDatum
                    {
                        Docid = docid,
                        Credit = detail.TxnType == "CREDIT" ? detail.Amount.ToString() : string.Empty,
                        Debit = detail.TxnType == "DEBIT" ? detail.Amount.ToString() : string.Empty,
                        Balance = detail.CurrentBalance.ToString(),
                        Description = detail.Narration,
                        ChequeNumber = detail.Reference,
                        TxnDate = !string.IsNullOrEmpty(detail.TransactionTimestamp) ?
                                  DateTime.Parse(detail.TransactionTimestamp) : (DateTime?)null,
                        ValueDate = detail.Valuedate,
                        TransactionId = detail.Txnid,
                        Mode = detail.Mode,
                        Type = detail.TxnType
                    };
                    _ratingContext.ExtractTransactionData.Add(entity);
                }

                _ratingContext.SaveChanges();
                //return Ok(result);
                //return new JsonResult(new
                //{
                //    code = "1",
                //    Extracted_Values = result.TransactionDetails,
                //    message = "Documents processing completed",
                //    status = "Success"
                //});

                return new JsonResult(new
                {
                    code = "1",
                    extracted_Values = result.TransactionDetails?.Any() == true
                     ? result.TransactionDetails.Select(detail => (object)new
                     {
                        //amount = detail.Amount.ToString("0.00"),
                        TxnDate = !string.IsNullOrEmpty(detail.TransactionTimestamp)
                            ? DateTime.Parse(detail.TransactionTimestamp).ToString("dd-MM-yyyy")
                            : null,
                        ValueDate = !string.IsNullOrEmpty(detail.Valuedate)
                            ? DateTime.Parse(detail.Valuedate).ToString("dd-MM-yyyy")
                            : null,
                        Description = detail.Narration,
                        Mode = detail.Mode,
                        Credit = detail.TxnType == "CREDIT" ? detail.Amount.ToString("0.00") : string.Empty,
                        Debit = detail.TxnType == "DEBIT" ? detail.Amount.ToString("0.00") : string.Empty,// Ensure two decimal places
                        Balance = detail.CurrentBalance.ToString("0.00"), // Ensure two decimal places
                        TxnId = detail.Txnid
                        
                    }).ToList<object>()
                      :new List<object>(),
                    message = result.TransactionDetails?.Any() == true ? "Documents processing completed" : "No transactions found",
                    status = "Success"
                });

            }
            catch (Exception ex)
            {
                // Handle exceptions, log, or throw as needed
                Console.WriteLine($"Error processing XML: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private static string? FormatDecimal(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return decimal.TryParse(value, out var result) ? result.ToString("0.00") : value;
        }




        [Authorize]
        [HttpPost("AddNewLoanDetails")]
        public async Task<ActionResult> InsertLoanDetails(LoanDetailsMod req)
        {
            try
            {
                if (req != null)
                {

                    if (await _context.Loandetails.AnyAsync(u => u.Applno == req.Applno))
                    {
                        //return Conflict("LoanDetails with Same Applno already exists in Database.");

                        return new JsonResult(new { code = "0", message = "LoanDetails with Same Applno already exists in Database.", status = "Failure" });
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
                return new JsonResult(new { code = "1", message = "LoanDetails Inserted Successfully.", status = "Success" });
            }


            catch (Exception ex)
            {
                return new JsonResult(new { code = "0", message = ex.Message, status = "Failure" });
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

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("Error In Calculating Rating engine: Income must be greater than 0"))
                    {
                        var loanDetails = await _context.Loandetails
                   .FirstOrDefaultAsync(e => e.Applno == applno);

                        if (loanDetails == null)
                        {
                            return NotFound();
                        }

                        loanDetails.Rating = 0;
                        loanDetails.Income = 0;
                        loanDetails.Expenses = 0;

                        // Convert any DateTime properties to UTC
                        if (loanDetails.Approvaldate.HasValue)
                            loanDetails.Approvaldate = DateTime.SpecifyKind(loanDetails.Approvaldate.Value, DateTimeKind.Utc);
                        if (loanDetails.Disbdate.HasValue)
                            loanDetails.Disbdate = DateTime.SpecifyKind(loanDetails.Disbdate.Value, DateTimeKind.Utc);
                        if (loanDetails.Emistartdate.HasValue)
                            loanDetails.Emistartdate = DateTime.SpecifyKind(loanDetails.Emistartdate.Value, DateTimeKind.Utc);

                        // Save changes to the PostgreSQL database
                        _context.Update(loanDetails);
                        await _context.SaveChangesAsync();
                        

                        var sqlServerEntity = await _docubotDbContext.LoanDetailsDemo
                            .FirstOrDefaultAsync(e => e.Applno == applno);

                        if (sqlServerEntity != null)
                        {
                            sqlServerEntity.Rating = 0;
                            sqlServerEntity.Income = 0;
                            sqlServerEntity.Expenses = 0;

                            _docubotDbContext.Update(sqlServerEntity);
                            await _docubotDbContext.SaveChangesAsync();
                        }

                        LoanDetails lndet = db.GetLoanDetails(applno);

                        return new JsonResult(new
                        {


                            code = "0",
                            message = "Your Application did not match the criteria due to income being 0",
                            loandetails = new InsertLoanDetailsReq
                            {
                                Applno = lndet.Applno,
                                //lndet.Loantypeid,
                                Loanamt = lndet.Loanamt,
                                Emi = lndet.Emi,
                                Assetval = lndet.Assetval,
                                Tenor = lndet.Tenor,
                                //lndet.Appid,
                                Approvaldate = lndet.Approvaldate,
                                Disbdate = lndet.Disbdate,
                                //lndet.Status,
                                Owncontrib = lndet.Owncontrib,
                                //lndet.Intrate,
                                //lndet.Loanacno,
                                Loantype = lndet.Loantype,
                                Income = lndet.Income,
                                Permth = lndet.Permth,
                                Taxpaid = lndet.Taxpaid,
                                Rir = lndet.Rir,
                                Othemi = lndet.Othemi,
                                Lvr = lndet.Lvr,
                                Cibil = lndet.Cibil,
                                Bounced = lndet.Bounced,
                                Delayed = lndet.Delayed,
                                Custtype = lndet.Custtype,
                                Ccbal = lndet.Ccbal,
                                Emistartdate = lndet.Emistartdate,

                                Rating = lndet.Rating,
                                MaxRating = 900,
                                RatingCalculatedDate = DateTime.Now.ToString(),
                                Dependents = lndet.Dependents,
                                Expenses = lndet.Expenses
                            },
                            status = "Failure"
                        });
                    }


                    if (response.IsSuccessStatusCode)

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

                        if (loanDetails.Rating > 400)
                        {


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
                                            //var ratingCalcObject = JsonConvert.DeserializeObject<RatingCalculationContainer>(lndet.RatingCalc);
                                            return new JsonResult(new
                                            {
                                                code = "1",
                                                loandetails = new InsertLoanDetailsReq
                                                {

                                                     Applno = lndet.Applno,
                                                    //lndet.Loantypeid,
                                                     Loanamt= lndet.Loanamt,
                                                    Emi= lndet.Emi,
                                                    Assetval = lndet.Assetval,
                                                    Tenor = lndet.Tenor,
                                                    //lndet.Appid,
                                                    Approvaldate = lndet.Approvaldate,
                                                    Disbdate = lndet.Disbdate,
                                                    //lndet.Status,
                                                    Owncontrib = lndet.Owncontrib,
                                                    //lndet.Intrate,
                                                    //lndet.Loanacno,
                                                    Loantype = lndet.Loantype,
                                                    Income = lndet.Income,
                                                    Permth = lndet.Permth,
                                                    Taxpaid = lndet.Taxpaid,
                                                    Rir = lndet.Rir,
                                                    Othemi = lndet.Othemi,
                                                    Lvr = lndet.Lvr,
                                                    Cibil = lndet.Cibil,
                                                    Bounced = lndet.Bounced,
                                                    Delayed = lndet.Delayed,
                                                    Custtype = lndet.Custtype,
                                                    Ccbal = lndet.Ccbal,
                                                    Emistartdate = lndet.Emistartdate,
                                                    Rating = lndet.Rating,
                                                    MaxRating = 900,
                                                    RatingCalculatedDate = DateTime.Now.ToString(),
                                                    Dependents = lndet.Dependents,
                                                    Expenses = lndet.Expenses
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
                                return new JsonResult(new { code = "0", message = "An error occurred while calculating Rating",
                                  status = "Failure" });
                            }
                            

                        }
                        //else if (response.statuscode == httpstatuscode.redirect || response.statuscode == httpstatuscode.temporaryredirect)
                        //{
                        //    // handle redirect by extracting the new location from headers
                        //    var redirecturl = response.headers.location;
                        //    // make a new request to the redirected url
                        //    response = await client.postasync(redirecturl, null);

                        //    if (response.issuccessstatuscode)
                        //    {
                        //        var responsecontent = await response.content.readasstringasync();
                        //        return ok(responsecontent);
                        //    }
                        //}
                        else
                        {
                            //var responseContent = await response.Content.ReadAsStringAsync();
                            //return StatusCode((int)response.StatusCode, responseContent);
                            //return Ok("Your Application did not match the criteria with low or negative rating");
                            LoanDetails lndet = db.GetLoanDetails(applno);
                            return new JsonResult(new { code = "0", message = "Your Application did not match the criteria with low or negative rating",
                                loandetails = new InsertLoanDetailsReq
                                {
                                    Applno = lndet.Applno,
                                    //lndet.Loantypeid,
                                    Loanamt = lndet.Loanamt,
                                    Emi = lndet.Emi,
                                    Assetval = lndet.Assetval,
                                    Tenor = lndet.Tenor,
                                    //lndet.Appid,
                                    Approvaldate = lndet.Approvaldate,
                                    Disbdate = lndet.Disbdate,
                                    //lndet.Status,
                                    Owncontrib = lndet.Owncontrib,
                                    //lndet.Intrate,
                                    //lndet.Loanacno,
                                    Loantype = lndet.Loantype,
                                    Income = lndet.Income,
                                    Permth = lndet.Permth,
                                    Taxpaid = lndet.Taxpaid,
                                    Rir = lndet.Rir,
                                    Othemi = lndet.Othemi,
                                    Lvr = lndet.Lvr,
                                    Cibil = lndet.Cibil,
                                    Bounced = lndet.Bounced,
                                    Delayed = lndet.Delayed,
                                    Custtype = lndet.Custtype,
                                    Ccbal = lndet.Ccbal,
                                    Emistartdate = lndet.Emistartdate,
                                    
                                    Rating = lndet.Rating,
                                    MaxRating = 900,
                                    RatingCalculatedDate = DateTime.Now.ToString(),
                                    Dependents = lndet.Dependents,
                                    Expenses = lndet.Expenses
                                },
                                status = "Failure" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    //_logger.LogError($"Exception occurred: {ex.Message}");
                    return new JsonResult(new { code = "0", message = "An error occurred while calculating Rating",
                        status = "Failure" });
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
