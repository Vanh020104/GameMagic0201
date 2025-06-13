using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using TMPro;
using static NotificationPopupUI;

public class LuckyPanelController : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform cardSpawnParent;
    public Transform cardCenterPoint;
    public Transform cardPointsRoot;

    [Header("Buttons")]
    public Button buttonStart;
    public Button buttonReset;

    public GameObject fingerHint;

    private List<GameObject> cards = new();
    private List<RectTransform> targetPoints = new();

    private Vector2 fingerOriginalPos;

    private enum PanelState { PreviewPhase, WaitingSecondClick, PlayingPhase }
    private PanelState currentState = PanelState.PreviewPhase;

    [Header("Highlight Frame")]
    public GameObject highlightFramePrefab;
    private GameObject highlightInstance;

    [Header("Reward Data")]
    public LuckyData rewardDatabase;

    [Header("Result Popup")]
    public GameObject panelRewardResult;
    public Image rewardIconUI;
    public TMPro.TextMeshProUGUI rewardNameUI;
    public TMPro.TextMeshProUGUI rewardAmountUI;

    [Header("Start Hint")]
    public GameObject handHintStart;

    [Header("Key Display")]
    public TMP_Text keyAmountText;

    private int currentKeyCount = 0;

    void Start()
    {
        currentKeyCount = PlayerPrefs.GetInt("LuckyKey", 3);
        UpdateKeyUI();

        buttonStart.gameObject.SetActive(false);
        buttonReset.onClick.AddListener(ResetCards);

        for (int i = 0; i < cardPointsRoot.childCount; i++)
        {
            targetPoints.Add(cardPointsRoot.GetChild(i).GetComponent<RectTransform>());
        }

        SpawnCenterCard();
        ShowHint();

        buttonStart.onClick.AddListener(() =>
        {
            if (currentKeyCount <= 0) return;

            currentKeyCount--;
            PlayerPrefs.SetInt("LuckyKey", currentKeyCount);
            UpdateKeyUI();
            DailyTaskProgressManager.Instance.AddProgress("use_spin");

            if (handHintStart != null)
            {
                handHintStart.transform.DOKill();
                handHintStart.SetActive(false);
            }

            buttonStart.gameObject.SetActive(false);
            StartCoroutine(HighlightRandomCard());
        });
    }

    public void SpawnCenterCard()
    {
        ClearCards();

        var card = Instantiate(cardPrefab, cardSpawnParent);
        card.name = "Card_Tu";

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchoredPosition = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

        Button btn = card.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                btn.interactable = false;
                HideHint();

                if (currentState == PanelState.PreviewPhase)
                {
                    DealCards(() => StartCoroutine(PreviewSequence()));
                }
                else if (currentState == PanelState.WaitingSecondClick)
                {
                    DealCards(() =>
                    {
                        currentState = PanelState.PlayingPhase;
                        buttonStart.gameObject.SetActive(true);
                        UpdateKeyUI();

                        if (handHintStart != null)
                        {
                            handHintStart.SetActive(true);
                            RectTransform rt = handHintStart.GetComponent<RectTransform>();
                            rt.DOAnchorPos(rt.anchoredPosition + new Vector2(20f, 0), 0.5f)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetEase(Ease.InOutSine);
                        }
                    });
                }
            });
        }

        cards.Add(card);
    }

    public void DealCards(System.Action onComplete)
    {
        GameObject oldCard = cards.Count > 0 ? cards[0] : null;
        if (oldCard != null) Destroy(oldCard);

        cards.Clear();

        List<LuckyItemData> pickedRewards = LuckyRewardPicker.Pick(rewardDatabase.allRewards, 9);

        for (int i = 0; i < 9; i++)
        {
            var card = Instantiate(cardPrefab, cardSpawnParent);
            card.name = $"Card_{i}";

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = cardCenterPoint.GetComponent<RectTransform>().anchoredPosition;

            cards.Add(card);

            var cardScript = card.GetComponent<LuckyCardItem>();
            if (cardScript != null)
            {
                cardScript.SetReward(pickedRewards[i]);
                cardScript.FlipToBack();
            }

            var target = targetPoints[i].anchoredPosition;
            rt.DOAnchorPos(target, 0.5f)
              .SetEase(Ease.OutBack)
              .SetDelay(i * 0.05f);
        }

        StartCoroutine(InvokeAfterDelay(0.6f + 9 * 0.05f, onComplete));
    }

    private IEnumerator PreviewSequence()
    {
        foreach (var card in cards)
        {
            FlipCard(card, true);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(2.5f);

        foreach (var card in cards)
        {
            FlipCard(card, false);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);
        ResetCardsAfterPreview();
    }

    private void ResetCardsAfterPreview()
    {
        ClearCards();
        SpawnCenterCard();
        ShowHint();
        currentState = PanelState.WaitingSecondClick;
    }

    public void ResetCards()
    {
        ClearCards();
        SpawnCenterCard();
        ShowHint();
        currentState = PanelState.PreviewPhase;
        buttonStart.gameObject.SetActive(false);
        UpdateKeyUI();
         if (handHintStart != null)
        {
            handHintStart.transform.DOKill();
            handHintStart.SetActive(false);
        }
    }

    private void ClearCards()
    {
        foreach (var card in cards)
        {
            if (card != null)
            {
                RectTransform rt = card.GetComponent<RectTransform>();
                rt.DOKill();
                Destroy(card);
            }
        }
        cards.Clear();
    }

    private void FlipCard(GameObject cardGO, bool toFront)
    {
        var flip = cardGO.GetComponent<LuckyCardItem>();
        if (flip == null) return;

        if (toFront)
            flip.FlipToFront();
        else
            flip.FlipToBack();
    }

    private void ShowHint()
    {
        fingerHint.SetActive(true);

        RectTransform rt = fingerHint.GetComponent<RectTransform>();
        if (fingerOriginalPos == Vector2.zero)
            fingerOriginalPos = rt.anchoredPosition;

        rt.anchoredPosition = fingerOriginalPos;

        rt.DOAnchorPos(fingerOriginalPos + new Vector2(-20f, 20f), 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideHint()
    {
        fingerHint.SetActive(false);
        fingerHint.transform.DOKill();
    }

    private IEnumerator InvokeAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private IEnumerator HighlightRandomCard()
    {
        if (highlightInstance == null)
        {
            highlightInstance = Instantiate(highlightFramePrefab, cardSpawnParent);
            highlightInstance.SetActive(true);
        }

        RectTransform frameRect = highlightInstance.GetComponent<RectTransform>();
        int total = cards.Count;
        int currentIndex = 0;
        int rounds = Random.Range(20, 30);
        float baseDelay = 0.05f;

        for (int i = 0; i < rounds; i++)
        {
            GameObject currentCard = cards[currentIndex];
            frameRect.SetParent(currentCard.transform);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.SetAsLastSibling();

            currentIndex = (currentIndex + 1) % total;
            yield return new WaitForSeconds(baseDelay + i * 0.003f);
        }

        int selectedIndex = (currentIndex + total - 1) % total;
        GameObject selectedCard = cards[selectedIndex];
        FlipCard(selectedCard, true);
        var reward = selectedCard.GetComponent<LuckyCardItem>().GetReward();
        ShowRewardPopup(reward);
        Debug.Log($"🎁 Bạn đã nhận được: {reward.rewardName} x{reward.amount}");
    }

    // private void ShowRewardPopup(LuckyItemData reward)
    // {
    //     panelRewardResult.SetActive(true);

    //     rewardIconUI.sprite = reward.rewardIcon;
    //     rewardNameUI.text = reward.rewardName;
    //     rewardAmountUI.text = $"x{reward.amount}";
    // }
    private void ShowRewardPopup(LuckyItemData reward)
    {
        panelRewardResult.SetActive(true);
        rewardIconUI.sprite = reward.rewardIcon;
        rewardNameUI.text = reward.rewardName;
        rewardAmountUI.text = $"x{reward.amount}";

        switch (reward.rewardType)
        {
            case RewardType.Gold:
                GoldGemManager.Instance.AddGold(reward.amount);
                break;
            case RewardType.Gem:
                GoldGemManager.Instance.AddGem(reward.amount);
                break;
            case RewardType.Key:
                int key = PlayerPrefs.GetInt("LuckyKey", 0);
                key += reward.amount;
                PlayerPrefs.SetInt("LuckyKey", key);
                PlayerPrefs.Save();
                UpdateKeyUI(); 
                KeyEvent.InvokeKeyChanged();
                break;
                
            case RewardType.Item:
                PlayerPrefs.SetInt($"Equip_{reward.id}_Unlocked", 1);
                PlayerPrefs.Save();
                BagEvent.InvokeItemBought(); 
                break;
        }
    }


    public void HideRewardPopup()
    {
        panelRewardResult.SetActive(false);
        ResetCards();
    }

    private void UpdateKeyUI()
    {
        currentKeyCount = PlayerPrefs.GetInt("LuckyKey", 0);
        keyAmountText.text = currentKeyCount.ToString();
        buttonStart.interactable = currentKeyCount > 0;
    }
} 