# SafeCrack-Numbers

A Unity puzzle game where the player must **crack a digital safe** by solving four number sequences.  
Built with **Unity 6 (URP)**, **TextMeshPro** and the **Unity Input System** — UI constructed entirely in code, no prefabs.

---

## Gameplay

The safe displays a number sequence with a missing value. The player must identify the hidden  
mathematical rule and enter the correct answer.

- **4 sequences** must be solved in a randomised order to open the safe  
- **3 attempts** per sequence — use them wisely  
- Wrong answers reduce the remaining tries; exhausting all attempts **resets the safe**  
- Invalid input (empty, non-numeric, negative) is rejected with a message

### Example sequences

| Rule | Display |
|------|---------|
| Triangular offset | `2 → 4 → 7 → 11 → 16 → ?` |
| Alternating ×3 / +1 | `2 → 6 → 7 → 21 → 22 → ?` |
| Fibonacci-like | `1 → 2 → 3 → 5 → 8 → ?` |
| Perfect squares | `1 → 4 → 9 → 16 → 25 → ?` |

---

## Running with Docker 🐳

> **Prerequisite:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) must be installed.

### 1 – Export the WebGL build from Unity

1. Open the project in **Unity 6+**
2. Go to **File → Build Settings**
3. Select **WebGL** → click **Switch Platform**
4. Click **Build** and choose the folder **`Build/WebGL`** inside this repository
5. Wait for the build to finish

### 2 – Build & run the Docker container

```bash
# Build the image and start the container
docker compose up --build
```

The game is now running at **http://localhost:8080** — open it in any browser.

### Stop the container

```bash
docker compose down
```

### Manual Docker commands (alternative)

```bash
# Build the image manually
docker build -t safecrack-numbers .

# Run the container
docker run -d -p 8080:80 --name safecrack safecrack-numbers
```

---

## Project structure

```
SafeCrack-Numbers/
├── Assets/
│   └── Scripts/
│       └── SafeGameManager.cs   # All game logic + runtime UI
├── Build/
│   └── WebGL/                   # ← Unity WebGL export goes here
├── Dockerfile
├── docker-compose.yml
├── nginx.conf                   # Unity WebGL MIME-type config
└── .dockerignore
```

---

## Tech stack

| Area | Technology |
|------|-----------|
| Engine | Unity 6 (URP) |
| Language | C# |
| UI | Runtime-built (no prefabs) |
| Text | TextMeshPro |
| Input | Unity Input System |
| Container | nginx:alpine via Docker |

---

*Projekt: Innovation Lab WH – Safe Crack Numbers | Kunde: Fabian Wagner*
