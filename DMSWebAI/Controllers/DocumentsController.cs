using DMSWebAI.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DMSWebAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public DocumentsController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<DocumentsController>
        [HttpGet]
        public IActionResult Get()
        {
            List<Documents> documents = new List<Documents>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            string condition = string.Empty;
            if (AuthControl == 0)
                return StatusCode(700, "You don't have access");
            else if (AuthControl == 1)
            {
                condition = "and DO.Status = 1 and DO.ExpiryDate <= Date(now()) and(DO.BPCodeList = 1 or '" + BPCode + "' in (select BPCode from t_bpcodelist where DocumentID = DO.DocumentID))";
            }
            else
                condition = "";
            try
            {
                connection.Open();
                string sql = "select DO. DocumentID, DO.CompCode, DO.DocumentType, DT.DocumentDesc, DO.DocumentFileName, DO.ActualFile,  " +
                             "DO.Status, DO.UploadDate, DO.ExpiryDate, DO.UploadBy, LT.Text " +
                             "from t_documents as DO " +
                             "left join t_longtext as LT on LT.ID = DO.LongTextID and LT.CompCode = DO.CompCode " +
                             "left join c_documenttype DT on DO.DocumentType = DT.ID and DO.Compcode = DT.compCode " +
                             "where DO.Compcode = '" + CompanyID + "' ";
                sql += condition;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Documents document = new Documents();
                    document.DocumentID = Convert.ToInt32(rdr["DocumentID"]);
                    document.CompCode = rdr["CompCode"].ToString();
                    document.DocumentType = Convert.ToInt32(rdr["DocumentType"]);
                    document.DocumentDesc = rdr["DocumentDesc"].ToString();
                    document.DocumentFileName = rdr["DocumentFileName"].ToString();
                    if(rdr["ActualFile"] != DBNull.Value)
                        document.ActualFile = (byte[])( rdr["ActualFile"]);
                    document.Status = Convert.ToBoolean(rdr["Status"]);
                    document.UploadDate = Convert.ToDateTime(rdr["UploadDate"]);
                    document.ExpiryDate = Convert.ToDateTime(rdr["ExpiryDate"]);
                    document.Text = rdr["Text"].ToString();

                    documents.Add(document);
                }
                return Ok(documents);
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

        // GET api/<DocumentsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet("DocumentType")]
        //Changed by Sohel
        // GET api/<DocumentsTypeController>
        public IActionResult GetDocumentType()
        {
            List<DocumentsType> DocType = new List<DocumentsType>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            string condition = string.Empty;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DocumentsType DT = null;
            if (AuthControl == 0)
                return StatusCode(700, "You don't have access");
            else if (AuthControl == 1)
            {
                condition = "and DO.Status = 1 and DO.ExpiryDate <= Date(now()) and(DO.BPCodeList = 1 or '" + BPCode + "' in (select BPCode from t_bpcodelist where DocumentID = DO.DocumentID))";
            }
            else
                condition = "";
            try
            {
                connection.Open();
                string sql = "select * from c_documenttype ";

                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();


                while (rdr.Read())
                {
                    DocumentsType documentType = new DocumentsType();

                    documentType.ID = Convert.ToInt32(rdr["ID"]);
                    documentType.CompCode = rdr["CompCode"].ToString();
                    documentType.DocumentDesc = rdr["DocumentDesc"].ToString();

                    DocType.Add(documentType);
                }
             
                return Ok(DocType);
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




        // POST api/<DocumentsController>
        [HttpPost]
        public IActionResult Post([FromBody] Documents documents)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            string sql;
            try
            {
                connection.Open();
                int? longtextid = null;
                mytrans = connection.BeginTransaction();
                if (!string.IsNullOrEmpty(documents.Text))
                {
                    sql = "insert into t_longtext (CompCode, Text) values (@CompCode, @Text)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    cmd.Parameters.Add(new MySqlParameter("@Text", MySqlDbType.VarChar)).Value = documents.Text;
                    cmd.ExecuteScalar();
                    longtextid = (int)cmd.LastInsertedId;
                }

                sql = "insert into t_documents (CompCode, DocumentType, DocumentLabel, DocumentFileName, ActualFile, BPCodeList, LongTextID, Status, UploadDate, ExpiryDate, UploadBy) values " +
                      "(@CompCode, @DocumentType, @DocumentLabel, @DocumentFileName, @ActualFile, @BPCodeList, @LongTextID, @Status, @UploadDate, @ExpiryDate, @UploadBy)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                cmd.Parameters.Add(new MySqlParameter("@DocumentType", MySqlDbType.Int32)).Value = documents.DocumentType;
                cmd.Parameters.Add(new MySqlParameter("@DocumentLabel", MySqlDbType.VarChar)).Value = documents.DocumentLabel;
                cmd.Parameters.Add(new MySqlParameter("@DocumentFileName", MySqlDbType.VarChar)).Value = documents.DocumentFileName;
                if(documents.ActualFile == null)
                    cmd.Parameters.Add(new MySqlParameter("@ActualFile", MySqlDbType.Blob)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@ActualFile", MySqlDbType.Blob)).Value = documents.ActualFile;
                if (documents.BPCodeListArray == null)
                    cmd.Parameters.Add(new MySqlParameter("@BPCodeList", MySqlDbType.Int16)).Value = 0;
                else
                    cmd.Parameters.Add(new MySqlParameter("@BPCodeList", MySqlDbType.Int16)).Value = 1;
                if (longtextid == null)
                    cmd.Parameters.Add(new MySqlParameter("@LongTextID", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@LongTextID", MySqlDbType.Int32)).Value = longtextid;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int32)).Value = documents.Status;
                cmd.Parameters.Add(new MySqlParameter("@UploadDate", MySqlDbType.DateTime)).Value = DateTime.Now;
                cmd.Parameters.Add(new MySqlParameter("@ExpiryDate", MySqlDbType.DateTime)).Value = documents.ExpiryDate;
                cmd.Parameters.Add(new MySqlParameter("@UploadBy", MySqlDbType.VarChar)).Value = Userid;
                cmd.ExecuteNonQuery();
                documents.DocumentID = (int)cmd.LastInsertedId;
                if (documents.BPCodeListArray != null)
                {
                    foreach (string st in documents.BPCodeListArray)
                    {
                        sql = "insert into t_bpcodelist (DocumentID, BPCode) values (@DocumentID, @BPCode)";
                        cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                        cmd.Parameters.Add(new MySqlParameter("@DocumentID", MySqlDbType.VarChar)).Value = documents.DocumentID;
                        cmd.Parameters.Add(new MySqlParameter("@Text", MySqlDbType.VarChar)).Value = st;
                        cmd.ExecuteScalar();
                    }
                }
                mytrans.Commit();
                return Ok("Record inserted sucessfuly");
            }
            catch (MySqlException ex)
            {
                try { mytrans.Rollback(); } catch { }
                return StatusCode(600, ex.Message);
            }
            catch (Exception ex)
            {
                try { mytrans.Rollback(); } catch { }
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        // PUT api/<DocumentsController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Documents documents)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            string sql;
            try
            {
                connection.Open();
                int? longtextid = null;
                mytrans = connection.BeginTransaction();
                if (!string.IsNullOrEmpty(documents.Text))
                {
                    sql = "delete from t_longtext where ID = " + documents.LongTextID;
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.ExecuteNonQuery();
                    sql = "insert into t_longtext (CompCode, Text) values (@CompCode, @Text)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    cmd.Parameters.Add(new MySqlParameter("@Text", MySqlDbType.VarChar)).Value = documents.Text;
                    cmd.ExecuteScalar();
                    longtextid = (int)cmd.LastInsertedId;
                }

                sql = "udate t_documents set CompCode = @CompCode, DocumentType =  @DocumentType, DocumentLabel = @DocumentLabel, DocumentFileName = @@DocumentFileName, ActualFile = @ActualFile, " +
                      "BPCodeList = @BPCodeList, LongText = @LongText, Status = @Status, UploadDate = @UploadDate, @ExpiryDate = ExpiryDate, UploadBy = @UploadBy where DocumentID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                cmd.Parameters.Add(new MySqlParameter("@DocumentType", MySqlDbType.Int32)).Value = documents.DocumentType;
                cmd.Parameters.Add(new MySqlParameter("@DocumentLabel", MySqlDbType.VarChar)).Value = documents.DocumentLabel;
                cmd.Parameters.Add(new MySqlParameter("@DocumentFileName", MySqlDbType.Blob)).Value = documents.DocumentFileName;
                cmd.Parameters.Add(new MySqlParameter("@ActualFile", MySqlDbType.VarChar)).Value = documents.ActualFile;
                if (documents.BPCodeListArray == null)
                    cmd.Parameters.Add(new MySqlParameter("@BPCodeList", MySqlDbType.Int16)).Value = 0;
                else
                    cmd.Parameters.Add(new MySqlParameter("@BPCodeList", MySqlDbType.Int16)).Value = 1;
                if (longtextid == null)
                    cmd.Parameters.Add(new MySqlParameter("@LongText", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@LongText", MySqlDbType.Int32)).Value = longtextid;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.VarChar)).Value = documents.Status;
                cmd.Parameters.Add(new MySqlParameter("@UploadDate", MySqlDbType.VarChar)).Value = DateTime.Now;
                cmd.Parameters.Add(new MySqlParameter("@ExpiryDate", MySqlDbType.Int32)).Value = documents.ExpiryDate;
                cmd.Parameters.Add(new MySqlParameter("@UploadBy", MySqlDbType.VarChar)).Value = Userid;
                cmd.ExecuteNonQuery();
                documents.DocumentID = (int)cmd.LastInsertedId;
                sql = "delete from t_bpcodelist where DocumentID = " + documents.DocumentID;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                foreach (string st in documents.BPCodeListArray)
                {
                    sql = "insert into t_bpcodelist (DocumentID, BPCode) values (@DocumentID, @BPCode)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@DocumentID", MySqlDbType.VarChar)).Value = documents.DocumentID;
                    cmd.Parameters.Add(new MySqlParameter("@Text", MySqlDbType.VarChar)).Value = st;
                    cmd.ExecuteScalar();
                }

                mytrans.Commit();
                return Ok("Record inserted sucessfuly");
            }
            catch (MySqlException ex)
            {
                try { mytrans.Rollback(); } catch { }
                return StatusCode(600, ex.Message);
            }
            catch (Exception ex)
            {
                try { mytrans.Rollback(); } catch { }
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        // DELETE api/<DocumentsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            try
            {
                connection.Open();
                string sql = "select LongText fomm t_documents where DocumentID = " + id;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr =  cmd.ExecuteReader();
                int lid = Convert.ToInt32(rdr["LongText"]);
                rdr.Close();
                sql = "Delete from t_documents where ID = " + lid;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                sql = "delete from t_bpcodelist where DocumentID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                connection.Close();
                return Ok("Successfuly deleted");
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
    }
}
