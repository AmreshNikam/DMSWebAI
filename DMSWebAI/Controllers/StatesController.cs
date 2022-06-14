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
    public class StatesController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public StatesController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        [HttpGet]
        public IActionResult GetStates()
        {
            List<States> states = new List<States>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select StateID, StateDesc, CountryID from c_state";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    States state = new States();
                    state.StateID = rdr["StateID"].ToString();
                    state.StateDesc = rdr["StateDesc"].ToString();
                    state.CountryID = rdr["CountryID"].ToString();
                    states.Add(state);
                }
                return Ok(states);
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
        [HttpGet("{id}")]
        public IActionResult GetStatesByCountry(string id)
        {
            List<States> states = new List<States>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                string sql = "select StateID, StateDesc, CountryID from c_state where CountryID = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    States state = new States();
                    state.StateID = rdr["StateID"].ToString();
                    state.StateDesc = rdr["StateDesc"].ToString();
                    state.CountryID = rdr["CountryID"].ToString();
                    states.Add(state);
                }
                return Ok(states);
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
        [HttpPost]
        public IActionResult Post([FromBody] States state)
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
                sql = "insert into c_state (StateID, StateDesc, CountryID) values (@StateID, @StateDesc, @CountryID)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = state.StateID;
                cmd.Parameters.Add(new MySqlParameter("@StateDesc", MySqlDbType.VarChar)).Value = state.StateDesc;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = state.CountryID;
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
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] States state)
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
                sql = "update c_state set StateID = @StateID, StateDesc = @StateDesc, CountryID = @CountryID where StateID = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.Int32)).Value = state.StateID;
                cmd.Parameters.Add(new MySqlParameter("@StateDesc", MySqlDbType.VarChar)).Value = state.StateDesc;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.Int32)).Value = state.CountryID;
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
                string sql = "Delete from c_state where StateID = '" + id + "'";
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
