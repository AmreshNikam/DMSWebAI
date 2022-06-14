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
    public class RoleController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public RoleController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<RoleController>
        [HttpGet]
        public IActionResult Get()
        {
            List<Role> roles = new List<Role>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("Userid", out var Userid);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            //if (AuthControl != 2) // check create permission
            //{
            //    return StatusCode(700, "You don't have access to create user");
            //}
            try
            {
                connection.Open();
                string sql = "select RoleID, CompCode, RoleName from c_role where CompCode = '" + CompanyID + "'";
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Role role = new Role();
                    role.RoleID = Convert.ToInt32(rdr["RoleID"]);
                    role.CompCode = rdr["CompCode"].ToString();
                    role.RoleName = rdr["RoleName"].ToString();
                    roles.Add(role);
                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    try
                    {
                        
                        Subconnection.Open();
                        sql = "select Auth_ID, Auth_Control, Label, Page_route, Icon " +
                              "from m_role_auth_object as RA " +
                              "left join c_auth_object as AO on RA.Auth_ID = AO.ID " +
                              "where Role_ID = " + role.RoleID;
                        MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                        MySqlDataReader Subrdr = Subcmd.ExecuteReader();
                        role.AuthObjects = new List<AuthObjectAccess>();
                        while (Subrdr.Read())
                        {
                            AuthObjectAccess AO = new AuthObjectAccess();
                            AO.AuthObjectID = Convert.ToInt32(Subrdr["Auth_ID"]);
                            AO.Auth_Control = Convert.ToInt32(Subrdr["Auth_Control"]);
                            AO.Label = Subrdr["Label"].ToString();
                            AO.Page_route = Subrdr["Page_route"].ToString();
                            AO.Icon = Subrdr["Icon"].ToString();
                            role.AuthObjects.Add(AO);
                        }
                    }
                    catch(Exception) { return StatusCode(500, "Some problem at to get assigne Auth Objects"); }
                    finally { Subconnection.Close(); }
                }
                return Ok(roles);
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

        // GET api/<RoleController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
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
                string sql = "select RoleID, CompCode, RoleName from c_role where RoleID = " + id;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                Role role = new Role();
                if (rdr.Read())
                {

                    role.RoleID = Convert.ToInt32(rdr["RoleID"]);
                    role.CompCode = rdr["CompCode"].ToString();
                    role.RoleName = rdr["RoleName"].ToString();
                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    try
                    {

                        Subconnection.Open();
                        sql = "select Auth_ID, Auth_Control, Label, Page_route, Icon " +
                              "from m_role_auth_object as RA " +
                              "left join c_auth_object as AO on RA.Auth_ID = AO.ID " +
                              "where Role_ID = " + role.RoleID;
                        MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                        MySqlDataReader Subrdr = Subcmd.ExecuteReader();
                        role.AuthObjects = new List<AuthObjectAccess>();
                        while (Subrdr.Read())
                        {
                            AuthObjectAccess AO = new AuthObjectAccess();
                            AO.AuthObjectID = Convert.ToInt32(Subrdr["Auth_ID"]);
                            AO.Auth_Control = Convert.ToInt32(Subrdr["Auth_Control"]);
                            AO.Label = Subrdr["Label"].ToString();
                            AO.Page_route = Subrdr["Page_route"].ToString();
                            AO.Icon = Subrdr["Icon"].ToString();
                            role.AuthObjects.Add(AO);
                        }
                    }
                    catch (Exception) { return StatusCode(500, "Some problem at to get assigne Auth Objects"); }
                    finally { Subconnection.Close(); }
                }
                return Ok(role);
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
            // POST api/<RoleController>
            [HttpPost]
        public IActionResult Post([FromBody] Role role)
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
                sql = "insert into c_role (CompCode, RoleName) values (@CompCode, @RoleName)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = role.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@RoleName", MySqlDbType.VarChar)).Value = role.RoleName;
                cmd.ExecuteScalar();
                int newid = (int) cmd.LastInsertedId;
                //***************************inset in m_role_auth_object*************************
                if(role.AuthObjects != null)
                {
                    foreach (AuthObjectAccess AO in role.AuthObjects)
                    {
                        if (AO.AuthObjectID != null)
                        {
                            sql = "insert into m_role_auth_object (Role_ID, Auth_ID, Auth_Control) values (@Role_ID, @Auth_ID, @Auth_Control)";
                            cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                            cmd.Parameters.Add(new MySqlParameter("@Role_ID", MySqlDbType.Int32)).Value = newid;
                            cmd.Parameters.Add(new MySqlParameter("@Auth_ID", MySqlDbType.Int32)).Value = AO.AuthObjectID;
                            cmd.Parameters.Add(new MySqlParameter("@Auth_Control", MySqlDbType.Int32)).Value = AO.Auth_Control;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

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

        // PUT api/<RoleController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Role role)
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
                sql = "update c_role set RoleID = @RoleID, CompCode = @CompCode, RoleName = @RoleName where Role_ID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@RoleName", MySqlDbType.Int32)).Value = id;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = role.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@RoleName", MySqlDbType.VarChar)).Value = role.RoleName;
                cmd.ExecuteNonQuery();
                //***************************inset in m_role_auth_object*************************
                sql = "delete from m_role_auth_object where Role_ID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                if (role.AuthObjects != null)
                {
                    foreach (AuthObjectAccess AO in role.AuthObjects)
                    {
                        if (AO.AuthObjectID != null)
                        {
                            sql = "insert into m_role_auth_object (Role_ID, Auth_ID, Auth_Control) values (@Role_ID, @Auth_ID, @Auth_Control)";
                            cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                            cmd.Parameters.Add(new MySqlParameter("@Role_ID", MySqlDbType.Int32)).Value = id;
                            cmd.Parameters.Add(new MySqlParameter("@Auth_ID", MySqlDbType.Int32)).Value = AO.AuthObjectID;
                            cmd.Parameters.Add(new MySqlParameter("@Auth_Control", MySqlDbType.Int32)).Value = AO.Auth_Control;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

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
        // DELETE api/<RoleController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
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
                //***************************inset in m_role_auth_object*************************
                sql = "delete from m_role_auth_object where Role_ID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                sql = "delete from c_role where RoleID = " + id;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                return Ok("Record deleted sucessfuly");
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
