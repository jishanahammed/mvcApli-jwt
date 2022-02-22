using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvcApli.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mvcApli.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [Route("GetCategories")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
           var result =await _context.Categories.ToListAsync();
            return result;
        }
        [Route("GetService")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetService()
        {
            return await _context.Services.ToListAsync();
        }


        // GET api/<TestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [Route("PostCategory")]
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromForm] Category category)
        {
            _context.Categories.Add(category);
          var res=  await _context.SaveChangesAsync();
            return Ok(new ResponseResult { Result = res, IsSuccess = true, Message = "Added successfully" });
        }

        [Route("PutCategory")]
        [HttpPut]
        public async Task<IActionResult> PutCategory([FromForm] Category category)
        {
            if (category.Id == 0)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }


        // DELETE: api/Category/5
        [Route("Delete/{id}")]
        [HttpDelete]
        public async Task<ActionResult<Category>> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
             var res= await _context.SaveChangesAsync();
            if (res==0)
            return Ok(new ResponseResult { Result = res, IsSuccess = true, Message = "Delete successfully" });
            return Ok(new ResponseResult { Result = res, IsSuccess = false, Message = "" });
        }

        [Route("GetById/{id}")]
        [HttpGet]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return Ok(new ResponseResult { Result = category, IsSuccess = true, Message = "Get successfully" });
        }

    }
}
