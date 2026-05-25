using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

public class Administrador : Usuario
{
    public Administrador()
    {
        Papel = PapelUsuario.Administrador;
    }

    // Campos eventualmente especificos do Administrador podem entrar aqui.
    public string? Cargo { get; set; }
}
