using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Test.Services;

public class PruebasUnitarias
{
    private readonly IPersistence _persistence;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppointmentService _appointmentTest;

    public PruebasUnitarias()
    {
        _persistence = Substitute.For<IPersistence>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        _appointmentTest = new AppointmentService(_persistence, _userManager); //Objeto de prueba
    }

    [Fact]
    public async Task AppointmentCreate_CuandoElDoctorNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var request = new AppointmentModel.CreateRequest(
            DoctorId: Guid.NewGuid(),
            AvailabilitySlotId: Guid.NewGuid(),
            PatientDni: 40123456,
            Reason: "Control de rutina");

        _persistence.GetById<Doctor>(request.DoctorId).Returns((Doctor?)null); //aqui hace como que el doctor no existe en la bd, asi que devuelve null.

        // Act
        var act = () => _appointmentTest.Create(request); //Prueba el create por el camino del doctor is null

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(act); //Aqui espera a q llegue la excepcion en el "act" 
    }

    [Fact]
    public async Task AppointmentCreate_CuandoElDoctorEstaEliminado_EntoncesLanzaEntityNotFoundException(){}

    [Fact]
    public async Task AppointmentCreate_CuandoElSlotYaEstaReservado_EntoncesLanzaConflictException(){}

    [Fact]
    public async Task AppointmentCreate_CuandoTodoEsValido_EntoncesCreaElTurnoConEstadoBooked(){}
}