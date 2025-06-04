
const handleFormSubmit = async (event) => {
    event.preventDefault();
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;
    const errorMessage = document.getElementById("error-message");
    errorMessage.classList.remove("visible");
    // בדיקה בסיסית אם השדות ריקים
    if (!email || !password) {
        errorMessage.textContent = "Please fill in all fields.";
        errorMessage.classList.add("visible");
        return;
    }
    try {
        const response = await fetch("/Login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ Email: email, Password: password }),
        });

        // טיפול במצב שבו המשתמש לא נמצא
        if (!response.ok) {
            if (response.status === 403) {
                errorMessage.textContent = "User does not exist.";
                errorMessage.classList.add("visible");
            } else {
                throw new Error("Login failed. Please check your credentials.");
            }
            return;
        }

        // שמירת הטוקן ב-localStorage
        const token = await response.text();
        console.log("Token:", token);
        localStorage.setItem("authToken", token);

        window.location.href = "/html/item.html";
    } catch (error) {
        console.log(error.message);
        errorMessage.textContent = "An error occurred. Please try again.";
        errorMessage.classList.add("visible");
    }
};

// פונקציה לטיפול בהצגת/הסתרת סיסמה
const handleTogglePassword = () => {
    const passwordInput = document.getElementById("password");
    const togglePassword = document.getElementById("togglePassword");

    if (passwordInput && togglePassword) {
        const isPasswordVisible = passwordInput.type === 'text';
        passwordInput.type = isPasswordVisible ? 'password' : 'text';
        togglePassword.src = isPasswordVisible ? '../images/visible.png' : '../images/hide.png';
    } else {
        console.error("Password input or toggle button is missing in the DOM.");
    }
};

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById("loginForm");
    const togglePassword = document.getElementById("togglePassword");

    if (loginForm) {
        loginForm.addEventListener("submit", handleFormSubmit);
    } else {
        console.error("Login form is missing in the DOM.");
    }

    if (togglePassword) {
        togglePassword.addEventListener("click", handleTogglePassword);
    }
});