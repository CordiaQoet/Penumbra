using System;
using System.Collections.Generic;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Penumbra.Util;

namespace Penumbra;

public class ValidityChecker
{
    public const string Repository          = "https://raw.githubusercontent.com/xivdev/Penumbra/master/repo.json";
    public const string RepositoryLower     = "https://raw.githubusercontent.com/xivdev/penumbra/master/repo.json";
    public const string TestRepositoryLower = "https://raw.githubusercontent.com/xivdev/penumbra/test/repo.json";

    public readonly bool DevPenumbraExists;
    public readonly bool IsNotInstalledPenumbra;
    public readonly bool IsValidSourceRepo;

    public readonly List<Exception> ImcExceptions = new();

    public ValidityChecker(DalamudPluginInterface pi)
    {
        DevPenumbraExists      = CheckDevPluginPenumbra(pi);
        IsNotInstalledPenumbra = CheckIsNotInstalled(pi);
        IsValidSourceRepo      = CheckSourceRepo(pi);
    }

    public void LogExceptions()
    {
        if( ImcExceptions.Count > 0 )
            ChatUtil.NotificationMessage( $"{ImcExceptions} IMC Exceptions thrown during Penumbra load. Please repair your game files.", "Warning", NotificationType.Warning );
    }

    // Because remnants of penumbra in devPlugins cause issues, we check for them to warn users to remove them.
    private static bool CheckDevPluginPenumbra( DalamudPluginInterface pi )
    {
#if !DEBUG
        var path = Path.Combine( pi.DalamudAssetDirectory.Parent?.FullName ?? "INVALIDPATH", "devPlugins", "Penumbra" );
        var dir = new DirectoryInfo( path );

        try
        {
            return dir.Exists && dir.EnumerateFiles( "*.dll", SearchOption.AllDirectories ).Any();
        }
        catch( Exception e )
        {
            Log.Error( $"Could not check for dev plugin Penumbra:\n{e}" );
            return true;
        }
#else
        return false;
#endif
    }

    // Check if the loaded version of Penumbra itself is in devPlugins.
    private static bool CheckIsNotInstalled( DalamudPluginInterface pi )
    {
#if !DEBUG
        var checkedDirectory = pi.AssemblyLocation.Directory?.Parent?.Parent?.Name;
        var ret = checkedDirectory?.Equals( "installedPlugins", StringComparison.OrdinalIgnoreCase ) ?? false;
        if( !ret )
        {
            Log.Error( $"Penumbra is not correctly installed. Application loaded from \"{pi.AssemblyLocation.Directory!.FullName}\"." );
        }

        return !ret;
#else
        return false;
#endif
    }

    // Check if the loaded version of Penumbra is installed from a valid source repo.
    private static bool CheckSourceRepo( DalamudPluginInterface pi )
    {
#if !DEBUG
        return pi.SourceRepository.Trim().ToLowerInvariant() switch
        {
            null                => false,
            RepositoryLower     => true,
            TestRepositoryLower => true,
            _                   => false,
        };
#else
        return true;
#endif
    }
}