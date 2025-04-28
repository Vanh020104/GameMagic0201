using System.Collections;
using System.Collections.Generic;
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
}
