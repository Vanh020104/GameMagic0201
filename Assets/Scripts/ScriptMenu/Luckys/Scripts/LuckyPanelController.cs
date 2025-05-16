// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using DG.Tweening;

// public class LuckyPanelController : MonoBehaviour
// {
//     [Header("Card Setup")]
//     public GameObject cardPrefab;                // Prefab lá bài
//     public Transform cardSpawnParent;            // Nơi chứa các lá bài sinh ra
//     public Transform cardCenterPoint;            // Vị trí tụ bài (chính giữa)
//     public Transform cardPointsRoot;             // Gốc chứa 9 điểm target (Point_0_0 → Point_2_2)

//     [Header("Buttons")]
//     public Button buttonStart;                   // Nút bắt đầu chia bài
//     public Button buttonReset;                   // Nút gom bài về lại tụ

//     private List<GameObject> cards = new();      // Danh sách 9 lá bài
//     private List<RectTransform> targetPoints = new(); // Danh sách các điểm đích (tọa độ chia bài)
//     public GameObject fingerHint;
//     /// <summary>
//     /// Hàm khởi tạo khi mở panel – tạo bài, gán listener cho nút
//     /// </summary>
//     void Start()
//     {
//         SpawnAllCards();

//         buttonStart.onClick.AddListener(() =>
//         {
//             DealCards();
//             HideHint();
//         });

//         buttonReset.onClick.AddListener(ResetCards);

//         for (int i = 0; i < cardPointsRoot.childCount; i++)
//         {
//             targetPoints.Add(cardPointsRoot.GetChild(i).GetComponent<RectTransform>());
//         }

//         ShowHint();
//     }


//     /// <summary>
//     /// Sinh 9 lá bài tại vị trí tụ bài chính giữa
//     /// </summary>
//     public void SpawnAllCards()
//     {
//         cards.Clear();

//         for (int i = 0; i < 9; i++)
//         {
//             var card = Instantiate(cardPrefab, cardSpawnParent);
//             card.name = $"Card_{i}";

//             RectTransform rt = card.GetComponent<RectTransform>();
//             rt.anchoredPosition = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

//             cards.Add(card);
//         }
//     }

//     /// <summary>
//     /// Chia 9 lá bài ra 9 vị trí Point_0_0 → Point_2_2 theo dạng 3x3
//     /// </summary>
//     public void DealCards()
//     {
//         for (int i = 0; i < cards.Count; i++)
//         {
//             var card = cards[i].GetComponent<RectTransform>();
//             var target = targetPoints[i].anchoredPosition;

//             card.DOAnchorPos(target, 0.5f)
//                 .SetEase(Ease.OutBack)
//                 .SetDelay(i * 0.05f);
//         }
//     }

//     /// <summary>
//     /// Gom tất cả bài trở lại vị trí tụ giữa màn hình
//     /// </summary>
//     public void ResetCards()
//     {
//         var center = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

//         foreach (var cardGO in cards)
//         {
//             var rt = cardGO.GetComponent<RectTransform>();
//             rt.DOAnchorPos(center, 0.4f).SetEase(Ease.InBack);
//         }
//     }

//     // gọi ngón tay chỉ
//     private void ShowHint()
//     {
//         fingerHint.SetActive(true);

//         RectTransform rt = fingerHint.GetComponent<RectTransform>();
//         Vector2 original = rt.anchoredPosition;

//         rt.DOAnchorPos(original + new Vector2(-20f, 20f), 0.5f)
//         .SetLoops(-1, LoopType.Yoyo)
//         .SetEase(Ease.InOutSine);
//     }


//     private void HideHint()
//     {
//         fingerHint.SetActive(false);
//         fingerHint.transform.DOKill(); // stop animation
//     }

