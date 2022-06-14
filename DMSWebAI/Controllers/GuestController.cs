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
    public class GuestController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public GuestController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<GuestController>
        [HttpGet]
        public IActionResult Get([FromQuery] DateTime? startdate = null, DateTime? enddate = null)
        {
            List<Visitors> visitors = new List<Visitors>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            string condition = string.Empty;

            if (startdate != null && enddate == null)
                condition = "Where VisitedOn = '" + startdate.Value.ToString("yyyy-MM-dd") + "'";
            else if ((startdate != null && enddate != null))
                condition = "Where VisitedOn  between '" + startdate.Value.ToString("yyyy-MM-dd") + "' and '" + enddate.Value.ToString("yyyy-MM-dd") + "'";
            try
            {
                connection.Open();
                string sql = "select * from t_visitors " + condition;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Visitors vs = new Visitors();
                    vs.ID = Convert.ToInt32(rdr["id"]);
                    vs.Name = rdr["Name"].ToString();
                    vs.Email = rdr["Email"].ToString();
                    vs.State = rdr["State"].ToString();
                    vs.City = rdr["City"].ToString();
                    if(rdr["VisitedOn"] != DBNull.Value)
                        vs.VisitedOn = Convert.ToDateTime(rdr["VisitedOn"]);
                    visitors.Add(vs);
                }
                return Ok(visitors);
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

        // GET api/<GuestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GuestController>
        [HttpPost]
        public IActionResult Post([FromBody] Visitors visitors)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            string sql;
            int ID = 0;
            try
            {
                connection.Open();

                sql = "select ID from t_visitors where PhoneNo = '" + visitors.PhoneNo + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    ID = Convert.ToInt32(rdr["ID"]);
                    rdr.Close();
                }
                else
                {
                    rdr.Close();
                    sql = "insert into t_visitors (Name, Email, PhoneNo, State, City, VisitedOn) values (@Name, @Email, @PhoneNo, @State, @City, @VisitedOn)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.VarChar)).Value = visitors.Name;
                    cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = visitors.Email;
                    cmd.Parameters.Add(new MySqlParameter("@PhoneNo", MySqlDbType.VarChar)).Value = visitors.PhoneNo;
                    cmd.Parameters.Add(new MySqlParameter("@State", MySqlDbType.VarChar)).Value = visitors.State;
                    cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = visitors.City;
                    cmd.Parameters.Add(new MySqlParameter("@VisitedOn", MySqlDbType.DateTime)).Value = DateTime.Now;
                    cmd.ExecuteNonQuery();
                }
                return Ok(ID.ToString());
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

        // PUT api/<GuestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GuestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
