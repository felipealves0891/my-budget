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

        private readonly ILogger<GroupsController> _logger;

        public GroupsController(
            ILogger<GroupsController> logger,
            ApplicationDbContext context
        ) {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Group>> Get()
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
            var group = await _context.Groups
                                 .Where(x => x.OwnerId == ownerId && x.Id == id)
                                 .FirstOrDefaultAsync();

            if (group is null)
                return NotFound();

            return Ok(group);
        }

        // POST api/<GroupsController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreatingGroupDto value)
        {
            string ownerId = GetOwnerId();
            bool canNameBeUsed = await CanNameBeUsed(value.Name, ownerId);

            if (!canNameBeUsed)
                ModelState.AddModelError("Nome invalido!", $"O nome {value.Name} já esta em uso!");

            Sense sense;
            if(Enum.TryParse<Sense>(value.Sense, out sense))
                ModelState.AddModelError("Tipo invalido!", $"O tipo do grupo é invalido {sense}!");

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            var group = new Group()
            {
                Name = value.Name,
                Abbr = value.Abbr,
                OwnerId = ownerId,
                Sense = sense
            };

            try
            {
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
                return Created($"api/group/{group.Id}", group);
            }
            catch (Exception)
            {
                _logger.LogError("Erro ao criar o group {0}, do owner {1}", value.Name, ownerId);
                return Problem(
                     "Desculpe, não foi possivel finalizar a operação"
                    ,"/api/group/"
                    ,StatusCodes.Status500InternalServerError
                    ,"Erro ao finalizar a operação");
            }
            
            
            
        }

        // PUT api/<GroupsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] ChangeGroupDto value)
        {
            if(value.Id != id)
            {
                ModelState.AddModelError("Dados invalido!", $"Não foi possivel executar a operação!");
                return BadRequest(ModelState);
            }

            var group = await _context.Groups.FindAsync(id);
            if (group is null)
            {
                ModelState.AddModelError("Dados invalido!", $"Grupo não localizado!");
                return BadRequest(ModelState);
            }

            string ownerId = GetOwnerId();
            bool canNameBeUsed = await CanNameBeUsed(value.Name, ownerId);
            if(group.Name != value.Name && !canNameBeUsed)
            {
                ModelState.AddModelError("Nome invalido!", $"O nome {value.Name} já esta em uso!");
                return BadRequest(ModelState);
            }

            try
            {
                group.Name = value.Name;
                group.Abbr = value.Abbr;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                _logger.LogError("Erro ao alterar o group {0}, do owner {1}", value.Name, ownerId);
                return Problem(
                     "Desculpe, não foi possivel finalizar a operação"
                    , "/api/group/"
                    , StatusCodes.Status500InternalServerError
                    , "Erro ao finalizar a operação");
            }
        }

        // DELETE api/<GroupsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var group = await _context.Groups.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
            if (group is null)
            {
                ModelState.AddModelError("Dados invalido!", $"Grupo não localizado!");
                return BadRequest(ModelState);
            }

            if(group.Categories.Count > 0)
            {
                ModelState.AddModelError("Dados invalido!", $"Grupo contem dados vinculados, por favor, mova ou apague as categorias vinculadas!");
                return BadRequest(ModelState);
            }

            try
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                _logger.LogError("Erro ao remover o group {0}, do owner {1}", group.Name, group.OwnerId);
                return Problem(
                     "Desculpe, não foi possivel finalizar a operação"
                    , "/api/group/"
                    , StatusCodes.Status500InternalServerError
                    , "Erro ao finalizar a operação");
            }

        }

        private string GetOwnerId()
        {
            return ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;
        }

        private async Task<bool> CanNameBeUsed(string name, string ownerId)
        {
            var group = await _context.Groups
                                 .Where(x => x.OwnerId == ownerId && x.Name == name)
                                 .FirstOrDefaultAsync();

            return group == null;
        }
    }
}
