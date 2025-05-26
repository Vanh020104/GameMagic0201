using System.Collections;
using System.Collections.Generic;
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
    public PlayerInfo playerInfo;

    // time hồi chiêu normal
    [Header("Countdown Normal Attack")]
    private bool canUserNormalAttack = true;
    public float normalAttackCoundown = 2f;
    public Image cooldownOverlayNormalAttack;
    public LevelUI levelUI;

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
    }

    public void NormalAttack()
    {
        if (!canUserNormalAttack) return;

        _animator.SetTrigger("Attack");
        StartCoroutine(DelayAddBullet());
        // hồi chiêuchiêu
        StartCoroutine(NormalAttackCooldown());
    }
    // ham đợi để bắn chiêu cho trùng với aniamtion chémchém
    private IEnumerator DelayAddBullet(){
        yield return new WaitForSeconds(0.3f);
        AddBullet();
    }

    void AddBullet(){
        var newProjectile = Instantiate(prefabProjectile, projectileProsition);
        newProjectile.GetComponent<ProjectileMoveScript>().owner = playerInfo;
        // var moveScript = newProjectile.GetComponent<ProjectileMoveScript>();
        // moveScript.rotate = false;
        newProjectile.transform.forward = _transform.forward;
        newProjectile.transform.SetParent(null);
        newProjectile.transform.position = projectileProsition.position;
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
}
