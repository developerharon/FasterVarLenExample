using FasterValLenApi.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FasterValLenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductStoreClient _client;

        public ProductController(ProductStoreClient client)
        {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var product = await _client.GetProductByIdAsync(id);
            if (product is not null)
                return Ok(product);
            return NoContent();
        }

        [HttpPost]
        public ActionResult Post(Product product)
        {
            if (ModelState.IsValid)
            {
                _client.AddProduct(product);
                return Ok();
            }

            return BadRequest();
        }
    }
}