// }
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class LuckyPanelController : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;                // Prefab lá bài
    public Transform cardSpawnParent;            // Nơi chứa các lá bài sinh ra
    public Transform cardCenterPoint;            // Vị trí tụ bài (chính giữa)
    public Transform cardPointsRoot;             // Gốc chứa 9 điểm target (Point_0_0 → Point_2_2)

    [Header("Buttons")]
    public Button buttonStart;                   // Nút dùng để random viền sáng sau này
    public Button buttonReset;                   // Nút gom bài về lại tụ

    public GameObject fingerHint;                // Icon chỉ tay

    private List<GameObject> cards = new();      // Danh sách các lá bài
    private List<RectTransform> targetPoints = new(); // Danh sách các điểm đích (tọa độ chia bài)

    private bool hasDealt = false;
    private Vector2 fingerOriginalPos; 

    /// <summary>
    /// Hàm khởi tạo khi mở panel – tạo tụ bài, gán listener cho nút
    /// </summary>
    void Start()
    {
        SpawnCenterCard();

        buttonReset.onClick.AddListener(ResetCards);

        // Gán sẵn target points
        for (int i = 0; i < cardPointsRoot.childCount; i++)
        {
            targetPoints.Add(cardPointsRoot.GetChild(i).GetComponent<RectTransform>());
        }

        ShowHint();
    }

    /// <summary>
    /// Sinh 1 lá tụ bài duy nhất ở giữa, cho phép click để chia bài
    /// </summary>
    public void SpawnCenterCard()
    {
        cards.Clear();

        var card = Instantiate(cardPrefab, cardSpawnParent);
        card.name = $"Card_Tu";

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchoredPosition = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

        // Lấy button có sẵn trong prefab
        Button btn = card.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                btn.interactable = false;    // Không cho click lại
                DealCards(card);             // Bắt đầu chia
                hasDealt = true;
                HideHint();                  // Ẩn icon chỉ tay
            });
        }

        cards.Add(card);
    }

    /// <summary>
    /// Chia bài ra 9 lá bài tại vị trí 3x3, từ vị trí trung tâm
    /// </summary>
    public void DealCards(GameObject oldCard)
    {
        Destroy(oldCard); // Xoá lá tụ cũ

        cards.Clear();

        for (int i = 0; i < 9; i++)
        {
            var card = Instantiate(cardPrefab, cardSpawnParent);
            card.name = $"Card_{i}";

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

            cards.Add(card);

            var target = targetPoints[i].anchoredPosition;
            rt.DOAnchorPos(target, 0.5f)
              .SetEase(Ease.OutBack)
              .SetDelay(i * 0.05f);
        }
    }

    /// <summary>
    /// Reset toàn bộ bài về trạng thái ban đầu – ẩn hết và tạo lại 1 tụ bài
    /// </summary>
    public void ResetCards()
    {
        // Xoá toàn bộ lá bài hiện tại
        foreach (var card in cards)
        {
            if (card != null)
            {
                // Kill tween trước khi destroy
                RectTransform rt = card.GetComponent<RectTransform>();
                rt.DOKill(); // Dừng tất cả tween liên quan object này

                Destroy(card);
            }
        }

        cards.Clear();

        // Tạo lại lá tụ duy nhất
        SpawnCenterCard();

        // Hiện icon chỉ tay lại
        ShowHint();

        hasDealt = false;
    }



    /// <summary>
    /// Hiện icon ngón tay và lắc nhẹ để dụ nhấn vào tụ
    /// </summary>
    private void ShowHint()
    {
        fingerHint.SetActive(true);

        RectTransform rt = fingerHint.GetComponent<RectTransform>();

        // 🔒 Gán vị trí gốc 1 lần duy nhất
        if (fingerOriginalPos == Vector2.zero)
            fingerOriginalPos = rt.anchoredPosition;

        rt.anchoredPosition = fingerOriginalPos;

        rt.DOAnchorPos(fingerOriginalPos + new Vector2(-20f, 20f), 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Ẩn icon chỉ tay
    /// </summary>
    private void HideHint()
    {
        fingerHint.SetActive(false);
        fingerHint.transform.DOKill(); // stop animation
    }
}
