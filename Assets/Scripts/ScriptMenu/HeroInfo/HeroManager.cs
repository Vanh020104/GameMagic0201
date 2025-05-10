using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance;
    public List<HeroData> allHeroes;

    private void Awake()
    {
        Instance = this;
    }

    public HeroData GetHeroById(string id)
    {
        return allHeroes.Find(h => h.heroId == id);
    }
}