# Function Responsibility Checklist — RPG Action Domain

**Purpose:** apply the project's 18 responsibility groups as a review vocabulary, not as a mandate to create 18 classes. The source checklist defines four layers and four baseline rules: open must close; Layer 2 must have no side effects; one primary responsibility per function; errors must not be swallowed.

## 1. The 18 groups and their current Action-domain mapping

| Layer | Group | Responsibility | Action-domain decision |
|---|---|---|---|
| 1 — Coordination | Handle | Receive and normalize an entry request | `TryExecute` is the compatibility entry point. It delegates; it does not mutate resources itself. |
| 1 — Coordination | Guard | Reject an invalid context early | `CanExecute` checks Actor eligibility. Source/Location/NPC context remains in its owning flow controller. |
| 1 — Coordination | Orchestrator | Call steps in the required order | `TryPrepare` and `TryApply` coordinate the use case; no separate orchestrator class is needed for this small flow. |
| 1 — Coordination | Policy | Choose a business branch | `CanExecute` selects normal versus recovery eligibility. A separate policy object is not justified by one small rule. |
| 1 — Coordination | Coordinator | Coordinate shared resources/locks | Not implemented: execution is sequential and single-threaded. No lock or parallel work exists to coordinate. |
| 1 — Coordination | Recovery | Retry or fallback after failure | Not implemented: invalid gameplay requests are rejected, not retried. No transient dependency exists. |
| 2 — Pure logic | Sanitizer | Clean input without changing its meaning | Not needed in runtime execution: authored JSON is parsed and validated by Infrastructure loaders. Do not silently repair Action definitions here. |
| 2 — Pure logic | Validation | Check shape and business constraints without mutation | Action definition constructors and `ActionEffectExecutor.Prepare` validate authored Action/effect data. `CanExecute` is the runtime eligibility guard, not a sanitizer. |
| 2 — Pure logic | Factory | Construct a valid object from explicit inputs | Constructors create validated immutable snapshots/plans. No additional factory is needed. |
| 2 — Pure logic | Mapper | Change data shape between representations | No bidirectional mapper is needed: runtime snapshots are purpose-built calculation values, not serialized DTOs or ViewModels. |
| 2 — Pure logic | Calculate | Derive values from explicit inputs | `ActivityTimeCalculator`, `ActivityFatigueCalculator`, `CharacterRuntimeState.Apply` and `GameClockState.AdvanceMinutes` return calculated values without mutating their inputs. |
| 2 — Pure logic | Apply | Apply rules and return a new state | `ActionEffectExecutor.Apply(ActionEffectPlan, CharacterRuntimeState)` returns a new immutable Character snapshot. |
| 3 — State | State Change | The authorized write boundary for owned state | `ActionStateChangeApplier` commits one prepared result; `Character` and `GameClock` remain the owners and validate/apply their own state. |
| 3 — State | Lifecycle | Pair open/close and cleanup | Not needed: Action execution opens no disposable resource, subscription or temporary scope. |
| 4 — External I/O | Query | Read state without side effects | Snapshot capture reads Character/GameClock state. Context availability queries remain in their existing Application query services. |
| 4 — External I/O | Persistence | Store/load/delete durable data | Not part of this in-memory execution flow. Save/load remains a separate concern. |
| 4 — External I/O | Gateway | Call an external service through an adapter | Not needed: Action execution has no external service dependency. |
| 4 — External I/O | Publisher | Publish an event after state is consistent | Not needed: no multiple independent subscribers currently react to Action completion. |

## 2. Action execution data flow

```text
TryExecute (Handle / compatibility facade)
  -> TryPrepare (Orchestrator)
       -> CanExecute (Guard / current Policy)
       -> ActivityTimeCalculator.Calculate
       -> ActivityFatigueCalculator.Calculate
       -> ActionEffectExecutor.Prepare (Validation + immutable effect plan)
       -> CharacterRuntimeState.Capture + GameClock.CaptureState (Query)
       -> pure state calculations
       -> ActionExecutionPlan + ActionStateChange (Factory)
  -> TryApply (Orchestrator)
       -> reject already-applied plan
       -> compare current snapshots with prepared snapshots (stale Guard)
       -> re-check Actor eligibility and clock bounds
       -> ActionStateChangeApplier (State Change)
            -> Character.ApplyRuntimeState
            -> GameClock.ApplyState
       -> mark plan applied
```

Layer 2 values are immutable. The Apply calculation never mutates Character, GameClock or its input snapshot. Only the State Change boundary writes the authoritative runtime owners.

## 3. Four baseline rules applied

1. **Open must close:** not applicable to this synchronous path because it opens no resource, subscription or lifecycle scope. Any future resource acquisition must be paired with cleanup even on failure.
2. **Layer 2 has no side effects:** snapshot calculations and effect application return new immutable values. They do not write to Character or GameClock.
3. **One function, one primary group:** entry, guard, calculation, immutable Apply and state commit have distinct methods. Small orchestration methods remain small and sequence the steps.
4. **Do not swallow errors:** invalid authored effects return a preparation failure with a reason; unexpected exceptions are not caught and hidden.

## 4. Explicit exceptions

- `ActionExecutionService.TryExecute` remains as a compatibility facade because existing callers use it. It delegates to Prepare → Apply and contains no domain mutation. `EXCEPTION(FACADE)`: remove only after all callers migrate to the explicit API.
- `ActionStateChangeApplier` writes Character and GameClock sequentially after validating both. `EXCEPTION(ATOMIC)`: current runtime is sequential and owner writes are validated, deterministic in-memory assignments; there is no concurrent mutation or external call between writes. If execution becomes concurrent or a write becomes fallible, introduce an actual transaction/rollback boundary before expanding this method.

## 5. Deliberately outside this Action slice

Coordinator, Recovery, Sanitizer, dedicated Factory, Mapper, Lifecycle, Persistence, Gateway and Publisher are not represented by placeholder classes. Add one only when a concrete use case introduces its responsibility. Logging, authentication, authorization, caching, transaction framework, serialization, notifications, aggregation, rate limiting, idempotency, scheduling and configuration are outside these 18 groups and must not be forced into them.
