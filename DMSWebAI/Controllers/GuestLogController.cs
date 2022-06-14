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
    public class GuestLogController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public GuestLogController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<GuestLogController>
        [HttpGet]
        public IActionResult Get([FromQuery] DateTime? startdate = null, DateTime? enddate = null)
        {
            List<Visitors> visitors = new List<Visitors>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            string condition = string.Empty;
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access to create user");
            }
            if (startdate != null && enddate == null)
                condition = "Where VisitedOn = '" + startdate.Value.ToString("yyyy-MM-dd") + "'";
            else if ((startdate != null && enddate != null))
                condition = "Where Date(VL.LogDateTime)  between '" + startdate.Value.ToString("yyyy-MM-dd") + "' and '" + enddate.Value.ToString("yyyy-MM-dd") + "'";
            try
            {
                connection.Open();
                string sql = "select VL.ID, Name, Email, PhoneNo, State, City, VisitedOn, " +
                             "Description, LogDateTime, BusinessType " +
                             "from t_visitorlogs as VL " +
                             "left join t_visitors as VI on VL.ID = VI.ID " + condition + " order by ID";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                int PrevID = -1;
                Visitors vs = null;
                visitorlogs vg = null;
                while (rdr.Read())
                {
                    int tid = Convert.ToInt32(rdr["id"]);
                    if (PrevID != tid)
                    {
                        if (vs!= null)
                            visitors.Add(vs);
                        vs = new Visitors();
                        vs.ID = Convert.ToInt32(rdr["id"]);
                        vs.Name = rdr["Name"].ToString();
                        vs.Email = rdr["Email"].ToString();
                        vs.State = rdr["State"].ToString();
                        vs.City = rdr["City"].ToString();
                        vs.VisitedOn = Convert.ToDateTime(rdr["VisitedOn"]);
                        vs.logs = new List<visitorlogs>();
                        vg = new visitorlogs();
                        vg.ID = tid;
                        vg.BusinessType = rdr["BusinessType"].ToString();
                        vg.Description = rdr["Description"].ToString();
                        vg.LogDateTime = Convert.ToDateTime(rdr["LogDateTime"]);
                        vs.logs.Add(vg);
                        PrevID = tid;
                    }
                    else
                    {
                        vg = new visitorlogs();
                        vg.ID = tid;
                        vg.Description = rdr["Description"].ToString();
                        vg.LogDateTime = Convert.ToDateTime(rdr["LogDateTime"]);
                        vs.logs.Add(vg);
                    }
                        
                }
                if (vs != null)
                    visitors.Add(vs);
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

        // GET api/<GuestLogController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GuestLogController>
        [HttpPost]
        public IActionResult Post([FromQuery] int ID, [FromBody] visitorlogs visitors)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            string sql;

            try
            {
                connection.Open();
                sql = "insert into t_visitorlogs (ID, Description, BusinessType, LogDateTime) values (@ID, @Description, @BusinessType, @LogDateTime)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@ID", MySqlDbType.Int32)).Value = ID;
                cmd.Parameters.Add(new MySqlParameter("@Description", MySqlDbType.VarChar)).Value = visitors.Description;
                cmd.Parameters.Add(new MySqlParameter("@BusinessType", MySqlDbType.VarChar)).Value = visitors.BusinessType;
                cmd.Parameters.Add(new MySqlParameter("@LogDateTime", MySqlDbType.DateTime)).Value = DateTime.Now;
                cmd.ExecuteNonQuery();
                return Ok("Log save sucessfuly");
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

        // PUT api/<GuestLogController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GuestLogController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
