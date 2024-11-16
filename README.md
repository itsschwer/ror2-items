# items from itsschwer

A content mod that adds new items and equipment.

## aim
to add items that synergise with existing game mechanics, maybe (and without diluting the item pool).

## additions

### equipment

#### **Replenisher**
> *Restores broken, consumed, and empty items. Transforms on use.*

<span style="color: #95cde5;">Restores</span> broken, consumed, and empty items back into their <span style="color: #e5c962;">original forms</span>. Does not affect items that can regenerate. Equipment <span style="color: #95cde5;">transforms</span> into a <span style="color: #e5c962;">random equipment</span> when depleted.

15s cooldown

##### *Notes*

- "Depleted" is similar to "used", except synergises with *Fuel Cells* *(i.e. only triggers when all equipment charges are used)*
- Affects the following items:
    - *Dio's Best Friend*
    - *Pluripotent Larva* — becomes non-void if restoring alongside *Dio's Best Friend*
    - *Delicate Watch*
    - *Power Elixir*
    - *Tonic Affliction*
- Can still be used even if the user does not have any items that can be restored
- Essentially a variation of the *Recycler* (transforms items) and the *Trophy Hunter's Tricorn* (consumable equipment)

##### *Strategies*
- Can be used to give broken items a second chance or to immediately scrap them at a Scrapper
- Given the low cooldown, it may synergise well with *Fuel Cells* to proc items that trigger on equipment activation (*War Horn*, *Bottled Chaos*)
- Since the equipment only transforms when all charges are depleted, *Fuel Cells* may enable the user to effectively become immortal by allowing them to repeatedly restore *Dio's Best Friend* as soon as they revive
    - This comes at the cost of not being able to carry other equipment across stages

### items
- *tba*
<!--
- void spare drone parts
    - *mainly because umbra-summoned col. droneman is OP and hard to see; also to add a third void red option*
    - → railgunner alt utility slow field?
    - → allies gain a buff ward (similar to celestine?)
        - *incentivise sticking together; maybe only organic allies?*
        - which buff?
            - warbanner?
            - lifesteal/leech?
- consumable (green?) — retaliation
    - on low health, trigger your on-kill effects at your location
        - *enable using on-kill effects against bosses*
        - *synergy with Replenisher*
-->

## api
- Use `itsschwer.Items.MendConsumedTransformations.Register()` to add additional transformations to this mod's [*Replenisher*](#replenisher) equipment
    - *e.g. for [**SivsContentPack**](https://thunderstore.io/package/Sivelos1/SivsContentPack/)*
      ```cs
      itsschwer.Items.MendConsumedTransformations.Register(new ReplenishTransformation(SivsItems.GlassShieldBroken, SivsItems.GlassShield));

      itsschwer.Items.MendConsumedTransformations.Register(new ReplenishTransformation(SivsItems.DropYellowItemOnKillUsed, SivsItems.DropYellowItemOnKill));
      ```

## notes
- using unused game assets for models and icons
    - ∴ the sprite outline for *Replenisher* suggests that the equipment is Lunar, even though it isn't
