using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotInfomation : MonoBehaviour
{
    public float hp = 200;
    public float mana = 80;
    public float maxHp = 200;
    public float maxMana = 80;
    // public Slider hpSlider;
    // public Slider manaSlider;
    public Animator anim;
    public bool hasdie = false;

    // Update is called once per frame
    void Update()
    {
        // hpSlider.value = hp / maxHp;
        // manaSlider.value = mana / maxMana;

        if (!hasdie && hp <= 0)
        {
            hasdie = true;
            // hpSlider.gameObject.SetActive(false);
            // manaSlider.gameObject.SetActive(false);
            StartCoroutine(Die());
            Destroy(gameObject, 3f);
        }
    }


    private IEnumerator Die(){
        anim.SetBool("Die", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("Die", false);
    }
}
