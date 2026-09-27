using MegaCrit.Sts2.Core.Helpers;

namespace ManosabaLin.Characters.Common;

internal static class CardEffectHoverTipFactory
{
    /// <summary>
    ///     按卡牌类型取<b>规范模型</b>生成效果提示。用于「自己没有这张卡的实例，但想在悬浮提示里
    ///     展示它的效果」的场合，例如「火之时代」预览三张分支牌。
    ///     规范模型没有 Owner，但 <c>EnergyIconHelper.GetPrefix</c> 会回退到 <c>CardModel.Pool</c>，
    ///     <c>DynamicVars</c> 也会按 <c>CanonicalVars</c> 惰性构建 ⇒ 数值占位符照常解析。
    /// </summary>
    public static IHoverTip FromCard<T>(string locEntry) where T : CardModel
    {
        return FromCard(ModelDb.Card<T>(), locEntry);
    }

    public static IHoverTip FromCard(CardModel card, string locEntry)
    {
        var title = new LocString("cards", $"{locEntry}.title");
        var description = new LocString("cards", $"{locEntry}.description");
        var energyPrefix = EnergyIconHelper.GetPrefix(card);

        title.Add("energyPrefix", energyPrefix);
        description.Add("energyPrefix", energyPrefix);
        card.DynamicVars.AddTo(title);
        card.DynamicVars.AddTo(description);
        ApplyEnergyPrefix(title, energyPrefix);
        ApplyEnergyPrefix(description, energyPrefix);
        return new HoverTip(title, description);
    }

    private static void ApplyEnergyPrefix(LocString locString, string energyPrefix)
    {
        foreach (var value in locString.Variables.Values)
        {
            if (value is EnergyVar energyVar)
                energyVar.ColorPrefix = energyPrefix;
        }
    }
}
