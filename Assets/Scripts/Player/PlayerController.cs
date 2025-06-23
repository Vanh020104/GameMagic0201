using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Transform _transform;
    public Animator _animator;
    public FixedJoystick fixedJoystick;
    public Rigidbody _rigidbody;
    public float speedPlayer = 10f;
    public float rotationSpeed = 10f;

    private Vector3 moveDirection;


    // Button attack
    public Button normal_attack_button, first_attack_button, second_attack_button, third_attack_button;


    // bullet
    public Transform _targetPosition;
    public GameObject prefabProjectile;
    public Transform projectileProsition;
    public Transform skill01Prosition;
    public Transform skill02Prosition;
    public PlayerInfo playerInfo;

    // time hồi chiêu normal
    [Header("Countdown Normal Attack")]
    private bool canUserNormalAttack = true;
    public float normalAttackCoundown = 2f;
    public Image cooldownOverlayNormalAttack;

    [Header("Countdown First Skill")]
    private bool canUseFirstSkill = true;
    public float firstSkillCooldown = 30f;
    public Image cooldownOverlayFirstSkill;
    public TMP_Text firstCooldownText;

    public LevelUI levelUI;

    [Header("Custom Skills")]
    public BaseSkill skill2;
    public BaseSkill skill3;

    // skill 2, 3 
    public Image cooldownOverlaySecondSkill;
    public TMP_Text secondCooldownText;
    public Image cooldownOverlayThirdSkill;
    public TMP_Text thirdCooldownText;

    private bool canUseSecondSkill = true;
    private bool canUseThirdSkill = true;

    void Start()
    {
        BattleLayout layout = UIManager.Instance.GetLayout();

        fixedJoystick = layout.joystick;
        normal_attack_button = layout.normalAttackBtn;
        first_attack_button = layout.firstSkillBtn;
        second_attack_button = layout.secondSkillBtn;
        third_attack_button = layout.thirdSkillBtn;
        cooldownOverlayNormalAttack = layout.normalCooldownOverlay;
        playerInfo._hpSlider = layout.healthPlayer;
        playerInfo._manaSlider = layout.manaPlayer;
        levelUI = layout.levelUI;
        playerInfo.hpText = layout.healthPlayer.GetComponentInChildren<TMPro.TMP_Text>();
        playerInfo.hpFillImage = layout.healthPlayer.fillRect.GetComponent<Image>();

        // hồi chiêu và lắng nghe của chiêu 1 
        cooldownOverlayFirstSkill = layout.firstCooldownOverlay;

        firstCooldownText = layout.firstCooldownText;
        firstCooldownText.gameObject.SetActive(false);

        cooldownOverlaySecondSkill = layout.secondCooldownOverlay;
        secondCooldownText = layout.secondCooldownText;
        secondCooldownText.gameObject.SetActive(false);

        cooldownOverlayThirdSkill = layout.thirdCooldownOverlay;
        thirdCooldownText = layout.thirdCooldownText;
        thirdCooldownText.gameObject.SetActive(false);
        first_attack_button.onClick.AddListener(UseFirstSkill);
        second_attack_button.onClick.AddListener(UseSecondSkill);
        third_attack_button.onClick.AddListener(UseThirdSkill);


        normal_attack_button.onClick.AddListener(NormalAttack);
    }

    void Update()
    {
        float moveX = fixedJoystick.Horizontal != 0 ? fixedJoystick.Horizontal : Input.GetAxis("Horizontal");
        float moveZ = fixedJoystick.Vertical != 0 ? fixedJoystick.Vertical : Input.GetAxis("Vertical");
        moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        _animator.SetBool("Move", moveDirection.magnitude > 0.1f);
        if (Input.GetKeyDown(KeyCode.C))
        {
            _animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("Jump", true);
        }
    }

    void FixedUpdate()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            _rigidbody.velocity = moveDirection * speedPlayer;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
        }
        if (!_rigidbody.isKinematic && !_rigidbody.IsSleeping())
        {
            _rigidbody.AddForce(Vector3.down * 250f, ForceMode.Acceleration); 
        }

    }

    /// <summary>
    /// Lisst cac chiêu của hero
    /// 
    /// </summary>



    /// Chiêu Normal bắn đạn 
    public void NormalAttack()
    {
        if (!canUserNormalAttack || playerInfo == null || playerInfo.hasDied || playerInfo._hp <= 0)
            return;

        _animator.SetTrigger("Attack");
        StartCoroutine(DelayAddBullet());
        StartCoroutine(NormalAttackCooldown());
    }

    // ham đợi để bắn chiêu cho trùng với aniamtion chémchém
    private IEnumerator DelayAddBullet()
    {
        yield return new WaitForSeconds(0.3f);
        AddBullet();
    }

    void AddBullet()
    {
        var newProjectile = Instantiate(prefabProjectile, projectileProsition);
        newProjectile.GetComponent<ProjectileMoveScript>().owner = playerInfo;
        // var moveScript = newProjectile.GetComponent<ProjectileMoveScript>();
        // moveScript.rotate = false;
        newProjectile.transform.forward = _transform.forward;
        newProjectile.transform.SetParent(null);
        newProjectile.transform.position = projectileProsition.position;
        if (AudioManager.Instance != null && AudioManager.Instance.sfxNormalAttack != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxNormalAttack);
        }
        Destroy(newProjectile, 2f);
        
    }

    private IEnumerator NormalAttackCooldown()
    {
        canUserNormalAttack = false;
        float elapsed = 0f;
        cooldownOverlayNormalAttack.fillAmount = 1;

        while (elapsed < normalAttackCoundown)
        {
            elapsed += Time.deltaTime;
            cooldownOverlayNormalAttack.fillAmount = 1 - (elapsed / normalAttackCoundown);

            float remainingTime = normalAttackCoundown - elapsed;
            yield return null;
        }

        cooldownOverlayNormalAttack.fillAmount = 0;
        canUserNormalAttack = true;
    }



    // end

    // Chiêu 1 + Hồi máu 
    public void UseFirstSkill()
    {
        if (!canUseFirstSkill || playerInfo._mana < 10) return;

        // Khóa ngay lập tức để ngăn spam
        canUseFirstSkill = false;

        playerInfo._mana -= 10;
        int totalHealAmount = 120;
         playerInfo._mana += 30;
        if (playerInfo._mana > playerInfo._manaMax)
        playerInfo._mana = playerInfo._manaMax;


        if (playerInfo.vfxHealPrefab != null && playerInfo.vfxAttachPoint != null)
        {
            GameObject vfx = Instantiate(playerInfo.vfxHealPrefab, playerInfo.vfxAttachPoint);
            vfx.transform.localPosition = Vector3.zero;
            Destroy(vfx, 3f);
        }
        StartCoroutine(playerInfo.HealOverTime(totalHealAmount));
        DailyTaskManager.Instance.TryAddProgress("use_skill_10");
        DailyTaskManager.Instance.TryAddProgress("use_heal_5");
        StartCoroutine(FirstSkillCooldown());
    }



    private IEnumerator FirstSkillCooldown()
    {
        float elapsed = 0f;
        cooldownOverlayFirstSkill.fillAmount = 1;
        firstCooldownText.gameObject.SetActive(true);

        while (elapsed < firstSkillCooldown)
        {
            elapsed += Time.deltaTime;
            cooldownOverlayFirstSkill.fillAmount = 1 - (elapsed / firstSkillCooldown);
            firstCooldownText.text = Mathf.CeilToInt(firstSkillCooldown - elapsed).ToString();
            yield return null;
        }

        cooldownOverlayFirstSkill.fillAmount = 0;
        firstCooldownText.gameObject.SetActive(false);
        canUseFirstSkill = true;
    }



    // end



    // chiêu 2,3
    void UseSkill(BaseSkill skill)
    {
        if (skill == null) return;
        skill.Activate(playerInfo, skill01Prosition);
    }

    // - Thêm trong PlayerController.cs:
    void UseSecondSkill()
    {
        if (!canUseSecondSkill || skill2 == null) return;

        // Khóa lại ngay lập tức
        canUseSecondSkill = false;

        // Gọi kỹ năng
        skill2.Activate(playerInfo, skill01Prosition);
        DailyTaskManager.Instance.TryAddProgress("use_skill_10");

        // Bắt đầu hồi chiêu
        StartCoroutine(SkillCooldown(
            cooldownOverlaySecondSkill,
            secondCooldownText,
            skill2.cooldown,
            () => canUseSecondSkill = true
        ));
    }

    void UseThirdSkill()
    {
        if (!canUseThirdSkill || skill3 == null) return;

        canUseThirdSkill = false;

        skill3.Activate(playerInfo, skill02Prosition);
        DailyTaskManager.Instance.TryAddProgress("use_skill_10");
        StartCoroutine(SkillCooldown(
            cooldownOverlayThirdSkill,
            thirdCooldownText,
            skill3.cooldown,
            () => canUseThirdSkill = true
        ));
    }


    IEnumerator SkillCooldown(Image overlay, TMP_Text text, float duration, System.Action setReady)
    {
        float elapsed = 0f;
        overlay.fillAmount = 1;
        text.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            overlay.fillAmount = 1 - (elapsed / duration);
            text.text = Mathf.CeilToInt(duration - elapsed).ToString();
            yield return null;
        }

        overlay.fillAmount = 0;
        text.gameObject.SetActive(false);
        setReady();
    }


}
