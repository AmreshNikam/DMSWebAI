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
    public class AccountGroupController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public AccountGroupController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet]
        public IActionResult GetAccountGroups()
        {
            List<AccountGroup> accountGroups = new List<AccountGroup>();
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
                //string sql = "select AccGroupID, AccGroup, Descs, CompCode, PlantCode from c_accountgroup";
                string sql = " select CompCode, ActGroupID, ActGroupDesc from c_accountgroup ";
                Console.WriteLine(sql);
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    AccountGroup account = new AccountGroup();
                    account.AccGroupID = rdr["ActGroupID"].ToString();
                   //account.AccGroup = rdr["AccGroup"].ToString();
                    account.Descs = rdr["ActGroupDesc"].ToString();
                    account.CompCode = rdr["CompCode"].ToString();
                   // account.PlantCode = rdr["PlantCode"].ToString();
                   //AccountGroup account = new AccountGroup();
                   //account.AccGroupID = Convert.ToInt32(rdr["AccGroupID"]);
                   //account.AccGroup = rdr["AccGroup"].ToString();
                   //account.Descs = rdr["Descs"].ToString();
                   //account.CompCode = rdr["CompCode"].ToString();                   
                   //account.PlantCode = rdr["PlantCode"].ToString();
                    accountGroups.Add(account);
                }
                return Ok(accountGroups);
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
        public IActionResult GetAccountGroup(int id)
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
                string sql = "select ActGroupID, AccGroup, Descs, CompCode, PlantCode from c_accountgroup where AccGroupID = " + id;
                //added by sohel
                //string sql = "select ActGroupID, ActGroupDesc, CompCode from c_accountgroup where ActGroupID = "+ id;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                AccountGroup account = new AccountGroup();
                if (rdr.Read())
                {
                    account.AccGroupID = rdr["ActGroupID"].ToString();
                    account.AccGroup = rdr["AccGroup"].ToString();
                    account.Descs = rdr["Descs"].ToString();
                    account.CompCode = rdr["CompCode"].ToString();
                    account.PlantCode = rdr["PlantCode"].ToString();
                }
                return Ok(account);
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
        public IActionResult Post([FromBody] AccountGroup accountGroup)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
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
                sql = "insert into c_accountgroup (AccGroup, Descs, CompCode, PlantCode) values (@AccGroup, @Descs, @CompCode, @PlantCode)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@AccGroup", MySqlDbType.VarChar)).Value = accountGroup.AccGroup;
                cmd.Parameters.Add(new MySqlParameter("@Descs", MySqlDbType.VarChar)).Value = accountGroup.Descs;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = accountGroup.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = accountGroup.PlantCode;
                cmd.ExecuteNonQuery();
                return Ok("Record inserted sucessfuly");
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

        // PUT api/<CompanyController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] AccountGroup accountGroup)
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
                mytrans = connection.BeginTransaction();
                sql = "update c_accountgroup set AccGroupID = @AccGroupID, AccGroup = AccGroup, Descs = @Descs, CompCode = @CompCode, PlantCode = @PlantCode where AccGroupID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@AccGroupID", MySqlDbType.Int32)).Value = accountGroup.AccGroupID;
                cmd.Parameters.Add(new MySqlParameter("@AccGroup", MySqlDbType.VarChar)).Value = accountGroup.AccGroup;
                cmd.Parameters.Add(new MySqlParameter("@Descs", MySqlDbType.VarChar)).Value = accountGroup.Descs;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = accountGroup.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = accountGroup.PlantCode;
                cmd.ExecuteNonQuery();
                mytrans.Commit();
                return Ok("Sucessfuly udated");
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
        public IActionResult Delete(int id)
        {
            string connString = this.Configuration.GetConnectionString("MFG_Tracker");
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
                string sql = "Delete from c_accountgroup where AccGroupID = " + id;
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
