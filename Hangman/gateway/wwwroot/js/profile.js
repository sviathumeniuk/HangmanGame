document.addEventListener('DOMContentLoaded', async () => {
    const profileContainer = document.getElementById('profile-container');
    const gameHistoryContainer = document.getElementById('game-history-container');

    const API_USER_PROFILE = 'http://localhost:5131/api/auth/profile';
    const API_GAME_HISTORY = 'http://localhost:5144/api/game/history';

    const getToken = () => localStorage.getItem('token');

    const loadUserProfile = async (userId) => {
        const token = getToken();
        const response = await fetch(`${API_USER_PROFILE}/${userId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            return await response.json();
        } else {
            const error = await response.json();
            throw new Error(error.Message || 'Failed to load user profile');
        }
    };

    const loadGameHistory = async (userId) => {
        const token = getToken();
        const response = await fetch(`${API_GAME_HISTORY}/${userId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            return await response.json();
        } else {
            const error = await response.json();
            throw new Error(error.Message || 'Failed to load game history');
        }
    };

    const displayUserProfile = (profile) => {
        profileContainer.innerHTML = `
            <h2>User Profile</h2>
            <p><strong>Username:</strong> ${profile.username}</p>
            <p><strong>Email:</strong> ${profile.email}</p>
            <p><strong>Rating:</strong> ${profile.rating}</p>
        `;
    };

    const displayGameHistory = (history) => {
        gameHistoryContainer.innerHTML = `
            <h2>Game History</h2>
            <ul>
                ${history.map(game => `
                    <li>
                        <strong>Word:</strong> ${game.word} |
                        <strong>Win:</strong> ${game.isWin ? 'Yes' : 'No'} |
                        <strong>Attempts Left:</strong> ${game.attemptsLeft} |
                        <strong>Start:</strong> ${new Date(game.startTime).toLocaleString()} |
                        <strong>End:</strong> ${game.endTime ? new Date(game.endTime).toLocaleString() : 'In Progress'}
                    </li>
                `).join('')}
            </ul>
        `;
    };

    const parseJwt = (token) => {
        try {
            const base64Url = token.split('.')[1];
            const base64 = atob(base64Url.replace(/-/g, '+').replace(/_/g, '/'));
            return JSON.parse(decodeURIComponent(escape(base64)));
        } catch (error) {
            console.error('Invalid token:', error);
            return null;
        }
    };

    const loadProfileData = async () => {
        const token = getToken();

        if (!token) {
            alert('Token is missing. Please log in.');
            window.location.href = 'login.html';
            return;
        }

        const decodedToken = parseJwt(token);
        if (!decodedToken || !decodedToken.nameid) {
            alert('Invalid token. Please log in again.');
            window.location.href = 'login.html';
            return;
        }

        const userId = decodedToken.nameid;

        try {
            const [userProfile, gameHistory] = await Promise.all([
                loadUserProfile(userId),
                loadGameHistory(userId)
            ]);

            displayUserProfile(userProfile);
            displayGameHistory(gameHistory);
        } catch (error) {
            console.error(error.message);
            alert('Error loading profile data.');
        }
    };

    await loadProfileData();
});