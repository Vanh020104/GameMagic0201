using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleLayout : MonoBehaviour
{
    public FixedJoystick joystick;
    public Button normalAttackBtn;
    public Button firstSkillBtn;
    public Button secondSkillBtn;
    public Button thirdSkillBtn;
    public Image normalCooldownOverlay;

    public Slider healthPlayer;
    public Slider manaPlayer;
    public LevelUI levelUI;
    public TMP_Text playerNameText;

    [Header("Chiêu Hồi máu")]
    public TMP_Text firstCooldownText;
    public Image firstCooldownOverlay;

    [Header("Chiêu 2")]
    public TMP_Text secondCooldownText;
    public Image secondCooldownOverlay;

    [Header("Chiêu 3")]
    public TMP_Text thirdCooldownText;
    public Image thirdCooldownOverlay;

}
