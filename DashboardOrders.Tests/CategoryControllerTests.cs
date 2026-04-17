using DashboardOrders.Controllers;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class CategoryControllerTests
{
    private readonly IDashboardOrdersDataService dataService;
    private readonly CategoryController sut;

    public CategoryControllerTests()
    {
        dataService = Substitute.For<IDashboardOrdersDataService>();
        sut = new CategoryController(dataService);
    }

    [Fact]
    public void Index_QuandoRichiesto_AlloraRestituisceVistaConModello()
    {
        // Arrange
        var modelloAtteso = new CategoryPageViewModel { TotalCategories = 2 };
        dataService.GetCategoryPageData("code", "asc").Returns(modelloAtteso);

        // Act
        var risultato = sut.Index();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(modelloAtteso);
    }

    [Fact]
    public void Details_QuandoCodiceMancante_AlloraRestituisceBadRequest()
    {
        // Arrange
        var code = string.Empty;

        // Act
        var risultato = sut.Details(code);

        // Assert
        risultato.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Details_QuandoCategoriaAssente_AlloraRestituisceNotFound()
    {
        // Arrange
        const string code = "CAT-404";
        dataService.GetCategory(code).Returns((Category?)null);

        // Act
        var risultato = sut.Details(code);

        // Assert
        risultato.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Details_QuandoCategoriaEsiste_AlloraRestituisceVistaConCategoria()
    {
        // Arrange
        var categoriaAttesa = new Category { Code = "CAT-001", Name = "Informatica" };
        dataService.GetCategory(categoriaAttesa.Code).Returns(categoriaAttesa);

        // Act
        var risultato = sut.Details(categoriaAttesa.Code);

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(categoriaAttesa);
    }

    [Fact]
    public void Create_QuandoCategoriaValida_AlloraReindirizzaAIndex()
    {
        // Arrange
        var categoria = new Category { Code = "CAT-900", Name = "Test", Description = "Categoria test" };
        dataService.CreateCategory(categoria).Returns(true);

        // Act
        var risultato = sut.Create(categoria);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(CategoryController.Index));
    }

    [Fact]
    public void Create_QuandoCategoriaNull_AlloraRestituisceVistaConModelStateNonValido()
    {
        // Arrange
        Category? categoria = null;

        // Act
        var risultato = sut.Create(categoria);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_QuandoCategoriaDuplicata_AlloraRestituisceVistaConErrore()
    {
        // Arrange
        var categoria = new Category { Code = "CAT-001", Name = "Duplicata" };
        dataService.CreateCategory(categoria).Returns(false);

        // Act
        var risultato = sut.Create(categoria);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Edit_QuandoCategoriaValida_AlloraReindirizzaAIndex()
    {
        // Arrange
        var categoria = new Category { Code = "CAT-001", Name = "Informatica", Description = "Aggiornata" };
        dataService.UpdateCategory(categoria).Returns(true);

        // Act
        var risultato = sut.Edit(categoria);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(CategoryController.Index));
    }

    [Fact]
    public void Edit_QuandoCategoriaNull_AlloraRestituisceVistaConModelStateNonValido()
    {
        // Arrange
        Category? categoria = null;

        // Act
        var risultato = sut.Edit(categoria);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteConfirmed_QuandoCodiceMancante_AlloraRestituisceBadRequest()
    {
        // Arrange
        var code = string.Empty;

        // Act
        var risultato = sut.DeleteConfirmed(code);

        // Assert
        risultato.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void DeleteConfirmed_QuandoCategoriaAssente_AlloraRestituisceNotFound()
    {
        // Arrange
        const string code = "CAT-404";
        dataService.GetCategory(code).Returns((Category?)null);

        // Act
        var risultato = sut.DeleteConfirmed(code);

        // Assert
        risultato.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void DeleteConfirmed_QuandoCategoriaConProdotti_AlloraRestituisceConflict()
    {
        // Arrange
        const string code = "CAT-001";
        dataService.GetCategory(code).Returns(new Category { Code = code, Name = "Informatica" });
        dataService.DeleteCategory(code).Returns(false);

        // Act
        var risultato = sut.DeleteConfirmed(code);

        // Assert
        risultato.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void DeleteConfirmed_QuandoCategoriaEliminabile_AlloraReindirizzaAIndex()
    {
        // Arrange
        const string code = "CAT-900";
        dataService.GetCategory(code).Returns(new Category { Code = code, Name = "Test" });
        dataService.DeleteCategory(code).Returns(true);

        // Act
        var risultato = sut.DeleteConfirmed(code);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(CategoryController.Index));
    }
}
