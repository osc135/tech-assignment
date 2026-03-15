# Unity Game Client — Skeleton Scripts

Starter scripts for **Option D: Unity Game Client**. These provide the data models and API client stub to get you up and running quickly.

## Prerequisites

- **Unity 2022.3+ LTS** (or Unity 6) — [Download](https://unity.com/releases/editor/archive)
- **Docker Desktop** with Docker Compose v2 — for the holdem-processor backend
- **Git** — to clone this repo

## Setup

### 1. Start the backend

```bash
cd tech-assignment
cp .env.example .env
docker compose --profile engine up -d
```

Verify it's running:

---

## What's Implemented

### Core Features (P0)
- 6-seat poker table with player names, stacks, bets, and actions
- Card rendering with suit symbols and colors (red for hearts/diamonds, black for clubs/spades)
- Face-down cards during play, revealed face-up at showdown (step 12+)
- Community cards appearing incrementally (flop/turn/river) with empty placeholders
- Position chips: Dealer (D, gold), Small Blind (SB, blue), Big Blind (BB, purple)
- Client-side dealer rotation (rotates based on hand number)
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

### 2. Create a Unity project

Open Unity Hub and create a new project:
- Template: **3D (Core)** or **2D (Core)** — your choice
- Name: whatever you like (e.g., `HijackPokerClient`)

### 3. Install Newtonsoft JSON

In Unity Editor:
1. **Window > Package Manager**
2. Click **+** > **Add package by name**
3. Enter: `com.unity.nuget.newtonsoft-json`
4. Click **Add**

### 4. Import skeleton scripts

Copy the contents of this `Scripts/` directory into your Unity project's `Assets/Scripts/`:

```
Assets/
└── Scripts/
    ├── Api/
    │   └── PokerApiClient.cs       # REST client stub (you'll implement the TODO methods)
    └── Models/
        ├── GameState.cs             # Game data models + API response wrappers
        └── PlayerState.cs           # Player data model + status code constants
```

### 5. Build your scene

Create a scene with:
- A Canvas for UI elements
- A poker table layout (6 seats around an oval)
- A `PokerApiClient` MonoBehaviour on a GameObject (configure `baseUrl` in the Inspector)
- Controls: Next Step button, Auto Play toggle, Speed selector

### 6. Hit Play

With the Docker backend running, press Play in the Unity Editor. Click your Next Step button to advance the hand and see the table update.

## API Reference

The holdem-processor runs at `http://localhost:3030` and exposes three endpoints.

### GET /health

Returns service status.

```json
{
  "service": "holdem-processor",
  "status": "ok",
  "timestamp": "2026-02-21T12:00:00.000Z"
}
```

### POST /process

Advances the current hand by one state machine step.

**Request:**
```json
{
  "tableId": 1
}
```

**Response:**
```json
{
  "success": true,
  "result": {
    "status": "processed",
    "tableId": 1,
    "step": 6,
    "stepName": "DEAL_FLOP"
  }
}
```

### GET /table/{tableId}

Returns the full table state: game info + all player states.

**Response:**
```json
{
  "game": {
    "id": 1,
    "tableId": 1,
    "tableName": "Starter Table",
    "gameNo": 3,
    "handStep": 6,
    "stepName": "DEAL_FLOP",
    "dealerSeat": 2,
    "smallBlindSeat": 3,
    "bigBlindSeat": 4,
    "communityCards": ["JH", "7D", "2C"],
    "pot": 3.00,
    "sidePots": [],
    "move": 0,
    "status": "in_progress",
    "smallBlind": 1.00,
    "bigBlind": 2.00,
    "maxSeats": 6,
    "currentBet": 0,
    "winners": []
  },
  "players": [
    {
      "playerId": 1,
      "username": "Alice",
      "seat": 1,
      "stack": 150.00,
      "bet": 0,
      "totalBet": 0,
      "status": "1",
      "action": "",
      "cards": ["AH", "KD"],
      "handRank": "",
      "winnings": 0
    },
    {
      "playerId": 2,
      "username": "Bob",
      "seat": 2,
      "stack": 149.00,
      "bet": 0,
      "totalBet": 1.00,
      "status": "1",
      "action": "call",
      "cards": ["QS", "JC"],
      "handRank": "",
      "winnings": 0
    }
  ]
}
```

### Card Format

Cards are strings where the last character is the suit and everything before it is the rank:

| Card | Rank | Suit | Display |
|------|------|------|---------|
| `"AH"` | A | H (Hearts) | A♥ |
| `"10D"` | 10 | D (Diamonds) | 10♦ |
| `"2C"` | 2 | C (Clubs) | 2♣ |
| `"KS"` | K | S (Spades) | K♠ |

### Player Status Codes

| Code | Meaning | Visual Hint |
|------|---------|-------------|
| `"1"` | Active | Normal display |
| `"11"` | Folded | Dimmed / grayed out |
| `"12"` | All-In | Highlighted |

### Hand Steps (state machine)

Each `POST /process` call advances one step:

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
15: RECORD_STATS_AND_NEW_HAND  →  Hand complete — next call starts a new hand
```

## Reference Implementation

See `ui/index.html` in the repo root — it's a vanilla JS implementation of exactly what you're building. Study it to understand:
- How cards are rendered (face-down vs. face-up)
- When showdown reveal happens (step 12+)
- How winner highlighting works
- The auto-play loop logic

## Tips

- **Start simple**: Get a single API call working and rendering one piece of data before building the full table.
- **Test your models**: Write unit tests that deserialize sample JSON into your C# models — catch issues early.
- **Use the Inspector**: Make fields `[SerializeField]` so you can tweak API URL, speeds, and other settings without recompiling.
- **Check the logs**: `Debug.Log` the raw JSON responses to verify you're getting what you expect.
