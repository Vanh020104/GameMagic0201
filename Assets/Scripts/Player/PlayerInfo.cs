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

    private bool hasDied = false;

    void Update()
    {
        _hpSlider.value = _hp / (float)_hpMax;
        _manaSlider.value = _mana / (float)_manaMax;

        if (!hasDied && _hp <= 0)
        {
            hasDied = true;
            StartCoroutine(HandleDeath());
        }
    }

    IEnumerator HandleDeath()
    {
        // Chạy animation die
        _animator.SetTrigger("Die");

        // Tắt điều khiển (nếu cần)
        PlayerController controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;

        // Dừng Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.velocity = Vector3.zero;

        // Optional: tắt collider nếu ko muốn bị va chạm
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Chờ 3s cho animation xong
        yield return new WaitForSeconds(3f);

        // Xoá toàn bộ player
        Destroy(gameObject);
    }
}