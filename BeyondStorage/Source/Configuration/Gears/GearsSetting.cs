using System;
using System.Collections.Generic;
using BeyondStorage.Infrastructure;
using GearsAPI.Settings.Global;

namespace BeyondStorage.Configuration.Gears;

internal interface IGearsSetting
{
    void Bind(IGlobalModSettingsCategory category);
}

/// <summary>
/// Bridges a GearsAPI global setting (<typeparamref name="TGears"/>, the type the setting stores) to a
/// <see cref="ModConfigData"/> field (<typeparamref name="TConfig"/>). The two differ only for the On/Off
/// switches, which Gears holds as strings so the menu can label the buttons.
/// </summary>
internal sealed class GearsSetting<TConfig, TGears> : IGearsSetting
{
    private readonly string _key;
    private readonly Func<ModConfigData, TConfig> _getConfig;
    private readonly Action<ModConfigData, TConfig> _setConfig;
    private readonly Func<TGears, TConfig> _fromGears;
    private readonly Func<TConfig, TGears> _toGears;

    public GearsSetting(string key, Func<ModConfigData, TConfig> getConfig, Action<ModConfigData, TConfig> setConfig, Func<TGears, TConfig> fromGears, Func<TConfig, TGears> toGears)
    {
        _key = key;
        _getConfig = getConfig;
        _setConfig = setConfig;
        _fromGears = fromGears;
        _toGears = toGears;
    }

    public void Bind(IGlobalModSettingsCategory category)
    {
        if (category.GetSetting<IGlobalValueSetting<TGears>>(_key) is not { } setting)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting `{_key}` is not a `{typeof(TGears).Name}` value setting");
            return;
        }

        setting.OnValueChanged += (_, newValue) =>
        {
            var value = _fromGears(newValue);

            if (!EqualityComparer<TConfig>.Default.Equals(_getConfig(ModConfig.ClientConfig), value))
            {
                _setConfig(ModConfig.ClientConfig, value);
                ModConfig.SaveConfig();
            }
        };

        Sync(setting);
    }

    // Gears restores the player's saved values silently, so OnValueChanged does not fire for them.
    private void Sync(IGlobalValueSetting<TGears> setting)
    {
        var gearsValue = _toGears(_getConfig(ModConfig.ClientConfig));

        if (EqualityComparer<TGears>.Default.Equals(setting.SettingValue, gearsValue))
        {
            return;
        }

        setting.SettingValue = gearsValue;
        setting.SelectedValue = gearsValue;
        setting.RefreshUI();
        GearsModAPI.SaveGlobalSettings();
    }
}