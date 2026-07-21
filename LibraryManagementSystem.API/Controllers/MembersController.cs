using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public MembersController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await _uow.Members.GetAllAsync();
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var member = await _uow.Members.GetByIdAsync(id);
            return member == null ? NotFound() : Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Member member)
        {
            await _uow.Members.AddAsync(member);
            await _uow.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Member updated)
        {
            var member = await _uow.Members.GetByIdAsync(id);
            if (member == null) return NotFound();

            member.FullName = updated.FullName;
            member.Email = updated.Email;

            _uow.Members.Update(member);
            await _uow.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _uow.Members.GetByIdAsync(id);
            if (member == null) return NotFound();

            member.isDeleted = true;
            _uow.Members.Update(member);
            await _uow.CompleteAsync();
            return NoContent();
        }
    }
}