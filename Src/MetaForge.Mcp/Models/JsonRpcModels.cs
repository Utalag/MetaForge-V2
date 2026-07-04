using System.Text.Json;

namespace MetaForge.Mcp.Models;

/// <summary>
/// JSON-RPC 2.0 požadavek.
/// </summary>
public sealed class JsonRpcRequest
{
    /// <summary>Identifikátor požadavku (null pro notifikace).</summary>
    public string? Id { get; set; }

    /// <summary>Název volané metody.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Parametry metody.</summary>
    public JsonElement? Params { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 odpověď.
/// </summary>
public sealed class JsonRpcResponse
{
    /// <summary>Identifikátor odpovídající požadavku.</summary>
    public string? Id { get; set; }

    /// <summary>Výsledek volání (null při chybě).</summary>
    public object? Result { get; set; }

    /// <summary>Chyba (null při úspěchu).</summary>
    public JsonRpcError? Error { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 chyba.
/// </summary>
public sealed class JsonRpcError
{
    /// <summary>Číselný kód chyby.</summary>
    public int Code { get; set; }

    /// <summary>Textový popis chyby.</summary>
    public string Message { get; set; } = string.Empty;
}
