using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using CondoHub.Data;
using CondoHub.Models.Entities;

namespace CondoHub.Controllers
{
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO, MORADOR")]
        public async Task<IActionResult> Index(string searchString, string status)
        {
            var query = _context.Properties
                .Include(i => i.Proprietario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i =>
                    i.Numero.Contains(searchString) ||
                    (i.Proprietario != null && i.Proprietario.Nome != null && i.Proprietario.Nome.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                var statusEnum = Enum.Parse<PropertyType>(status);
                query = query.Where(i => i.Type == statusEnum);
            }

            var imoveis = await query
                .OrderByDescending(p => p.Id)
                .Select(p => new Property
                {
                    Id = p.Id,
                    Bloco = p.Bloco,
                    Numero = p.Numero,
                    Area = p.Area,
                    Type = p.Type,
                    ProprietarioId = p.ProprietarioId,
                    ProprietarioNome = p.ProprietarioNome,
                    TipoImovel = p.TipoImovel,
                    Proprietario = p.Proprietario
                })
                .ToListAsync();

            var propertyIds = imoveis.Select(i => i.Id).ToList();
            var ownersDict = new Dictionary<int, string>();

            var contractOwners = await _context.Contracts
                .Where(c => c.PropertyId != null && propertyIds.Contains(c.PropertyId.Value) && (!string.IsNullOrEmpty(c.ProprietarioNome) || !string.IsNullOrEmpty(c.LocatarioNome)))
                .GroupBy(c => c.PropertyId.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => !string.IsNullOrEmpty(x.ProprietarioNome) ? x.ProprietarioNome : x.LocatarioNome).FirstOrDefault());

            foreach (var owner in contractOwners)
            {
                ownersDict[owner.Key] = owner.Value;
            }

            foreach (var p in imoveis)
            {
                if (!string.IsNullOrWhiteSpace(p.Proprietario?.Nome))
                {
                    ownersDict[p.Id] = p.Proprietario.Nome;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(p.ProprietarioNome))
                {
                    ownersDict[p.Id] = p.ProprietarioNome;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(p.ProprietarioId))
                {
                    var userById = await _context.Users.FindAsync(p.ProprietarioId);
                    if (userById != null && !string.IsNullOrWhiteSpace(userById.Nome))
                    {
                        ownersDict[p.Id] = userById.Nome;
                        continue;
                    }

                    var userByName = await _context.Users.FirstOrDefaultAsync(u => u.Nome != null && u.Nome.ToLower() == p.ProprietarioId.ToLower());
                    if (userByName != null)
                    {
                        ownersDict[p.Id] = userByName.Nome;
                        continue;
                    }

                    ownersDict[p.Id] = p.ProprietarioId;
                }
            }

            ViewBag.PropertyOwners = ownersDict;

            System.Diagnostics.Debug.WriteLine($"[PropertyOwners] Total de imóveis: {imoveis.Count}, Total no dicionário: {ownersDict.Count}");
            foreach (var item in ownersDict.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"  ID {item.Key}: {item.Value}");
            }

