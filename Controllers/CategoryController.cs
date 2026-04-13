using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

/// <summary>
/// Controller per la gestione delle categorie.
/// </summary>
public class CategoryController : Controller
{
    /// <summary>
    /// Recupera la lista di tutte le categorie.
    /// </summary>
    public IActionResult Index()
    {
        var model = MockDataService.GetCategories();
        return View(model);
    }

    /// <summary>
    /// Ottiene i dettagli di una categoria specifica.
    /// </summary>
    /// <param name="code">Codice della categoria.</param>
    public IActionResult Details(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Il campo code è obbligatorio.");
        }

        var categories = MockDataService.GetCategories();
        var category = categories.FirstOrDefault(c => c.Code == code);

        if (category == null)
        {
            return NotFound("Categoria non trovata.");
        }

        return View(category);
    }

    /// <summary>
    /// Mostra il modulo per creare una nuova categoria.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Crea una nuova categoria tramite POST.
    /// </summary>
    /// <param name="category">Oggetto categoria da creare.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Mostra il modulo per modificare una categoria esistente.
    /// </summary>
    /// <param name="code">Codice della categoria da modificare.</param>
    public IActionResult Edit(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Il campo code è obbligatorio.");
        }

        var categories = MockDataService.GetCategories();
        var category = categories.FirstOrDefault(c => c.Code == code);

        if (category == null)
        {
            return NotFound("Categoria non trovata.");
        }

        return View(category);
    }

    /// <summary>
    /// Aggiorna una categoria esistente tramite POST.
    /// </summary>
    /// <param name="category">Oggetto categoria con i dati aggiornati.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Conferma l'eliminazione di una categoria.
    /// </summary>
    /// <param name="code">Codice della categoria da eliminare.</param>
    public IActionResult Delete(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Il campo code è obbligatorio.");
        }

        var categories = MockDataService.GetCategories();
        var category = categories.FirstOrDefault(c => c.Code == code);

        if (category == null)
        {
            return NotFound("Categoria non trovata.");
        }

        return View(category);
    }

    /// <summary>
    /// Elimina una categoria tramite POST.
    /// </summary>
    /// <param name="category">Oggetto categoria da eliminare.</param>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Category category)
    {
        return RedirectToAction(nameof(Index));
    }
}
