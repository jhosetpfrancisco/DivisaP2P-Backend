namespace DivisaP2P.Library.Core.Common;

/// <summary>
/// Códigos de error de negocio que el controlador traduce a códigos HTTP.
/// </summary>
public enum CodigoError
{
    Ninguno = 0,
    NoEncontrado = 404,
    Validacion = 400,
    NoAutorizado = 403,
    Conflicto = 409
}

/// <summary>
/// Resultado de una operación de servicio. Lleva el estado de éxito, un mensaje
/// para el usuario y (en la variante genérica) los datos resultantes. Permite que
/// los controladores se mantengan finos y traduzcan el resultado a la respuesta HTTP.
/// </summary>
public class ResultadoServicio
{
    public bool Exito { get; init; }
    public string? Mensaje { get; init; }
    public CodigoError Codigo { get; init; }

    public static ResultadoServicio Ok(string? mensaje = null) =>
        new() { Exito = true, Mensaje = mensaje };

    public static ResultadoServicio Error(CodigoError codigo, string mensaje) =>
        new() { Exito = false, Codigo = codigo, Mensaje = mensaje };
}

public class ResultadoServicio<T> : ResultadoServicio
{
    public T? Datos { get; init; }

    public static ResultadoServicio<T> Ok(T datos, string? mensaje = null) =>
        new() { Exito = true, Datos = datos, Mensaje = mensaje };

    public static new ResultadoServicio<T> Error(CodigoError codigo, string mensaje) =>
        new() { Exito = false, Codigo = codigo, Mensaje = mensaje };
}
