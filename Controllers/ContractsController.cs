using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondoHub.Data;
using CondoHub.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CondoHub.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Index(string searchString, ContractType? type, string tab = "contracts")
        {
            var query = _context.Contracts.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c =>
                    c.Nome.Contains(searchString) ||
                    (c.Property != null && (
                        (!string.IsNullOrEmpty(c.Property.Bloco) && c.Property.Bloco.Contains(searchString)) ||
                        (!string.IsNullOrEmpty(c.Property.Numero) && c.Property.Numero.Contains(searchString))
                    ))
                );
            }

            if (type.HasValue)
                query = query.Where(c => c.Type == type);

            var contracts = await query
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            var propertyIds = contracts.Where(c => c.PropertyId.HasValue).Select(c => c.PropertyId!.Value).Distinct().ToList();
            var propertyMap = new Dictionary<int, string>();
            if (propertyIds.Any())
            {
                propertyMap = await _context.Properties
                    .Where(p => propertyIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => (string.IsNullOrEmpty(p.Bloco) ? p.Numero : (p.Bloco + " - " + p.Numero)));
            }

            ViewBag.ContractPropertyMap = propertyMap;
            ViewBag.ActiveTab = tab;
            return View(contracts);
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public IActionResult CreateLocatario()
        {
            PopulatePropertiesSelectList().GetAwaiter().GetResult();
            return View();
        }

        private async Task PopulatePropertiesSelectList()
        {
            var props = await _context.Properties
                .OrderByDescending(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    Display = (p.Bloco == null || p.Bloco == "") ? p.Numero : (p.Bloco + " - " + p.Numero)
                })
                .ToListAsync();
            ViewBag.PropertyList = new SelectList(props, "Id", "Display");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> CreateLocatario(Contract model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Type = ContractType.LocatarioProprietario;
            _context.Contracts.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tab = "contracts" });
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public IActionResult CreateCondomino()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> CreateCondomino(Contract model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Type = ContractType.Condomino;
            _context.Contracts.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tab = "residents" });
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public IActionResult CreateFuncionario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> CreateFuncionario(Contract model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Type = ContractType.Funcionario;
            _context.Contracts.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tab = "employees" });
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();
            await PopulatePropertiesSelectList();
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Edit(int id, Contract model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var tab = model.Type switch
            {
                ContractType.LocatarioProprietario => "contracts",
                ContractType.Condomino => "residents",
                ContractType.Funcionario => "employees",
                _ => "contracts"
            };

            return RedirectToAction(nameof(Index), new { tab });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            var tabAfterDelete = contract.Type switch
            {
                ContractType.LocatarioProprietario => "contracts",
                ContractType.Condomino => "residents",
                ContractType.Funcionario => "employees",
                _ => "contracts"
            };

            return RedirectToAction(nameof(Index), new { tab = tabAfterDelete });
        }
    }
}