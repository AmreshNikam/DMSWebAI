
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
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public UsersController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<UsersController>
        [HttpGet]
        public IActionResult Get()
        {
            List<Users> users = new List<Users>();
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
                string sql = "select MU.Userid, CompCode, FName, MName, LName, Street, AddressLine, City, PostalCode, StateID, CountryID, Phone, " +
                             "Email, CreatedBy, ChangedBy, CreatedOn, ChangedOn, Password, Role, Expiry_Date, Active_Inactive "+
                             "from m_user as MU " +
                             "left join t_user_credential as UC on MU.Userid = UC.Userid " +
                             "where MU.CompCode = '" + CompanyID + "' and Active_Inactive != 999; ";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    Users user = new Users();
                    user.Userid = rdr["Userid"].ToString();
                    user.CompCode = rdr["CompCode"].ToString();
                    user.FName = rdr["FName"].ToString();
                    user.MName = rdr["MName"].ToString();
                    user.LName = rdr["LName"].ToString();
                    user.Street = rdr["Street"].ToString();
                    user.AddressLine = rdr["AddressLine"].ToString();
                    user.City = rdr["City"].ToString();
                    user.PostalCode = rdr["PostalCode"].ToString();
                    user.StateID = rdr["StateID"].ToString();
                    user.CountryID = rdr["CountryID"].ToString();
                    user.Phone = rdr["Phone"].ToString();
                    user.Email = rdr["Email"].ToString();
                    user.CreatedBy = rdr["CreatedBy"].ToString();
                    user.ChangedBy = rdr["ChangedBy"].ToString();
                    if(rdr["CreatedOn"] != DBNull.Value)
                        user.CreatedOn = Convert.ToDateTime(rdr["CreatedOn"]);
                    if (rdr["ChangedOn"] != DBNull.Value)
                        user.ChangedOn = Convert.ToDateTime(rdr["ChangedOn"]);





                    users.Add(user);
                }
                return Ok(users);
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

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UsersController>
        [HttpPost]
        public IActionResult Post([FromBody] Users user)
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
            if(AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            string sql;
            try
            {
                connection.Open();
                mytrans = connection.BeginTransaction();
                sql = "insert into m_user (Userid, CompCode, FName, MName, LName, Street, AddressLine, City, PostalCode, StateID, CountryID, Phone, Email, CreatedBy, ChangedBy, CreatedOn, ChangedOn) values " +
                      "(@Userid, @CompCode, @FName, @MName, @LName, @Street, @AddressLine, @City, @PostalCode, @StateID, @CountryID, @Phone, @Email, @CreatedBy, @ChangedBy, @CreatedOn, @ChangedOn)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Userid", MySqlDbType.VarChar)).Value = user.Userid;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                cmd.Parameters.Add(new MySqlParameter("@FName", MySqlDbType.VarChar)).Value = user.FName;
                cmd.Parameters.Add(new MySqlParameter("@MName", MySqlDbType.VarChar)).Value = user.MName;
                cmd.Parameters.Add(new MySqlParameter("@LName", MySqlDbType.VarChar)).Value = user.LName;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = user.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = user.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = user.City;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = user.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = user.StateID;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = user.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = user.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = user.Email;
                cmd.Parameters.Add(new MySqlParameter("@CreatedBy", MySqlDbType.VarChar)).Value = Userid;
                cmd.Parameters.Add(new MySqlParameter("@ChangedBy", MySqlDbType.VarChar)).Value = Userid;
                cmd.Parameters.Add(new MySqlParameter("@CreatedOn", MySqlDbType.Date)).Value = DateTime.Now;
                cmd.Parameters.Add(new MySqlParameter("@ChangedOn", MySqlDbType.Date)).Value = DateTime.Now;
                cmd.ExecuteNonQuery();
                //********************************Entery in t_user_credential table******************************
                sql = "insert into t_user_credential (Userid, Password, Role, Expiry_Date, Active_Inactive) values " +
                      "(@Userid, @Password, @Role, @Expiry_Date, @Active_Inactive)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Userid", MySqlDbType.VarChar)).Value = user.Userid;
                cmd.Parameters.Add(new MySqlParameter("@Password", MySqlDbType.VarChar)).Value = user.Password;
                cmd.Parameters.Add(new MySqlParameter("@Role", MySqlDbType.Int32)).Value = user.Role;
                cmd.Parameters.Add(new MySqlParameter("@Expiry_Date", MySqlDbType.Date)).Value = user.Expiry_Date;
                cmd.Parameters.Add(new MySqlParameter("@Active_Inactive", MySqlDbType.Int32)).Value = user.Active_Inactive;
                cmd.ExecuteNonQuery();
                if(!string.IsNullOrEmpty(BPCode))
                {
                    sql = "insert into m_user_bp_compcode (UserID, BPCode, CompCode) values " +
                      "(@UserID, @BPCode, @CompCode)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@UserID", MySqlDbType.VarChar)).Value = user.Userid;
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = user.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    cmd.ExecuteNonQuery();
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

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public IActionResult put(string id, [FromBody] Users user)
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
                sql = "update m_user set Userid = @Userid, CompCode = @CompCode, FName = @FName, MName = @MName, LName = @LName, Street = @, AddressLine = @AddressLine, City = @City, PostalCode = @PostalCode, StateID = @StateID, CountryID = @CountryID, Phone = @Phone, Email = @Email, ChangedBy = @ChangedBy, ChangedOn = @ChangedOn where Userid = '" + id +"'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Userid", MySqlDbType.VarChar)).Value = user.Userid;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = user.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@FName", MySqlDbType.VarChar)).Value = user.FName;
                cmd.Parameters.Add(new MySqlParameter("@MName", MySqlDbType.VarChar)).Value = user.MName;
                cmd.Parameters.Add(new MySqlParameter("@LName", MySqlDbType.VarChar)).Value = user.LName;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = user.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = user.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = user.City;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = user.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.Int32)).Value = user.StateID;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = user.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = user.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = user.Email;
                cmd.Parameters.Add(new MySqlParameter("@ChangedBy", MySqlDbType.Int16)).Value = Userid; ;
                cmd.Parameters.Add(new MySqlParameter("@ChangedOn", MySqlDbType.Int16)).Value = DateTime.Now;
                cmd.ExecuteNonQuery();
                //********************************Entery in t_user_credential table******************************
                sql = "udate t_user_credential set Userid = @Userid, Password = @Password, Role = @Role, Expiry_Date = @Expiry_Date, Active_Inactive = @Active_Inactive where Userid = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Userid", MySqlDbType.VarChar)).Value = user.Userid;
                cmd.Parameters.Add(new MySqlParameter("@Password", MySqlDbType.VarChar)).Value = user.Password;
                cmd.Parameters.Add(new MySqlParameter("@Role", MySqlDbType.Int32)).Value = user.Role;
                cmd.Parameters.Add(new MySqlParameter("@Expiry_Date", MySqlDbType.Date)).Value = user.Expiry_Date;
                cmd.Parameters.Add(new MySqlParameter("@Active_Inactive", MySqlDbType.Int32)).Value = user.Active_Inactive;
                cmd.ExecuteNonQuery();
                if (!string.IsNullOrEmpty(BPCode))
                {
                    sql = "delete from m_user_bp_compcode where UserID = '" + id + "' and CompCode '" + CompanyID + "'";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.ExecuteNonQuery();
                    sql = "insert into m_user_bp_compcode (UserID, BPCode, CompCode) values " +
                      "(@UserID, @BPCode, @CompCode)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@UserID", MySqlDbType.VarChar)).Value = user.Userid;
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = user.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = CompanyID;
                    cmd.ExecuteNonQuery();
                }

                mytrans.Commit();
                return Ok("Record udated sucessfuly");
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

        // DELETE api/<UsersController>/5
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
                //string sql = "Delete from t_user_credential where ComapCode = '" + id + "'";
                string sql = "udate t_user_credential set Active_Inactive = 999 where Userid = '" + id + "'";
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
