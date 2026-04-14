using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

/// <summary>
/// Controller per la gestione delle categorie.
/// </summary>
public class CategoryController : Controller
{
    private const string RequiredCodeMessage = "Il campo code è obbligatorio.";
    private const string CategoryNotFoundMessage = "Categoria non trovata.";

    /// <summary>
    /// Recupera la lista di tutte le categorie con ordinamento.
    /// </summary>
    public IActionResult Index(string sortBy = "code", string sortDirection = "asc")
    {
        return View(MockDataService.GetCategoryPageData(sortBy, sortDirection));
    }

    /// <summary>
    /// Ottiene i dettagli di una categoria specifica.
    /// </summary>
    /// <param name="code">Codice della categoria.</param>
    public IActionResult Details(string code)
    {
        var category = FindCategory(code);

        return category == null
            ? CategoryLookupError(code)
            : View(category);
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
        return ModelState.IsValid
            ? RedirectToAction(nameof(Index))
            : View(category);
    }

    /// <summary>
    /// Mostra il modulo per modificare una categoria esistente.
    /// </summary>
    /// <param name="code">Codice della categoria da modificare.</param>
    public IActionResult Edit(string code)
    {
        var category = FindCategory(code);

        return category == null
            ? CategoryLookupError(code)
            : View(category);
    }

    /// <summary>
    /// Aggiorna una categoria esistente tramite POST.
    /// </summary>
    /// <param name="category">Oggetto categoria con i dati aggiornati.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        return ModelState.IsValid
            ? RedirectToAction(nameof(Index))
            : View(category);
    }

    /// <summary>
    /// Conferma l'eliminazione di una categoria.
    /// </summary>
    /// <param name="code">Codice della categoria da eliminare.</param>
    public IActionResult Delete(string code)
    {
        var category = FindCategory(code);

        return category == null
            ? CategoryLookupError(code)
            : View(category);
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

    private static Category? FindCategory(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return MockDataService
            .GetCategories()
            .FirstOrDefault(category => string.Equals(category.Code, code, StringComparison.OrdinalIgnoreCase));
    }

    private IActionResult CategoryLookupError(string code)
    {
        return string.IsNullOrWhiteSpace(code)
            ? BadRequest(RequiredCodeMessage)
            : NotFound(CategoryNotFoundMessage);
    }
}
