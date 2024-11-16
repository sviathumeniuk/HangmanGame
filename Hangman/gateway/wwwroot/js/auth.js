document.getElementById('login-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
  
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
  
    try {
      const response = await fetch('http://localhost:5131/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });
  
      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('token', data.token);
        window.location.href = 'game.html';
      } else {
        alert('Login failed.');
      }
    } catch (error) {
      console.error('Error:', error);
      alert('An error occurred.');
    }
  });

  document.getElementById('register-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
  
    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
  
    try {
      const response = await fetch('/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ username, email, password })
      });
  
      if (response.ok) {
        try {
          const data = await response.json();
          alert('Registration successful! Redirecting to login...');
          window.location.href = 'login.html';
        } catch {
          alert('Registration successful! Redirecting to login...');
          window.location.href = 'login.html';
        }
      } else {
        const errorData = await response.json().catch(() => ({ Message: 'Unknown error' }));
        alert(`Error: ${errorData.Message}`);
      }
    } catch (error) {
      console.error('Error during registration:', error);
      alert('An unexpected error occurred. Please try again.');
    }
  });  