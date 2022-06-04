using Microsoft.AspNetCore.Mvc;
using MyBudget.Core.Dtos.Groups;
using MyBudget.Core.Models;
using MyBudget.Core.Models.Repositories;
using System.Security.Claims;

namespace MyBudget.Core.Controllers
{

    public class GroupsController : ControllerBase
    {
        private readonly IGroupsRepository _repository;

        public GroupsController(IGroupsRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ActionResult Post([FromBody] CreatingGroupDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var exists = _repository.Get(dto.Name, ownerId);
            if (exists != null)
                return BadRequest("Você já possui um grupo com esse nome!");

            if (!Enum.TryParse<Flow>(dto.Flow, out var sense))
                return BadRequest("O fluxo escolhido é invalido!");

            var group = new Group
            {
                Name = dto.Name,
                Abbr = dto.Abbr,
                Flow = sense,
                OwnerId = ownerId
            };

            _repository.Add(group);
            return Created("", group);
        }

        [HttpPut]
        public ActionResult Put([FromBody] ChangeGroupDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var exists = _repository.Get(dto.Name, ownerId);
            if (exists != null && exists.Id != dto.Id)
                return BadRequest("Você já possui um grupo com esse nome!");

            var group = _repository.Get(dto.Id, ownerId);
            if (group == null)
                return NotFound();

            group.Name = dto.Name;
            group.Abbr = dto.Abbr;

            _repository.Update(group);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var group = _repository.Get(id, ownerId);
            if (group is null)
                return NotFound();

            var categories = group.Categories;
            if (categories != null && categories.Count > 0)
                return BadRequest("Este grupo não pode ser removido, pois possui dependentes");

            var deleted = _repository.Delete(id);
            return Ok(deleted);
        }

    }
}
