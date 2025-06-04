const taskUri = '/tasks';
let tasks = [];
const userUri = '/users';
let currentUserDetails = {};


// פונקציה לבדוק אם יש טוקן ולנווט
const checkTokenAndRedirect = () => {
  const token = getToken();
  const currentPath = window.location.pathname;
  if (token && currentPath !== "/html/item.html") {
    window.location.href = "/html/item.html";
  } else if (!token && currentPath !== "/html/login.html") {
    window.location.href = "/html/login.html";
  }
};

const currentUserDetailsFromToken = () => {
  let token = getToken();
  if (token) {
    let payload = token.split('.')[1];
    let decoded = JSON.parse(atob(payload));
    let userType = decoded.type;
    let userId = decoded.userId;
    return { userType, userId };
  }
  return null;
}

window.onload = () => {
  checkTokenAndRedirect()
  let { userType } = currentUserDetailsFromToken();
  if (userType === "admin") {
    document.getElementById('show-users-button').style.display = 'inline-block';
  }
  getItems();
  getUserDetails();

  if (userType === "admin") {
    const tableHeadRow = document.querySelector("table tr");
    const th = document.createElement("th");
    th.textContent = "User Details";
    tableHeadRow.appendChild(th);
  }
};

// פונקציה לקבלת פריטים
const getItems = async () => {
  try {
    const data = await getAllItems();
    displayItems(data);
  } catch (error) {
    console.error('Unable to get items.', error);
  }
};

// פונקציה להוספת פריט
const addItem = async () => {
  const token = getToken();
  if (!token) return;

  // קבלת ערכים מהשדות בטופס
  const titleTextbox = document.getElementById('add-title');
  const descriptionTextbox = document.getElementById('add-description');
  const dueDateTextbox = document.getElementById('add-dueDate');
  const statusSelect = document.getElementById('add-status');
  const prioritySelect = document.getElementById('add-priority');

  let userId = currentUserDetails.id;
  let { userType } = currentUserDetailsFromToken();

  // אם אדמין - שדה בחירה למזהה משתמש
  if (userType === "admin") {
    const addUserIdTextbox = document.getElementById('add-userId');
    userId = parseInt(addUserIdTextbox.value, 10);
  }

  // יצירת אובייקט משימה
  const item = {
    title: titleTextbox.value.trim(),
    description: descriptionTextbox.value.trim() || "",
    dueDate: dueDateTextbox.value ? new Date(dueDateTextbox.value).toISOString() : undefined,
    status: parseInt(statusSelect.value, 10),
    priority: parseInt(prioritySelect.value, 10),
    userId: userId,
  };

  try {
    await fetch(taskUri, {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(item),
    });

    await getItems();

    // איפוס שדות
    titleTextbox.value = '';
    descriptionTextbox.value = '';
    dueDateTextbox.value = '';
    statusSelect.selectedIndex = 0;
    prioritySelect.selectedIndex = 0;

    if (userType === "admin") {
      document.getElementById('add-userId').value = '';
    }

    closeAddItemModal();

  } catch (error) {
    console.error('Unable to add item.', error);
  }
};

