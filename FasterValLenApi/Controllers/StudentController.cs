using FasterValLenApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FasterValLenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly StudentStoreClient _client;

        public StudentController(StudentStoreClient client)
        {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var student = await _client.GetStudentByIdAsync(id);
            if (student is not null)
                return Ok(student);
            return NoContent();
        }

        [HttpPost]
        public ActionResult Post(Student student)
        {
            if (ModelState.IsValid)
            {
                _client.AddStudent(student);
                return Ok();
            }

            return BadRequest();
        }
    }
}
