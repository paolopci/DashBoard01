using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

/// <summary>
/// Controller per la gestione delle categorie.
/// </summary>
[Authorize]
public class CategoryController : Controller
{
    private const string RequiredCodeMessage = "Il campo code è obbligatorio.";
    private const string RequiredNameMessage = "Il campo name è obbligatorio.";
    private const string CategoryNotFoundMessage = "Categoria non trovata.";
    private const string CategoryDeleteBlockedMessage = "Categoria non eliminabile perché ha prodotti associati.";
    private const string CategoryDuplicateMessage = "Categoria già presente.";

    private readonly IDashboardOrdersDataService dataService;

    public CategoryController(IDashboardOrdersDataService dataService)
    {
        this.dataService = dataService;
    }

    /// <summary>
    /// Recupera la lista di tutte le categorie con ordinamento.
    /// </summary>
    public IActionResult Index(string sortBy = "code", string sortDirection = "asc")
    {
        return View(dataService.GetCategoryPageData(sortBy, sortDirection));
    }

    /// <summary>
    /// Ottiene i dettagli di una categoria specifica.
    /// </summary>
    /// <param name="code">Codice della categoria.</param>
    public IActionResult Details(string code)
    {
        var category = dataService.GetCategory(code);

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
    public IActionResult Create(Category? category)
    {
        category ??= new Category();
        ValidateCategory(category, requireCode: true, requireName: true);

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        if (!dataService.CreateCategory(category))
        {
            ModelState.AddModelError(nameof(Category.Code), CategoryDuplicateMessage);
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
        var category = dataService.GetCategory(code);

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
    public IActionResult Edit(Category? category)
    {
        category ??= new Category();
        ValidateCategory(category, requireCode: true, requireName: true);

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        return dataService.UpdateCategory(category)
            ? RedirectToAction(nameof(Index))
            : CategoryLookupError(category.Code);
    }

    /// <summary>
    /// Conferma l'eliminazione di una categoria.
    /// </summary>
    /// <param name="code">Codice della categoria da eliminare.</param>
    public IActionResult Delete(string code)
    {
        var category = dataService.GetCategory(code);

        return category == null
            ? CategoryLookupError(code)
            : View(category);
    }

    /// <summary>
    /// Elimina una categoria tramite POST.
    /// </summary>
    /// <param name="code">Codice della categoria da eliminare.</param>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(RequiredCodeMessage);
        }

        var category = dataService.GetCategory(code);
        if (category == null)
        {
            return NotFound(CategoryNotFoundMessage);
        }

        if (!dataService.DeleteCategory(code))
        {
            return Conflict(CategoryDeleteBlockedMessage);
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult CategoryLookupError(string code)
    {
        return string.IsNullOrWhiteSpace(code)
            ? BadRequest(RequiredCodeMessage)
            : NotFound(CategoryNotFoundMessage);
    }

    private void ValidateCategory(Category category, bool requireCode, bool requireName)
    {
        if (requireCode && string.IsNullOrWhiteSpace(category.Code))
        {
            ModelState.AddModelError(nameof(Category.Code), RequiredCodeMessage);
        }

        if (requireName && string.IsNullOrWhiteSpace(category.Name))
        {
            ModelState.AddModelError(nameof(Category.Name), RequiredNameMessage);
        }
    }
}
