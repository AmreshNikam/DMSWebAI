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
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public OrderController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<OrderController>
        [HttpGet]
        public IActionResult GetOrder([FromQuery] DateTime startdate, DateTime enddate)
        {
            List<OrdeHeader> orders = new List<OrdeHeader>();
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
                    condition = " and OH.BPCode in ( " + condition + ")";
                }
                else if (ActGroupID == "SP")//if it is SP type
                {
                    condition = " and OH.BPCode in ('" + BPCode + "')";
                }
                else
                    return StatusCode(700, "You don't have access");
            }
            else
                condition = "";
            try
            {
                connection.Open();
                sql = "select OH.Order_Id, OH.CompCode, CRN_No, BPCode, Shift_TO_Party, Total_Value, DeliveryBlockStatus, GSTNumber, OrderCreatedBy, CreatedBy, CreatedOn, ChangedBy, ChangedOn " +
                             "Line_Item, Product_Id, Product_Old_Id, Quantity, UOM, DeliveryDate, PricingGroupID, Rate, Currency, Discount, HSNCode, GSTPercentage, Status " +
                             "from t_order_header as OH " +
                             "left join t_order_line_item as OI on OH.Order_Id = OI.Order_Id and OH.CompCode = OI.CompCode " +
                             "where OH.CreatedOn between '" + startdate.ToString("yyyy-MM-dd") + "' and '" + enddate.ToString("yyyy-MM-dd") + "' and OH.CompCode = '" + CompanyID + "' " + condition;
                string PrevID = "X";
                OrdeHeader OH = null;
                OrderLineItems OI = null;
                while (rdr.Read())
                {
                    string tid = rdr["Order_Id"].ToString();
                    if (PrevID != tid)
                    {
                        if (OH != null)
                            orders.Add(OH);
                        OH = new OrdeHeader();
                        OH.Order_Id = tid;
                        OH.CompCode = rdr["CompCode"].ToString();
                        OH.CompCode = rdr["CompCode"].ToString();
                        OH.CRN_No = rdr["CRN_No"].ToString();
                        OH.BPCode = rdr["BPCode"].ToString();
                        OH.Shift_TO_Party = rdr["Shift_TO_Party"].ToString();
                        OH.Total_Value = Convert.ToDecimal(rdr["Total_Value"]);
                        OH.DeliveryBlockStatus = rdr["DeliveryBlockStatus"].ToString();
                        OH.GSTNumber = rdr["GSTNumber"].ToString();
                        OH.OrderCreatedBy = rdr["OrderCreatedBy"].ToString();
                        OH.CreatedOn = Convert.ToDateTime(rdr["CreatedOn"]);
                        OH.ChangedBy = rdr["ChangedBy"].ToString();
                        OH.ChangedOn = Convert.ToDateTime(rdr["ChangedOn"]);
                        OH.SoldTo = rdr["SoldTo"].ToString();
                        OH.SoldToAddress = rdr["SoldToAddress"].ToString();
                        OH.ShipTo = rdr["ShipTo"].ToString();
                        OH.ShipToAddress = rdr["ShipToAddress"].ToString();
                        OH.OrderStatus = Convert.ToInt16(rdr["OrderStatus"]);
                        //****************************************************
                        OH.Items = new List<OrderLineItems>();
                        OI = new OrderLineItems();
                        OI.Order_Id = rdr["LineItemOrderID"].ToString();
                        OI.Line_Item = Convert.ToInt32(rdr["Line_Item"]);
                        OI.Product_Id = rdr["Product_Id"].ToString();
                        OI.Product_Old_Id = rdr["Product_Old_Id"].ToString();
                        OI.Quantity = Convert.ToDecimal(rdr["Quantity"]);
                        OI.UOM = rdr["UOM"].ToString();
                        OI.DeliveryDate = Convert.ToDateTime(rdr["DeliveryDate"]);
                        OI.PricingGroupID = rdr["PricingGroupID"].ToString();
                        OI.Rate = Convert.ToDecimal(rdr["Rate"]);
                        OI.Currency = rdr["Currency"].ToString();
                        OI.Discount = Convert.ToDecimal(rdr["Discount"]);
                        OI.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
                        OI.GSTPercentage = Convert.ToDecimal(rdr["GSTPercentage"]);
                        OI.Status = rdr["Status"].ToString();
                        OH.Items.Add(OI);
                        PrevID = tid;
                    }
                    else
                    {
                        OI = new OrderLineItems();
                        OI.Order_Id = rdr["LineItemOrderID"].ToString();
                        OI.Line_Item = Convert.ToInt32(rdr["Line_Item"]);
                        OI.Product_Id = rdr["Product_Id"].ToString();
                        OI.Product_Old_Id = rdr["Product_Old_Id"].ToString();
                        OI.Quantity = Convert.ToDecimal(rdr["Quantity"]);
                        OI.UOM = rdr["UOM"].ToString();
                        OI.DeliveryDate = Convert.ToDateTime(rdr["DeliveryDate"]);
                        OI.PricingGroupID = rdr["PricingGroupID"].ToString();
                        OI.Rate = Convert.ToDecimal(rdr["Rate"]);
                        OI.Currency = rdr["Currency"].ToString();
                        OI.Discount = Convert.ToDecimal(rdr["Discount"]);
                        OI.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
                        OI.GSTPercentage = Convert.ToDecimal(rdr["GSTPercentage"]);
                        OI.Status = rdr["Status"].ToString();
                        OH.Items.Add(OI);
                    }
                }
                if (OH != null)
                    orders.Add(OH);
                return Ok(orders);
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
        [HttpGet("Open")]
        public IActionResult GetOpenOrder()
        {
            List<OrdeHeader> orders = new List<OrdeHeader>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            string sql = string.Empty;
            string condition = string.Empty;
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
                    condition = " and OH.BPCode in ( " + condition + ")";
                }
                else if (ActGroupID == "SP")//if it is SP type
                {
                    condition = " and OH.BPCode in ('" + BPCode + "')";
                }
                else
                    return StatusCode(700, "You don't have access");
            }
            else
                condition = "";
            try
            {
                connection.Open();
                //sql = "select OH.Order_Id, OH.CompCode, CRN_No, OH.BPCode, Shift_TO_Party, Total_Value, DeliveryBlockStatus, " +
                //             "GSTNumber, OrderCreatedBy, CreatedBy, CreatedOn, ChangedBy, ChangedOn, OrderStatus, OI.Order_Id as LineItemOrderID, Line_Item, Product_Id, Product_Old_Id, " +
                //             "Quantity, UOM, DeliveryDate, PricingGroupID, Rate, Currency, Discount, HSNCode, GSTPercentage, OI.Status, " +
                //             "concat(if( BP1.BPFName is null, '', BP1.BPFName), if( BP1.BPMName is null, '', BP1.BPMName), if( BP1.BPLName is null, '', BP1.BPLName)) as SoldTo, " +
                //             "concat(if(BP1.Street is null, '', BP1.Street), ' ', if(BP1.AddressLine is null, '', BP1.AddressLine), ' ', if(BP1.PostalCode is null, '', BP1.PostalCode), ' PostalCode: ', if(BP1.PostalCode is null, '', BP1.PostalCode), ' City: ', if(BP1.City is null, '', BP1.City), ' State: ', if(ST1.StateDesc is null, '', ST1.StateDesc), ' Country: ', if(CN1.CountryDesc is null, '', CN1.CountryDesc)) as SoldToAddress, " +
                //             "concat(if( BP2.BPFName is null, '', BP2.BPFName), if( BP2.BPMName is null, '', BP2.BPMName), if( BP2.BPLName is null, '', BP2.BPLName)) as ShipTo, " +
                //             "concat(if(BP2.Street is null, '', BP2.Street), ' ', if(BP2.AddressLine is null, '', BP2.AddressLine), ' ', if(BP2.PostalCode is null, '', BP2.PostalCode), ' PostalCode: ', if(BP2.PostalCode is null, '', BP2.PostalCode), ' City: ', if(BP2.City is null, '', BP2.City), ' State: ', if(ST2.StateDesc is null, '', ST2.StateDesc), ' Country: ', if(CN2.CountryDesc is null, '', CN2.CountryDesc)) as ShipToAddress " +
                //             "from t_order_header as OH " +
                //             "left join t_order_line_item as OI on OH.Order_Id = OI.Order_Id and OH.CompCode = OI.CompCode " +
                //             "left join  t_business_partner BP1 on OH.BPCode = BP1.BPCode and OH.CompCode = BP1.CompCode " +
                //             "left join  t_business_partner BP2 on OH.Shift_TO_Party = BP2.BPCode and OH.CompCode = BP2.CompCode " +
                //             "left join c_country as CN1 on BP1.CountryID = CN1.CountryID " +
                //             "left join c_country as CN2 on BP2.CountryID = CN2.CountryID " +
                //             "left join c_state as ST1 on BP1.StateID = ST1.StateID and ST1.CountryID = BP1.CountryID " +
                //             "left join c_state as ST2 on BP2.StateID = ST2.StateID and ST2.CountryID = BP2.CountryID " +
                //             "where OH.OrderStatus = 1 and OH.CompCode = '" + CompanyID + "' " + condition + " order by OH.Order_Id";



                //Changed By Sohel

                sql = "select OH.Order_Id, OH.CompCode, CRN_No, OH.BPCode, Shift_TO_Party, Total_Value, DeliveryBlockStatus, " +
               " GSTNumber,OrderCreatedBy, OH.CreatedBy, OH.CreatedOn, OH.ChangedBy, OH.ChangedOn, OrderStatus, OI.Order_Id as LineItemOrderID, Line_Item, Product_Id, ProductDesc, Product_Old_Id, " +
               " Quantity, OI.UOM, DeliveryDate, OI.PricingGroupID,Rate, Currency, Discount, OI.HSNCode, GSTPercentage, OI.Status, " +
              
                "BP1.Street as SoldToStreet, BP1.AddressLine as SoldToAddressLine, BP1.PostalCode as SoldToPostalCode, BP1.City as SoldToCity, ST1.StateDesc as SoldToStateDesc, CN1.CountryDesc as SoldToCountryDesc , " +
                 "BP2.Street as ShipToStreet, BP2.AddressLine as ShipToAddressLine, BP2.PostalCode as ShipToPostalCode , BP2.City as ShipToCity, ST2.StateDesc as ShipToStateDesc, CN2.CountryDesc as ShipToCountryDesc, " +
    
               "concat(if (BP1.BPFName is null, '', BP1.BPFName), if (BP1.BPMName is null, '', BP1.BPMName), if (BP1.BPLName is null, '', BP1.BPLName)) as SoldTo, " +
                 "concat(if (BP1.Street is null, '', BP1.Street), ' ',  if (BP1.AddressLine is null, '', BP1.AddressLine), ' ', if (BP1.PostalCode is null, '', BP1.PostalCode), ' PostalCode: ',  if (BP1.PostalCode is null, '', BP1.PostalCode), ' City: ', if (BP1.City is null, '', BP1.City), ' State: ', if (ST1.StateDesc is null, '', ST1.StateDesc), ' Country: ', if (CN1.CountryDesc is null, '', CN1.CountryDesc)) as   SoldToAddress, " +
                 "concat(if (BP2.BPFName is null, '', BP2.BPFName), if (BP2.BPMName is null, '', BP2.BPMName),  if (BP2.BPLName is null, '', BP2.BPLName)) as ShipTo, " +
                 "concat(if (BP2.Street is null, '', BP2.Street), ' ', if (BP2.AddressLine is null, '', BP2.AddressLine), ' ', if (BP2.PostalCode is null, '', BP2.PostalCode), ' PostalCode: ', if (BP2.PostalCode is null, '', BP2.PostalCode), ' City: ', if (BP2.City is null, '', BP2.City), ' State: ',  if (ST2.StateDesc is null, '', ST2.StateDesc), ' Country: ', if (CN2.CountryDesc is null, '', CN2.CountryDesc)) as   ShipToAddress from t_order_header as OH " +
                "left join t_order_line_item as OI on OH.Order_Id = OI.Order_Id and OH.CompCode = OI.CompCode left join t_business_partner BP1 on OH.BPCode = BP1.BPCode and OH.CompCode = BP1.CompCode " +
               "left join t_business_partner BP2 on OH.Shift_TO_Party = BP2.BPCode and OH.CompCode = BP2.CompCode " +
                "left join c_country as CN1 on BP1.CountryID = CN1.CountryID left join c_country as CN2 on BP2.CountryID = CN2.CountryID " +
                " left join c_state as ST1 on BP1.StateID = ST1.StateID and ST1.CountryID = BP1.CountryID " +
                "left join c_state as ST2 on BP2.StateID = ST2.StateID and ST2.CountryID = BP2.CountryID " +
                "left join m_product_master as PM on OI.Product_Id = PM.ProductID and PM.CompCode = OI.CompCode " +
                "left join c_material_pricing_group as MPG on MPG.PricingGroupID = OI.PricingGroupID and MPG.CompCode = OI.CompCode " +
                "where OH.OrderStatus = 1 and OH.CompCode = '" + CompanyID + "' " + condition + " order by OH.Order_Id ";
                
                //Console.WriteLine(sql);


                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                rdr = cmd.ExecuteReader();
                string PrevID = "X";
                OrdeHeader OH = null;
                OrderLineItems OI = null;
                //added by sohel
                ProductMaster PM = null;
                while (rdr.Read())
                {
                    string tid = rdr["Order_Id"].ToString();
                    if (PrevID != tid)
                    {
                        if (OH != null)
                            orders.Add(OH);
                        OH = new OrdeHeader();
                        OH.Order_Id = tid;
                        OH.CompCode = rdr["CompCode"].ToString();
                        OH.CompCode = rdr["CompCode"].ToString();
                        OH.CRN_No = rdr["CRN_No"].ToString();
                        OH.BPCode = rdr["BPCode"].ToString();
                        OH.Shift_TO_Party = rdr["Shift_TO_Party"].ToString();
                        OH.Total_Value = Convert.ToDecimal(rdr["Total_Value"]);
                        OH.DeliveryBlockStatus = rdr["DeliveryBlockStatus"].ToString();
                        OH.GSTNumber = rdr["GSTNumber"].ToString();
                        OH.OrderCreatedBy = rdr["OrderCreatedBy"].ToString();
                        OH.CreatedOn = Convert.ToDateTime(rdr["CreatedOn"]);
                        OH.ChangedBy = rdr["ChangedBy"].ToString();
                        OH.ChangedOn = Convert.ToDateTime(rdr["ChangedOn"]);
                        OH.SoldTo = rdr["SoldTo"].ToString();
                        OH.SoldToAddress = rdr["SoldToAddress"].ToString();
                        OH.ShipTo = rdr["ShipTo"].ToString();
                        OH.ShipToAddress = rdr["ShipToAddress"].ToString();
                        //Added by sohel
                        OH.SoldToStreet = rdr["SoldToStreet"].ToString();
                        OH.SoldToAddressLine = rdr["SoldToAddressLine"].ToString();
                        OH.SoldToPostalCode = rdr["SoldToPostalCode"].ToString();
                        OH.SoldToCity = rdr["SoldToCity"].ToString();
                        OH.SoldToStateDesc = rdr["SoldToStateDesc"].ToString();
                        OH.SoldToCountryDesc = rdr["SoldToCountryDesc"].ToString();

                        OH.ShipToStreet = rdr["ShipToStreet"].ToString();
                        OH.ShipToAddressLine = rdr["ShipToAddressLine"].ToString();
                        OH.ShipToPostalCode = rdr["ShipToPostalCode"].ToString();
                        OH.ShipToCity = rdr["ShipToCity"].ToString();
                        OH.ShipToStateDesc = rdr["ShipToStateDesc"].ToString();
                        OH.ShipToCountryDesc = rdr["ShipToCountryDesc"].ToString();
                        



                        OH.OrderStatus = Convert.ToInt16(rdr["OrderStatus"]);
                        //****************************************************
                        OH.Items = new List<OrderLineItems>();
                        OI = new OrderLineItems();
                        OI.Order_Id = rdr["LineItemOrderID"].ToString();
                        OI.Line_Item = Convert.ToInt32(rdr["Line_Item"]);
                        OI.Product_Id = rdr["Product_Id"].ToString();
                        //added my sohel
                        OI.ProductDesc = rdr["ProductDesc"].ToString();
                        OI.Product_Old_Id = rdr["Product_Old_Id"].ToString();
                        OI.Quantity = Convert.ToDecimal(rdr["Quantity"]);
                        OI.UOM = rdr["UOM"].ToString();
                        OI.DeliveryDate = Convert.ToDateTime(rdr["DeliveryDate"]);
                        OI.PricingGroupID = rdr["PricingGroupID"].ToString();
                        OI.Rate = Convert.ToDecimal(rdr["Rate"]);
                        OI.Currency = rdr["Currency"].ToString();
                        OI.Discount = Convert.ToDecimal(rdr["Discount"]);
                        OI.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
                        OI.GSTPercentage = Convert.ToDecimal(rdr["GSTPercentage"]);
                        OI.Status = rdr["Status"].ToString();
                        OH.Items.Add(OI);

                        PrevID = tid;
                    }
                    else
                    {
                        OI = new OrderLineItems();
                        OI.Order_Id = rdr["LineItemOrderID"].ToString();
                        OI.Line_Item = Convert.ToInt32(rdr["Line_Item"]);
                        OI.Product_Id = rdr["Product_Id"].ToString();
                        //added my sohel
                        OI.ProductDesc = rdr["ProductDesc"].ToString();
                        OI.Product_Old_Id = rdr["Product_Old_Id"].ToString();
                        OI.Quantity = Convert.ToDecimal(rdr["Quantity"]);
                        OI.UOM = rdr["UOM"].ToString();
                        OI.DeliveryDate = Convert.ToDateTime(rdr["DeliveryDate"]);
                        OI.PricingGroupID = rdr["PricingGroupID"].ToString();
                        OI.Rate = Convert.ToDecimal(rdr["Rate"]);
                        OI.Currency = rdr["Currency"].ToString();
                        OI.Discount = Convert.ToDecimal(rdr["Discount"]);
                        OI.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
                        OI.GSTPercentage = Convert.ToDecimal(rdr["GSTPercentage"]);
                        OI.Status = rdr["Status"].ToString();
                        OH.Items.Add(OI);
                    }
                }
                if (OH != null)
                    orders.Add(OH);
                return Ok(orders);
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
        
        // POST api/<OrderController>
        [HttpPost]
        public IActionResult Post([FromBody] OrdeHeader order)
        {
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand cmd = null;
            MySqlTransaction mytrans = null;
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue("Token", out var token);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            string sql;
            try
            {
                connection.Open();
                mytrans = connection.BeginTransaction();

                cmd = new MySqlCommand("GetOrderNumber", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                };
                cmd.Parameters.Add("?CompanyID", MySqlDbType.VarChar).Value = CompanyID;
                cmd.Parameters.Add(new MySqlParameter("mRes", MySqlDbType.String, 0));
                cmd.Parameters["mRes"].Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();
                string orderID = cmd.Parameters["mRes"].Value.ToString();

                //****************************Order Header Table*****************************
                sql = "insert into t_order_header (Order_Id, CompCode, CRN_No, BPCode, Shift_TO_Party, Total_Value, DeliveryBlockStatus, GSTNumber, OrderCreatedBy, CreatedBy, CreatedOn, ChangedBy, ChangedOn) " +
                    "values (@Order_Id, @CompCode, @CRN_No, @BPCode, @Shift_TO_Party, @Total_Value, @DeliveryBlockStatus, @GSTNumber, @OrderCreatedBy, @CreatedBy, @CreatedOn, @ChangedBy, @ChangedOn)";
                cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                cmd.Parameters.Add(new MySqlParameter("@Order_Id", MySqlDbType.VarChar)).Value = orderID;
                cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = order.CompCode;
                cmd.Parameters.Add(new MySqlParameter("@CRN_No", MySqlDbType.VarChar)).Value = order.CRN_No;
                cmd.Parameters.Add(new MySqlParameter("@BPCode", MySqlDbType.VarChar)).Value = order.BPCode;
                cmd.Parameters.Add(new MySqlParameter("@Shift_TO_Party", MySqlDbType.VarChar)).Value = order.Shift_TO_Party;
                cmd.Parameters.Add(new MySqlParameter("@Total_Value", MySqlDbType.Decimal)).Value = order.Total_Value;
                cmd.Parameters.Add(new MySqlParameter("@DeliveryBlockStatus", MySqlDbType.VarChar)).Value = order.DeliveryBlockStatus;
                cmd.Parameters.Add(new MySqlParameter("@GSTNumber", MySqlDbType.VarChar)).Value = order.GSTNumber;
                cmd.Parameters.Add(new MySqlParameter("@OrderCreatedBy", MySqlDbType.VarChar)).Value = order.OrderCreatedBy;
                cmd.Parameters.Add(new MySqlParameter("@CreatedBy", MySqlDbType.VarChar)).Value = BPCode;
                //**********************************System********************************************************
                cmd.Parameters.Add(new MySqlParameter("@CreatedOn", MySqlDbType.DateTime)).Value = DateTime.Now;
                cmd.Parameters.Add(new MySqlParameter("@ChangedBy", MySqlDbType.VarChar)).Value = order.ChangedBy;
                cmd.Parameters.Add(new MySqlParameter("@ChangedOn", MySqlDbType.DateTime)).Value = order.ChangedOn;
                cmd.ExecuteNonQuery();
                //****************************Order Line Item Table*****************************
                foreach (OrderLineItems item in order.Items)
                {
                    sql = "insert into t_order_line_item (Order_Id, CompCode, Line_Item, Product_Id, Product_Old_Id, Quantity, UOM, DeliveryDate, PricingGroupID, Rate, Currency, Discount, HSNCode, GSTPercentage, Status) values " +
                          "(@Order_Id, @CompCode, @Line_Item, @Product_Id, @Product_Old_Id, @Quantity, @UOM, @DeliveryDate, @PricingGroupID, @Rate, @Currency, @Discount, @HSNCode, @GSTPercentage, @Status)";
                    cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                    cmd.Parameters.Add(new MySqlParameter("@Order_Id", MySqlDbType.VarChar)).Value = orderID;
                    cmd.Parameters.Add(new MySqlParameter("@CompCode", MySqlDbType.VarChar)).Value = item.CompCode;
                    cmd.Parameters.Add(new MySqlParameter("@Line_Item", MySqlDbType.Int32)).Value = item.Line_Item;
                    cmd.Parameters.Add(new MySqlParameter("@Product_Id", MySqlDbType.VarChar)).Value = item.Product_Id;
                    cmd.Parameters.Add(new MySqlParameter("@Product_Old_Id", MySqlDbType.VarChar)).Value = item.Product_Old_Id;
                    cmd.Parameters.Add(new MySqlParameter("@Quantity", MySqlDbType.Decimal)).Value = item.Quantity;
                    cmd.Parameters.Add(new MySqlParameter("@UOM", MySqlDbType.VarChar)).Value = item.UOM;
                    cmd.Parameters.Add(new MySqlParameter("@DeliveryDate", MySqlDbType.Date)).Value = item.DeliveryDate;
                    cmd.Parameters.Add(new MySqlParameter("@PricingGroupID", MySqlDbType.VarChar)).Value = item.PricingGroupID;
                    cmd.Parameters.Add(new MySqlParameter("@Rate", MySqlDbType.Decimal)).Value = item.Rate;
                    cmd.Parameters.Add(new MySqlParameter("@Currency", MySqlDbType.VarChar)).Value = item.Currency;
                    cmd.Parameters.Add(new MySqlParameter("@Discount", MySqlDbType.Decimal)).Value = item.Discount;
                    cmd.Parameters.Add(new MySqlParameter("@HSNCode", MySqlDbType.Decimal)).Value = item.HSNCode;
                    cmd.Parameters.Add(new MySqlParameter("@GSTPercentage", MySqlDbType.Decimal)).Value = item.GSTPercentage;
                    cmd.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.VarChar)).Value = item.Status;
                    cmd.ExecuteNonQuery();
                }
                mytrans.Commit();
                return Ok(orderID + " Order created");
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

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
