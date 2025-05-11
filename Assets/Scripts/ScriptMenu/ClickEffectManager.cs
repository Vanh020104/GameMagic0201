using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEffectManager : MonoBehaviour
{
    public GameObject clickEffectPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 screenPos = Input.mousePosition;
            screenPos.z = 5f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            GameObject fx = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);

            ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);

                // 🧠 Huỷ sau đúng thời gian thực chạy
                float totalDuration = ps.main.duration + ps.main.startLifetime.constant;
                Destroy(fx, totalDuration);
            }
        }
    }
}
