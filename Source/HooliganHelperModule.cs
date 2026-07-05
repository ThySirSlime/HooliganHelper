namespace Celeste.Mod.HooliganHelper;

public class HooliganHelperModule : EverestModule {
    public static HooliganHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(HooliganHelperModuleSettings);
    public static HooliganHelperModuleSettings Settings => (HooliganHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(HooliganHelperModuleSession);
    public static HooliganHelperModuleSession Session => (HooliganHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(HooliganHelperModuleSaveData);
    public static HooliganHelperModuleSaveData SaveData => (HooliganHelperModuleSaveData) Instance._SaveData;

    
    public HooliganHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(HooliganHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(HooliganHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        LifecycleMethods.OnLoad();
    }

    public override void Unload() {
        LifecycleMethods.OnUnload();
    }
}