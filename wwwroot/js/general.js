

// פונקציה לניווט לעמוד לוגין
const redirectToLogin = () => {
    window.location.href = "/html/login.html"; 
};

// פונקציה להתנתקות המשתמש
const logoutUser = () => {
    localStorage.removeItem("authToken"); 
    redirectToLogin(); 
};

const getToken = () => {
    const token = localStorage.getItem("authToken");
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        if (payload.exp > Math.floor(Date.now() / 1000)) return token;
        localStorage.removeItem("authToken");
    } catch {
        localStorage.removeItem("authToken");
    }
    return null;
};

// פונקציה להבאת כל המשתמשים (admin בלבד)
const getAllUsers = async () => {
    try {
       const token= getToken(); 
        if (!token) {
            alert("You are not logged in. Please log in.");
            return;
        }
        const response = await fetch(userUri, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) {
            if (response.status === 403) {
                console.log("Access denied. Admin permissions required.");
            } else {
                throw new Error(`Unable to fetch users: ${response.statusText}`);
            }
            return;
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching users:', error);
    }
};

// פונקציה לקבלת פריטים
const getAllItems = async () => {
    const token = getToken();
    if (!token)
      return;
    try {
      const response = await fetch('/tasks', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      const data = await response.json();            
      return data;
    } catch (error) {
      console.error('Unable to get items.', error);
    }
  };

  const apiRequest = async(url, method, body = null) => {
    const token = getToken();
    if (!token) throw new Error('Authentication required.');
    const headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
    };
    const response = await fetch(url, { method, headers, body: body ? JSON.stringify(body) : null });
    if (!response.ok) throw new Error(`Unable to perform ${method} request: ${response.statusText}`);
    return await response.json();
};