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
    public class DesignationsController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public DesignationsController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<DesignationsController>
        [HttpGet]
        public IActionResult Get()
        {
            List<Designations> designations = new List<Designations>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select DesigID, CompCode, ShortCode, Description, SeniorDesgID from c_designation";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Designations designation = new Designations();
                    designation.DesigID = Convert.ToInt32(rdr["DesigID"]);
                    designation.ShortCode = rdr["ShortCode"].ToString();
                    designation.CompCode = rdr["CompCode"].ToString();
                    designation.Description = rdr["Description"].ToString();
                    if(rdr["SeniorDesgID"] != DBNull.Value)
                    designation.SeniorDesgID = Convert.ToInt32(rdr["SeniorDesgID"]);
                    designations.Add(designation);
                }
                return Ok(designations);
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

        // GET api/<DesignationsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<DesignationsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DesignationsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DesignationsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
