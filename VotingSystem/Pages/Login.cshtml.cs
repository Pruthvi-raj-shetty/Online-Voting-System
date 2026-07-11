using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace VotingSystem.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // ✅ 1. Empty field check
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["LoginError"] = "Please fill in all fields.";
                return Page();
            }

            // ✅ 2. Basic email format check (must contain @ and valid structure)
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TempData["LoginError"] = "Please enter a valid email address.";
                return Page();
            }

            // ✅ 3. Optional: Simple password length check
            if (Password.Length < 6)
            {
                TempData["LoginError"] = "Password must be at least 6 characters long.";
                return Page();
            }

            try
            {
                string connStr = "server=localhost;user=root;password=sqlkey;database=VotingSystem;";
                using (var connection = new MySqlConnection(connStr))
                {
                    connection.Open();

                    string hashedInput = HashPassword(Password);

                    // 🔍 Check if user exists
                    string userQuery = "SELECT UserId FROM Users WHERE Email=@Email AND Password=@Password";
                    int userId = 0;
                    using (var cmd = new MySqlCommand(userQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@Password", hashedInput);

                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            TempData["LoginError"] = "Invalid Email or Password!";
                            return Page();
                        }
                        userId = Convert.ToInt32(result);
                    }

                    // ✅ Save userId in session
                    HttpContext.Session.SetInt32("UserId", userId);

                    // 🔍 Admin check
                    string adminQuery = "SELECT AdminId FROM Admins WHERE UserId=@UserId";
                    using (var cmd = new MySqlCommand(adminQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        var adminResult = cmd.ExecuteScalar();
                        if (adminResult != null)
                            return RedirectToPage("/AdminDashboard");
                    }

                    // 🔍 Candidate check
                    string candidateQuery = "SELECT CandidateId FROM Candidates WHERE UserId=@UserId AND Status='Active'";
                    using (var cmd = new MySqlCommand(candidateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        var candidateResult = cmd.ExecuteScalar();
                        if (candidateResult != null)
                            return RedirectToPage("/CandidateDashboard");
                    }

                    // ✅ Normal user
                    return RedirectToPage("/UserDashboard");
                }
            }
            catch (Exception ex)
            {
                TempData["LoginError"] = "Error: " + ex.Message;
                return Page();
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
