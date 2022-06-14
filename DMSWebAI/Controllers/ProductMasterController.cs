using DMSWebAI.AppClasses;
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
    public class ProductMasterController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public ProductMasterController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        // GET: api/<ProductMasterController>
        [HttpGet]
        public IActionResult Get()
        {
            List<ProductMaster> products = new List<ProductMaster>();
            string connString = this.Configuration.GetConnectionString("DMS");
            MySqlConnection connection = new MySqlConnection(connString);
            Request.Headers.TryGetValue("BPCode", out var BPCode);
            Request.Headers.TryGetValue ("Auth_Control", out var Auth_Control);
            Request.Headers.TryGetValue("CompanyID", out var CompanyID);
            int AuthControl = int.Parse(Auth_Control);
            string condition = string.Empty;
            if (AuthControl == 0)
                return StatusCode(700, "You don't have access");
            else if(AuthControl == 1)
            {
                if(string.IsNullOrEmpty(BPCode))
                    return StatusCode(700, "You account is not associate with business patner");
                condition = "where PM.Status = 1 and PM.ProductGroupID in (select ProductGroupID  from t_bp_segment where BPCode = '" + BPCode + "' and CompCode = '" + CompanyID + "') and PM.CompCode = '" + CompanyID + "'";
            }
            else
                condition = "Where PM.CompCode = '" + CompanyID + "'";

            try
            {
                connection.Open();
                string sql = "select PM.CompCode, PM. ProductID, PM. ProductDesc, PM. ProductOldID, PG.Description, PM. ProductLongDesc, PM. ProductGroupID, " +
                             "PM. UOM, PM. PricingGroupID, MP.Description as MaterialPricingDesc, PM. HSNCode, PM. PackingLotSize, PM. ProductImageUrl, PM. ProductStatus, " +
                             "PM. CreatedBy, PM. CreatedOn, PM. ChangedBy, PM. ChangedOn, PC1.Price as MRP, PC1.Currency as MRPCurrency, PC2.Price as ListPrice, PC2.Currency as ListPriceCurrency, PM.Status " +
                             "from m_product_master as PM " +
                             "left join c_product_group as PG on PM.ProductGroupID = PG.ProductGroupID and PM.CompCode = PG.CompCode " +
                             "left join t_price_condition as PC1 on PM.ProductID = PC1.ProductID and PM.CompCode = PC1.CompCode and PC1.PriceCondition = 'MRP' " +
                             "left join t_price_condition as PC2 on PM.ProductID = PC2.ProductID and PM.CompCode = PC2.CompCode and PC2.PriceCondition = 'ZLP1'" +
                             "left join c_material_pricing_group as MP on  PM.PricingGroupID = MP.PricingGroupID and PM.CompCode = MP.CompCode ";
                             
                sql += condition;
                MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    ProductMaster product = new ProductMaster();
                    product.CompCode = rdr["CompCode"].ToString();
                    product.ProductID = rdr["ProductID"].ToString();
                    product.ProductDesc = rdr["ProductDesc"].ToString();
                    product.ProductOldID = rdr["ProductOldID"].ToString();
                    product.ProductLongDesc = rdr["ProductLongDesc"].ToString();
                    product.ProductGroupID = rdr["ProductGroupID"].ToString();
                    product.UOM = rdr["UOM"].ToString();
                    product.PricingGroupID = rdr["PricingGroupID"].ToString();
                    product.Description = rdr["Description"].ToString();
                    product.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
                    product.PackingLotSize = rdr["PackingLotSize"].ToString();
                    product.ProductImageUrl = rdr["ProductImageUrl"].ToString();
                    product.ProductStatus = rdr["ProductStatus"].ToString();
                    product.CreatedBy = rdr["CreatedBy"].ToString();
                    product.CreatedOn = rdr["CreatedOn"].ToString();
                    product.ChangedBy = rdr["ChangedBy"].ToString();
                    product.ChangedOn = rdr["ChangedOn"].ToString();
                    product.MRP = Convert.ToDecimal(rdr["MRP"]);
                    product.MRPCurrency = rdr["MRPCurrency"].ToString();
                    product.MaterialPricingDesc = rdr["MaterialPricingDesc"].ToString();
                    product.ListPrice = Convert.ToDecimal(rdr["ListPrice"]);
                    product.ListPriceCurrency = rdr["ListPriceCurrency"].ToString();
                    product.Status = Convert.ToBoolean(rdr["Status"]);
                    products.Add(product);
                }
                return Ok(products);
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

        // GET api/<ProductMasterController>/5
        //[HttpGet("{id}")]
        //public IActionResult Get(string id)
        //{
        //    List<ProductMaster> products = new List<ProductMaster>();
        //    string connString = this.Configuration.GetConnectionString("DMS");
        //    MySqlConnection connection = new MySqlConnection(connString);
        //    Request.Headers.TryGetValue("BPCode", out var BPCode);
        //    Request.Headers.TryGetValue("Token", out var token);
        //    Request.Headers.TryGetValue("CompanyID", out var CompanyID);
        //    string condition = string.Empty;
        //    condition = "where PM.Status = 1 and PM.ProductGroupID in (select ProductGroupID  from t_bp_segment where BPCode = '" + id + "' and CompCode = '" + CompanyID + "') and PM.CompCode = '" + CompanyID + "'";
        //    try
        //    {
        //        connection.Open();
        //        string sql = "select PM.CompCode, PM. ProductID, PM. ProductDesc, PM. ProductOldID, PG.Description, PM. ProductLongDesc, PM. ProductGroupID, " +
        //                     "PM. UOM, PM. PricingGroupID, MP.Description as MaterialPricingDesc, PM. HSNCode, PM. PackingLotSize, PM. ProductImageUrl, PM. ProductStatus, M.Status " +
        //                     "PM. CreatedBy, PM. CreatedOn, PM. ChangedBy, PM. ChangedOn, PC1.Price as MRP, PC1.Currency as MRPCurrency, PC2.Price as ListPrice, PC2.Currency as ListPriceCurrency, HG.GSTPer, PC3.Price as Discount " +
        //                     "from m_product_master as PM " +
        //                     "left join c_product_group as PG on PM.ProductGroupID = PG.ProductGroupID and PM.CompCode = PG.CompCode " +
        //                     "left join t_price_condition as PC1 on PM.ProductID = PC1.ProductID and PM.CompCode = PC1.CompCode and PC1.PriceCondition = 'MRP' " +
        //                     "left join t_price_condition as PC2 on PM.ProductID = PC2.ProductID and PM.CompCode = PC2.CompCode and PC2.PriceCondition = 'ZLP1'" +
        //                     "left join c_material_pricing_group as MP on  PM.PricingGroupID = MP.PricingGroupID and PM.CompCode = MP.CompCode " +
        //                     "left join t_price_condition as PC3 on PM.PricingGroupID = PC3.PricingGroupID and PM.CompCode = PC3.CompCode and PC3.PriceCondition = 'TD01' " +
        //                     "left join t_hsn_gst as HG on PM.HSNCode = HG.HSNCode and PM.CompCode = HG.CompCode and Date(now()) between HG.FromDate and HG.ToDate ";
        //        sql = sql + condition;
        //        MySqlCommand cmd = new MySqlCommand(sql, connection) { CommandType = CommandType.Text };
        //        MySqlDataReader rdr = cmd.ExecuteReader();
        //        while (rdr.Read())
        //        {
        //            ProductMaster product = new ProductMaster();
        //            product.CompCode = rdr["CompCode"].ToString();
        //            product.ProductID = rdr["ProductID"].ToString();
        //            product.ProductDesc = rdr["ProductDesc"].ToString();
        //            product.ProductOldID = rdr["ProductOldID"].ToString();
        //            product.ProductLongDesc = rdr["ProductLongDesc"].ToString();
        //            product.ProductGroupID = rdr["ProductGroupID"].ToString();
        //            product.UOM = rdr["UOM"].ToString();
        //            product.PricingGroupID = rdr["PricingGroupID"].ToString();
        //            product.Description = rdr["Description"].ToString();
        //            product.HSNCode = Convert.ToDecimal(rdr["HSNCode"]);
        //            product.PackingLotSize = rdr["PackingLotSize"].ToString();
        //            product.ProductImageUrl = rdr["ProductImageUrl"].ToString();
        //            product.ProductStatus = rdr["ProductStatus"].ToString();
        //            product.CreatedBy = rdr["CreatedBy"].ToString();
        //            product.CreatedOn = rdr["CreatedOn"].ToString();
        //            product.ChangedBy = rdr["ChangedBy"].ToString();
        //            product.ChangedOn = rdr["ChangedOn"].ToString();
        //            product.MRP = Convert.ToDecimal(rdr["MRP"]);
        //            product.MRPCurrency = rdr["MRPCurrency"].ToString();
        //            product.MaterialPricingDesc = rdr["MaterialPricingDesc"].ToString();
        //            product.ListPrice = Convert.ToDecimal(rdr["ListPrice"]);
        //            product.ListPriceCurrency = rdr["ListPriceCurrency"].ToString();
        //            product.GSTPer = Convert.ToDecimal(rdr["GSTPer"]);
        //            product.Discount = Convert.ToDecimal(rdr["Discount"]);
        //            product.Status = Convert.ToBoolean(rdr["Status"]);
        //            products.Add(product);
        //        }
        //        return Ok(products);
        //    }
        //    catch (MySqlException ex)
        //    {
        //        return StatusCode(600, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    finally
        //    {
        //        connection.Close();
        //    }
        //}

        // POST api/<ProductMasterController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProductMasterController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProductMasterController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
