using DMSWebAI.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public LedgerController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<OutstandingHomeController>
        [HttpGet]
        public IActionResult Get([FromQuery] string Quarter, int year)
        {
            List<Ledgerstatement> statements = new List<Ledgerstatement>();
            string connString = this.Configuration.GetConnectionString("DMS");
            string condition = string.Empty;
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            string sql = string.Empty;
            if (AuthControl == 0)
                return StatusCode(700, "You don't have access");
            else if (AuthControl == 1)
            {
                if (string.IsNullOrEmpty(BPCode))
                    return StatusCode(700, "You account is not associate with business patner");
                //check here BPCode is EM type or SP type
                string ActGroupID = string.Empty;
                try
                {
                    connection.Open();
                    sql = "select ActGroupID from t_business_partner where BPCode = '" + BPCode + "' and CompCode = '" + CompanyID + "'";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        ActGroupID = rdr["ActGroupID"].ToString();
                }
                catch (Exception) { return StatusCode(700, "Invalid Business Patner"); }
                finally { connection.Close(); }
                //if it is EM type
                if (ActGroupID == "EM")
                {
                    condition = "WITH RECURSIVE subordinate AS ( " +
                              "    SELECT  BP.BPCODE, BD.SeniorBPCode " +
                              "    FROM t_business_partner as BP left join t_bp_designation BD on BP.BPCode = BD.BPCode and BP.CompCode = BD.CompCode " +
                              "    WHERE BP.BPCODE = '" + BPCode + "' and BP.CompCode = '" + CompanyID + "'" +
                              "     UNION ALL " +
                              "     SELECT  e.BPCODE, d.SeniorBPCode " +
                              "    FROM t_bp_designation e left join t_bp_designation d on e.BPCode = d.BPCode and e.CompCode = d.CompCode " +
                              "JOIN subordinate s ON e.SeniorBPCode = s.BPCODE " +
                              ") select BPCode from t_bp_relation where RelationBPCode in (select BPCODE from subordinate) and CompCode = '" + CompanyID + "'";
                    condition = " and OS.Account in ( " + condition + ")";
                }
                else if (ActGroupID == "SP")//if it is SP type
                {
                    condition = " and OS.Account in ( '" + BPCode + "')";
                }
                else
                    return StatusCode(700, "You don't have access");
            }
            else
                condition = "";
            try
            {
                connection.Open();
                sql = "select ledgerID, Account, " +
                      "concat(if (BP.BPFName is null, '', BP.BPFName), if (BP.BPMName is null, '', BP.BPMName), if (BP.BPLName is null, '', BP.BPLName)) as Customer, " +
                      "City, RegionID " +
                      "from t_ledger_statement as OS " +
                      "left join t_business_partner BP on OS.Account = BP.BPCode and OS.CompCode = BP.CompCode " +
                      "where OS.Quarter = '" + Quarter + "' and OS.Year = " + year + " and OS.CompCode = '" + CompanyID + "'" + condition;
                //Role Base Selecton
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    Ledgerstatement st = new Ledgerstatement();
                    st.ledgerID = Convert.ToInt32(rdr["ledgerID"]);
                    st.Account = rdr["Account"].ToString();
                    st.Customer = rdr["Customer"].ToString();
                    st.City = rdr["City"].ToString();
                    st.RegionID = rdr["RegionID"].ToString();
                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    MySqlDataReader Subrdr;
                    sql = "select Assignment, BillDoc, Reference, PostingKey, DocumentNo, DocType, DocTypeDesc, DocDate, PostingDate, Amount, Debit, Credit, Currency, ClearingDoc, ClearingDate, Text, Period, Sequence from t_ledger where ledgerID = " + st.ledgerID + " order by Sequence";
                    Subconnection.Open();
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    st.ledgers = new List<ledger>();
                    while (Subrdr.Read())
                    {
                        ledger os = new ledger();
                        os.BillDoc = Subrdr["Assignment"].ToString();
                        os.BillDoc = Subrdr["BillDoc"].ToString();
                        //if (Subrdr["DocDate"] != DBNull.Value)
                        //    os.DocDate = Convert.ToDateTime(Subrdr["DocDate"]);
                        if (Subrdr["Reference"] != DBNull.Value)
                            os.Reference = Convert.ToString(Subrdr["Reference"]);
                        if (Subrdr["PostingKey"] != DBNull.Value)
                            os.PostingKey = Convert.ToString(Subrdr["PostingKey"]);
                        if (Subrdr["DocumentNo"] != DBNull.Value)
                            os.DocumentNo = Convert.ToString(Subrdr["DocumentNo"]);
                        if (Subrdr["DocType"] != DBNull.Value)
                            os.DocType = Convert.ToString(Subrdr["DocType"]);
                        if (Subrdr["DocTypeDesc"] != DBNull.Value)
                            os.DocTypeDesc = Convert.ToString(Subrdr["DocTypeDesc"]);
                        if (Subrdr["DocDate"] != DBNull.Value)
                            os.DocDate = Convert.ToDateTime(Subrdr["DocDate"]);
                        if (Subrdr["PostingDate"] != DBNull.Value)
                            os.PostingDate = Convert.ToDateTime(Subrdr["PostingDate"]);
                        if (Subrdr["Amount"] != DBNull.Value)
                            os.Amount = Convert.ToDecimal(Subrdr["Amount"]);
                        if (Subrdr["Debit"] != DBNull.Value)
                            os.Debit = Convert.ToDecimal(Subrdr["Debit"]);
                        if (Subrdr["Credit"] != DBNull.Value)
                            os.Credit = Convert.ToDecimal(Subrdr["Credit"]);
                        if (Subrdr["Currency"] != DBNull.Value)
                            os.Currency = Convert.ToString(Subrdr["Currency"]);
                        if (Subrdr["ClearingDoc"] != DBNull.Value)
                            os.ClearingDoc = Convert.ToString(Subrdr["ClearingDoc"]);
                        if (Subrdr["ClearingDate"] != DBNull.Value)
                            os.ClearingDate = Convert.ToDateTime(Subrdr["ClearingDate"]);
                        os.Text = Subrdr["Text"].ToString();
                        if (Subrdr["Period"] != DBNull.Value)
                            os.Period = Convert.ToString(Subrdr["Period"]);

                        st.ledgers.Add(os);
                    }
                    statements.Add(st);
                    Subrdr.Close();
                    Subconnection.Close();

                }
                rdr.Close();
                return Ok(statements);
            }
            catch (MySqlException ex)
            {
                return StatusCode(600, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        // POST api/<LedgerHomeController>
        [HttpPost]
        public IActionResult Post([FromQuery] string Quarter, int year)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction myTrans = null;
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            IFormFile formFile = Request.Form.Files[0];
            if (formFile == null || formFile.Length <= 0)
            {
                return StatusCode(700, "Empty excel file");
            }
            string fileExt = Path.GetExtension(formFile.FileName);
            try
            {
                using MemoryStream stream = new MemoryStream();
                formFile.CopyTo(stream);
                stream.Position = 0;
                ISheet worksheet = null;
                if (fileExt == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //HSSWorkBook object will read the Excel 97-2000 formats  
                    worksheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook  
                }
                else if (fileExt == ".xlsx")
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //XSSFWorkBook will read 2007 Excel format  
                    worksheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook   
                }
                else
                {
                    return StatusCode(700, "Not Support file extension");
                }
                DataTable dtDetals = new DataTable();
                DataColumn dcName;
                dcName = new DataColumn("Account", typeof(string)); dtDetals.Columns.Add(dcName);
                //dcName = new DataColumn("Customer", typeof(string)); dtDetals.Columns.Add(dcName);
                //dcName = new DataColumn("City", typeof(string)); dtDetals.Columns.Add(dcName);
                //dcName = new DataColumn("Zone", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Assignment", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("BillDoc", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Reference", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("PostingKey", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DocumentNo", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DocType", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DocTypeDesc", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DocDate", typeof(DateTime)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("PostingDate", typeof(DateTime)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Amount", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Debit", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Credit", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Currency", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("ClearingDoc", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("ClearingDate", typeof(DateTime)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Text", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Period", typeof(string)); dtDetals.Columns.Add(dcName);


                string Account = string.Empty;
                //string Customer = string.Empty;
                //string City = string.Empty;
                //string Zone = string.Empty;
                string Assignment = string.Empty;
                string BillDoc = string.Empty;
                string Reference = string.Empty;
                string PostingKey = string.Empty;
                string DocumentNo = string.Empty;
                string DocType = string.Empty;
                string DocTypeDesc = string.Empty;
                string DocDate = string.Empty;
                string PostingDate = string.Empty;
                string Amount = string.Empty;
                string Debit = string.Empty;
                string Credit = string.Empty;
                string Currency = string.Empty;
                string ClearingDoc = string.Empty;
                string ClearingDate = string.Empty;
                string Text = string.Empty;
                string Period = string.Empty;

                DataFormatter formatter = new DataFormatter();
                for (int row = 0; row <= worksheet.LastRowNum; row++) //Loop the records upto filled row  
                {
                    if (worksheet.GetRow(row) != null) //null is when the row only contains empty cells   
                    {

                        Account = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(0));
                        //Customer = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(1));
                        //City = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(2));
                        //Zone = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(3));
                        Assignment = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(4));
                        BillDoc = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(5));
                        Reference = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(6));
                        PostingKey = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(7));
                        DocumentNo = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(8));
                        DocType = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(9));
                        DocTypeDesc = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(10));
                        DocDate = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(11));
                        PostingDate = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(12));
                        Amount = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(13));
                        Debit = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(14));
                        Credit = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(15));
                        Currency = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(16));
                        ClearingDoc = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(17));
                        ClearingDate = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(18));
                        Text = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(19));
                        Period = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(20));


                        if (row.Equals(0))
                        {

                            continue;
                        }
                        DataRow ldr = dtDetals.NewRow();
                        ldr["Account"] = Account;
                        //ldr["Customer"] = Customer;
                        //ldr["City"] = City;
                        //ldr["Zone"] = Zone;
                        ldr["Assignment"] = Assignment;
                        ldr["BillDoc"] = BillDoc;
                        ldr["Reference"] = Reference;
                        ldr["PostingKey"] = PostingKey;
                        ldr["DocumentNo"] = DocumentNo;
                        ldr["DocType"] = DocType;
                        ldr["DocTypeDesc"] = DocTypeDesc;
                        if (!string.IsNullOrEmpty(DocDate))
                            ldr["DocDate"] = Convert.ToDateTime(DocDate);
                        if (!string.IsNullOrEmpty(PostingDate))
                            ldr["PostingDate"] = Convert.ToDateTime(PostingDate);
                        if (!string.IsNullOrEmpty(Amount))
                        {
                            if (Amount.StartsWith("(") && Amount.EndsWith(")"))
                                Amount = "-" + Amount.Substring(1, Amount.Length - 2);
                            ldr["Amount"] = Convert.ToDecimal(Amount);
                        }
                        if (!string.IsNullOrEmpty(Debit))
                        {
                            if (Debit.StartsWith("(") && Debit.EndsWith(")"))
                                Debit = "-" + Debit.Substring(1, Debit.Length - 2);
                            ldr["Debit"] = Convert.ToDecimal(Debit);
                        }
                        if (!string.IsNullOrEmpty(Credit))
                        {
                            if (Credit.StartsWith("(") && Credit.EndsWith(")"))
                                Credit = "-" + Credit.Substring(1, Credit.Length - 2);
                            ldr["Credit"] = Convert.ToDecimal(Credit);
                        }
                        ldr["Currency"] = Currency;
                        ldr["ClearingDoc"] = ClearingDoc;
                        if (!string.IsNullOrEmpty(ClearingDate))
                            ldr["ClearingDate"] = Convert.ToDateTime(ClearingDate);
                        ldr["Text"] = Text;
                        if(!string.IsNullOrEmpty(Period))
                            ldr["Period"] = Convert.ToInt32(Period);
                        dtDetals.Rows.Add(ldr);
                    }
                 }

                DataView dv = new DataView(dtDetals);
                dv.Sort = "Account";
                dtDetals = dv.ToTable();

                var result = from rows in dtDetals.AsEnumerable()
                             group rows by new { Account = rows["Account"] } into grp
                             select grp;
                connection.Open();
                myTrans = connection.BeginTransaction();
                foreach (var bcode in result)

                {
                    string sql = "insert into t_ledger_statement (CompCode, Account, Quarter, Year) values " +
                                 "(@CompCode, @Account, @Quarter, @Year)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    string bpcode = bcode.Key.Account.ToString();
                    cmd.Parameters.Add(new MySqlParameter("@Account", MySqlDbType.VarChar)).Value = bpcode;
                    cmd.Parameters.Add(new MySqlParameter("@Quarter", MySqlDbType.VarChar)).Value = Quarter;
                    cmd.Parameters.Add(new MySqlParameter("@Year", MySqlDbType.Int32)).Value = year;
                    cmd.ExecuteScalar();
                    int id = (int)cmd.LastInsertedId;
                    DataTable dt = new DataTable();
                    dcName = new DataColumn("Assignment", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("BillDoc", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Reference", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("PostingKey", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DocumentNo", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DocType", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DocTypeDesc", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DocDate", typeof(DateTime)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("PostingDate", typeof(DateTime)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Amount", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Debit", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Credit", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Currency", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("ClearingDoc", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("ClearingDate", typeof(DateTime)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Text", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Period", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("ledgerID", typeof(int)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Sequence", typeof(int)); dt.Columns.Add(dcName);
                    //DataTable A = bcode.CopyToDataTable();
                    int Sequence = 0;
                    foreach (DataRow rw in bcode.CopyToDataTable().Rows)
                    {
                        DataRow ldr = dt.NewRow();
                        ldr["Assignment"] = rw["Assignment"];
                        ldr["BillDoc"] = rw["BillDoc"];
                        ldr["Reference"] = rw["Reference"];
                        ldr["PostingKey"] = rw["PostingKey"];
                        ldr["DocumentNo"] = rw["DocumentNo"];
                        ldr["DocType"] = rw["DocType"];
                        ldr["DocTypeDesc"] = rw["DocTypeDesc"];
                        ldr["DocDate"] = rw["DocDate"];
                        ldr["PostingDate"] = rw["PostingDate"];
                        ldr["Amount"] = rw["Amount"];
                        ldr["Debit"] = rw["Debit"];
                        ldr["Credit"] = rw["Credit"];
                        ldr["Currency"] = rw["Currency"];
                        ldr["ClearingDoc"] = rw["ClearingDoc"];
                        ldr["ClearingDate"] = rw["ClearingDate"];
                        ldr["Text"] = rw["Text"];
                        ldr["Period"] = rw["Period"];
                        ldr["ledgerID"] = id;
                        ldr["Sequence"] = Sequence++;
                        dt.Rows.Add(ldr);
                    }



                    cmd = new MySqlCommand("BulkUploadLedger", connection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        Transaction = myTrans,
                        UpdatedRowSource = UpdateRowSource.None
                    };
                    cmd.Parameters.Add("Assignment", MySqlDbType.String).SourceColumn = "Assignment";
                    cmd.Parameters.Add("BillDoc", MySqlDbType.String).SourceColumn = "BillDoc";
                    cmd.Parameters.Add("Reference", MySqlDbType.String).SourceColumn = "Reference";
                    cmd.Parameters.Add("PostingKey", MySqlDbType.String).SourceColumn = "PostingKey";
                    cmd.Parameters.Add("DocumentNo", MySqlDbType.String).SourceColumn = "DocumentNo";
                    cmd.Parameters.Add("DocType", MySqlDbType.String).SourceColumn = "DocType";
                    cmd.Parameters.Add("DocTypeDesc", MySqlDbType.String).SourceColumn = "DocTypeDesc";
                    cmd.Parameters.Add("DocDate", MySqlDbType.Date).SourceColumn = "DocDate";
                    cmd.Parameters.Add("PostingDate", MySqlDbType.Date).SourceColumn = "PostingDate";
                    cmd.Parameters.Add("Amount", MySqlDbType.Decimal).SourceColumn = "Amount";
                    cmd.Parameters.Add("Debit", MySqlDbType.Decimal).SourceColumn = "Debit";
                    cmd.Parameters.Add("Credit", MySqlDbType.Decimal).SourceColumn = "Credit";
                    cmd.Parameters.Add("Currency", MySqlDbType.String).SourceColumn = "Currency";
                    cmd.Parameters.Add("ClearingDoc", MySqlDbType.String).SourceColumn = "ClearingDoc";
                    cmd.Parameters.Add("ClearingDate", MySqlDbType.Date).SourceColumn = "ClearingDate";
                    cmd.Parameters.Add("Text", MySqlDbType.String).SourceColumn = "Text";
                    cmd.Parameters.Add("Period", MySqlDbType.Int32).SourceColumn = "Period";
                    cmd.Parameters.Add("ledgerID", MySqlDbType.Int32).SourceColumn = "ledgerID";
                    cmd.Parameters.Add("Sequence", MySqlDbType.Int32).SourceColumn = "Sequence";
                    MySqlDataAdapter da = new MySqlDataAdapter()
                    { InsertCommand = cmd };
                    int records = da.Update(dt);
                }
                myTrans.Commit();
            
            }
            catch (MySqlException ex)
            {
                try { myTrans.Rollback(); } catch { }
                return StatusCode(600, ex.Message);
            }
            catch (Exception ex)
            {
                try { myTrans.Rollback(); } catch { }
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return Ok("Record uploaded successfuly");
        }
    }
}
