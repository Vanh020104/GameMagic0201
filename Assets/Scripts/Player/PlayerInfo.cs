using System.Collections;
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


    void Start()
    {
        StartCoroutine(RegenerateMana());
    }
    void Update()
    {
        _hpSlider.value = _hp / (float)_hpMax;
        _manaSlider.value = _mana / (float)_manaMax;

        if (!hasDied && _hp <= 0)
        {
            hasDied = true;
            int currentAlive = FindObjectOfType<KillInfoUIHandler>().GetAliveCount();
            GameResultData.playerRank = currentAlive + 1;
            FindObjectOfType<BattleEndManager>().isWin = false;
            FindObjectOfType<BattleEndManager>().EndMatch();
            StartCoroutine(HandleDeath());
        }
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
}