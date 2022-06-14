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
    public class BusinessPatnerController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public BusinessPatnerController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<BusinessPatnerController>
        [HttpGet]
        public IActionResult Get([FromQuery] string AccountGroup)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            string condition = string.Empty;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            string sql = string.Empty;
            if (AuthControl == 0)
                return StatusCode(700, "You don't have access");
            else if (AuthControl == 1)
            {
                if (string.IsNullOrEmpty(BPCode))
                    return StatusCode(700, "You account is not associate with business patner");
                //check here BPCode is EM type or SP type
                string ActGroupID = string.Empty;
                try
                {
                    connection.Open();
                    sql = "select ActGroupID from t_business_partner where BPCode = '" + BPCode + "' and CompCode = '" + CompanyID + "'";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        ActGroupID = rdr["ActGroupID"].ToString();
                }
                catch (Exception) { return StatusCode(700, "Invalid Business Patner"); }
                finally { connection.Close(); }
                //if it is EM type
                if (ActGroupID == "EM")
                {
                    condition = "WITH RECURSIVE subordinate AS ( " +
                              "    SELECT  BP.BPCODE, BD.SeniorBPCode " +
                              "    FROM t_business_partner as BP left join t_bp_designation BD on BP.BPCode = BD.BPCode and BP.CompCode = BD.CompCode " +
                              "    WHERE BP.BPCODE = '" + BPCode + "' and BP.CompCode = '" + CompanyID + "'" +
                              "     UNION ALL " +
                              "     SELECT  e.BPCODE, d.SeniorBPCode " +
                              "    FROM t_bp_designation e left join t_bp_designation d on e.BPCode = d.BPCode and e.CompCode = d.CompCode " +
                              "JOIN subordinate s ON e.SeniorBPCode = s.BPCODE " +
                              ") select BPCODE from subordinate";
                    condition = " and BP.BPCode in ( " + condition + ")";
                }
                else if (ActGroupID == "SP")//if it is SP type
                {
                    condition = " and BP.BPCode in ('" + BPCode + "')"; //find all employee which s assigned to delar
                }
                else
                    return StatusCode(700, "You don't have access");
            }
            else
                condition = "";
            try
            {
                connection.Open();
                sql = "select BP.BPCode, BP.CompCode, BP.PlantCode, BP.ActGroupID, BP.BPFName, BP.BPMName, BP.BPLName, BP.ShortKey, BP.GSTNo, BP.Phone, BP.Email, BP.CountryID, " +
                       "BP.StateID, BP.City, BP.RegionID, BP.Street, BP.AddressLine, BP.PostalCode, BP.Status, AG.ActGroupDesc, CN.CountryDesc, StateDesc, RegionDesc " +
                       "from t_business_partner as BP " +
                       "left join c_accountgroup AG on BP.ActGroupID = AG.ActGroupID and BP.CompCode = AG.CompCode " +
                       "left join c_country as CN on BP.CountryID = CN.CountryID " +
                       "left join c_state as ST on BP.StateID = ST.StateID and BP.CountryID = ST.CountryID " +
                       "left join c_region RG on BP.RegionId = RG.RegionId and BP.CountryID = RG.CountryID " +
                       "where BP.ActGroupID = 'EM' and BP.Status != 999" + "and BP.CompCode = '" + CompanyID + "'" + condition;

                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                List<BusinessPatner> bps = new List<BusinessPatner>();
                while (rdr.Read())
                {
                    BusinessPatner bp = new BusinessPatner();
                    bp.BPCode = rdr["BPCode"].ToString();
                    bp.Name = rdr["Name"].ToString();
                    bps.Add(bp);
                }
                return Ok(bpName);
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

        // GET api/<BusinessPatnerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<BusinessPatnerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<BusinessPatnerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BusinessPatnerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
