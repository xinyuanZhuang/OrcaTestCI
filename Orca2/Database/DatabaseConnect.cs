
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using Orca.Services;
using Microsoft.Extensions.Logging;
using Orca.Entities;

namespace Orca.Database
{
    public class DatabaseConnect
    {
        string _connString = "Server = 127.0.0.1; Port=3306; Database = orca_db; Uid = Orca; Pwd = orcaorcaorca;";

        public void StoreEventToDatabase(StudentEvent studentEvent)
        {
            string sql = "INSERT INTO event(student_ID, course_ID, Timestamp, event_type, activity_name, activity_details) VALUES(@student_ID,@course_ID, @Timestamp, @event_type, @activity_name, @activity_details)";
            MySqlParameter[] parameters = {
                                            new MySqlParameter("@student_ID", studentEvent.Student.ID),
                                            new MySqlParameter("@course_ID", studentEvent.CourseID),
                                            new MySqlParameter("@Timestamp", studentEvent.Timestamp),
                                            new MySqlParameter("@event_type", studentEvent.EventType),
                                            new MySqlParameter("@activity_name", studentEvent.ActivityName),
                                            new MySqlParameter("@activity_details", studentEvent.ActivityType)
                                        };
            AddInfoToDatabase(sql, parameters);
            Console.WriteLine("event of student {0} has been added into database", studentEvent.Student.ID);
        }

        public void StoreStudentToDatabase(StudentEvent studentEvent)
        {
            string sql = "INSERT INTO student(id, first_name, last_name,email) VALUES(@ID, @first_name, @last_name,@studentEmail)";
            MySqlParameter[] parameters = {
                                                        new MySqlParameter("@ID", studentEvent.Student.ID),
                                                        new MySqlParameter("@first_name", studentEvent.Student.FirstName),
                                                        new MySqlParameter("@last_name", studentEvent.Student.LastName),
                                                        new MySqlParameter("@studentEmail", studentEvent.Student.Email)
                                                    };
            AddInfoToDatabase(sql, parameters);
            Console.WriteLine("student: {0} {1} has been added into database", studentEvent.Student.FirstName, studentEvent.Student.LastName);
        }

        public void AddInfoToDatabase(string sql, MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = new MySqlConnection(_connString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteScalar();
                }
            }
        }

        public void ReadData()
        {

            using (MySqlConnection conn = new MySqlConnection(_connString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT first_name FROM student";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(reader.GetOrdinal("first_name"));
                    Console.WriteLine("student name: {0}", name);
                }
            }
        }
    }
}
