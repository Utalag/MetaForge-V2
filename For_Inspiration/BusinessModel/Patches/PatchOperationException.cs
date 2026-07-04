using System;
using System.Text.Json;

namespace MetaForge.BusinessModel;

internal sealed class PatchOperationException : Exception
{
    public PatchOperationException(string code, string message, string path)
        : base(message)
    {
        Code = code;
        Path = path;
    }

    public string Code { get; }

    public string Path { get; }
}
