using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VotingSystem.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty] public string Name { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }

        public string Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // ✅ 1. Empty field check
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                Message = "Please fill in all fields.";
                return Page();
            }

            // ✅ 2. Validate name length
            if (Name.Length < 3)
            {
                Message = "Name must be at least 3 characters long.";
                return Page();
            }

            // ✅ 3. Validate email format
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Message = "Please enter a valid email address.";
                return Page();
            }

            // ✅ 4. Password strength check
            if (Password.Length < 6)
            {
                Message = "Password must be at least 6 characters long.";
                return Page();
            }

            try
            {
                string connStr = "server=localhost;user=root;password=sqlkey;database=VotingSystem;";
                using (var connection = new MySqlConnection(connStr))
                {
                    connection.Open();

                    // 🔎 Optional: Check if email already exists
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
                    using (var checkCmd = new MySqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", Email);
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            Message = "This email is already registered.";
                            return Page();
                        }
                    }

                    // 🛡 Hash password
                    string hashedPassword = HashPassword(Password);

                    // ➕ Insert new user
                    string query = "INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                        cmd.ExecuteNonQuery();
                    }
                }

                // ✅ Success: Redirect to login
                TempData["RegisterSuccess"] = "Registration successful! Please log in.";
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
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
