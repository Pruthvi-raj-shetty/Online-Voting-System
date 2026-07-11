using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Pages
{
    public class ElectionManagementModel : PageModel
    {
        [BindProperty] public int ElectionId { get; set; }
        [BindProperty, Required] public string Title { get; set; }
        [BindProperty] public string Description { get; set; }

        [BindProperty, Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [BindProperty, Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [BindProperty] public string Status { get; set; }
        public string Message { get; set; }

        public List<Election> ElectionList { get; set; } = new();

        private readonly string connectionString = "server=localhost;uid=root;pwd=sqlkey;database=votingsystem";

        public void OnGet()
        {
            LoadElections();
        }

        public IActionResult OnPostSave()
        {
            if (!ModelState.IsValid)
            {
                Message = "⚠️ Please fill all required fields.";
                LoadElections();
                return Page();
            }

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = ElectionId > 0
                    ? @"UPDATE Elections 
                        SET Title=@Title, Description=@Description, StartDate=@StartDate, EndDate=@EndDate, Status=@Status 
                        WHERE ElectionId=@ElectionId"
                    : @"INSERT INTO Elections (Title, Description, StartDate, EndDate, Status)
                        VALUES (@Title, @Description, @StartDate, @EndDate, @Status)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", Title);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(Description) ? DBNull.Value : Description);
                cmd.Parameters.AddWithValue("@StartDate", StartDate);
                cmd.Parameters.AddWithValue("@EndDate", EndDate);
                cmd.Parameters.AddWithValue("@Status", Status);

                if (ElectionId > 0)
                    cmd.Parameters.AddWithValue("@ElectionId", ElectionId);

                cmd.ExecuteNonQuery();

                Message = ElectionId > 0
                    ? "✅ Election updated successfully!"
                    : "✅ Election created successfully!";

                ElectionId = 0;
                Title = Description = Status = string.Empty;
                StartDate = EndDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                Message = "❌ Error saving election: " + ex.Message;
            }

            LoadElections();
            return Page();
        }

        public IActionResult OnPostEdit(int electionId)
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                using var cmd = new MySqlCommand("SELECT * FROM Elections WHERE ElectionId=@ElectionId", conn);
                cmd.Parameters.AddWithValue("@ElectionId", electionId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ElectionId = reader.GetInt32("ElectionId");
                    Title = reader.GetString("Title");
                    Description = reader["Description"]?.ToString();
                    StartDate = reader.GetDateTime("StartDate");
                    EndDate = reader.GetDateTime("EndDate");
                    Status = reader["Status"].ToString();

                    Message = "📝 Election is set for update. Modify fields and click 'Update Election'.";
                }
                else
                {
                    Message = "⚠️ Election not found.";
                }
            }
            catch (Exception ex)
            {
                Message = "❌ Error loading election: " + ex.Message;
            }

            LoadElections();
            return Page();
        }

        public IActionResult OnPostDelete(int electionId)
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                // 1️⃣ Revert candidates back to normal users
                string revertCandidatesQuery = @"
            UPDATE Candidates 
            SET Status='User' 
            WHERE ElectionId=@ElectionId AND Status='Active'";

                using (var revertCmd = new MySqlCommand(revertCandidatesQuery, conn))
                {
                    revertCmd.Parameters.AddWithValue("@ElectionId", electionId);
                    revertCmd.ExecuteNonQuery();
                }

                // 2️⃣ Delete candidates linked to this election (optional)
                string deleteCandidatesQuery = @"
            DELETE FROM Candidates 
            WHERE ElectionId=@ElectionId AND Status!='User'";

                using (var delCandCmd = new MySqlCommand(deleteCandidatesQuery, conn))
                {
                    delCandCmd.Parameters.AddWithValue("@ElectionId", electionId);
                    delCandCmd.ExecuteNonQuery();
                }

                // 3️⃣ Delete votes linked to this election to prevent FK constraint error
                string deleteVotesQuery = "DELETE FROM Votes WHERE ElectionId=@ElectionId";
                using (var delVotesCmd = new MySqlCommand(deleteVotesQuery, conn))
                {
                    delVotesCmd.Parameters.AddWithValue("@ElectionId", electionId);
                    delVotesCmd.ExecuteNonQuery();
                }

                // 4️⃣ Delete the election
                string deleteElectionQuery = "DELETE FROM Elections WHERE ElectionId=@ElectionId";
                using (var delElectionCmd = new MySqlCommand(deleteElectionQuery, conn))
                {
                    delElectionCmd.Parameters.AddWithValue("@ElectionId", electionId);
                    delElectionCmd.ExecuteNonQuery();
                }

                Message = "🗑️ Election deleted successfully! Candidates and votes reverted.";
            }
            catch (Exception ex)
            {
                Message = "❌ Error deleting election: " + ex.Message;
            }

            LoadElections();
            return Page();
        }


        private void LoadElections()
        {
            ElectionList.Clear();
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                using (var updateCmd = new MySqlCommand(
                    "UPDATE Elections SET Status='Completed' WHERE EndDate < @Now AND Status != 'Completed'", conn))
                {
                    updateCmd.Parameters.AddWithValue("@Now", DateTime.Now);
                    updateCmd.ExecuteNonQuery();
                }

                using var cmd = new MySqlCommand("SELECT * FROM Elections ORDER BY StartDate DESC", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ElectionList.Add(new Election
                    {
                        ElectionId = reader.GetInt32("ElectionId"),
                        Title = reader.GetString("Title"),
                        Description = reader["Description"]?.ToString(),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Status = reader["Status"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Message = "❌ Error loading elections: " + ex.Message;
            }
        }

        public class Election
        {
            public int ElectionId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }
        }
    }
}
