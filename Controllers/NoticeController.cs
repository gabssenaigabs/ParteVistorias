using CondoHub.Models.Entities;
using CondoHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CondoHub.Controllers
{
    [Authorize]
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NoticeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string tab = "")
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Unauthorized();

                List<Notice> notices;

                if (User.IsInRole("GESTOR") || User.IsInRole("SINDICO") || User.IsInRole("MORADOR"))
                {
                    notices = await _context.Notices
                        .Include(n => n.Solicitante)
                        .Include(n => n.Gestor)
                        .Include(n => n.Itens)
                        .Include(n => n.Fotos)
                        .Include(n => n.Property)
                        .ToListAsync();
                }
                else
                {
                    notices = await _context.Notices
                        .Include(n => n.Solicitante)
                        .Include(n => n.Gestor)
                        .Include(n => n.Itens)
                        .Include(n => n.Fotos)
                        .Include(n => n.Property)
                        .Where(n => n.SolicitanteId == user.Id)
                        .ToListAsync();
                }

                // Define a aba padrão baseado no papel do usuário
                string activeTab = tab;
                if (string.IsNullOrEmpty(tab))
                {
                    if (User.IsInRole("GESTOR"))
                    {
                        activeTab = "checklist";
                    }
                    else if (User.IsInRole("MORADOR") || User.IsInRole("SINDICO"))
                    {
                        activeTab = "historico";
                    }
                }

                ViewBag.ActiveTab = activeTab;
                var properties = await _context.Properties
                    .OrderBy(p => p.Numero)
                    .ToListAsync();

                ViewBag.Properties = properties;
                return View(notices);
            }
            catch (Exception ex)
            {
                // Instrumentação temporária para diagnóstico: retorna mensagem de erro
                // Remova ou ajuste após identificar o problema
                return Content($"Erro ao carregar página de vistorias: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> Iniciar(int id)
        {
            var notice = await _context.Notices
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            if (notice == null)
                return NotFound();

            notice.GestorId = user.Id;
            notice.Status = NoticeStatus.EmProgresso;

            if (!notice.Itens.Any())
            {
                var itensPadrao = new List<ChecklistItem>
                {
                    new ChecklistItem { Categoria = "Sala", Descricao = "Paredes e pintura" },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Piso e revestimento" },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Portas e janelas" },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Iluminação" },

                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Armários e bancadas" },
                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Pia e torneiras" },
                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Fogão e coifa" },

                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Vaso sanitário" },
                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Box e chuveiro" },
                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Pia e torneiras" },

                    new ChecklistItem { Categoria = "Quartos", Descricao = "Paredes e pintura" },
                    new ChecklistItem { Categoria = "Quartos", Descricao = "Armários embutidos" },

                    new ChecklistItem { Categoria = "Área de Serviço", Descricao = "Tanque" },
                    new ChecklistItem { Categoria = "Área de Serviço", Descricao = "Instalações elétricas" },

                    new ChecklistItem { Categoria = "Geral", Descricao = "Chaves entregues" }
                };

                foreach (var item in itensPadrao)
                {
                    item.NoticeId = notice.Id;
                    _context.ChecklistItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Checklist", new { id = notice.Id });
        }

        [HttpPost]
        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> IniciarVistoria([FromBody] IniciarVistoriaDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "Usuário não autenticado" });

                var notice = new Notice
                {
                    PropertyId = dto.PropertyId,
                    SolicitanteId = user.Id,
                    GestorId = user.Id,
                    Status = NoticeStatus.EmProgresso,
                    DataCriacao = DateTime.UtcNow
                };
                _context.Notices.Add(notice);
                await _context.SaveChangesAsync();

                var itensPadrao = new List<ChecklistItem>
                {
                    new ChecklistItem { Categoria = "Sala", Descricao = "Paredes e pintura", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Piso e revestimento", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Portas e janelas", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Sala", Descricao = "Iluminação", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Armários e bancadas", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Pia e torneiras", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Cozinha", Descricao = "Fogão e coifa", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Vaso sanitário", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Box e chuveiro", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Banheiro", Descricao = "Pia e torneiras", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Quartos", Descricao = "Paredes e pintura", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Quartos", Descricao = "Armários embutidos", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Área de Serviço", Descricao = "Tanque", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Área de Serviço", Descricao = "Instalações elétricas", NoticeId = notice.Id },
                    new ChecklistItem { Categoria = "Geral", Descricao = "Chaves entregues", NoticeId = notice.Id }
                };
                _context.ChecklistItems.AddRange(itensPadrao);
                await _context.SaveChangesAsync();

                var itemIds = itensPadrao.Select(i => i.Id).ToList();
                return Json(new { success = true, noticeId = notice.Id, itemIds = itemIds });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao criar vistoria: {ex.Message}" });
            }
        }

        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> Checklist(int id)
        {
            var notice = await _context.Notices
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notice == null)
                return NotFound();

            return View(notice);
        }

        [Authorize]
        public async Task<IActionResult> ViewChecklist(int id)
        {
            var notice = await _context.Notices
                .Include(n => n.Itens)
                .Include(n => n.Solicitante)
                .Include(n => n.Gestor)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notice == null)
                return NotFound();

            ViewBag.IsReadOnly = true;
            return View("Checklist", notice);
        }

        [HttpPost]
        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> SalvarChecklist(int id, [FromBody] List<ChecklistItemDto> itens)
        {
            try
            {
                var notice = await _context.Notices
                    .Include(n => n.Itens)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notice == null)
                    return Json(new { success = false, message = "Vistoria não encontrada" });

                foreach (var item in itens)
                {
                    var itemDb = notice.Itens.FirstOrDefault(i => i.Id == item.Id);
                    if (itemDb != null)
                    {
                        if (Enum.TryParse<ItemStatus>(item.Status, ignoreCase: true, out var status))
                        {
                            itemDb.Status = status;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Checklist salvo com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao salvar checklist: {ex.Message}" });
            }
        }

        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> Concluir(int id)
        {
            var notice = await _context.Notices.FindAsync(id);

            if (notice == null)
                return NotFound();

            notice.Status = NoticeStatus.Resolvido;
            notice.DataConclusao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vistoria concluída!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> ConcluirVistoria(int id)
        {
            try
            {
                var notice = await _context.Notices.FindAsync(id);

                if (notice == null)
                    return Json(new { success = false, message = "Vistoria não encontrada" });

                notice.Status = NoticeStatus.Resolvido;
                notice.DataConclusao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Vistoria concluída com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao concluir vistoria: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Historico()
        {
            var user = await _userManager.GetUserAsync(User);
            List<Notice> historico;

            if (user == null)
                return Unauthorized();

            if (User.IsInRole("GESTOR") || User.IsInRole("SINDICO") || User.IsInRole("MORADOR"))
            {
                historico = await _context.Notices
                    .Include(n => n.Solicitante)
                    .Include(n => n.Gestor)
                    .Include(n => n.Itens)
                    .Include(n => n.Fotos)
                    .ToListAsync();
            }
            else
            {
                historico = await _context.Notices
                    .Include(n => n.Solicitante)
                    .Include(n => n.Itens)
                    .Include(n => n.Fotos)
                    .Where(n => n.SolicitanteId == user.Id)
                    .ToListAsync();
            }

            return View(historico);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadPhoto(int noticeId, int? checklistItemId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Nenhum arquivo foi selecionado" });

                if (file.Length > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "Arquivo muito grande (máx 5MB)" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                    return Json(new { success = false, message = "Tipo de arquivo não permitido. Use jpg, png ou gif" });

                var notice = await _context.Notices.FindAsync(noticeId);
                if (notice == null)
                    return Json(new { success = false, message = "Vistoria não encontrada" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "vistorias");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var photoUrl = $"/uploads/vistorias/{uniqueFileName}";
                var noticeFoto = new NoticeFoto
                {
                    NoticeId = noticeId,
                    ChecklistItemId = checklistItemId,
                    FotoFile = photoUrl,
                    DataUpload = DateTime.UtcNow
                };
                
                _context.NoticeFotos.Add(noticeFoto);
                await _context.SaveChangesAsync();

                return Json(new { success = true, filePath = photoUrl, fileName = uniqueFileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao processar arquivo: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetChecklistItems(int id)
        {
            try
            {
                var notice = await _context.Notices
                    .Include(n => n.Itens)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notice == null)
                    return Json(new { success = false, message = "Vistoria não encontrada" });

                var itemIds = notice.Itens.Select(i => i.Id).ToList();
                return Json(new { success = true, itemIds = itemIds });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao carregar items: {ex.Message}" });
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNoticeDetails(int id)
        {
            try
            {
                var notice = await _context.Notices
                    .Include(n => n.Itens)
                    .Include(n => n.Fotos)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notice == null)
                    return Json(new { success = false, message = "Vistoria não encontrada" });

                var noticeData = new
                {
                    id = notice.Id,
                    itens = notice.Itens.Select(i => new
                    {
                        id = i.Id,
                        categoria = i.Categoria,
                        descricao = i.Descricao,
                        status = i.Status.ToString()
                    }).ToList(),
                    fotos = notice.Fotos.Select(f => new
                    {
                        id = f.Id,
                        fotoFile = f.FotoFile,
                        checklistItemId = f.ChecklistItemId
                    }).ToList()
                };

                return Json(new { success = true, notice = noticeData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }
    }

    public class IniciarVistoriaDto
    {
        public int PropertyId { get; set; }
    }

    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
}
