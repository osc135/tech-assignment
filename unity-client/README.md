# Hijack Poker — Unity Game Client (Option D)

## Why Option D

I chose Option D because it plays to my game development interest while still requiring full-stack awareness. Working within a JS/Node backend repo, consuming REST APIs, and building a polished client on top gives me the chance to show range. It's also the most tangible option: a reviewer can start Docker, hit Play in Unity, and immediately see a working poker table. The other options are solid engineering challenges, but this one lets me demonstrate both technical depth and visual output.

---

## Quick Start

```bash
# 1. Start the backend
cp .env.example .env
docker compose --profile engine up -d

# 2. Verify API is running
curl http://localhost:3030/health

# 3. Open Unity project
#    Open unity-client/ in Unity 2022.3+ LTS or Unity 6

# 4. Hit Play in the Unity Editor
```

The poker table connects to the backend automatically and shows connection status. Click "Next Step" or "Auto Play" to start watching hands.

---

## What This Is

A **spectator/viewer client** — not an interactive poker game. The backend engine makes all betting decisions automatically (players check, call, fold, etc. on their own). This client visualizes what the engine is doing, step by step, through a 16-step hand state machine.

You can:
- **Step through hands** one state at a time with "Next Step"
- **Auto-play** hands continuously at configurable speeds (0.25s, 0.5s, 1s, 2s)
- **Watch** cards get dealt, bets posted, community cards revealed, and winners paid out
- **Review** a running hand history log of what happened each step

You cannot fold, call, raise, or make any betting decisions — that would require interactive betting UI and backend changes that are out of scope for this challenge.

---

## What's Implemented

### Core Features (P0)
- 6-seat poker table displaying player names, stacks, bets, and engine-decided actions
- Card rendering with suit symbols and colors (red for hearts/diamonds, black for clubs/spades)
- Face-down cards during play, revealed face-up at showdown (step 12+)
- Community cards appearing incrementally (flop/turn/river) with empty placeholders
- Position chips: Dealer (D, gold, fixed center-top of felt), Small Blind (SB, blue), Big Blind (BB, purple)
- Pot display with side pot support
- Winner highlighting with gold border, hand rank text, and winnings amount
- Phase label HUD showing current step and hand number
- Next Step button with double-click prevention
- Health check on startup with connection status display
- Error handling for API failures
- Unit tests: card parsing, model deserialization, money formatting, GameManager, TableStateManager

### Should-Have Features (P1)
- Auto-play with speed selector (1s, 0.5s, 0.25s, 2s)
- Hand history log — dropdown panel with step-by-step action log including player actions, community cards, and winner info
- Cumulative stack tracking across hands (with $0 floor clamping)

### Not Implemented
- Interactive betting (fold, call, raise, all-in) — the engine auto-plays all decisions
- Animations (card flip, pot count-up, stack transitions)
- Custom card sprites
- Sound effects
- Structured API call logging / observability
- WebSocket integration with cash-game-broadcast

---

## Architecture

```
┌─────────────────────────────────────┐
│            UI Layer                 │
│  TableView, SeatView, CardView,    │
│  HudView, ControlsView,           │
│  CommunityCardsView,              │
│  HandHistoryView                   │
│  (UXML templates + USS styles)    │
├─────────────────────────────────────┤
│         Event System                │
│  TableStateManager.OnStateChanged   │
│  TableStateManager.OnHandCompleted  │
│  GameManager.OnProcessingChanged    │
│  HandHistoryManager.OnEntries...    │
├─────────────────────────────────────┤
│        Manager Layer                │
│  GameManager (API orchestration)    │
│  TableStateManager (state + fixes)  │
│  HandHistoryManager (action log)    │
│  GameBootstrap (composition root)   │
├─────────────────────────────────────┤
│         Domain Models               │
│  GameState, PlayerState,            │
│  TableResponse, ProcessResponse     │
├─────────────────────────────────────┤
│          API Layer                  │
│  IPokerApi (interface)              │
│  PokerApiClient (UnityWebRequest)   │
└─────────────────────────────────────┘
```

### Data Flow

