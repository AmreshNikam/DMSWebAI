using DMSWebAI.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DMSWebAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public LoginController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<LoginController>
        [HttpGet]
        public IActionResult Get()
        {
            List<Loginresponse> loginresponses = new List<Loginresponse>();
            string connString = this.Configuration.GetConnectionString("DMS");

            string username = string.Empty;
            string password = string.Empty;

            if (Request.Headers.TryGetValue("Authorization", out StringValues authToken))
            {
                string authHeader = authToken.First();
                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                int seperatorIndex = usernamePassword.IndexOf(':');
                username = usernamePassword.Substring(0, seperatorIndex);
                password = usernamePassword.Substring(seperatorIndex + 1);
            }
            else
            {
                return BadRequest("Missing Authorization Header.");
            }

            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "Select * from t_user_credential where Userid = '" + username + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                List<Loginresponse> Response = new List<Loginresponse>();
                if (rdr.Read())
                {
                    if(password == rdr["Password"].ToString())
                    {
                        if (Convert.ToBoolean(rdr["Active_Inactive"]))
                        {
                            if (Convert.ToDateTime(rdr["Expiry_Date"]).Date >= DateTime.Now.Date)
                            {
                                rdr.Close();
                                sql = "select if(UB.BPCode is null,'ALL',UB.BPCode) as BPCode, RO.Auth_Control , AO.Label, AO.Page_route, AO.Icon " +
                                      "from t_user_credential as UC " +
                                      "left join m_role_auth_object as RO on UC.Role = RO.Role_ID " +
                                      "left join c_auth_object as AO on RO.Auth_ID = AO.ID " +
                                      "left join m_user_bp_compcode as UB on UC.Userid = UB.Userid " +
                                      "where UC.Userid = '" + username + "'";
                                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                                rdr = cmd.ExecuteReader();
                                while(rdr.Read())
                                {
                                    Loginresponse lResp = new Loginresponse();
                                    lResp.BPCode = rdr["BPCode"].ToString();
                                    lResp.Auth_Control = Convert.ToInt32(rdr["Auth_Control"]);
                                    lResp.Label = rdr["Label"].ToString();
                                    lResp.Page_route = rdr["Page_route"].ToString();
                                    lResp.Icon = rdr["Icon"].ToString();
                                    Response.Add(lResp);
                                }
                            }
                            else
                                return StatusCode(700, "Your access is expired");
                        }
                        else
                            return StatusCode(700, "You are not active");
                    }
                    else
                        return StatusCode(700, "Invalid Password");
                }
                else
                {
                    return StatusCode(700, "Invalid User Name");
                }
                return Ok(Response);
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

        // GET api/<LoginController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<LoginController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LoginController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LoginController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
