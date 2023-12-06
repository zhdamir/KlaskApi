using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;
using KlaskApi.Services;

namespace KlaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RundeController : ControllerBase
    {
        private readonly TurnierContext _context;

        public RundeController(TurnierContext context)
        {
            _context = context;
        }

        /*staring the Vorrunde*/
        [HttpPost("startVorrunde")]
        public async Task<IActionResult> StartVorrunde([FromQuery] long turnierId)
        {
            if (turnierId <= 0)
            {
                // Log or return a more specific error response for an invalid turnierId
                return BadRequest("Invalid turnierId. TurnierId must be greater than 0.");
            }

            try
            {
                var groupingService = new TeilnehmerGroupingService(_context);
                var createdGroups = await groupingService.GroupTeilnehmer(turnierId);

                if (createdGroups == null)
                {
                    // Handle the case where grouping failed
                    return BadRequest("Grouping failed. Check your data.");
                }

                // Return success response or createdGroups if needed
                return Ok(createdGroups);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error in StartTurnier: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        // GET: api/Runde
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Runde>>> GetRunden()
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            return await _context.Runden.ToListAsync();
        }

        // GET: api/Runde/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Runde>> GetRunde(long id)
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            var runde = await _context.Runden.FindAsync(id);

            if (runde == null)
            {
                return NotFound();
            }

            return runde;
        }

        // PUT: api/Runde/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRunde(long id, Runde runde)
        {
            if (id != runde.RundeId)
            {
                return BadRequest();
            }

            _context.Entry(runde).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RundeExists(id))
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

        // POST: api/Runde
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Runde>> PostRunde(Runde runde)
        {
            if (_context.Runden == null)
            {
                return Problem("Entity set 'TurnierContext.Runden'  is null.");
            }
            _context.Runden.Add(runde);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRunde", new { id = runde.RundeId }, runde);
        }

        // DELETE: api/Runde/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRunde(long id)
        {
            if (_context.Runden == null)
            {
                return NotFound();
            }
            var runde = await _context.Runden.FindAsync(id);
            if (runde == null)
            {
                return NotFound();
            }

            _context.Runden.Remove(runde);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RundeExists(long id)
        {
            return (_context.Runden?.Any(e => e.RundeId == id)).GetValueOrDefault();
        }
    }
}
