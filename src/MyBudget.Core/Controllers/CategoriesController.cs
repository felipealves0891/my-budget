using Microsoft.AspNetCore.Mvc;
using MyBudget.Core.Dtos.Categories;
using MyBudget.Core.Models;
using MyBudget.Core.Models.Repositories;
using System.Security.Claims;

namespace MyBudget.Core.Controllers
{
    public class CategoriesController : ControllerBase
    {
        private ICategoriesRepository _repository;

        public CategoriesController(ICategoriesRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ActionResult Post([FromBody] CreatingCategoryDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            if (!Enum.TryParse<Flow>(dto.Flow, out Flow flow))
                return BadRequest("O Fluxo é invalido!");

            var exists = _repository.Get(dto.Name, ownerId);
            if (exists != null)
                return BadRequest("Você já possui um grupo com esse nome!");

            var group = _repository.GetGroupById(dto.GroupId, ownerId);
            if (group is null)
                return NotFound();

            if (group.Flow != flow)
                return BadRequest("O Fluxo esta no sentido invalido!");

            var category = new Category
            {
                Name = dto.Name,
                Abbr = dto.Abbr,
                Flow = flow,
                OwnerId = ownerId,
                GroupId = dto.GroupId,
                Group = group,
            };

            _repository.Add(category);
            return Created("", category);
        }

        [HttpPut]
        public ActionResult Put([FromBody] ChangeCategoryDto dto)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var category = _repository.Get(dto.Id, ownerId);
            if (category is null)
                return NotFound();

            var exists = _repository.Get(dto.Name, ownerId);
            if (exists != null && exists.Id != category.Id)
                return BadRequest("Você já possui um grupo com esse nome!");

            category.Name = dto.Name;
            category.Abbr = dto.Abbr;

            _repository.Update(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var ownerId = ((ClaimsIdentity)User.Identity).FindFirst("Id").Value;

            var category = _repository.Get(id, ownerId);
            if (category is null)
                return NotFound();

            if (category.Cashes != null && category.Cashes.Count > 0)
                return BadRequest("Esta categoria não pode ser removida, pois possui dependentes");

            _repository.Delete(id);
            return Ok(category);
        }
    }
}
