using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using System;
using System.Threading.Tasks;

namespace Test_SDC;

[TestClass]
public class ClienteTest
{
    private ClienteController _controller;
    private AppDBContext _context;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDBContext(options);

        _context.Clientes.Add(new Cliente
        {
            cliente_id = 1,
            nombre_cliente = "Juan Perez",
            correo_cliente = "juan@correo.com",
            ruc = "12345678",
            fecha_registro_cliente = DateTime.Now,
            telefono_cliente = "999999999",
            direccion_cliente = "Calle Falsa 123",
            tipo = "Persona Natural"
        });
        _context.SaveChanges();

        _controller = new ClienteController(_context);
    }

    [TestMethod]
    public async Task NuevoCliente_CreacionExitosa()
    {
        var cliente = new Cliente
        {
            nombre_cliente = "Ana Torres",
            correo_cliente = "ana@correo.com",
            ruc = "87654321",
            telefono_cliente = "888888888",
            direccion_cliente = "Av. Real 456",
            tipo = "Persona Natural"
        };

        var result = await _controller.Nuevo(cliente);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirect = result as RedirectToActionResult;
        Assert.AreEqual("Mostrar", redirect.ActionName);
    }

    [TestMethod]
    public async Task NuevoCliente_CorreoDuplicado()
    {
        var cliente = new Cliente
        {
            nombre_cliente = "Pedro",
            correo_cliente = "juan@correo.com",
            ruc = "99999999",
            telefono_cliente = "777777777",
            direccion_cliente = "Av. Siempre Viva",
            tipo = "Persona Natural"
        };

        var result = await _controller.Nuevo(cliente);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var view = result as ViewResult;
        Assert.AreEqual("Correo ya registrado.", view.ViewData["mensaje"]);
    }

    [TestMethod]
    public async Task EditarCliente_ActualizaDatos()
    {
        var cliente = new Cliente
        {
            cliente_id = 1,
            nombre_cliente = "Juan Actualizado",
            correo_cliente = "juan@correo.com",
            ruc = "12345678",
            telefono_cliente = "999999999",
            direccion_cliente = "Calle Falsa 123",
            tipo = "Persona Natural"
        };

        var result = await _controller.Editar(cliente);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var actualizado = await _context.Clientes.FindAsync(1);
        Assert.AreEqual("Juan Actualizado", actualizado.nombre_cliente);
    }

    [TestMethod]
    public void EliminarCliente_MuestraVista()
    {
        var result = _controller.Eliminar(1);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var view = result as ViewResult;
        Assert.IsNotNull(view.Model);
    }

    [TestMethod]
    public void ConfirmacionEliminar_EliminaCliente()
    {
        var result = _controller.ConfirmacionEliminar(1);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var cliente = _context.Clientes.Find(1);
        Assert.IsNull(cliente);
    }
}

