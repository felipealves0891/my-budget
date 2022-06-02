using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBudget.Core.Data;
using MyBudget.Core.Dtos.Groups;
using MyBudget.Core.Models;
using System.Security.Claims;

namespace MyBudget.Core.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> Get()
        {
            var ownerId = GetOwnerId();
            return await _context.Groups
                                 .Where(x => x.OwnerId == ownerId)
                                 .ToArrayAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> Get(int id)
        {
            var ownerId = GetOwnerId();
            return await _context.Groups
                                 .Where(x => x.OwnerId == ownerId && x.Id == id)
                                 .FirstOrDefaultAsync();
        }

        // POST api/<GroupsController>
        [HttpPost]
        public void Post([FromBody] CreatingGroupDto value)
        {
        }

        // PUT api/<GroupsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] ChangeGroupDto value)
        {
        }

        // DELETE api/<GroupsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private string GetOwnerId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;
        }
    }
}