            var contractTypeDict = await _context.Contracts
                .Where(c => c.PropertyId != null && propertyIds.Contains(c.PropertyId.Value))
                .GroupBy(c => c.PropertyId.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Type).FirstOrDefault());

            var contractTypeLabels = new Dictionary<int, string>();

            foreach (var kv in contractTypeDict)
            {
                var label = kv.Value switch
                {
                    Models.Entities.ContractType.LocatarioProprietario => "Locatário/Proprietário",
                    Models.Entities.ContractType.Condomino => "Condomínio",
                    Models.Entities.ContractType.Funcionario => "Funcionário",
                    _ => kv.Value.ToString()
                };
                contractTypeLabels[kv.Key] = label;
            }

            foreach (var p in imoveis)
            {
                if (!string.IsNullOrWhiteSpace(p.TipoImovel))
                {
                    contractTypeLabels[p.Id] = p.TipoImovel;
                }
                else
                {
                    if (!contractTypeLabels.ContainsKey(p.Id))
                    {
                        contractTypeLabels[p.Id] = "—";
                    }
                }
            }

            ViewBag.PropertyContractTypes = contractTypeLabels;
            return View(imoveis);
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public IActionResult Create()
        {
            ViewBag.ProprietarioNome = string.Empty;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Create(Property model)
        {
            var proprietarioNome = (Request.Form["ProprietarioNome"].FirstOrDefault() ?? string.Empty).Trim();
            var tipoImovel = (Request.Form["TipoImovel"].FirstOrDefault() ?? string.Empty).Trim();

            if (!ModelState.IsValid)
            {
                ViewBag.ProprietarioNome = proprietarioNome;
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(proprietarioNome))
            {
                var user = _context.Users.FirstOrDefault(u => u.Nome != null && u.Nome.ToLower() == proprietarioNome.ToLower());
                if (user != null)
                {
                    model.ProprietarioId = user.Id;
                }
                else
                {
                    model.ProprietarioId = null;
                }
            }
            else
            {
                model.ProprietarioId = null;
            }

            model.ProprietarioNome = string.IsNullOrWhiteSpace(proprietarioNome) ? null : proprietarioNome;

            model.TipoImovel = string.IsNullOrWhiteSpace(tipoImovel) ? null : tipoImovel;

            _context.Properties.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Edit(int id)
        {
            var imovel = await _context.Properties
                .Include(i => i.Proprietario)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imovel == null)
                return NotFound();
            string proprietarioNome = string.Empty;
            string debug_userById = string.Empty;
            string debug_userByName = string.Empty;

            if (imovel.Proprietario != null && !string.IsNullOrWhiteSpace(imovel.Proprietario.Nome))
            {
                proprietarioNome = imovel.Proprietario.Nome;
            }
            else if (!string.IsNullOrWhiteSpace(imovel.ProprietarioNome))
            {
                proprietarioNome = imovel.ProprietarioNome;
            }
            else if (!string.IsNullOrWhiteSpace(imovel.ProprietarioId))
            {
                var user = await _context.Users.FindAsync(imovel.ProprietarioId);
                if (user != null)
                {
                    proprietarioNome = user.Nome;
                }
                else
                {
                    var userByName = await _context.Users.FirstOrDefaultAsync(u => u.Nome != null && u.Nome.ToLower() == imovel.ProprietarioId.ToLower());
                    proprietarioNome = userByName?.Nome ?? string.Empty;
                }
            }
            else
            {
                var contractOwner = await _context.Contracts.Where(c => c.PropertyId == imovel.Id && !string.IsNullOrEmpty(c.ProprietarioNome)).Select(c => c.ProprietarioNome).FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(contractOwner))
                {
                    proprietarioNome = contractOwner;
                }
            }

            ViewBag.ProprietarioNome = proprietarioNome;
            ViewBag.TipoImovel = imovel.TipoImovel ?? string.Empty;
            return View(imovel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Edit(int id, Property model)
        {
            if (id != model.Id)
                return BadRequest();

            var proprietarioNome = (Request.Form["ProprietarioNome"].FirstOrDefault() ?? string.Empty).Trim();

            if (!ModelState.IsValid)
            {
                ViewBag.ProprietarioNome = proprietarioNome;
                return View(model);
            }

            var imovel = await _context.Properties.FindAsync(id);
            if (imovel == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(proprietarioNome))
            {
                var user = _context.Users.FirstOrDefault(u => u.Nome != null && u.Nome.ToLower() == proprietarioNome.ToLower());
                imovel.ProprietarioId = user?.Id;

                if (user == null && !string.IsNullOrWhiteSpace(imovel.ProprietarioId))
                {
                    var userByName = _context.Users.FirstOrDefault(u => u.Nome != null && u.Nome.ToLower() == imovel.ProprietarioId.ToLower());
                    if (userByName != null)
                    {
                        imovel.ProprietarioId = userByName.Id;
                    }
                }
            }
            else
            {
                imovel.ProprietarioId = null;
            }

            imovel.ProprietarioNome = string.IsNullOrWhiteSpace(proprietarioNome) ? null : proprietarioNome;

            imovel.Bloco = model.Bloco;
            imovel.Numero = model.Numero;
            imovel.TipoImovel = model.TipoImovel;
            imovel.Type = model.Type;
            imovel.Area = model.Area;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO, MORADOR")]
        public async Task<IActionResult> Detalhes(int id)
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "GESTOR, SINDICO, MORADOR")]
        public async Task<IActionResult> DebugProperty(int id)
        {
            var imovel = await _context.Properties
                .Include(i => i.Proprietario)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imovel == null)
                return NotFound();

            var contracts = await _context.Contracts
                .Where(c => c.PropertyId == id)
                .Select(c => new
                {
                    c.Id,
                    c.Type,
                    c.ProprietarioNome,
                    c.LocatarioNome,
                    c.TipoCondomino
                })
                .ToListAsync();

            return Json(new
            {
                Property = new
                {
                    imovel.Id,
                    imovel.Bloco,
                    imovel.Numero,
                    imovel.Type,
                    imovel.Area,
                    imovel.ProprietarioId,
                    ProprietarioNomeStored = imovel.ProprietarioNome,
                    ProprietarioUser = imovel.Proprietario?.Nome
                },
                Contracts = contracts
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> AtualizarStatus(int id, PropertyType status)
        {
            var imovel = await _context.Properties.FindAsync(id);
            if (imovel == null)
                return NotFound();

            imovel.Type = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detalhes), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GESTOR, SINDICO")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                TempData["Error"] = "ID do imóvel inválido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var imovel = await _context.Properties.FindAsync(id.Value);
                if (imovel == null)
                {
                    TempData["Error"] = "Imóvel não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var hasContracts = await _context.Contracts.AnyAsync(c => c.PropertyId == id.Value);
                var hasNotices = await _context.Notices.AnyAsync(n => n.PropertyId == id.Value);
                
                if (hasContracts || hasNotices)
                {
                    TempData["Error"] = "Não é possível excluir o imóvel enquanto existirem contratos ou vistorias vinculadas.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Properties.Remove(imovel);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Imóvel excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {dbEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner: {dbEx.InnerException?.Message}");
                TempData["Error"] = "Erro ao excluir: Verifique se há registros vinculados.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = "Erro ao excluir o imóvel.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}