
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
    public class DealerController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public DealerController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<DealerController>
        [HttpGet]
        public IActionResult Get()
        {
            List<DealerDetails> dealerDetails = new List<DealerDetails>();
            string connString = this.Configuration.GetConnectionString("DMS");
            string condition = string.Empty;
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
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
                              ") select BPCode from t_bp_relation where RelationBPCode in (select BPCODE from subordinate) and CompCode = '" + CompanyID + "'";
                    condition = " and BP.BPCode in ( " + condition + ")";
                }
                else if (ActGroupID == "SP")//if it is SP type
                {
                    condition = " and BP.BPCode in ('" + BPCode + "')";
                }
                else
                    return StatusCode(700, "You don't have access");
            }
            else
                condition = " and BP.CompCode = '" + CompanyID + "'";
            try
            {
                connection.Open();
                sql = "select " +
                      "BP.BPCode, BP.CompCode, BP.PlantCode, BP.ActGroupID, AG.ActGroupDesc, BP.BPFName, BP.BPMName, BP.BPLName, " +
                      "BP.ShortKey, BP.GSTNo, BP.Phone, BP.Email, BP.CountryID, CN.CountryDesc, BP.StateID, ST.StateDesc, " +
                      "BP.City, BP.RegionID, RG.RegionDesc, BP.Street, BP.AddressLine, BP.PostalCode, BP.Status " +
                      "from t_business_partner as BP " +
                      "left join c_accountgroup as AG on BP.ActGroupID = AG.ActGroupID and BP.CompCode = AG.CompCode " +
                      "left join c_country CN on BP.CountryID = CN.CountryID " +
                      "left join c_state as ST on BP.StateID = ST.StateID and CN.CountryID = ST.CountryID " +
                      "left join c_region as RG on BP.RegionID = RG.RegionID " +
                      "where BP.ActGroupID = 'SP' and BP.Status != 999" + condition;
                //Role Base Selecton
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DealerDetails dealer = new DealerDetails();
                    dealer.BPCode = rdr["BPCode"].ToString();
                    dealer.CompCode = rdr["CompCode"].ToString();
                    dealer.PlantCode = rdr["PlantCode"].ToString();
                    dealer.ActGroupID = rdr["ActGroupID"].ToString();
                    dealer.ActGroupDesc = rdr["ActGroupDesc"].ToString();
                    dealer.BPFName = rdr["BPFName"].ToString();
                    dealer.BPMName = rdr["BPMName"].ToString();
                    dealer.BPMName = rdr["BPMName"].ToString();
                    dealer.ShortKey = rdr["ShortKey"].ToString();
                    dealer.GSTNo = rdr["GSTNo"].ToString();
                    dealer.Phone = rdr["Phone"].ToString();
                    dealer.Email = rdr["Email"].ToString();
                    dealer.CountryID = rdr["CountryID"].ToString();
                    dealer.CountryDesc = rdr["CountryDesc"].ToString();
                    dealer.StateID = rdr["StateID"].ToString();
                    dealer.StateDesc = rdr["StateDesc"].ToString();
                    dealer.City = rdr["City"].ToString();
                    dealer.RegionID = rdr["RegionID"].ToString();
                    dealer.RegionDesc = rdr["RegionDesc"].ToString();
                    dealer.Street = rdr["Street"].ToString();
                    dealer.AddressLine = rdr["AddressLine"].ToString();
                    dealer.PostalCode = rdr["PostalCode"].ToString();
                    dealer.Status = Convert.ToInt32(rdr["Status"]);

                    MySqlConnection Subconnection = new MySqlConnection(connString);
                    MySqlCommand Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    MySqlDataReader Subrdr;
                    sql = "select RelationBPCode, BR.RelationActGroupID, " +
                      "concat(if(BP.BPFName is null, '', BP.BPFName), if(BP.BPMName is null, '', BP.BPFName), if(BP.BPMName is null, '', BP.BPLName)) as Name, " +
                      "BP.ShortKey, BP.GSTNo, BP.Phone, BP.Email, BP.CountryID, CN.CountryDesc, BP.StateID, ST.StateDesc, " +
                      "BP.City, BP.RegionID, RG.RegionDesc, BP.Street, BP.AddressLine, BP.PostalCode, RelationBPDesignation " +
                      "from t_bp_relation as BR " +
                      "left join t_business_partner as BP on BR.RelationBPCode = BP.BPCode and BR.CompCode = BP.CompCode " +
                      "left join c_country CN on BP.CountryID = CN.CountryID " +
                      "left join c_state as ST on BP.StateID = ST.StateID and CN.CountryID = ST.CountryID " +
                      "left join c_region as RG on BP.RegionID = RG.RegionID " +
                      "where BR.BPCode = '" + dealer.BPCode + "' and (BR.RelationActGroupID = 'EM' or BR.RelationActGroupID = 'CP' or BR.RelationActGroupID = 'SH')";
                    Subconnection.Open();
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    dealer.Contact = new List<DealerAssigned>();
                    while (Subrdr.Read())
                    {
                        DealerAssigned da = new DealerAssigned();
                        da.RelationBPCode = Subrdr["RelationBPCode"].ToString();
                        da.RelationActGroupID = Subrdr["RelationActGroupID"].ToString();
                        da.Name = Subrdr["Name"].ToString();
                        da.ShortKey = Subrdr["ShortKey"].ToString();
                        da.GSTNo = Subrdr["GSTNo"].ToString();
                        da.Phone = Subrdr["Phone"].ToString();
                        da.Email = Subrdr["Email"].ToString();
                        da.CountryID = Subrdr["CountryID"].ToString();
                        da.CountryDesc = Subrdr["CountryDesc"].ToString();
                        da.StateID = Subrdr["StateID"].ToString();
                        da.StateDesc = Subrdr["StateDesc"].ToString();
                        da.City = Subrdr["City"].ToString();
                        da.RegionID = Subrdr["RegionID"].ToString();
                        da.RegionDesc = Subrdr["RegionDesc"].ToString();
                        da.Street = Subrdr["Street"].ToString();
                        da.AddressLine = Subrdr["AddressLine"].ToString();
                        da.PostalCode = Subrdr["PostalCode"].ToString();
                        da.RelationBPDesignation = Subrdr["RelationBPDesignation"].ToString();
                        dealer.Contact.Add(da);
                    }
                    Subrdr.Close();
                    sql = "select BS.ProductGroupID, PG.Description from t_bp_segment as BS " +
                          "left join c_product_group as PG on BS.ProductGroupID = PG.ProductGroupID and BS.CompCode = PG.CompCode " +
                          "where BS.BPCode = '" + dealer.BPCode + "' and BS.CompCode = '" + dealer.CompCode + "'";
                    Subcmd = new MySqlCommand(sql, Subconnection) { CommandType = CommandType.Text };
                    Subrdr = Subcmd.ExecuteReader();
                    dealer.ProductGroup = new List<DealerProductGroup>();
                    while (Subrdr.Read())
                    {
                        DealerProductGroup pg = new DealerProductGroup();
                        pg.ProductGroupID = Subrdr["ProductGroupID"].ToString();
                        pg.Description = Subrdr["Description"].ToString();
                        dealer.ProductGroup.Add(pg);
                    }
                    Subrdr.Close();
                    Subconnection.Close();
                    dealerDetails.Add(dealer);
                }
                rdr.Close();
                return Ok(dealerDetails);
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
        [HttpPost("DealerStatewise")]
        public IActionResult GetDealerStatewise([FromBody] List<States> states)
        {
            List<BPCodeWithName> nameAddresses = new List<BPCodeWithName>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            try
            {
                connection.Open();
                foreach (States st in states)
                {
                    string sql = "select BPCode, concat(if (BP.BPFName is null, '', BP.BPFName), if (BP.BPMName is null, '', BP.BPMName), if (BP.BPLName is null, '', BP.BPLName)) as Name, " +
                                 "concat(if (BP.Street is null, '', BP.Street), ' ', if (BP.AddressLine is null, '', BP.AddressLine), ' ', if (BP.PostalCode is null, '', BP.PostalCode), ' PostalCode: ', if (BP.PostalCode is null, '', BP.PostalCode), ' City: ', if (BP.City is null, '', BP.City), ' State: ', if (ST.StateDesc is null, '', ST.StateDesc), ' Country: ', if (CN.CountryDesc is null, '', CN.CountryDesc)) as Address " +
                                 "from t_business_partner  as BP " +
                                 "left join c_country as CN on BP.CountryID = CN.CountryID " +
                                 "left join c_state as ST on BP.StateID = ST.StateID and BP.CountryID = ST.CountryID " +
                                 "where BP.CountryID = '" + st.CountryID + "' and BP.StateID = '" + st.StateID + "' and ActGroupID = 'SP' and BP.CompCode = '" + CompanyID + "'";
                    MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        BPCodeWithName name = new BPCodeWithName();

                        name.BPCode = rdr["BPCode"].ToString();
                        name.Name = rdr["Name"].ToString();
                        name.Address = rdr["Address"].ToString();
                        nameAddresses.Add(name);
                    }
                    rdr.Close();
                }
                return Ok(nameAddresses);
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
        // POST api/<DealerController>
        [HttpPost]
        public IActionResult Post([FromBody] DealerDetails emp)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            string sql = string.Empty;
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

                sql = "insert into t_business_partner (BPCode, CompCode, PlantCode, ActGroupID, BPFName, BPMName, BPLName, ShortKey, Phone, Email, CountryID, StateID, City, RegionID, Street, AddressLine, PostalCode, Status) values " +
                    "(@BPCode, @CompCode, @PlantCode, @ActGroupID, @BPFName, @BPMName, @BPLName, @ShortKey, @Phone, @Email, @CountryID, @StateID, @City, @RegionID, @Street, @AddressLine, @PostalCode, @Status)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = emp.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                cmd.Parameters.Add(new MySqlParameter("@BPFName", MySqlDbType.VarChar)).Value = emp.BPFName;
                cmd.Parameters.Add(new MySqlParameter("@BPMName", MySqlDbType.VarChar)).Value = emp.BPMName;
                cmd.Parameters.Add(new MySqlParameter("@BPLName", MySqlDbType.VarChar)).Value = emp.BPLName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = emp.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = emp.GSTNo;
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
                //****************************Entery in t_bp_segment************************************** 
                foreach (DealerProductGroup pg in emp.ProductGroup)
                {
                    sql = "Insert into t_bp_segment (CompCode, BPCode, ProductGroupID) values (@CompCode, @BPCode, @ProductGroupID)";
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@ProductGroupID", MySqlDbType.VarChar)).Value = pg.ProductGroupID;
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.ExecuteNonQuery();
                }
                //****************************Entery in t_bp_relation************************************** 

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

        // PUT api/<DealerController>/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] DealerDetails emp)
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
                sql = "update t_business_partner set BPCode = @BPCode, CompCode = @CompCode, PlantCode = @PlantCode, ActGroupID = @ActGroupID, BPFName = @BPFName, BPMName = @BPMName, BPLName = @BPLName, ShortKey = @ShortKey, GSTNo = @GSTNo, Phone = @Phone, Email = @Email, CountryID = @CountryID, StateID = @StateID, City = @City, RegionID = @RegionID, Street = @Street, AddressLine = @AddressLine, PostalCode = @PostalCode, Status = @Status where BPCODE = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@PlantCode", MySqlDbType.VarChar)).Value = emp.PlantCode;
                cmd.Parameters.Add(new MySqlParameter("@ActGroupID", MySqlDbType.VarChar)).Value = emp.ActGroupID;
                cmd.Parameters.Add(new MySqlParameter("@BPFName", MySqlDbType.VarChar)).Value = emp.BPFName;
                cmd.Parameters.Add(new MySqlParameter("@BPMName", MySqlDbType.VarChar)).Value = emp.BPMName;
                cmd.Parameters.Add(new MySqlParameter("@BPLName", MySqlDbType.VarChar)).Value = emp.BPLName;
                cmd.Parameters.Add(new MySqlParameter("@ShortKey", MySqlDbType.VarChar)).Value = emp.ShortKey;
                cmd.Parameters.Add(new MySqlParameter("@GSTNo", MySqlDbType.VarChar)).Value = emp.GSTNo;
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
                //****************************update t_bp_segment************************************** 
                sql = "delete from t_bp_segment where CompCode = '" + emp.CompCode + "' and BPCode = '" + id + "'";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.ExecuteNonQuery();
                foreach (DealerProductGroup pg in emp.ProductGroup)
                {
                    sql = "Insert into t_bp_segment (CompCode, BPCode, ProductGroupID) values (@CompCode, @BPCode, @ProductGroupID)";
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = emp.CompCode;
                    cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = emp.BPCode;
                    cmd.Parameters.Add(new MySqlParameter("@ProductGroupID", MySqlDbType.VarChar)).Value = pg.ProductGroupID;
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.ExecuteNonQuery();
                }
                //****************************update t_bp_relation************************************** 
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

        // DELETE api/<DealerController>/5
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
