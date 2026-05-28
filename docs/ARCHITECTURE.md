# Architecture

This document explains the engineering decisions behind the project. It complements the [README](../README.md) — the README is for the elevator pitch; this is the deep dive.

## Table of contents

1. [High-level architecture](#high-level-architecture)
2. [Dependency injection (VContainer, 2-scope)](#dependency-injection-vcontainer-2-scope)
3. [Event bus (GenericEventBus, dual-pipe)](#event-bus-genericeventbus-dual-pipe)
4. [Asset loading (Unity Addressables)](#asset-loading-unity-addressables)
5. [Scene flow](#scene-flow)
6. [Feature module pattern](#feature-module-pattern)
7. [Domain layer purity](#domain-layer-purity)
8. [Interaction & animation](#interaction--animation)
9. [Editor tooling](#editor-tooling)
10. [Conventions and project rules](#conventions-and-project-rules)
11. [Testing strategy](#testing-strategy)
12. [Known trade-offs](#known-trade-offs)

---

## High-level architecture

Four layers, single direction:

```
┌───────────────────────────────────────────────────────────────┐
│  Presentation (depends on UnityEngine)                        │
│    Views, UIPanelBase, MonoBehaviour Controllers, Canvas      │
└─────────────────────────────────┬─────────────────────────────┘
                                  │ via interfaces / signals
                                  ▼
┌───────────────────────────────────────────────────────────────┐
│  Application (POCO controllers)                               │
│    Feature controllers, presenters, Bootstrap/Main installers │
└─────────────────────────────────┬─────────────────────────────┘
                                  │
                                  ▼
┌───────────────────────────────────────────────────────────────┐
│  Domain (pure C#, zero UnityEngine references)                │
│    MatchingService, PricingService, FollowerNormalizer        │
└─────────────────────────────────┬─────────────────────────────┘
                                  │
                                  ▼
┌───────────────────────────────────────────────────────────────┐
│  Data (ScriptableObject + value-types)                        │
│    InfluencerDatabase, MatchingConfig, CategoryConfig,        │
│    SerializableGuid, ScoredInfluencer, PriceBreakdown         │
└───────────────────────────────────────────────────────────────┘
```

**Cross-layer rules:**

- A lower layer **never** references a higher layer.
- Domain compiles without `UnityEngine.dll` (other than `UnityEngine.Object` for `ScriptableObject` data carriers).
- Presentation depends on Application via interfaces (`IMatchingService`, `IPricingService`) — substitutable for tests.

---

## Dependency injection (VContainer, 2-scope)

The project uses [VContainer](https://github.com/hadashiA/VContainer) with a two-tier scope hierarchy:

```
BootLifetimeScope  (DontDestroyOnLoad — alive for app lifetime)
        │
        └── SceneLifetimeScope  (per scene — disposed on scene exit)
```

### BootLifetimeScope

Registered in `BootInstaller.cs`. Survives every scene transition.

| Registered service | Why Boot scope |
|---|---|
| `ProjectPipe` (GenericEventBus) | Carries cross-scene signals (`SplashCompletedMessage`, `MatchInfluencerRequestedMessage`) |
| `IPricingService`, `IMatchingService` | Stateless calculators — instantiating once is cheaper than per-scene |
| `AppState` | Holds budget + selected categories across scenes (volatile, no persistence) |
| `IScreenFader` | Cross-scene screen fader component (registered via `TransitionInstaller`); pulls `ScreenFaderConfig` from DI |
| `UISharedConfig`, `SplashConfig`, `ScreenFaderConfig` (loaded via Addressables) | Cross-scene UI / fader configs loaded by `UIConfigInstaller` and released at app shutdown |
| `InputService` (new Input System wrapper) | Single input source, app-wide |
| `AppNavigationController` | Listens to `ProjectPipe` and drives scene transitions |

### SceneLifetimeScope (Main scene only)

Registered in `MainInstaller.cs` and feature installers. Disposed when Main scene unloads.

| Registered service | Why Scene scope |
|---|---|
| `MainPipe` (GenericEventBus) | Scene-local signal channel; gone when Main unloads |
| `InfluencerDatabase` + 100 `InfluencerData` (loaded via Addressables) | ~16-30 MB atlas footprint — only resident when Main is active |
| `CategoryConfig`, `MatchingConfig`, `BudgetConfig`, `RecommendationConfig`, `ScoreBarConfig`, `PlatformConfig` | Every Main-only authored config loaded via Addressables alongside the database |
| `AddressableHandleRegistry` | Tracks `AsyncOperationHandle`s and releases on scope dispose |
| Feature controllers (`BudgetCategoryInputController`, `RecommendationListController`, `InfluencerDetailController`, `EmptyStateController`) | One per feature, scene-local lifecycle |
| Feature views (registered via `[SerializeField]` in feature installers) | MonoBehaviour singletons in the scene |
| `UIManager`, `PanelNavigationController` | Composition root for panel switching |

### Why two scopes (not three or one)?

- **One scope** would force everything to live for the app's lifetime → atlas resident on Splash + MainMenu → memory wasted.
- **Three scopes** (Boot + MainMenu + Main) was explored. MainMenu has no scene-specific DI bindings (the controller only needs `ProjectPipe` from Boot), so a third scope was pure ceremony. Memory rule: see [`pipe_architecture.md`](#) — scene-local pipe only exists when scene-specific traffic justifies it.

---

## Event bus (GenericEventBus, dual-pipe)

Cross-feature and cross-scene communication is mediated by [GenericEventBus](https://github.com/PeturDarri/GenericEventBus) — a strongly-typed signal bus over a marker interface `ISignal`.

### Two pipes, two scopes

| Pipe | Scope | Purpose | Examples |
|---|---|---|---|
| `ProjectPipe` | Boot | Cross-scene navigation, app-lifetime events | `SplashCompletedMessage`, `MatchInfluencerRequestedMessage` |
| `MainPipe` | Main | Within-scene feature coordination | `BudgetCommittedMessage`, `CardSelectedMessage`, `BackRequestedMessage`, `EmptyStateRequestedMessage` |

A signal raised on `MainPipe` is invisible to subscribers of `ProjectPipe` (and vice versa). This isolation prevents accidental cross-scene listeners and keeps the Main scene's signal traffic contained.

### Signal definition

Signals are `readonly struct` types implementing `ISignal`:

```csharp
public readonly struct BudgetCommittedMessage : ISignal { }

public readonly struct CardSelectedMessage : ISignal
{
    public readonly SerializableGuid InfluencerId;

    public CardSelectedMessage(SerializableGuid influencerId)
    {
        InfluencerId = influencerId;
    }
}
```

Public readonly fields are permitted (immutable DTOs); public mutable fields are forbidden.

### Subscribe / publish pattern

Controllers subscribe in `IInitializable.Initialize()` and unsubscribe in `IDisposable.Dispose()` — interface methods are **always explicit**:

```csharp
[Inject] private MainPipe m_Pipe;

void IInitializable.Initialize()
{
    m_Pipe.SubscribeTo<BudgetCommittedMessage>(OnBudgetCommitted);
}

void IDisposable.Dispose()
{
    m_Pipe.UnsubscribeFrom<BudgetCommittedMessage>(OnBudgetCommitted);
}

private void OnBudgetCommitted(ref BudgetCommittedMessage msg) { ... }
```

VContainer handles `IInitializable` and `IDisposable` lifecycle automatically.

### Why the bus, not direct calls?

- **Zero compile-time cycle risk.** Two features can react to the same signal without referencing each other.
- **Easy to add observers.** A future telemetry/analytics service can subscribe to the bus and emit events for every signal — no controller modification.
- **Tests can drive controllers directly** by raising signals on a test container's pipe.

### What is *not* on the bus

Intra-feature interactions (controller ↔ its own view) use plain C# events and method calls. Putting every interaction on the bus would add ceremony with no isolation gain. Rule of thumb: **signal across feature boundaries; direct calls within a feature**.

---

## Asset loading (Unity Addressables)

The project uses Unity Addressables for **scene-scoped data assets** and **scene loading itself**. UI panel prefabs are *not* Addressable — they're live scene GameObjects under the Main canvas.

### What is Addressable

| Group | Address | Loaded by |
|---|---|---|
| `Scenes` | `Scene/MainMenu`, `Scene/Main` | `SceneLoader.LoadAsync()` |
| `Data` | `Data/SplashConfig`, `Data/UISharedConfig`, `Data/ScreenFaderConfig` | `UIConfigInstaller` (Boot scope, released at app shutdown) |
| `Data` | `Data/InfluencerDatabase`, `Data/CategoryConfig`, `Data/MatchingConfig`, `Data/BudgetConfig`, `Data/RecommendationConfig`, `Data/ScoreBarConfig`, `Data/PlatformConfig` | `MainDataInstaller` (Main scope, released on Main scene exit) |

Splash stays in Build Settings as the initial scene (Addressables cannot serve the bootstrap scene).

### `AddressableHandleRegistry`

A registry per DI scope (one in Boot, one in Main). Each `LoadAndRegister<T>` call loads the asset via `Addressables.LoadAssetAsync<T>(address).WaitForCompletion()`, registers the instance with VContainer, and records the `AsyncOperationHandle`. When the owning scope disposes — app shutdown for Boot, Main scene exit for Main — the registry releases every handle via `Addressables.Release(handle)`, freeing the underlying asset and any dependency atlas.

```csharp
public void LoadAndRegister<T>(IContainerBuilder builder, string address)
    where T : UnityEngine.Object
{
    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
    T instance = handle.WaitForCompletion();
    builder.RegisterInstance(instance).AsSelf();
    Track(handle);
}
```

`WaitForCompletion()` blocks ~5–10 ms total for Boot configs (2 tiny SOs) and ~100–300 ms for Main scope (6 assets including the avatar atlas) — Main's blocking is concealed behind the `ScreenFader` transition, imperceptible to the user.

### Memory profile

| Scene | Avatar atlas (~16–30 MB) resident? |
|---|---|
| Splash | ❌ |
| MainMenu | ❌ |
| Main | ✅ (loaded on `MainInstaller.InstallBindings()`) |
| Main → MainMenu (back nav, if added) | ❌ (released on scope dispose) |

---

## Scene flow

```
[Splash.unity] ── SplashCompletedMessage ──▶ AppNavigationController
                                                    │
                                                    ▼  SceneLoader.LoadAsync("Scene/MainMenu")
[MainMenu.unity] ── MatchInfluencerRequestedMessage ──▶ AppNavigationController
                                                    │
                                                    ▼  SceneLoader.LoadAsync("Scene/Main")
[Main.unity] (BudgetCategoryInputView shown by PanelNavigationController.Start)
        │
        │  user inputs budget + categories, clicks Continue
        ▼  BudgetCommittedMessage
[Main.unity] (RecommendationListView shown by RecommendationListController)
        │
        │  user taps a card
        ▼  CardSelectedMessage
[Main.unity] (InfluencerDetailView shown by InfluencerDetailController)
        │
        │  user taps Back
        ▼  BackRequestedMessage → UIManager.GoBack()
```

`SceneLoader` is a static utility (no instance state) wrapping `Addressables.LoadSceneAsync(address, LoadSceneMode.Single)` with `UniTask` integration. `LoadSceneMode.Single` auto-releases the previously loaded Addressables scene.

`AppNavigationController` lives in the Boot scope, hides scene transitions behind `IScreenFader.FadeOut/FadeIn` to mask the brief `WaitForCompletion` block in `MainInstaller`.

---

## Feature module pattern

Each feature (`Budget`, `Recommendation`, `Detail`, `EmptyState`, `Splash`, `MainMenu`) follows the same shape:

```
Feature/
├── Controllers/         # POCO IInitializable + IDisposable, lives in DI scope
│   └── XController.cs
│   └── XPresenter.cs    # (optional) pure presentation logic
├── Views/               # MonoBehaviour, owns Canvas/RectTransform/Buttons
│   └── XView.cs
├── Models/              # (optional) value types / ScriptableObject configs
│   └── XConfig.cs
├── Signals/             # (optional) feature-owned ISignal structs
│   └── XMessage.cs
└── Installers/          # [Serializable] IInstaller, embedded in MainInstaller
    └── XInstaller.cs
```

### Responsibilities

- **View** — MonoBehaviour, captures UGUI events, exposes C# events upward (`event Action XClicked`), exposes `Display*`/`Set*` methods to be called by the controller.
- **Controller** — POCO (no MonoBehaviour). Subscribes to bus signals + view events. Drives the view by calling its `Display`/`Set` methods. Raises bus signals for cross-feature handoff. Lifecycle via `IInitializable` + `IDisposable`.
- **Presenter** — pure C# class with no Unity dependencies. Translates Domain output to a ViewModel struct the View can render. EditMode-testable.
- **Installer** — `[Serializable]` non-MonoBehaviour, embedded as a `[SerializeField]` field in `MainInstaller`. Registers the controller as `EntryPoint`, the view as instance, the presenter as singleton.

### Show pattern

| Show type | Mechanism |
|---|---|
| Own-feature show (controller showing its own view) | `m_UIManager.Show<OwnView>()` direct call |
| Cross-feature show | Raise a navigation signal; `PanelNavigationController` subscribes and calls `Show` |
| Generic navigation (back, error) | Raise a navigation signal |

The composition root for `UIManager.Show<T>()` calls is `PanelNavigationController` for cross-feature shows; controllers calling `Show` on their own view is intra-feature and stays direct.

---

## Domain layer purity

`MatchingService`, `PricingService`, `FollowerNormalizer`, and the `MatchingConfig` / `CategoryConfig` data layer compile without any `UnityEngine` reference except for `ScriptableObject` (data carrier only).

- `MatchingService.Rank(...)` accepts plain `IReadOnlyList<InfluencerData>` + config + budget + categories and returns sorted `IReadOnlyList<ScoredInfluencer>`.
- No `Time.deltaTime`, no `Debug.Log` in domain code.
- All randomness is removed (deterministic tie-break: hybrid score → engagement rate → `SerializableGuid`).

This makes the entire scoring/pricing layer EditMode-testable in <50 ms per test, no Play mode required.

### Scoring formula

```
hybrid_score = categoryWeight   * normalize(avg_category_score)
             + followersWeight  * normalize(followers, min_followers, max_followers)
             + engagementWeight * engagement_rate

final_score  = hybrid_score * (over_budget ? over_budget_penalty : 1.0)
```

Coefficients (`CategoryWeight + FollowersWeight + EngagementWeight = 1.0`) and the penalty multiplier are authored in `MatchingConfig`. The custom `MatchingConfigInspector` enforces the sum invariant at edit time.

---

## Interaction & animation

The project uses DOTween for all tweening with UniTask for awaitable sequencing — no coroutines, no `Mathf.Lerp` in `Update`. Every interactive surface has visible feedback within ~100 ms of touch.

### Animation surface

| Layer | Trigger | Implementation |
|---|---|---|
| Press-down feedback (all Buttons) | `IPointerDown` / `IPointerUp` / `IPointerExit` on every `Button` | `UIButtonPressFeedback` MonoBehaviour — scales transform to `0.95` on press, `1.0` on release; skips when `Button.interactable == false` |
| Category toggle bounce | `Toggle.onValueChanged` | `CategoryToggleView.PlayBounce` (`DOPunchScale`) |
| Card tap punch | UI Button click on a recommendation card | `InfluencerCardView.HandleClickButtonClicked` — `DOPunchScale` on the card root, `CardClicked` raised only after the tween completes (touch feels committed) |
| Card list stagger | After `RecommendationListPresenter.Build` returns a non-empty result | `RecommendationListView.SpawnCardsStaggeredAsync` — `UniTask.Delay(StaggerDelay)` between spawns, per-card `CanvasGroup.DOFade(0 → 1)` |
| Panel entrance fade | `IUIPanel.Show` | `UIPanelBase.PlayEntranceFade` — `CanvasGroup.DOFade(0 → 1)` linked to gameObject |
| Splash title / subtitle | Initial scene activation | `SplashView.Start` — `TMP_Text.DOFade` with subtitle delay |
| Budget validation shake | `BudgetCategoryInputView.ShowError(non-null)` | `RectTransform.DOShakeAnchorPos(0.3s, x: 10px)`; original anchored position cached on `Awake` and restored before each shake (re-trigger safe) |

### Cancellation and lifetime

- All tweens use `.SetLink(gameObject)` so Unity destruction kills the tween automatically.
- `CancellationTokenSource` linked to `destroyCancellationToken` wraps the staggered card spawn (`RecommendationListView.m_SpawnCts`); cancellation cleanly aborts the in-flight loop without leaking timers.
- Tweens that need explicit re-triggering (`m_TapTween`, `m_BounceTween`, `m_ShakeTween`) are tracked as fields and `Kill`-ed in `OnDestroy` and before each new tween start.

### Tuning surface

| Scope | Location |
|---|---|
| Recommendation feature (stagger delay, card fade, card tap punch) | `RecommendationConfig.asset` |
| Budget feature (toggle bounce, validation shake) | `BudgetConfig.asset` |
| Cross-panel framework animations (panel fade, button press) | `UISharedConfig.asset` |

Config scope follows the existing pattern: each feature owns its tunables in a feature-level config (matching `RecommendationConfig`, `SplashConfig`, `ScoreBarConfig`); `UISharedConfig` is reserved for tunables that are genuinely shared by every panel or every button. No hardcoded animation constants in interaction components — designers tune through `Config Manager Window` grouped by category.

`UIButtonPressFeedback` is attached per-Button in prefabs via the one-shot editor utility (see [Editor tooling](#editor-tooling)). Runtime auto-attach was deliberately rejected as Unity-non-idiomatic magic.

---

## Editor tooling

Two editor windows + three custom inspectors + one property drawer, all sharing a single flat-modern style cache.

### `EditorStyleCache` (`Assets/Final/Editor/EditorStyleCache.cs`)

Lazy-initialized shared GUIStyles, palette, and spacing constants. Centralizes the editor visual identity and eliminates per-OnGUI GUIStyle / Texture2D allocations. Resources are released on assembly reload via `AssemblyReloadEvents.beforeAssemblyReload`.

Highlights:

- 4 / 8 / 12 / 16 / 24 spacing grid as `const float`
- Typography hierarchy: `TitleLarge`, `TitleMedium`, `SectionLabel`, `CaptionLabel`, `ListItem`, `ListItemSelected`
- `DrawSelectionHighlight(rect)` paints a soft tint + 3 px left-edge accent bar — VSCode / JetBrains style
- `DrawHoverHighlight(rect)` paints a subtle hover background; consumers set `wantsMouseMove = true`

### `ConfigManagerWindow`

Lists every `ScriptableObject` that implements `IVisibleConfig`, grouped by Category. Two-pane layout: category-grouped list (left) + selected config's Inspector embedded inline (right). Used for centralized config authoring.

### `InfluencerDatabaseEditorWindow`

Master-detail editor for the 100-influencer database. Searchable, sortable (Name / Followers / BasePrice), with avatar thumbnails rendered correctly from the `SpriteAtlas` via `sprite.textureRect` UV-mapped `GUI.DrawTextureWithTexCoords`. Add / Remove / Regenerate-ID actions, two-stage confirmation dialogs for destructive bulk operations.

### `InfluencerDatabaseEditorHelper`

Static helper exposing shared `RefreshFromFolder`, `ValidateAll`, and `IsEmailMissingOrMalformed` so the Inspector and the editor window share one implementation (zero duplication).

---

## Conventions and project rules

| Rule | Rationale |
|---|---|
| `m_` prefix for non-serialized private fields, `s_` for private static, `k_` for `const` | Compile-time grep-ability of mutation surface |
| Explicit interface implementation always (`void IInitializable.Initialize()`) | Compile-time clarity about which call belongs to which interface; prevents leaking interface methods on the public API |
| `[SerializeField] private` for all Inspector-bound fields | Field is private to the class, exposed only to the Inspector binding |
| Never null-check `[SerializeField]` references | Inspector binding is the contract; a null is a bug, not a runtime branch |
| Member-level XML doc forbidden | Names already document themselves; type-level summary captures the *why* |
| No defensive `try/catch` on internal API calls | Internal contracts; catching exceptions hides real bugs (boundary `try/catch` for user input / file IO / network is fine) |
| Pure-C# Domain layer | EditMode testability without Play mode |
| New Input System only | `UnityEngine.Input` legacy API never used |
| DOTween for tweens, UniTask for async | No coroutines, no manual `Mathf.Lerp` in `Update` |
| Editor-only paths live in `Final.Editor.EditorAssetPaths` | Runtime never embeds editor strings |

---

## Testing strategy

```
Tests/
├── EditMode/           # Pure C#, no scene, <1s total runtime
│   ├── Common/         # MatchingConfig + CategoryConfig validation
│   ├── Recommendation/ # Presenter + Database + scoring tests
│   ├── Detail/         # Detail presenter tests
│   └── Helpers/        # TestDataFactory (deterministic data builders)
└── PlayMode/           # Requires runtime, scene + Unity objects
    ├── Recommendation/ # Card view binding, pool recycling
    ├── Detail/         # Detail view binding
    ├── Integration/    # End-to-end flow
    ├── Transition/     # Screen fader
    └── UI/             # Safe-area container
```

**EditMode covers:**
- `MatchingService.Rank` determinism + tie-break order
- `PricingService.Calculate` edge cases (empty categories, zero budget)
- Presenter view-model construction
- `InfluencerDatabase.TryFindById` lookups
- Inspector validation rules (weight sum invariant, category-count consistency)

**PlayMode covers:**
- View ↔ controller binding (events fire, state updates propagate to UI)
- Recommendation list object pooling lifecycle
- Scene transition behavior (fader timing)
- `RectTransform` safe-area math against simulated notch insets

The split keeps the fast feedback loop (EditMode) wide and reserves PlayMode for things that genuinely require a frame.

---

## Known trade-offs

These are conscious choices, not gaps. They're listed here so a reviewer doesn't mistake a design choice for an oversight.

- **`AppState` is volatile.** Budget + selected categories reset on app restart. Saving them would require a persistence layer the MVP doesn't justify.
- **Splash is not Addressable.** Build Settings cannot bootstrap from an Addressable scene. Splash stays in Build Settings; MainMenu + Main are Addressable.
- **`WaitForCompletion` blocks the main thread during Main scope load.** ~100–300 ms, masked behind the screen fader. Async-throughout would propagate `UniTask` into installer chains and require VContainer's `IAsyncStartable`, adding complexity without a measurable UX gain at this asset volume.
- **Atlas is a single Addressable, not per-avatar.** Splitting individual avatars would defeat sprite batching (10 cards → 10 draw calls vs. 1 batched call). The atlas trade-off is correct at this scale.
- **`AppNavigationController` ↔ `SceneLoader`.** `SceneLoader` is a static utility (no DI registration) since it holds no state. If retry / telemetry / progress reporting becomes needed, it can be promoted to an injectable singleton in 5 minutes.
- **No remote profile yet.** Addressables is configured for local content only. The foundation is in place to host bundles remotely and ship content updates post-launch; no remote-content workflow is wired up.
- **Single monolithic runtime assembly.** Per-feature `asmdef` was considered and rejected — the cross-feature signal types would create circular references between assemblies. One `Final.InfluencerMatch` assembly + separate `Final.Systems.*` for framework code (the "house pattern") is intentional.

---

If you spot a decision in the code that isn't documented here, it's likely either (a) a regretted hack that should be documented or removed, or (b) a missed entry — please open an issue.
