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
    public class EmployeeDesignationsController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public EmployeeDesignationsController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet("{Pid}")]
        public IActionResult Get(string Pid)
        {
            List<EmployeeDesignations> designations = new List<EmployeeDesignations>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            try
            {
                connection.Open();
                string sql = "select ED1.DesigID, ED1.ShortCode, ED1.Description, ED1.SeniorDesgID, ED2.ShortCode as Senior, ED1.CompCode, ED1.PlantCode " +
                             "from employeedesignations as ED1 " +
                             "left join employeedesignations as ED2 on ED1.SeniorDesgID = ED2.DesigID and ED1.CompCode = ED2.CompCode and ED1.PlantCode = ED2.PlantCode " +
                             "where ED1.CompCode = '" + CompanyID + "' and ED1.PlantCode = '" + Pid + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    EmployeeDesignations desg = new EmployeeDesignations();
                    desg.DesigID = Convert.ToInt32(rdr["DesigID"]);
                    desg.ShortCode = rdr["ShortCode"].ToString();
                    desg.Description = rdr["Description"].ToString();
                    if (rdr["SeniorDesgID"] == DBNull.Value)
                        desg.SeniorDesgID = null;
                    else
                        desg.SeniorDesgID = Convert.ToInt32(rdr["SeniorDesgID"]);
                    desg.Senior = rdr["Senior"].ToString();
                    desg.CompCode = rdr["CompCode"].ToString();
                    desg.PlantCode = rdr["PlantCode"].ToString();
                    designations.Add(desg);
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
       
        // POST api/<CompanyController>
        [HttpPost]
        public IActionResult Post([FromBody] EmployeeDesignations designations)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
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
                sql = "insert into employeedesignations (ShortCode, Description, SeniorDesgID, CompCode, PlantCode) values (@ShortCode, @Description, @SeniorDesgID, @CompCode, @PlantCode)";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@ShortCode", MySqlDbType.VarChar)).Value = designations.ShortCode;
                cmd.Parameters.Add(new MySqlParameter("@Description", MySqlDbType.VarChar)).Value = designations.Description;
                if(designations.SeniorDesgID == null)
                    cmd.Parameters.Add(new MySqlParameter("@SeniorDesgID", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@SeniorDesgID", MySqlDbType.Int32)).Value = designations.SeniorDesgID;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = designations.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = designations.PlantCode;
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
        public IActionResult Put(int id, [FromBody] EmployeeDesignations designations)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
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

                sql = "update employeedesignations set DesigID = @DesigID, ShortCode = @ShortCode, Description = @Description, SeniorDesgID = @SeniorDesgID, CompCode = @CompCode, PlantCode = @PlantCode where DesigID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@ShortCode", MySqlDbType.VarChar)).Value = designations.ShortCode;
                cmd.Parameters.Add(new MySqlParameter("@Description", MySqlDbType.VarChar)).Value = designations.Description;
                if (designations.SeniorDesgID == null)
                    cmd.Parameters.Add(new MySqlParameter("@SeniorDesgID", MySqlDbType.Int32)).Value = DBNull.Value;
                else
                    cmd.Parameters.Add(new MySqlParameter("@SeniorDesgID", MySqlDbType.Int32)).Value = designations.SeniorDesgID;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = designations.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = designations.PlantCode;
                cmd.ExecuteNonQuery();

                return Ok("Sucessfuly udated");
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
                string sql = "Delete from employeedesignations where DesigID = " + id;
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
