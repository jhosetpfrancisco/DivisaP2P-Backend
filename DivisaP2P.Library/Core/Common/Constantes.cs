namespace DivisaP2P.Library.Core.Common;

/// <summary>
/// Constantes del dominio. Centraliza los valores válidos de los campos de tipo
/// "estado" / "tipo" que se persisten como texto, para evitar literales sueltos
/// repartidos por los servicios.
/// </summary>
public static class Roles
{
    public const string Usuario = "USU";
    public const string Empresa = "ETU";
    public const string Administrador = "ADM";
}

public static class EstadosUsuario
{
    public const string PendienteVerificacion = "PendienteVerificacion";
    public const string PendienteAprobacion = "PendienteAprobacion";
    public const string Activo = "Activo";
    public const string Bloqueado = "Bloqueado";
}

public static class Divisas
{
    public const string Pen = "PEN";
    public const string Usd = "USD";
    public const string Eur = "EUR";

    public static readonly string[] Soportadas = { Pen, Usd, Eur };

    public static bool EsValida(string divisa) => Soportadas.Contains(divisa);
}

public static class TipoOperacion
{
    public const string Compra = "Compra";
    public const string Venta = "Venta";
}

public static class EstadosOferta
{
    public const string Activa = "Activa";
    public const string Expirada = "Expirada";
    public const string Agotada = "Agotada";
    public const string Cancelada = "Cancelada";
}

public static class EstadosTransaccion
{
    public const string PendientePago = "PendientePago";
    public const string PagoReportado = "PagoReportado";
    public const string PagoConfirmado = "PagoConfirmado";
    public const string EntregaReportada = "EntregaReportada";
    public const string Completada = "Completada";
    public const string Cancelada = "Cancelada";
    public const string EnDisputa = "EnDisputa";
}

public static class TipoVoucher
{
    public const string Pago = "Pago";
    public const string Entrega = "Entrega";
}

public static class EstadosDisputa
{
    public const string Abierta = "Abierta";
    public const string Resuelta = "Resuelta";
}

public static class ResolucionDisputa
{
    public const string AFavorComprador = "AFavorComprador";
    public const string AFavorVendedor = "AFavorVendedor";
    public const string Anulada = "Anulada";
}

/// <summary>
/// Reglas de negocio numéricas declaradas en el documento del proyecto (RN-005, RN-006, etc.).
/// </summary>
public static class ReglasNegocio
{
    public const decimal MontoMinimoOferta = 100m;
    public const decimal MontoMaximoUsuario = 50_000m;
    public const decimal MontoMaximoEmpresa = 500_000m;
    public const int VigenciaOfertaHoras = 24;
    public const int MaxOfertasActivasUsuario = 5;
    public const int MinutosReportePago = 30;
    public const int HorasValidacionPago = 2;
    public const int DiasParaCalificar = 7;
    public const int MaxIntentosLogin = 5;
    public const int MinutosBloqueoLogin = 15;
    public const int MinutosExpiracionJwt = 60;
}
