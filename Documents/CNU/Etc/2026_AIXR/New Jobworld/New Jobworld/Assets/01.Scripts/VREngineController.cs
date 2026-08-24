using System;
using UnityEngine;
using DG.Tweening;
using VInspector;

public class VREngineController : MonoBehaviour, IDamageable
{
    public static VREngineController Instance;

    public Shark shark;
    public Jennie jennie;

    public Transform jennieSpawnAnchor;
    public Transform sharkSpawnAnchor;

    public GameObject VREngineStartObj;

    [Header("VR Engine")]
    public Transform damangedEngine;
    public GameObject fixedEngine;
    public GameObject electricEffect;
    public Animator VRPortalOpenAnimator;
    public float duration;
    public Vector3 targetScale;
    Vector3 originalScale;

    // Phase 2 - Weapon Swap
    [Header("Phase 2 - Weapon Swap")]
    public GameObject currentWeapon;    // 기존에 들고 있던 무기
    public GameObject newWeaponPF;      // 새로 바꿀 무기 프리팹

    private Tween tween;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalScale = damangedEngine.transform.localScale;
        EventBus<FloorSpawnedEvent>.OnEvent += Reposition;
        EventBus<ResetWorldEvent>.OnEvent += HandleReset;

        MuteAllSounds();

#if UNITY_EDITOR
        // 에디터 테스트용: 몬스터 및 기본 오브젝트만 활성화 (StartPhase2 강제 호출 제거)
        Invoke(nameof(ForceStartGameForEditor), 1f);
#endif
    }

#if UNITY_EDITOR
    private void ForceStartGameForEditor()
    {
        Debug.Log("PC 에디터 모드: 기본 몬스터 및 엔진 오브젝트 활성화");

        if (shark != null) shark.gameObject.SetActive(true);
        if (jennie != null) jennie.gameObject.SetActive(true);
    }
#endif

    private void OnDestroy()
    {
        EventBus<FloorSpawnedEvent>.OnEvent -= Reposition;
        EventBus<ResetWorldEvent>.OnEvent -= HandleReset;
    }

    // 게임 전체 사운드 차단
    public void MuteAllSounds()
    {
        AudioListener.pause = true;
        AudioListener.volume = 0f;
        Debug.Log("게임 시작: 전체 사운드 완전 차단 (Phase 1 진행 중)");
    }

    // Phase 2 종료 시 사운드 원복
    [Button("On Phase 2 Ended (Test)")]
    public void OnPhase2Ended()
    {
        AudioListener.volume = 1f;
        AudioListener.pause = false;
        Debug.Log("Phase 2 종료: 전체 사운드 재생 재개");
    }

    private void Reposition(FloorSpawnedEvent e)
    {
        Renderer render = SpawnedFloor.Instance.transform.GetComponent<Renderer>();
        Vector3 center = render.bounds.center;
        float y = render.bounds.max.y;

        transform.position = new Vector3(0, y, 0);

        if (shark != null && sharkSpawnAnchor != null)
            shark.transform.position = sharkSpawnAnchor.position;

        if (jennie != null && jennieSpawnAnchor != null)
            jennie.transform.position = jennieSpawnAnchor.position;
    }

    // EngineStart() 호출 시 Phase 1이 완료된 것으로 판단하여 자동으로 Phase 2 시작
    public void EngineStart()
    {
        if (VREngineStartObj != null)
            VREngineStartObj.SetActive(true);

        Debug.Log("Phase 1 완료: VR 엔진 가동 및 Phase 2 자동 전환");

        // Phase 1 종료 후 Phase 2 자동 실행
        StartPhase2();
    }

    [Button("Start Phase 2 (Test)")]
    public void StartPhase2()
    {
        Debug.Log("StartPhase2 실행");

        if (newWeaponPF == null)
        {
            Debug.LogError("Error: 'New Weapon PF'에 바꿀 무기 프리팹을 넣어주세요!");
            return;
        }

        if (currentWeapon == null)
        {
            Debug.LogWarning("currentWeapon이 비어있어서 씬에서 이름으로 찾아봅니다.");
            currentWeapon = GameObject.Find("Gun") ?? GameObject.Find("Weapon");
        }

        Transform handTransform = (currentWeapon != null) ? currentWeapon.transform.parent : transform;
        Vector3 localPos = (currentWeapon != null) ? currentWeapon.transform.localPosition : Vector3.zero;
        Quaternion localRot = (currentWeapon != null) ? currentWeapon.transform.localRotation : Quaternion.identity;

#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == currentWeapon)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif

        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(newWeaponPF, handTransform);
        newWeapon.transform.localPosition = localPos;
        newWeapon.transform.localRotation = localRot;

        currentWeapon = newWeapon;

        Debug.Log("Phase 2 시작: 무기 교체 완료");
    }

    public void TakeDamage()
    {
    }

    [Button("Engine fixed")]
    public void OnEngineFixed()
    {
        if (damangedEngine != null) damangedEngine.gameObject.SetActive(false);
        if (electricEffect != null) electricEffect.SetActive(false);
        if (fixedEngine != null) fixedEngine.SetActive(true);
    }

    public void OnSpinningTweenComplete() =>
        tween = fixedEngine.transform.DOLocalRotate(new Vector3(0, 360f, 0), 12f).SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Restart);

    private void HandleReset(ResetWorldEvent e)
    {
        if (tween != null && tween.IsActive())
        {
            tween.Kill();
        }

        if (damangedEngine != null) damangedEngine.gameObject.SetActive(true);
        if (electricEffect != null) electricEffect.SetActive(true);
        if (fixedEngine != null) fixedEngine.SetActive(false);
        if (VREngineStartObj != null) VREngineStartObj.SetActive(false);
        if (VRPortalOpenAnimator != null) VRPortalOpenAnimator.Rebind();

        MuteAllSounds();
    }
}