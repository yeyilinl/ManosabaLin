using ManosabaLin.Characters.Hiro.Cards;
using MegaCrit.Sts2.Core.Random;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 13 张先古原罪诅咒的创建目录。新卡需要随机获得/展示原罪诅咒时复用。
/// 注意：默认不挂载 <see cref="Originalsin"/> 组件——只有亚里沙的卡在获得原罪诅咒时
/// 主动挂载组件；其他人正常获得该诅咒卡不带组件（纯诅咒）。
/// </summary>
internal static class AncientSinCardCatalog
{
    private static readonly Func<ICombatState, Player, CardModel>[] All =
    [
        (cs, p) => cs.CreateCard<Hiroparanoid>(p),
        (cs, p) => cs.CreateCard<MargeCharm>(p),
        (cs, p) => cs.CreateCard<MeruruCowardice>(p),
        (cs, p) => cs.CreateCard<NoahEnsnare>(p),
        (cs, p) => cs.CreateCard<NayukaJealousy>(p),
        (cs, p) => cs.CreateCard<SherryVoid>(p),
        (cs, p) => cs.CreateCard<RaiyaMadness>(p),
        (cs, p) => cs.CreateCard<AnanlinVanity>(p),
        (cs, p) => cs.CreateCard<Emaregret>(p),
        (cs, p) => cs.CreateCard<Hannadelusion>(p),
        (cs, p) => cs.CreateCard<Cocoworry>(p),
        (cs, p) => cs.CreateCard<ArisaGuilt>(p),
        (cs, p) => cs.CreateCard<MiliaLost>(p),
    ];

    /// <summary>随机创建一张原罪诅咒卡（默认不带原罪组件，由亚里沙的调用方按需挂载）。</summary>
    public static CardModel CreateRandom(ICombatState combatState, Player owner, Rng rng)
    {
        var factory = rng.NextItem(All) ?? All[0];
        return factory(combatState, owner);
    }

    /// <summary>随机创建 count 张互不重复的原罪诅咒卡（默认不带原罪组件，由调用方按需挂载）。</summary>
    public static List<CardModel> CreateRandomDistinct(ICombatState combatState, Player owner, Rng rng, int count)
    {
        var pool = All.ToList();
        var result = new List<CardModel>(Math.Min(count, pool.Count));
        while (result.Count < count && pool.Count > 0)
        {
            var factory = rng.NextItem(pool);
            if (factory is null) break;
            pool.Remove(factory);
            result.Add(factory(combatState, owner));
        }
        return result;
    }
}