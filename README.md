<img width="1024" height="290" alt="image" src="https://github.com/user-attachments/assets/5d78b635-6000-43c1-89ed-95f8fb2fb45d" />

Take The King is a 2D 5x5 chess puzzle game where every move counts. Strategize your moves and attempt to checkmate the enemy king in just 3 moves or less. Each of the 20 levels is a brand new setup built to challenge you.
On startup, the game begins with a tutorial teaching the rules of the game. The tutorial only appears on startup and will appear again on reset.

![Take the King Gameplay](https://github.com/YAG1307/Take-The-King/blob/main/Demo.gif)
# Where to Play
**Play directly on itch.io WEB or Desktop:** [Take the King on itch.io](https://yag1307.itch.io/take-the-king) 

---

# Information
* BoardManager: Majority board logic in BoardManager.cs tracking turn counts, valid tile selections, and win/loss conditions.
* **ScriptableObject Level Pipeline:** Level layouts and piece starting configurations decoupled via `LevelDataSO.cs` for rapid scene creation.
* PlayerPrefs: Used Unity's PlayerPrefs to save player level progression and unlock levels.

---

# Tools
* Game Engine: Unity 6 
* Programming Language: C#
* Target Platforms: WebGL (HTML5 Browser), Windows Desktop

---

## Credits & Acknowledgments
* **Game Development & Programming:** YAG
* **Title Screen Artwork:** Zarlynn
* **Board & Piece Sprites:** [Kenney Board Game Icons](https://kenney.nl/assets/board-game-icons)
* **Audio & Sound Effects:** [Pixabay](https://pixabay.com/sound-effects/)
