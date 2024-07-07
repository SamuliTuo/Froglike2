using UnityEngine;

public class Singleton : MonoBehaviour {

    public static Singleton instance { get; private set; }

    public GameState GameState { get; private set; }
    public Transform Player { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public LootSpawner LootSpawner { get; private set; }
    public DamageInstanceSpawner DamageInstanceSpawner { get; private set; }
    public EnemyHPBars EnemyHPBars { get; private set; }
    public PlayerInventory PlayerInventory { get; private set; }
    public PlayerAttackSpawner PlayerAttackSpawner { get; private set; }
    public GameEvents GameEvents { get; private set; }
    public CameraChanger CameraChanger { get; private set; }
    public VFXManager VFXManager { get; private set; }
    public ParticleEffects ParticleEffects { get; private set; }
    //public SFXManager SFXManager { get; private set; }

    public PlayerHurt PlayerHurt { get; private set; }
    public PlayerMana PlayerMana { get; private set; }
    public PlayerStamina PlayerStamina { get; private set; }
    public PlayerAbilityUpgrader PlayerUpgradeHolder { get; private set; }
    public AimingArrows AimingArrows { get; private set; }

    void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        SetPlayerScripts();
        GameState = GetComponentInChildren<GameState>();
        LevelManager = GetComponentInChildren<LevelManager>();
        LootSpawner = GetComponentInChildren<LootSpawner>();
        DamageInstanceSpawner = GetComponentInChildren<DamageInstanceSpawner>();
        EnemyHPBars = GetComponentInChildren<EnemyHPBars>();
        PlayerInventory = GetComponentInChildren<PlayerInventory>();
        PlayerAttackSpawner = GetComponentInChildren<PlayerAttackSpawner>();
        GameEvents = GetComponentInChildren<GameEvents>();
        CameraChanger = GetComponent<CameraChanger>();
        VFXManager = GetComponentInChildren<VFXManager>();
        ParticleEffects = GetComponentInChildren<ParticleEffects>();
        AimingArrows = GetComponentInChildren<AimingArrows>();
        //SFXManager = GetComponentInChildren<SFXManager>();
    }
    public void SetPlayerScripts() {
        Player = GameObject.Find("Player").transform;
        PlayerHurt = Player.GetComponent<PlayerHurt>();
        PlayerMana = Player.GetComponent<PlayerMana>();
        PlayerStamina = Player.GetComponent<PlayerStamina>();
        PlayerUpgradeHolder = Player.GetComponentInChildren<PlayerAbilityUpgrader>();
    }
    public void RebootSingleton()
    {
        SetPlayerScripts();
        DamageInstanceSpawner.ClearPools();
        EnemyHPBars.ClearPools();
        LootSpawner.ClearPools();
        PlayerAttackSpawner.ClearPool();
        VFXManager.ClearPool();
    }
}
