# 🗳️ Online Voting System | ASP.NET Core MVC

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![C#](https://img.shields.io/badge/C%23-Language-purple)
![MySQL](https://img.shields.io/badge/MySQL-Database-orange)
![Bootstrap](https://img.shields.io/badge/Bootstrap-Frontend-blueviolet)
![Status](https://img.shields.io/badge/Status-Completed-success)

A secure, role-based **Online Voting System** built using **ASP.NET Core MVC**, **C#**, **MySQL**, **HTML**, **CSS**, **JavaScript**, and **Bootstrap**.

The application enables administrators to manage elections, candidates, and voters while allowing users to securely cast their votes through an intuitive web interface. The system ensures that each voter can vote only once while providing administrators with complete control over the election process.

---

# 🎯 Project Highlights

- 🔐 Secure Authentication
- 👥 Role-Based Access Control
- 🗳️ Election Management
- 👤 Candidate Registration & Approval
- 🧑‍💼 Voter Management
- 📊 Election Results
- 💻 Responsive User Interface

---

# ✨ Features

## 👨‍💼 Administrator

- Secure Login
- Dashboard
- Manage Elections
- Add, Edit and Delete Candidates
- Approve Candidate Registration Requests
- Manage Voters
- Monitor Election Status
- View Election Results
- Manage User Accounts

---

## 🧑 Candidate

- Register/Login
- Submit Candidate Request
- View Request Status
- Participate in Elections
- View Election Results

---

## 🗳️ Voter

- Secure Login
- View Available Elections
- Cast Vote
- One Vote Per Election
- View Election Results

---

# 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| ASP.NET Core MVC | Backend Framework |
| C# | Business Logic |
| MySQL | Database |
| HTML5 | Structure |
| CSS3 | Styling |
| Bootstrap | Responsive UI |
| JavaScript | Client-side Functionality |

---

# 📂 Project Structure

```
Online-Voting-System
│
├── Database
│   └── VotingSystem.sql
│
├── VotingSystem
│   ├── Pages
│   ├── wwwroot
│   ├── Properties
│   ├── appsettings.json
│   ├── Program.cs
│   └── VotingSystem.csproj
│
├── .gitignore
└── VotingSystem.sln
```

---

# ⚙️ Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Pruthvi-raj-shetty/Online-Voting-System.git
```

### 2. Open the Project

Open the solution file in **Visual Studio 2022**.

```
VotingSystem.sln
```

### 3. Restore NuGet Packages

Visual Studio will automatically restore the required packages.

### 4. Configure Database

Import the SQL file located inside:

```
Database/VotingSystem.sql
```

using **MySQL Workbench**.

### 5. Update Connection String

Open:

```
appsettings.json
```

Update the MySQL connection string according to your local database configuration.

### 6. Run the Application

Press **F5** or click **Run** in Visual Studio.

---

# 🗄️ Database

The complete database schema is available in:

```
Database/VotingSystem.sql
```

The database contains tables for:

- Users
- Elections
- Candidates
- Votes
- Candidate Requests
- Results

---

# 👥 User Roles

| Role | Permissions |
|------|-------------|
| Administrator | Manage Elections, Candidates, Voters and Results |
| Candidate | Register and Participate in Elections |
| Voter | Cast Vote and View Results |

---

# 🔒 Security Features

- Role-Based Authentication
- One Vote Per Voter
- Session Management
- Candidate Approval Workflow
- Secure Database Operations
- Server-side Validation

---

# 📸 Screenshots

Screenshots will be added soon.

Planned screenshots include:

- Login Page
- Administrator Dashboard
- Candidate Dashboard
- Election Management
- Candidate Management
- Voting Page
- Election Results

---

# 🚀 Future Enhancements

- Email Verification
- OTP Authentication
- Password Reset
- Election Scheduling
- Audit Logs
- Export Results to PDF
- Multi-language Support
- Cloud Deployment (Azure / AWS)
- Two-Factor Authentication

---

# 🤝 Contributing

Contributions are welcome.

If you'd like to improve this project:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your branch
5. Open a Pull Request

---

# 👨‍💻 Author

**Pruthvi Raj Shetty**

BCA Student | Full Stack Developer | Cloud & Cybersecurity Enthusiast

GitHub:
https://github.com/Pruthvi-raj-shetty

---

## ⭐ If you found this project useful, consider giving it a star!