```
User clicks "Next Step"
  → ControlsView fires OnNextStep
    → GameManager.AdvanceStep()
      → POST /process (advance one step)
      → GET /table/1 (fetch new state)
      → TableStateManager.UpdateState()
        → Corrects dealer position
        → Applies stack offsets
        → Fires OnStateChanged
          → TableView renders all seats, cards, pot, HUD
          → HandHistoryManager logs the step
```

---

## Key Decisions

### UI Toolkit over uGUI

UI Toolkit fits this project better than uGUI because the poker table is essentially a data-driven display. USS stylesheets let me manage visual states like folded, all-in, and winner through class toggles instead of writing imperative code to change colors and opacity. The UXML/USS pattern also maps closely to HTML/CSS, which makes sense in a repo that already has a web-based reference viewer. uGUI would work fine, but it scatters styling across Inspector fields and code, which gets messy with this many visual states.

### Plain C# Core, MonoBehaviour Only at Boundaries

GameManager, TableStateManager, and HandHistoryManager are plain C# classes. Only GameBootstrap and PokerApiClient are MonoBehaviours, since they need Unity's lifecycle (Update loop for auto-play, UnityWebRequest for HTTP). This keeps the core logic testable in Edit Mode without spinning up a scene, which is why all unit tests run fast with simple mock injection.

### IPokerApi Interface Extraction

IPokerApi lets GameManager depend on an abstraction instead of the concrete PokerApiClient. This is what makes the GameManager tests possible — they inject a MockPokerApi that returns preset responses, so tests run without a backend or network. It also means swapping the HTTP implementation later (e.g., for WebSocket-based updates) wouldn't require changing any game logic.

### Dealer Chip Placement

The dealer chip (D) is fixed at the center-top of the felt rather than floating next to a specific seat. The backend has a bug where `dealer_seat` resets to 1 on every new game, so rotating the chip between seats would be misleading. The client still computes the correct dealer index from `gameNo` (`dealerIndex = (gameNo - 1) % playerCount`) for internal state, but visually the chip stays put. SB/BB chips float next to their respective seats since the backend controls who actually posts the blinds.

### Full State Replace

Each API call returns the complete table state, and the client replaces everything rather than tracking deltas. This keeps the rendering logic simple — each view just reads the current state and renders it, without needing to know what changed. It also avoids a whole class of bugs where the client and server get out of sync from missed or partial updates.

---

## Known Limitations

- **Spectator only** — there is no way to fold, call, raise, or make any betting decisions. The backend engine auto-plays all actions and this client just visualizes the results.
- **SB/BB don't rotate** — the backend always assigns the same seats for blinds. Only the dealer chip rotates (client-side fix). Fixing this properly requires a backend change.
- **Stack offsets can diverge** — cumulative stack tracking is a client-side approximation. The backend resets stacks each hand, so displayed values may drift from reality over many hands.
- **Action badges reflect engine decisions** — badges like CHECK, CALL, FOLD show what the engine decided, not player input.

---

## Project Structure

```
unity-client/Assets/
├── Scripts/
│   ├── Api/
│   │   ├── IPokerApi.cs                 # Interface for testability
│   │   └── PokerApiClient.cs            # UnityWebRequest implementation
│   ├── Models/
│   │   ├── GameState.cs                 # Game state + IsShowdown, IsHandComplete
│   │   ├── PlayerState.cs              # Player state + status helpers
│   │   ├── TableResponse.cs            # Top-level API wrapper
│   │   ├── ProcessResponse.cs          # POST /process response
│   │   ├── HealthResponse.cs           # GET /health response
│   │   ├── SidePot.cs                  # Side pot model
│   │   └── Winner.cs                   # Winner model
│   ├── Managers/
│   │   ├── GameBootstrap.cs            # Composition root (MonoBehaviour)
│   │   ├── GameManager.cs             # API orchestration, error handling
│   │   ├── TableStateManager.cs       # State tracking, dealer fix, stack offsets
│   │   └── HandHistoryManager.cs      # Action log builder
│   ├── UI/
│   │   ├── TableView.cs               # Root table renderer
│   │   ├── SeatView.cs                # Per-seat: name, stack, cards, chips, actions
│   │   ├── CardView.cs                # Card rendering (face-up/down/empty)
│   │   ├── CommunityCardsView.cs      # 5 community card slots
│   │   ├── HudView.cs                 # Phase label + hand number
│   │   ├── ControlsView.cs            # Next Step, Auto Play, Speed
│   │   └── HandHistoryView.cs         # Dropdown action log panel
│   └── Utils/
│       ├── CardUtils.cs               # Card string parsing + suit symbols
│       ├── MoneyFormatter.cs          # Currency formatting ($150.00, +$12.00)
│       └── PhaseLabels.cs             # Step name → display label mapping
├── UI/
│   ├── Templates/
│   │   └── PokerTable.uxml           # Main layout
│   └── Styles/
│       ├── common.uss                 # Theme variables
│       ├── table.uss                  # Table, felt, controls, history panel
│       ├── player.uss                 # Seats, position chips, actions
│       └── card.uss                   # Card face-up/down/empty styles
├── Tests/EditMode/
│   ├── CardUtilsTests.cs
│   ├── MoneyFormatterTests.cs
│   ├── ModelTests.cs
│   ├── GameManagerTests.cs
│   ├── TableStateManagerTests.cs
│   └── TestData/
│       ├── table_preflop.json
│       └── table_showdown.json
└── Scenes/
    └── PokerTable.unity
```

