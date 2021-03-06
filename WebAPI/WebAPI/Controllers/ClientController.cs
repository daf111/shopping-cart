using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("client")]
    public class ClientController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            SqlConnection conexion = new SqlConnection(@"Data Source=DESKTOP-3B107HD\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True");
            conexion.Open();

            string SQL_STATEMENT = "select id, name, email, birth_day FROM client ORDER BY name ASC";
            SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conexion);
            SqlDataReader reader = cmd.ExecuteReader();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> dictionary;

            while (reader.Read())
            {
                dictionary = new Dictionary<string, string>();
                dictionary.Add("id", reader.GetInt32(0).ToString());
                dictionary.Add("name", reader.GetString(1));
                dictionary.Add("email", reader.GetString(2));
                dictionary.Add("birthDate", reader.GetDateTime(3).ToString());
                list.Add(dictionary);
            }

            return Ok(list);
        }

        [HttpPost]
        public IActionResult Post(string n, string e, DateTime b)
        {
            string error = "";

            SqlConnection conexion = new SqlConnection(@"Data Source=DESKTOP-3B107HD\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True");
            conexion.Open();
            
            if (n.Length >= 3 && n.Length <= 50)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(e);
                if (match.Success)
                {
                    // Calculate the age.
                    var age = DateTime.Today.Year - b.Year;
                    // Go back to the year in which the person was born in case of a leap year
                    if (b.Date > DateTime.Today.AddYears(-age)) age--;
                    if (age >= 18)
                    {

                        string SQL_STATEMENT = "select id FROM client WHERE email = @email";
                        SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conexion);
                        cmd.Parameters.Add(new SqlParameter("@email", e));
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        { //Existe el email!!!
                            error = "El email está en uso para otro usuario";
                        } else
                        {
                            reader.Close();

                            SQL_STATEMENT = "INSERT INTO client OUTPUT INSERTED.ID VALUES (@name, @email, @birth_day)";
                            cmd = new SqlCommand(SQL_STATEMENT, conexion);
                            cmd.Parameters.Add(new SqlParameter("@name", n));
                            cmd.Parameters.Add(new SqlParameter("@email", e));
                            cmd.Parameters.Add(new SqlParameter("@birth_day", b));
                            int id = (int)cmd.ExecuteScalar();

                            return Ok(new Dictionary<string, string>
                            {
                                {"id", id.ToString()},
                                {"name", n},
                                {"email", e},
                                {"birthDay", b.ToString()}
                            });
                        }
                    } else
                    {
                        error = "Cient must be 18 years old or older";
                    }
                } else
                {
                    error = "Email address invalid";
                }
            } else
            {
                error = "Name must have between 3 and 50 characters long";
            }

            return BadRequest(new Dictionary<string, string>
            {
                { "error",  error }
            });
        }

        [HttpPut]
        public IActionResult Put(int id, string n, string e, DateTime b)
        {
            string error = "";

            SqlConnection conexion = new SqlConnection(@"Data Source=DESKTOP-3B107HD\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True");
            conexion.Open();

            if (n.Length >= 3 && n.Length <= 50)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(e);
                if (match.Success)
                {
                    // Calculate the age.
                    var age = DateTime.Today.Year - b.Year;
                    // Go back to the year in which the person was born in case of a leap year
                    if (b.Date > DateTime.Today.AddYears(-age)) age--;
                    if (age >= 18)
                    {
                        string SQL_STATEMENT = "select id FROM client WHERE email = @email and id != @id";
                        SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conexion);
                        cmd.Parameters.Add(new SqlParameter("@email", e));
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        { //Existe el email!!!
                            error = "El email está en uso para otro usuario";
                        }
                        else
                        {
                            reader.Close();

                            SQL_STATEMENT = "select id, name, email, birth_day FROM client WHERE id = @id";
                            cmd = new SqlCommand(SQL_STATEMENT, conexion);
                            cmd.Parameters.Add(new SqlParameter("@id", id));
                            reader = cmd.ExecuteReader();

                            if (reader.Read())
                            { //Existe el cliente!!!

                                reader.Close();

                                SQL_STATEMENT = "UPDATE client SET name = @name, email = @email, birth_day = @birth_day WHERE id = @id";
                                cmd = new SqlCommand(SQL_STATEMENT, conexion);
                                cmd.Parameters.Add(new SqlParameter("@id", id));
                                cmd.Parameters.Add(new SqlParameter("@name", n));
                                cmd.Parameters.Add(new SqlParameter("@email", e));
                                cmd.Parameters.Add(new SqlParameter("@birth_day", b));
                                cmd.ExecuteNonQuery();

                                return Ok(new Dictionary<string, string>
                                {
                                    {"id", id.ToString()},
                                    {"name", n},
                                    {"email", e},
                                    {"birthDay", b.ToString()}
                                });
                            }
                            else
                            {
                                error = "El cliente no existe";
                            }
                        }
                    }
                    else
                    {
                        error = "Cient must be 18 years old or older";
                    }
                }
                else
                {
                    error = "Email address invalid";
                }
            }
            else
            {
                error = "Name must have between 3 and 50 characters long";
            }

            return BadRequest(new Dictionary<string, string>
            {
                { "error",  error }
            });
        }
    }
}