const deleteItem = async (id) => {
  const token = getToken();
  if (!token) return;
  try {
    await fetch(`${taskUri}/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    getItems();
  } catch (error) {
    console.error('Unable to delete item.', error);
  }
};

// פונקציה לעדכון פריט
const updateItem = async () => {
  const token = getToken();
  if (!token) return;

  const itemId = document.getElementById('edit-id').value;
  const title = document.getElementById('edit-title').value.trim();
  const description = document.getElementById('edit-description').value.trim();
  const dueDateValue = document.getElementById('edit-dueDate').value;
  const status = document.getElementById('edit-status').value;
  const priority = parseInt(document.getElementById('edit-priority').value, 10);

  let userId = currentUserDetails.id;
  let { userType } = currentUserDetailsFromToken();

  if (userType === "admin") {
    const editUserIdTextbox = document.getElementById('edit-userId');
    userId = parseInt(editUserIdTextbox.value, 10);
  }
  const updatedTask = {
    id: parseInt(itemId, 10),
    title,
    description,
    dueDate: dueDateValue ? new Date(dueDateValue).toISOString() : null,
    status: parseInt(status, 10),
    priority: parseInt(priority, 10),
    userId: parseInt(userId, 10)
  };

  try {
    const response = await fetch(`${taskUri}/${itemId}`, {
      method: 'PUT',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(updatedTask),
    });
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Server responded with error:', response.status, errorText);
      return;
    }

    await getItems();
    closeEditItemModal();
  } catch (error) {
    console.error('Unable to update item.', error);
  }
};



const statusToText = (status) => {
  switch (status) {
    case 0: return 'Pending';
    case 1: return 'In Progress';
    case 2: return 'Completed';
    case 3: return 'Canceled';
    default: return 'Unknown';
  }
};

const displayItems = (data) => {
  const tBody = document.getElementById('tasks');
  tBody.innerHTML = '';
  const button = document.createElement('button');

  let { userType } = currentUserDetailsFromToken();

  data.forEach(item => {
    let editButton = button.cloneNode(false);
    editButton.innerText = 'Edit';
    editButton.setAttribute('onclick', `openEditItemModal(${item.id})`);

    let deleteButton = button.cloneNode(false);
    deleteButton.innerText = 'Delete';
    deleteButton.className = 'delete-button';
    deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);

    let userDetailsBtn = button.cloneNode(false);
    userDetailsBtn.innerText = 'User Details';
    userDetailsBtn.setAttribute('onclick', `displayUserDetailsToItem(${item.userId})`);

    let tr = tBody.insertRow();

    let tdTitle = tr.insertCell(0);
    tdTitle.innerText = item.title;

    let tdDescription = tr.insertCell(1);
    tdDescription.innerText = item.description || '';

    let tdCreated = tr.insertCell(2);
    tdCreated.innerText = new Date(item.createdAt).toLocaleDateString();

    let tdDue = tr.insertCell(3);
    tdDue.innerText = item.dueDate ? new Date(item.dueDate).toLocaleDateString() : '';

    let tdStatus = tr.insertCell(4);
    tdStatus.innerText = statusToText(item.status); // ← כאן השינוי

    let tdPriority = tr.insertCell(5);
    tdPriority.innerText = priorityToText(item.priority);

    let tdEdit = tr.insertCell(6);
    tdEdit.appendChild(editButton);

    let tdDelete = tr.insertCell(7);
    tdDelete.appendChild(deleteButton);

    if (userType === "admin") {
      let tdUser = tr.insertCell(8);
      tdUser.appendChild(userDetailsBtn);
    }
  });

  tasks = data;
};

// פונקציה להמרת עדיפות למילים
function priorityToText(priority) {
  switch (priority) {
    case 1: return 'Low';
    case 2: return 'Medium';
    case 3: return 'High';
    default: return 'Unknown';
  }
}


// פונקציה לקבלת פרטי משתמש
const getUserDetails = async () => {
  const token = getToken();
  if (!token)
    return;

  let { userId } = currentUserDetailsFromToken();
  if (userId == null)
    return;

  try {
    const response = await fetch(`${userUri}/${userId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    if (response.ok) {
      currentUserDetails = await response.json();
      displayUserDetails(currentUserDetails);
    } else {
      console.error('Unable to fetch user details.');
    }
  } catch (error) {
    console.error('Error fetching user details:', error);
  }
};

// פונקציה להצגת פרטי משתמש בראש העמוד
const displayUserDetails = (user) => {
  document.getElementById('user-name').innerText = user.userName || '';
};

const checkAuthState = () => {
  const token = localStorage.getItem("authToken");
  const logoutButton = document.getElementById("logout-button");

  if (token) {
    // משתמש מחובר
    logoutButton.style.display = "inline-block";
  } else {
    // משתמש לא מחובר
    logoutButton.style.display = "none";
  }
};

const redirectToUsersPage = () => {
  window.location.href = "/html/user.html";
}

const openAddItemModal = async () => {
  let { userType } = currentUserDetailsFromToken();
  if (userType === "admin") {
    try {
      const data = await getAllUsers();
      const select = document.getElementById('add-userId');
      select.innerHTML = '';

      data.forEach(user => {
        const option = document.createElement('option');
        option.value = user.id;
        option.textContent = user.userName;
        select.appendChild(option);
      });

      select.setAttribute('add-userId', 'true');
      select.style.display = 'inline-block';
    } catch (err) {
      console.error('שגיאה בקבלת רשימת משתמשים:', err);
    }
  }

  document.getElementById('addItemModal').style.display = 'block';
};
//פונ להמרת השעות בצורה נכונה
const localIsoString = (dateStr) => {
  const date = new Date(dateStr);
  const offsetMs = date.getTimezoneOffset() * 60000;
  const local = new Date(date.getTime() - offsetMs);
  return local.toISOString().slice(0, 16); // yyyy-MM-ddTHH:mm
};
const closeAddItemModal = () => {
  document.getElementById('addItemModal').style.display = 'none';
};
// פונקציה להצגת חלון עריכה
const openEditItemModal = async (id) => {
  const task = tasks.find(s => s.id === id);
  if (!task) return;

  document.getElementById('edit-id').value = task.id || '';
  document.getElementById('edit-title').value = task.title || '';
  document.getElementById('edit-description').value = task.description || '';

  document.getElementById('edit-dueDate').value = task.dueDate ? localIsoString(task.dueDate) : '';

  const statusInput = document.getElementById('edit-status');
  statusInput.value = typeof task.status === 'number' ? task.status.toString() : '';

  document.getElementById('edit-priority').value = task.priority || 1;

  document.getElementById('editItemModal').style.display = 'block';

  const { userType } = currentUserDetailsFromToken();

  if (userType === "admin") {
    try {
      const data = await getAllUsers();
      const select = document.getElementById('edit-userId');
      select.innerHTML = '';

      data.forEach(user => {
        const option = document.createElement('option');
        option.value = user.id;
        option.textContent = user.userName;
        select.appendChild(option);
      });

      select.value = task.userId || '';
      select.style.display = 'inline-block';
    } catch (err) {
      console.error('שגיאה בקבלת רשימת משתמשים:', err);
    }
  }
};

// פונקציה לסגירת חלון עריכה
const closeEditItemModal = () => {
  document.getElementById('editItemModal').style.display = 'none';
};

// פונקציה להצגת חלון עריכה
const openEditUserModalByHimSelf = () => {
  const user = currentUserDetails;
  if (!user) {
    console.log("User not found.");
    return;
  }
  document.getElementById('edit-username').value = user.userName || '';
  document.getElementById('edit-email').value = user.email || '';
  document.getElementById('edit-password').value = user.password || '';
  document.getElementById('editUserModal').style.display = 'block';
};

// פונקציה לסגירת חלון עריכה
const closeEditUserModal = () => {
  document.getElementById('editUserModal').style.display = 'none';
};

// פונקציה לעדכון משתמש (admin בלבד)
const updateUser = async () => {
  let { userId } = currentUserDetailsFromToken();
  if (userId == null)
    return;
  const user = {
    id: userId,
    userName: document.getElementById('edit-username').value.trim(),
    password: document.getElementById('edit-password').value.trim(),
    email: document.getElementById('edit-email').value.trim(),
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

    await displayUserDetails(currentUserDetails); // עדכון הרשימה
    await getUserDetails(); // עדכון פרטי המשתמש
    closeEditUserModal(); // סגירת חלון העריכה
  } catch (error) {
    console.error('Error updating user:', error);
  }
};

// פונקציה להצגת פרטי משתמש בפריט
const displayUserDetailsToItem = async (userId) => {
  const dataUsers = await getAllUsers();
  const user = dataUsers.find(u => u.id === userId);

  if (!user) {
    console.log("User not found.");
    return;
  }

  const userDetailsDiv = document.getElementById('user-details');
  userDetailsDiv.innerHTML = `
    <p><strong>Username:</strong> ${user.userName}</p>
    <p><strong>Password:</strong> ${user.password}</p>
    <p><strong>Email:</strong> ${user.email}</p>
    <p><strong>Type:</strong> ${user.type}</p>
  `;

  document.getElementById('displayUserDetailsModal').style.display = 'block';
};

const closeDisplayUserDetailsModal = () => {
  document.getElementById('displayUserDetailsModal').style.display = 'none';
}
