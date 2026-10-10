# TrickyRing

One-tap arcade game (Unity 6, `6000.0.50f1`, URP 2D). A ball orbits a ring; tap to switch between the
inner and outer lane, collect stars, avoid the obstacles.

This repository additionally contains the **coin economy, shop, themes and power-ups** feature
(see [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md) for the one-page design note).

## Run it

1. Open the project with Unity `6000.0.50f1` (Unity 6 – no upgrade needed).
2. Open `Assets/Scenes/Scene_Splash.unity` and press Play (build order: Splash → Loading → Menu → Game).
3. Menu: swipe or use the bottom tabs. Left tab = **Shop**, middle = Home, right = Leaderboard.

Handy while testing (Unity menu **Tools ▸ Shop**): *Add 100 / 1000 / 10000 Coins* (Ctrl/Cmd+Shift+K adds 1000), *Reset Save*, *Open Save Folder*.

## What was added

| Requirement | Where / how |
|---|---|
| **Currency** earned from runs, spent in the shop | Each run pays `floor(score × coinsPerScorePoint)` coins when it ends (`GameSession.OnRunEnded`). The balance is shown in the shop header. |
| **Survives kill-app** | Every change is written immediately to `persistentDataPath/shop_save.json` (temp file + `.bak` copy, so an interrupted write cannot lose it). |
| **Buying without enough coins fails visibly** | Button shakes and flashes red, the balance pill shakes, a red toast says how many coins are missing. Nothing is deducted. |
| **Shop replaces "Coming Soon"** | `Prefab_ShopPanel` now carries `ShopPanelView`, which builds a scrolling list from the config (power-ups, player themes, world themes). |
| **2 power-ups** (3 shipped) | Shield, Double score, Clear all – see below. Delete one from the JSON to sell only two. |
| **≥2 player themes / ≥2 world themes** | 3 each (1 free default + 2 purchasable). Player theme tints the ball and its particles, world theme tints the ring. |
| **Own ≠ equip** | Ownership and equipping are separate in the shop rules (`ShopService`); the shop UI equips a theme right after it is bought. Owned themes show **EQUIP** to switch; exactly one player and one world theme are equipped (**EQUIPPED**). |
| **Power-ups used from a HUD control during a run** | Three buttons at the bottom of the game screen, visible/tappable only while a run is active. Unused counts persist; they cannot be used on Game Over or before the run starts. |
| **Open for later additions** | New items/sections/prices = JSON only. New power-up behaviour = one class + one registry line. New equippable kind = JSON (+ a few lines to apply it). |
| **Config via JSON** | `Assets/Resources/Shop/ShopConfig.json` – prices, items, section order, economy. |
| **Unity 6, no IAP** | Coins are earned in-game only. |
| **Bonus UI** | Same palette, font, sliced button sprites and tab styling as the existing menu. |

### Power-ups

* **Shield** – absorbs the next obstacle hit (the obstacle disappears, the shield pops).
* **Double score** – stars are worth ×2 for `duration` seconds (default 10). Coins follow the score.
* **Clear all** – removes every obstacle on the ring; the game's own spawner refills it as stars are collected.

## Shop config (`Assets/Resources/Shop/ShopConfig.json`)

```jsonc
{
  "economy":  { "startingCoins": 0, "coinsPerScorePoint": 1.0, "minCoinsPerRun": 0 },
  "sections": [ { "id": "powerups", "title": "Power-ups" }, ... ],   // display order = list order
  "items": [                                                         // order inside a section = list order
    { "id": "powerup_shield", "section": "powerups", "kind": "powerup",
      "name": "Shield", "description": "...", "price": 60, "maxOwned": 99,
      "effect": "shield", "duration": 0, "icon": "shield" },
    { "id": "player_sunset", "section": "player_themes", "kind": "playerTheme",
      "name": "Sunset", "description": "...", "price": 250, "color": "#FF8A3D" },
    { "id": "player_classic", "section": "player_themes", "kind": "playerTheme",
      "price": 0, "ownedByDefault": true, "color": "#397FA0", "name": "Classic" }
  ]
}
```

* `kind`: `powerup`, `playerTheme`, `worldTheme`. Anything else non-`powerup` is treated as an equippable slot.
* `enabled: false` hides an item from the shop and blocks buying; what the player already owns is kept.
* `ownedByDefault: true` marks the default theme of a slot (needed once per theme kind).
* Invalid entries (missing/duplicate id, unknown section) are skipped with a warning, never a crash.
* To try other values on a device without rebuilding, put a file named `shop_config.override.json` into
  `Application.persistentDataPath`; it takes precedence over the bundled config.

### Adding things

* **Sell something new that already has behaviour** (another theme, a cheaper shield): edit the JSON only.
* **A new power-up**: implement `IPowerUpEffect`, call `PowerUpEffectRegistry.Register("myEffect", ...)`
  (or add it to the dictionary), add a JSON item with `"effect": "myEffect"` and an icon key (icons live in
  `Resources/Shop/ShopResources.asset`). The HUD button and shop row appear automatically.
* **A new theme kind** (e.g. trails): add items with a new `kind` and a section; the shop, ownership, equip
  and persistence already work – only applying it in-game (like `ThemeApplier` does for ball/ring) needs code.

## Save file

`persistentDataPath/shop_save.json` – coins, owned theme ids, power-up counts, equipped theme per kind. Only ids
are stored (never prices/names), so re-balancing the JSON never invalidates an existing save.

## Project structure (new code)

```
Assets/Scripts/Shop/
  Core/          Shop.Core assembly – pure logic, no scene/UI dependencies, unit-tested
    Model/       JSON config + save DTOs
  Runtime/       bootstrap, game-scene integration, theme applier, power-up effects
  UI/            shop tab, HUD, uGUI helpers (all built from code in the game's existing style)
Assets/Resources/Shop/   ShopConfig.json, ShopResources.asset (sprites/font/event references)
Assets/Sprite/Sprite_Shop/   coin + power-up icons
Assets/Tests/EditMode/   NUnit tests for the shop rules (Window ▸ General ▸ Test Runner)
Assets/Editor/ShopDebugMenu.cs
```

## Changes to existing files

Kept to the minimum:

* `Prefab_ShopPanel.prefab` – the "Coming Soon" label was removed and `ShopPanelView` added.
* `BallController.cs` (+6/−2 lines):
  * `ScoreMultiplier` property used by *Double score* (stars give `1 × multiplier`).
  * The tap that flips the ball is ignored when it lands on a power-up button (the game reads raw
    mouse/touch input, so without this a HUD tap would also move the ball).

Everything else (scenes, other scripts, event assets) is untouched – the feature hooks in by listening to the
existing `OnStartMovingBallEvent`, `OnScoreChangeEveent` and `OnGameEndEvent` assets.

## Tests

`Window ▸ General ▸ Test Runner ▸ EditMode`: purchase failing without coins, unlock ≠ equip, one item per slot,
power-up stacking/consumption, persistence round-trip, corrupt-save fallback, reward calculation, config validation.

## Known limitations / notes

* The power-up and shop UIs are built from code at runtime (no new prefabs/scenes) so the existing scenes stay
  untouched; the layout was designed for the game's 1080×1920 portrait reference and stretches for landscape.
* The existing leaderboard and high-score code is unrelated to the economy and was not modified.
* Existing quirks noticed but intentionally not touched: high-score/`PlayerPrefs` key mismatch between
  `GameSceneManager` and `HomePanel`, and the leaderboard rank off-by-one.
