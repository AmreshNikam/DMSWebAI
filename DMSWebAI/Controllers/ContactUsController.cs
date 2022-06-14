using DMSWebAI.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly IConfiguration Configuration;
        public ContactUsController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        [HttpGet]
        public IActionResult Get([FromQuery] DateTime? StartDate = null, DateTime? EndDate = null)
        {
            string sql = string.Empty;
            if (StartDate != null && EndDate != null)
                sql = "select CU.Name, CU.Email, CU.Phone, CU.Support, CU.City, CU.CountryID, CU.StateID, CU.Subject, CU.LongText, CU.DateOfPosting, " +
                      "ST.StateDesc, CN.CountryDesc " +
                    "from t_contact_us as CU " +
                    "Left join c_country as CN on CU.CountryID = CN.CountryID " +
                    "Left join c_state as ST on CU.StateID = ST.StateID and CU.CountryID = ST.CountryID " +
                    "where DateOfPosting between '" + StartDate.Value.ToString("yyyy-MM-dd") + "' and '" + EndDate.Value.ToString("yyyy-MM-dd") + "'";
            else
                sql = "select * from t_contact_us";
            List<ContactUs> contactUs = new List<ContactUs>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            if (AuthControl != 2) // check create permission
            {
                return StatusCode(700, "You don't have access");
            }
            try
            {
                connection.Open();
                //string sql = "select AccGroupID, AccGroup, Descs, CompCode, PlantCode from c_accountgroup";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    ContactUs contact = new ContactUs();
                    contact.Name = rdr["Name"].ToString();
                    contact.Email = rdr["Email"].ToString();
                    contact.Phone = rdr["Phone"].ToString();
                    contact.Support = rdr["Support"].ToString();
                    contact.City = rdr["City"].ToString();
                    contact.CountryID = rdr["CountryID"].ToString();
                    contact.Country = rdr["CountryDesc"].ToString();
                    contact.StateID = rdr["StateID"].ToString();
                    contact.State = rdr["StateDesc"].ToString();
                    contact.Subject = rdr["Subject"].ToString();
                    contact.LongText = rdr["LongText"].ToString();
                    contact.DateOfPosting = Convert.ToDateTime(rdr["DateOfPosting"]);
                    contactUs.Add(contact);
                }
                return Ok(contactUs);
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
        [HttpPost]
        public IActionResult Post([FromBody] ContactUs contactUs)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            string sql;
            try
            {
                connection.Open();
                sql = "insert into t_contact_us (Name, Email, Phone, Support, City, CountryID, StateID, Subject, LongText, DateOfPosting) " +
                    "values (@Name, @Email, @Phone, @Support, @City, @CountryID, @StateID, @Subject, @LongText, @DateOfPosting)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.VarChar)).Value = contactUs.Name;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = contactUs.Email;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = contactUs.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Support", MySqlDbType.VarChar)).Value = contactUs.Support;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = contactUs.City;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = contactUs.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = contactUs.StateID;
                cmd.Parameters.Add(new MySqlParameter("@Subject", MySqlDbType.VarChar)).Value = contactUs.Subject;
                cmd.Parameters.Add(new MySqlParameter("@LongText", MySqlDbType.VarChar)).Value = contactUs.LongText;
                cmd.Parameters.Add(new MySqlParameter("@DateOfPosting", MySqlDbType.Date)).Value = DateTime.Now.Date;
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
    }
}
