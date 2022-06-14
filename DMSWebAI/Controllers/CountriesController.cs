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
    public class CountriesController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public CountriesController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet]
        public IActionResult GetCountries()
        {
            List<Countries> countries = new List<Countries>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select CountryID, CountryDesc, CountryCallingCode from c_country";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Countries country = new Countries();
                    country.CountryID = rdr["CountryID"].ToString();
                    country.CountryDesc = rdr["CountryDesc"].ToString();
                    if (rdr["CountryCallingCode"] == DBNull.Value)
                        country.CountryCallingCode = null;
                    else
                        country.CountryCallingCode = Convert.ToInt32(rdr["CountryCallingCode"]);

                    countries.Add(country);
                }
                return Ok(countries);
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
                string sql = "CountryID, CountryDesc, CountryCallingCode from c_country where CountryID = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                Countries country = new Countries();
                if (rdr.Read())
                {
                    country.CountryID = rdr["CountryID"].ToString();
                    country.CountryDesc = rdr["CountryDesc"].ToString();
                    if (rdr["CountryCallingCode"] == DBNull.Value)
                        country.CountryCallingCode = null;
                    else
                        country.CountryCallingCode = Convert.ToInt32(rdr["CountryCallingCode"]);
                }
                return Ok(country);
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
        public IActionResult Post([FromBody] Countries country)
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
                sql = "insert into c_country (CountryID, CountryDesc, CountryCallingCode) values (@CountryID, @CountryDesc, @CountryCallingCode)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = country.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@CountryDesc", MySqlDbType.VarChar)).Value = country.CountryDesc;
                if(country.CountryCallingCode == null)
                    cmd.Parameters.Add(new MySqlParameter("@CountryCallingCode", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@CountryCallingCode", MySqlDbType.Int32)).Value = country.CountryCallingCode;
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
        public IActionResult Put(int id, [FromBody] Countries country)
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
                sql = "update c_country set CountryID = @CountryID, CountryDesc = @CountryDesc, CountryCallingCode = @CountryCallingCode where CountryID = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = country.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@CountryDesc", MySqlDbType.VarChar)).Value = country.CountryDesc;
                if(country.CountryCallingCode == null)
                    cmd.Parameters.Add(new MySqlParameter("@CountryCallingCode", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@CountryCallingCode", MySqlDbType.Int32)).Value = country.CountryCallingCode;
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
                string sql = "Delete from c_country where CountryID = '" + id + "'";
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