---

## Tests

All tests run in Edit Mode (no Play Mode required):

| Test File | What It Covers |
|-----------|---------------|
| CardUtilsTests | Card parsing, suit symbols, colors, display strings |
| MoneyFormatterTests | Currency formatting with/without sign |
| ModelTests | JSON deserialization, status properties, showdown detection |
| GameManagerTests | Connection checks, state loading, step advancement, double-click prevention, error events |
| TableStateManagerTests | Dealer rotation, stack clamping, state events, hand completion |

---

## Future Work

- WebSocket integration with cash-game-broadcast for real-time state updates instead of request-based polling
- Card deal and flip animations, pot count-up transitions, stack change tweens
- Sound effects for card deals, chip movements, and winner announcements
- Multi-table support with a table selector to connect to different table IDs
- Interactive betting UI allowing players to fold, call, raise, and go all-in instead of auto-played actions
- Players with $0 stacks should be sat out and unable to continue playing until they rebuy
- Additional game types like Blackjack using the same client architecture

---

## API Reference

The holdem-processor runs at `http://localhost:3030` and exposes three endpoints.

### GET /health

```json
{ "service": "holdem-processor", "status": "ok", "timestamp": "..." }
```

### POST /process

Advances the hand by one step.

```json
// Request
{ "tableId": 1 }

// Response
{ "success": true, "result": { "status": "processed", "tableId": 1, "step": 6, "stepName": "DEAL_FLOP" } }
```

### GET /table/{tableId}

Returns full game + player state. See `Models/` for the C# types that map to this response.

### Card Format

Last character = suit (H/D/C/S), everything before = rank (2-10, J, Q, K, A).

| Card | Display |
|------|---------|
| `"AH"` | A♥ |
| `"10D"` | 10♦ |
| `"2C"` | 2♣ |
| `"KS"` | K♠ |

### Hand Steps

```
 0: GAME_PREP                  →  Shuffle deck, reset state
 1: SETUP_DEALER               →  Rotate dealer button
 2: SETUP_SMALL_BLIND          →  Post small blind
 3: SETUP_BIG_BLIND            →  Post big blind
 4: DEAL_CARDS                 →  Deal 2 hole cards per player
 5: PRE_FLOP_BETTING_ROUND     →  Pre-flop betting
 6: DEAL_FLOP                  →  Deal 3 community cards
 7: FLOP_BETTING_ROUND         →  Flop betting
 8: DEAL_TURN                  →  Deal 1 turn card
 9: TURN_BETTING_ROUND         →  Turn betting
10: DEAL_RIVER                 →  Deal 1 river card
11: RIVER_BETTING_ROUND        →  River betting
12: AFTER_RIVER_BETTING_ROUND  →  Prepare for showdown
13: FIND_WINNERS               →  Evaluate hands
14: PAY_WINNERS                →  Distribute pot
15: RECORD_STATS_AND_NEW_HAND  →  Hand complete
```
