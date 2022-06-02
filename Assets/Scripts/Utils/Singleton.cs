using UnityEngine;

public class Singleton : MonoBehaviour {

    public static Singleton instance { get; private set; }

    public Transform Player { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public LootSpawner LootSpawner { get; private set; }
    public DamageInstanceSpawner DamageInstanceSpawner { get; private set; }
    public EnemyHPBars EnemyHPBars { get; private set; }
    public PlayerInventory PlayerInventory { get; private set; }
    public GameEvents GameEvents { get; private set; }
    public CameraChanger CameraChanger { get; private set; }
    public VFXManager VFXManager { get; private set; }
    //public SFXManager SFXManager { get; private set; }
    public PlayerHurt PlayerHurt { get; private set; }
    public PlayerAbilityUpgrader PlayerUpgradeHolder { get; private set; }

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LevelManager = GetComponentInChildren<LevelManager>();
        LootSpawner = GetComponentInChildren<LootSpawner>();
        DamageInstanceSpawner = GetComponentInChildren<DamageInstanceSpawner>();
        EnemyHPBars = GetComponentInChildren<EnemyHPBars>();
        PlayerInventory = GetComponentInChildren<PlayerInventory>();
        GameEvents = GetComponentInChildren<GameEvents>();
        CameraChanger = GetComponent<CameraChanger>();
        VFXManager = GetComponentInChildren<VFXManager>();
        //SFXManager = GetComponentInChildren<SFXManager>();
    }

    public void SetPlayerScripts() {
        Player = GameObject.Find("Player").transform;
        PlayerHurt = Player.GetComponent<PlayerHurt>();
        PlayerUpgradeHolder = Player.GetComponentInChildren<PlayerAbilityUpgrader>();
    }
}
