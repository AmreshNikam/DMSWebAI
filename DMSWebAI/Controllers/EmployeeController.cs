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
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public EmployeeController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        [HttpGet]
        public IActionResult Get()
        {
            List<Emloyees> employees = new List<Emloyees>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
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
                catch (Exception ) { return StatusCode(700, "Invalid Business Patner"); }
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
                sql = "select BP.BPCode, BP.CompCode, BP.PlantCode, BP.ActGroupID, BP.BPFName, BP.BPMName, BP.BPLName, BP.ShortKey, BP.GSTNo, BP.Phone, BP.Email, BP.CountryID, BP.StateID, BP.City, BP.RegionID, BP.Street, BP.AddressLine, BP.PostalCode, BP.Status, " +
                             "AG.ActGroupDesc, CN.CountryDesc, StateDesc, RegionDesc, BD.DesigID, BD.SeniorBPCode, DG.Description as Designation, " +
                             "concat(if (BP1.BPFName is null, '', BP1.BPFName), if (BP1.BPMName is null, '', BP1.BPMName), if (BP1.BPLName is null, '', BP1.BPLName)) as SeniorName " +

                             "from t_business_partner as BP " +
                             "left join c_accountgroup AG on BP.ActGroupID = AG.ActGroupID and BP.CompCode = AG.CompCode " +
                             "left join c_country as CN on BP.CountryID = CN.CountryID " +
                             "left join c_state as ST on BP.StateID = ST.StateID and BP.CountryID = ST.CountryID " +
                             "left join c_region RG on BP.RegionId = RG.RegionId and BP.CountryID = RG.CountryID " +
                             "left join t_bp_designation as BD on BP.BPCode = BD.BPCode and BP.CompCode = BD.CompCode " +
                             "left join c_designation DG on BD.DesigID = DG.DesigID and BD.CompCode = DG.CompCode " +
                             "left join t_business_partner as BP1 on BD.SeniorBPCode = BP1.BPCode and BD.CompCode = BP1.CompCode " +
                             "where BP.ActGroupID = 'EM' and BP.Status != 999" + condition;
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Emloyees emp = new Emloyees();
                    emp.BPCode = rdr["BPCode"].ToString();
                    emp.CompCode = rdr["CompCode"].ToString();
                    emp.PlantCode = rdr["PlantCode"].ToString();
                    emp.ActGroupID = rdr["ActGroupID"].ToString();
                    emp.ActGroupDesc = rdr["ActGroupDesc"].ToString();
                    emp.BPFName = rdr["BPFName"].ToString();
                    emp.BPMName = rdr["BPMName"].ToString();
                    emp.BPLName = rdr["BPLName"].ToString();
                    emp.ShortKey = rdr["ShortKey"].ToString();
                    emp.GSTNo = rdr["GSTNo"].ToString();
                    emp.Phone = rdr["Phone"].ToString();
                    emp.Email = rdr["Email"].ToString();
                    emp.CountryID = rdr["CountryID"].ToString();
                    emp.CountryDesc = rdr["CountryDesc"].ToString();
                    emp.StateID = rdr["StateID"].ToString();
                    emp.StateDesc = rdr["StateDesc"].ToString();
                    emp.City = rdr["City"].ToString();
                    emp.RegionID = rdr["RegionID"].ToString();
                    emp.RegionDesc = rdr["RegionDesc"].ToString();
                    emp.Street = rdr["Street"].ToString();
                    emp.AddressLine = rdr["AddressLine"].ToString();
                    emp.PostalCode = rdr["PostalCode"].ToString();
                    emp.Status = Convert.ToInt32(rdr["Status"]);
                    emp.ActGroupDesc = rdr["ActGroupDesc"].ToString();
                    emp.CountryDesc = rdr["CountryDesc"].ToString();
                    emp.StateDesc = rdr["StateDesc"].ToString();
                    emp.RegionDesc = rdr["RegionDesc"].ToString();
                    if(rdr["DesigID"] != DBNull.Value)
                        emp.DesigID = Convert.ToInt32(rdr["DesigID"]);
                    emp.Designation = rdr["Designation"].ToString();
                    emp.SeniorBPCode = rdr["SeniorBPCode"].ToString();
                    emp.SeniorName = rdr["SeniorName"].ToString();
                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    MySqlDataReader Subrdr;
                    Subconnection.Open();
                    emp.StateAssignments = new List<StateAssignment>();
                    sql = "select SA.CountryID, SA.StateID, CN.CountryDesc, ST.StateDesc " +
                          "from t_bp_state_assignment as SA " +
                          "left join c_country as CN on SA.CountryID = CN.CountryID " +
                          "left join c_state as ST on SA.StateID = ST.StateID and SA.CountryID = ST.CountryID " +
                          "where SA.BPCode = '" + emp.BPCode + "' and CompCode = '" + emp.CompCode + "'";
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    while (Subrdr.Read())
                    {
                        StateAssignment st = new StateAssignment();
                        st.CountryID = Subrdr["CountryID"].ToString();
                        st.CountryDesc = Subrdr["CountryDesc"].ToString();
                        st.StateID = Subrdr["StateID"].ToString();
                        st.StateDesc = Subrdr["StateDesc"].ToString();
                        emp.StateAssignments.Add(st);
                    }
                    Subrdr.Close();

                    sql = "select BR.BPCode, concat(if( BP.BPFName is null, '', BP.BPFName), if( BP.BPMName is null, '', BP.BPMName), if( BP.BPLName is null, '', BP.BPLName)) as DealerName, " +
                          "concat(if(BP.Street is null, '', BP.Street), ' ', if(BP.AddressLine is null, '', BP.AddressLine), ' ', if(BP.PostalCode is null, '', BP.PostalCode), ' PostalCode: ', if(BP.PostalCode is null, '', BP.PostalCode), ' City: ', if(BP.City is null, '', BP.City), ' State: ', if(ST.StateDesc is null, '', ST.StateDesc), ' Country: ', if(CN.CountryDesc is null, '', CN.CountryDesc)) as Address, " +
                          "BP.Email, BP.Phone " +
                          "from t_bp_relation as BR " +
                          "left join t_business_partner as BP on BR.BPCode = BP.BPCode and BR.CompCode = BP.CompCode " +
                          "left join c_country as CN on BP.CountryID = CN.CountryID " +
                          "left join c_state as ST on BP.StateID = ST.StateID and ST.CountryID = BP.CountryID " +
                          "where BR.RelationBPCode = '" + emp.BPCode + "' and BR.CompCode = '" + emp.CompCode + "'";
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    emp.Dealers = new List<BPCodeWithName>();
                    while (Subrdr.Read())
                    {
                        BPCodeWithName st = new BPCodeWithName();
                        st.BPCode = Subrdr["BPCode"].ToString();
                        st.Name = Subrdr["DealerName"].ToString();
                        st.Address = Subrdr["Address"].ToString();
                        st.Email = Subrdr["Email"].ToString();
                        st.Phone = Subrdr["Phone"].ToString();
                        emp.Dealers.Add(st);
                    }
                    Subrdr.Close();
                    Subconnection.Close();
                    employees.Add(emp);
                }
                rdr.Close();
                return Ok(employees);
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
        [HttpGet("GetSenior/{DesigID}")]
        public IActionResult GetSenior(int DesigID)
        {
            List<BPCodeWithName> bPCodeWithNames = new List<BPCodeWithName>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            try
            {
                connection.Open();
                string sql = "WITH RECURSIVE subordinate AS (  " +
                             "SELECT  SeniorDesgID, CompCode " +
                             "FROM  c_designation  " +
                             "WHERE DesigID = " + DesigID + " and CompCode = '" + CompanyID + "' " +
                             "UNION ALL  " +
                             "SELECT  e.SeniorDesgID, e.CompCode " +
                             "FROM c_designation e " +
                             "JOIN subordinate s ON e.DesigID = s.SeniorDesgID  and e.CompCode = s.CompCode " +
                             ") select BP.BPCode, " +
                             "concat(if( BP.BPFName is null, '', BP.BPFName), if( BP.BPMName is null, '', BP.BPMName), if( BP.BPLName is null, '', BP.BPLName)) as Name  " +
                             "from t_business_partner as BP where BPCode in " +
                             "(select BPCode from t_bp_designation where CompCode = '" + CompanyID + "' and DesigID in (select SeniorDesgID from subordinate)) "; ;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    BPCodeWithName bPCodeWithName = new BPCodeWithName();
                    bPCodeWithName.BPCode = rdr["BPCode"].ToString();
                    bPCodeWithName.Name = rdr["Name"].ToString();
                    bPCodeWithNames.Add(bPCodeWithName);
                }
                return Ok(bPCodeWithNames);
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
        public IActionResult Post([FromBody] Emloyees emp)
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
                cmd = new MySqlCommand("GetBusinessPatnerNumber", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                };
                cmd.Parameters.Add("?CompanyID", MySqlDbType.VarChar).Value = CompanyID;
                cmd.Parameters.Add(new MySqlParameter("mRes", MySqlDbType.String, 0));
                cmd.Parameters["mRes"].Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();
                string newBOCode = cmd.Parameters["mRes"].Value.ToString();
                //****************************Entery in t_business_partner************************************** 
                sql = "insert into t_business_partner (BPCode, CompCode, PlantCode, ActGroupID, BPFName, BPMName, BPLName, ShortKey, Phone, Email, CountryID, StateID, City, RegionID, Street, AddressLine, PostalCode, Status) values " +
                    "(@BPCode, @CompCode, @PlantCode, @ActGroupID, @BPFName, @BPMName, @BPLName, @ShortKey, @Phone, @Email, @CountryID, @StateID, @City, @RegionID, @Street, @AddressLine, @PostalCode, @Status)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = newBOCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = emp.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                cmd.Parameters.Add(new MySqlParameter("@BPFName", MySqlDbType.VarChar)).Value = emp.BPFName;
                cmd.Parameters.Add(new MySqlParameter("@BPMName", MySqlDbType.VarChar)).Value = emp.BPMName;
                cmd.Parameters.Add(new MySqlParameter("@BPLName", MySqlDbType.VarChar)).Value = emp.BPLName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = emp.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = emp.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = emp.Email;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = emp.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = emp.StateID;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = emp.City;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.VarChar)).Value = emp.RegionID;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = emp.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = emp.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = emp.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = emp.Status;
                cmd.ExecuteNonQuery();
                //****************************Entery in t_bp_designation************************************** 
                sql = "insert into t_bp_designation (CompCode, BPCODE, DesigID, SeniorBPCode) values " +
                   "(@CompCode, @BPCODE, @DesigID, @SeniorBPCode)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = newBOCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@DesigID", MySqlDbType.VarChar)).Value = emp.DesigID; // changed by sohel (from DesignationID to DesingID)
                cmd.Parameters.Add(new MySqlParameter("@SeniorBPCode", MySqlDbType.VarChar)).Value = emp.SeniorBPCode;
                cmd.ExecuteNonQuery();
                //****************************Entery in t_bp_state_assignment************************************** 
                if (emp.StateAssignments != null)
                {
                    foreach (StateAssignment state in emp.StateAssignments)
                    {
                        sql = "insert into t_bp_state_assignment (BPCode, CompCode, CountryID, StateID) values " +
                              "(@BPCode, @CompCode, @CountryID, @StateID)";
                        cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                        cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = newBOCode;
                        cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                        cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = state.CountryID;
                        cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = state.StateID;
                        cmd.ExecuteNonQuery();
                    }
                }
                //****************************Entery in t_bp_relation************************************** 
                if (emp.Dealers != null)
                {
                    foreach (BPCodeWithName dealer in emp.Dealers)
                    {
                        sql = "insert into t_bp_relation (CompCode, BPCode, RelationActGroupID, RelationBPCode, RelationBPDesignation) values " +
                              "(@CompCode, @BPCode, @RelationActGroupID, @RelationBPCode, @RelationBPDesignation)";
                        cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                        cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = dealer.BPCode;
                        cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                        cmd.Parameters.Add(new MySqlParameter("@RelationActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                        cmd.Parameters.Add(new MySqlParameter("@RelationBPCode", MySqlDbType.VarChar)).Value = newBOCode;
                        cmd.Parameters.Add(new MySqlParameter("@RelationBPDesignation", MySqlDbType.VarChar)).Value = emp.Designation;
                        cmd.ExecuteNonQuery();
                    }
                }
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
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Emloyees emp)
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
                //****************************update in t_business_partner************************************** 
                sql = "update t_business_partner set BPCode = @BPCode, CompCode = @CompCode, PlantCode = @PlantCode, ActGroupID = @ActGroupID, BPFName = @BPFName, BPMName = @BPMName, BPLName = @BPLName, ShortKey = @ShortKey, Phone = @Phone, Email = @Email, CountryID = @CountryID, StateID = @StateID, City = @City, RegionID = @RegionID, Street = @Street, AddressLine = @AddressLine, PostalCode = @PostalCode, Status = @Status where BPCODE = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = emp.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                cmd.Parameters.Add(new MySqlParameter("@BPFName", MySqlDbType.VarChar)).Value = emp.BPFName;
                cmd.Parameters.Add(new MySqlParameter("@BPMName", MySqlDbType.VarChar)).Value = emp.BPMName;
                cmd.Parameters.Add(new MySqlParameter("@BPLName", MySqlDbType.VarChar)).Value = emp.BPLName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = emp.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar)).Value = emp.Phone;
                cmd.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar)).Value = emp.Email;
                cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = emp.CountryID;
                cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = emp.StateID;
                cmd.Parameters.Add(new MySqlParameter("@City", MySqlDbType.VarChar)).Value = emp.City;
                cmd.Parameters.Add(new MySqlParameter("@RegionID", MySqlDbType.VarChar)).Value = emp.RegionID;
                cmd.Parameters.Add(new MySqlParameter("@Street", MySqlDbType.VarChar)).Value = emp.Street;
                cmd.Parameters.Add(new MySqlParameter("@AddressLine", MySqlDbType.VarChar)).Value = emp.AddressLine;
                cmd.Parameters.Add(new MySqlParameter("@PostalCode", MySqlDbType.VarChar)).Value = emp.PostalCode;
                cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int16)).Value = emp.Status;
                cmd.ExecuteNonQuery();
                //****************************update t_bp_designation************************************** 
                sql = "update t_bp_designation  set CompCode = @CompCode, BPCODE = @BPCODE, DesigID = @DesigID, SeniorBPCode = @SeniorBPCode where CompCode = '" + emp.CompCode + "' and BPCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@DesigID", MySqlDbType.VarChar)).Value = emp.Designation;
                cmd.Parameters.Add(new MySqlParameter("@SeniorBPCode", MySqlDbType.VarChar)).Value = emp.SeniorBPCode;
                cmd.ExecuteNonQuery();
                //****************************update in t_bp_state_assignment************************************** 
                sql = "delete from t_bp_state_assignment where CompCode = '" + emp.CompCode + "' and BPCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                foreach (StateAssignment state in emp.StateAssignments)
                {
                    sql = "insert into t_bp_state_assignment (BPCode, CompCode, CountryID, StateID) values " +
                          "(@BPCode, @CompCode, @CountryID, @StateID)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                    cmd.Parameters.Add(new MySqlParameter("@CountryID", MySqlDbType.VarChar)).Value = state.CountryID;
                    cmd.Parameters.Add(new MySqlParameter("@StateID", MySqlDbType.VarChar)).Value = state.StateID;
                    cmd.ExecuteNonQuery();
                }
                //****************************update in t_bp_relation************************************** 
                sql = "delete from t_bp_relation where CompCode = '" + emp.CompCode + "' and RelationBPCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                foreach (BPCodeWithName dealer in emp.Dealers)
                {
                    sql = "insert into t_bp_relation (CompCode, BPCode, RelationActGroupID, RelationBPCode, RelationBPDesignation) values " +
                          "(@CompCode, @BPCode, @RelationActGroupID, @RelationBPCode, @RelationBPDesignation)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = dealer.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                    cmd.Parameters.Add(new MySqlParameter("@RelationActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                    cmd.Parameters.Add(new MySqlParameter("@RelationBPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@RelationBPDesignation", MySqlDbType.VarChar)).Value = emp.Designation;
                    cmd.ExecuteNonQuery();
                }
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
                //string sql = "Delete from t_business_partner where BPCode = '" + id + "'";
                string sql = "update t_business_partner set Status = 999 where BPCode = '" + id + "'";
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
