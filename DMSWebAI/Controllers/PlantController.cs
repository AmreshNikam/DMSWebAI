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
    public class PlantController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public PlantController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // GET: api/<CompanyController>
        [HttpGet]
        public IActionResult GetAllPlant()
        {
            List<DetailsPlant> company = new List<DetailsPlant>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "select PL.PlantCode, PL.Name as PlantName, CO.ComapCode, CO.Name as CompanyName, " +
                             "PL.ShortKey, RG.RegionId, RG.RegionName, PL.AddrNo, PL.CIN, PL.Phone, PL.Email, PL.GSTNo, " +
                             "AD.AddressLine_1, AD.AddressLine_2, AD.PostalCode, " +
                             "CT.id as CityId, CT.name as City, ST.id as StateId, ST.name as State, CN.id as Country_id, CN.name as Country, PL.Status " +
                             "from plant as PL " +
                             "left join company as CO on PL.CompCode = CO.ComapCode " +
                             "left join region as RG on CO.RegionId = RG.RegionId " +
                             "left join address as AD on CO.AddrNo = AD.AddrNo " +
                             "left join cities as CT on AD.City = CT.id " +
                             "left join states as ST on CT.state_id = ST.id " +
                             "left join countries CN on ST.country_id = CN.id";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DetailsPlant cmp = new DetailsPlant();

                    cmp.ComapCode = rdr["ComapCode"].ToString();
                    cmp.CompanyName = rdr["CompanyName"].ToString();
                    cmp.PlantCode = rdr["PlantCode"].ToString();
                    cmp.PlantName = rdr["PlantName"].ToString();
                    cmp.ShortKey = rdr["ShortKey"].ToString();
                    if (rdr["RegionId"] == DBNull.Value)
                        cmp.RegionId = null;
                    else
                        cmp.RegionId = Convert.ToInt32(rdr["RegionId"]);
                    cmp.RegionName = rdr["RegionName"].ToString();
                    if (rdr["AddrNo"] == DBNull.Value)
                        cmp.AddrNo = null;
                    else
                        cmp.AddrNo = Convert.ToInt32(rdr["AddrNo"]);
                    cmp.CIN = rdr["CIN"].ToString();
                    cmp.Phone = rdr["Phone"].ToString();
                    cmp.Email = rdr["Email"].ToString();
                    cmp.GSTNo = rdr["GSTNo"].ToString();
                    cmp.AddressLine_1 = rdr["AddressLine_1"].ToString();
                    cmp.AddressLine_2 = rdr["AddressLine_2"].ToString();
                    cmp.PostalCode = rdr["PostalCode"].ToString();
                    if (rdr["CityId"] == DBNull.Value)
                        cmp.CityId = null;
                    else
                        cmp.CityId = Convert.ToInt32(rdr["CityId"]);
                    cmp.City = rdr["City"].ToString();
                    if (rdr["StateId"] == DBNull.Value)
                        cmp.StateId = null;
                    else
                        cmp.StateId = Convert.ToInt32(rdr["StateId"]);
                    cmp.State = rdr["State"].ToString();
                    if (rdr["Country_id"] == DBNull.Value)
                        cmp.Country_id = null;
                    else
                        cmp.Country_id = Convert.ToInt32(rdr["Country_id"]);
                    cmp.Country = rdr["Country"].ToString();
                    cmp.Status = Convert.ToBoolean(rdr["Status"]);

                    company.Add(cmp);
                }
                return Ok(company);
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
        [HttpGet("{Cid}/{Pid}")]
        public IActionResult GetCompany(string Cid, string Pid)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                string sql = "select PL.PlantCode, PL.Name as PlantName, CO.ComapCode, CO.Name as CompanyName, " +
                             "PL.ShortKey, RG.RegionId, RG.RegionName, PL.AddrNo, PL.CIN, PL.Phone, PL.Email, PL.GSTNo, " +
                             "AD.AddressLine_1, AD.AddressLine_2, AD.PostalCode, " +
                             "CT.id as CityId, CT.name as City, ST.id as StateId, ST.name as State, CN.id as Country_id, CN.name as Country, PL.Status " +
                             "from plant as PL " +
                             "left join company as CO on PL.CompCode = CO.ComapCode " +
                             "left join region as RG on CO.RegionId = RG.RegionId " +
                             "left join address as AD on CO.AddrNo = AD.AddrNo " +
                             "left join cities as CT on AD.City = CT.id " +
                             "left join states as ST on CT.state_id = ST.id " +
                             "left join countries CN on ST.country_id = CN.id " +
                             "where CO.ComapCode = '" + Cid + "' and PL.PlantCode = '" + Pid + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                DetailsPlant cmp = new DetailsPlant();
                if (rdr.Read())
                {
                    cmp.ComapCode = rdr["ComapCode"].ToString();
                    cmp.CompanyName = rdr["CompanyName"].ToString();
                    cmp.PlantCode = rdr["PlantCode"].ToString();
                    cmp.PlantName = rdr["PlantName"].ToString();
                    cmp.ShortKey = rdr["ShortKey"].ToString();
                    if (rdr["RegionId"] == DBNull.Value)
                        cmp.RegionId = null;
                    else
                        cmp.RegionId = Convert.ToInt32(rdr["RegionId"]);
                    cmp.RegionName = rdr["RegionName"].ToString();
                    if (rdr["AddrNo"] == DBNull.Value)
                        cmp.AddrNo = null;
                    else
                        cmp.AddrNo = Convert.ToInt32(rdr["AddrNo"]);
                    cmp.CIN = rdr["CIN"].ToString();
                    cmp.Phone = rdr["Phone"].ToString();
                    cmp.Email = rdr["Email"].ToString();
                    cmp.GSTNo = rdr["GSTNo"].ToString();
                    cmp.AddressLine_1 = rdr["AddressLine_1"].ToString();
                    cmp.AddressLine_2 = rdr["AddressLine_2"].ToString();
                    cmp.PostalCode = rdr["PostalCode"].ToString();
                    if (rdr["CityId"] == DBNull.Value)
                        cmp.CityId = null;
                    else
                        cmp.CityId = Convert.ToInt32(rdr["CityId"]);
                    cmp.City = rdr["City"].ToString();
                    if (rdr["StateId"] == DBNull.Value)
                        cmp.StateId = null;
                    else
                        cmp.StateId = Convert.ToInt32(rdr["StateId"]);
                    cmp.State = rdr["State"].ToString();
                    if (rdr["Country_id"] == DBNull.Value)
                        cmp.Country_id = null;
                    else
                        cmp.Country_id = Convert.ToInt32(rdr["Country_id"]);
                    cmp.Country = rdr["Country"].ToString();
                    cmp.Status = Convert.ToBoolean(rdr["Status"]);
                }
                return Ok(cmp);
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
        public IActionResult Post([FromBody] DetailsPlant plant)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            string sql;
            try
            {
                connection.Open();
                mytrans = connection.BeginTransaction();
                sql = "insert into address (City, AddressLine_1, AddressLine_2, PostalCode, Defaults) values (@City, @AddressLine_1, @AddressLine_2, @PostalCode, @Defaults)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.Int32)).Value = plant.CityId;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine_1", MySqlDbType.VarChar)).Value = plant.AddressLine_1;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine_2", MySqlDbType.VarChar)).Value = plant.AddressLine_2;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = plant.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@Defaults", MySqlDbType.Int16)).Value = 1;
                cmd.ExecuteScalar();
                int id = (int)cmd.LastInsertedId;

                sql = "insert into plant (PlantCode, ComapCode, Name, ShortKey, RegionID, AddrNo, CIN, Phone, Email, GSTNo, Status) values (@PlantCode,@ComapCode, @Name, @ShortKey, @RegionID, @AddrNo, @CIN, @Phone, @Email, @GSTNo, @Status)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = plant.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ComapCode", MySqlDbType.VarChar)).Value = plant.ComapCode;
                cmd.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.VarChar)).Value = plant.PlantName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = plant.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.Int32)).Value = plant.RegionId;
                cmd.Parameters.Add(new MySqlParameter("@AddrNo", MySqlDbType.Int32)).Value = id;
                cmd.Parameters.Add(new MySqlParameter("@CIN", MySqlDbType.VarChar)).Value = plant.CIN;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = plant.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = plant.Email;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = plant.GSTNo;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = plant.Status;
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

        // PUT api/<CompanyController>/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] DetailsPlant plant)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            string sql;
            try
            {
                connection.Open();
                mytrans = connection.BeginTransaction();
                sql = "update address set City = @City, AddressLine_1 = @AddressLine_1, AddressLine_2 = @AddressLine_2, PostalCode = @PostalCode, Defaults = @Defaults where AddrNo = " + plant.AddrNo;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.Int32)).Value = plant.CityId;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine_1", MySqlDbType.VarChar)).Value = plant.AddressLine_1;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine_2", MySqlDbType.VarChar)).Value = plant.AddressLine_2;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = plant.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@Defaults", MySqlDbType.Int16)).Value = 1;
                cmd.ExecuteNonQuery();

                sql = "update plant set PlantCode = #PlantCode, ComapCode = @ComapCode, Name = @Name, ShortKey = @ShortKey, RegionID = @RegionID, CIN = @CIN, Phone = @Phone, Email = @Email, GSTNo = @GSTNo, Status = @Status where PlantCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = plant.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ComapCode", MySqlDbType.VarChar)).Value = plant.ComapCode;
                cmd.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.VarChar)).Value = plant.PlantName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = plant.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.Int32)).Value = plant.RegionId;
                cmd.Parameters.Add(new MySqlParameter("@CIN", MySqlDbType.VarChar)).Value = plant.CIN;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = plant.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = plant.Email;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = plant.GSTNo;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = plant.Status;
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
        // DELETE api/<CompanyController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                string sql = "Delete from plant where PlantCode = '" + id + "'";
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
