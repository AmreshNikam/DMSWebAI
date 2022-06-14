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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DMSWebAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutstandingController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public OutstandingController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<OutstandingHomeController>
        [HttpGet]
        public IActionResult Get([FromQuery] int month,int year)
        {
            List<OutstandingStatement> statements = new List<OutstandingStatement>();
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
                sql = "select OutstandingID, Account, " +
                      "concat(if (BP.BPFName is null, '', BP.BPFName), if (BP.BPMName is null, '', BP.BPMName), if (BP.BPLName is null, '', BP.BPLName)) as Customer, " +
                      "City, RegionID " +
                      "from t_outstanding_statement as OS " +
                      "left join t_business_partner BP on OS.Account = BP.BPCode and OS.CompCode = BP.CompCode " +
                      "where OS.Month = " + month + " and OS.Year = " + year + " and OS.CompCode = '" + CompanyID + "'" + condition;
                //Role Base Selecton
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    
                    OutstandingStatement st = new OutstandingStatement();
                    st.OutstandingID = Convert.ToInt32(rdr["OutstandingID"]);
                    st.Account = rdr["Account"].ToString();
                    st.Customer = rdr["Customer"].ToString();
                    st.City = rdr["City"].ToString();
                    st.RegionID = rdr["RegionID"].ToString();
                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    MySqlDataReader Subrdr;
                    sql = "select BillDoc, DocDate, Net_due_dt, GrandTotal, DueMonthPluseOne, DueMonth, D0_10, D11_30, D31_60, D61_90, DG91, ON_Account, Comments from t_outstanding where OutstandingID =" + st.OutstandingID;
                    Subconnection.Open();
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    st.outstandings = new List<Outstanding>();
                    while (Subrdr.Read())
                    {
                        Outstanding os = new Outstanding();
                        os.BillDoc = Subrdr["BillDoc"].ToString();
                        if(Subrdr["DocDate"] != DBNull.Value)
                            os.DocDate = Convert.ToDateTime(Subrdr["DocDate"]);
                        if(Subrdr["Net_due_dt"] != DBNull.Value)
                            os.Net_due_dt = Convert.ToDateTime(Subrdr["Net_due_dt"]);
                        if(Subrdr["GrandTotal"] !=DBNull.Value)
                            os.GrandTotal = Convert.ToDecimal(Subrdr["GrandTotal"]);
                        if(Subrdr["DueMonthPluseOne"] != DBNull.Value)
                            os.DueMonthPluseOne = Convert.ToDecimal(Subrdr["DueMonthPluseOne"]);
                        if(Subrdr["DueMonth"] != DBNull.Value)
                            os.DueMonth = Convert.ToDecimal(Subrdr["DueMonth"]);
                        if(Subrdr["D0_10"] !=DBNull.Value)
                            os.D0_10 = Convert.ToDecimal(Subrdr["D0_10"]);
                        if(Subrdr["D11_30"] !=DBNull.Value)
                            os.D11_30 = Convert.ToDecimal(Subrdr["D11_30"]);
                        if(Subrdr["D31_60"] !=DBNull.Value)
                            os.D31_60 = Convert.ToDecimal(Subrdr["D31_60"]);
                        if(Subrdr["D61_90"] != DBNull.Value)
                            os.D61_90 = Convert.ToDecimal(Subrdr["D61_90"]);
                        if(Subrdr["DG91"] != DBNull.Value)
                            os.DG91 = Convert.ToDecimal(Subrdr["DG91"]);
                        if(Subrdr["ON_Account"] != DBNull.Value)
                            os.ON_Account = Convert.ToDecimal(Subrdr["ON_Account"]);
                        os.Comments = Subrdr["Comments"].ToString();
                        st.outstandings.Add(os);
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
        // POST api/<OutstandingHomeController>
        [HttpPost]
        public IActionResult Post([FromQuery] int month, int year)
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
                dcName = new DataColumn("BillDoc", typeof(string)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DocDate", typeof(DateTime)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Net_due_dt", typeof(DateTime)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("GrandTotal", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DueMonthPluseOne", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DueMonth", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("D0_10", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("D11_30", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("D31_60", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("D61_90", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("DG91", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("ON_Account", typeof(decimal)); dtDetals.Columns.Add(dcName);
                dcName = new DataColumn("Comments", typeof(string)); dtDetals.Columns.Add(dcName);

                string Account = string.Empty;
                //string Customer = string.Empty;
                //string City = string.Empty;
                //string Zone = string.Empty;
                string BillDoc = string.Empty;
                string DocDate = string.Empty;
                string Net_due_dt = string.Empty;
                string GrandTotal = string.Empty;
                string DueMonthPluseOne = string.Empty;
                string DueMonth = string.Empty;
                string D0_10 = string.Empty;
                string D11_30 = string.Empty;
                string D31_60 = string.Empty;
                string D61_90 = string.Empty;
                string DG91 = string.Empty;
                string ON_Account = string.Empty;
                string Comments = string.Empty;

                DataFormatter formatter = new DataFormatter();
                for (int row = 0; row <= worksheet.LastRowNum; row++) //Loop the records upto filled row  
                {
                    if (worksheet.GetRow(row) != null) //null is when the row only contains empty cells   
                    {

                        Account = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(0));
                        //Customer = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(1));
                        //City = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(2));
                        //Zone = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(3));
                        BillDoc = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(4));
                        DocDate = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(5));
                        Net_due_dt = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(6));
                        GrandTotal = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(7));
                        DueMonthPluseOne = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(8));
                        DueMonth = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(9));
                        D0_10 = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(10));
                        D11_30 = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(11));
                        D31_60 = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(12));
                        D61_90 = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(13));
                        DG91 = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(14));
                        ON_Account = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(15));
                        Comments = formatter.FormatCellValue(worksheet.GetRow(row).GetCell(16));

                        if (row.Equals(0))
                        {

                            continue;
                        }
                        DataRow ldr = dtDetals.NewRow();
                        ldr["Account"] = Account;
                        //ldr["Customer"] = Customer;
                        //ldr["City"] = City;
                        //ldr["Zone"] = Zone;
                        ldr["BillDoc"] = BillDoc;
                        ldr["DocDate"] = DocDate;
                        if (!string.IsNullOrEmpty(Net_due_dt))
                            ldr["Net_due_dt"] = Convert.ToDateTime(Net_due_dt);
                        if (!string.IsNullOrEmpty(GrandTotal))
                            ldr["GrandTotal"] = Convert.ToDecimal(GrandTotal);
                        if (!string.IsNullOrEmpty(DueMonthPluseOne))
                            ldr["DueMonthPluseOne"] = Convert.ToDecimal(DueMonthPluseOne);
                        if (!string.IsNullOrEmpty(DueMonth))
                            ldr["DueMonth"] = Convert.ToDecimal(DueMonth);
                        if (!string.IsNullOrEmpty(D0_10))
                            ldr["D0_10"] = Convert.ToDecimal(D0_10);
                        if (!string.IsNullOrEmpty(D11_30))
                            ldr["D11_30"] = Convert.ToDecimal(D11_30);
                        if (!string.IsNullOrEmpty(D31_60))
                            ldr["D31_60"] = Convert.ToDecimal(D31_60);
                        if (!string.IsNullOrEmpty(D61_90))
                            ldr["D61_90"] = Convert.ToDecimal(D61_90);
                        if (!string.IsNullOrEmpty(DG91))
                            ldr["DG91"] = Convert.ToDecimal(DG91);
                        if (!string.IsNullOrEmpty(ON_Account))
                            ldr["ON_Account"] = Convert.ToDecimal(ON_Account);
                        ldr["Comments"] = Comments;
                        dtDetals.Rows.Add(ldr);
                    }
                }
                DataView dv = new DataView(dtDetals);
                dv.Sort = "Account, BillDoc";
                dtDetals = dv.ToTable();

                var result = from rows in dtDetals.AsEnumerable()
                             group rows by new { Account = rows["Account"] } into grp
                             select grp;
                connection.Open();
                myTrans = connection.BeginTransaction();
                foreach (var bcode in result)
                {
                    //Decimal TotalPrice = Convert.ToDecimal(bcode.CopyToDataTable().Compute("SUM(GrandTotal)", "Account = " + bcode.Key.Account.ToString()));
                    string sql = "insert into t_outstanding_statement (CompCode, Account, Month, Year, UploadBy, UploadedOn) values " +
                                 "(@CompCode, @Account, @Month, @Year, @UploadBy,@UploadedOn)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    string bpcode = bcode.Key.Account.ToString();
                    cmd.Parameters.Add(new MySqlParameter("@Account", MySqlDbType.VarChar)).Value = bpcode;
                    cmd.Parameters.Add(new MySqlParameter("@Month", MySqlDbType.VarChar)).Value = month;
                    cmd.Parameters.Add(new MySqlParameter("@Year", MySqlDbType.VarChar)).Value = year;
                    cmd.Parameters.Add(new MySqlParameter("@UploadBy", MySqlDbType.VarChar)).Value = Userid;
                    cmd.Parameters.Add(new MySqlParameter("@UploadedOn", MySqlDbType.Date)).Value = DateTime.Now;
                    cmd.ExecuteScalar();
                    int id = (int)cmd.LastInsertedId;
                    DataTable dt = new DataTable();
                    dcName = new DataColumn("BillDoc", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DocDate", typeof(DateTime)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Net_due_dt", typeof(DateTime)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("GrandTotal", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DueMonthPluseOne", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DueMonth", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("D0_10", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("D11_30", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("D31_60", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("D61_90", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("DG91", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("ON_Account", typeof(decimal)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("Comments", typeof(string)); dt.Columns.Add(dcName);
                    dcName = new DataColumn("OutstandingID", typeof(int)); dt.Columns.Add(dcName);
                    //DataTable A = bcode.CopyToDataTable();
                    foreach (DataRow rw in bcode.CopyToDataTable().Rows)
                    {
                        DataRow ldr = dt.NewRow();
                        ldr["BillDoc"] = rw["BillDoc"];
                        ldr["DocDate"] = rw["DocDate"];
                        ldr["Net_due_dt"] = rw["Net_due_dt"];
                        ldr["GrandTotal"] = rw["GrandTotal"];
                        ldr["DueMonthPluseOne"] = rw["DueMonthPluseOne"];
                        ldr["DueMonth"] = rw["DueMonth"];
                        ldr["D0_10"] = rw["D0_10"];
                        ldr["D11_30"] = rw["D11_30"];
                        ldr["D31_60"] = rw["D31_60"];
                        ldr["D61_90"] = rw["D61_90"];
                        ldr["DG91"] = rw["DG91"];
                        ldr["ON_Account"] = rw["ON_Account"];
                        ldr["Comments"] = rw["Comments"];
                        ldr["OutstandingID"] = id;
                        dt.Rows.Add(ldr);
                    }

                    cmd = new MySqlCommand("BulkUploadOutstanding", connection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        Transaction = myTrans,
                        UpdatedRowSource = UpdateRowSource.None
                    };
                    cmd.Parameters.Add("BillDoc", MySqlDbType.String).SourceColumn = "BillDoc";
                    cmd.Parameters.Add("DocDate", MySqlDbType.Date).SourceColumn = "DocDate";
                    cmd.Parameters.Add("Net_due_dt", MySqlDbType.Date).SourceColumn = "Net_due_dt";
                    cmd.Parameters.Add("GrandTotal", MySqlDbType.Decimal).SourceColumn = "GrandTotal";
                    cmd.Parameters.Add("DueMonthPluseOne", MySqlDbType.Decimal).SourceColumn = "DueMonthPluseOne";
                    cmd.Parameters.Add("DueMonth", MySqlDbType.Decimal).SourceColumn = "DueMonth";
                    cmd.Parameters.Add("D0_10", MySqlDbType.Decimal).SourceColumn = "D0_10";
                    cmd.Parameters.Add("D11_30", MySqlDbType.Decimal).SourceColumn = "D11_30";
                    cmd.Parameters.Add("D31_60", MySqlDbType.Decimal).SourceColumn = "D31_60";
                    cmd.Parameters.Add("D61_90", MySqlDbType.Decimal).SourceColumn = "D61_90";
                    cmd.Parameters.Add("DG91", MySqlDbType.Decimal).SourceColumn = "DG91";
                    cmd.Parameters.Add("ON_Account", MySqlDbType.Decimal).SourceColumn = "ON_Account";
                    cmd.Parameters.Add("Comments", MySqlDbType.String).SourceColumn = "Comments";
                    cmd.Parameters.Add("OutstandingID", MySqlDbType.Int32).SourceColumn = "OutstandingID";
                    MySqlDataAdapter da = new MySqlDataAdapter()
                    { InsertCommand = cmd };
                    int records = da.Update(dt);
                }
                myTrans.Commit();
            }
            catch (MySqlException ex)
            {
                myTrans.Rollback();
                return StatusCode(600, ex.Message);
            }
            catch (Exception ex)
            {
                myTrans.Rollback();
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return Ok("Record uploaded successfuly");

        }
        // DELETE api/<OutstandingHomeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
