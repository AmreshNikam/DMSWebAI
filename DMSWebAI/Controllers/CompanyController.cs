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
    public class CompanyController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public CompanyController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet]
        public IActionResult GetAllCompany()
        {
            List<Company> company = new List<Company>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select CompCode, CompName, ShortKey, Street, AddressLine, City, PostalCode, CP.StateID, CP.RegionID, Phone, Email, GSTNo, CIN, Status, " +
                             "ST.StateDesc, CN.CountryID, CountryDesc, RegionDesc " +
                             "from c_company CP " +
                             "left join c_country CN on CP.CountryID = CN.CountryID " +
                             "left join c_state as ST on CP.StateID = ST.StateID and CN.CountryID = ST.CountryID " +
                             "left join c_region RG on CP.RegionId = RG.RegionId and CN.CountryID = RG.CountryID " +
                             "Where CP.Status != 999";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Company cmp = new Company();
                    
                    cmp.CompCode = rdr["ComaCode"].ToString();
                    cmp.CompName = rdr["CompName"].ToString();
                    cmp.ShortKey = rdr["ShortKey"].ToString();
                    cmp.Street = rdr["Street"].ToString();
                    cmp.AddressLine = rdr["AddressLine"].ToString();
                    cmp.City = rdr["City"].ToString();
                    cmp.PostalCode = rdr["PostalCode"].ToString();
                    cmp.StateID = rdr["StateID"].ToString();
                    cmp.RegionID = rdr["RegionID"].ToString();
                    cmp.Phone = rdr["Phone"].ToString();
                    cmp.Email = rdr["Email"].ToString();
                    cmp.GSTNo = rdr["GSTNo"].ToString();
                    cmp.CIN = rdr["CIN"].ToString();
                    cmp.Status = Convert.ToBoolean(rdr["Status"]);
                    //**************derived fiels *************************
                    cmp.RegionName = rdr["RegionName"].ToString();
                    cmp.State = rdr["State"].ToString();
                    cmp.CountryID = rdr["CountryID"].ToString();
                    cmp.Country = rdr["Country"].ToString();
                    company.Add(cmp);
                }
                return Ok(company);
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

        // GET api/<CompanyController>/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select CompCode, CompName, ShortKey, Street, AddressLine, City, PostalCode, CP.StateID, CP.RegionID, Phone, Email, GSTNo, CIN, Status, " +
                             "ST.StateDesc, CN.CountryID, CountryDesc, RegionDesc " +
                             "from c_company CP " +
                             "left join c_country CN on CP.CountryID = CN.CountryID " +
                             "left join c_state as ST on CP.StateID = ST.StateID and CP.CountryID = ST.CountryID " +
                             "left join c_region RG on CP.RegionId = RG.RegionId and CP.CountryID = RG.CountryID " +
                             "where CP.CompCode = '" + id + "' and CP.Status != 999";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                Company cmp = new Company();
                if (rdr.Read())
                {
                    cmp.CompCode = rdr["ComaCode"].ToString();
                    cmp.CompName = rdr["CompName"].ToString();
                    cmp.ShortKey = rdr["ShortKey"].ToString();
                    cmp.Street = rdr["Street"].ToString();
                    cmp.AddressLine = rdr["AddressLine"].ToString();
                    cmp.City = rdr["City"].ToString();
                    cmp.PostalCode = rdr["PostalCode"].ToString();
                    cmp.StateID = rdr["StateID"].ToString();
                    cmp.RegionID = rdr["RegionID"].ToString();
                    cmp.Phone = rdr["Phone"].ToString();
                    cmp.Email = rdr["Email"].ToString();
                    cmp.GSTNo = rdr["GSTNo"].ToString();
                    cmp.CIN = rdr["CIN"].ToString();
                    cmp.Status = Convert.ToBoolean(rdr["Status"]);
                    //**************derived fiels *************************
                    cmp.RegionName = rdr["RegionName"].ToString();
                    cmp.State = rdr["State"].ToString();
                    cmp.CountryID = rdr["CountryID"].ToString();
                    cmp.Country = rdr["Country"].ToString();
                }
                return Ok(cmp);
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

        // POST api/<CompanyController>
        [HttpPost]
        public IActionResult Post([FromBody] Company comoany)
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
                mytrans = connection.BeginTransaction();
                sql = "insert into c_company (CompCode, CompName, ShortKey, Street, AddressLine, City, PostalCode, CountryID, StateID, RegionID, Phone, Email, GSTNo, CIN, Status) values " +
                      "(@CompCode, @CompName, @ShortKey, @Street, @AddressLine, @City, @PostalCode, @CountryID, @StateID, @RegionID, @Phone, @Email, @GSTNo, @CIN, @Status)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = comoany.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@CompName", MySqlDbType.VarChar)).Value = comoany.CompName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = comoany.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = comoany.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = comoany.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = comoany.City;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = comoany.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = comoany.StateID;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = comoany.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.Int32)).Value = comoany.RegionID;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = comoany.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = comoany.Email;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = comoany.GSTNo;
                cmd.Parameters.Add(new MySqlParameter("@CIN", MySqlDbType.VarChar)).Value = comoany.CIN;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = comoany.Status;
                cmd.ExecuteNonQuery();
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

        // PUT api/<CompanyController>/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Company comoany)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            string sql;
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
                sql = "update c_company set CompCode = @CompCode,CompName = @CompName, ShortKey = @ShortKey, Street = @Street, AddressLine= @AddressLine, City = @City, PostalCode = @PostalCode, CountryID = @CountryID, StateID = @StateID, RegionID = @RegionID, Phone= @Phone, Email= @Email, GSTNo= @GSTNo, CIN= @CIN, Status = @Status where CompCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = comoany.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@CompName", MySqlDbType.VarChar)).Value = comoany.CompName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = comoany.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = comoany.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = comoany.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = comoany.City;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = comoany.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = comoany.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = comoany.StateID;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.Int32)).Value = comoany.RegionID;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = comoany.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = comoany.Email;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = comoany.GSTNo;
                cmd.Parameters.Add(new MySqlParameter("@CIN", MySqlDbType.VarChar)).Value = comoany.CIN;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = comoany.Status;
                cmd.ExecuteNonQuery();
                mytrans.Commit();
                return Ok("Sucessfuly updated");
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
        // DELETE api/<CompanyController>/5
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
                //string sql = "Delete from c_company where ComapCode = '" + id + "'";
                string sql = "update c_company set Status = 999 where ComapCode = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
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
