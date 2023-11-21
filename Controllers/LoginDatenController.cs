using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;

namespace KlaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginDatenController : ControllerBase
    {
        private readonly TurnierContext _context;

        public LoginDatenController(TurnierContext context)
        {
            _context = context;
        }

        // GET: api/LoginDaten
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoginDaten>>> GetLoginDaten()
        {
          if (_context.LoginDaten == null)
          {
              return NotFound();
          }
            return await _context.LoginDaten.ToListAsync();
        }

        // GET: api/LoginDaten/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoginDaten>> GetLoginDaten(long id)
        {
          if (_context.LoginDaten == null)
          {
              return NotFound();
          }
            var loginDaten = await _context.LoginDaten.FindAsync(id);

            if (loginDaten == null)
            {
                return NotFound();
            }

            return loginDaten;
        }

        // PUT: api/LoginDaten/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoginDaten(long id, LoginDaten loginDaten)
        {
            if (id != loginDaten.LoginId)
            {
                return BadRequest();
            }

            _context.Entry(loginDaten).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginDatenExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LoginDaten
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LoginDaten>> PostLoginDaten(LoginDaten loginDaten)
        {
          if (_context.LoginDaten == null)
          {
              return Problem("Entity set 'TurnierContext.LoginDaten'  is null.");
          }
            _context.LoginDaten.Add(loginDaten);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoginDaten", new { id = loginDaten.LoginId }, loginDaten);
        }

        // DELETE: api/LoginDaten/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoginDaten(long id)
        {
            if (_context.LoginDaten == null)
            {
                return NotFound();
            }
            var loginDaten = await _context.LoginDaten.FindAsync(id);
            if (loginDaten == null)
            {
                return NotFound();
            }

            _context.LoginDaten.Remove(loginDaten);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoginDatenExists(long id)
        {
            return (_context.LoginDaten?.Any(e => e.LoginId == id)).GetValueOrDefault();
        }
    }
}
