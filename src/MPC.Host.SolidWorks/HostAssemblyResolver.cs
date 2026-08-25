using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MPC.Host.SolidWorks;

internal static class HostAssemblyResolver
{
    private static readonly HashSet<string>
        AllowedAssemblyNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "Microsoft.Bcl.AsyncInterfaces",
                "System.Buffers",
                "System.Memory",
                "System.Numerics.Vectors",
                "System.Runtime.CompilerServices.Unsafe",
                "System.Text.Encodings.Web",
                "System.Text.Json",
                "System.Threading.Tasks.Extensions",
                "System.ValueTuple"
            };

    private static bool _isInstalled;

    internal static void Install()
    {
        if (_isInstalled)
        {
            return;
        }

        AppDomain.CurrentDomain.AssemblyResolve +=
            ResolveLocalAssembly;

        _isInstalled = true;
    }

    private static Assembly? ResolveLocalAssembly(
        object sender,
        ResolveEventArgs args)
    {
        var requestedName =
            new AssemblyName(
                args.Name);

        string? simpleName =
            requestedName.Name;

        if (string.IsNullOrWhiteSpace(simpleName) ||
            !AllowedAssemblyNames.Contains(simpleName))
        {
            return null;
        }

        string assemblyDirectory =
            Path.GetDirectoryName(
                typeof(HostAssemblyResolver)
                    .Assembly
                    .Location)
            ?? string.Empty;

        string candidatePath =
            Path.Combine(
                assemblyDirectory,
                simpleName + ".dll");

        if (!File.Exists(candidatePath))
        {
            return null;
        }

        try
        {
            return Assembly.LoadFrom(
                candidatePath);
        }
        catch (Exception exception)
            when (exception is FileLoadException
                  || exception is FileNotFoundException
                  || exception is BadImageFormatException)
        {
            return null;
        }
    }
}
