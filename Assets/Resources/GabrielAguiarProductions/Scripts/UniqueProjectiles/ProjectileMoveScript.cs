//
//
//NOTES:
//
//This script is used for DEMONSTRATION porpuses of the Projectiles. I recommend everyone to create their own code for their own projects.
//THIS IS JUST A BASIC EXAMPLE PUT TOGETHER TO DEMONSTRATE VFX ASSETS.
//
//




#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{

    public bool rotate = false;
    public float rotateAmount = 45;
    public bool bounce = false;
    public float bounceForce = 10;
    public float speed;
    [Tooltip("From 0% to 100%")]
    public float accuracy;
    public float fireRate;
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public List<GameObject> trails;

    private Vector3 startPos;
    private float speedRandomness;
    private Vector3 offset;
    private bool collided;
    private Rigidbody rb;
    private RotateToMouseScript rotateToMouse;
    private GameObject target;

    // sua de ban danj
    public PlayerInfo owner;
    public BotStats ownerBot;
    private bool collided22 = false;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        //used to create a radius for the accuracy and have a very unique randomness
        if (accuracy != 100)
        {
            accuracy = 1 - (accuracy / 100);

            for (int i = 0; i < 2; i++)
            {
                var val = 1 * Random.Range(-accuracy, accuracy);
                var index = Random.Range(0, 2);
                if (i == 0)
                {
                    if (index == 0)
                        offset = new Vector3(0, -val, 0);
                    else
                        offset = new Vector3(0, val, 0);
                }
                else
                {
                    if (index == 0)
                        offset = new Vector3(0, offset.y, -val);
                    else
                        offset = new Vector3(0, offset.y, val);
                }
            }
        }

        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward + offset;
            var ps = muzzleVFX.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(muzzleVFX, ps.main.duration);
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }
    }

    void FixedUpdate()
    {
        if (target != null)
            rotateToMouse.RotateToMouse(gameObject, target.transform.position);
        if (rotate)
            transform.Rotate(0, 0, rotateAmount, Space.Self);
        if (speed != 0 && rb != null)
            rb.position += (transform.forward + offset) * (speed * Time.deltaTime);
    }
    private void HandleKill(PlayerInfo owner, BotStats ownerBot, string victimName)
    {
        string killerName = "Unknown";
        if (ownerBot != null)
            killerName = ownerBot.botName;
        else if (owner != null)
        {
            killerName = PlayerPrefs.GetString("PlayerName", "Player");
            var playerController = owner.GetComponent<PlayerController>();
            if (playerController != null && playerController.levelUI != null)
            {
                playerController.levelUI.AddExp(30);
            }
        }

        if (owner != null && owner.isLocalPlayer)
        {
            FindObjectOfType<KillInfoUIHandler>()?.AddKill();

            // ✅ Ghi nhận nhiệm vụ giết địch
            DailyTaskBridge.Instance?.TryAddProgress("kill_10_enemies");
            DailyTaskBridge.Instance?.TryAddProgress("kill_20_enemies");
        }



        FindObjectOfType<KillFeedUI>()?.ShowKill(killerName, victimName);
    }
    void OnCollisionEnter(Collision co)
    {
        if (collided) return;
        collided = true;

        var heroInfo = co.GetContact(0).otherCollider.GetComponentInParent<PlayerInfo>();
        var botInfo = co.GetContact(0).otherCollider.GetComponentInParent<BotStats>();

        if ((heroInfo != null && heroInfo == owner) || (botInfo != null && botInfo == ownerBot))
        {
            return;
        }

        // === DAMAGE HANDLING ===
        if (botInfo != null && botInfo != ownerBot)
        {
            int damage = owner != null ? owner.baseDamage : 50;
            botInfo.currentHP = Mathf.Max(0, botInfo.currentHP - damage);

            if (owner != null && owner.isLocalPlayer)
            {
                DailyTaskBridge.Instance?.TryAddProgress("deal_500_damage", damage);
                DailyTaskBridge.Instance?.TryAddProgress("deal_1000_damage", damage);
            }

            if (botInfo.floatingTextPrefab && botInfo.popupPoint)
            {
                var popup = Instantiate(botInfo.floatingTextPrefab, botInfo.popupPoint.position, Quaternion.identity);
                var text = popup.GetComponent<TMPro.TextMeshPro>();
                if (text != null)
                {
                    text.text = damage.ToString();
                    text.color = Color.red;
                }
            }

            if (botInfo.currentHP <= 0 && !botInfo.isDead)
            {
                botInfo.currentHP = 0;
                botInfo.isDead = true;
                HandleKill(owner, ownerBot, botInfo.botName);
                // Spawn Heal nếu đủ điều kiện
                if (botInfo.healHeartPrefab != null && Random.value < 0.7f && GameManagerUI.currentHealItemCount < GameManagerUI.maxHealItems)
                {
                    Instantiate(botInfo.healHeartPrefab, botInfo.transform.position + Vector3.up * 4f, Quaternion.identity);
                    GameManagerUI.currentHealItemCount++;
                }


            }
        }
        else if (heroInfo != null && heroInfo != owner)
        {
            if (heroInfo.isInvincible) return;

            int damage = owner != null ? owner.baseDamage : (ownerBot != null ? ownerBot.baseDamage : 50);
            heroInfo._hp = Mathf.Max(0, heroInfo._hp - damage);

            if (heroInfo._hp <= 0 && !heroInfo.isDead)
            {
                heroInfo.isDead = true;
                HandleKill(owner, ownerBot, heroInfo.playerName);
            }
        }

        // === EFFECTS / TRAILS / DESTROY ===
        if (!bounce)
        {
            if (trails.Count > 0)
            {
                foreach (var trail in trails)
                {
                    trail.transform.parent = null;
                    var ps = trail.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                }
            }

            speed = 0;
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false; // 🛡 Ngăn collision thêm lần nào nữa
            }

            ContactPoint contact = co.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            if (hitPrefab != null)
            {
                var hitVFX = Instantiate(hitPrefab, pos, rot);
                var ps = hitVFX.GetComponent<ParticleSystem>();
                if (ps != null)
                    Destroy(hitVFX, ps.main.duration);
                else
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX, psChild.main.duration);
                }
            }

            StartCoroutine(DestroyParticle(0f));
        }
        else
        {
            if (rb != null)
            {
                rb.useGravity = true;
                rb.drag = 0.5f;
                ContactPoint contact = co.contacts[0];
                rb.AddForce(Vector3.Reflect((contact.point - startPos).normalized, contact.normal) * bounceForce, ForceMode.Impulse);
            }
            Destroy(this);
        }
    }


    public IEnumerator DestroyParticle(float waitTime)
    {

        if (transform.childCount > 0 && waitTime != 0)
        {
            List<Transform> tList = new List<Transform>();

            foreach (Transform t in transform.GetChild(0).transform)
            {
                tList.Add(t);
            }

            while (transform.GetChild(0).localScale.x > 0)
            {
                yield return new WaitForSeconds(0.01f);
                transform.GetChild(0).localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                for (int i = 0; i < tList.Count; i++)
                {
                    tList[i].localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        }

        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }

    public void SetTarget(GameObject trg, RotateToMouseScript rotateTo)
    {
        target = trg;
        rotateToMouse = rotateTo;
    }
}
