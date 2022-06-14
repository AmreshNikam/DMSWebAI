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
    public class RegionController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public RegionController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet]
        public IActionResult GetRegion()
        {
            List<Region> regions = new List<Region>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select RegionId, RegionDesc, CountryID from c_region";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Region region = new Region();
                    region.RegionId = rdr["RegionId"].ToString();
                    region.RegionDesc = rdr["RegionDesc"].ToString();
                    region.CountryID = rdr["CountryID"].ToString();
                    regions.Add(region);
                }
                return Ok(regions);
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
            List<Region> regions = new List<Region>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                string sql = "select RegionId, RegionDesc, CountryID from c_region where RegionId = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                AccountGroup account = new AccountGroup();
                while (rdr.Read())
                {
                    Region region = new Region();
                    region.RegionId = rdr["RegionId"].ToString();
                    region.RegionDesc = rdr["RegionDesc"].ToString();
                    region.CountryID = rdr["CountryID"].ToString();
                    regions.Add(region);
                }
                return Ok(regions);
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
        [HttpGet("Country/{id}")]
        public IActionResult GeteRgionCountryWise(string id)
        {
            List<Region> regions = new List<Region>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                string sql = "select RegionId, RegionDesc, CountryID from c_region where CountryID = '" + id + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                AccountGroup account = new AccountGroup();
                while (rdr.Read())
                {
                    Region region = new Region();
                    region.RegionId = rdr["RegionId"].ToString();
                    region.RegionDesc = rdr["RegionDesc"].ToString();
                    region.CountryID = rdr["CountryID"].ToString();
                    regions.Add(region);
                }
                return Ok(regions);
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
        public IActionResult Post([FromBody] Region region)
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
                sql = "insert into c_region (RegionId, RegionDesc, CountryID) values (@RegionId, @RegionDesc, @CountryID)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@RegionId", MySqlDbType.VarChar)).Value = region.RegionId;
                cmd.Parameters.Add(new MySqlParameter("@RegionDesc", MySqlDbType.VarChar)).Value = region.RegionDesc;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = region.CountryID;
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
        public IActionResult Put(string id, [FromBody] Region region)
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
                sql = "update c_region set RegionId = @RegionId, RegionDesc = @RegionDesc, CountryID = @CountryID where RegionId = '" + id + "'";
                cmd.Parameters.Add(new MySqlParameter("@RegionId", MySqlDbType.VarChar)).Value = region.RegionId;
                cmd.Parameters.Add(new MySqlParameter("@RegionDesc", MySqlDbType.VarChar)).Value = region.RegionDesc;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = region.CountryID;
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
                string sql = "Delete from c_region where RegionId = '" + id + "'";
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
