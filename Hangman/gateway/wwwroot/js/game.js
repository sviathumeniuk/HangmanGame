document.addEventListener('DOMContentLoaded', async () => {
    const wordTemplateEl = document.getElementById('word-template');
    const attemptsLeftEl = document.getElementById('attempts-left');
    const gameStatusEl = document.getElementById('game-status');
    const guessForm = document.getElementById('guess-form');
    const restartButton = document.getElementById('restart-game');
    const settingsForm = document.getElementById('settings-form');
    const gameArea = document.getElementById('game-area');
    const connectButton = document.getElementById('connect-to-game');

    const API_BASE = 'http://localhost:5131/game';

    const getToken = () => localStorage.getItem('token');

    const showGameArea = (data) => {
        wordTemplateEl.textContent = data.wordTemplate;
        attemptsLeftEl.textContent = data.attemptsLeft;
        gameStatusEl.textContent = data.gameStatus;

        gameArea.style.display = 'block';
        settingsForm.style.display = 'none';
        connectButton.style.display = 'none';
    };

    const startGame = async (language, difficultyLevel, category) => {
        const token = getToken();
        if (!token) {
            alert('You must be logged in to start the game.');
            window.location.href = 'login.html'; 
            return;
        }

        const response = await fetch(`${API_BASE}/start`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ language, difficultyLevel, category })
        });

        if (response.ok) {
            const data = await response.json();
            console.log('Game Started:', data);
            showGameArea(data);
        } else {
            const error = await response.json();
            alert(error.message || 'Failed to start the game. Please try again.');
        }
    };

    const connectToGame = async () => {
        const token = getToken();
        if (!token) {
            alert('You must be logged in to connect to a game.');
            window.location.href = 'login.html'; 
            return;
        }

        const response = await fetch(`${API_BASE}/connect`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            console.log('Connected to Active Game:', data);
            showGameArea(data);
        } else {
            const error = await response.json();
            alert(error.message || 'No active game session found.');
        }
    };

    guessForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const letter = document.getElementById('letter').value.toUpperCase();
        const token = getToken();

        const response = await fetch(`${API_BASE}/guess`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ letter })
        });

        if (response.ok) {
            const data = await response.json();
            console.log('Guess Response:', data);
            wordTemplateEl.textContent = data.wordTemplate;
            attemptsLeftEl.textContent = data.attemptsLeft;
            gameStatusEl.textContent = data.gameStatus;

            if (data.isWin) {
                gameStatusEl.textContent = 'You Win!';
                alert('Congratulations! You won!');
            } else if (data.gameStatus === 'Game Over') {
                alert('Game over. Try again.');
            }
        } else {
            const error = await response.json();
            console.error('Guess Error:', error);
            alert(error.message || 'An error occurred. Try again.');
        }

        document.getElementById('letter').value = ''; 
    });

    settingsForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const language = document.getElementById('language').value;
        const difficultyLevel = parseInt(document.getElementById('difficulty').value);
        const category = document.getElementById('category').value;

        await startGame(language, difficultyLevel, category);
    });

    connectButton.addEventListener('click', connectToGame);

    restartButton.addEventListener('click', () => {
        gameArea.style.display = 'none';
        settingsForm.style.display = 'block'; 
        connectButton.style.display = 'block';
    });
});