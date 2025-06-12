using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public int _hp = 500;
    public int _mana = 100;
    public int _hpMax = 500;
    public int _manaMax = 100;
    public Slider _hpSlider;
    public Slider _manaSlider;
    public Animator _animator;

    public bool hasDied = false;
    public bool isLocalPlayer = false;
    public string playerName;
    private ReviveManager reviveManager;
    public bool isInvincible = false;
    public GameObject vfxHealPrefab;
    public Transform vfxAttachPoint;
    public LevelUI levelUI;
    public int baseDamage = 50;
    public TMPro.TMP_Text hpText;
    public Image hpFillImage;

    void Start()
    {
        StartCoroutine(RegenerateMana());
        reviveManager = FindObjectOfType<ReviveManager>();
    }

    void Update()
    {
        _hpSlider.value = _hp / (float)_hpMax;
        _manaSlider.value = _mana / (float)_manaMax;

        if (!hasDied && _hp <= 0)
        {
            hasDied = true;
            var controller = GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
            _animator.SetTrigger("Die");
            StartCoroutine(WaitThenShowRevivePanel());
        }
        if (hpText != null)
        {
            hpText.text = $"{_hp}/{_hpMax}";
        }
        // update thanh mau
        if (hpFillImage != null)
        {
            float ratio = _hp / (float)_hpMax;

            if (ratio > 0.6f)
                hpFillImage.color = Color.green;
            else
                hpFillImage.color = Color.red;
        }


    }
    private IEnumerator WaitThenShowRevivePanel()
    {
        // chờ 1.5s cho Die animation chạy
        yield return new WaitForSeconds(1f);

        reviveManager.TriggerRevive(this);
    }



    IEnumerator HandleDeath()
    {
        FindObjectOfType<KillInfoUIHandler>()?.PlayerDied();
        _animator.SetTrigger("Die");
        PlayerController controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }


    private IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (_mana < _manaMax)
            {
                _mana += Mathf.CeilToInt(3f);
                if (_mana > _manaMax)
                {
                    _mana = _manaMax;
                }
            }
        }
    }

    public void ReviveFromDeath()
    {
        if (!hasDied) return;

        _hp = _hpMax;
        _mana = _manaMax;
        EnablePhysics();

        _animator.SetTrigger("Revive");
        isInvincible = true;
        StartCoroutine(WaitForReviveAnimation());
    }

    private void EnablePhysics()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    private IEnumerator WaitForReviveAnimation()
    {
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Revive"))
            yield return null;
        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;
        yield return new WaitForSeconds(1.5f);

        isInvincible = false;
        hasDied = false;
    }


    public void FinishDeath()
    {
        int currentAlive = FindObjectOfType<KillInfoUIHandler>().GetAliveCount();
        GameResultData.playerRank = currentAlive + 1;
        FindObjectOfType<BattleEndManager>().isWin = false;
        FindObjectOfType<BattleEndManager>().EndMatch();
        StartCoroutine(HandleDeath());
    }

    public IEnumerator HealOverTime(int totalAmount)
    {
        int healed = 0;
        while (healed < totalAmount)
        {
            // Random giá trị mỗi lần cộng
            int healThisTick = Random.Range(5, Mathf.Min(20, totalAmount - healed + 1));

            _hp = Mathf.Min(_hp + healThisTick, _hpMax);
            healed += healThisTick;
            yield return new WaitForSeconds(0.1f);
        }
    }
    



}