using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BankFull.Models;
using Microsoft.AspNetCore.Authorization;

using BCryptNet = BCrypt.Net.BCrypt;

namespace BankFull.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly TransferOffContext _context;

        public UsersController(TransferOffContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var transferOffContext = _context.user.Include(u => u.Role);


            return transferOffContext != null ?
            View(await transferOffContext.ToListAsync()) :
            Problem("Entity set 'TransferOffContext.tblMessages'  is null.");
        }


        
        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.user == null)
            {
                return NotFound();
            }

            var user = await _context.user
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Role1");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Email,Phone,Status,RoleId,Password")] User user)
        {
            if (ModelState.IsValid)
            {

               // User usr = new User();

                if(_context.user.Where(x=>x.Email == user.Email).Count() != 1) {


                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    user.Password = passwordHash;


                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["error"] = "Username already exist";
                    return RedirectToAction(nameof(Index));
                }


            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.user == null)
            {
                return NotFound();
            }

            var user = await _context.user.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Email,Phone,Status,RoleId,Password")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.user == null)
            {
                return NotFound();
            }

            var user = await _context.user
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.user == null)
            {
                return Problem("Entity set 'TransferOffContext.user'  is null.");
            }
            var user = await _context.user.FindAsync(id);
            if (user != null)
            {
                _context.user.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        public async Task<IActionResult> AssignAsync( )
        {
            
            


            string email = User.Identity.Name;
            int uid = _context.user.Where(x => x.Email == email).FirstOrDefault().Id;

            List<UserMessage> Assign = await this._context.UserMessages.Include(x => x.User).Include(x => x.tblMessage).Include(x => x.tblMessage.BankDetail).Include(x => x.tblMessage.BankDetail).Where(x => x.tblMessage.Transactions.Where(x => x.DrAmount == null).Count() >= 1).Where(x => x.UserId == uid).ToListAsync();
            List<UserMessage> AssignComplete = await this._context.UserMessages.Include(x => x.User).Include(x => x.tblMessage).Include(x => x.tblMessage.BankDetail).Include(x => x.tblMessage.BankDetail.User).Where(x => x.tblMessage.Transactions.Where(x => x.DrAmount == null).Count() == 0).Where(x => x.UserId == uid).ToListAsync();


            dynamic model = new System.Dynamic.ExpandoObject();

            model.Assign = Assign ;
            model.AssignComplete = AssignComplete ;






            return _context.tblMessages != null ?
            View( model) :
            Problem("Entity set 'TransferOffContext.tblMessages'  is null.");


        }

        public IActionResult Setting()
        {
            return View();
        }


        private bool UserExists(int id)
        {
          return (_context.user?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
