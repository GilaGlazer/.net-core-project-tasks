const userUri = '/users'; // URI למשתמשים
let users = []; // מערך למשתמשים

const statusLabels = {
  0: 'Pending',
  1: 'In Progress',
  2: 'Done',
  3: 'Canceled'
};

const priorityLabels = {
  1: 'Low',
  2: 'Medium',
  3: 'High'
};

const getUsers = async () => {
    const data = await getAllUsers();
    displayUsers(data);
}

const addUser = async () => {
    const addUsernameTextbox = document.getElementById('add-username');
    const addPasswordTextbox = document.getElementById('add-password');
    const addEmailTextbox = document.getElementById('add-email');
    const addTypeTextbox = document.getElementById('add-type');

    const user = {
        userName: addUsernameTextbox.value.trim(),
        password: addPasswordTextbox.value.trim(),
        email: addEmailTextbox.value.trim(),
        type: addTypeTextbox.value.trim()
    };

    try {
        const token = getToken();
        if (!token) {
            alert("You are not logged in. Please log in.");
            return;
        }
        const response = await fetch(userUri, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(user)
        });

        if (!response.ok) {
            throw new Error(`Unable to add user: ${response.statusText}`);
        }

        await getUsers(); // עדכון הרשימה
        addUsernameTextbox.value = '';
        addPasswordTextbox.value = '';
        addEmailTextbox.value = '';
        addTypeTextbox.value = '';
        closeAddUserModal(); // סגירת החלון לאחר ההוספה
    } catch (error) {
        console.error('Error adding user:', error);
    }
};

// פונקציה למחיקת משתמש (admin בלבד)
const deleteUser = async (id) => {
    try {
        const token = getToken();
        if (!token) {
            alert("You are not logged in. Please log in.");
            return;
        }

        const response = await fetch(`${userUri}/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`Unable to delete user: ${response.statusText}`);
        }

        await getUsers(); // עדכון הרשימה
    } catch (error) {
        console.error('Error deleting user:', error);
    }
};

// // פונקציה להצגת טופס עריכה
const displayEditUserForm = (id) => {

    const user = users.find(user => user.id === id);
    if (!user) {
        console.log("User not found.");
        return;
    }
    // טוען את פרטי המשתמש לטופס
    document.getElementById('edit-username').value = user.userName || '';
    document.getElementById('edit-email').value = user.email || '';
    document.getElementById('edit-password').value = user.password || '';
    document.getElementById('edit-type').value = user.type || '';
    document.getElementById('edit-id').value = user.id || '';
    // פותח את חלון העריכה
    openEditUserModal(id);
};


// פונקציה לסגירת טופס עריכה
const closeUserInput = () => {
    document.getElementById('editUserForm').style.display = 'none';
};


// פונקציה להצגת המשתמשים בטבלה
const displayUsers = (data) => {
    const tBody = document.getElementById('users');
    tBody.innerHTML = '';
    const button = document.createElement('button');

    data.forEach(user => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditUserForm(${user.id})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.className = 'delete-button';
        deleteButton.setAttribute('onclick', `deleteUser(${user.id})`);

        let userItemsBtn = button.cloneNode(false);
        userItemsBtn.innerHTML = `User's Tasks`;
        userItemsBtn.setAttribute('onclick', `displayUserItemsToUser(${user.id})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNodeUsername = document.createTextNode(user.userName);
        td1.appendChild(textNodeUsername);

        let td2 = tr.insertCell(1);
        let textNodeEmail = document.createTextNode(user.email);
        td2.appendChild(textNodeEmail);

        let td3 = tr.insertCell(2);
        let textNodePassword = document.createTextNode(user.password);
        td3.appendChild(textNodePassword);

        let td4 = tr.insertCell(3);
        let textNodeType = document.createTextNode(user.type);
        td4.appendChild(textNodeType);

        let td5 = tr.insertCell(4);
        td5.appendChild(editButton);

        let td6 = tr.insertCell(5);
        td6.appendChild(deleteButton);

        let td7 = tr.insertCell(6);
        td7.appendChild(userItemsBtn);
    });

    users = data;
};



const openAddUserModal = () => {
    document.getElementById('addUserModal').style.display = 'block';
};

const closeAddUserModal = () => {
    document.getElementById('addUserModal').style.display = 'none';
};



// פונקציה להצגת חלון עריכה
const openEditUserModal = (id) => {
    const user = users.find(user => user.id === id);
    if (!user) {
        console.log("User not found.");
        return;
    }

    // טוען את פרטי המשתמש לטופס
    document.getElementById('edit-username').value = user.userName || '';
    document.getElementById('edit-email').value = user.email || '';
    document.getElementById('edit-password').value = user.password || '';
    document.getElementById('edit-id').value = user.id || '';

    let select = document.getElementById('edit-type');
    select.value = user.type || '';

    Array.from(select.options).forEach(option => {
        if (option.value === user.type) {
            option.selected = true;
        }
    });
    // מציג את החלון
    document.getElementById('editUserModal').style.display = 'block';
};

// פונקציה לסגירת חלון עריכה
const closeEditUserModal = () => {
    document.getElementById('editUserModal').style.display = 'none';
};

// פונקציה לעדכון משתמש (admin בלבד)
const updateUser = async () => {
    const userId = document.getElementById('edit-id').value;
    const user = {
        id: parseInt(userId, 10),
        userName: document.getElementById('edit-username').value.trim(),
        password: document.getElementById('edit-password').value.trim(),
        email: document.getElementById('edit-email').value.trim(),
        type: document.getElementById('edit-type').value.trim()
    };
    try {
        const token = getToken();
        if (!token) {
            alert("You are not logged in. Please log in.");
            return;
        }

        const response = await fetch(`${userUri}/${userId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(user)
        });

        if (!response.ok) {
            throw new Error(`Unable to update user: ${response.statusText}`);
        }

        await getUsers(); // עדכון הרשימה
        closeEditUserModal(); // סגירת חלון העריכה
    } catch (error) {
        console.error('Error updating user:', error);
    }
};


const redirectToItemsPage = () => {
    window.location.href = "/html/item.html";
}

const closeUserItemsModal = () => {
    document.getElementById('userItemsModal').style.display = 'none';
}

const displayUserItemsToUser = async (id) => {
    const tasks = await getAllItems();
    const userItems = tasks.filter(t => t.userId === id);

    if (!userItems || userItems.length === 0) {
        console.log("לא נמצאו פריטים למשתמש זה.");
        return;
    }

    let tableHtml = `
        <table style="width:100%; border-collapse: collapse;" border="1">
            <thead style="background-color:#f0f0f0;">
                <tr>
                    <th>Title</th>
                    <th>Description</th>
                    <th>Created At</th>
                    <th>Due Date</th>
                    <th>Status</th>
                    <th>Priority</th>
                </tr>
            </thead>
            <tbody>
    `;

    userItems.forEach(task => {
        console.log(task);
        tableHtml += `
            <tr>
                <td>${task.title}</td>
                <td>${task.description || '-'}</td>
                <td>${new Date(task.createdAt).toLocaleDateString()}</td>
                <td>${task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}</td>
                <td>${statusLabels[task.status]}</td>
                <td>${priorityLabels[task.priority]}</td>
            </tr>
        `;
    });

    tableHtml += '</tbody></table>';

    document.getElementById('user-items-table').innerHTML = tableHtml;
    document.getElementById('userItemsModal').style.display = 'block';
};