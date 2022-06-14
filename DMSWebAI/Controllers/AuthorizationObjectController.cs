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
    public class AuthorizationObjectController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public AuthorizationObjectController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<AuthorizationObjectController>
        [HttpGet]
        public IActionResult Get()
        {
            List<AuthorizationObject> authorizationObjects = new List<AuthorizationObject>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select ID, Label, Page_route, Icon from c_auth_object";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    AuthorizationObject auth = new AuthorizationObject();
                    auth.ID = Convert.ToInt32(rdr["ID"]);
                    auth.Label = rdr["Label"].ToString();
                    auth.Page_route = rdr["Page_route"].ToString();
                    auth.Icon = rdr["Icon"].ToString();
                    authorizationObjects.Add(auth);
                }
                return Ok(authorizationObjects);
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

        // GET api/<AuthorizationObjectController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                string sql = "select ID, Label, Page_route, Icon from c_auth_object";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                AuthorizationObject auth = new AuthorizationObject();
                if (rdr.Read())
                {
                    auth.ID = Convert.ToInt32(rdr["ID"]);
                    auth.Label = rdr["Label"].ToString();
                    auth.Page_route = rdr["Page_route"].ToString();
                    auth.Icon = rdr["Icon"].ToString();
                }
                return Ok(auth);
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

        // POST api/<AuthorizationObjectController>
        [HttpPost]
        public IActionResult Post([FromBody] AuthorizationObject auth)
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
                sql = "insert into c_auth_object (Label, Page_route, Icon) values " +
                      "(Label, Page_route, Icon)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Label", MySqlDbType.VarChar)).Value = auth.Label;
                cmd.Parameters.Add(new MySqlParameter("@Page_route", MySqlDbType.VarChar)).Value = auth.Page_route;
                cmd.Parameters.Add(new MySqlParameter("@Icon", MySqlDbType.VarChar)).Value = auth.Icon;
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

        // PUT api/<AuthorizationObjectController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] AuthorizationObject auth)
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
                sql = "update c_auth_object set ID = @ID, Label = @Label, Page_route = @Page_route, Icon = @Icon where ID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@ID", MySqlDbType.Int32)).Value = id;
                cmd.Parameters.Add(new MySqlParameter("@Label", MySqlDbType.VarChar)).Value = auth.Label;
                cmd.Parameters.Add(new MySqlParameter("@Page_route", MySqlDbType.VarChar)).Value = auth.Page_route;
                cmd.Parameters.Add(new MySqlParameter("@Icon", MySqlDbType.VarChar)).Value = auth.Icon;
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
        // DELETE api/<AuthorizationObjectController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
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
                string sql = "delete c_auth_object" +
                    " where ComapCode = '" + id + "'";
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
