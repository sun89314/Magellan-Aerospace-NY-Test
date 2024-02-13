using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Text.Json.Serialization;
using System;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ItemsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // POST: items
        [HttpPost]
        public ActionResult<int> CreateItem([FromBody] ItemCreateDto itemDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@item_name, @parent_item, @cost, @req_date) RETURNING id", connection);
            command.Parameters.AddWithValue("@item_name", itemDto.ItemName);
            command.Parameters.AddWithValue("@parent_item", (object)itemDto.ParentItem ?? DBNull.Value);
            command.Parameters.AddWithValue("@cost", itemDto.Cost);
            command.Parameters.AddWithValue("@req_date", itemDto.ReqDate);
            
            var newId = (int)command.ExecuteScalar();
            return Ok(newId);
        }

        // GET: items/{id}
        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetItem(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("SELECT id, item_name, parent_item, cost, req_date FROM item WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            
            if (!reader.Read())
            {
                return NotFound();
            }

            var item = new ItemDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                ItemName = reader.GetString(reader.GetOrdinal("item_name")),
                ParentItem = reader.IsDBNull(reader.GetOrdinal("parent_item")) ? null : reader.GetInt32(reader.GetOrdinal("parent_item")),
                Cost = reader.GetInt32(reader.GetOrdinal("cost")),
                ReqDate = reader.GetDateTime(reader.GetOrdinal("req_date"))
            };
            return Ok(item);
        }

        // GET: items/GetTotalCost/{itemName}
        [HttpGet("GetTotalCost/{itemName}")]
        public ActionResult<int?> GetTotalCost(string itemName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("SELECT Get_Total_Cost(@item_name)", connection);
            command.Parameters.AddWithValue("@item_name", itemName);
            var result = command.ExecuteScalar();
            
            if (result == null || result == DBNull.Value)
            {
                return NotFound("Item not found or multiple items with the same name.");
            }
            return Ok((int)result);
        }
    }


    public class ItemCreateDto
    {
        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }

        [JsonPropertyName("parent_item")]
        public int? ParentItem { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("req_date")]
        public DateTime ReqDate { get; set; }
    }

    public class ItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }

        [JsonPropertyName("parent_item")]
        public int? ParentItem { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("req_date")]
        public DateTime ReqDate { get; set; }
    }
}
