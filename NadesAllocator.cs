using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;

namespace NadesAllocator;

public class NadesAllocatorConfig : BasePluginConfig
{
    [JsonPropertyName("Admin")]
    public AdminConfig Admin { get; set; } = new();

    [JsonPropertyName("VIP")]
    public VIPConfig VIP { get; set; } = new();

    [JsonPropertyName("Player")]
    public PlayerConfig Player { get; set; } = new();
}

public class AdminConfig
{
    [JsonPropertyName("flag")]
    public string flag { get; set; } = "@css/admin";

    [JsonPropertyName("hegrenade")]
    public float hegrenade { get; set; } = 0.5f;

    [JsonPropertyName("flashbang")]
    public float flashbang { get; set; } = 0.5f;

    [JsonPropertyName("smoke")]
    public float smoke { get; set; } = 0.5f;

    [JsonPropertyName("decoy")]
    public float decoy { get; set; } = 0.5f;

    [JsonPropertyName("firegrenade")]
    public float firegrenade { get; set; } = 0.5f;
}

public class VIPConfig
{
    [JsonPropertyName("flag")]
    public string flag { get; set; } = "@css/vip";

    [JsonPropertyName("hegrenade")]
    public float hegrenade { get; set; } = 0.3f;

    [JsonPropertyName("flashbang")]
    public float flashbang { get; set; } = 0.3f;

    [JsonPropertyName("smoke")]
    public float smoke { get; set; } = 0.3f;

    [JsonPropertyName("decoy")]
    public float decoy { get; set; } = 0.3f;

    [JsonPropertyName("firegrenade")]
    public float firegrenade { get; set; } = 0.3f;
}

public class PlayerConfig
{
    [JsonPropertyName("hegrenade")]
    public float hegrenade { get; set; } = 0.01f;

    [JsonPropertyName("flashbang")]
    public float flashbang { get; set; } = 0.1f;

    [JsonPropertyName("smoke")]
    public float smoke { get; set; } = 0.05f;

    [JsonPropertyName("decoy")]
    public float decoy { get; set; } = 0.3f;

    [JsonPropertyName("firegrenade")]
    public float firegrenade { get; set; } = 0.01f;
}

public class RoundEndThings : BasePlugin, IPluginConfig<NadesAllocatorConfig>
{
    public override string ModuleName => "RoundEndThings";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "unfortunate";

    public NadesAllocatorConfig Config { get; set; } = new();

    public void OnConfigParsed(NadesAllocatorConfig config)
    {
        // Once we've validated the config, we can set it to the instance
        Config = config;
    }

    #region Events
    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!IsPlayerValid(player))
            return HookResult.Continue;

        if (AdminManager.PlayerHasPermissions(player, $"{Config.Admin.flag}"))
        {
            AllocateNades(
                player,
                Config.Admin.hegrenade,
                Config.Admin.flashbang,
                Config.Admin.smoke,
                Config.Admin.decoy,
                Config.Admin.firegrenade
            );
            return HookResult.Continue;
        }
        else if (AdminManager.PlayerHasPermissions(player, $"{Config.VIP.flag}"))
        {
            AllocateNades(
                player,
                Config.VIP.hegrenade,
                Config.VIP.flashbang,
                Config.VIP.smoke,
                Config.VIP.decoy,
                Config.VIP.firegrenade
            );
            return HookResult.Continue;
        }

        AllocateNades(
            player,
            Config.Player.hegrenade,
            Config.Player.flashbang,
            Config.Player.smoke,
            Config.Player.decoy,
            Config.Player.firegrenade
        );
        return HookResult.Continue;
    }
    #endregion

    #region Functions
    public void AllocateNades(
        CCSPlayerController player,
        float hegrenade,
        float flashbang,
        float smoke,
        float decoy,
        float firegrenade
    )
    {
        if (!IsPlayerValid(player))
            return;

        Random random = new Random();
        double randomNumber = random.Next(1, 101) / 100.0;
        if (randomNumber >= hegrenade)
        {
            if (!PlayerHasWeapon(player, "weapon_hegrenade"))
                player.GiveNamedItem("weapon_hegrenade");
        }

        randomNumber = random.Next(1, 101) / 100.0;
        if (randomNumber >= flashbang)
        {
            if (!PlayerHasWeapon(player, "weapon_flashbang"))
                player.GiveNamedItem("weapon_flashbang");
        }

        randomNumber = random.Next(1, 101) / 100.0;
        if (randomNumber >= smoke)
        {
            if (!PlayerHasWeapon(player, "weapon_smokegrenade"))
                player.GiveNamedItem("weapon_smokegrenade");
        }

        randomNumber = random.Next(1, 101) / 100.0;
        if (randomNumber >= decoy)
        {
            if (!PlayerHasWeapon(player, "weapon_decoy"))
                player.GiveNamedItem("weapon_decoy");
        }

        randomNumber = random.Next(1, 101) / 100.0;
        if (randomNumber >= firegrenade)
        {
            switch (player.TeamNum)
            {
                case 3:
                    if (!PlayerHasWeapon(player, "weapon_incgrenade"))
                        player.GiveNamedItem("weapon_incgrenade");
                    break;
                case 2:
                    if (!PlayerHasWeapon(player, "weapon_molotov"))
                        player.GiveNamedItem("weapon_molotov");
                    break;
            }
        }
    }
    #endregion

    #region Helpers

    public static bool IsPlayerValid(CCSPlayerController player)
    {
        return player != null
            && player.IsValid
            && !player.IsBot
            && player.Pawn != null
            && player.Pawn.IsValid
            && player.Connected == PlayerConnectedState.PlayerConnected
            && !player.IsHLTV;
    }

    public bool PlayerHasWeapon(CCSPlayerController player, string designerName)
    {
        if (!IsPlayerValid(player))
            return false;

        var weaponServices = player?.PlayerPawn?.Value?.WeaponServices;
        if (weaponServices == null)
        {
            return false;
        }

        foreach (var weaponHandle in weaponServices.MyWeapons)
        {
            var weapon = weaponHandle.Value;
            if (weapon != null && weapon.IsValid)
            {
                if (weapon.DesignerName == designerName)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}
