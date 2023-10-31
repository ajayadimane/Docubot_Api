namespace DocuBot_Api.Models
{
    public class CustomModel
    {

    }

    /*For UserProfile*/
    public class SignInDetails
    {
        public string Emailid { get; set; }
        public string Password { get; set; }
    }
    public class SignUpDetails
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Password { get; set; }
        public long? Phonenumber { get; set; }
        public string Emailid { get; set; }
        public string Jobtitle { get; set; }
        public string Companyname { get; set; }
        public string Companysize { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
    }
    public class EditProfDetails
    {
        public int Userid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Emailid { get; set; }
        public string Password { get; set; }
        public long? Phonenumber { get; set; }
        public string Jobtitle { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public string Role { get; set; }
    }

    /*For UserProfile*/

    /*For User Management*/

    public class InsertUserDetails
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public long? Phonenumber { get; set; }
        public string Emailid { get; set; }
    }

    public class UpdateUserDetails
    {
        public int UserID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public long? Phonenumber { get; set; }
        public string Emailid { get; set; }

    }
    /*For User Management*/

    /*To hold user details after login */
    public class UserLoginDetails
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
    }
    /*To hold user details after login */

    //public class TempAutoTrain
    //{
    //    public List<MdkeywordsTemp> MdkeywordsTemps { get; set; }
    //    public List<MdtableTemp> MdtableTemps { get; set; }
    //}
    public class PageProcess
    {
        public int Id { get; set; }
        public string Linedump { get; set; }
        public int Pgst { get; set; }
        public int Pgnd { get; set; }
        public int Docid { get; set; }
        public int Docpgno { get; set; }
        public string Indoc { get; set; }
        public int Docstpg { get; set; }
        public int Docndpg { get; set; }
        public int Docstlin { get; set; }
        public int Docndlin { get; set; }
    }
    /*For Files Extraction*/
    public class DocDetails
    {
        public int DocId { get; set; }
        public int DocTypeId { get; set; }
        public string DocName { get; set; }
        public string DocPath { get; set; }
    }
    /*For Files Extraction*/

    public class MDTableInput
    {
        public bool IsValidated { get; set; }
        public MDTablesDisp MDTable { get; set; }
        public string DocData { get; set; }
    }
    public class MDTableOutput
    {
        public bool IsValidated { get; set; }
        public List<MDTablesDisp> MDTable { get; set; }
        public string DocData { get; set; }
    }

    public class MDKeywordsInput
    {
        public bool IsValidated { get; set; }
        public MDKeywordsMod MDKeywords { get; set; }
        public string DocData { get; set; }
    }

    public class MDKeywordsOutput
    {
        public bool IsValidated { get; set; }
        public List<MDKeywordsMod> MDKeywords { get; set; }
        public string DocData { get; set; }
    }

    public class MDKeywordsMod
    {
        public int Id { get; set; }
        public int DoctypeID { get; set; }
        public string Keyword { get; set; }
        public string SearchStr { get; set; }
        public string ReturnKey { get; set; }
    }

    public class MDDoctypeMod
    {
        public bool IsValidated { get; set; }
        public int DoctypeId { get; set; }
        public string DoctypeName { get; set; }
        public int DocGroupSelected { get; set; }
        public string DocStart { get; set; }
        public string DocEnd { get; set; }
        public string DocData { get; set; }
    }

    public class MDTablesDisp
    {
        public int Id { get; set; }
        public int DocTypeID { get; set; }
        public string Header { get; set; }
        public string Returnkey { get; set; }
        public int DataStart { get; set; }
        public int DataEnd { get; set; }
        public string TableStart { get; set; }
        public string TableEnd { get; set; }
        public int Iskeyval { get; set; }
        public int Hdrextlinecnt { get; set; }
        public int TableCount { get; set; }
        public int ColumnCount { get; set; }
    }

    public class MDTablesMod
    {
        public int Id { get; set; }
        public int DocTypeID { get; set; }
        public string Header { get; set; }
        public int HeadStart { get; set; }
        public int HeadEnd { get; set; }
        public int DataStart { get; set; }
        public int DataEnd { get; set; }
        public string TableStart { get; set; }
        public string TableEnd { get; set; }
        public int Hdrextlinecnt { get; set; }
        public int Iskeyval { get; set; }
        public int ColumnCount { get; set; }
        public int TableCount { get; set; }
    }

    public class UserSelFields
    {
        public string ReturnKey { get; set; }
        public string Source { get; set; }
    }

    public class MstKeyValueFields
    {
        public string Keyword { get; set; }
        public string KeyValue { get; set; }
    }

    public class DocList
    {
        public int DocId { get; set; }
        public string DocName { get; set; }
        public string DocData { get; set; }
    }


    public class ReturnKeyList
    {
        public int? DocGroupId { get; set; }
        public string ReturnKey { get; set; }
        public string Src { get; set; }
    }

    public class DocDet
    {
        public int DocID { get; set; }
    }

    public class DocGroupDetails
    {
        public int DocGroupId { get; set; }
        public string DocGroupName { get; set; }
    }

    public class OutputTypeDetails
    {
        public int OutputTypeId { get; set; }
        public string OutputTypeFormat { get; set; }
    }

    public class DocGroupID
    {
        public int DocGroupId { get; set; }
    }

    public class ExtractDataResult
    {
        public string DocName { get; set; }
        public byte[] DocData { get; set; }

    }
    public class UpdateSelectedReturnKeys
    {
        public int OutputTypeId { get; set; }
        public List<SelectedReturnKeys> SelectedReturnKeys { get; set; }
    }
    public class SelectedReturnKeys
    {
        public int DocGroupId { get; set; }
        public string ReturnKey { get; set; }
        public string Src { get; set; }
    }

    public class DocTypeDetails
    {
        public int DoctypeID { get; set; }
        public string DoctypeName { get; set; }
        public bool IsValidated { get; set; }
    }

    public class MatchedFileDetails
    {
        public int Id { get; set; }
        public string Lines { get; set; }
    }

    public class NewDoctypeDetails
    {
        public string DocName { get; set; }
        public Dictionary<int, string> DocData { get; set; }
        public Dictionary<int, List<string>> DocDataList { get; set; }
        public Dictionary<int, List<string>> BinDocDataList { get; set; }
        public List<string> CommonLines { get; set; }
        public List<string> DocDataToCompare { get; set; }
        public string FirstDocData { get; set; }
        public string SecondDocData { get; set; }
        public List<string> FirstDocDataList { get; set; }
        public List<string> SecondDocDataList { get; set; }
        //public DocData DocDatas{ get; set; }        
        //public DocDataList DocDataLists { get; set; }
        //public BinDocDataList BinDocDataLists { get; set; }

    }


    public class DocData
    {
        public string DocData1 { get; set; }
        public string DocData2 { get; set; }
        public string DocData3 { get; set; }
        public string DocData4 { get; set; }
        public string DocData5 { get; set; }
        public string DocData6 { get; set; }
        public string DocData7 { get; set; }
        public string DocData8 { get; set; }
        public string DocData9 { get; set; }
        public string DocData10 { get; set; }
    }
    public class DocDataList
    {
        public List<string> DocDataList1 { get; set; }
        public List<string> DocDataList2 { get; set; }
        public List<string> DocDataList3 { get; set; }
        public List<string> DocDataList4 { get; set; }
        public List<string> DocDataList5 { get; set; }
        public List<string> DocDataList6 { get; set; }
        public List<string> DocDataList7 { get; set; }
        public List<string> DocDataList8 { get; set; }
        public List<string> DocDataList9 { get; set; }
        public List<string> DocDataList10 { get; set; }
    }

    public class BinDocDataList
    {
        public List<string> BinDocDataList1 { get; set; }
        public List<string> BinDocDataList2 { get; set; }
        public List<string> BinDocDataList3 { get; set; }
        public List<string> BinDocDataList4 { get; set; }
        public List<string> BinDocDataList5 { get; set; }
        public List<string> BinDocDataList6 { get; set; }
        public List<string> BinDocDataList7 { get; set; }
        public List<string> BinDocDataList8 { get; set; }
        public List<string> BinDocDataList9 { get; set; }
        public List<string> BinDocDataList10 { get; set; }
    }
    public class MetaData
    {
        public List<string> IgnoreLines { get; set; }
        public List<MDKeywordsMod> KeyValMetaData { get; set; }
        public List<MDTablesMod> DetailsData { get; set; }
    }

    public class WordList
    {
        public string Keyword { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
    }

    public class DocIDList
    {
        public List<int> ListofDocID { get; set; }
    }
    //Training Models
    public class DocTypeStatus
    {
        public bool IsValidated { get; set; }
    }
    public class DocType
    {
        public int DoctypeID { get; set; }
        public bool IsValidated { get; set; }
    }

    public class MDTableDelID
    {
        public List<int> Id { get; set; }
        public bool IsValidated { get; set; }
    }

    public class MDKeywordDelID
    {
        public List<int> Id { get; set; }
        public bool IsValidated { get; set; }
    }

    public class UniqueDocsToTrain
    {
        public int UniqueDocID { get; set; }
        public int Count { get; set; }
        public string DocName { get; set; }

    }

    public class RequiredDataOutput
    {
        public string DocGroupName { get; set; }
        public int OutputType { get; set; }
        public List<ReturnKey> ReturnKeys { get; set; }
    }
    public class ReturnKey
    {
        public Dictionary<string, List<string>> ReturnKeyName { get; set; }
        //public List<string> ActualKeyword { get; set; }
        public string Source { get; set; }
    }

    public class RequiredDataValues
    {
        public string DocGroupName { get; set; }
        public int OutputType { get; set; }
        public List<DataValues> DataValues { get; set; }
    }
    public class DataValues
    {
        public string ReturnKey { get; set; }
        public List<string> Values { get; set; }
    }

    public class ReportDateRange
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class AutoTrainFileTbl
    {
        public int LineNo { get; set; }
        public string Words { get; set; }
        public int WordPosition { get; set; }
        public int WordStartIndex { get; set; }
    }

    public class TotalProcUnProc
    {
        public DateTime BatchDate { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalUnProcessed { get; set; }

    }

    //public class ExtKVTBLData
    //{
    //    public List<Mdkeyword> Keywords { get; set; }
    //    public List<Mdtable> Tables { get; set; }
    //}


    public class UnProcessMatched
    {
        public int Groupid { get; set; }
        public string Keyword { get; set; }
    }
}
